using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace HeatSource.View.SolutionAtrrBean
{
    class SolutionAttrCompareTable
    {
        
        BaseInfo baseInfo;
        CustomAttrCollection solutionInfos;

        public SolutionAttrCompareTable()
        {
            baseInfo = new BaseInfo("基本信息");
            solutionInfos = new CustomAttrCollection();
            foreach (var solution in HeatSourceLayoutApp.solutions)
            {
                solutionInfos.Add(new SolutionInfo(solution.SolutionName, solution));
            }
        }

        [Category("基本信息")]
        [DisplayName("基本信息")]
        [PropertyOrder(1)]
        public BaseInfo Base
        {
            get { return baseInfo; }
        }

        [Category("方案信息")]
        [DisplayName("方案信息")]
        [PropertyOrder(2)]
        [ExpandableObject()]
        public CustomAttrCollection SolutionInfos
        {
            get { return solutionInfos; }
        }
    }
}
