using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace HeatSource.View
{
    /// <summary>
    /// Interaction logic for SubstationAttrEditor.xaml
    /// </summary>
    public partial class SubstationAttrEditor : Page
    {
        public SubstationAttrEditor()
        {
            InitializeComponent();
            SetSubstation(new SubStation());
        }

        public void SetSubstation(SubStation substation)
        {
           
            var selectedObject = new SubstationAttributes();
            selectedObject.SetSubstation(substation,this);
            this.DataContext = selectedObject;
            _propertyGrid.Update();
        }

        public class SubstationAttributes
        {
            public SubstationAttrEditor substationAttrEditor;
            public SubStation currentSubStation = null;

            public void SetSubstation(SubStation substation, SubstationAttrEditor editor)
            {
                this.currentSubStation = substation;
                substationAttrEditor = editor;
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
                    HeatSourceLayoutApp.heatProducerAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("供暖方式")]
            [Description("供暖方式")]
            [ItemsSource(typeof(HeatStyleItemsSource))]
            [PropertyOrder(1)]
            public int heatStyle
            {
                get
                {
                    return currentSubStation.HeatStyle;
                }
                set
                {
                    if (value != currentSubStation.HeatStyle)
                    {
                        if (value == 1)
                        {
                            currentSubStation.SupplyWaterTemp = 50;
                            currentSubStation.ReturnWaterTemp = 40;
                        }
                        else if (value == 2)
                        {
                            currentSubStation.SupplyWaterTemp = 55;
                            currentSubStation.ReturnWaterTemp = 40;
                        }
                        else if (value == 3)
                        {
                            currentSubStation.SupplyWaterTemp = 75;
                            currentSubStation.ReturnWaterTemp = 55;
                        }
                        else if (value == 4)
                        {
                            currentSubStation.SupplyWaterTemp = 0;
                            currentSubStation.ReturnWaterTemp = 0;
                        }
                    }
                    this.currentSubStation.HeatStyle = value;
                    this.currentSubStation.Save();
                    this.substationAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("水泵供水温度")]
            [Description("水泵供水温度")]
            public double supplyWaterTemp
            {
                get
                {
                    return currentSubStation.SupplyWaterTemp;
                }
                set
                {
                    currentSubStation.SupplyWaterTemp = value;
                    currentSubStation.Save();
                    this.substationAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("水泵回水温度")]
            [Description("水泵回水温度")]
            public double returnWaterTemp
            {
                get
                {
                    return currentSubStation.ReturnWaterTemp;
                }
                set
                {
                    currentSubStation.ReturnWaterTemp = value;
                    currentSubStation.Save();
                    this.substationAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("换热器台数(台)")]
            [Description("换热器台数（台）")]
            [PropertyOrder(2)]
            public int heatSwitcherCount
            {
                get
                {
                    return currentSubStation.HeatSwitcherCount;
                }
                set
                {
                    if (value < 2)
                    {
                        value = 2;
                    }
                    currentSubStation.HeatSwitcherCount = value;
                    heatSwitcherVolumn = (int)(currentSubStation.TotalHeatingDesignLoad / value);
                    this.substationAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("换热器容量(kW)")]
            [Description("换热器容量（千瓦）")]
            [PropertyOrder(1)]
            public double heatSwitcherVolumn
            {
                get
                {
                    return currentSubStation.HeatSwitcherVolumn;
                }
                set
                {
                    currentSubStation.HeatSwitcherVolumn = value;
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵台数(台)")]
            [Description("水泵台数（台）")]
            [PropertyOrder(2)]
            public int waterPumpCount
            {
                get
                {
                    return this.currentSubStation.WaterPumpCount;
                }
                
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵流量(m3)")]
            [Description("水泵流量（立方米）")]
            [PropertyOrder(3)]
            public double waterPumpVolume
            {
                get
                {
                    return Math.Round(this.currentSubStation.WaterPumpVolume, 2);

                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵扬程(m)")]
            [Description("水泵扬程（米）")]
            [PropertyOrder(4)]
            public double waterPumpLift
            {
                get
                {
                    return Math.Round(this.currentSubStation.WaterPumpLift, 2);
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("水泵功率(kW)")]
            [Description("水泵功率（千瓦）")]
            [PropertyOrder(5)]
            public double waterPumpPower
            {
                get
                {
                    return Math.Round(currentSubStation.WaterPumpPower, 2);
                }
                set
                {
                    this.currentSubStation.WaterPumpPower = value;
                    this.currentSubStation.Save();
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("热力站总负荷(kW)")]
            [Description("热力站总负荷")]
            [PropertyOrder(6)]
            public double totalLoad
            {
                get
                {
                    return Math.Round(this.currentSubStation.TotalHeatingDesignLoad,2);
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("主管道长度(m)")]
            [Description("主管道长度（米）")]
            [PropertyOrder(7)]
            public double masterPipeLength
            {
                get
                {
                    return Math.Round(this.currentSubStation.MasterPipeLength, 2);
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("管道总长度(m)")]
            [Description("管道总长度（米）")]
            [PropertyOrder(8)]
            public double totalPipeLength
            {
                get
                {
                    return 2*Math.Round(this.currentSubStation.TotoalPipeLength, 2);
                }
            }

            [Category(Constants.CATEGORY_DATABASE)]
            [ReadOnly(true)]
            [DisplayName("水泵供回水温差(℃)")]
            [Description("水泵供回水温差（摄氏度）")]
            public double waterPumpTemp
            {
                get
                {
                    return currentSubStation.WaterPumpTemp;
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
                sizes.Add(4, "其他");
                return sizes;
            }
        }
    }
}
