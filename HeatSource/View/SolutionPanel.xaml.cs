using System;
using System.Windows.Controls;
using ChromeTabs;
using HeatSource.Model;
using Autodesk.AutoCAD.DatabaseServices;
using HeatSource.Utils;
using Autodesk.AutoCAD.Internal;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;

namespace HeatSource.View
{
    /// <summary>
    /// SolutionPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SolutionPanel : UserControl
    {
        public SolutionPanel()
        {
            InitializeComponent();
            this.chrometabs.NewTabWillCreatedHandler = this.NewTabWillCreated;
            this.chrometabs.NewTabHaveCreatedHandler = this.NewTabHaveCreated;
            this.chrometabs.TabHaveSelectedHandler = this.TabHaveSelected;
            this.chrometabs.TabHaveClosedHandler = this.TabHaveClosed;
        }

        public void NewTabWillCreated(ChromeTabItem item)
        {
            item.Header = HeatSource.Model.Solution.NextDefaultName();
        }

        public void AddCloseHandler(ChromeTabItem item)
        {
            item.TabItemHaveClosedHandler = this.TabHaveClosed;
        }
        public void NewTabHaveCreated(ChromeTabItem item)
        {
            //判断是否存在该解决方案
            //暂时无法判断该item是新建还是移动
            foreach(var sol in HeatSource.HeatSourceLayoutApp.solutions)
            {
                if(sol.TabItem == item)
                {
                    return;
                }
            }
            item.CanDelete = true;
            item.TabItemHaveClosedHandler = this.TabHaveClosed;
            Solution s = new Solution(true);
            s.SolutionName = (String)item.Header;
            ObjectId objectid = Utility.ContainLayer(s.SolutionName);
            if (objectid == ObjectId.Null)
                s.BaseObjectId = HeatSource.Utils.Utility.CreateLayer(s.SolutionName);
            else
                s.BaseObjectId = objectid;
            s.TabItem = item;
            s.SId = Solution.NextDefaultSId();
            HeatSource.HeatSourceLayoutApp.solutions.Add(s);

        }
        public void TabWillClosed()
        {

        }
        public void TabHaveClosed(ChromeTabItem item)
        {
            Solution deleteSol = null;
            Solution lastSol = null;
            Solution nextSol = null;
            ChromeTabItem selItm = null;
            int index = 0;
            foreach (var sol in HeatSource.HeatSourceLayoutApp.solutions)
            {
                if(sol.TabItem == item)
                {
                    deleteSol = sol;
                    if(index < HeatSourceLayoutApp.solutions.Count - 1)
                    {
                        nextSol = HeatSourceLayoutApp.solutions[index + 1];
                    }
                    break;
                }
                lastSol = sol;
                index++;
            }
            if(lastSol != null)
            {
                selItm = lastSol.TabItem;
            }
            else if(nextSol != null)
            {
                selItm = nextSol.TabItem;
            }
            else
            {
                selItm = OutlineLayer;
            }

            if(deleteSol != null)
            {
                //select layer
                TabHaveSelected(selItm);
                //remove entity
                deleteSol.RemoveSelf();
                //delete layer
                Utility.RemoveLayer(deleteSol.BaseObjectId);
               
            }
        }
        public void TabWillSelected()
        {

        }

        public static String currentItemHeader = "";
        public void TabHaveSelected(ChromeTabItem item)
        {

            if(currentItemHeader.CompareTo((string)(item.Header)) == 0)
            {
                return;
            }
            else
            {
                currentItemHeader = (string)item.Header;
            }
            //取消命令
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
            Utility.makeImageBottom();
            
            //先设置下clayer
            if (item.Name.CompareTo("OutlineLayer") == 0)
            {
                Utility.ResetCLayer(HeatSource.HeatSourceLayoutApp.globalProperty.BaseObjectId);
                HeatSource.HeatSourceLayoutApp.globalProperty.ActiveTab = -1;
            }
            else if (item.Name.CompareTo("OriginLayer") == 0)
            {
                HeatSource.Utils.Utility.ActiveOriginLayer();
                HeatSource.HeatSourceLayoutApp.globalProperty.ActiveTab = -1;
            }
            else
            {
                foreach (var sol in HeatSource.HeatSourceLayoutApp.solutions)
                {
                    if(sol.TabItem.Header.ToString().CompareTo(item.Header.ToString()) == 0 && sol.TabItem != item)
                    {
                        sol.TabItem = item;
                        sol.TabItem.CanDelete = true;
                    }
                    if (sol.TabItem == item)
                    {
                        Utility.ResetCLayer(sol.BaseObjectId);
                        HeatSource.HeatSourceLayoutApp.globalProperty.ActiveTab = sol.SId;
                    }
                }
            }
            //关闭其他图层
            if (item.Name.CompareTo("OutlineLayer") == 0)
            {
                HeatSource.HeatSourceLayoutApp.currentSolution = null;
                ObjectId layerId = HeatSource.HeatSourceLayoutApp.globalProperty.BaseObjectId;
                HeatSource.Utils.Utility.OpenOrCloseLayer(layerId, false);
            }
            else if (item.Name.CompareTo("OriginLayer") == 0)
            {
                HeatSource.HeatSourceLayoutApp.currentSolution = null;
                ObjectId layerId = HeatSource.HeatSourceLayoutApp.globalProperty.BaseObjectId;
                HeatSource.Utils.Utility.OpenOrCloseLayer(layerId, true);
            }
            else
            {
                ObjectId layerId = HeatSource.HeatSourceLayoutApp.globalProperty.BaseObjectId;
                HeatSource.Utils.Utility.OpenOrCloseLayer(layerId, false);
            }
            foreach(var sol in HeatSource.HeatSourceLayoutApp.solutions)
            {
                if(sol.TabItem == item)
                {
                    HeatSource.HeatSourceLayoutApp.currentSolution = sol;
                    //改变圈选的情况
                    HeatSource.View.ToolPanel.changeSolution(sol.SId - 1);
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("选择了" + sol.SId.ToString()+"\n");
                    
                }
                else
                {
                    HeatSource.Utils.Utility.OpenOrCloseLayer(sol.BaseObjectId, true);
                }
            }
            //
            List<ObjectId> ids = new List<ObjectId>();
            ids.Add(HeatSourceLayoutApp.globalProperty.BaseObjectId);
            foreach (var sol in HeatSourceLayoutApp.solutions)
            {
                ids.Add(sol.BaseObjectId);
            }           
            CoreLayerUtilities.RegenLayers(ids.ToArray(), 0);
            //Autodesk.AutoCAD.DatabaseServices.ObjectId] layers, int regenPending);
            //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.Regen();
            //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
        }

        public void SelectOutLineLayer()
        {
            this.chrometabs.ChangeSelectedItem(this.OutlineLayer);
        }

        public void SelectOriginLayer()
        {
            this.chrometabs.ChangeSelectedItem(this.OriginLayer);
        }




    }
}
