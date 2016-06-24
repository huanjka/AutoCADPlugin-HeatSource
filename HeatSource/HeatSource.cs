using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using HeatSource.Model;
using HeatSource.View;
using HeatSource.Utils;
using System.Drawing;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Autodesk.AutoCAD.Colors;
using ChromeTabs;
using System.IO;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using MenuItem = Autodesk.AutoCAD.Windows.MenuItem;
using Xceed.Wpf.Toolkit.PropertyGrid;
using HeatSource.Utils.UseDrawJig;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

[assembly: ExtensionApplication(typeof(HeatSource.HeatSourceLayoutApp))]

namespace HeatSource
{
    /// <summary>
    /// 程序的入口
    /// 该类的主要功能包括：
    /// 1. 全局数据，包括楼房、解决方法
    /// 2. 全局事件响应的监听， 包括doubleclick, erase
    /// 3. command 的测试程序，后面要移除
    /// </summary>
    public class HeatSourceLayoutApp : IExtensionApplication
    {

        #region Global Data Structure

        /// <summary>
        /// 当前dll的路径，使用System.IO.GetCurrentDirectory()的方法调用当前路径是错误的
        /// 在AutoCAD中的该方法返回的结果会随着操作而变，也就是说，如果当你打开dll的时候，这个时候的路径
        /// 恰好是我们的dll所在的目录，此后在打开新的文件的时候，currentDirectory就变成另外的文件的路径
        /// 因此，在该程序中不应该出现System.IO.GetCurrentDirectory()
        /// </summary>
        public static String CurrentDirectory { set; get; }

        /// <summary>
        /// 楼房，楼房是指建立在out_line_layer 上的所有的BaseModel
        /// </summary>
        public static Dictionary<ObjectId, Building> buildings = new Dictionary<ObjectId, Building>();

        /// <summary>
        /// 解决方案，一个解决方案对应于一个layer,该layer的name属性以“方案”开头的
        /// </summary>
        public static List<Solution> solutions = new List<Solution>();

        /// <summary>
        /// 全局设置
        /// </summary>
        public static GlobalProperty globalProperty = new GlobalProperty(true);

        /// <summary>
        /// 当前所在的解决方案
        /// </summary>
        public static Solution _currentSolution;

        /// <summary>
        /// 当前所在的解决方案
        /// </summary>
        public static ObjectId currentSelectedHeatSource = ObjectId.Null;

        /// <summary>
        /// 操作命令的队列
        /// </summary>
        public static CommandManager CommandManager = new CommandManager();

        /// <summary>
        /// progressbar
        /// </summary>
        private static ProgressBarFunc progressbar = null;

        #endregion


        public static Solution currentSolution
        {
            set
            {
                _currentSolution = value;
                //update view
                HeatSourceLayoutApp.solutionAttrPanel.SetSolution(_currentSolution);
            }
            get { return _currentSolution; }
        }
        private static void initProgressBar()
        {
            progressbar = new ProgressBarFunc();
            progressbar.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Left + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2 - progressbar.Width / 2;
            progressbar.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Top + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 2 - progressbar.Height / 2;
            Application.ShowModalWindow(null, progressbar, false);
            progressbar.Visibility = System.Windows.Visibility.Collapsed;
        }

        [CommandMethod("progressbar", CommandFlags.UsePickSet)]
        public static void OpenProgressbar()
        {
            if (progressbar == null)
                initProgressBar();
            if (progressbar != null)
                progressbar.Visibility = System.Windows.Visibility.Visible;
        }

        [CommandMethod("closeprogressbar", CommandFlags.UsePickSet)]
        public static void CloseProgressbar()
        {
            if (progressbar != null)
                progressbar.Visibility = System.Windows.Visibility.Collapsed;
        }


        public void Initialize()
        {
            try
            {
                CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
                //bool verify = Verify();
                //if (!verify)
                //{
                //    System.Windows.MessageBox.Show("您的软件使用权已经过期.\n 请联系软件提供商.", "Error");
                //    return;
                //}

                Utility.ClearApplicationConfig();
                DataConfig.init_config_path();
                DataConfig.loadCustomConfig();
                loadConfigData();
                //初始化常量
                Constants.InitilizeConstant();
                //显示各控件
                showSolution();
                //关闭选项卡
                //closeRibbon();
                showTool();
                showAttrsEditor();
                //最后进行文件恢复
                restore();
                //测试截图
                Application.SetSystemVariable("NAVVCUBEDISPLAY", 0);
                Application.SetSystemVariable("NAVBARDISPLAY", 0);
                Object obj = Application.GetSystemVariable("DBLCLKEDIT");
                Application.SetSystemVariable("DBLCLKEDIT", 0);
            }
            catch(System.Exception e)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
            }
        }

        public bool Verify()
        {
            var publicKey =
                "<RSAKeyValue><Modulus>twbSt6je14wVIDcL3jHnL73s1sX6+aOrq4ox+dW+ZRKgipkA5qTDrcIByBwG9mu/GOBpxhc/D2t0ppHOPe+Iax4Xq3gRtOFmsK8g9cgtOVkbSggZUsL+gWwVrnw0u2g0tA3M6/rSeJfWwfb3SNQMcLUKgB5lyxvV1gbjR3caFDE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            try
            {
                // Create a UnicodeEncoder to convert between byte array and string.
                ASCIIEncoding ByteConverter = new ASCIIEncoding();
                string[] lines = System.IO.File.ReadAllLines(HeatSourceLayoutApp.CurrentDirectory + "\\license.txt");
                if (lines.Length < 2)
                {
                    return false;
                }
                // Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] originalData = Convert.FromBase64String(lines[0]);
                byte[] signedData = Convert.FromBase64String(lines[1]);

                // Create a new instance of the RSACryptoServiceProvider class 
                // and automatically create a new key-pair.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                // Verify the data and display the result to the 
                // console.
                RSAalg.FromXmlString(publicKey);
                RSAParameters Key1 = RSAalg.ExportParameters(false);
                if (VerifySignedHash(originalData, signedData, Key1))
                {

                    Console.WriteLine("The data was verified.");
                    DateTime t1 = DateTime.Now;
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                    dtFormat.ShortDatePattern = "yyyy-MM-dd";
                    DateTime t2 = Convert.ToDateTime(System.Text.Encoding.Default.GetString(originalData), dtFormat);
                    if (t1 < t2)
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    Console.WriteLine("The data does not match the signature.");
                    return false;
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine("The data was not signed or verified");
                return false;
            }


        }

        public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.  
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Hash and sign the data. Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.SignData(DataToSign, new SHA1CryptoServiceProvider());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Verify the data using the signature.  Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.VerifyData(DataToVerify, new SHA1CryptoServiceProvider(), SignedData);

            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return false;
            }
        }


        public void BindEventListener()
        {
            Application.BeginDoubleClick -= this.DoubleClick;
            Application.BeginDoubleClick += this.DoubleClick;

            Application.DocumentManager.MdiActiveDocument.CommandEnded -= this.OnCommandEnd;
            Application.DocumentManager.MdiActiveDocument.CommandEnded += this.OnCommandEnd;
            Application.DocumentManager.MdiActiveDocument.CommandCancelled -= this.OnCommandCancelled;
            Application.DocumentManager.MdiActiveDocument.CommandCancelled += this.OnCommandCancelled;
            Application.DocumentManager.MdiActiveDocument.Database.ObjectErased -= OnObjectErased;
            Application.DocumentManager.MdiActiveDocument.Database.ObjectErased += OnObjectErased;
            Application.DocumentManager.DocumentActivated -= activedocument;
            Application.DocumentManager.DocumentActivated += activedocument;
            Application.DocumentManager.MdiActiveDocument.LayoutSwitched += this.layoutSwitched;
            Application.DocumentManager.MdiActiveDocument.ViewChanged += this.viewChanged;
        }

        [CommandMethod("IMAGEORDER")]
        public void ImageOrder()
        {
            Utility.makeImageBottom();
        }

        private void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string cmd = string.Format("IMAGEORDER\n");
            doc.SendStringToExecute(cmd, true, false, false);
        }

        private void viewChanged(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("view changed\n");
        }

        void layoutSwitched(object sender, LayoutSwitchedEventArgs e)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("图层切换完毕\n");
            //lisa
        }
        void activedocument(object o, DocumentCollectionEventArgs e)
        {
            restore();
        }

        void toBeDeActivated(object o, DocumentCollectionEventArgs e)
        {
            if (currentSolution == null)
            {
                globalProperty.ActiveTab = -1;
            }
            else
            {
                globalProperty.ActiveTab = _currentSolution.SId;
                var tmp = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Name;
            }
        }

        private void OnObjectErased(object sender, ObjectErasedEventArgs e)
        {
            // Very simple: we just add our ObjectId to the list
            // for later processing
            if (e.DBObject.ObjectId.IsErased)
            {
                //String msg = e.DBObject.ObjectId.ToString();
                ObjectId oId = e.DBObject.ObjectId;

                Dictionary<ObjectId, Building> bui = buildings;
                Solution sln = currentSolution;
                if (bui.Keys.Contains(oId))
                {
                    //楼房
                    Building buiTmp = buildings[oId];
                    buiTmp.Delete();
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
                        .WriteMessage("删除了楼房\n");
                    return;
                }
                else if (sln == null)
                {

                }
                else if (sln.HeatProducers.Keys.Contains(oId))
                {
                    //删除了热源
                    currentSolution.HeatProducers[oId].Delete();
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
                        .WriteMessage("删除了热源\n");
                }
                else if (sln.SubStations.Keys.Contains(oId))
                {
                    currentSolution.SubStations[oId].Delete();
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
                        .WriteMessage("删除了热力站\n");
                }
                else if (sln.Districts.Keys.Contains(oId))
                {
                    //删除了云线
                    currentSolution.Districts[oId].Delete();
                }
                if (sln != null)
                {
                    sln.TryDeleteObjectInSolution(oId);
                }
            }
        }

        private void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName.CompareTo("REVCLOUD") == 0)
            {
                //云线执行到一半被取消了
                if (HeatSourceLayoutApp.CommandManager.Status())
                {
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    HeatSourceLayoutApp.CommandManager.TriggerCommand();
                    return;
                }
                HeatSourceLayoutApp.CommandManager.ReleaseLock();

            }
        }
        private void OnCommandEnd(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName.CompareTo("REVCLOUD") == 0)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                PromptSelectionResult lastRes = doc.Editor.SelectLast();
                if (lastRes.Value != null && lastRes.Value.Count == 1)
                {
                    //先给云线改颜色
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        //绑定数据
                        Autodesk.AutoCAD.Colors.Color selectedColor = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);
                        int index = ToolPanel.currentConnectedType;
                        if (index == 1)
                        {
                            selectedColor = Autodesk.AutoCAD.Colors.Color.FromRgb(233, 150, 122);
                        }
                        else if (index == 2)
                        {
                            selectedColor = Autodesk.AutoCAD.Colors.Color.FromRgb(30, 144, 255);
                        }
                        else if (index == 3)
                        {
                            selectedColor = Autodesk.AutoCAD.Colors.Color.FromRgb(102, 249, 207);
                        }
                        doc.Editor.WriteMessage(index.ToString());
                        if (index != 0)
                        {
                            Entity ent = (Entity)tr.GetObject(lastRes.Value[0].ObjectId, OpenMode.ForRead);
                            ent.UpgradeOpen();
                            ent.Color = selectedColor;
                            //是否继续循环
                        }
                        tr.Commit();
                        //Utility.makeImageBottom();
                    }
                    District district = new District(currentSolution);
                    district.BaseObjectId = lastRes.Value[0].ObjectId;
                    Error err = district.CheckAndSave(ToolPanel.currentConnectedType);
                    if (err.Err != Error.ErrorCode.NoErr)
                    {
                        System.Windows.MessageBox.Show(err.Message);
                        district.RemoveSelf();

                    }
                    currentSolution.Districts.Add(district.BaseObjectId, district);
                    //MessageBox.Show("in");
                    ToolPanel.DrawCloudLine();
                }
            }

        }

        public void loadConfigData()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Start loading config data\n");

            DataConfig.loadData();

            ed.WriteMessage("Current Version: " + DataConfig.getCurrentVersion());
            ed.WriteMessage("Finish loading config data\n");

        }

        private void closeRibbon()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string cmd = string.Format("RIBBONCLOSE\n");
            doc.SendStringToExecute(cmd, true, false, false);
        }

        public void Terminate()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Application.DocumentManager.MdiActiveDocument.CommandEnded -= new CommandEventHandler(this.OnCommandEnd);
        }

        public static PaletteSet toolPaletteSet;
        public PaletteSet solutionPaletteSet;
        public static PaletteSet attrPaletteSet;
        public static ToolPanel tooPanel;
        public static SolutionPanel solutionPanel;
        public static BaseAttrEditor baseAttrEditor;
        public static SolutionAttrPanel solutionAttrPanel;
        public static SolutionAttrComparePanel solutionAttrComparePanel;
        public static BuildingAttrEditor buildingAttrEditor;
        public static HeatProducerAttrEditor heatProducerAttrEditor;
        public static SubstationAttrEditor substationAttrEditor;
        //test

        [CommandMethod("SCREENSHOT")]
        static public bool createScreen()
        {
            ProgressMeter pm = new ProgressMeter();
            pm.Start("正在生成说明书...");
            pm.SetLimit(100);
            for (int i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(5);
                pm.MeterProgress();
                System.Windows.Forms.Application.DoEvents();
            }
            bool status = createScreenShot(pm);
            //关闭progressbar
            pm.Stop();
            return status;
        }

        //截图每个解决方案
        static public bool createScreenShot(ProgressMeter pm)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;

            CropMessage message = new CropMessage();
            Rectangle rect = new Rectangle();
            bool Crop = false;
            if (message.ShowDialog() == false)
            {
                if (message.SelectedValue == true)
                {
                    Crop = true;
                    bool status = ScreenshotUtil.SelectCropArea(out rect);
                    if (status == false)
                    {
                        return false;
                    }
                }
            }
            int number_display = 10;//进度条的进度；
            if (solutions.Count <= 0)
            {
                return false;
            }
            int len = (100 - number_display) / solutions.Count;
            foreach (var i_solution in solutions)
            {
                //隐藏云线
                solutionPanel.chrometabs.ChangeSelectedItem(i_solution.TabItem);
                HeatSourceLayoutApp.currentSolution = i_solution;
                using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var district in i_solution.Districts)
                        {
                            Entity obj = (Entity)(tr.GetObject(district.Value.BaseObjectId, OpenMode.ForRead));
                            if (!obj.IsErased)
                            {
                                obj.UpgradeOpen();
                                obj.Visible = false;
                            }
                        }
                        tr.Commit();
                    }
                }
                //重新生成模型
                Application.DocumentManager.MdiActiveDocument.Editor.Regen();
                //创建截图目录
                var dir = HeatSourceLayoutApp.CurrentDirectory + "\\tmp\\";
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                //截图
                Bitmap pic = null;
                if (Crop)
                {
                    pic = ScreenshotUtil.CreateScreenShot(rect);
                }
                else
                {
                    pic = ScreenshotUtil.CreateScreenShot();
                }
                if (pic != null)
                {
                    var i = solutions.IndexOf(i_solution);
                    var outFile = HeatSourceLayoutApp.CurrentDirectory + "\\tmp\\" + (i + ".jpg");
                    pic.Save(outFile, GetFormat(outFile));
                }
                //完成一个更新一下进度条
                for (int i = number_display; i < number_display + len; i++)
                {
                    System.Threading.Thread.Sleep(5);
                    pm.MeterProgress();
                    System.Windows.Forms.Application.DoEvents();
                }
                number_display = number_display + len;
                //重新显示云线
                using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var district in i_solution.Districts)
                        {
                            Entity obj = (Entity)(tr.GetObject(district.Value.BaseObjectId, OpenMode.ForRead));
                            if (!obj.IsErased)
                            {
                                obj.UpgradeOpen();
                                obj.Visible = true;
                            }
                        }
                        tr.Commit();
                    }
                }
            }
            return true;
        }

        public static System.Drawing.Imaging.ImageFormat GetFormat(string filename)
        {
            // If all else fails, let's create a PNG
            // (might also choose to throw an exception)

            var imf = System.Drawing.Imaging.ImageFormat.Png;
            if (filename.Contains("."))
            {
                // Get the filename's extension (what follows the last ".")

                string ext = filename.Substring(filename.LastIndexOf(".") + 1);

                // Get the first three characters of the extension

                if (ext.Length > 3)
                    ext = ext.Substring(0, 3);

                // Choose the format based on the extension (in lowercase)

                switch (ext.ToLower())
                {
                    case "bmp":
                        imf = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                    case "gif":
                        imf = System.Drawing.Imaging.ImageFormat.Gif;
                        break;
                    case "jpg":
                        imf = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case "tif":
                        imf = System.Drawing.Imaging.ImageFormat.Tiff;
                        break;
                    case "wmf":
                        imf = System.Drawing.Imaging.ImageFormat.Wmf;
                        break;
                    default:
                        imf = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                }
            }
            return imf;
        }

        //test
        [CommandMethod("hs_tool")]
        public void showTool()
        {
            if (toolPaletteSet == null)
            {
                toolPaletteSet = new PaletteSet("工具栏", new System.Guid("D61D0875-A507-4b73-8B5F-9266BE399595"));
                tooPanel = new ToolPanel();
                toolPaletteSet.AddVisual("ToolPanel", tooPanel);
            }
            toolPaletteSet.Visible = true;
            toolPaletteSet.SetSize(new System.Drawing.Size(185, 570));
        }

        [CommandMethod("hs_solution")]
        public void showSolution()
        {
            if (solutionPaletteSet == null)
            {
                solutionPaletteSet = new PaletteSet("解决方法", new System.Guid("D61D0875-A507-4b73-8B5F-9233FF399585"));
            }
            while (solutionPaletteSet.Count > 0)
            {
                solutionPaletteSet.Remove(0);
            }
            solutionPanel = new SolutionPanel();
            //solutionPaletteSet.Location = new System.Drawing.Point(20, 20);
            solutionPaletteSet.AddVisual("SolutionPanel", solutionPanel);
            solutionPaletteSet.DockEnabled = DockSides.Bottom;
            solutionPanel.Height = 41;
            solutionPaletteSet.Visible = true;
        }

        [CommandMethod("hs_attr")]
        public void showAttrsEditor()
        {
            if (attrPaletteSet == null)
            {
                attrPaletteSet = new PaletteSet("属性面板", new System.Guid("D61D0875-A507-4b73-8B5F-9221FF392200"));
                baseAttrEditor = new BaseAttrEditor();
                attrPaletteSet.AddVisual("基础属性", baseAttrEditor);
                buildingAttrEditor = new BuildingAttrEditor();
                attrPaletteSet.AddVisual("楼房属性", buildingAttrEditor);
                heatProducerAttrEditor = new HeatProducerAttrEditor();
                attrPaletteSet.AddVisual("热源属性", heatProducerAttrEditor);
                substationAttrEditor = new SubstationAttrEditor();
                attrPaletteSet.AddVisual("热力站属性", substationAttrEditor);
                attrPaletteSet.SetSize(new System.Drawing.Size(120, 350));
                solutionAttrPanel = new SolutionAttrPanel();
                attrPaletteSet.AddVisual("方案属性", solutionAttrPanel);
                solutionAttrComparePanel = new SolutionAttrComparePanel();
                attrPaletteSet.AddVisual("方案对比", solutionAttrComparePanel);
            }
            attrPaletteSet.Visible = true;
            attrPaletteSet.Activate(0);
            attrPaletteSet.PaletteActivated += new PaletteActivatedEventHandler(paletteActive);
        }

        [CommandMethod("hs_base_attr")]
        public void showBaseAttrsEditor()
        {
            baseAttrEditor.updateBaseView();
            attrPaletteSet.Activate(0);
        }

        [CommandMethod("hs_solution_attr")]
        public void showSolutionAttrsEditor()
        {
            attrPaletteSet.Activate(4);
        }

        [CommandMethod("hs_building_attr")]
        public void showBuildingAttrsEditor()
        {
            attrPaletteSet.Activate(1);
        }

        [CommandMethod("hs_heatproducer_attr")]
        public void showHeatproducerAttrsEditor()
        {
            attrPaletteSet.Activate(2);
        }

        [CommandMethod("hs_substation_attr")]
        public void showSubstationAttrsEditor()
        {
            attrPaletteSet.Activate(3);
        }

        public void LoadMLineStyle()
        {
            Utility.LoadMlineStyle();
        }

        public static void changePaletteWidth(int width)
        {
            toolPaletteSet.SetSize(new System.Drawing.Size(width, 570));
        }

        public void paletteActive(object sender, PaletteActivatedEventArgs e)
        {
            baseAttrEditor.updateBaseView();
            solutionAttrPanel.SetSolution(currentSolution);
            solutionAttrComparePanel.SetSolution();
        }

        public BaseModel lastDoubleClickModel = null;

        public void DoubleClick(object sender, BeginDoubleClickEventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptSelectionResult res = ed.SelectImplied();
            if (res.Status != PromptStatus.Error)
            {
                if ((lastDoubleClickModel is SubStation || lastDoubleClickModel is HeatProducer) &&
                    currentSolution != null)
                {
                    currentSolution.CancelHighLightPipeLine();
                }
                SelectionSet set = res.Value;
                if (set.Count > 0)
                {
                    List<ObjectId> vals = new List<ObjectId>();
                    for (int i = 0; i < set.Count; i++)
                    {
                        if (set[i].SelectionMethod != SelectionMethod.Unavailable)
                        {
                            vals.Add(set[i].ObjectId);
                        }
                    }
                    if (vals.Count == 0)
                    {
                        return;
                    }
                    ObjectId lastId = vals.Last();
                    //从选择的项目中移除除最后的选择外的其他选择
                    ObjectId[] newids = { lastId };
                    ed.SetImpliedSelection(newids);
                    if (HeatSourceLayoutApp.buildings.ContainsKey(lastId))
                    {
                        Building b = buildings[lastId];
                        lastDoubleClickModel = b;
                        showBuildingAttrsEditor();
                        buildingAttrEditor.SetBuilding(b);
                        return;
                    }
                    if (currentSolution == null)
                    {
                        return;
                    }
                    HeatProducer producer = HeatSourceLayoutApp.currentSolution.GetHeatProducer(lastId);
                    if (producer != null)
                    {
                        lastDoubleClickModel = producer;
                        showHeatproducerAttrsEditor();
                        heatProducerAttrEditor.SetHeatProducer(producer);
                        currentSolution.HighLight(producer);
                        return;
                    }
                    SubStation substation = HeatSourceLayoutApp.currentSolution.GetSubStation(lastId);
                    if (substation != null)
                    {
                        lastDoubleClickModel = substation;
                        showSubstationAttrsEditor();
                        substationAttrEditor.SetSubstation(substation);
                        currentSolution.HighLight(substation);
                        return;
                    }
                }

            }

        }
        
        [CommandMethod("insertmap")]
        public static void InsertMap()
        {
            ObjectId imageId = Utility.addStaticGoogleMap();
            //pick scale
            if(imageId != ObjectId.Null)
            {
                globalProperty.RasterImageID = imageId;
            }
        }
        [CommandMethod("rescale")]
        public static void rescale()
        {
            if (globalProperty.RasterImageID == ObjectId.Null)
            {
                return;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptDoubleResult distance = ed.GetDistance("\n 选择校正线段");
            PromptDoubleOptions opt = new PromptDoubleOptions("\n 请输入实际距离(m)");
            PromptDoubleResult res = ed.GetDouble(opt);
            if (res.Status == PromptStatus.Cancel)
            {
                return;
            }
            //calculate scale
            double d = distance.Value;
            double r = res.Value;
            //set scale
            double scale = r / d;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                RasterImage img = (RasterImage)tr.GetObject(globalProperty.RasterImageID, OpenMode.ForWrite);
                double oldscale = img.Scale.X;
                img.Orientation = new CoordinateSystem3d(new Point3d(0, 0, 0), new Vector3d(img.Width * scale, 0, 0), new Vector3d(0, img.Height * scale, 0));
                double newscale = img.Scale.X;
                ed.WriteMessage(oldscale.ToString() + ":" + newscale.ToString());
                tr.Commit();
            }
        }

        public static bool isReStoring = false;
        [CommandMethod("restore")]
        public void restore()
        {
            isReStoring = true;
            if (Application.DocumentManager.MdiActiveDocument == null) return;
            Application.DocumentManager.MdiActiveDocument.Database.ObjectAppended -= Database_ObjectAppended;
            globalProperty = new GlobalProperty(true);
            Utility.ImportBlocks();
            BindEventListener();
            showSolution();
            BaseModel.ApplicationBaseModels.Clear();
            buildings.Clear();
            solutions.Clear();
            ToolPanel.changedEntityList.Clear();
            currentSolution = null;
            this.LoadMLineStyle();
            if (Application.DocumentManager.MdiActiveDocument == null)
            {
                return;
            }
            //先将所有注册的对象都清空!
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                globalProperty.Initialize();
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    //首先重建所有的Solution
                    LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite, false);
                    foreach (ObjectId layerId in lt)
                    {
                        //设置图层颜色
                        LayerTableRecord ltr = (LayerTableRecord)trans.GetObject(layerId, OpenMode.ForWrite);
                        Dictionary<String, String> pairs = BaseModel.Load(layerId);
                        if (pairs != null && pairs.ContainsKey("type") && pairs["type"].CompareTo("solution") == 0)
                        {
                            ChromeTabItem chrometabitem = solutionPanel.chrometabs.RestoreTab(new System.Windows.Controls.Label(), false);
                            chrometabitem.CanDelete = true;
                            solutionPanel.AddCloseHandler(chrometabitem);
                            Solution solution = new Solution(false);
                            solution.ResetAttributes(pairs, layerId);
                            solution.RestoreAttributes();
                            solution.TabItem = chrometabitem;
                            chrometabitem.Header = solution.SolutionName;
                            solutions.Add(solution);
                        }
                        else if (ltr.Name != "our_outline_layer" && (pairs == null || !pairs.ContainsKey("type")))
                        {
                            ltr.Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Gray);
                        }
                    }
                    trans.Commit();
                    trans.Dispose();
                }
                using (Transaction trans1 = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForWrite);
                    foreach (var layerid in lt)
                    {
                        LayerTableRecord ltr = (LayerTableRecord)layerid.GetObject(OpenMode.ForRead);
                        if (ltr.Name.Contains("outline_layer") || ltr.Name.Contains("方案"))
                        {
                            TypedValue[] tvs = new TypedValue[1] { new TypedValue((int)DxfCode.LayerName, (ltr.Name)) };
                            SelectionFilter sf = new SelectionFilter(tvs);
                            PromptSelectionResult psr = ed.SelectAll(sf);
                            if (psr.Value != null)
                            {
                                foreach (var objectid in psr.Value.GetObjectIds())
                                {
                                    Dictionary<String, String> pairs = BaseModel.Load(objectid);
                                    if (pairs != null && pairs.ContainsKey("type"))
                                    {
                                        String type = pairs["type"];
                                        if (type.CompareTo(Building.modelType) == 0)
                                        {
                                            Building b = new Building(false);
                                            b.ResetAttributes(pairs, objectid);
                                            buildings.Add(objectid, b);
                                        }
                                        else if (type.CompareTo(HeatProducer.modelType) == 0)
                                        {
                                            var b = new HeatProducer();
                                            b.ResetAttributes(pairs, objectid);
                                        }
                                        else if (type.CompareTo(SubStation.modelType) == 0)
                                        {
                                            var b = new SubStation();
                                            b.ResetAttributes(pairs, objectid);
                                        }

                                        else if (type.CompareTo(PipeLine.modelType) == 0)
                                        {
                                            var b = new PipeLine();
                                            b.ResetAttributes(pairs, objectid);
                                            b.RetriveMline();
                                        }
                                        else if (type.CompareTo(District.modelType) == 0)
                                        {
                                            var d = new District();
                                            d.ResetAttributes(pairs, objectid);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    trans1.Dispose();
                }
                using (Transaction trans2 = db.TransactionManager.StartTransaction())
                {

                    //然后遍历所有的管道结头
                    DBDictionary groups = trans2.GetObject(db.GroupDictionaryId, OpenMode.ForRead) as DBDictionary;
                    foreach (DBDictionaryEntry entry in groups)
                    {
                        ObjectId objId = entry.Value;
                        Dictionary<String, String> pairs = BaseModel.Load(objId);
                        if (pairs != null && pairs.ContainsKey("type"))
                        {
                            string type = pairs["type"];
                            if (type.CompareTo(SimplePipeJoint.modelType) == 0)
                            {
                                var b = new SimplePipeJoint();
                                b.ResetAttributes(pairs, objId);
                                b.Retrive();
                            }
                            if (type.CompareTo(MultiPipeJoint.modelType) == 0)
                            {
                                var b = new MultiPipeJoint();
                                b.ResetAttributes(pairs, objId);
                                b.Retrive();
                            }
                        }
                        // }

                    }
                    trans2.Dispose();
                }
                foreach (var item in BaseModel.ApplicationBaseModels)
                {
                    item.Value.RestoreAttributes();
                }

                if (globalProperty.ActiveTab == -1 || globalProperty.ActiveTab == 0)
                {
                    solutionPanel.SelectOutLineLayer();
                    HeatSourceLayoutApp.currentSolution = null;
                }
                else
                {
                    foreach (var i_solution in solutions)
                    {
                        if (i_solution.SId == globalProperty.ActiveTab)
                        {
                            solutionPanel.chrometabs.ChangeSelectedItem(i_solution.TabItem);
                            HeatSourceLayoutApp.currentSolution = i_solution;
                            break;
                        }
                    }
                }
            }
            isReStoring = false;
            Application.DocumentManager.MdiActiveDocument.Database.ObjectAppended += Database_ObjectAppended;
        }

        public static double TotalBuildingsArea()
        {
            double area = 0;
            foreach (var b in buildings)
            {
                area += b.Value.Area;
            }
            return area;
        }

        public static double TotalHeatConsumption()
        {
            double heat = 0;
            foreach (var b in buildings)
            {
                heat += b.Value.YearHeat;
            }
            return heat;
        }
    }
}
