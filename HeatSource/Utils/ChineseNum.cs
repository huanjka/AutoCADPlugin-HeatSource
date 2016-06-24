using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Utils
{
    class ChineseNum
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static String NumToChinese(int num)
        {
            if (num < 1 || num > 99)
            {
                throw new Exception("范围取值为1-99");

            }
            String[] a = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            if (num < 11)
            {
                return a[num - 1];
            }
            else
            {
                if ((num % 10) == 0)
                {
                    return a[(num / 10) - 1] + "十";
                }
                else if (num < 20)
                {
                    return "十" + a[(num % 10) - 1];
                }
                else
                {
                    return a[(num / 10) - 1] + "十" + a[(num % 10) - 1];
                }
            }

        }
    }
}
