using HeatSource.Model;
using HeatSource.View.SolutionAtrrBean;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeatSource.View
{
    /// <summary>
    /// Interaction logic for SolutionAttrPanel.xaml
    /// </summary>
    public partial class SolutionAttrComparePanel : Page
    {
        private SolutionAttrCompareTable solutionCompareTable;
        public SolutionAttrComparePanel()
        {
            InitializeComponent();
            solutionCompareTable = new SolutionAttrCompareTable();
            _propertyGrid.SelectedObject = solutionCompareTable;
        }
        public void SetSolution()
        {
            solutionCompareTable = new SolutionAttrCompareTable();
            _propertyGrid.SelectedObject = solutionCompareTable;
            _propertyGrid.Update();
        }
    }
}
