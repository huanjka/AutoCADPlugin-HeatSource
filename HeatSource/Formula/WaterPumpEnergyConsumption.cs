using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 6 - 计算水泵全年运行能耗
        * 使用公式5前必须初始化公式1,2,3,4,5
        * WaterPumpEnergyConsumption - Compute water pump's annual year energy consumption 
        * @param heatingDsys : heating days
        * @param waterPumpPower : water pump power
        * @param indoorTemp : designed indoor temperature, user's own input 
        * @param outdoorMeanTemp : outdoor mean temperature during heating period
        * @param outdoorTemp : outdoor temperature during heating period 
        * @waterPumpEnergyConsumption : water pump's annual year energy consumption
        */
    class WaterPumpEnergyConsumption : Formula
    {
        public WaterPumpEnergyConsumption() : this(0, 0, 0, 0, 0) { }
        public WaterPumpEnergyConsumption(double heatingDays, double indoorDesignTemp, double outdoorMeanTemp, double outdoorTemp, double waterPumpPower)
        {
            this.parameters.Add("HeatingDays", heatingDays);
            this.parameters.Add("IndoorDesignTemp", indoorDesignTemp);
            this.parameters.Add("OutdoorMeanTemp", outdoorMeanTemp);
            this.parameters.Add("OutdoorTemp", outdoorTemp);
            this.parameters.Add("WaterPumpPower", waterPumpPower);
        }

        public override double GetResult()
        {

            double waterPumpEnergyConsumption = 1;
            waterPumpEnergyConsumption = 24 * this.parameters["HeatingDays"] * this.parameters["WaterPumpPower"]
                * (this.parameters["IndoorDesignTemp"] - this.parameters["OutdoorMeanTemp"])
                / (this.parameters["IndoorDesignTemp"] - this.parameters["OutdoorTemp"]);
            return waterPumpEnergyConsumption;
        }
    }
}
