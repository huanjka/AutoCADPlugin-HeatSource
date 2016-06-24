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
using HeatSource.Utils;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace HeatSource.View
{

    public partial class BuildingAttrEditor : Page
    {

        public BuildingAttrEditor()
        {
            InitializeComponent();
            SetBuilding(new Building(false));
        }
        public void SetBuilding(Building b)
        {
            var selectedObject = new BuildingAttributes();
            selectedObject.SetCurrentBuilding(b, this);
            this.DataContext = selectedObject;
            _propertyGrid.Update();
        }

        public class BuildingAttributes
        {
            public BuildingAttrEditor buildingAttrEditor;
            private Building currentBuilding = null;
            public void SetCurrentBuilding(Building b, BuildingAttrEditor editor)
            {
                this.currentBuilding = b;
                buildingAttrEditor = editor;

            }
           
            [Category(Constants.CATEGORY_UI)]
            [DisplayName( "楼房轮廓调整")]
            [Description("楼房轮廓宽度")]
            public double BuildingOutlineWidth
            {
                get
                {
                    return HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                }
                set
                {
                    if(HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth != value && value > 0)
                    {
                        HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth = value;
                        foreach(var b in HeatSourceLayoutApp.buildings)
                        {
                            b.Value.UpdateOutlineWidth();
                        }
                    }
                    HeatSourceLayoutApp.baseAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("建筑类型")]
            [Description("该楼房的类型")]
            [ItemsSource(typeof(BuildingTypeItemsSource))]
            public int btype
            {
                get
                {
                    return this.currentBuilding.BuildingType;
                }
                set
                {
                    this.currentBuilding.BuildingType = value;
                    this.currentBuilding.Save();
                    this.buildingAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("建筑高度(m)")]
            [Description("该楼房的高度")]
            public double bheight
            {
                get
                {
                    return this.currentBuilding.Height;
                }
                set
                {
                    this.currentBuilding.Height = value;
                    this.currentBuilding.Save();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("建筑面积(m2)")]
            [Description("该楼房的面积")]
            public double barea
            {
                get
                {
                    return this.currentBuilding.Area;
                }
                set
                {
                    this.currentBuilding.Area = value;
                    this.currentBuilding.Save();
                    this.buildingAttrEditor._propertyGrid.Update();

                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("建筑层数(层)")]
            [Description("该楼房的层数")]
            public int bheightlevel
            {
                get
                {
                    return this.currentBuilding.LayerCount;
                }
                set
                {
                    this.currentBuilding.LayerCount = value;
                    this.currentBuilding.Save();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("供暖方式")]
            [Description("该楼房的供暖方式")]
            [ItemsSource(typeof(HeatStyleItemsSource))]
            public int heatStyle
            {
                get
                {
                    return this.currentBuilding.HeatStyle;
                }
                set
                {
                    this.currentBuilding.HeatStyle = value;
                    this.currentBuilding.Save();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("节能措施")]
            [Description("该楼房的节能措施")]
            [ItemsSource(typeof(EnergySavingStyleItemsSource))]
            public int energeSavingStyle
            {
                get
                {
                    return this.currentBuilding.EnergySavingStyle;
                }
                set
                {
                    this.currentBuilding.EnergySavingStyle = value;
                    this.currentBuilding.Save();
                    this.buildingAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_INPUT)]
            [DisplayName("采暖热指标(W/m2)")]
            [Description("该楼房的采暖热指标")]
            public double heatindex
            {
                get
                {
                    return currentBuilding.HeatIndex;
                }
                set
                {
                    currentBuilding.HeatIndex = value;
                    this.currentBuilding.Save();
                    this.buildingAttrEditor._propertyGrid.Update();
                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("采暖热负荷(kW)")]
            [Description("该楼房的采暖热负荷")]
            public double heatload
            {
                get
                {
                    return currentBuilding.HeatLoad;
                }
                set
                {
                    this.currentBuilding.HeatLoad = value;
                    this.currentBuilding.Save();

                }
            }

            [Category(Constants.CATEGORY_FORMULA)]
            [ReadOnly(true)]
            [DisplayName("全年耗热量(GJ)")]
            [Description("该楼房的全年供热量")]
            public double yearheat
            {
                get
                {
                    return this.currentBuilding.YearHeat;
                }
                set
                {
                    this.currentBuilding.YearHeat = value;
                    this.currentBuilding.Save();

                }

            }

            public class BuildingTypeItemsSource : IItemsSource
            {
                public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
                {
                    var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                    for (int i = 0; i < Constants.BuildingType.Count; ++i)
                    {
                        sizes.Add(i, Constants.BuildingType[i]);
                    }
                    return sizes;
                }
            }
            public class HeatStyleItemsSource : IItemsSource
            {
                public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
                {
                    var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                    for (int i = 0; i < Constants.HeatStyle.Count; ++i)
                    {
                        sizes.Add(i, Constants.HeatStyle[i]);
                    }
                    return sizes;
                }
            }
            public class EnergySavingStyleItemsSource : IItemsSource
            {
                public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
                {
                    var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
                    for (int i = 0; i < Constants.SaveStyle.Count; ++i)
                    {
                        sizes.Add(i, Constants.SaveStyle[i]);
                    }
                    return sizes;
                }
            }
        }
    }
}
