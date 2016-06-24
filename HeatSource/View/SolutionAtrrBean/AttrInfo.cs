using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.View.SolutionAtrrBean
{
    public abstract class AttrInfo
    {
        public string name;
        public AttrInfo(string name)
        {
            this.name = name;
        }
    }
}
