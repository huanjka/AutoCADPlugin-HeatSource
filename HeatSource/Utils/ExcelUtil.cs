using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace HeatSource.Utils
{
    class ExcelUtil
    {

        #region 热源选型软件设备参数表相关函数
        private static Dictionary<string, List<Dictionary<string, string>>> ExcelFileDataCache = new Dictionary<string, List<Dictionary<string, string>>>();
#if DEBUG
        private static string Excel_folder = HeatSourceLayoutApp.CurrentDirectory + @"/../../Excels/热源选型软件设备参数表/";
        private static string City_Excel_folder = HeatSourceLayoutApp.CurrentDirectory + @"/../../Excels/城市列表及相关热指标数据表/";
#else
        private static string Excel_folder = HeatSourceLayoutApp.CurrentDirectory + @"/Excels/热源选型软件设备参数表/";
        private static string City_Excel_folder = HeatSourceLayoutApp.CurrentDirectory + @"/Excels/城市列表及相关热指标数据表/";
#endif
        /// <summary>
        /// 读取特定热源选型软件设备参数
        /// </summary>
        /// <param name="filename">文件名，例如：总吨数700kw_台数1_间接_编号1.xlsx</param>
        /// <returns name="results">一个列表，每一项是Excel中的一行数据</returns>
        static public List<Dictionary<string, string>> readSheet(string filename)
        {
            if(ExcelFileDataCache.ContainsKey(filename))
            {
                return ExcelFileDataCache[filename];
            }
            filename = Excel_folder + filename;
            XDocument doc = XDocument.Load(filename.Replace("xlsx", "xml"));


            XNamespace ad = "urn:schemas-microsoft-com:office:spreadsheet";

            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

            string[] Fields = new string[8]; // A-H
            string[] Keys = new string[8];

            int rowcount = 0;
            foreach (XElement xl in doc.Root.Descendants(ad + "Row"))
            {
                if (rowcount < 1)
                {
                    rowcount++;
                    continue;
                }

                Dictionary<string, string> row = new Dictionary<string, string>();

                int count = 0;
                foreach (XElement cell in xl.Descendants(ad + "Cell"))
                {
                    if (rowcount == 1)
                    {
                        Keys[count] = cell.Value;
                        Fields[count] = "";
                    }
                    else
                        Fields[count] = cell.Value;
                    count++;
                }

                if (rowcount >= 1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (Fields[i] != null)
                            row[Keys[i].Replace(" ", "")] = Fields[i];
                        else
                            row[Keys[i].Replace(" ", "")] = null;
                    }
                }

                results.Add(row);

                rowcount++;
            }

            
            ExcelFileDataCache[filename] = results;
            
            return results;
        }

        /// <summary>
        /// 返回excel目录下所有xlsx文件的名字
        /// </summary>
        /// <returns></returns>
        static public List<string> getExcelNamelist()
        {
            List<string> ExcelNames = new List<string>();

            DirectoryInfo folder = new DirectoryInfo(Excel_folder);
            foreach (FileInfo file in folder.GetFiles("*.xlsx"))
            {
                string filename = file.Name;
                if (filename.Contains("$")) // 可能打开临时文件，过滤掉
                {
                    continue;
                }
                ExcelNames.Add(filename);
            }

            return ExcelNames;
        }
        #endregion

        #region 城市列表及城市的热指标数据

        /// <summary>
        /// 返回城市列表
        /// </summary>
        /// <returns></returns>
        static public List<List<string>> citylist()
        {
            XDocument doc = XDocument.Load(City_Excel_folder + "城市列表.xml");


            XNamespace ad = "urn:schemas-microsoft-com:office:spreadsheet";

            List<List<string>> results = new List<List<string>>();

            foreach (XElement xl in doc.Root.Descendants(ad + "Row"))
            {
                List<string> list = new List<string>();
                foreach (XElement cell in xl.Descendants(ad + "Cell"))
                {
                    list.Add(cell.Value);
                }
                list.RemoveAt(0);
                results.Add(list);
            }

            return results;
        }

        static public List<string> getSeverColdAreas()
        {
            XDocument doc = XDocument.Load(City_Excel_folder + "严寒地区.xml");

            XNamespace ad = "urn:schemas-microsoft-com:office:spreadsheet";
            List<string> results = new List<string>();
            foreach (XElement xl in doc.Root.Descendants(ad + "Row"))
            {
                foreach (XElement cell in xl.Descendants(ad + "Cell"))
                {
                    results.Add(cell.Value);
                }
            }
            return results;
        }

        static public List<string> getColdAreas()
        {
            XDocument doc = XDocument.Load(City_Excel_folder + "寒冷地区.xml");

            XNamespace ad = "urn:schemas-microsoft-com:office:spreadsheet";
            List<string> results = new List<string>();
            foreach (XElement xl in doc.Root.Descendants(ad + "Row"))
            {
                foreach (XElement cell in xl.Descendants(ad + "Cell"))
                {
                    results.Add(cell.Value);
                }
            }
            return results;
        }

        static public List<string> getSummerHotWinterColdAreas()
        {
            XDocument doc = XDocument.Load(City_Excel_folder + "夏热冬冷地区.xml");

            XNamespace ad = "urn:schemas-microsoft-com:office:spreadsheet";
            List<string> results = new List<string>();
            foreach (XElement xl in doc.Root.Descendants(ad + "Row"))
            {
                foreach (XElement cell in xl.Descendants(ad + "Cell"))
                {
                    results.Add(cell.Value);
                }
            }
            return results;
        }
        

        #endregion
    }
}
