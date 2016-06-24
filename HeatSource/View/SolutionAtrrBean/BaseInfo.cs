using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Collections;
using HeatSource.Utils;

namespace HeatSource.View.SolutionAtrrBean
{

    [ExpandableObject()]
    public class BaseInfo : AttrInfo
    {
        public BaseInfo(string name) : base(name) { }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("地点")]
        [PropertyOrder(1)]
        public string City
        {
            get {
                return HeatSourceLayoutApp.globalProperty.CityName;
            }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("地区类型")]
        [PropertyOrder(2)]
        public string Location
        {
            get {
                return Constants.LocationType[HeatSourceLayoutApp.globalProperty.LocationType];
            }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("室内设计温度")]
        [PropertyOrder(3)]
        public string TempInside
        {
            get { return HeatSourceLayoutApp.globalProperty.IndoorTemperature.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("采暖期室外温度")]
        [PropertyOrder(4)]
        public string TempOutside
        {
            get { return HeatSourceLayoutApp.globalProperty.OutDoorTemp.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("采暖期室外均温")]
        [PropertyOrder(5)]
        public string TempAverageOutside
        {
            get { return HeatSourceLayoutApp.globalProperty.OutAverageTemp.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("供暖天数")]
        [PropertyOrder(6)]
        public string HeatDays
        {
            get { return HeatSourceLayoutApp.globalProperty.HeatingDays.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("燃气单价")]
        [PropertyOrder(8)]
        public string GasPrice
        {
            get { return HeatSourceLayoutApp.globalProperty.GasPrice.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("电费单价")]
        [PropertyOrder(9)]
        public string ElecPrice
        {
            get { return HeatSourceLayoutApp.globalProperty.ElecPrice.ToString(); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
