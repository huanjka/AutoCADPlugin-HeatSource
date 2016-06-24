using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using HeatSource.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeatSource.Formula;

namespace HeatSource.Model
{
    public class SubStation : BaseModel
    {
        public Solution OwnSolution { set; get; }
        public static string modelType = "substation";
        //这个的定义为热力站与楼房相连
        private District ownMasterDistrict;
        public District OwnMasterDistrict {
            set
            {
                ownMasterDistrict = value;
                Save();
            }
            get
            {
                return ownMasterDistrict;
            }
        }
        //这个的定义为热源与热力站相连
        public District ownSlaveDistrict;
        public District OwnSlaveDistrict {
            set
            {
                ownSlaveDistrict = value;
                Save();
            }
            get
            {
                return ownSlaveDistrict;
            }
        }


        //换热器容量
        double heatSwitcherVolumn;
        public double HeatSwitcherVolumn
        {
            get
            {
                heatSwitcherVolumn = this.TotalHeatingDesignLoad / this.HeatSwitcherCount;
                if (double.IsInfinity(heatSwitcherVolumn))
                    return 0;
                else if (double.IsNaN(heatSwitcherVolumn))
                    return 0;
                return Math.Round(heatSwitcherVolumn, 2);
            }
            set
            {
                heatSwitcherVolumn = value;
                Save();
            }
        }
        
        //水泵台数
        public int WaterPumpCount
        {
            get
            {
                return this.HeatSwitcherCount;
            }
        }
        // 水泵流量
        public double WaterPumpVolume
        {
            get
            {
                return Utility.DoubleWithTwoDecimalPlaces(new WaterPumpFlow(this.WaterPumpTemp, this.TotalHeatingDesignLoad, this.WaterPumpCount).GetResult());
            }
        }
        // 水泵扬程
        public double WaterPumpLift
        {
            get
            {
                double wpl = 0;
                if (this.OwnSolution != null)
                {
                    wpl = new WaterPumpHead(this.WaterPumpTemp, this.MasterPipeLength,
                        this.WaterPumpVolume, this.TotalHeatingDesignLoad, this.WaterPumpCount).GetResult();
                }
                return wpl;
            }
        }
        //水泵功率
        public double WaterPumpPower
        {
            get
            {
                return new WaterPumpPower(this.WaterPumpLift, this.WaterPumpVolume).GetResult();
            }
           set
            {
            }
        }
        //热力站总负荷
        public double TotalHeatingDesignLoad
        {
            get
            {
                double value = 0;
                if (this.OwnMasterDistrict != null)
                {
                    foreach (var item in this.OwnMasterDistrict.Buildings)
                    {
                        value += item.HeatLoad;
                    }
                }

                //增加外网散热部分
                double tempDiff = this.WaterPumpTemp;
                double supplyWaterTemp = this.SupplyWaterTemp;
                double totalPipeLengh = this.TotoalPipeLength;
                double outerHeating = new OuterHeating(value, tempDiff, supplyWaterTemp, totalPipeLengh).GetResult();
                value += outerHeating;
                return value;
            }
            set
            {
            }
        }
        // 主管道长度，即最长管道长度
        public double MasterPipeLength
        {
            get
            {
                if (this.OwnSolution == null)
                {
                    return 0;
                }
                List<PipeLine> lines;
                return 2 * this.OwnSolution.GetMaxPipeLengthOfModel(this, out lines);
            }
        }
        // 管道总长度
        public double TotoalPipeLength
        {
            get
            {
                //测试用
                //return 100;
                if (this.OwnSolution == null)
                {
                    return 0;
                }
                List<PipeLine> lines;
                return this.OwnSolution.GetTotalPipeLengthOfModel(this, out lines);
            }
        }
        //水泵供水温度
        private double supplyWaterTemp;
        public double SupplyWaterTemp
        {
            get
            {
                return supplyWaterTemp;
            }
            set
            {
                supplyWaterTemp = value;
                Save();
            }
        }

        //水泵回水温度
        private double returnWaterTemp;
        public double ReturnWaterTemp
        {
            get
            {
                return returnWaterTemp;
            }
            set
            {
                returnWaterTemp = value;
                Save();
            }
        }
        //水泵供回水温差
        public double WaterPumpTemp
        {
            get
            {
                
                return SupplyWaterTemp - ReturnWaterTemp;
            }
           
        }
   
        //供热方式
        public int HeatStyle { set; get; }
        //换热器台数
        public int HeatSwitcherCount { set; get; }


        //热力站的水泵全年运行能耗
        public double totalWaterPumpEnergyConsumption
        {
            get
            {
                double value = 0;
                double heatingDays = HeatSourceLayoutApp.globalProperty.HeatingDays;
                double indoorTemp = HeatSourceLayoutApp.globalProperty.IndoorTemperature;
                double outdoorMeanTemp = HeatSourceLayoutApp.globalProperty.OutAverageTemp;
                double outdoorTemp = HeatSourceLayoutApp.globalProperty.OutDoorTemp;
                double waterPumpPower = this.WaterPumpPower;

                //直接供热
                value = new WaterPumpEnergyConsumption(heatingDays, indoorTemp, outdoorMeanTemp, outdoorTemp, waterPumpPower).GetResult()*this.WaterPumpCount;
                return value;
            }
        }

        public SubStation(Solution sol, bool needId) : base(needId)
        {
            this.OwnSolution = sol;
            this.HeatStyle = 3;
            this.HeatSwitcherCount = 2;
            this.HeatSwitcherVolumn = 0;
            this.WaterPumpPower = 0;
            this.OwnMasterDistrict = null;
            this.OwnSlaveDistrict = null;
            this.SupplyWaterTemp = 75;
            this.ReturnWaterTemp = 55;
        }


        //仅仅用于restore
        public SubStation() : base(false)
        {

        }

        protected override void GetAttributes()
        {
            attrs.Clear();
            attrs.Add("WaterPumpCount", this.WaterPumpCount.ToString());
            attrs.Add("HeatStyle", this.HeatStyle.ToString());
            attrs.Add("supplywatertemp", this.SupplyWaterTemp.ToString());
            attrs.Add("returnwatertemp", this.ReturnWaterTemp.ToString());
            attrs.Add("HeatSwitcherCount", this.HeatSwitcherCount.ToString());
            attrs.Add("HeatSwitcherVolumn", this.HeatSwitcherVolumn.ToString());

            attrs.Add("WaterPumpPower", this.WaterPumpPower.ToString());
            attrs.Add("WaterPumpTemp", this.WaterPumpTemp.ToString());
            attrs.Add("solution", Utils.ModelIdManager.toString(this.OwnSolution.BaseModelId));
            if (OwnMasterDistrict != null)
            {
                attrs.Add("masterdistrict", ModelIdManager.toString(OwnMasterDistrict.BaseModelId));
            }
            if (OwnSlaveDistrict != null)
            {
                attrs.Add("slavedistrict", ModelIdManager.toString(OwnSlaveDistrict.BaseModelId));
            }
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            //if (attrs.ContainsKey("WaterPumpCount"))
            //{
            //    this.WaterPumpCount = int.Parse(attrs["WaterPumpCount"]);
            //}
            if (attrs.ContainsKey("HeatStyle"))
            {
                this.HeatStyle = int.Parse(attrs["HeatStyle"]);
            }
            if (attrs.ContainsKey("HeatSwitcherCount"))
            {
                this.HeatSwitcherCount = int.Parse(attrs["HeatSwitcherCount"]);
            }
            if (attrs.ContainsKey("HeatSwitcherVolumn"))
            {
                this.HeatSwitcherVolumn = double.Parse(attrs["HeatSwitcherVolumn"]);
            }


            if (attrs.ContainsKey("WaterPumpPower"))
            {
                this.WaterPumpPower = double.Parse(attrs["WaterPumpPower"]);
            }
            if (attrs.ContainsKey("supplywatertemp"))
            {
                this.SupplyWaterTemp = double.Parse(attrs["supplywatertemp"]);
            }
            if (attrs.ContainsKey("returnwatertemp"))
            {
                this.ReturnWaterTemp = double.Parse(attrs["returnwatertemp"]);
            }
            if (attrs.ContainsKey("solution"))
            {
                this.OwnSolution = (Solution)BaseModel.GetModelById(Utils.ModelIdManager.Parse(attrs["solution"]));
                if (this.OwnSolution != null)
                {
                    this.OwnSolution.SubStations.Add(this.BaseObjectId, this);
                }
            }
            if (attrs.ContainsKey("masterdistrict"))
            {
                this.OwnMasterDistrict = (District)BaseModel.GetModelById(ModelIdManager.Parse(attrs["masterdistrict"]));
            }
            if (attrs.ContainsKey("slavedistrict"))
            {
                this.OwnSlaveDistrict = (District)BaseModel.GetModelById(ModelIdManager.Parse(attrs["slavedistrict"]));
            }

        }

        protected override string ModelType()
        {
            return SubStation.modelType;
        }

        public bool isInpolyline(Polyline pline)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(this.BaseObjectId, OpenMode.ForRead);
                BlockReference block = (BlockReference)obj;
                Extents3d bd = block.GeometryExtentsBestFit();
                Point3d p1 = bd.MinPoint;
                Point3d p2 = bd.MaxPoint;
                if (Utils.Utility.PointInPolygon(p1, pline))
                {
                    if (Utils.Utility.PointInPolygon(p2, pline))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //全年耗热量
        public double TotalLoad
        {
            get
            {
                double value = 0;
                if (this.OwnMasterDistrict != null)
                {
                    foreach (var item in this.OwnMasterDistrict.Buildings)
                    {
                        value += item.YearHeat;
                    }
                }
                return value;
            }
        }
        
        public void Delete()
        {
            //remove itself from current solution
            OwnSolution.HeatProducers.Remove(BaseObjectId);
            //热力站删除，热力站与楼房的关联要删除
            if (OwnMasterDistrict != null)
            {
                OwnMasterDistrict.RemoveSelf();
            }
            //热力站删除，热力站与热源的关联要被移除，但圈选要等到所有的热力站都被移除才发生
            if (OwnSlaveDistrict != null)
            {
                OwnSlaveDistrict.SubStations.Remove(this);
                if (OwnSlaveDistrict.SubStations.Count == 0)
                {
                    OwnSlaveDistrict.RemoveSelf();
                }
            }
        }

        public void RemoveSelf()
        {
            //remove self
            Delete();
            Database db = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
            using (Autodesk.AutoCAD.ApplicationServices.DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    tr.GetObject(BaseObjectId, OpenMode.ForWrite).Erase();
                    tr.Commit();
                    tr.Dispose();
                }
            }
        }

        public double TotalArea
        {
            get
            {
                double total = 0;
                if (this.OwnMasterDistrict != null)
                {
                    total += this.OwnMasterDistrict.TotalArea;
                }
                return total;
            }
        }
        public void UpdateScale()
        {
            Autodesk.AutoCAD.EditorInput.Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            using (Autodesk.AutoCAD.ApplicationServices.DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction trans = ed.Document.Database.TransactionManager.StartTransaction())
                {
                    DBObject obj = trans.GetObject(this.BaseObjectId, OpenMode.ForWrite);
                    BlockReference block = (BlockReference)obj;
                    block.ScaleFactors = new Scale3d(HeatSourceLayoutApp.globalProperty.BlockScale);
                    trans.Commit();
                    trans.Dispose();
                }
            }
        }
    }
}
