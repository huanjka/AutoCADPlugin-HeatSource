using System.Windows.Controls;
using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using HeatSource.Model;
using HeatSource.Utils;
using System.Collections;
using Autodesk.AutoCAD.Colors;
using System.Linq;
using Autodesk.AutoCAD.Internal;
using HeatSource.Utils.UseDrawJig;
using System.Threading;
using Autodesk.AutoCAD.Runtime;


namespace HeatSource.View
{
    /// <summary>
    /// Interaction logic for ToolPanel.xaml
    /// </summary>
    public partial class ToolPanel : UserControl
    {
        static int currentSolutionIndex = -1;
        //解决方案上一次使用的颜色
        static List<int> currentColorIndex = new List<int>();
        static List<Color> colors = new List<Autodesk.AutoCAD.Colors.Color>(){
                Color.FromRgb(255, 0, 0),
                Color.FromRgb(255, 125, 0),
                Color.FromRgb(255, 255, 0),
                Color.FromRgb(0, 255, 0),
                Color.FromRgb(0, 0,255),
                Color.FromRgb(0,255,255),
                Color.FromRgb(255, 0, 255),
                Color.FromRgb(231, 14, 145),
                Color.FromRgb(8, 4, 3),
                Color.FromRgb(165, 234, 255),
                Color.FromRgb(255, 220, 252)
        };
        //刚刚新建一个解决方案时，需要将所有变色的原素变回来
        public static HashSet<ObjectId> changedEntityList = new HashSet<ObjectId>(); 
        //每个解决方案每个对象对应的颜色list
        public static List<Dictionary<ObjectId, Color>> solutionColorList = new List<Dictionary<ObjectId, Color>>();
        // current selected item is heatsource-substaion:1 or heatsource-building:2 substation-building:3
        public static int currentConnectedType = 0;

        public bool controlLoop = true;



        public static void resetCurrentConnectedType(){
            currentConnectedType = 0;
        }

        public ToolPanel()
        {
            InitializeComponent();
        }

        //圈选后将圈选的信息保存
        private static void  saveColorInfo(ObjectId objId,Color color){
            if (currentSolutionIndex + 1 > solutionColorList.Count){
                solutionColorList.Add(new Dictionary<ObjectId, Color>());
            }
            Dictionary<ObjectId, Color> currDic = solutionColorList[currentSolutionIndex];
            if (currDic.ContainsKey(objId))
            {
                currDic[objId] = color;
            }
            else
            {
                currDic.Add(objId, color);
            }
        }

        //删除当前解决方案里颜色列表的某个对象
        private static void deleteColorObjById(ObjectId objId) { 
            Dictionary<ObjectId, Color> currDic = solutionColorList[currentSolutionIndex];
            if (currDic.ContainsKey(objId)) {
                currDic.Remove(objId);
            }
        }

        //改变目前所在的解决方案
        public static void changeSolution(int index) {
            currentSolutionIndex = index;
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("当前图层" + (index).ToString());
            
            //同时需要更新当前对象的所有颜色
            updateColor();
        }

        //删除解决方案
        public static void deleteSolution(int index ) {
            solutionColorList[index] = null;
            //如果后面有，切换解决方案颜色
            if (index + 1 <= solutionColorList.Count-1) {
                changeSolution(index + 1);
               
            }
            else if (index - 1 >= 0)
            {
                changeSolution(index - 1);
               
            }
            else {
                changeSolution(-1);
            }
        }

        //更新颜色
        private static void updateColor()
        {
            //初始化颜色表
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            //将目前所有颜色都置为白色
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
               
                for(int i = changedEntityList.Count-1;i >= 0;i--)
                {
                    ObjectId oId = changedEntityList.ElementAt(i);
                    //此对象被删除了
                    if (oId.IsErased)
                    {
                        changedEntityList.Remove(oId);
                        //将每个图层的保存的键值对中将此线段删除
                        foreach (Dictionary<ObjectId, Color> item in solutionColorList) {
                            item.Remove(oId);
                        }
                    }
                    else
                    {
                        Entity ent = (Entity)tr.GetObject(oId, OpenMode.ForRead);

                        ent.UpgradeOpen();
                        ent.Color = Color.FromRgb(255, 255, 255);
                    }


                }
                tr.Commit();
            }
            //
            if (currentSolutionIndex + 1 > solutionColorList.Count )
            {
                solutionColorList.Add(new Dictionary<ObjectId, Color>());

                return;
            }
            Dictionary<ObjectId, Color> currDic = solutionColorList[currentSolutionIndex];
            tr = db.TransactionManager.StartTransaction();
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {

                foreach (var item in currDic)
                {
                    //检查此对象是否被删除
                    Color currColor = null;
                    currColor = item.Value;
                    //改变选中线段的颜色
                    Entity ent = (Entity)tr.GetObject(item.Key, OpenMode.ForRead);
                    if (ent != null) {
                        ent.UpgradeOpen();
                        ent.Color = currColor;
                    }
                    else
                    {
                        //此对象可以被删除了，因此要在记录中也删除相关对象
                        deleteColorObjById(item.Key);
                    }

                }
                tr.Commit();
            }
            
        }

        //获取下一个颜色
        private static Color getNextColor()
        {
            if(currentColorIndex == null)
                currentColorIndex = new List<int>();
            if (currentColorIndex.Count < currentSolutionIndex+1)
                currentColorIndex.Add(-1);
            var index = currentColorIndex[currentSolutionIndex] + 1;
            
            if (index >= colors.Count) {
                index = 0;
            }
            currentColorIndex[currentSolutionIndex] = index;
            return colors[index];

        }

        //生成block预览
     

        //插入block
        public void Insertblock(string s, out bool status)
        {
            status = false;
            DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument();
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId spaceId = db.CurrentSpaceId;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            String promptText = s.Contains("锅炉") ? "\n 选择插入热源的位置" : "\n 选择插入热力站的位置";
            PromptPointOptions getPointOptions = new PromptPointOptions(promptText);
            getPointOptions.AppendKeywordsToMessage = true;
            PromptPointResult getPointResult = ed.GetPoint(getPointOptions);

            if(getPointResult.Status == PromptStatus.Cancel)
            {
                status = true;
                return;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectId layerid = HeatSourceLayoutApp.currentSolution.BaseObjectId;
                LayerTableRecord layer = trans.GetObject(layerid, OpenMode.ForWrite) as LayerTableRecord;
                ObjectId blockid = spaceId.InsertBlockReference(layer.Name, s, getPointResult.Value, new Scale3d(HeatSourceLayoutApp.globalProperty.BlockScale), 0);
                trans.Commit();
                //将新建的热力站/热源存入解决方案的对象中
                if (s.CompareTo("锅炉5吨") == 0)
                {
                    HeatProducer producer = new HeatProducer(HeatSourceLayoutApp.currentSolution, true);
                    producer.BaseObjectId = blockid;
                    producer.Save();
                    HeatSourceLayoutApp.currentSolution.HeatProducers.Add(blockid, producer);
                }
                else if (s.CompareTo("换热机组") == 0)
                {
                        SubStation station = new SubStation(HeatSource.HeatSourceLayoutApp.currentSolution, true);
                        station.BaseObjectId = blockid;
                        station.Save();
                        HeatSourceLayoutApp.currentSolution.SubStations.Add(blockid, station);
                 }
            }
        }

        //改变按钮颜色
        private void changeButtonBackground(Button btn, String imgSource)
        {
            System.Windows.Media.ImageBrush background = new System.Windows.Media.ImageBrush();
#if (DEBUG)
            background.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(HeatSourceLayoutApp.CurrentDirectory + @"/../../img/"+imgSource+".png"));
#else
            background.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(HeatSourceLayoutApp.CurrentDirectory +"/img/" + imgSource + ".png")); 
#endif
            
            background.Stretch = System.Windows.Media.Stretch.Uniform;
            btn.Background = background;        
        }

        //改变按钮样式
        public void changeBtnStyle(int index) {
            //工具栏的所有btn
            List<Button> toolBtn = new List<Button>();
           //顺序不能变
            toolBtn.Add(basePropertyBtn);
            toolBtn.Add(drawBuildingBtn);
            toolBtn.Add(drawRecBtn);
            toolBtn.Add(drawPiplineBtn);
            toolBtn.Add(addSourceBtn);
            toolBtn.Add(addStationBtn);
            toolBtn.Add(connectHSBtn);
            toolBtn.Add(connectHBBtn);
            toolBtn.Add(connectSBBtn);
            toolBtn.Add(generateIntroBtn);
            toolBtn.Add(drawPiplineBuildingBtn);
            toolBtn.Add(importImageBtn);
            toolBtn.Add(ajustPropBtn);

            foreach (Button btn in toolBtn) {
                changeButtonBackground(btn, btn.Name.Substring(0, btn.Name.Length - 3));
            }


            if (index != -1) {
                var e = toolBtn[index];
                changeButtonBackground((Button)myListView.FindName(e.Name.Substring(0, e.Name.Length - 3)+"Btn"), e.Name.Substring(0, e.Name.Length - 3) + "_select");
            }
        }

        private ObjectId AppendEntity(Entity ent)
        {
            ObjectId entId;
            ent.Visible = true;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId,
                    OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject
                    (bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ((Polyline)ent).ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                entId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }
            return entId;
        }

        //获取圈选的楼房
        public static  List<Building> getCircleBuildings() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            List<Building> buildings;
            if (currentSolutionIndex == -1)
            {
                System.Windows.Forms.MessageBox.Show("请先新建解决方案!");
                return null ;
            }
            try
            {
                //设置过滤器
                TypedValue[] tvs = new TypedValue[] {
                    new TypedValue((int)DxfCode.Operator,"<and"),
                    new TypedValue((int)DxfCode.LayerName,"our_outline_layer"),
                    new TypedValue((int)DxfCode.Operator,"and>")      
                };
                SelectionFilter sf = new SelectionFilter(tvs);
                //让用户进行圈画选择

                PromptSelectionResult psr = ed.GetSelection(sf);
                if (psr.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("Action releasd");
                    return null;
                }
                else
                {
                    if (psr.Value == null)
                        return null;

                    Database db = doc.Database;
                    Transaction tr = db.TransactionManager.StartTransaction();

                    //获得所选的objectId的集合 
                    ObjectId[] objectIds = psr.Value.GetObjectIds();

                    ed.WriteMessage(">>>", psr.Value.Count);
                    buildings = new List<Building>();

                    using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        Color currColor = getNextColor();
                        foreach (ObjectId oId in objectIds)
                        {
                            if (HeatSourceLayoutApp.buildings.ContainsKey(oId))
                            {
                                Building bui = HeatSourceLayoutApp.buildings[oId];
                                buildings.Add(bui);
                            }
                            //改变选中线段的颜色
                            Entity ent = (Entity)tr.GetObject(oId, OpenMode.ForRead);
                            ent.UpgradeOpen();
                            ent.Color = currColor;
                            //保存颜色信息
                            saveColorInfo(oId, currColor);
                            //记录修改过的颜色的信息 
                            changedEntityList.Add(oId);
                        }
                        //生成地区
                        //District dt = new District();
                        //dt.Buildings = buildings;
                        //加入solution
                        Solution sl = HeatSource.HeatSourceLayoutApp.currentSolution;
                        if (sl != null)
                        {
                            //sl.Districts.Add(dt);
                        }
                        tr.Commit();
                    }
                }
                return buildings;
            }
            catch (System.Exception exc)
            {
                ed.WriteMessage("Exception" + exc.ToString());
                return null;
            }
      
        }

        /*
        * click events start
        */

        //折叠按钮
        private void foldBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            var width = HeatSource.HeatSourceLayoutApp.toolPaletteSet.PaletteSize.Width;
           // System.Windows.MessageBox.Show(width + "");
            if (width >= 165)
            {
                return;
            }
            var changewidth = 0;
            if (width <= 70)
            {
                changewidth = 180;
                changeButtonBackground(foldBtn, "left-arrow");

            }
            else
            {
                changewidth = 70;
                changeButtonBackground(foldBtn, "right-arrow");
            }
            HeatSource.HeatSourceLayoutApp.changePaletteWidth(changewidth);
        }

        //基础属性框按钮
        private void basePropertyBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string cmd = string.Format("hs_attr\n");
            doc.SendStringToExecute(cmd, true, false, false);
        }
        private void SaveImage(System.Drawing.Bitmap image)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(fileName);
                if (extension == ".jpg")
                {
                    image.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    image.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }

        //绘制楼房按钮
        public void drawBuildingBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawBuildingPoly);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(1);
            }
            Utility.SetOrthoMode(false);
            while (true) {
                HeatSourceLayoutApp.solutionPanel.SelectOutLineLayer();

                bool status = false;
                ObjectId objId = Utils.MyPlineCmds.MyPolyJig(out status);
                if(HeatSourceLayoutApp.CommandManager.Status())
                {
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    HeatSourceLayoutApp.CommandManager.TriggerCommand();
                    break;
                }
                if(status == false)
                {
                    //CommandManager.UnRegister();
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    break;
                }
                if (objId != ObjectId.Null)
                {
                    Building b = new Building(true);
                    b.BaseObjectId = objId;
                    b.Save();
                    HeatSourceLayoutApp.buildings.Add(objId, b);
                }     
            }
        }


        //矩形绘制按钮
        public void drawRectBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            //CommandManager.Register();
            if(!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawBuildingRect);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);

                return;
            }
            else
            {
                changeBtnStyle(2);
            }
            Utility.SetOrthoMode(false);      
            HeatSourceLayoutApp.solutionPanel.SelectOutLineLayer();
            using(DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                while (true)
                {
                    PromptPointOptions optPoint = new PromptPointOptions
                        ("\n请指定矩形的一个角点");
                    PromptPointResult resPoint = ed.GetPoint(optPoint);
                    if(HeatSourceLayoutApp.CommandManager.Status())
                    {
                        HeatSourceLayoutApp.CommandManager.ReleaseLock();
                        HeatSourceLayoutApp.CommandManager.TriggerCommand();
                        return;
                    }
                    if (resPoint.Status != PromptStatus.OK)
                    {
                        HeatSourceLayoutApp.CommandManager.ReleaseLock();
                        return;
                    }
                    Point3d pt1 = resPoint.Value;
                  

                    Polyline polyLine2 = new Polyline();
                    for (int i = 0; i < 4; i++)
                    {
                        polyLine2.AddVertexAt(i, new Point2d(0, 0), 0, 0, 0);
                    }
                    polyLine2.Closed = true;

                    polyLine2.Color = Color.FromRgb(255, 0, 0);

                    RecJig elRecJig = new RecJig(pt1, polyLine2);

                    PromptResult resJig = ed.Drag(elRecJig);
                    if (resJig.Status == PromptStatus.OK)
                    {
                        if (elRecJig.m_PolyLine2.Area >= 1e-10)
                        {
                            ObjectId objid = AppendEntity(elRecJig.m_PolyLine2);
                            Building b = new Building(true);
                            b.BaseObjectId = objid;
                            HeatSource.HeatSourceLayoutApp.buildings.Add(objid, b);
                            //b.AddBuildingNumber();
                            b.Save();
                        }
                    }
                }
            }
        }

        public void drawPiplineBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (HeatSourceLayoutApp.currentSolution == null)
            {
                System.Windows.MessageBox.Show("请选择解决方案");
                HeatSourceLayoutApp.CommandManager.ReleaseLock();
                return;
            }
            else
            {
                changeBtnStyle(3);
            }

            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawPipeLine);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            //默认打开正交模式
            Utility.SetOrthoMode(true);
            HeatSourceLayoutApp.currentSolution.CurrentLineStyle = Solution.PipeLineStyle.HeatSourceConnectHeatProducer;
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                while (true)
                {
                    if (HeatSourceLayoutApp.currentSolution != null)
                    {
                        bool status = false;
                        HeatSourceLayoutApp.currentSolution.CreatePipeLine(out status);
                        if (HeatSourceLayoutApp.CommandManager.Status())
                        {
                            HeatSourceLayoutApp.CommandManager.ReleaseLock();
                            HeatSourceLayoutApp.CommandManager.TriggerCommand();
                            Utility.SetOrthoMode(false);
                            return;
                        }
                        if(status)
                        {
                            HeatSourceLayoutApp.CommandManager.ReleaseLock();
                            Utility.SetOrthoMode(false);
                            return;
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("请先进入解决方案");
                        HeatSourceLayoutApp.CommandManager.ReleaseLock();
                        Utility.SetOrthoMode(false);
                        return;
                    }
                }
            }
            
            //计算主管道长度的变化
            //HeatSourceLayoutApp.currentSolution.GetMainPipeLineLength();
        }

        //热源热力站-楼房按钮
        public void drawPiplineBuildingBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (HeatSourceLayoutApp.currentSolution == null)
            {
                System.Windows.MessageBox.Show("请选择解决方案");
                HeatSourceLayoutApp.CommandManager.ReleaseLock();
                return;
            }
            else
            {
               changeBtnStyle(10);
            }

            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawPipeLineBuilding);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            //默认打开正交模式
            Utility.SetOrthoMode(true);
            HeatSourceLayoutApp.currentSolution.CurrentLineStyle = Solution.PipeLineStyle.AnyConnectedBuilding;
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                while (true)
                {
                    if (HeatSourceLayoutApp.currentSolution != null)
                    {
                        bool status = false;
                        HeatSourceLayoutApp.currentSolution.CreatePipeLine(out status);
                        if (HeatSourceLayoutApp.CommandManager.Status())
                        {
                            HeatSourceLayoutApp.CommandManager.ReleaseLock();
                            HeatSourceLayoutApp.CommandManager.TriggerCommand();
                            Utility.SetOrthoMode(false);
                            return;
                        }
                        if (status)
                        {
                            HeatSourceLayoutApp.CommandManager.ReleaseLock();
                            Utility.SetOrthoMode(false);
                            return;
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("请先进入解决方案");
                        HeatSourceLayoutApp.CommandManager.ReleaseLock();
                        Utility.SetOrthoMode(false);
                        return;
                    }
                }
            }

            //计算主管道长度的变化
            //HeatSourceLayoutApp.currentSolution.GetMainPipeLineLength();
        }

        //添加热源按钮
        public void addSourceBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawHeatProducer);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);

                return;
            }
            else
            {
                changeBtnStyle(4);
            }
            
            //添加block
           
            while (true)
            {
                if (HeatSourceLayoutApp.currentSolution == null)
                {
                    System.Windows.MessageBox.Show("请选择解决方案");
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    return;
                }
                bool status;
                Insertblock("锅炉5吨", out status);
                if (HeatSourceLayoutApp.CommandManager.Status())
                {
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    HeatSourceLayoutApp.CommandManager.TriggerCommand();
                    return;
                }
                if (status)
                {                   
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    return;
                }
            }

        }


        //添加热力站按钮
        public void addStationBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawSubStation);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(5);
            }
           
            //添加block
           
            while (true)
            {
                if (HeatSourceLayoutApp.currentSolution == null)
                {
                    System.Windows.MessageBox.Show("请选择解决方案");
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    return;
                }
                bool status;
                Insertblock("换热机组", out status);
                if (HeatSourceLayoutApp.CommandManager.Status())
                {
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    HeatSourceLayoutApp.CommandManager.TriggerCommand();
                    return;
                }
                if (status)
                {
                    HeatSourceLayoutApp.CommandManager.ReleaseLock();
                    return;
                }
            }
        }

        //关联热力站－热源
        public void connectHSBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawHeatProducerSubStationCloud);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(6);
            }
           
            currentConnectedType = 1;
            DrawCloudLine();
          
        }
        //关联热源-楼房
        public void connectHBBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawHeatProducerBuildingCloud);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(7);
            }

            if (HeatSourceLayoutApp.currentSolution == null)
            {
                System.Windows.MessageBox.Show("请先进入解决方案");
                HeatSourceLayoutApp.CommandManager.ReleaseLock();
                return;
            }
            currentConnectedType = 2;
            DrawCloudLine();

        }
        //关联热力站-楼房
        public void connectSBBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
   
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.DrawSubStationBuildingCloud);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(8);
            }

            if (HeatSourceLayoutApp.currentSolution == null)
            {
                System.Windows.MessageBox.Show("请先进入解决方案");
                changeBtnStyle(-1);
                HeatSourceLayoutApp.CommandManager.ReleaseLock();
                return;
            }


            currentConnectedType = 3;
            DrawCloudLine();

        }
        
        //生成说明书按钮
        public void generateIntroBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.GenerateDocument);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(9);
            }
            WordUtil.CreateWord();
            HeatSourceLayoutApp.CommandManager.ReleaseLock();
            changeBtnStyle(-1);
        }

        //导入图片
        public void importImageBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.ImportBackgroundImage);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(11);
            }
            HeatSourceLayoutApp.InsertMap();
            HeatSourceLayoutApp.CommandManager.ReleaseLock();
            changeBtnStyle(-1);
        }

        //调整比例
        public void ajustPropBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HeatSourceLayoutApp.CommandManager.RequireLock())
            {
                HeatSourceLayoutApp.CommandManager.AddCommand(CommandManager.ToolCommand.AdjustImageScale);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, false);
                return;
            }
            else
            {
                changeBtnStyle(12);
            }
            HeatSourceLayoutApp.rescale();
            HeatSourceLayoutApp.CommandManager.ReleaseLock();
            changeBtnStyle(-1);
        }
        
        public static void DrawCloudLine()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string cmd = string.Format("revCloud\n");
            doc.SendStringToExecute(cmd, true, false, false);
            cmd = string.Format("F\n");
            doc.SendStringToExecute(cmd, true, false, false);
            cmd = string.Format("A\n");
            doc.SendStringToExecute(cmd, true, false, false);
            cmd = string.Format("20\n");
            doc.SendStringToExecute(cmd, true, false, false);
            cmd = string.Format("40\n");
            doc.SendStringToExecute(cmd, true, false, false);
        }
        //锅炉楼房数据关联的选择
        private void beganDataBindSelect()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string cmd = string.Format("SELECT\n");
            doc.SendStringToExecute(cmd, true, false, false);

            cmd = string.Format("WP\n");
            doc.SendStringToExecute(cmd, true, false, false);
        }

        /*
        * click events end
        */
    }

    //Block工具静态类
    public static class BlockTools
    {
        /// <summary>
        /// 在AutoCAD图形中插入块参照
        /// </summary>
        /// <param name="spaceId">块参照要加入的模型空间或图纸空间的Id</param>
        /// <param name="layer">块参照要加入的图层名</param>
        /// <param name="blockName">块参照所属的块名</param>
        /// <param name="position">插入点</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <param name="attNameValues">属性的名称与取值</param>
        /// <returns>返回块参照的Id</returns>
        public static ObjectId InsertBlockReference(this ObjectId spaceId, string layer, string blockName, Point3d position, Scale3d scale, double rotateAngle)
        {
            DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument();
            ObjectId blockRefId;
            Database db = spaceId.Database;
            BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            if (!bt.Has(blockName)) return ObjectId.Null;
            BlockTableRecord space = (BlockTableRecord)spaceId.GetObject(OpenMode.ForWrite);
            BlockReference br = new BlockReference(position, bt[blockName]);
            br.ScaleFactors = scale;
            br.Layer = layer;
            br.Rotation = rotateAngle;
            ObjectId btrId = bt[blockName];
            BlockTableRecord record = (BlockTableRecord)btrId.GetObject(OpenMode.ForRead);
            if (record.Annotative == AnnotativeStates.True)
            {
                ObjectContextCollection contextCollection = db.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                ObjectContexts.AddContext(br, contextCollection.GetContext("1:1"));
            }
            blockRefId = space.AppendEntity(br);
            db.TransactionManager.AddNewlyCreatedDBObject(br, true);
            space.DowngradeOpen();
            docLock.Dispose();
            return blockRefId;
        }

    }
}
