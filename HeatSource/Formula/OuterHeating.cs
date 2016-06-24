using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    //计算热源或热力站的外线
    class OuterHeating : Formula
    {
        public OuterHeating() : this(0, 0, 0, 0) { }

        public OuterHeating(double heatingDesignLoad, double tempDiff, double supplyWaterTemp, double totalPipeLength)
        {
            this.parameters.Add("heatingDesignLoad", heatingDesignLoad);
            this.parameters.Add("tempDiff", tempDiff);
            this.parameters.Add("supplyWaterTemp", supplyWaterTemp);
            this.parameters.Add("totalPipeLength", totalPipeLength);
        }

        public override double GetResult()
        {
            double heatingDesignLoad = this.parameters["heatingDesignLoad"];
            double supplyWaterTemp = this.parameters["supplyWaterTemp"];
            double totalPipeLength = this.parameters["totalPipeLength"];
            double tempDiff = this.parameters["tempDiff"];

            //计算管道设计流量
            double pipeDesignFlow = 2.38 * 0.0001 * heatingDesignLoad / tempDiff;

            //计算外线平均管径
            double x = 4 * pipeDesignFlow / 1.5;
            double y = x / 3.14;
            double z = Math.Sqrt(y) * 2 / 3; 
            double dia = z;
            //计算管道散热量
            double pipeHeatRelease = 0;
            if (supplyWaterTemp <= 50)
                pipeHeatRelease = supplyWaterTemp / 50 * 104;
            else if (supplyWaterTemp <= 100)
                pipeHeatRelease = 104 + (supplyWaterTemp - 50) / (100 - 50) * (147 - 104);
            else if (supplyWaterTemp <= 150)
                pipeHeatRelease = 147 + (supplyWaterTemp - 100) / 50 * (183 - 147);
            else if (supplyWaterTemp <= 200)
                pipeHeatRelease = 183 + (supplyWaterTemp - 150) / 50 * (220 - 183);
            else if (supplyWaterTemp <= 250)
                pipeHeatRelease = 220 + (supplyWaterTemp - 200) / 50 * (251 - 220);
            else if (supplyWaterTemp <= 300)
                pipeHeatRelease = 251 + (supplyWaterTemp - 250) / 50 * (272 - 251);
            else if (supplyWaterTemp > 300)
                pipeHeatRelease = supplyWaterTemp / 300 * 272;

            //计算外线热损失
            double outHeating = 3.14 * dia * totalPipeLength * pipeHeatRelease * 0.001;
            if (double.IsInfinity(outHeating))
                return 0;
            else if (double.IsNaN(outHeating))
                return 0;
            return outHeating;
        }
    }
}
