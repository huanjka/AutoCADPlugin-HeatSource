using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeatSource.Formula;
using HeatSource.Utils;

namespace HeatSource.Model
{
    /// <summary>
    /// 热源的类
    /// </summary>
    public class HeatProducer:BaseModel
    {
        public static string modelType = "heatproducer";
        public Solution OwnSolution { set; get; }
        //这个的定义是热源连接到楼房
        private District ownMasterDistrict = null;
        public District OwnMasterDistrict
        {
            set
            {
                ownMasterDistrict = value;
                this.Save();
            }
            get
            {
                return ownMasterDistrict;
            }
        }
        //这个的定义是热源连接到热力站
        private District ownSlaveDistrict = null;
        public District OwnSlaveDistrict
        {
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

        #region basemodel override
        protected override void GetAttributes()
        {
            attrs.Clear();
            attrs.Add("pumpnum", this.WaterPumpCount.ToString());
            attrs.Add("heatstyle", this.HeatStyle.ToString());
            attrs.Add("boilercount", this.BoilerCount.ToString());
            //throw new NotImplementedException();
            attrs.Add("boilerEfficiency", this.BoilerEfficiency.ToString());
            attrs.Add("supplywatertemp", this.SupplyWaterTemp.ToString());
            attrs.Add("returnwatertemp", this.ReturnWaterTemp.ToString());
            attrs.Add("totalLoad", this.TotalLoad.ToString());
            attrs.Add("solution", Utils.ModelIdManager.toString(this.OwnSolution.BaseModelId));
            if (OwnMasterDistrict != null)
            {
                attrs.Add("masterdistrict", ModelIdManager.toString(OwnMasterDistrict.BaseModelId));
            }
            if (OwnSlaveDistrict != null)
            {
                attrs.Add("slavedistrict", ModelIdManager.toString(OwnSlaveDistrict.BaseModelId));
            }
            if (BoilerSolutionFile != null && BoilerSolutionFile.Length > 0)
            {
                attrs.Add("boilersolutionfile", BoilerSolutionFile);
            }
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            if (attrs.ContainsKey("pumpnum"))
            {
                this.WaterPumpCount = int.Parse(attrs["pumpnum"]);
            }
            if (attrs.ContainsKey("heatstyle"))
            {
                this.HeatStyle = int.Parse(attrs["heatstyle"]);
            }
            if (attrs.ContainsKey("boilercount"))
            {
                this.BoilerCount = int.Parse(attrs["boilercount"]);
            }
            if (attrs.ContainsKey("boilerEfficiency"))
            {
                this.BoilerEfficiency = double.Parse(attrs["boilerEfficiency"]);
            }
            if (attrs.ContainsKey("solution"))
            {
                this.OwnSolution = (Solution)BaseModel.GetModelById(Utils.ModelIdManager.Parse(attrs["solution"]));
                if (this.OwnSolution != null)
                {
                    this.OwnSolution.HeatProducers.Add(this.BaseObjectId, this);
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
            if (attrs.ContainsKey("boilersolutionfile"))
            {
                this.BoilerSolutionFile = attrs["boilersolutionfile"];
            }
            if(attrs.ContainsKey("supplywatertemp"))
            {
                this.SupplyWaterTemp = double.Parse(attrs["supplywatertemp"]);
            }
            if(attrs.ContainsKey("returnwatertemp"))
            {
                this.ReturnWaterTemp = double.Parse(attrs["returnwatertemp"]);
            }
        }

        protected override string ModelType()
        {
            return HeatProducer.modelType;
        }
        #endregion

        //热源总负荷
        private double totalHeatingDesignLoad;
        public double TotalHeatingDesignLoad
        {
            get
            {
                double value = 0;
                //判断是直接供热还是间接供热
                //间接供热
                if(this.OwnSlaveDistrict != null)
                {
                    foreach (var item in this.OwnSlaveDistrict.SubStations)
                    {
                        value += item.TotalHeatingDesignLoad;
                    }
                    //外网散热
                    double tempDiff = this.WaterPumpTemp;
                    double supplyWaterTemp = this.SupplyWaterTemp;
                    double totalPipeLengh = this.TotoalPipeLength;
                    double outerHeating = new OuterHeating(value, tempDiff, supplyWaterTemp, totalPipeLengh).GetResult();
                    value += outerHeating;
                }
                else if(this.OwnMasterDistrict != null)
                {
                    //直接供热
                    foreach (var item in this.OwnMasterDistrict.Buildings)
                    {
                        value += item.HeatLoad;
                    }
                    //外网散热
                    double tempDiff = this.WaterPumpTemp;
                    double supplyWaterTemp = this.SupplyWaterTemp;
                    double totalPipeLengh = this.TotoalPipeLength;
                    double outerHeating = new OuterHeating(value, tempDiff, supplyWaterTemp, totalPipeLengh).GetResult();
                    value += outerHeating;
                }
                totalHeatingDesignLoad = value;
                return Math.Round(totalHeatingDesignLoad,2);
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

        //建筑总面积
        public double TotalArea
        {
            get
            {
                double total = 0;
                if (this.OwnMasterDistrict != null)
                {
                    total += this.OwnMasterDistrict.TotalArea;
                }
                if (this.OwnSlaveDistrict != null)
                {
                    total += this.OwnSlaveDistrict.TotalArea;
                }
                return total;
            }
        }
        // 水泵功率
        public double WaterPumpPower
        {
            get
            {
                return new WaterPumpPower(this.WaterPumpLift, this.WaterPumpVolume).GetResult();
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
        // 水泵容量
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
                try
                {
                    if (this.OwnSlaveDistrict != null)
                    {
                        return this.MasterPipeLength * 70 * 1.5 / 10000 + 15;
                    }
                    else
                    {
                        if (this.OwnSolution != null)
                        {
                            wpl = new WaterPumpHead(this.WaterPumpTemp, this.MasterPipeLength,
                                this.WaterPumpVolume, this.TotalHeatingDesignLoad, this.WaterPumpCount).GetResult();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                return wpl;
            }

        }
        // 水泵台数
        public int WaterPumpCount { set; get; }
        //水泵全年运行能耗
        public double totalWaterPumpEnergyConsumption
        {
            get
            {
                double value = 0;
                try
                {
                    double heatingDays = HeatSourceLayoutApp.globalProperty.HeatingDays;
                    double indoorTemp = HeatSourceLayoutApp.globalProperty.IndoorTemperature;
                    double outdoorMeanTemp = HeatSourceLayoutApp.globalProperty.OutAverageTemp;
                    double outdoorTemp = HeatSourceLayoutApp.globalProperty.OutDoorTemp;
                    double waterPumpPower = this.WaterPumpPower;

                    //直接供热或者间接供热的第一部分
                    double direactHeatingConsumption = new WaterPumpEnergyConsumption(heatingDays, indoorTemp, outdoorMeanTemp, outdoorTemp, waterPumpPower).GetResult();
                    value += direactHeatingConsumption;

                    //间接供热的第二部分
                    //把连接的热力站的水泵全年运行能耗加起来
                    double indirectHeatingConsumption = 0;
                    if (this.OwnSlaveDistrict != null)
                    {
                        foreach (SubStation sub in this.OwnSlaveDistrict.SubStations)
                        {
                            indirectHeatingConsumption += sub.totalWaterPumpEnergyConsumption;
                        }
                    }
                    value += indirectHeatingConsumption;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                return value;
            }
        }
        // 水泵供回水温差
        private double waterPumpTemp;
        public double WaterPumpTemp
        {
            get
            {
                return SupplyWaterTemp - ReturnWaterTemp;
            }
        }
      

        //锅炉方案
        private string boilerSolutionFile;
        public string BoilerSolutionFile
        {
            set
            {
                boilerSolutionFile = value;
                Save();
            }
            get
            {
                return boilerSolutionFile;
            }
        }
        // 供暖方式
        private int heatStyle;
        public int HeatStyle
        {
            set
            {
                heatStyle = value;
                
                Save();
            }
            get
            {
                return heatStyle;
            }
        }
        // 燃气锅炉效率
        private double boilerEfficiency;
        public double BoilerEfficiency {
            get
            {
                return boilerEfficiency;
            }
            set
            {
                boilerEfficiency = value;
                Save();
            }
        }

        // 锅炉台数
        public int BoilerCount { set; get; }

        
        
        public HeatProducer(Solution sol, bool needId):base(needId)
        {
            this.OwnSolution = sol;
            this.WaterPumpCount = 1;
            this.HeatStyle = 3;
            this.BoilerCount = 1;
            this.WaterPumpCount = 1;
            this.BoilerEfficiency = 90;
            this.heatStyle = 1;
            this.SupplyWaterTemp = 75;
            this.ReturnWaterTemp = 50;
            OwnMasterDistrict = null;
            OwnSlaveDistrict = null;
        }

        //用于restore的构造函数
        public HeatProducer():base(false)
        {
            this.WaterPumpCount = 0;
            this.HeatStyle = 3;
            this.BoilerCount = 0;
            this.BoilerEfficiency = 90;
            this.WaterPumpCount = 0;
            this.heatStyle = 1;
            this.SupplyWaterTemp = 75;
            this.ReturnWaterTemp = 50;
        }

        
        //全年耗热量
        public double TotalLoad
        {
            get
            {
                double value = 0;
                if(this.OwnMasterDistrict != null)
                {
                    foreach(var item in this.OwnMasterDistrict.Buildings)
                    {
                        value += item.YearHeat;
                    }
                    //直接供热全年耗热量为0.95 * Q_h_a
                    if(OwnSolution.HeatProducers.Count >= 2)
                        value = value * 0.95;


                }
                else if(this.OwnSlaveDistrict != null)
                {
                    foreach (var item in this.OwnSlaveDistrict.SubStations)
                    {
                        value += item.TotalLoad;
                    }
                }
                return value;
            }
        }
        //燃气供热运行费用
        public double GetGasHeatingCost()
        {
            double cost = new GasHeatingCost(HeatSourceLayoutApp.globalProperty.GasPrice,this.BoilerEfficiency, this.TotalLoad).GetResult();
            return cost;
        }
        //水泵运行费用
        public double GetWaterPumpOperationCost()
        {
            double cost = 0;
            double elecPrice = HeatSourceLayoutApp.globalProperty.ElecPrice;
            cost = new WaterPumpOperationCost(elecPrice, this.totalWaterPumpEnergyConsumption).GetResult();
            return cost;
        }
        //总运行费用
        public double GetTotalOperationCost()
        {
            double value = 0;
            value = this.GetGasHeatingCost() + GetWaterPumpOperationCost();
            return value;
        }
        
        //计算锅炉造价
        public double GetBoildersCost()
        {
            double cost = 0;
            if(this.BoilerSolutionFile != null && this.BoilerSolutionFile.Length > 0)
            {
                List<Dictionary<string, string>> val = ExcelUtil.readSheet(this.BoilerSolutionFile);
                foreach(var item in val)
                {
                    if(item["名称"].Contains("锅炉"))
                    {
                        cost += double.Parse(item["间接价格"]);
                    }
                }

            }
            return cost;
        }
        //计算换热器造价
        public double GetHeatExchangerCost()
        {
            double cost = 0;
            if (this.BoilerSolutionFile != null && this.BoilerSolutionFile.Length > 0)
            {
                List<Dictionary<string, string>> val = ExcelUtil.readSheet(this.BoilerSolutionFile);
                foreach (var item in val)
                {
                    if (item["名称"].Contains("换热"))
                    {
                        cost += double.Parse(item["间接价格"]);
                    }
                }
            }
            return cost;
        }

        //计算管道造价
        public double GetPipeLineCost()
        {
            List<PipeLine> lines;
            if(this.OwnSolution != null)
            {
                return this.OwnSolution.GetTotalPipeLengthOfModel(this, out lines) * 5000;
            }
            return 0;
        }

        //计算水泵造价
        public double GetWaterPumpCost()
        {
            double cost = 0;
            if (this.BoilerSolutionFile != null && this.BoilerSolutionFile.Length > 0)
            {
                List<Dictionary<string, string>> val = ExcelUtil.readSheet(this.BoilerSolutionFile);
                foreach (var item in val)
                {
                    if (item["名称"].Contains("循环泵"))
                    {
                        cost += double.Parse(item["间接价格"]);
                    }
                }
            }
            return cost;
        }

        //计算总造价
        public double GetTotalCost()
        {
            double cost = 0;
            if (this.BoilerSolutionFile != null && this.BoilerSolutionFile.Length > 0)
            {
                List<Dictionary<string, string>> val = ExcelUtil.readSheet(this.BoilerSolutionFile);
                foreach (var item in val)
                {
                    if (item["参数"].Contains("总价"))
                    {
                        cost += double.Parse(item["间接价格"]);
                    }
                }
                cost += GetPipeLineCost();

            }
            return cost;
        }
      

        #region AutoCAD
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

        public void Delete()
        {
            //remove itself from current solution
            OwnSolution.HeatProducers.Remove(BaseObjectId);
            //热源被删除，该圈选应该被删除
            if (OwnMasterDistrict != null)
            {
                OwnMasterDistrict.RemoveSelf();
            }
            //热源被删除，和热力站相连的圈选应该被删除
            if (OwnSlaveDistrict != null)
            {
                OwnSlaveDistrict.RemoveSelf();
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
        #endregion
    }
}
