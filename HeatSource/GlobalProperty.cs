using Autodesk.AutoCAD.DatabaseServices;
using HeatSource.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeatSource.Utils;
using HeatSource.View;

namespace HeatSource
{
    //global property 的属性绑定到outlinelayer上
    public class GlobalProperty:BaseModel
    {
        #region UI Control
        private double buildingOutlineWidth = 10;
        public double BuildingOutlineWidth
        {
            get
            {
                return buildingOutlineWidth;
            }
            set
            {
                buildingOutlineWidth = value;
                this.Save();
            }
        }
        private double blockScale = 0.0025;
        public double BlockScale
        {
            get
            {
                return blockScale;
            }
            set
            {
                blockScale = value;
                this.Save();
            }
        }

        private double pipeLineScale = 1;
        public double PipeLineScale
        {
            set
            {
                pipeLineScale = value;
                this.Save();
            }
            get
            {
                return pipeLineScale;
            }
        }

        #endregion

        private int provinceId = 0;

        public int ProvinceId
        {
            set
            {
                provinceId = value;
                this.Save();
            }
            get
            {
                return provinceId;
            }
        }

        private int activeTab = -1;
        public int ActiveTab
        {
            set
            {
                activeTab = value;
                this.Save();
            }
            get
            {
                return activeTab;
            }
        }

        private int cityId = 0;
        public int CityId
        {
            set
            {
                cityId = value;
                this.Save();
            }
            get
            {
                return cityId;
            }
        }

        private string cityName = "北京市";
        public string CityName
        {
            set
            {
                cityName = value;
                this.Save();
            }
            get
            {
                return cityName;
            }
        }
        //地区类型
        private int locationType = 0;
        public int LocationType
        {
            set
            {
                locationType = value;
                this.Save();
            }
            get
            {
                //更改地区类型
                //严寒地区
                if (Constants.SeverColdAreas.Contains(cityName))
                {
                    locationType = 0;
                }
                //寒冷地区
                else if (Constants.ColdAreas.Contains(cityName))
                {
                    locationType = 1;
                }
                //夏热冬冷地区
                else if (Constants.SummerHotWinterColdAreas.Contains(cityName))
                {
                    locationType = 2;
                }
                return locationType;
            }
        }


        private double indoorTemperature = 18;
        public double IndoorTemperature
        {
            get
            {
                return indoorTemperature;
            }
            set
            {
                indoorTemperature = value;
                this.Save();
            }
        }

        private int heatingDays = 123;

        public int HeatingDays
        {
            get
            {
                return heatingDays;
            }
            set
            {
                this.heatingDays = value;
                this.Save();
            }
        }
        private double outDoorTemp = -7.6;
        public double OutDoorTemp {
            get
            {
                return outDoorTemp;
            }
            set
            {
                this.outDoorTemp = value;
                Save();
            }
        }
        private double outAverTemp = -0.7;
        public double OutAverageTemp
        {
            get
            {
                return outAverTemp;
            }
            set
            {
                this.outAverTemp = value;
                Save();
            }
        }
        public double WaterTempDiff  { get; set; }

        private double elecPrice = 0.5;
        public double ElecPrice
        {
            set
            {
                elecPrice = value;
                this.Save();
            }
            get
            {
                return elecPrice;
            }
        }

        public double HeatValueofGas { set; get; }
        private double gasPrice = 2.5;
        public double GasPrice
        {
            get
            {
                return gasPrice;
            }
            set
            {
                gasPrice = value;
                Save();
            }
        }

        private double graphScale = 1;
        public double GraphScale
        {
            get
            {
                return graphScale;
            }
            set
            {
                graphScale = value;
                Save();
            }
        }

        private ObjectId rasterImageID = ObjectId.Null;
        public ObjectId RasterImageID {
            set { rasterImageID = value;  Save(); }
            get { return rasterImageID; } }
        public GlobalProperty(bool needId):base(needId)
        { 

        }

        public  void Initialize()
        {
            ObjectId id = Utils.Utility.ContainLayer("our_outline_layer");
            if (id != ObjectId.Null)
            {
                this.BaseObjectId = id;
                Dictionary<string, string> attrs = BaseModel.Load(this.BaseObjectId);
                if (attrs != null)
                {
                    this.ResetAttributes(attrs, BaseObjectId);
                }
            }
            else
            {
                this.BaseObjectId = Utils.Utility.CreateLayer("our_outline_layer");
            }
        }

        protected override string ModelType()
        {
            return "globalproperty";
        }

        protected override void GetAttributes()
        {
            attrs.Clear();
            attrs.Add("CityId",CityId.ToString());
            attrs.Add("ProvinceId", ProvinceId.ToString());
            attrs.Add("IndoorTemperature", IndoorTemperature.ToString());
            attrs.Add("GraphScale", GraphScale.ToString());
            attrs.Add("ActiveTab", ActiveTab.ToString());
            attrs.Add("buildingwidth", BuildingOutlineWidth.ToString());
            attrs.Add("blockscale", BlockScale.ToString());
            attrs.Add("pipeLineScale", PipeLineScale.ToString());
            attrs.Add("elecprice", ElecPrice.ToString());
            attrs.Add("gasprice", GasPrice.ToString());
            if(rasterImageID.IsNull == false)
            {
                attrs.Add("rasterImage", rasterImageID.Handle.ToString());
            }
        }

        protected override void _SetAttributes(Dictionary<string, string> attrs)
        {
            if(attrs.ContainsKey("rasterImage"))
            {
                rasterImageID = Utility.GetObjectId(attrs["rasterImage"]);
            }
            if (attrs.ContainsKey("ActiveTab"))
            {
                activeTab = int.Parse(attrs["ActiveTab"]);
            }
            
            if(attrs.ContainsKey("CityId"))
            {
                cityId = int.Parse(attrs["CityId"]);
            }
            if(attrs.ContainsKey("ProvinceId"))
            {
                provinceId = int.Parse(attrs["ProvinceId"]);
            }
            if(attrs.ContainsKey("IndoorTemperature"))
            {
                indoorTemperature = double.Parse(attrs["IndoorTemperature"]);
            }
            if(attrs.ContainsKey("GraphScale"))
            {
                graphScale = int.Parse(attrs["GraphScale"]);
            }
            if (attrs.ContainsKey("buildingwidth"))
            {
                buildingOutlineWidth = double.Parse(attrs["buildingwidth"]);
            }
            if(attrs.ContainsKey("blockscale"))
            {
                blockScale = double.Parse(attrs["blockscale"]);
            }
            if(attrs.ContainsKey("pipeLineScale"))
            {
                pipeLineScale = double.Parse(attrs["pipeLineScale"]);
            }
            if(attrs.ContainsKey("elecprice"))
            {
               elecPrice = double.Parse(attrs["elecprice"]);
            }
            if(attrs.ContainsKey("gasprice"))
            {
                gasPrice = double.Parse(attrs["gasprice"]);
            }
        }
    }
}
