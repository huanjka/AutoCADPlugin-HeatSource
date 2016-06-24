using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using HeatSource.Model;
using HeatSource.Formula;
using HeatSource.Utils;
using System.Collections;

namespace HeatSource.View
{
    /// <summary>
    /// Interaction logic for BaseAttrEditor.xaml
    /// </summary>
    public partial class BaseAttrEditor : Page
    {
        public static Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection viewCities = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
        BaseAttributes selectedObject;
        public BaseAttrEditor()
        {
            InitializeComponent();
            selectedObject = new BaseAttributes();
            selectedObject.SetCurrentBase(this);
            this.DataContext = selectedObject;
            _propertyGrid.Update();

            for (int j = 0; j < Constants.City[HeatSourceLayoutApp.globalProperty.ProvinceId].Count; ++j)
            {
                viewCities.Add(j, Constants.City[HeatSourceLayoutApp.globalProperty.ProvinceId][j]);
            }
            //第一次初始化，更新【北京】的供暖天数等值
            //获取数据库中的CityID，如果不存在，返回一个默认值1
            ArrayList cityListInDB = DataConfig.getCustomConfig("城市列表");
            String[] cityList = (String[])cityListInDB.ToArray(typeof(String));
            String cityName = Constants.City[0][0];
            int i;
            for (i = 0; i < cityList.Length; i++)
            {
                String testCityName0 = cityList[i];
                String testCityName1 = cityList[i] + "市";
                String testCityName2 = cityList[i] + "州";
                String testCityName3 = cityList[i] + "地区";
                String testCityName4 = cityList[i] + "盟";

                if ((testCityName0 == cityName) || (testCityName1 == cityName) || (testCityName2 == cityName) || (testCityName3 == cityName) || (testCityName4 == cityName))
                {
                    HeatSourceLayoutApp.globalProperty.CityId = i;
                    break;
                }
            }
            if (i == cityList.Length)
                HeatSourceLayoutApp.globalProperty.CityId = 0;
            HeatSourceLayoutApp.globalProperty.HeatingDays = (int)double.Parse(DataConfig.getDailyTemperatureLessthanFive(HeatSourceLayoutApp.globalProperty.CityId));
            HeatSourceLayoutApp.globalProperty.OutDoorTemp = double.Parse(DataConfig.getHeatingOutsideTemperature(HeatSourceLayoutApp.globalProperty.CityId));
            try
            {
                HeatSourceLayoutApp.globalProperty.OutAverageTemp = double.Parse(DataConfig.getDailyTemperatureLessthanFiveAverageTemperature(HeatSourceLayoutApp.globalProperty.CityId));
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public void updateBaseView()
        {
            _propertyGrid.Update();
        }

        public class BaseAttributes
        {

            public BaseAttrEditor baseAttrEditor;
            public void SetCurrentBase(BaseAttrEditor editor)
            {
                baseAttrEditor = editor;
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("省份")]
            [Description("该建筑区域所在的省份")]
            [ItemsSource(typeof(ProvinceItemsSource))]
            [PropertyOrder(1)]
            public int province
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.ProvinceId;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.ProvinceId = value;
                    viewCities.Clear();
                    PropertyItem item = (PropertyItem)baseAttrEditor._propertyGrid.Properties[1];
                    Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox c = (Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox)item.Editor;
                    for (int j = 0; j < Constants.City[value].Count; ++j)
                    {
                        viewCities.Add(j, Constants.City[value][j]);
                    }

                    //HeatSourceLayoutApp.globalProperty.CityId = 0; // 初始化城市id
                    c.ItemsSource = null; // 为了正常更新列表视图
                    c.ItemsSource = viewCities;
                    c.SelectedIndex = 0;

                    //初始化时更新CityID及HeatingDays等
                    HeatSourceLayoutApp.globalProperty.CityId = 0;
                    HeatSourceLayoutApp.globalProperty.CityName = Constants.City[value][0];
                    int index = DataConfig.getCityIndexFromCityName(Constants.City[value][0]);
                    HeatSourceLayoutApp.globalProperty.HeatingDays = (int)double.Parse(DataConfig.getDailyTemperatureLessthanFive(index));
                    HeatSourceLayoutApp.globalProperty.OutDoorTemp = double.Parse(DataConfig.getHeatingOutsideTemperature(index));
                    try
                    {
                        HeatSourceLayoutApp.globalProperty.OutAverageTemp = double.Parse(DataConfig.getDailyTemperatureLessthanFiveAverageTemperature(index));
                    }
                    catch(Exception e)
                    {
                        e.ToString();
                    }
                    this.baseAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("城市")]
            [Description("该建筑区域所在的城市")]
            [ItemsSource(typeof(CityItemsSource))]
            [PropertyOrder(2)]
            public int city
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.CityId;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.CityId = value;
                    HeatSourceLayoutApp.globalProperty.CityName = Constants.City[HeatSourceLayoutApp.globalProperty.ProvinceId][value];
                    int index = DataConfig.getCityIndexFromCityName(Constants.City[HeatSourceLayoutApp.globalProperty.ProvinceId][value]);
                    HeatSourceLayoutApp.globalProperty.HeatingDays = (int)double.Parse(DataConfig.getDailyTemperatureLessthanFive(index));
                    HeatSourceLayoutApp.globalProperty.OutDoorTemp = double.Parse(DataConfig.getHeatingOutsideTemperature(index));
                    try
                    {
                        HeatSourceLayoutApp.globalProperty.OutAverageTemp = double.Parse(DataConfig.getDailyTemperatureLessthanFiveAverageTemperature(index));
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }

                    baseAttrEditor._propertyGrid.Update();

                }
            }
            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("地区类型")]
            [Description("该建筑区域所在的地区类型")]
            [ItemsSource(typeof(LocationItemsSource))]
            [PropertyOrder(3)]
            public int location
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.LocationType;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.LocationType = value;

                }
            }

        


            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("室内温度(℃)")]
            [Description("楼房的室内温度（摄氏度）")]
            public double in_temp
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.IndoorTemperature;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.IndoorTemperature = value;
                }
            }

            [Category(Constants.CATEGORY_UI)]
            [DisplayName("比例尺(1:xx)")]
            [Description("用于计算管道长度")]
            public double scale
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.GraphScale;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.GraphScale = value;
                }
            }

            [Category(Constants.CATEGORY_UI)]
            [DisplayName("管道比例调整")]
            [Description("管道粗细")]
            public double pipelineScale
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.PipeLineScale;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.PipeLineScale = value;
                }
            }
            [Category(Constants.CATEGORY_UI)]
            [DisplayName("楼房轮廓调整")]
            [Description("楼房轮廓宽度")]
            public double BuildingOutlineWidth
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                }
                set
                {
                    if (HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth != value && value > 0)
                    {
                        HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth = value;
                        foreach (var b in HeatSourceLayoutApp.buildings)
                        {
                            b.Value.UpdateOutlineWidth();
                        }
                    }
                    HeatSourceLayoutApp.buildingAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_UI)]
            [DisplayName("热源大小调整")]
            [Description("热源显示大小")]
            public double HeatProducerSize
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.BlockScale;
                }
                set
                {
                    if (HeatSourceLayoutApp.globalProperty.BlockScale != value && value > 0)
                    {
                        HeatSourceLayoutApp.globalProperty.BlockScale = value;
                        foreach (var s in HeatSourceLayoutApp.solutions)
                        {
                            foreach (var item in s.HeatProducers)
                            {
                                item.Value.UpdateScale();
                            }
                            foreach (var item in s.SubStations)
                            {
                                item.Value.UpdateScale();
                            }
                        }
                    }
                }
            }


            [Category(Constants.CATEGORY_DATABASE)]
            [ReadOnly(true)]
            [DisplayName("燃气热值(MJ/m3)")]
            [Description("燃气的热值常量（兆焦/立方米）")]
            public double gas_heat
            {
                get
                {
                    return 38.93;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.HeatValueofGas = value;
                }
            }

            [Category(Constants.CATEGORY_DATABASE)]
            [ReadOnly(true)]
            [DisplayName("供暖天数(d)")]
            [Description("该建筑区域的供暖天数（天）")]
            public int heat_days
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.HeatingDays;
                }
                set
                {

                    HeatSourceLayoutApp.globalProperty.HeatingDays = value;
                }


            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("电费单价(元/kW·h)")]
            [Description("电费单价（元/千瓦时）")]
            public double elec_fee
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.ElecPrice;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.ElecPrice = value;
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("燃气单价(元/m3)")]
            [Description("燃气单价（元/立方米）")]
            public double gas_fee
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.GasPrice;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.GasPrice = value;
                }
            }

            [Category(Constants.CATEGORY_DATABASE)]
            [ReadOnly(true)]
            [DisplayName("采暖期室外温度(℃)")]
            [Description("该建筑区域室外采暖的温度（摄氏度）")]
            public double out_temp
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.OutDoorTemp;
                }
                set
                {
                    HeatSourceLayoutApp.globalProperty.OutDoorTemp = value;
                }
            }

            [Category(Constants.CATEGORY_DATABASE)]
            [ReadOnly(true)]
            [DisplayName("采暖期室外平均温度(℃)")]
            [Description("该区域室外的平均温度（摄氏度）")]
            public double out_average_temp
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.OutAverageTemp;
                }
                set
                {

                    HeatSourceLayoutApp.globalProperty.OutAverageTemp = value;
                }
            }
        }


        public class ProvinceItemsSource : IItemsSource
        {
            public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
            {
                var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                for (int i = 0; i < Constants.Province.Count; ++i)
                {
                    sizes.Add(i, Constants.Province[i]);
                }
                return sizes;
            }
        }
        public class CityItemsSource : IItemsSource
        {
            public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
            {
                //var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                //for (int i = 0; i < Constants.City[10].Count; ++i)
                //{
                //    sizes.Add(i, Constants.City[10][i]);
                //}
                return BaseAttrEditor.viewCities;
            }
        }
        public class LocationItemsSource : IItemsSource
        {
            public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
            {
                var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                for (int i = 0; i < Constants.LocationType.Count; ++i)
                {
                    sizes.Add(i, Constants.LocationType[i]);
                }
                return sizes;
            }
        }
    }
}
