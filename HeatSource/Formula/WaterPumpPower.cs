using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /** 
        * Formula 5 - 计算水泵功率，改成每台水泵的功率
        * 使用公式5前必须初始化公式3和公式4
        * WaterPumpPower - Compute water pump power
        * @param waterPumpFlow : cycle water pump flow
        * @param waterPumpHead : designed head for each water pump.
        * @pumpRatio : designed working effiency for each water pump
        * @waterPumpPower : water pump power
        *
        */
    class WaterPumpPower:Formula
    {
        public const double const_g = 9.8;
        public const double const_1 = 1.2;
        public const int const_2 = 3600;

        public WaterPumpPower() : this(0, 0) { }

        public WaterPumpPower(double waterPumpHead,double waterPumpFlow)
        {
            this.parameters.Add("WaterPumpHead", waterPumpHead);
            this.parameters.Add("WaterPumpFlow", waterPumpFlow);
        }

        public override double GetResult()
        {
            double pumpRatio;
            double perWaterPumpFlow = this.parameters["WaterPumpFlow"];
            if(perWaterPumpFlow > 200)
                pumpRatio = 0.71;
            else if(perWaterPumpFlow < 60)
                pumpRatio = 0.63;
            else
                pumpRatio = 0.69;


            //增加水泵台数-计算所有水泵的总功率
            double waterPumpPower = 0;
            waterPumpPower = const_1 * this.parameters["WaterPumpFlow"] * this.parameters["WaterPumpHead"] * const_g / (const_2 * pumpRatio);

            if (double.IsNaN(waterPumpPower))
                return 0;
            else if (double.IsInfinity(waterPumpPower))
                return 0;
            else
                return waterPumpPower;
        
        }

    }

    
}
