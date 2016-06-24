using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

namespace HeatSource.Utils
{
    class DataConfig
    {
#if DEBUG
        public static string CONFIG_PATH = HeatSourceLayoutApp.CurrentDirectory + @"/../../Config/";
#else
        public static string CONFIG_PATH = HeatSourceLayoutApp.CurrentDirectory + @"/Config/";
#endif

        public static void init_config_path()
        {
#if DEBUG
        CONFIG_PATH = HeatSourceLayoutApp.CurrentDirectory + @"/../../Config/";
#else
        CONFIG_PATH = HeatSourceLayoutApp.CurrentDirectory + @"/Config/";
#endif
    }
    public static void loadData()
        {
            loadCurrentVersion();
            loadGB50736_2012();
            loadCustomConfig();
        }

#region Config Version
        private static String current_version;

        public static void loadCurrentVersion()
        {
            current_version = "";
        }

        public static String getCurrentVersion()
        {
            return current_version;
        }

        public static int setCurrentVersion(String version)
        {
            current_version = version;
            return 1;
        }
#endregion

#region Config GB50736_2012
        //Usage: map[cityname_attributename] = data, 
        //ex. City_attribute_map["北京_年平均温度"] = 12.3 
        private static Dictionary<String, String> City_attribute_map = new Dictionary<String, String>();
        private static String[] city_list;
        private static String[] attribute_list;

        public static void loadGB50736_2012()
        {
            XmlDocument doc = new XmlDocument();
            city_list = System.IO.File.ReadAllLines(CONFIG_PATH + "city_list.txt");
            attribute_list = System.IO.File.ReadAllLines(CONFIG_PATH + "attribute_label_list.txt");
            doc.Load(CONFIG_PATH + "GB50736_2012.xml");
            XmlNode root = doc.SelectSingleNode("GB50736_2012");
            XmlNodeList citys = root.ChildNodes;
            foreach (XmlNode city in citys)
            {
                XmlNodeList cityattr = city.ChildNodes;
                String current_city_name = "";
                foreach (XmlNode attr in cityattr)
                {
                    if (attr.Name == "#text")
                    {
                        current_city_name = attr.InnerText;
                    }
                    else
                    {
                        String attrtype = attr.Attributes[0].Value;
                        String value = attr.InnerText;
                        City_attribute_map[current_city_name + "_" + attrtype] = value;
                    }
                }
            }
        }

        public static string getConfigValue(String city, String attrtype)
        {
            return City_attribute_map[city + "_" + attrtype];
        }


        //以下为特定城市属性的接口

        //冬季供暖温度（供暖室外计算温度）
        public static String getHeatingOutsideTemperature(int cityindex)
        {
            return getConfigValue(city_list[cityindex], "冬季供暖温度");
        }

        public static int getCityIndexFromCityName(string name)
        {
            int index = 0;
            foreach(var item in city_list)
            {
                if(item.CompareTo(name) == 0 || item.StartsWith(name) || name.StartsWith(item))
                {
                    return index;
                }
                index++;
            }
            return 0;
        }

        //日平均温度≤+5℃的天数
        public static String getDailyTemperatureLessthanFive(int cityindex)
        {
            return getConfigValue(city_list[cityindex], "日平均温度5的天数");
        }

        //日平均温度≤+5℃的平均温度
        public static String getDailyTemperatureLessthanFiveAverageTemperature(int cityindex)
        {
            return getConfigValue(city_list[cityindex], "平均温度5期间内的平均温度");
        }
#endregion

#region Config Custom
        //Usage: 自定义配置属性，其配置文件为Config目录下的Custom_data.xml
        //      Custom _data包括列表和keyvalue两种自定义形式，具体可参考现有的Custom_data.xml
        private static XmlDocument custom_config_doc = new XmlDocument();

        //load xml
        public static void loadCustomConfig()
        {
            custom_config_doc.Load(CONFIG_PATH + "Custom_data.xml");
        }

        //对外获取属性接口,getCustomConfig("test")
        public static ArrayList getCustomConfig(String key)
        {
            XmlNode root = custom_config_doc.SelectSingleNode("Configs");
            XmlNodeList config_nodes = root.ChildNodes;
            ArrayList data = new ArrayList();
            foreach (XmlNode config_node in config_nodes)
            {
                XmlNode key_node = config_node.FirstChild;
                XmlNode data_node = config_node.LastChild;
                if (key_node.InnerText == key)
                {
                    if (data_node.Name == "listdata")
                    {
                        foreach (XmlNode subdata in data_node.ChildNodes)
                        {
                            data.Add(subdata.InnerText);
                        }
                    }
                    else if (data_node.Name == "single")
                    {
                        String singledata = data_node.InnerText;
                        data.Add(singledata);
                    }
                }
            }
            return data;
        }

        /*
         * 以下为特定自定义配置属性的获取接口
         * 任何属性均包含getConfig<属性名>List()与 getConfig<属性名>ByIndex()两个函数
         *      getConfig<属性名>List() 返回为完整列表，id从0开始为数组下标
         *      getConfig<属性名>ByIndex() 使用上函数的id下标作为索引，返回单个特定属性值
         *      所有属性值均为string类型
         */

        //城市列表
        public static String[] getCustomCityList()
        {
            ArrayList ar_result = getCustomConfig("城市列表");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result;
        }

        public static String getCustomCityByIndex(int index)
        {
            ArrayList ar_result = getCustomConfig("城市列表");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result[index]; ;
        }

        //供暖天数

        //供暖方式
        public static String[] getCustomHeatingtypeList()
        {
            ArrayList ar_result = getCustomConfig("供暖方式");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result;
        }

        public static String getCustomHeatingtypeByIndex(int index)
        {
            ArrayList ar_result = getCustomConfig("供暖方式");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result[index]; ;
        }

        //建筑类型
        public static String[] getCustomBuildingtypeList()
        {
            ArrayList ar_result = getCustomConfig("建筑类型");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result;
        }

        public static String getCustomBuildingtypeByIndex(int index)
        {
            ArrayList ar_result = getCustomConfig("建筑类型");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result[index]; ;
        }


        //节能措施
        public static String[] getCustomEnergytypeList()
        {
            ArrayList ar_result = getCustomConfig("节能措施");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result;
        }

        public static String getCustomEnergytypeByIndex(int index)
        {
            ArrayList ar_result = getCustomConfig("节能措施");
            String[] str_result = (String[])ar_result.ToArray(typeof(String));
            return str_result[index]; ;
        }
#endregion
    }
}
