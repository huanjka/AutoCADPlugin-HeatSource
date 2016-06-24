using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using HeatSource.Utils;
using HeatSource.Formula;
using HeatSource.View;

namespace HeatSource.Model
{
    public class Building : BaseModel
    {
        public static string modelType = "building";
        //建筑面积
        private double area;
        public double Area
        {
            set
            {
                area = value;
                Save();
            }
            get
            {
                return area;
            }
        }
        //楼房层数
        private int layerCount;
        public int LayerCount
        {
            set
            {
                layerCount = value;
                Save();
            }
            get
            {
                return layerCount;
            }
        }
        //建筑高度
        private double height;
        public double Height
        {
            set
            {
                height = value;
                Save();
            }
            get
            {
                return height;
            }
        }
        //供暖方式
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
        //节能措施
        private int energySavingStyle;
        public int EnergySavingStyle
        {
            set
            {
                energySavingStyle = value;
                this.heatIndex = GetBuildingHeatIndex();
                Save();
            }
            get
            {
                return energySavingStyle;
            }
        }

        //采暖热指标
        double heatIndex;
        public double HeatIndex
        {
            get
            {
               
                return heatIndex;
            }
            set
            {
                heatIndex = value;
                Save();
            }
        }
        //获取采暖热指标
        double GetBuildingHeatIndex()
        {
            string cityName = HeatSourceLayoutApp.globalProperty.CityName;
            string buildingType = Constants.BuildingType[BuildingType];
            int savaStyle = this.EnergySavingStyle;
            int i = Constants.BuildingType.IndexOf(buildingType);
            int locationType = HeatSourceLayoutApp.globalProperty.LocationType;
            switch(locationType)
            {
                case 0:
                    return Constants.SeverColdAreasHeatingIndex[i, savaStyle];
                case 1:
                    return Constants.ColdAreasHeatingIndex[i, savaStyle];
                case 2:
                    return Constants.SummerHotWinterColdAreasHeatingIndex[i, savaStyle];
                default:
                    return 60;

            }
        }

        //建筑类型
        private int buildingType;
        public int BuildingType
        {
            set
            {
                buildingType = value;
                this.heatIndex = GetBuildingHeatIndex();
                Save();
            }
            get
            {
                return buildingType;
            }
        }
        //采暖热负荷
        private double heatLoad;
        public double HeatLoad
        {
            get
            {
                heatLoad = new HeatingDesignLoad(this.HeatIndex, this.Area).GetResult();
                return heatLoad;
            }
            set
            {
                heatLoad = value;
                Save();
            }
        }
        //全年耗热量
        double yearHeat;
        public double YearHeat
        {
            get
            {
                int heatingDays = HeatSourceLayoutApp.globalProperty.HeatingDays;
                double outTemp = HeatSourceLayoutApp.globalProperty.OutDoorTemp;
                double outAverageTemp = HeatSourceLayoutApp.globalProperty.OutAverageTemp;
                double inTemp = HeatSourceLayoutApp.globalProperty.IndoorTemperature;
                yearHeat = new AnnualHeatConsumption(heatingDays, inTemp, outAverageTemp, outTemp, this.HeatLoad).GetResult();
                yearHeat = Math.Round(yearHeat, 2);
                return yearHeat;
            }
            set
            {
                yearHeat = value;
                Save();
            }

        }
        //楼房号
        private int buildingNum;
        public int BuildingNum
        {
            set
            {
                buildingNum = value;
                Save();
            }
            get
            {
                return buildingNum;
            }
        }

        public static int GetBuildingNumber()
        {
            int num = 1;
            while (true)
            {
                bool find = false;
                foreach (var item in HeatSourceLayoutApp.buildings)
                {
                    if (item.Value.BuildingNum == num)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                {
                    num++;
                    continue;
                }
                else
                {
                    return num;
                }
            }
        }

        private Dictionary<Solution, District> OwnDistrict { set; get; } //每一个楼房所在的区域，每一个楼房在一个解决方案中只能有唯一的区域
        //构造函数
        public Building(bool needId) : base(needId)
        {
            BaseObjectId = ObjectId.Null;
            this.Area = 0;
            this.LayerCount = 0;
            this.Height = 0;
            this.HeatStyle = 2;
            this.EnergySavingStyle = 0;
            this.BuildingType = 0;
            this.HeatLoad = 0;
            this.YearHeat = 0;
            OwnDistrict = new Dictionary<Solution, District>();
            this.BuildingNum = 0;
            this.heatIndex = GetBuildingHeatIndex();
        }
        public void AddDistrict(Solution s, District d)
        {
            if (OwnDistrict.ContainsKey(s))
            {
                OwnDistrict[s] = d;
            }
            else
            {
                OwnDistrict.Add(s, d);
            }
            this.Save();
        }

        public void RemoveDistrict(Solution s)
        {
            if (OwnDistrict.ContainsKey(s))
            {
                OwnDistrict.Remove(s);
                this.Save();
            }
        }

        public bool HasDistrict(Solution s)
        {
            return OwnDistrict.ContainsKey(s);
        }


        protected override string ModelType()
        {
            return Building.modelType;
        }
        protected override void GetAttributes()
        {
            attrs.Clear();
            attrs.Add("area", this.Area.ToString());
            attrs.Add("layercount", this.LayerCount.ToString());
            attrs.Add("height", this.Height.ToString());
            attrs.Add("heatload", this.HeatLoad.ToString());
            attrs.Add("yearheat", this.YearHeat.ToString());
            //OwnDistrict
            string str = "";
            if (OwnDistrict.Count > 0)
            {
                foreach (var item in OwnDistrict)
                {
                    str += ModelIdManager.toString(item.Key.BaseModelId) + ":" + ModelIdManager.toString(item.Value.BaseModelId) + " ";
                }
                //去除最后的空格
                str = str.Substring(0, str.Length - 1);
                attrs.Add("district", str);
            }
            if (this.BuildingNum == 0)
            {
                this.RequireLock();
                this.AddBuildingNumber();
                this.ReleaseLock();
            }
            attrs.Add("buildingnum", this.BuildingNum.ToString());
            attrs.Add("buildingtype", this.buildingType.ToString());
            attrs.Add("heatindex", this.heatIndex.ToString());
        }


        protected override void _SetAttributes(Dictionary<String, String> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            if (attrs.ContainsKey("area"))
            {
                Area = double.Parse(attrs["area"]);
            }
            if (attrs.ContainsKey("layercount"))
            {
                LayerCount = int.Parse(attrs["layercount"]);
            }
            if (attrs.ContainsKey("height"))
            {
                Height = double.Parse(attrs["height"]);
            }
            if (attrs.ContainsKey("district"))
            {
                OwnDistrict.Clear();
                string str = attrs["district"];
                string[] strs = str.Split(' ');
                foreach (var item in strs)
                {
                    string[] kv = item.Split(':');
                    if (kv.Length == 2)
                    {
                        int id1 = ModelIdManager.Parse(kv[0]);
                        int id2 = ModelIdManager.Parse(kv[1]);
                        Solution s = (Solution)BaseModel.GetModelById(id1);
                        District t = (District)BaseModel.GetModelById(id2);
                        OwnDistrict[s] = t;
                    }
                }
            }
            if (attrs.ContainsKey("buildingnum"))
            {
                this.BuildingNum = int.Parse(attrs["buildingnum"]);
            }
            if(attrs.ContainsKey("buildingtype"))
            {
                this.BuildingType = int.Parse(attrs["buildingtype"]);
            }
            if(attrs.ContainsKey("heatindex"))
            {
                this.heatIndex = double.Parse(attrs["heatindex"]);
            }
        }


        public static Building SearchBuildingById(ObjectId bId)
        {
            if (HeatSource.HeatSourceLayoutApp.buildings.ContainsKey(bId))
            {
                return HeatSource.HeatSourceLayoutApp.buildings[bId];
            }
            else
                return null;

        }

        public bool isInPolyline(Polyline pline)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (Transaction trans = ed.Document.Database.TransactionManager.StartTransaction())
            {
                Polyline l = (Polyline)trans.GetObject(this.BaseObjectId, OpenMode.ForRead, false);
                for (int i = 0; i < l.NumberOfVertices; i++)
                {
                    if (Utils.Utility.PointInPolygon(l.GetPoint3dAt(i), pline) == false)
                    {
                        return false;
                    }
                }
                trans.Dispose();
            }
            return true;
        }

        public void Delete()
        {
            if (this.OwnDistrict != null)
            {
                foreach (var item in this.OwnDistrict)
                {
                    item.Value.Buildings.Remove(this);
                    if (item.Value.Buildings.Count == 0)
                    {
                        item.Value.RemoveSelf();
                    }
                }
            }
            HeatSourceLayoutApp.buildings.Remove(this.BaseObjectId);
        }

        public void UpdateOutlineWidth()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction trans = ed.Document.Database.TransactionManager.StartTransaction())
                {
                    Polyline l = (Polyline)trans.GetObject(this.BaseObjectId, OpenMode.ForWrite, false);
                    l.ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                    trans.Commit();
                    trans.Dispose();
                }
            }
        }
        public void AddBuildingNumber()
        {
            if (this.BuildingNum == 0)
            {
                this.BuildingNum = GetBuildingNumber();
            }
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            ObjectIdCollection col = new ObjectIdCollection();
            col.Add(this.BaseObjectId);
            using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {

                    Polyline l = (Polyline)trans.GetObject(this.BaseObjectId, OpenMode.ForRead, false);
                    Point3d center = new Point3d(0, 0, 0);
                    double minX = Double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                    for (int i = 0; i < l.NumberOfVertices; i++)
                    {
                        Point3d p = l.GetPoint3dAt(i);
                        if (p.X < minX)
                        {
                            minX = p.X;
                        }
                        if (p.Y < minY)
                        {
                            minY = p.Y;
                        }
                        if (p.X > maxX)
                        {
                            maxX = p.X;
                        }
                        if (p.Y > maxY)
                        {
                            maxY = p.Y;
                        }
                        center = center + (p - new Point3d());
                    }

                    center = center / (l.NumberOfVertices);
                    double radius = Math.Min(maxX - minX, maxY - minY) / 4;
                    Circle c = new Circle(center, new Vector3d(0, 0, 1), radius);
                    TextStyleTable ts = (TextStyleTable)trans.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                    ObjectId mtStyleid = Utility.GetDefaultStyleId();
                    MText text = new MText();
                    text.TextStyleId = mtStyleid;
                    text.Width = radius * Math.Sqrt(2);
                    text.Height = radius * Math.Sqrt(2);
                    text.TextHeight = text.Height * 0.8;
                    text.Contents = this.BuildingNum.ToString();
                    text.Location = new Point3d(center.X - text.ActualWidth / 2, center.Y + text.ActualHeight / 2, 0);
                    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    col.Add(ms.AppendEntity(text));
                    trans.AddNewlyCreatedDBObject(text, true);
                    col.Add(ms.AppendEntity(c));
                    trans.AddNewlyCreatedDBObject(c, true);
                    trans.Commit();
                }
            }
            Utility.createGroup(col, ModelIdManager.toString(this.BaseModelId));
        }

    }
}
