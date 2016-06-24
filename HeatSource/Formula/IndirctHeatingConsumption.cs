using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
       * Formula 12 - 计算外网
       * 使用公式12 前必须初始化公式1,3
       */
    class IndirctHeatingConsumption:Formula
    {
        public IndirctHeatingConsumption():this(0,0,0,0,0){}

        public IndirctHeatingConsumption(double heatingDays, double waterPumpFlow,double indoorTemp,
            double outdoorTemp,double outdoorAverageTemp)
        {
            this.parameters.Add("heatingDays", heatingDays);
            this.parameters.Add("waterPumpFlow", waterPumpFlow);
            this.parameters.Add("indoorTemp", indoorTemp);
            this.parameters.Add("outdoorTemp", outdoorTemp);
            this.parameters.Add("outdoorAverageTemp", outdoorAverageTemp);
        }
        public override double GetResult()
        {
            double waterPumpEffiency = 0;
            if (this.parameters["waterPumpFlow"] > 200)
                waterPumpEffiency = 0.71;
            else if (this.parameters["waterPumpFlow"] > 60)
                waterPumpEffiency = 0.69;
            else if (this.parameters["waterPumpFlow"] <= 60)
                waterPumpEffiency = 0.63;
            double indirectHeatPower = 1.2 * this.parameters["waterPumpFlow"] * 15 * 9.8
                / (3600 * waterPumpEffiency);

            double indoorTemp = this.parameters["indoorTemp"];
            double outdoorAverageTemp = this.parameters["outdoorAverageTemp"];
            double outdoorTemp = this.parameters["outdoorTemp"];

            double indirectHeatConsumption = 0;

            indirectHeatConsumption = 24 * this.parameters["heatingDays"] * indirectHeatPower
                * (indoorTemp - outdoorAverageTemp) / (indoorTemp - outdoorTemp);
            if (double.IsInfinity(indirectHeatConsumption))
                return 0;
            else if (double.IsNaN(indirectHeatConsumption))
                return 0;
            return indirectHeatConsumption;
        }
    }
}
