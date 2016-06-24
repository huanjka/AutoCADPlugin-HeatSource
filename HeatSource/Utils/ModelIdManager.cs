using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using Autodesk.AutoCAD.DatabaseServices;

namespace HeatSource.Utils
{
    class ModelIdManager
    {
        //这个必须初始化
        private static int  num = 0;
        
        //model Id 是从1开始的
        public static int nextModelId()
        {
            return ++num;
        }

        public static int Parse(String name)
        {
            if(name.Length < 10)
            {
                Logging.WriteMessage("Parse Model Id 错误");
            }
            int v = int.Parse(name.Substring(10));
            if(v > num)
            {
                num = v;
            }
            return v;
        }

        public static String toString(int id)
        {
            String result = "basemodel_";
            result = result + String.Format("{0:D8}", id);
            return result; 
        }
    }
}
