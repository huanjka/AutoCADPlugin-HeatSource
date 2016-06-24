using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using HeatSource.Model;

namespace HeatSource.View.SolutionAtrrBean
{

    [ExpandableObject()]
    public class BuildingInfo : AttrInfo
    {
        Building _building;
        public BuildingInfo(string name, Building building) : base(name) {
            _building = building;
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("建筑类型")]
        [PropertyOrder(1)]
        public string Type
        {
            get { return Constants.BuildingType[_building.BuildingType]; }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("建筑面积")]
        [PropertyOrder(2)]
        public string Area
        {
            get { return _building.Area.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("供暖方式")]
        [PropertyOrder(3)]
        public string HeatStyle
        {
            get { return Constants.HeatStyle[_building.HeatStyle]; }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("节能措施")]
        [PropertyOrder(4)]
        public string SaveMethods
        {
            get { return Constants.SaveStyle[_building.EnergySavingStyle]; }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("采暖热指标")]
        [PropertyOrder(5)]
        public string HeatIndex
        {
            get { return _building.HeatIndex.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("采暖热负荷")]
        [PropertyOrder(6)]
        public string HeatLoad
        {
            get { return Math.Round(_building.HeatLoad, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("全年热耗量")]
        [PropertyOrder(7)]
        public string HeatYear
        {
            get { return Math.Round(_building.YearHeat, 2).ToString(); }
        }

        //[Category(Constants.CATEGORY_DETAIL)]
        //[DisplayName("燃气供热运行费用")]
        //[PropertyOrder(8)]
        //public string GasHeatFee
        //{
        //    get { return "2"; }
        //}

        public override string ToString()
        {
            return "";
        }
    }
}
