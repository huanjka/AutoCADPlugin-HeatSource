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
    public class SolutionInfo : AttrInfo
    {
        private Solution _solution;
        public SolutionInfo(string name, Solution solution) : base(name) {
            _solution = solution;
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("运行成本")]
        [PropertyOrder(1)]
        public TotoalInfo TotalFee
        {
            get { return new TotoalInfo("运行成本", _solution); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("设备造价")]
        [PropertyOrder(2)]
        public SolutionDeviceInfo DevicePrice
        {
            get { return new SolutionDeviceInfo("设备造价", _solution); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
