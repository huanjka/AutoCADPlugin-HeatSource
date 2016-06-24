using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Utils
{
    public class Error
    {
        public String Message { set; get; }
        public ErrorCode Err { set; get; }
        public enum ErrorCode
        {
            NoErr,
            ConflictErr,
            LogicalErr,
        }
    }
}
