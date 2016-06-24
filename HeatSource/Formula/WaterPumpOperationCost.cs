using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
     * Formuala 9 - 计算水泵运行费用
     * 使用公式9前必须初始化公式1，2，3，4，5，6
     * WaterPumpOperationCost - Compute Water Pump Operation Cost
     */
    class WaterPumpOperationCost:Formula
    {
        public WaterPumpOperationCost() : this(0, 0) { }

        public WaterPumpOperationCost(double electricityPrice, double waterPumpEnergyConsumption)
        {
            this.parameters.Add("ElectricityPrice", electricityPrice);
            this.parameters.Add("WaterPumpEnergyConsumption", waterPumpEnergyConsumption);
        }
        public override double GetResult()
        {
            double waterPumpOperationCost = 1;
            waterPumpOperationCost = this.parameters["ElectricityPrice"] * this.parameters["WaterPumpEnergyConsumption"];
            return waterPumpOperationCost;
        }
    }
}
