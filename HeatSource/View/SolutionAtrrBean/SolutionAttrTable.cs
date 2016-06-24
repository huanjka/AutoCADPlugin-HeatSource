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
    public class SolutionAttrTable
    {
        private Solution _solution;

        BaseInfo baseInfo;
        CustomAttrCollection heatInfos;
        TotoalInfo totalInfo;

        public SolutionAttrTable(Solution solution)
        {
            _solution = solution;
            baseInfo = new BaseInfo("基本信息");

            heatInfos = new CustomAttrCollection();
            int heattag = 1;
            if (solution != null)
            {
                if (solution.HeatProducers.Count != 0)
                {
                    foreach (var heatproducer in HeatSourceLayoutApp.currentSolution.HeatProducers.Values)
                    {
                        heatInfos.Add(new HeatProducerInfo("热源" + (heattag++), heatproducer));
                    }
                }
                totalInfo = new TotoalInfo("工程总表", _solution);
            }
        }


        [Category("基本信息")]
        [DisplayName("基本信息")]
        [PropertyOrder(1)]
        public BaseInfo Base
        {
            get { return baseInfo; }
        }

        [Category("热源信息")]
        [DisplayName("热源信息")]
        [PropertyOrder(2)]
        [ExpandableObject()]
        public CustomAttrCollection HeatInfos
        {
            get { return heatInfos; }
        }

        [Category("工程总表")]
        [DisplayName("工程总表")]
        [PropertyOrder(3)]
        public TotoalInfo Total
        {
            get { return totalInfo; }
        }
    }
}
