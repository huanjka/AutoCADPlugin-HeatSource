using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 3 - 计算水泵流量,改为计算单台水泵流量
        * 调用公式3需先初始化公式1
        * WaterPumpFlow - Compute water pump flow
        * @param heatingDesignLoad - design space-heating load
        * @param tempDiff : the water temperature difference between inlet and outlet 
        * @waterPumpFlow : selection cycle water pump flow
        */
    class WaterPumpFlow : Formula
    {
        public WaterPumpFlow() : this(0, 0, 0) { }

        public WaterPumpFlow(double tempDiff, double heatingDesignLoad, double waterPumpCount)
        {
            this.parameters.Add("TempDiff", tempDiff);
            this.parameters.Add("heatingDesignLoad", heatingDesignLoad);
            this.parameters.Add("waterPumpCount", waterPumpCount);
        }

        public override double GetResult()
        {
            double waterPumpFlow = 1;
            double tempDiff = this.parameters["TempDiff"];
            waterPumpFlow = 1.1 * 0.86 * this.parameters["heatingDesignLoad"]
                / (tempDiff * this.parameters["waterPumpCount"]);
            if (double.IsNaN(waterPumpFlow))
                return 0;
            else if (double.IsInfinity(waterPumpFlow))
                return 0;
            else
                return waterPumpFlow;
        }

    }
}
