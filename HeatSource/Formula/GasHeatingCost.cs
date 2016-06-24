using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 7 - 计算燃气供热运行费用
        * 使用公式7前必须初始化公式2
        * GasHeatingCost - Compute gas heating expense
        * @param gasPrice : gas price
        * @param gasHeatingValue : gas heating value, fixed value
        * @param annualHeatConsumption : annual heat consumption during heating period
        * @param effiency : boiler effiency
        * @gasHeatingCost : total gas heating expense
        */
    class GasHeatingCost : Formula
    {
        public const double gasHeatingValue = 38.931;
        public GasHeatingCost() : this(0, 0, 0) { }

        public GasHeatingCost(double gasPrice, double effiency, double annualHeatConsumption)
        {
            this.parameters.Add("GasPrice", gasPrice);
            this.parameters.Add("Effiency", effiency);
            this.parameters.Add("AnnualHeatConsumption", annualHeatConsumption);

        }

        public override double GetResult()
        {
            double gasHeatingCost = 0;
            gasHeatingCost = 1000 * this.parameters["GasPrice"]
                * this.parameters["AnnualHeatConsumption"]
                / (gasHeatingValue * this.parameters["Effiency"]/100);
            return gasHeatingCost;
        }
    }
}
