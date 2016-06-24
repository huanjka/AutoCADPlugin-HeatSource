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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using HeatSource.Model;
using HeatSource.Formula;
using HeatSource.View;
using Autodesk.AutoCAD.DatabaseServices;
using Xceed.Wpf.Toolkit.PropertyGrid;
using HeatSource.Utils;

namespace HeatSource.View
{
    /// <summary>
    /// Interaction logic for HeatProducerAttrEditor.xaml
    /// </summary>
    public partial class HeatProducerAttrEditor : Page
    {
        public static Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection viewBoilers = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
        public static Dictionary<string, List<Dictionary<string, string>>> boilersSolution = null;

        HeatProducerAttributes selectedObject;
        public HeatProducerAttrEditor()
        {
            InitializeComponent();
            selectedObject = new HeatProducerAttributes();
            SetHeatProducer(new HeatProducer());
        }

        // public API for update UI
        public void SetHeatProducer(HeatProducer p)
        {
            selectedObject.SetHeatProducer(p, this);
            this.DataContext = selectedObject;
            _propertyGrid.Update();
        }

        public class HeatProducerAttributes
        {
            public HeatProducerAttrEditor heatProducerAttrEditor;
            private HeatProducer currentProducer = null;
            public void SetHeatProducer(HeatProducer p, HeatProducerAttrEditor editor)
            {
                this.currentProducer = p;
                heatProducerAttrEditor = editor;

            }
            [Category(Constants.CATEGORY_UI)]
            [DisplayName("热源显示大小")]
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
                    HeatSourceLayoutApp.substationAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("供暖方式")]
            [Description("供暖方式")]
            [ItemsSource(typeof(HeatStyleItemsSource))]
            public int heatStyle
            {
                get
                {
                    return currentProducer.HeatStyle;
                }
                set
                {
                    if (value != currentProducer.HeatStyle)
                    {
                        if (value == 1)
                        {
                            currentProducer.SupplyWaterTemp = 50;
                            currentProducer.ReturnWaterTemp = 40;
                        }
                        else if (value == 2)
                        {
                            currentProducer.SupplyWaterTemp = 55;
                            currentProducer.ReturnWaterTemp = 40;
                        }
                        else if (value == 3)
                        {
                            currentProducer.SupplyWaterTemp = 75;
                            currentProducer.ReturnWaterTemp = 55;
                        }
                        else if(value == 4)
                        {
                            currentProducer.SupplyWaterTemp = 120;
                            currentProducer.ReturnWaterTemp = 90;
                        }
                    }
                    currentProducer.HeatStyle = value;
                    currentProducer.Save();
                    this.heatProducerAttrEditor._propertyGrid.Update();
                }
            }

          

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("锅炉配选(台)")]
            [Description("锅炉台数（台）")]
            [ItemsSource(typeof(BoilersSolutionItemsSource))]
            [PropertyOrder(1)]
            public string boilerCount
            {
                get
                {
                    if (currentProducer.BoilerSolutionFile == null)
                    {
                        return "";
                    }
                    else
                    {
                        return currentProducer.BoilerSolutionFile;
                    }
                }
                set
                {
                    if (value != null)
                    {
                        currentProducer.BoilerSolutionFile = value;
                        int wpc = 0;
                        foreach (var row in boilersSolution[value])
                        {
                            wpc += int.Parse(row["数量"]);
                        }
                        waterPumpCount = wpc;
                    }
                    //this.heatProducerAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵台数(台)")]
            [Description("水泵台数（台）")]
            public int waterPumpCount
            {
                get
                {
                    return currentProducer.WaterPumpCount;
                }

                set
                {
                    currentProducer.WaterPumpCount = value;
                    currentProducer.Save();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("燃气锅炉效率(百分比)")]
            [Description("燃气锅炉效率")]
            public double boilerEfficiency
            {
                get
                {
                    return this.currentProducer.BoilerEfficiency;
                }
                set
                {
                    this.currentProducer.BoilerEfficiency = value;
                    currentProducer.Save();
                    this.heatProducerAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("水泵供水温度")]
            [Description("水泵供水温度")]
            public double supplyWaterTemp
            {
                get
                {
                    return currentProducer.SupplyWaterTemp;
                }
                set
                {
                    currentProducer.SupplyWaterTemp = value;
                    currentProducer.Save();
                    this.heatProducerAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("水泵回水温度")]
            [Description("水泵回水温度")]
            public double returnWaterTemp
            {
                get
                {
                    return currentProducer.ReturnWaterTemp;
                }
                set
                {
                    currentProducer.ReturnWaterTemp = value;
                    currentProducer.Save();
                    this.heatProducerAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [ReadOnly(true)]
            [DisplayName("水泵供回水温差(℃)")]
            [Description("水泵供回水温差（摄氏度）")]
            public double waterPumpTemp
            {
                get
                {
                    return currentProducer.WaterPumpTemp;
                }
            }

            
            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("热源总负荷(kW)")]
            [Description("热源总负荷")]
            [PropertyOrder(1)]
            public double totalLoad
            {
                get
                {
                    //更新热源的总耗热量 = 关联的热力站和楼房的全年耗热量之和
                    //目前只计算关联的楼房的耗热量之和
                    //根据 totalLoad/700 得到所需的锅炉数量
                    double total = currentProducer.TotalHeatingDesignLoad;
                    int itotal = (int)total;
                    int least = (itotal % 700 != 0) ? (itotal / 700 + 1) : (itotal / 700);
                    if (heatProducerAttrEditor._propertyGrid.Properties.Count != 0)
                    {
                        viewBoilers.Clear();

                        string totalheat = (least * 700).ToString();
                        string connect = this.currentProducer.OwnSlaveDistrict == null ? "直接" : "间接";
                        boilersSolution = getBoilersSolution(totalheat, connect); // :)
                        if (boilersSolution.Count != 0)
                        {
                            viewBoilers.Add("", "请选择锅炉方案:");
                            foreach (var bs in boilersSolution)
                            {
                                string output = "";
                                foreach (var row in bs.Value)
                                {
                                    if (connect == "直接")
                                        output += string.Format("{3:N1}×700KW，锅炉名称：{0}，台数：{1}，价格：{2}", row["名称"], row["数量"], row["间接价格"], total / 700);
                                    else
                                        output += string.Format("{3:N1}×700KW，锅炉名称：{0}，台数：{1}，单价：{2}", row["名称"], row["数量"], row["间接价格"], total / 700);
                                }
                                viewBoilers.Add(bs.Key, output);
                            }
                        }
                        else
                        {
                            viewBoilers.Add("", string.Format("{0:N1}×700KW，无可选择方案", total / 700));
                        }
                    }
                    if (heatProducerAttrEditor._propertyGrid.Properties.Count < 3)
                        return total;
                    PropertyItem item = (PropertyItem)heatProducerAttrEditor._propertyGrid.Properties[2];
                    Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox c = (Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox)item.Editor;
                    //if (c.ItemsSource != viewBoilers)
                    {
                        c.ItemsSource = null; // 为了正常更新列表视图
                        c.ItemsSource = viewBoilers;
                        //c.SelectedIndex = 0;
                        string key;
                        if (currentProducer.BoilerSolutionFile == null)
                        {
                            key = "";
                        }
                        else
                        {
                            key = currentProducer.BoilerSolutionFile;
                        }
                        for (int i = 0; i < viewBoilers.Count; i++)
                        {
                            if (viewBoilers[i].Value.ToString() == key)
                            {
                                c.SelectedIndex = i;
                                break;
                            }
                        }

                        return total;
                    }

                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("建筑总面积(m^2)")]
            [Description("建筑总面积（平方米）")]
            [PropertyOrder(2)]
            public double totalArea
            {
                get
                {
                    return this.currentProducer.TotalArea;
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵功率(kW)")]
            [Description("水泵功率（千瓦）")]
            [PropertyOrder(3)]
            public double waterPumpPower
            {
                get
                {
                    double result = this.currentProducer.WaterPumpPower;
                    return Math.Round(result, 2);
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("主管道长度(m)")]
            [Description("主管道长度（米）")]
            [PropertyOrder(4)]
            public double masterPipeLength
            {
                get
                {
                    return Math.Round(this.currentProducer.MasterPipeLength, 2);

                }
            }
            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("管道总长度(m)")]
            [Description("管道总长度（米）")]
            [PropertyOrder(5)]
            public double totoalPipeLength
            {
                get
                {
                    return 2*Math.Round(this.currentProducer.TotoalPipeLength, 2);
                }
            }
            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵全年运行能耗(kWh)")]
            [Description("水泵全年运行能耗")]
            [PropertyOrder(5)]
            public double totalWaterPumpEnergyConsumption
            {
                get
                {
                    return Math.Round(this.currentProducer.totalWaterPumpEnergyConsumption, 2);
                }
            }
            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵流量(m3)")]
            [Description("水泵流量（立方米）")]
            [PropertyOrder(6)]
            public double waterPumpVolume
            {
                get
                {
                    return Math.Round(this.currentProducer.WaterPumpVolume, 2);

                }
            }
            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵扬程(m)")]
            [Description("水泵扬程（米）")]
            [PropertyOrder(7)]
            public double waterPumpLift
            {
                get
                {
                    return Math.Round(this.currentProducer.WaterPumpLift, 2);
                }
            }
        }

        public class HeatStyleItemsSource : IItemsSource
        {
            public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
            {
                var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                sizes.Add(1, "地板供暖");
                sizes.Add(2, "空调供暖");
                sizes.Add(3, "散热器采暖");
                sizes.Add(4, "一次热源");
                return sizes;
            }
        }

        public class BoilersSolutionItemsSource : IItemsSource
        {
            public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
            {
                return HeatProducerAttrEditor.viewBoilers;
            }
        }

        #region 读取Excel表格得到锅炉选配方案
        public static Dictionary<string, List<Dictionary<string, string>>> getBoilersSolution(string totalheat, string connect)
        {
            Dictionary<string, List<Dictionary<string, string>>> results = new Dictionary<string, List<Dictionary<string, string>>>();
            foreach (string filename in ExcelUtil.getExcelNamelist()) // 从Excel目录下所有的表中搜索
            {
                if (filename.Contains(totalheat) && filename.Contains(connect)) // 若文件名含有目标吨数
                {
                    foreach (var row in ExcelUtil.readSheet(filename)) // 打开该文件，读取表内容，遍历每一行
                    {
                        foreach (var key in row.Values) // 若这一行是关于
                        {
                            if (key.Contains("锅炉")) // 锅炉的
                            {
                                if (!results.ContainsKey(filename))
                                {
                                    results.Add(filename, new List<Dictionary<string, string>>());
                                }
                                results[filename].Add(row); // 加入结果，一个文件可能对应多行锅炉
                            }
                        }
                    }
                }
            }
            return results;
        }
        #endregion
    }
}