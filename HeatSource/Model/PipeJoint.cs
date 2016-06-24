using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using HeatSource.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Model
{
    /// <summary>
    /// 管道接头的类
    /// </summary>
    public abstract class PipeJoint:BaseModel
    {
        private List<PipeLine> _ConnectedPipes;
        public Point3d CenterPoint;
        public ReadOnlyCollection<PipeLine> ConnectedPipes
        {
            get
            {
                return new ReadOnlyCollection<PipeLine>(_ConnectedPipes);
            }
        }
        public void SortConnectedPipeLine()
        {
            this._ConnectedPipes.Sort((first, second) =>
            {
                Point3d p1 = first.Point(0);
                Point3d p2 = second.Point(0);
                Vector3d v1 = (p1 - CenterPoint);
                Vector3d v2 = (p2 - CenterPoint);
                Vector3d n = new Vector3d(1, 0, 0);
                double cos1;
                double cos2;
                //这个函数需要测试
                cos1 = Math.Atan2(v1.Y, v1.X);
                cos2 = Math.Atan2(v2.Y, v2.X);
                return cos1.CompareTo(cos2);
            });
        }
        public Solution OwnSolution { set; get; }
        public PipeJoint(Solution s, bool needId):base(needId)
        {
            OwnSolution = s;
            _ConnectedPipes = new List<PipeLine>();
        }
        

        public void AddPipeLine(PipeLine line)
        {
            _ConnectedPipes.Add(line);
            this.Save();
        }

        public void SetPipeLines(List<PipeLine> lines)
        {
            _ConnectedPipes.Clear();
            _ConnectedPipes.AddRange(lines);
            this.Save();
        }

        public void RemovePipeLine(PipeLine line)
        {
            this._ConnectedPipes.Remove(line);
            this.Save();
        }

        public void RemoveEntity()
        {
            if (this is SimplePipeJoint)
            {
                OwnSolution.SimplePipeJoints.Remove(this.BaseObjectId);
            }
            else
            {
                OwnSolution.MultiPipeJoints.Remove(this.BaseObjectId);
            }

            //delete group
            using (Autodesk.AutoCAD.ApplicationServices.DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Group g = (Group)tr.GetObject(this.BaseObjectId, OpenMode.ForWrite);
                    foreach (var item in g.GetAllEntityIds())
                    {
                        tr.GetObject(item, OpenMode.ForWrite).Erase();
                    }
                    g.Erase();
                    tr.Commit();
                    tr.Dispose();
                }
            }
        }
       
    }
    //这种接口为只有两条管道相连
    public class SimplePipeJoint:PipeJoint
    {
        public static string modelType = "simplejoint";

        public  double jointRadius
        {
            get
            {
                return PipeLine.GetScale();
            }
        }
        public ObjectId BoundPolygon { set; get; }
        public List<ObjectId> SplitLines { set; get; }
        public ObjectId CenterArc { set; get; }

        public Solution.PipeLineStyle Style { set; get; }
        private Point3d p1;
        private Point3d p2;
        private Point3d p3;
        private Arc arc;
        private List<Line> lines;
        private Polyline polyline;

        protected override void GetAttributes()
        {
            attrs.Clear();
            attrs.Add("centerpoint", CenterPoint.ToString());
            string str = "";
            foreach(var item in ConnectedPipes)
            {
                str += ModelIdManager.toString(item.BaseModelId) + ",";
            }
            attrs.Add("connectedpipes", str);
            if (this.OwnSolution != null)
            {
                attrs.Add("solution", Utils.ModelIdManager.toString(this.OwnSolution.BaseModelId));
            }
            attrs.Add("style", ((int)Style).ToString());
        }

        protected override string ModelType()
        {
            return SimplePipeJoint.modelType;
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            if (attrs.ContainsKey("centerpoint"))
            {
                CenterPoint = Utility.ParsePoint3d(attrs["centerpoint"]);
            }
            if(attrs.ContainsKey("connectedpipes"))
            {
                string[] strs = attrs["connectedpipes"].Split(',');
                List<PipeLine> lines = new List<PipeLine>(); 
                for(int i = 0; i < strs.Count() - 1; i ++)
                {
                    int modelId = ModelIdManager.Parse(strs[i]);
                    lines.Add((PipeLine)BaseModel.GetModelById(modelId));
                }
                this.SetPipeLines(lines);
            }
            if (attrs.ContainsKey("solution"))
            {
                this.OwnSolution = (Solution)BaseModel.GetModelById(Utils.ModelIdManager.Parse(attrs["solution"]));
                if (this.OwnSolution != null)
                {
                    this.OwnSolution.SimplePipeJoints.Add(this.BaseObjectId, this);
                }
            }
            if(attrs.ContainsKey("style"))
            {
                this.Style = (Solution.PipeLineStyle)int.Parse(attrs["style"]);
            }
        }
        public List<Point3d> GetConnectedPipePoints(int index)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (Transaction trans = ed.Document.Database.TransactionManager.StartTransaction())
            {
                Mline ent = (Mline)trans.GetObject(this.ConnectedPipes[index].BaseObjectId, OpenMode.ForRead);
                List<Point3d> v = new List<Point3d>();
                v.Add(ent.VertexAt(0));
                v.Add(ent.VertexAt(1));
                v.Sort(new ComparePoint(CenterPoint));
                return v;
            }
        }

        public void Retrive()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Group g = (Group)tr.GetObject(this.BaseObjectId, OpenMode.ForRead);
                ObjectId[] ids = g.GetAllEntityIds();
                foreach (var item in ids)
                {
                    DBObject obj = tr.GetObject(item, OpenMode.ForRead);
                    if (obj is Polyline)
                    {
                        this.BoundPolygon = item;
                        this.polyline = obj as Polyline;
                    }
                    else if (obj is Line)
                    {
                        this.SplitLines.Add(item);
                    }
                    else if (obj is Arc)
                    {
                        this.CenterArc = item;
                    }
                }
                tr.Dispose();
            }
        }
        public bool IsSimpleJointPart(ObjectId id)
        {
            if(CenterArc == id)
            {
                return true;
            }
            if(BoundPolygon == id)
            {
                return true;
            }
            foreach(var item in SplitLines)
            {
                if(item == id)
                {
                    return true;
                }
            }
            return false;
        }

        public PipeLine GetConnectedPipeLine(int index)
        {
            ObjectId id = this.ConnectedPipes[index].BaseObjectId;
            if(OwnSolution.PipeLines.ContainsKey(id))
            {
                return OwnSolution.PipeLines[id];
            }
            return null;
        }

         class ComparePoint:IComparer<Point3d>
        {
             private Point3d RefPoint;
             public ComparePoint(Point3d refPoint)
             {
                 RefPoint = refPoint;
             }
            public int Compare(Point3d p1, Point3d p2)
            {
                double v1 = (p1 - RefPoint).Length;
                double v2 = (p2 - RefPoint).Length;
                return v1.CompareTo(v2);
            }

        }

        public bool PointInJoint(Point3d p)
        {
            //return Utils.Utility.InsidePolygon(polyline, p);
             return Utils.Utility.PointInPolygon(p, polyline);
        }

        /// <summary>
        /// 进行restore时候的构造函数
        /// </summary>
        /// <param name="sol"></param>
        public SimplePipeJoint()
            : base(null, false)
        {
            this.SplitLines = new List<ObjectId>();
        }


        public SimplePipeJoint(Point3d p1, Point3d p2, Point3d p3, double pipeWidth, Solution sol, bool needId):base(sol, needId)
        {
            this.Style = sol.CurrentLineStyle;
            this.SplitLines = new List<ObjectId>();
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            CenterPoint = p2;
            Vector3d d1 = (p2 - p1).GetNormal();
            Vector3d d2 = (p3 - p2).GetNormal();
            Vector3d d3 = (d2 - d1).GetNormal(); //角平分线
            double cos1 = d3.DotProduct(-d1);
            double sin1 = Math.Sqrt(1 - cos1 * cos1);
            double R1 = jointRadius / sin1;
            Point3d center = p2 + R1 * d3;
            Point3d arcStartPoint = p2 - R1 * cos1 * d1;
            Point3d arcEndPoint = p2 + R1 * cos1 * d2;
            Point3d arcCenterPoint = center - d3 * jointRadius;
            CircularArc3d c = new CircularArc3d(arcStartPoint, arcCenterPoint, arcEndPoint);
            double angle1 = c.ReferenceVector.AngleOnPlane(new Plane(c.Center, c.Normal));
            arc = new Arc();
            arc.Center = c.Center;
            arc.Radius = c.Radius;
            arc.Normal = c.Normal;
            arc.StartAngle = c.StartAngle + angle1; 
            arc.EndAngle = c.EndAngle + angle1;
            double angle = d2.GetAngleTo(d1);
            if(d1.CrossProduct(d2).Z < 0)
            {
                angle = -angle;
            }
            int seg = 3;
            if(Math.Abs(angle) < Math.PI / 2)
            {
                seg = 3;
            }
            else if(Math.Abs(angle) < Math.PI / 4 * 3)
            {
                seg = 4;
            }
            else
            {
                seg = 5;
            }
            polyline = new Polyline();
            double ShortR = jointRadius - pipeWidth;
            double WideR = jointRadius + pipeWidth;
            Vector3d d4 = (arcStartPoint - center).GetNormal();
            Point3d p4 = center + d4 * ShortR;
            Point3d p5 = center + d4 * WideR;
            lines = new List<Line>();
            for(int i = 0; i < seg - 1; i ++)
            {
                Point3d p6;
                Point3d p7;
                if(i == 0)
                {
                    d4 = d4.RotateBy(angle / (seg - 1) / 2, Vector3d.ZAxis);
                }
                else
                {
                    d4 = d4.RotateBy(angle / (seg - 1), Vector3d.ZAxis);

                }
                p6 = center + d4 * ShortR;
                p7 = center + d4 * WideR;
                Line l = new Line(p6, p7);
                lines.Add(l);
            }

            Vector3d d5 = (arcEndPoint - center).GetNormal();
            Point3d p8 = center + d5 * ShortR;
            Point3d p9 = center + d5 * WideR;
            polyline.AddVertexAt(0, new Point2d(p4.X, p4.Y), 0, 0, 0);
            for(int i = 0; i < lines.Count; i ++)
            {
                Point3d p = lines[i].StartPoint;
                polyline.AddVertexAt(i + 1, new Point2d(p.X, p.Y), 0, 0, 0);
            }
            polyline.AddVertexAt(lines.Count + 1, new Point2d(p8.X, p8.Y), 0, 0, 0);
            polyline.AddVertexAt(lines.Count + 2, new Point2d(p9.X, p9.Y), 0, 0, 0);
            for(int i = lines.Count - 1; i >= 0; i --)
            {
                Point3d p = lines[i].EndPoint;
                polyline.AddVertexAt(lines.Count + 2 + lines.Count - i, new Point2d(p.X, p.Y), 0, 0, 0);
            }
            polyline.AddVertexAt(2 * lines.Count + 3, new Point2d(p5.X, p5.Y), 0, 0, 0);
            polyline.AddVertexAt(2 * lines.Count + 4, new Point2d(p4.X, p4.Y), 0, 0, 0);
            //save all objects
            this.SaveEntities();
        }

        public void SaveEntities()
        {
            ObjectIdCollection col = new ObjectIdCollection();
            Database db = HostApplicationServices.WorkingDatabase;
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                this.CenterArc = btr.AppendEntity(arc);
                if (Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    arc.Color = Color.FromRgb(0, 0, 255);
                }
                else
                {
                    arc.Color = Color.FromRgb(255, 255, 0);
                }
                tr.AddNewlyCreatedDBObject(arc, true);
                this.BoundPolygon = btr.AppendEntity(polyline);
                if (Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    polyline.Color = Color.FromRgb(0, 255, 255);
                }
                else
                {
                    polyline.Color = Color.FromRgb(255, 0, 0);
                }
                tr.AddNewlyCreatedDBObject(polyline, true);
                this.SplitLines = new List<ObjectId>();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                    {
                        lines[i].Color = Color.FromRgb(0, 0, 255);
                    }
                    else
                    {
                        lines[i].Color = Color.FromRgb(255, 255, 0);
                    }
                    this.SplitLines.Add(btr.AppendEntity(lines[i]));
                    tr.AddNewlyCreatedDBObject(lines[i], true);
                }
                col.Add(this.BoundPolygon);
                col.Add(this.CenterArc);
                foreach(var item in this.SplitLines)
                {
                    col.Add(item);
                }
                tr.Commit();
            }
            this.BaseObjectId = Utils.Utility.createGroup(col, ModelIdManager.toString(this.BaseModelId));
            OwnSolution.SimplePipeJoints.Add(this.BaseObjectId, this);
            this.Save();            
        }

        public Point3d CalculateLastMlineEndPoint()
        {
            Vector3d d1 = (p2 - p1).GetNormal();
            Vector3d d2 = (p3 - p2).GetNormal();
            Vector3d d3 = (d2 - d1).GetNormal(); //角平分线
            double cos1 = d3.DotProduct(-d1);
            double sin1 = Math.Sqrt(1 - cos1 * cos1);
            double R1 = jointRadius / sin1;
            Point3d center = p2 + R1 * d3;
            return p2 - R1 * cos1 * d1;
        }
        public Point3d CalculateMlineStartPoint()
        {
            Vector3d d1 = (p2 - p1).GetNormal();
            Vector3d d2 = (p3 - p2).GetNormal();
            Vector3d d3 = (d2 - d1).GetNormal(); //角平分线
            double cos1 = d3.DotProduct(-d1);
            double sin1 = Math.Sqrt(1 - cos1 * cos1);
            double R1 = jointRadius / sin1;
            Point3d center = p2 + R1 * d3;
            return p2 + R1 * cos1 * d2;
        }
    }
   
    //这种接口为可以有多条管道相连
    public class MultiPipeJoint: PipeJoint
    {
        public static string modelType = "multijoint";
        public Solution.PipeLineStyle Style { set; get; }
        public double Length {
            get
            {
                return PipeLine.GetScale() * 2;
            }
        }
        public ObjectId BoundPolygon { set; get; }
        public List<ObjectId> CenterLines { set; get; }
        private Polyline pline = new Polyline();
        private List<Line> lines = new List<Line>();
        public MultiPipeJoint(Solution sol, bool needId):base(sol, needId)
        {
            Style = sol.CurrentLineStyle;
            CenterLines = new List<ObjectId>();
        }

        public MultiPipeJoint() : base(null, false) { CenterLines = new List<ObjectId>(); }
        public bool IsMultiJointPart(ObjectId id)
        {
            if(BoundPolygon == id)
            {
                return true;
            }
            foreach(var item in CenterLines)
            {
                if(item == id)
                {
                    return true;
                }
            }
            return false;
        }



        public List<Point3d> GenerateFromCrossPipe(PipeLine line, Point3d sPoint, Point3d ePoint, double width)
        {
            List<Point3d> res = new List<Point3d>();
            //计算与mline的交点
            Point3d sm = line.Point(0);
            Point3d em = line.Point(1);
            Vector3d v1 = (em - sm).GetNormal();
            Vector3d n1 = v1.RotateBy(0.5 * Math.PI, Vector3d.ZAxis);
            Point3d p1 = sm + line.Width * n1;
            Point3d p2 = em + line.Width * n1;
            Point3d p3 = sm - line.Width * n1;
            Point3d p4 = em - line.Width * n1;

            Point3d centerPoint = Utils.Utility.CrossPoint(sm, em, sPoint, ePoint);
            Vector3d v2 = (ePoint - sPoint).GetNormal();
            double len1 = line.Width / Math.Abs(v2.DotProduct(n1));
            Point3d p5 = centerPoint + v2 * len1;
            double cos1 = v2.DotProduct(v1);
            double sin1 = Math.Sqrt(1 - cos1 * cos1);
            Point3d p6 = p5 - v1 * (width / sin1);
            Point3d p7 = p5 + v1 * (width / sin1);
            int sign;
            if (v2.CrossProduct(v1).Z > 0)
            {
                sign = 1;
            }
            else
                sign = -1;
            Point3d p8 = (centerPoint - Length * v1) - n1 * width * sign;
            Point3d p9 = (centerPoint + Length * v1) - n1 * width * sign;
            
            Point3d e1 = centerPoint - Length * v1;
            Point3d e2 = centerPoint + Length * v1;
            Point3d p10 = e1 + (e1 - p8);
            Point3d p11 = e2 + (e2 - p9);
            
            Point3d p13 = centerPoint + Length * v2;
            
            Vector3d n2 = v2.RotateBy(sign * 0.5 * Math.PI, Vector3d.ZAxis);
            Point3d p14 = p13 - n2 * width;
            Point3d p15 = p13 + n2 * width;

            pline.AddVertexAt(0, Utils.Utility.Point3DTo2D(p8), 0, 0, 0);
            pline.AddVertexAt(1, Utils.Utility.Point3DTo2D(p6), 0, 0, 0);
            pline.AddVertexAt(2, Utils.Utility.Point3DTo2D(p14), 0, 0, 0);
            pline.AddVertexAt(3, Utils.Utility.Point3DTo2D(p15), 0, 0, 0);
            pline.AddVertexAt(4, Utils.Utility.Point3DTo2D(p7), 0, 0, 0);
            pline.AddVertexAt(5, Utils.Utility.Point3DTo2D(p9), 0, 0, 0);
            pline.AddVertexAt(6, Utils.Utility.Point3DTo2D(p11), 0, 0, 0);
            pline.AddVertexAt(7, Utils.Utility.Point3DTo2D(p10), 0, 0, 0);
            pline.AddVertexAt(8, Utils.Utility.Point3DTo2D(p8), 0, 0, 0);

            Line l1 = new Line(e1, centerPoint);
            Line l2 = new Line(e2, centerPoint);
            Line l3 = new Line(p13, centerPoint);

            lines.Add(l1);
            lines.Add(l2);
            lines.Add(l3);

            SaveEntity();

            res.Add(e1);
            res.Add(e2);
            res.Add(p13);
            CenterPoint = centerPoint;
            return res;
            
        }

        

        public PipeLine GenerateFromSimpleJoint(SimplePipeJoint pipeJoint,  Point3d ePoint, double width)
        {
            //
            
            this.CenterPoint = pipeJoint.CenterPoint;
            PipeLine pipeline1 = pipeJoint.GetConnectedPipeLine(0);
            PipeLine pipeline2 = pipeJoint.GetConnectedPipeLine(1);
            Point3d p1 = CenterPoint + (pipeline1.Point(0) - CenterPoint).GetNormal() * Length;
            Point3d p2 = CenterPoint + (pipeline2.Point(0) - CenterPoint).GetNormal() * Length;
            Point3d p3 = CenterPoint + (ePoint - CenterPoint).GetNormal() * Length;
            if(pipeline1.HeadConnectedObject == pipeJoint)
            {
                pipeline1.HeadConnectedObject = this;
                pipeline1.UpdateStartPoint(p1);

            }
            if(pipeline1.TailConnectedObject == pipeJoint)
            {
                pipeline1.TailConnectedObject = this;
                pipeline1.UpdateEndPoint(p1);
            }
            if (pipeline2.HeadConnectedObject == pipeJoint)
            {
                pipeline2.HeadConnectedObject = this;
                pipeline2.UpdateStartPoint(p2);
            }
            if (pipeline2.TailConnectedObject == pipeJoint)
            {
                pipeline2.TailConnectedObject = this;
                pipeline2.UpdateEndPoint(p2);
            }
            PipeLine pipeline3 = new PipeLine(p3, ePoint, false, OwnSolution, true);
            List<PipeLine> lists = new List<PipeLine>();
            lists.Add(pipeline1);
            lists.Add(pipeline2);
            lists.Add(pipeline3);
            this.SetPipeLines(lists);
            this.GenerateFromConnectedPipes();
            pipeJoint.RemoveEntity();
            return pipeline3;
        }


        



        public PipeLine UpdateMultiJoint(Point3d ePoint)
        {
            Vector3d v = (ePoint - CenterPoint).GetNormal();
            Point3d sp = CenterPoint + Length * v;
            PipeLine pipeLine = new PipeLine(sp, ePoint, false, OwnSolution);
            this.AddPipeLine(pipeLine);
            this.GenerateFromConnectedPipes();
            return pipeLine;
        }

        public void SaveEntity()
        {
            ObjectIdCollection col = new ObjectIdCollection();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                if (Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                {
                    pline.Color = Color.FromRgb(0, 255, 255);
                }
                else
                {
                    pline.Color = Color.FromRgb(255, 0, 0);
                }
                this.BoundPolygon = btr.AppendEntity(pline);
                tr.AddNewlyCreatedDBObject(pline, true);
                
                for (int i = 0; i < lines.Count; i++)
                {
                    if (Style == Solution.PipeLineStyle.AnyConnectedBuilding)
                    {
                        lines[i].Color = Color.FromRgb(0, 0, 255);
                    }
                    else
                    {
                        lines[i].Color = Color.FromRgb(255, 255, 0);
                    }
                    this.CenterLines.Add(btr.AppendEntity(lines[i]));
                    tr.AddNewlyCreatedDBObject(lines[i], true);
                }
                tr.Commit();
                tr.Dispose();
            }
            col.Add(this.BoundPolygon);
            foreach(var item in this.CenterLines)
            {
                col.Add(item);
            }
            this.BaseObjectId = Utility.createGroup(col, ModelIdManager.toString(this.BaseModelId));
            OwnSolution.MultiPipeJoints.Add(this.BaseObjectId, this);
            this.Save();
        }

        public void Retrive()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Group g = (Group) tr.GetObject(this.BaseObjectId, OpenMode.ForRead);
                ObjectId [] ids = g.GetAllEntityIds();
                foreach (var item in ids)
                {
                    DBObject obj = tr.GetObject(item, OpenMode.ForRead);
                    if (obj is Polyline)
                    {
                        this.BoundPolygon = item;
                        
                    }
                    else if (obj is Line)
                    {
                        this.CenterLines.Add(item);
                    }
                }
                tr.Dispose();
            }
        }



        protected override string ModelType()
        {
            return MultiPipeJoint.modelType;
        }

        protected override void GetAttributes()
        {
            attrs.Clear();
            attrs.Add("centerpoint", CenterPoint.ToString());
            string str = "";
            foreach (var item in ConnectedPipes)
            {
                str += ModelIdManager.toString(item.BaseModelId) + ",";
            }
            attrs.Add("connectedpipes", str);
            if (this.OwnSolution != null)
            {
                attrs.Add("solution", Utils.ModelIdManager.toString(this.OwnSolution.BaseModelId));
            }
            attrs.Add("style", ((int)Style).ToString());
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            if (attrs.ContainsKey("centerpoint"))
            {
                CenterPoint = Utility.ParsePoint3d(attrs["centerpoint"]);
            }
            if (attrs.ContainsKey("connectedpipes"))
            {
                string[] strs = attrs["connectedpipes"].Split(',');
                List<PipeLine> lines = new List<PipeLine>();
                for (int i = 0; i < strs.Count() - 1; i++)
                {
                    int modelId = ModelIdManager.Parse(strs[i]);
                    lines.Add((PipeLine)BaseModel.GetModelById(modelId));
                }
                this.SetPipeLines(lines);
            }
            if (attrs.ContainsKey("solution"))
            {
                this.OwnSolution = (Solution)BaseModel.GetModelById(Utils.ModelIdManager.Parse(attrs["solution"]));
                if (this.OwnSolution != null)
                {
                    this.OwnSolution.MultiPipeJoints.Add(this.BaseObjectId, this);
                }
            }
            if(attrs.ContainsKey("style"))
            {
                Style = (Solution.PipeLineStyle)int.Parse(attrs["style"]);
            }
        }

        public BaseModel DownGrade(PipeLine line)
        {
            //
            if(this.ConnectedPipes.Contains(line))
            {
                this.RemovePipeLine(line);
                if(this.ConnectedPipes.Count == 2)
                {
                    //退化成SimpleJoint 或者 PipeLine
                    PipeLine line1 = this.ConnectedPipes[0];
                    PipeLine line2 = this.ConnectedPipes[1];
                    //退化成新的PipeLine
                    if(line1.PipeLineCrossTest(line2) == PipeLine.CrossType.ParallelNoConnect)
                    {
                        Point3d p1, p2;
                        PipeJoint co1, co2;
                        if((line1.Point(0) - this.CenterPoint).Length > (line1.Point(1) - this.CenterPoint).Length)
                        {
                            p1 = line1.Point(0);
                            co1 = line1.HeadConnectedObject;
                        }
                        else
                        {
                            p1 = line1.Point(1);
                            co1 = line1.TailConnectedObject;
                        }
                        if((line2.Point(0) - this.CenterPoint).Length > (line2.Point(1) - this.CenterPoint).Length)
                        {
                            p2 = line2.Point(0);
                            co2 = line2.HeadConnectedObject;
                        }
                        else
                        {
                            p2 = line2.Point(1);
                            co2 = line2.TailConnectedObject;
                        }
                        PipeLine pLine = new PipeLine(p1, p2, OwnSolution, true);
                        line1.Delete();
                        line2.Delete();
                        pLine.HeadConnectedObject = co1;
                        if(co1 != null)
                        {
                            co1.RemovePipeLine(line1);
                            co1.AddPipeLine(pLine);
                        }
                        pLine.TailConnectedObject = co2;
                        if (co2 != null)
                        {
                            co2.RemovePipeLine(line2);
                            co2.AddPipeLine(pLine);
                        }
                        this.RemoveEntity();
                    }
                    else
                    {
                        Point3d p1, p2, p3, p4;
                        bool ht1, ht2; // head or tail, true for head, false for tail
                        if ((line1.Point(0) - this.CenterPoint).Length > (line1.Point(1) - this.CenterPoint).Length)
                        {
                            p1 = line1.Point(0);
                            p3 = line1.Point(1);
                            ht1 = false;
                        }
                        else
                        {
                            p1 = line1.Point(1);
                            p3 = line1.Point(0);
                            ht1 = true;
                        }
                        if ((line2.Point(0) - this.CenterPoint).Length > (line2.Point(1) - this.CenterPoint).Length)
                        {
                            p2 = line2.Point(0);
                            p4 = line2.Point(1);
                            ht2 = false;
                        }
                        else
                        {
                            p2 = line2.Point(1);
                            p4 = line2.Point(0);
                            ht2 = true;
                        }
                        Point3d intersect = Utility.Intersect(p1, p3 - p1, p2, p4 - p2);
                        SimplePipeJoint simplePipeJoint = new SimplePipeJoint(p1, intersect, p2, PipeLine.GetScale() / 2, OwnSolution, true);
                        Point3d p5 = simplePipeJoint.CalculateLastMlineEndPoint();
                        Point3d p6 = simplePipeJoint.CalculateMlineStartPoint();
                        if(ht1)
                        {
                            line1.UpdateStartPoint(p5);
                            line1.HeadConnectedObject = simplePipeJoint;
                        }
                        else
                        {
                            line1.UpdateEndPoint(p5);
                            line1.TailConnectedObject = simplePipeJoint;
                        }
                        if(ht2)
                        {
                            line2.UpdateStartPoint(p6);
                            line2.HeadConnectedObject = simplePipeJoint;
                        }
                        else
                        {
                            line2.UpdateEndPoint(p6);
                            line2.TailConnectedObject = simplePipeJoint;
                        }
                        simplePipeJoint.AddPipeLine(line1);
                        simplePipeJoint.AddPipeLine(line2);
                        this.RemoveEntity();
                    }
                }
                else
                {
                    this.GenerateFromConnectedPipes();
                }
            }
            return null;
        }

        public void GenerateFromConnectedPipes()
        {
            this.SortConnectedPipeLine();
            int n = this.ConnectedPipes.Count;
            List<Vector3d> ds = new List<Vector3d>();
            List<Point3d> ePoints = new List<Point3d>(); // 线段截断的两点
            List<Point3d> cPoints = new List<Point3d>();
            Polyline newPolyline = new Polyline();
            List<Line> newLines = new List<Line>();
            foreach (var item in this.ConnectedPipes)
            {
                Vector3d v = (item.Point(0) - this.CenterPoint).GetNormal();
                ds.Add(v);
                Vector3d vn = v.RotateBy(0.5 * Math.PI, Vector3d.ZAxis);
                Point3d vp = this.CenterPoint + v * Length;
                cPoints.Add(vp);
                Point3d pl = vp - PipeLine.GetScale() / 2 * vn;
                Point3d pr = vp + PipeLine.GetScale() / 2 * vn;
                ePoints.Add(pl);
                ePoints.Add(pr);
            }
            int index = 0;
            for(int i = 0; i < n; i ++)
            {
                int j = (i + 1) % n;
                newPolyline.AddVertexAt(index++, Utility.Point3DTo2D(ePoints[i * 2]), 0, 0, 0);
                newPolyline.AddVertexAt(index++, Utility.Point3DTo2D(ePoints[i * 2 + 1]), 0, 0, 0);
                if(!Utility.Parallel(this.CenterPoint, cPoints[i], this.CenterPoint, cPoints[j]))
                {
                    Point3d intersect = Utility.Intersect(ePoints[i * 2 + 1], ds[i], ePoints[j * 2], ds[j]);
                    newPolyline.AddVertexAt(index++, Utility.Point3DTo2D(intersect), 0, 0, 0);
                }
            }
            //闭合
            newPolyline.AddVertexAt(index++, newPolyline.GetPoint2dAt(0), 0, 0, 0);
            //
            foreach(var p in cPoints)
            {
                Line l = new Line();
                l.StartPoint = this.CenterPoint;
                l.EndPoint = p;
                newLines.Add(l);
            }
            if(this.BaseObjectId != ObjectId.Null)
            {
                this.RemoveEntity();
            }
            this.pline = newPolyline;
            this.lines = newLines;
            this.SaveEntity();
        }
        public bool PointInJoint(Point3d p)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Polyline line = (Polyline)tr.GetObject(this.BoundPolygon, OpenMode.ForRead, false);
                return Utils.Utility.PointInPolygon(p, line);
            }
        }

    }
}
