using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace HeatSource.Model
{
    public class PipeLine : BaseModel
    {
        public static string modelType = "pipeline";
        public enum CrossType
        {
            CrossNone, //不相交
            CrossHead, //被测试管道的头部和管道的内部相交
            CrossTail, //被测试管道的尾部和管道的内部相交，以上两个状态要创建复杂结头
            CrossMid,  //未使用，暂时保留
            CrossOver, //被测试管道跨交管道  
            ParallelHead, //被测试管道与管道平行且和管道头部相交，
            ParallelTail, //被测试管道与管道平行且和管道尾部相交，以上两个状态要将两条管道合并
            ParallelNoConnect, //平行但不相连
            NonParallelHead,   //与管道头部相交
            NonParallelTail,   //与管道尾部相交，以上两个状态要创建简单结头
        }

        private PipeJoint _headObject;
        private PipeJoint _tailObject;
        public Solution.PipeLineStyle Style { set; get; }
        //管道左端连接的对象
        public PipeJoint HeadConnectedObject
        {
            set
            {
                _headObject = value;
                if (this.BaseObjectId != ObjectId.Null)
                {
                    this.Save();
                }
            }
            get
            {

                return _headObject;

            }
        }
        //管道右端连接的对象
        public PipeJoint TailConnectedObject
        {
            set
            {
                _tailObject = value;
                if (this.BaseObjectId != ObjectId.Null)
                {
                    this.Save();
                }
            }
            get
            {
                return _tailObject;
            }
        }
        public double Width { set; get; }

        private Mline mline;
        Solution OwnSolution { set; get; }
        //在计算最长主管道的时候避免遇到环
        public bool IsVisit { set; get; }
        protected override void GetAttributes()
        {
            attrs.Clear();
            if (this.HeadConnectedObject != null)
            {
                attrs.Add("headobject", Utils.ModelIdManager.toString(this.HeadConnectedObject.BaseModelId));
            }
            if (this.TailConnectedObject != null)
            {
                attrs.Add("tailobject", Utils.ModelIdManager.toString(this.TailConnectedObject.BaseModelId));
            }
            if (this.OwnSolution != null)
            {
                attrs.Add("solution", Utils.ModelIdManager.toString(this.OwnSolution.BaseModelId));
            }
            attrs.Add("style", ((int)this.Style).ToString());
        }

        protected override string ModelType()
        {
            return PipeLine.modelType;
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            if (attrs.ContainsKey("headobject"))
            {
                int modelId = Utils.ModelIdManager.Parse(attrs["headobject"]);
                this.HeadConnectedObject = (PipeJoint)BaseModel.GetModelById(modelId);
            }
            if (attrs.ContainsKey("tailobject"))
            {
                int modelId = Utils.ModelIdManager.Parse(attrs["tailobject"]);
                this.TailConnectedObject = (PipeJoint)BaseModel.GetModelById(modelId);
            }
            if (attrs.ContainsKey("solution"))
            {
                this.OwnSolution = (Solution)BaseModel.GetModelById(Utils.ModelIdManager.Parse(attrs["solution"]));
                if (this.OwnSolution != null)
                {
                    this.OwnSolution.PipeLines.Add(this.BaseObjectId, this);
                }
            }
            if(attrs.ContainsKey("style"))
            {
                this.Style = (Solution.PipeLineStyle)(int.Parse(attrs["style"]));
            }
        }

        public PipeLine(Point3d p1, Point3d p2, Solution sol, bool needId):base(needId)
        {
            this.BaseObjectId = ObjectId.Null;
            this.OwnSolution = sol;
            this.HeadConnectedObject = null;
            this.TailConnectedObject = null;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                mline = new Mline();
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                this.Style = OwnSolution.CurrentLineStyle;
                if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    mline.Style = mlineStyleDic.GetAt("PIPELINE");
                }
                else
                {
                    mline.Style = mlineStyleDic.GetAt("PIPELINEW");
                }
                mline.Normal = Vector3d.ZAxis;
                mline.Scale = GetLineScale();
                this.Width = GetLineScale() / 2;
                mline.AppendSegment(p1);
                mline.AppendSegment(p2);
                this.BaseObjectId = btr.AppendEntity(mline);
                tr.AddNewlyCreatedDBObject(mline, true);
                tr.Commit();
                OwnSolution.PipeLines.Add(this.BaseObjectId, this);
            }
        }

        /// <summary>
        /// Save并没有使用
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="Save">不保存</param>
        /// <param name="sol"></param>
        public PipeLine(Point3d p1, Point3d p2, bool NoSave, Solution sol):base(true)
        {
            this.OwnSolution = sol;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                mline = new Mline();
                DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                this.Style = OwnSolution.CurrentLineStyle;
                if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    mline.Style = mlineStyleDic.GetAt("PIPELINE");
                }
                else
                {
                    mline.Style = mlineStyleDic.GetAt("PIPELINEW");
                }
                mline.Normal = Vector3d.ZAxis;
                mline.Scale = GetLineScale();
                this.Width = GetLineScale() / 2;
                mline.AppendSegment(p1);
                mline.AppendSegment(p2);

            }
        }

        public PipeLine(Point3d p1, Point3d p2, bool NoSave, Solution sol, bool needId)
            : base(needId)
        {
            this.OwnSolution = sol;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                mline = new Mline();
                DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                this.Style = OwnSolution.CurrentLineStyle;
                if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    mline.Style = mlineStyleDic.GetAt("PIPELINE");
                }
                else
                {
                    mline.Style = mlineStyleDic.GetAt("PIPELINEW");
                }
                mline.Normal = Vector3d.ZAxis;
                mline.Scale = GetLineScale();
                this.Width = GetLineScale() / 2;
                mline.AppendSegment(p1);
                mline.AppendSegment(p2);

            }
        }

        // 该构造函数是在进行恢复时的构造函数
        public PipeLine()
            : base(false)
        {
            //this.Style = OwnSolution.CurrentLineStyle;
        }

        public void SaveEntity()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                this.BaseObjectId = btr.AppendEntity(mline);
                tr.AddNewlyCreatedDBObject(mline, true);
                tr.Commit();
                OwnSolution.PipeLines.Add(this.BaseObjectId, this);
            }
            this.Save();
        }

        public void Highlight()
        {

            Database db = HostApplicationServices.WorkingDatabase;
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Mline newmline = new Mline();
                        BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                        if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                        {
                            newmline.Style = mlineStyleDic.GetAt("HIGHLIGHT");
                        }
                        else
                        {
                            newmline.Style = mlineStyleDic.GetAt("HIGHLIGHTW");
                        }
                        newmline.Normal = Vector3d.ZAxis;
                        newmline.Scale = GetLineScale();
                        this.Width = GetLineScale() / 2;
                        newmline.AppendSegment(this.Point(0));
                        newmline.AppendSegment(this.Point(1));
                        this.Delete();
                        this.BaseObjectId = btr.AppendEntity(newmline);
                        tr.AddNewlyCreatedDBObject(newmline, true);
                        tr.Commit();

                        mline = newmline;
                        OwnSolution.PipeLines.Add(this.BaseObjectId, this);
                        this.Save();
                    }
                    catch (Exception e)
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
                    }
                }
            }

        }

        public void CancelHighlight()
        {

            Database db = HostApplicationServices.WorkingDatabase;
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {


                        BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                        if (mline.Style == mlineStyleDic.GetAt("HIGHLIGHT") || mline.Style == mlineStyleDic.GetAt("HIGHLIGHTW"))
                        {
                            Mline newmline = new Mline();
                            if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                            {
                                newmline.Style = mlineStyleDic.GetAt("PIPELINE");
                            }
                            else
                            {
                                newmline.Style = mlineStyleDic.GetAt("PIPELINEW");
                            }
                            newmline.Normal = Vector3d.ZAxis;
                            newmline.Scale = GetLineScale();
                            this.Width = GetLineScale() / 2;
                            newmline.AppendSegment(this.Point(0));
                            newmline.AppendSegment(this.Point(1));
                            this.Delete();
                            this.BaseObjectId = btr.AppendEntity(newmline);
                            tr.AddNewlyCreatedDBObject(newmline, true);
                            tr.Commit();
                            mline = newmline;
                            OwnSolution.PipeLines.Add(this.BaseObjectId, this);
                            this.Save();
                        }
                       
                    }
                    catch (Exception e)
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
                    }
                }
            }

        }



        public void UpdateEndPoint(Point3d endPoint)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                //mline 在修改后界面上不会自动更新，不知道是不是一个bug，所以在这里我们将线段先删除再插入
                Mline lastLine = (Mline)tr.GetObject(BaseObjectId, OpenMode.ForWrite, false);
                HeatSource.HeatSourceLayoutApp.currentSolution.PipeLines.Remove(BaseObjectId);
                lastLine.Erase();
                Mline newLine = new Mline();
                if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    newLine.Style = mlineStyleDic.GetAt("PIPELINE");
                }
                else
                {
                    newLine.Style = mlineStyleDic.GetAt("PIPELINEW");
                }
                newLine.Normal = Vector3d.ZAxis;
                newLine.Scale = GetLineScale();
                newLine.AppendSegment(lastLine.VertexAt(0));
                newLine.AppendSegment(endPoint);
                //lastLine.RemoveLastSegment(endPoint);
                BaseObjectId = btr.AppendEntity(newLine);
                tr.AddNewlyCreatedDBObject(newLine, true);
                tr.Commit();
                mline = newLine;
                HeatSource.HeatSourceLayoutApp.currentSolution.PipeLines.Add(BaseObjectId, this);
                this.Save();
            }
        }

        public void UpdateStartPoint(Point3d startPoint)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                //mline 在修改后界面上不会自动更新，不知道是不是一个bug，所以在这里我们将线段先删除再插入
                Mline lastLine = (Mline)tr.GetObject(BaseObjectId, OpenMode.ForWrite, false);
                HeatSource.HeatSourceLayoutApp.currentSolution.PipeLines.Remove(BaseObjectId);
                lastLine.Erase();
                Mline newLine = new Mline();
                if (this.Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    newLine.Style = mlineStyleDic.GetAt("PIPELINE");
                }
                else
                {
                    newLine.Style = mlineStyleDic.GetAt("PIPELINEW");
                }
                newLine.Normal = Vector3d.ZAxis;
                newLine.Scale = GetLineScale();
                newLine.AppendSegment(startPoint);
                newLine.AppendSegment(lastLine.VertexAt(1));
                //lastLine.RemoveLastSegment(endPoint);
                BaseObjectId = btr.AppendEntity(newLine);
                tr.AddNewlyCreatedDBObject(newLine, true);
                tr.Commit();
                mline = newLine;
                HeatSource.HeatSourceLayoutApp.currentSolution.PipeLines.Add(BaseObjectId, this);
                this.Save();
            }
        }
        public CrossType PointInPipe(Point3d point)
        {
            Polyline p = new Polyline();
            Point3d s = mline.VertexAt(0);
            Point3d e = mline.VertexAt(1);
            Vector3d u = (e - s).GetNormal();
            Vector3d n = u.RotateBy(0.5 * Math.PI, Vector3d.ZAxis);
            Point3d p1 = s + n * this.Width;
            Point3d p2 = e + n * this.Width;
            Point3d p3 = e - n * this.Width;
            Point3d p4 = s - n * this.Width;

            p.AddVertexAt(0, Utils.Utility.Point3DTo2D(p1), 0, 0, 0);
            p.AddVertexAt(1, Utils.Utility.Point3DTo2D(p2), 0, 0, 0);
            p.AddVertexAt(2, Utils.Utility.Point3DTo2D(p3), 0, 0, 0);
            p.AddVertexAt(3, Utils.Utility.Point3DTo2D(p4), 0, 0, 0);
            p.AddVertexAt(4, Utils.Utility.Point3DTo2D(p1), 0, 0, 0);

            bool isInner = Utils.Utility.PointInPolygon(point, p);
            if (!isInner)
            {
                return CrossType.CrossNone;
            }
            else
            {
                double v1 = Math.Abs((point - s).DotProduct(u));
                if (v1 < Width)
                {
                    return CrossType.CrossNone;
                }
                double v2 = Math.Abs((e - point).DotProduct(u));
                if (v2 < Width)
                {
                    return CrossType.CrossNone;
                }
                return CrossType.CrossMid;
            }
        }

        public CrossType PipeLineCrossTest(PipeLine line)
        {
            if(this.Style != line.Style)
            {
                return CrossType.CrossNone;
            }
            Point3d s = mline.VertexAt(0);
            Point3d e = mline.VertexAt(1);
            Point3d s1 = line.mline.VertexAt(0);
            Point3d e1 = line.mline.VertexAt(1);
            //
            Vector3d v1 = s - e;
            Vector3d v2 = e1 - s1;
            Vector3d v3 = e1 - e;

            //Inner product
            double cos1 = Math.Abs(v1.DotProduct(v2)) / (v1.Length * v2.Length);
            if (cos1 > 0.999)
            {
                //
                if (((s - e1).Length < 0.5 * GetLineScale() && (e - s1).Length > 2 * GetLineScale() || (s - s1).Length < 0.5 * GetLineScale() && (e - e1).Length > 2 * GetLineScale()) && this.HeadConnectedObject == null)
                {
                    return CrossType.ParallelHead;
                }
                else if (((e - s1).Length < 0.5 * GetLineScale() && (e - e1).Length > 2 * GetLineScale() || (e - e1).Length < 0.5 * GetLineScale() && (e - s1).Length > 2 * GetLineScale()) && this.TailConnectedObject  == null)
                {
                    return CrossType.ParallelTail;
                }
                return CrossType.ParallelNoConnect;
            }

            double CrossProduct = v1.X * v2.Y - v1.Y * v2.X;//cross value
            double lambda = 1 / CrossProduct * (v2.Y * v3.X - v2.X * v3.Y);
            double tao = 1 / CrossProduct * (-v1.Y * v3.X + v1.X * v3.Y);
            //接近与端点的地方也认为是不相交
            if (lambda <= 1 && ((lambda - 1) * v1).Length < GetLineScale() && this.HeadConnectedObject == null)
            {
                return CrossType.NonParallelHead;
            }
            else if (lambda >= 0 && (lambda * v1).Length < GetLineScale() && this.TailConnectedObject == null)
            {
                return CrossType.NonParallelTail;
            }
            if (v1.Length * lambda < GetLineScale() || v1.Length * (lambda - 1) > GetLineScale())
            {
                return CrossType.CrossNone;
            }
            else if ((lambda >= 0 && lambda <= 1 && (lambda * v1).Length > 2 * GetLineScale()) || (lambda >= 0 && lambda <= 1 && ((lambda - 1) * v1).Length > 2 * GetLineScale()))
            {
                //相交位置为管道附近
                if (tao * v2.Length > -0.5 * GetLineScale() && tao * v2.Length < 0.5 * GetLineScale())
                {

                    return CrossType.CrossTail;
                }
                else if (tao * v2.Length > v2.Length - 0.5 * GetLineScale() && tao * v2.Length < v2.Length + 0.5 * GetLineScale())
                {

                    return CrossType.CrossHead;
                }
                else if (tao > 0 && tao < 1)
                {
                    return CrossType.CrossOver;
                }
            }
            //
            return CrossType.CrossNone;

        }


        public Point3d Point(int index)
        {
            return mline.VertexAt(index);
        }

        public static double GetScale()
        {
            if (HeatSourceLayoutApp.currentSolution.CurrentLineStyle == Solution.PipeLineStyle.HeatSourceConnectHeatProducer)
            {
                return 2.0 * HeatSourceLayoutApp.globalProperty.PipeLineScale * 2;
            }
            else
            {
                return 2.0 * HeatSourceLayoutApp.globalProperty.PipeLineScale;
            }
        }

        public double GetLineScale()
        {
            if(Style == Solution.PipeLineStyle.AnyConnectedBuilding)
            {
                return 2.0 * HeatSourceLayoutApp.globalProperty.PipeLineScale;
            }
            else
            {
                return 2.0 * HeatSourceLayoutApp.globalProperty.PipeLineScale * 2;
            }
        }

        public double CalculateLength()
        {
            double res = 0;
            if(BaseObjectId == ObjectId.Null)
            {
                return 0;
            }
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                Mline line = (Mline)tr.GetObject(BaseObjectId, OpenMode.ForRead, false);
                Point3d s = mline.VertexAt(0);
                Point3d e = mline.VertexAt(1);
                Vector3d v = e - s;
                res = v.Length * HeatSourceLayoutApp.globalProperty.GraphScale;
                tr.Dispose();
            }
            return res;
        }

        public void RemoveJoint(PipeJoint joint)
        {
            if (this.HeadConnectedObject == joint)
            {
                this.HeadConnectedObject = null;
            }
            if (this.TailConnectedObject == joint)
            {
                this.TailConnectedObject = null;
            }
        }

        public void Delete()
        {
            OwnSolution.PipeLines.Remove(this.BaseObjectId);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction trans = ed.Document.Database.TransactionManager.StartTransaction())
                {
                    DBObject ent = trans.GetObject(BaseObjectId, OpenMode.ForWrite);
                    ent.Erase();
                    trans.Commit();
                    trans.Dispose();
                }
            }
        }

        public void RetriveMline()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                this.mline = (Mline) tr.GetObject(BaseObjectId, OpenMode.ForRead, false);
                tr.Dispose();
            }
        }


    }
}
