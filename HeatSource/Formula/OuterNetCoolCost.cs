using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 8 - 外网散热损失费用
        * 使用公式8之前必须初始化公式1，2
        */
    class OuterNetCoolCost:Formula
    {
        public OuterNetCoolCost() : this(0,0) { }

        public OuterNetCoolCost(double gasPrice,double heatConsumption)
        {
            this.parameters.Add("GasPrice", gasPrice);
            this.parameters.Add("HeatConsumption", heatConsumption);
        }
        public override double GetResult()
        {
            double R = 38.931;
            double outerNetCoolCost = 0;
            outerNetCoolCost = 1000 * this.parameters["GasPrice"] * 0.05 * this.parameters["HeatConsumption"] / R;
            if (double.IsInfinity(outerNetCoolCost))
                return 0;
            else if (double.IsNaN(outerNetCoolCost))
                return 0;
            return outerNetCoolCost;

        }
    }
}
