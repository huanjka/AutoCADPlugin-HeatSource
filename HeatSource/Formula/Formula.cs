using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Formula
{
    class Formula
    {
        /* README - 公式调用
         * 使用公式1无须初始化其他公式对象
         * 使用公式2必须先初始化公式1
         * 使用公式3必须先初始化公式1，2
         * 使用公式4必须先初始化公式1，2，3
         * 使用公式5必须先初始化公式1，2，3，4
         * 使用公式6必须先初始化公式1，2，3，4，5
         * 使用公式7必须先初始化公式1，2
         * 使用公式9必须先初始化公式1，2，3，4，5，6
         */


       protected Dictionary<String, double> parameters = new Dictionary<string, double>();

       public virtual double GetResult()
       {
           return 0;
       }

       //public void Save()
       //{
       //    foreach(var item in parameters)
       //    {
       //        item.Value.Save();
       //    }
       //}
       //public virtual void Load()
       //{

       //}

   }
}
