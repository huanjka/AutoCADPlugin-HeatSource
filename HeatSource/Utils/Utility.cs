using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using System.Reflection;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using System.Text;
using Autodesk.AutoCAD.Windows;
using System.Globalization;

namespace HeatSource.Utils
{
    class Utility
    {

        public enum TypeofLine
        {
            StraghtLine = 0,
            ImaginaryLine = 1,
        };



        // 创建直线
        // <param name="p1"></param>
        // <param name="p2"></param>
        // <returns>id of the line object</returns>
        public static ObjectId createLine(Point3d p1, Point3d p2, TypeofLine lt = TypeofLine.StraghtLine)
        {
            ObjectId createdLineId = new ObjectId();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = ed.Document.Database;
            Transaction trans = db.TransactionManager.StartTransaction();
            try
            {
                //TODO: create other new line types
                ObjectId ltId = ObjectId.Null;
                if (lt == TypeofLine.ImaginaryLine)
                {
                    LinetypeTable ltt =
                        (LinetypeTable)trans.GetObject(db.LinetypeTableId, OpenMode.ForWrite);

                    if (ltt.Has("ImaginaryLine"))
                        ltId = ltt["ImaginaryLine"];
                    else
                    {
                        //create new line type
                        LinetypeTableRecord ltr = new LinetypeTableRecord();

                        //line properties
                        ltr.AsciiDescription = "ImaginaryLineForPipe";
                        ltr.PatternLength = 1.0;
                        ltr.NumDashes = 2;
                        ltr.SetDashLengthAt(0, 0.5);
                        ltr.SetDashLengthAt(1, -0.5);
                        ltr.Name = "ImaginaryLine";
                        ltId = ltt.Add(ltr);
                        trans.AddNewlyCreatedDBObject(ltr, true);
                    }

                }

                Line l = new Line((Point3d)p1, (Point3d)p2);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                // now the circle to the current space, model space more than likely

                if (lt == TypeofLine.ImaginaryLine)
                    l.LinetypeId = ltId;

                createdLineId = btr.AppendEntity(l);
                // tell the transaction about the new circle so that it can autoclose it
                trans.AddNewlyCreatedDBObject(l, true);
                // now commit the transaction
                trans.Commit();
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("Problem:" + ex.Message);

            }
            finally
            {
                trans.Dispose();
            }
            return createdLineId;
        }

        public static ObjectId LineAttribute(TypeofLine lt)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = ed.Document.Database;
            Transaction trans = db.TransactionManager.StartTransaction();
            ObjectId ltId = ObjectId.Null;
            if (lt == TypeofLine.ImaginaryLine)
            {
                LinetypeTable ltt =
                    (LinetypeTable)trans.GetObject(db.LinetypeTableId, OpenMode.ForWrite);

                if (ltt.Has("ImaginaryLine"))
                    ltId = ltt["ImaginaryLine"];
                else
                {
                    //create new line type
                    LinetypeTableRecord ltr = new LinetypeTableRecord();

                    //line properties
                    ltr.AsciiDescription = "ImaginaryLineForPipe";
                    ltr.PatternLength = 1.0;
                    ltr.NumDashes = 2;
                    ltr.SetDashLengthAt(0, 0.5);
                    ltr.SetDashLengthAt(1, -0.5);
                    ltr.Name = "ImaginaryLine";
                    ltId = ltt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    trans.Commit();
                    trans.Dispose();
                }

            }
            return ltId;
        }

        // Group objects
        // <param name="idCol"></param>
        // <param name="groupName"></param>
        // <returns>id of group object</returns>
        public static ObjectId createGroup(ObjectIdCollection idCol, String groupName)
        {
            ObjectId groupId;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId,
                                    OpenMode.ForRead);
                Group group = new Group("my group", true);
                gd.UpgradeOpen();
                ObjectId grpId = gd.SetAt(groupName, group);
                tr.AddNewlyCreatedDBObject(group, true);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                btr.AssumeOwnershipOf(idCol);
                group.InsertAt(0, idCol);
                tr.Commit();
                tr.Dispose();
                groupId = grpId;
            }
            return groupId;
        }


        //str = "(d1, d2, d3)"
        public static Point3d ParsePoint3d(string str)
        {
            Point3d p = new Point3d();
            str = str.Substring(1, str.Length - 2);
            string[] strs = str.Split(',');
            double[] v = new double[3];
            for (int i = 0; i < 3; i++)
            {
                v[i] = double.Parse(strs[i]);
            }
            p = new Point3d(v[0], v[1], v[2]);
            return p;
        }


        // show all attributes of group object
        // just for debug
        // <param name="groupId">group id of object</param>
        public static void showAttr(ObjectId groupId)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
            try
            {
                Group ent = (Group)trans.GetObject(groupId, OpenMode.ForRead);
                // test the IsNull property of the ExtensionDictionary of the ent. 
                if (ent.ExtensionDictionary.IsNull)
                {
                    return;
                }
                // variable as DBDictionary. 
                DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                DBObject dbObj;
                foreach (System.Collections.DictionaryEntry dEntry in extensionDict)
                {
                    dbObj = (DBObject)trans.GetObject((ObjectId)dEntry.Value, OpenMode.ForRead, false);
                    if (dbObj is DBDictionary)
                    {

                    }
                    else if (dbObj is Xrecord)
                    {
                        foreach (TypedValue value in (Xrecord)dbObj)
                        {
                            ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                ed.WriteMessage("\n" + e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        // add attributes to group object
        // <param name="groupId"></param>
        // <param name="pairs"></param>
        public static void addData(ObjectId groupId, Dictionary<String, String> pairs)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
            try
            {
                Group ent = (Group)trans.GetObject(groupId, OpenMode.ForRead);
                if (ent.ExtensionDictionary.IsNull)
                {
                    ent.UpgradeOpen();
                    ent.CreateExtensionDictionary();
                }

                DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                foreach (KeyValuePair<string, string> item in pairs)
                {
                    String key = item.Key;
                    String value = item.Value;
                    Xrecord myXrecord;
                    if (extensionDict.Contains(key))
                    {
                        // Check to see if the entry we are going to add is already there. 
                        ObjectId entryId = extensionDict.GetAt(key);
                        // Extract the Xrecord. Declare an Xrecord variable. 
                        myXrecord = default(Xrecord);
                        extensionDict.UpgradeOpen();
                        // Instantiate the Xrecord variable 
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForWrite);
                        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, value));
                        myXrecord.Data = data;

                    }
                    else
                    {
                        // If the code gets to here then the data entry does not exist 
                        // upgrade the ExtensionDictionary created in step 5 to write
                        extensionDict.UpgradeOpen();
                        //  Create a new XRecord. 
                        myXrecord = new Xrecord();
                        // Create the resbuf list. 
                        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, value));
                        // Add the ResultBuffer to the Xrecord 
                        myXrecord.Data = data;
                        // Create the entry in the ExtensionDictionary. 
                        extensionDict.SetAt(key, myXrecord);
                        // Tell the transaction about the newly created Xrecord 
                        trans.AddNewlyCreatedDBObject(myXrecord, true);
                    }
                }
                trans.Commit();
            }
            catch (System.Exception ex)
            {
                // a problem occured, lets print it 
                ed.WriteMessage("a problem occured because " + ex.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        public static void RemoveLayer(ObjectId id)
        {
            //
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                using (Transaction trans = ed.Document.Database.TransactionManager.StartTransaction())
                {
                    DBObject ent = trans.GetObject(id, OpenMode.ForWrite);
                    ent.Erase();
                    trans.Commit();
                    trans.Dispose();
                }
            }
        }
        public static ObjectId CreateLayer(String name)
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    LayerTableRecord ltr = new LayerTableRecord();
                    // ... and set its properties
                    ltr.Name = name;
                    ltr.Color = Color.FromRgb(255, 0, 0);
                    //ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                    // Add the new layer to the layer table
                    lt.UpgradeOpen();
                    ObjectId ltId = lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                    db.Clayer = ltId;
                    // Commit the transaction
                    tr.Commit();
                    return ltId;
                }
            }
        }

        public static ObjectId ContainLayer(String name)
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                    SymbolUtilityServices.ValidateSymbolName(name, false);
                    if (lt.Has(name))
                    {
                        foreach (ObjectId layerId in lt)
                        {
                            LayerTableRecord layer = tr.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;

                            if (layer.Name.CompareTo(name) == 0)
                            {
                                return layerId;
                            }

                        }

                    }

                }
                return ObjectId.Null;

            }

        }

        /// <summary>
        /// 
        ///
        /// </summary>
        /// <param name="layerId">图层的ObjectID</param>
        /// <param name="isOpenOrClose">false:打开图层, true: 关闭图层</param>
        /// <param name="setClayer">true: 设置为可以编辑的图层</param>
        public static void OpenOrCloseLayer(ObjectId layerId, bool isOpenOrClose)
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                Transaction tr = db.TransactionManager.StartTransaction();
                try
                {
                    using (tr)
                    {
                        LayerTableRecord layer = tr.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
                        if (layer.IsOff != isOpenOrClose)
                        {
                            layer.UpgradeOpen();
                            layer.IsOff = isOpenOrClose;
                            if (layer.IsFrozen != isOpenOrClose)
                            {
                                layer.IsFrozen = isOpenOrClose;
                            }
                            tr.Commit();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("a problem occured because " + ex.Message);
                }
                finally
                {
                    tr.Dispose();
                }
            }

        }

        public static void ActiveOriginLayer()
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                Transaction tr = db.TransactionManager.StartTransaction();
                try
                {
                    using (tr)
                    {
                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        foreach (ObjectId objId in lt)
                        {
                            LayerTableRecord layer = tr.GetObject(objId, OpenMode.ForWrite) as LayerTableRecord;

                            if (layer.Name != "our_outline_layer" && !layer.Name.StartsWith("方案") && layer.IsErased == false)
                            {
                                db.Clayer = objId;
                                tr.Commit();
                                return;
                            }
                        }
                    }
                    //db.TransactionManager.QueueForGraphicsFlush();
                }
                catch (System.Exception e)
                {
                    ed.WriteMessage(e.Message);
                }
            }

        }

        public static void ResetCLayer(ObjectId layerId)
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                Transaction tr = db.TransactionManager.StartTransaction();
                try
                {
                    using (tr)
                    {
                        LayerTableRecord layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;

                        layer.IsOff = false;
                        if (layer.IsFrozen)
                        {
                            layer.IsFrozen = false;
                        }
                        db.Clayer = layerId;
                        tr.Commit();
                    }
                    db.TransactionManager.QueueForGraphicsFlush();
                }
                catch (System.Exception e)
                {
                    ed.WriteMessage(e.Message);
                }
                finally
                {
                    tr.Dispose();
                }
            }

        }

        public static void LoadMlineStyle()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
#if DEBUG
            string supportDir = HeatSourceLayoutApp.CurrentDirectory + "\\..\\..\\Resource";
#else
            string supportDir = HeatSourceLayoutApp.CurrentDirectory + "\\Resource";
#endif
            try
            {

                object acadObject = Application.AcadApplication;

                object preferences =
                        acadObject.GetType().InvokeMember("Preferences",
                        BindingFlags.GetProperty,
                        null, acadObject, null);

                //get the files
                object files =
                        preferences.GetType().InvokeMember("Files",
                        BindingFlags.GetProperty,
                        null, preferences, null);

                //get the support path SupportPath
                string supportPath =
                        (string)files.GetType().InvokeMember("SupportPath",
                        BindingFlags.GetProperty,
                        null, files, null);
                if (!supportPath.Contains(supportDir))
                {
                    supportPath = supportPath + ";" + supportDir;
                }

                object[] dataArry = new object[1];
                dataArry[0] = supportPath;
                files.GetType().InvokeMember("SupportPath",
                         BindingFlags.SetProperty,
                         null, files, dataArry);

                db.LoadMlineStyleFile("MLINEJIG", "mlinejig.mln");
                db.LoadMlineStyleFile("PIPELINE", "pipeline.mln");
                db.LoadMlineStyleFile("HIGHLIGHT", "highlight.mln");
                db.LoadMlineStyleFile("MLINEJIGW", "mlinejigw.mln");
                db.LoadMlineStyleFile("PIPELINEW", "pipelinew.mln");
                db.LoadMlineStyleFile("HIGHLIGHTW", "highlightw.mln");
            }
            catch (System.Exception e)
            {
                ed.WriteMessage(e.Message);
            }

        }

        public static void TestPoint(Point3d p)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                Circle c = new Circle(p, Vector3d.ZAxis, 0.4);
                c.Color = Color.FromRgb(0, 255, 255);

                btr.AppendEntity(c);
                tr.AddNewlyCreatedDBObject(c, true);
                tr.Commit();
            }
        }

        public static bool PointInPolygon(Point3d p, Polyline pline)
        {

            int i, j = pline.NumberOfVertices - 2;
            bool oddNodes = false;
            double x = p.X;
            double y = p.Y;


            for (i = 0; i < pline.NumberOfVertices - 1; i++)
            {
                if ((pline.GetPoint3dAt(i).Y < y && pline.GetPoint3dAt(j).Y >= y
                || pline.GetPoint3dAt(j).Y < y && pline.GetPoint3dAt(i).Y >= y)
               )
                {
                    if ((pline.GetPoint3dAt(i).X + (y - pline.GetPoint3dAt(i).Y) / (pline.GetPoint3dAt(j).Y - pline.GetPoint3dAt(i).Y) * (pline.GetPoint3dAt(j).X - pline.GetPoint3dAt(i).X) < x))
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }



        public static Point2d Point3DTo2D(Point3d p)
        {
            return new Point2d(p.X, p.Y);
        }

        public static bool Parallel(Point3d a, Point3d b, Point3d c, Point3d d)
        {
            Vector3d v1 = a - b;
            Vector3d v2 = d - c;
            Vector3d v3 = d - b;
            double CrossProduct = v1.X * v2.Y - v1.Y * v2.X;//cross value
            if (Math.Abs(CrossProduct) < 0.001)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断点在线段上
        /// </summary>
        /// <param name="a">线段的起点</param>
        /// <param name="b">线段的终点</param>
        /// <param name="c">测试点</param>
        /// <returns></returns>
        public static bool PointInSegment(Point3d a, Point3d b, Point3d c)
        {
            Vector3d v1 = c - a;
            Vector3d v2 = c - b;
            double CrossProduct = v1.X * v2.Y - v1.Y * v2.X;//cross value
            double InnerProducet = v1.DotProduct(v2);
            if (Math.Abs(CrossProduct) < 0.001 && InnerProducet <= 0)
            {
                return true;
            }
            return false;
        }

        public static bool SegmentsIntr(Point3d a, Point3d b, Point3d c, Point3d d, out Point3d res)
        {

            res = new Point3d();
            // 三角形abc 面积的2倍
            var area_abc = (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);

            // 三角形abd 面积的2倍
            var area_abd = (a.X - d.X) * (b.Y - d.Y) - (a.Y - d.Y) * (b.X - d.X);

            // 面积符号相同则两点在线段同侧,不相交 (对点在线段上的情况,本例当作不相交处理);
            if (area_abc * area_abd >= 0)
            {
                return false;
            }

            // 三角形cda 面积的2倍
            var area_cda = (c.X - a.X) * (d.X - a.X) - (c.Y - a.Y) * (d.X - a.X);
            // 三角形cdb 面积的2倍
            // 注意: 这里有一个小优化.不需要再用公式计算面积,而是通过已知的三个面积加减得出.
            var area_cdb = area_cda + area_abc - area_abd;
            if (area_cda * area_cdb >= 0)
            {
                return false;
            }

            //计算交点坐标
            var t = area_cda / (area_abd - area_abc);
            var dx = t * (b.X - a.X);
            var dy = t * (b.Y - a.Y);
            res = new Point3d(dx, dy, 0);
            return true;
        }

        public static Point3d CrossPoint(Point3d a, Point3d b, Point3d c, Point3d d)
        {
            double lambda, tao;
            Vector3d v1 = a - b;
            Vector3d v2 = d - c;
            Vector3d v3 = d - b;
            double CrossProduct = v1.X * v2.Y - v1.Y * v2.X;//cross value
            lambda = 1 / CrossProduct * (v2.Y * v3.X - v2.X * v3.Y);
            tao = 1 / CrossProduct * (-v1.Y * v3.X + v1.X * v3.Y);
            return b + lambda * (a - b);
        }

        public static Point3d Intersect(Point3d a, Vector3d va, Point3d b, Vector3d vb)
        {
            if (va.X == 0)
            {
                double x = a.X;
                double y = (x - b.X) / vb.X * vb.Y + b.Y;
                return new Point3d(x, y, 0);
            }
            else if (vb.X == 0)
            {
                double x = b.X;
                double y = (x - a.X) / va.X * va.Y + a.Y;
                return new Point3d(x, y, 0);
            }
            else
            {
                double k1 = va.Y / va.X;
                double k2 = vb.Y / vb.X;
                double x = (k1 * a.X - k2 * b.X + b.Y - a.Y) / (k1 - k2);
                double y = a.Y + (x - a.X) * k1;
                return new Point3d(x, y, 0);
            }
        }

        static int currentColorIndex = -1;
        static List<Color> colors = new List<Autodesk.AutoCAD.Colors.Color>(){
                Color.FromRgb(255, 0, 0),
                Color.FromRgb(255, 125, 0),
                Color.FromRgb(255, 255, 0),
                Color.FromRgb(0, 255, 0),
                Color.FromRgb(0, 0,255),
                Color.FromRgb(0,255,255),
                Color.FromRgb(255, 0, 255),
                Color.FromRgb(189, 189, 189)
        };

        public static Color GetNextColor()
        {
            currentColorIndex = (currentColorIndex + 1) % colors.Count;
            return colors[currentColorIndex];
        }

        public static void ClearApplicationConfig()
        {
            String folder = GetAcadLocation();
            String supportDir = folder + "\\Support";
            String lspfile = null;
            foreach (String filename in Directory.GetFileSystemEntries(supportDir))
            {
                if (filename.Contains("acad20"))
                {
                    lspfile = filename;
                    break;
                }
            }
            //clear lsp file
            List<string> lines = new List<string>();
            StreamReader sr = new StreamReader(lspfile, Encoding.Default);
            bool NeedDelete = false;
            String line;
            string scriptStart = ";;; HeatSource Start";
            string scriptEnd = ";;; HeatSource End";
            bool NeedClearFile = false;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(scriptStart))
                {
                    NeedDelete = true;
                    NeedClearFile = true;
                }
                if (!NeedDelete)
                {
                    lines.Add(line);
                }
                if (line.Contains(scriptEnd))
                {
                    NeedDelete = false;
                }
            }
            sr.Close();
            //rewrite the file
            if (NeedClearFile)
            {
                FileStream fs1 = new FileStream(lspfile, FileMode.Create);
                StreamWriter sw1 = new StreamWriter(fs1);
                foreach (var str in lines)
                {
                    sw1.WriteLine(str);
                }
                sw1.Flush();
                sw1.Close();
                fs1.Close();
            }
        }

        // Get the AutoCAD current version registry key
        public static string GetAcadCurVerKey()
        {
            RegistryKey rkcu = Registry.CurrentUser;
            string path = @"Software\Autodesk\AutoCAD\";
            using (RegistryKey rk1 = rkcu.OpenSubKey(path))
            {
                path += rk1.GetValue("CurVer");
                using (RegistryKey rk2 = rkcu.OpenSubKey(path))
                {
                    return path + "\\" + rk2.GetValue("CurVer");
                }
            }
        }

        // Get the acad.exe location for the AutoCAD current version
        public static string GetAcadLocation()
        {
            RegistryKey rklm = Registry.LocalMachine;
            string path = GetAcadCurVerKey();
            using (RegistryKey rk = rklm.OpenSubKey(path))
            {
                return (string)rk.GetValue("AcadLocation");
            }
        }

        //将double 转为两位浮点数
        public static double DoubleWithTwoDecimalPlaces(double x)
        {
            return Math.Truncate(x * 100) / 100;
        }

        public static void SetOrthoMode(bool mode)
        {
            if (mode)
            {
                Application.SetSystemVariable("ORTHOMODE", 1);
            }
            else
            {
                Application.SetSystemVariable("ORTHOMODE", 0);
            }
        }

        //创建building编号的文本样式
        public static ObjectId GetDefaultStyleId()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction newTransaction = doc.Database.TransactionManager.StartTransaction())
            {
                TextStyleTable newTextStyleTable = newTransaction.GetObject(doc.Database.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                if (!newTextStyleTable.Has("HEATSOURCE"))  //The TextStyle is currently not in the database
                {
                    newTextStyleTable.UpgradeOpen();
                    var newTextStyleTableRecord = new TextStyleTableRecord();
                    newTextStyleTableRecord.FileName = "Arial";
                    newTextStyleTableRecord.Name = "HEATSOURCE";
                    newTextStyleTable.Add(newTextStyleTableRecord);
                    newTransaction.AddNewlyCreatedDBObject(newTextStyleTableRecord, true);
                }
                newTransaction.Commit();
                return newTextStyleTable["HEATSOURCE"];
            }

        }

        public static void ImportBlocks()
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                DocumentCollection dm = Application.DocumentManager;
                Editor ed = dm.MdiActiveDocument.Editor;
                Database destDb = dm.MdiActiveDocument.Database;
                Database sourceDb = new Database(false, true);
                try
                {
                    // Read the DWG into a side database
#if DEBUG
                sourceDb.ReadDwgFile("../../88.dwg", System.IO.FileShare.Read, true,"");
#else
                    sourceDb.ReadDwgFile("./Resource/88.dwg", System.IO.FileShare.Read, true, "");
#endif
                    // Create a variable to store the list of block identifiers
                    ObjectIdCollection blockIds = new ObjectIdCollection();
                    Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = sourceDb.TransactionManager;
                    using (Transaction myT = tm.StartTransaction())
                    {
                        // Open the block table
                        BlockTable bt = (BlockTable)tm.GetObject(sourceDb.BlockTableId, OpenMode.ForRead, false);

                        // Check each block in the block table
                        foreach (ObjectId btrId in bt)
                        {
                            BlockTableRecord btr =
                              (BlockTableRecord)tm.GetObject(btrId,
                                                            OpenMode.ForRead,
                                                            false);
                            // Only add named & non-layout blocks to the copy list
                            if (!btr.IsAnonymous && !btr.IsLayout)
                                blockIds.Add(btrId);
                            btr.Dispose();
                        }
                    }
                    // Copy blocks from source to destination database
                    IdMapping mapping = new IdMapping();
                    sourceDb.WblockCloneObjects(blockIds, destDb.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage("\nError during copy: " + ex.Message);
                }
                sourceDb.Dispose();
            }
        }

        public static void makeImageBottom()
        {
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document activeDoc
                = Application.DocumentManager.MdiActiveDocument;
                Database db = activeDoc.Database;
                Editor ed = activeDoc.Editor;

                ObjectId oid = HeatSourceLayoutApp.globalProperty.RasterImageID;
                if (oid == ObjectId.Null)
                {
                    return;
                }

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(
                                                    db.BlockTableId,
                                                    OpenMode.ForRead
                                                ) as BlockTable;
                    BlockTableRecord btrModelSpace =
                            tr.GetObject(
                                            bt[BlockTableRecord.ModelSpace],
                                            OpenMode.ForRead
                                        ) as BlockTableRecord;

                    DrawOrderTable dot =
                            tr.GetObject(
                                            btrModelSpace.DrawOrderTableId,
                                            OpenMode.ForWrite
                                        ) as DrawOrderTable;

                    ObjectIdCollection objToMove = new ObjectIdCollection();
                    objToMove.Add(oid);
                    dot.MoveToBottom(objToMove);
                    tr.Commit();
                }
            }
        }

        public static ObjectId addStaticGoogleMap()
        {
            var dialog = new OpenFileDialog("选择文件存储位置", "燃气热源方案比选", "jpg;png;bmp;*", "SaveFile", OpenFileDialog.OpenFileDialogFlags.NoShellExtensions);
            //var dialog = new OpenFileDialog();

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return ObjectId.Null;
            }
            ObjectId val = ObjectId.Null;
            string imagePath = dialog.Filename;
            string szname = "import";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument();
            Autodesk.AutoCAD.ApplicationServices.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            using (Database db = doc.Database)
            {
                using (Transaction tm = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        ObjectId imageDictID = RasterImageDef.GetImageDictionary(db);
                        if (imageDictID == ObjectId.Null)
                        {
                            imageDictID = RasterImageDef.CreateImageDictionary(db);
                        }
                        RasterImageDef rasterImageDef = new RasterImageDef();
                        DBDictionary imageDict = (DBDictionary)tm.GetObject(imageDictID, OpenMode.ForWrite);
                        ObjectId rasterImageDefID;
                        if (imageDict.Contains(szname))
                        {
                            rasterImageDefID = imageDict.GetAt(szname);
                            rasterImageDef = (RasterImageDef)tm.GetObject(rasterImageDefID, OpenMode.ForWrite);
                        }
                        else
                        {
                            rasterImageDefID = imageDict.SetAt(szname, rasterImageDef);
                            rasterImageDef.SourceFileName = imagePath;
                            rasterImageDef.Load();
                            rasterImageDef.ImageModified = true;
                            rasterImageDef.ResolutionUnits = Unit.Millimeter;
              
                            tm.AddNewlyCreatedDBObject(rasterImageDef, true);
                        }
                        RasterImage rasterImage = new RasterImage();
                        RasterImage.EnableReactors(true);
                        rasterImage.ImageDefId = rasterImageDef.ObjectId;
                        LayerTable lt = (LayerTable)tm.GetObject(db.LayerTableId, OpenMode.ForRead);
                        foreach (ObjectId objId in lt)
                        {
                            LayerTableRecord layer = tm.GetObject(objId, OpenMode.ForWrite) as LayerTableRecord;

                            if (layer.Name != "our_outline_layer" && !layer.Name.StartsWith("方案") && layer.IsErased == false)
                            {
                                rasterImage.LayerId = objId;
                                break;
                            }
                        }
                        BlockTable bt = (BlockTable)tm.GetObject(doc.Database.BlockTableId, OpenMode.ForRead, false);
                        BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false);
                        val = btr.AppendEntity(rasterImage);
                        tm.AddNewlyCreatedDBObject(rasterImage, true);
                        tm.Commit();
                        tm.Dispose();
                    }
                    catch (System.Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }

                }
            }
            return val;
            
        }

        public static ObjectId GetObjectId(string handle)
        {
            try
            {
                ObjectId newObjectId = Application.DocumentManager.MdiActiveDocument.Database.GetObjectId(false, new Handle(Convert.ToInt64(handle, 16)), 0);
                return newObjectId;
            }
            catch(System.Exception e)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
            }
            return ObjectId.Null;
        }
    }
}
