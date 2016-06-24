using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Media.Animation;
namespace HeatSource.View
{
    /// <summary>
    /// ProgressBarFunc.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressBarFunc : Window
    {
        public ProgressBarFunc()
        {

            InitializeComponent();

        }
        static ProgressBarFunc()
        {
               Timeline.DesiredFrameRateProperty.OverrideMetadata(
                          typeof(Timeline),
                              new FrameworkPropertyMetadata { DefaultValue = 100 });
        }
    }
}
