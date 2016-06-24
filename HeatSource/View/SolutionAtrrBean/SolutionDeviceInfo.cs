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
    public class SolutionDeviceInfo : AttrInfo
    {
        private Solution _solution;
        public SolutionDeviceInfo(string name, Solution solution)
            : base(name)
        {
            _solution = solution;
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("锅炉造价")]
        [PropertyOrder(1)]
        public string BoilerPrice
        {
            get { return Math.Round(_solution.GetBoildersCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("换热器造价")]
        [PropertyOrder(2)]
        public string HeatChangerPrice
        {
            get { return Math.Round(_solution.GetHeatExchangerCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵总造价")]
        [PropertyOrder(3)]
        public string PumpPrice
        {
            get { return Math.Round(_solution.GetWaterPumpDeviceCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("管线造价")]
        [PropertyOrder(4)]
        public string PiplinePrice
        {
            get { return Math.Round(_solution.GetPipeLineCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("总造价")]
        [PropertyOrder(5)]
        public string TotalPrice
        {
            get { return Math.Round(_solution.GetTotalCost()).ToString(); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
