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

namespace HeatSource.View
{
    /// <summary>
    /// CropMessage.xaml 的交互逻辑
    /// </summary>
    public partial class CropMessage : Window
    {
        public CropMessage()
        {
            InitializeComponent();
        }
        public bool SelectedValue { set; get; }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SelectedValue = true;
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SelectedValue = false;
            this.Close();
        }
    }
}
