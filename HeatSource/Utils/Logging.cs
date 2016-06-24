using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Utils
{
    //该类实现能够输出到AUTOCAD Command Line的Exception 类
    class Logging
    {
        public static void WriteMessage(string msg)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
        }
        
    }
}
