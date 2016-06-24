using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /**
        * Formula 4 - 计算水泵扬程，改成单台水泵扬程
        * 使用公式4前必须初始化公式1，3
        * WaterPumpHead - Calculate water pump head
        * @param waterPumpFlow : cycle water pump flow
        * @param totalLength : length of outdoor main trunk, provided automatically after drawing
        * @param tempDiff : the water temperature difference between inlet and outlet 
        * @A : calculation factor depending on water pump flow
        * @B : calculation factor depending on pump level
        * @a : calculation factor depending on total_L
        * @pumpHead : water pump head
        */
    class WaterPumpHead : Formula
    {
        public WaterPumpHead() : this(0, 0, 0, 0,0) { }

        //masterPipeLength 双向主管道长度
        public WaterPumpHead(double tempDiff, double masterPipeLength, double waterPumpFlow, double totalHeatingDesignLoad,double waterPumpCount)
        {
            this.parameters.Add("TempDiff", tempDiff);
            this.parameters.Add("MasterPipeLength", masterPipeLength);
            this.parameters.Add("WaterPumpFlow", waterPumpFlow);
            this.parameters.Add("totalHeatingDesignLoad", totalHeatingDesignLoad);
            this.parameters.Add("waterPumpCount", waterPumpCount);
        }

        public override double GetResult()
        {
            double perWaterPumpFlow = this.parameters["WaterPumpFlow"];
            double totalHeatingDesignLoad = this.parameters["totalHeatingDesignLoad"];
            double tempDiff = this.parameters["TempDiff"];

            double pumpRatio = 0;
            double A = 0;
            if (perWaterPumpFlow > 200)
            {
                A = 0.003749;
                pumpRatio = 0.71;
            }
            else if (perWaterPumpFlow <= 60)
            {
                A = 0.004225;
                pumpRatio = 0.63;
            }
            else
            {
                A = 0.003858;
                pumpRatio = 0.69;
            }

            double a;
            double masterPipeLength = this.parameters["MasterPipeLength"];

            if (masterPipeLength <= 400)
                a = 0.0015;
            else if (masterPipeLength >= 1000)
                a = 0.0069;
            else
                a = 0.003833 + 3.067 / masterPipeLength;

            double temp = A * (20.4 + a * masterPipeLength) / tempDiff;

            double pumpHead = 0;
            pumpHead = temp * pumpRatio * totalHeatingDesignLoad / (0.003096 * perWaterPumpFlow);
            pumpHead = pumpHead / this.parameters["waterPumpCount"]; 

            if (double.IsNaN(pumpHead))
                return 0;
            else if (double.IsInfinity(pumpHead))
                return 0;
            else
                return pumpHead;

        }
    }
}
