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
    public class HeatProducerInfo : AttrInfo
    {
        HeatProducer _heatproducer;
        CustomAttrCollection substationInfos;
        CustomAttrCollection buildingInfos;

        public HeatProducerInfo(string name, HeatProducer heatproducer)
            : base(name) 
        {
            _heatproducer = heatproducer;
            buildingInfos = new CustomAttrCollection();
            substationInfos = new CustomAttrCollection();

            // get all buildings either directly connecting or indirectly
            List<Building> buildings = new List<Building>();
            if (_heatproducer.OwnMasterDistrict != null) {
                buildings.AddRange(_heatproducer.OwnMasterDistrict.Buildings); // direct
            }
            if (heatproducer.OwnSlaveDistrict != null)
            {
                int subtag = 1;
                foreach (var sub in heatproducer.OwnSlaveDistrict.SubStations) // indirect
                {
                    substationInfos.Add(new SubstationInfo("热力站" + (subtag++), sub));
                    if (sub.OwnMasterDistrict != null)
                    {
                        buildings.AddRange(sub.OwnMasterDistrict.Buildings);
                    }
                }
            }
            if (buildings.Count != 0)
            {
                foreach (var b in buildings)
                {
                    buildingInfos.Add(new BuildingInfo("楼房" + b.BuildingNum, b));
                }
            }
        }

        [Category("运行成本")]
        [DisplayName("水泵流量")]
        [PropertyOrder(1)]
        public string PumpVolumn
        {
            get { return Math.Round(_heatproducer.WaterPumpVolume, 2).ToString(); }
        }

        [Category("运行成本")]
        [DisplayName("水泵扬程")]
        [PropertyOrder(2)]
        public string PumpLift
        {
            get { return Math.Round(_heatproducer.WaterPumpLift, 2).ToString(); }
        }

        [Category("运行成本")]
        [DisplayName("水泵功率")]
        [PropertyOrder(3)]
        public string PumpPower
        {
            get { return Math.Round(_heatproducer.WaterPumpPower, 2).ToString(); }
        }

        [Category("运行成本")]
        [DisplayName("水泵全年运行能耗")]
        [PropertyOrder(4)]
        public string PumpYearEnergy
        {
            get { return Math.Round(_heatproducer.totalWaterPumpEnergyConsumption, 2).ToString(); }
        }

        [Category("运行成本")]
        [DisplayName("水泵运行费用")]
        [PropertyOrder(5)]
        public string PumpFee
        {
            get { return Math.Round(_heatproducer.GetWaterPumpOperationCost(), 2).ToString(); }
        }

        [Category("运行成本")]
        [DisplayName("总运行费用")]
        [PropertyOrder(6)]
        public string TotalFee
        {
            get { return Math.Round(_heatproducer.GetTotalOperationCost(), 2).ToString(); }
        }

        [Category("运行成本")]
        [DisplayName("热力站信息")]
        [PropertyOrder(7)]
        [ExpandableObject()]
        public CustomAttrCollection SubstationInfos
        {
            get { return substationInfos; }
        }

        [Category("运行成本")]
        [DisplayName("楼房信息")]
        [PropertyOrder(8)]
        [ExpandableObject()]
        public CustomAttrCollection BuildingInfos
        {
            get { return buildingInfos; }
        }

        [Category("设备造价")]
        [DisplayName("设备造价")]
        [PropertyOrder(1)]
        public HeatproducerDeviceInfo DeviceInfo
        {
            get { return new HeatproducerDeviceInfo("设备造价", _heatproducer); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
