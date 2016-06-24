using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    /** 
        * Formula 1 - 计算采暖热负荷
        * HeatingDesignLoad - Compute design space-heating load
        * @param buildingArea : the user's own input
        * @param loadIndex : look-up in the database which depends on area or building types or building time scale
        * @heatingDesignLoad : design space-heating load
        */
    class HeatingDesignLoad : Formula
    {
        public HeatingDesignLoad() : this(0, 0) { }
        public HeatingDesignLoad(double loadIndex, double buildingArea)
        {
            this.parameters.Add("BuildingArea", buildingArea);
            this.parameters.Add("LoadIndex", loadIndex);
        }

        public override double GetResult()
        {
            double heatingDesignLoad = 1;
            heatingDesignLoad =  this.parameters["LoadIndex"] * this.parameters["BuildingArea"] / 1000;
            return heatingDesignLoad;
        }


    }
}
