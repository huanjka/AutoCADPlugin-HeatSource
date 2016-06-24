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
    public class SubstationInfo : AttrInfo
    {
        SubStation _substation;
        public SubstationInfo(string name, SubStation substation)
            : base(name)
        {
            _substation = substation;
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("供暖方式")]
        [PropertyOrder(1)]
        public string HeatStyle
        {
            get { return Constants.HeatStyle[_substation.HeatStyle-1]; }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("换热器台数")]
        [PropertyOrder(2)]
        public string HeatSwitcherCount
        {
            get { return _substation.HeatSwitcherCount.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("换热器容量")]
        [PropertyOrder(3)]
        public string HeatSwitcherVolumn
        {
            get { return Math.Round(_substation.HeatSwitcherVolumn, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵台数")]
        [PropertyOrder(4)]
        public string WaterPumpCount
        {
            get { return _substation.WaterPumpCount.ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵流量")]
        [PropertyOrder(5)]
        public string WaterPumpVolume
        {
            get { return Math.Round(_substation.WaterPumpVolume, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵扬程")]
        [PropertyOrder(6)]
        public string WaterPumpLift
        {
            get { return Math.Round(_substation.WaterPumpLift, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵功率")]
        [PropertyOrder(7)]
        public string WaterPumpPower
        {
            get { return Math.Round(_substation.WaterPumpPower, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("热力站总负荷")]
        [PropertyOrder(8)]
        public string TotalHeatingDesignLoad
        {
            get { return Math.Round(_substation.TotalHeatingDesignLoad, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("主管道长度")]
        [PropertyOrder(9)]
        public string MasterPipeLength
        {
            get { return Math.Round(_substation.MasterPipeLength, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("管道总长度")]
        [PropertyOrder(10)]
        public string TotoalPipeLength
        {
            get { return Math.Round(_substation.TotoalPipeLength, 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵供回水温度")]
        [PropertyOrder(11)]
        public string WaterPumpTemp
        {
            get { return Math.Round(_substation.WaterPumpTemp, 2).ToString(); }
        }


        public override string ToString()
        {
            return "";
        }
    }
}
