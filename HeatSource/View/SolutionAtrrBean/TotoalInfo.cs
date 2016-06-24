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
    public class TotoalInfo : AttrInfo
    {
        private Solution _solution;
        public TotoalInfo(string name, Solution solution) : base(name) {
            _solution = solution;
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("建筑总面积")]
        [PropertyOrder(1)]
        public string TotalArea
        {
            get { return Math.Round(HeatSourceLayoutApp.TotalBuildingsArea(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("全年总耗热量")]
        [PropertyOrder(2)]
        public string TotalHeat
        {

            get { return Math.Round(_solution.TotalLoad, 2).ToString(); }

        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("燃气供热运行费用")]
        [PropertyOrder(3)]
        public string TotalHeatFee
        {
            get { return Math.Round(_solution.GetGasHeatingCost(), 2).ToString(); }

        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("水泵运行费用")]
        [PropertyOrder(5)]
        public string TotalPumpFee
        {

            get { return Math.Round(_solution.GetWaterPumpCost(), 2).ToString(); }
        }

        [Category(Constants.CATEGORY_DETAIL)]
        [DisplayName("总运行成本")]
        [PropertyOrder(6)]
        public string TotalFee
        {

            get { return Math.Round(_solution.GetTotalOperationCost(), 2).ToString(); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
