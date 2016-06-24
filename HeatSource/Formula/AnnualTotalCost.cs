using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 10 - 计算总运行费用
        * 使用公式10前必须初始化公式1，2，3，4，5，6，7，9
        * AnnualOperationCost - Compute Annual Total Operation Cost
        * AnnualOperationCost = WaterPumpOperationCost + GasHeatingCost;
        */
    class AnnualTotalCost:Formula
    {
        public AnnualTotalCost() : this(0, 0) { }

        public AnnualTotalCost(double waterPumpOperationCost, double gasHeatingCost)
        {
            this.parameters.Add("WaterPumpOperationCost", waterPumpOperationCost);
            this.parameters.Add("GasHeatingCost", gasHeatingCost);

        }
        public override double GetResult()
        {
            double annualOperationCost = 1;
            annualOperationCost = this.parameters["WaterPumpOperationCost"] + this.parameters["GasHeatingCost"];
            return annualOperationCost;

        }
    }
}
