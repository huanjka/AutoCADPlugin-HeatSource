using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 2 - 计算全年耗热量
        * AnnualHeatConsumption - Compute annual heat consumption
        * @param heatingDays : heating days
        * @param indoorTemp : designed indoor temperature, user's own input 
        * @param outdoorMeanTemp : outdoor mean temperature during heating period
        * @param outdoorTemp : outdoor temperature during heating period 
        * @annualHeatConsumption : annual heat consumption during heating period
        */
    class AnnualHeatConsumption :Formula
    {

        public const double heatConst = 0.0864;


        public AnnualHeatConsumption() : this(0, 0, 0, 0, 0) { }

        public AnnualHeatConsumption(double heatingDays, double indoorDesignTemp, double outdoorMeanTemp,
                             double outdoorTemp, double heatLoad)
        {
            this.parameters.Add("HeatingDays", heatingDays);
            this.parameters.Add("IndoorDesignTemp", indoorDesignTemp);
            this.parameters.Add("OutdoorMeanTemp", outdoorMeanTemp);
            this.parameters.Add("OutdoorTemp", outdoorTemp);
            this.parameters.Add("HeatingDesignLoad", heatLoad);
        }


        public override double GetResult()
        {
            double annualHeatConsumption = 1;
            annualHeatConsumption = heatConst * this.parameters["HeatingDays"] * this.parameters["HeatingDesignLoad"]
                * (this.parameters["IndoorDesignTemp"] - this.parameters["OutdoorMeanTemp"])
                / (this.parameters["IndoorDesignTemp"] - this.parameters["OutdoorTemp"]);
            return annualHeatConsumption;
        }

    }
}
