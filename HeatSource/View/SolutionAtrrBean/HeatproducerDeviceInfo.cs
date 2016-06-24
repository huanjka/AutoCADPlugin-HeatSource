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
    public class HeatproducerDeviceInfo : AttrInfo
    {
        private HeatProducer _heatproducer;
        public HeatproducerDeviceInfo(string name, HeatProducer heatproducer)
            : base(name)
        {
            _heatproducer = heatproducer;
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("锅炉造价")]
        [PropertyOrder(1)]
        public string BoilerPrice
        {
            get { return Math.Round(_heatproducer.GetBoildersCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("换热器造价")]
        [PropertyOrder(2)]
        public string HeatChangerPrice
        {
            get { return Math.Round(_heatproducer.GetHeatExchangerCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("热源水泵造价")]
        [PropertyOrder(3)]
        public string PumpPrice
        {
            get { return Math.Round(_heatproducer.GetWaterPumpCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("管线造价")]
        [PropertyOrder(4)]
        public string PiplinePrice
        {
            get { return Math.Round(_heatproducer.GetPipeLineCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("总造价")]
        [PropertyOrder(5)]
        public string TotalPrice
        {
            get { return Math.Round(_heatproducer.GetTotalCost(), 2).ToString(); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
