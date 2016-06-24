using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Windows;
using HeatSource.Model;
namespace HeatSource.Utils
{
    class PipeJig : DrawJig
    {
        private Mline mline;
        public Point3d? startPoint;
        public Point3d endPoint;

        public PipeLine GeneratedPipeLine { set; get; }
        
        
        public PipeJig(Point3d? _startPoint)
        {
                this.startPoint = _startPoint;
        }

         
        
        protected override Autodesk.AutoCAD.EditorInput.SamplerStatus Sampler(Autodesk.AutoCAD.EditorInput.JigPrompts prompts)
        {
               
            JigPromptPointOptions jigOpts = new JigPromptPointOptions();
            jigOpts.UserInputControls = UserInputControls.GovernedByOrthoMode |  UserInputControls.NullResponseAccepted;
            jigOpts.UseBasePoint = true;
            jigOpts.BasePoint = (Point3d)this.startPoint;
            jigOpts.Message = "\n 选择下一点或者Enter结束";
            PromptPointResult res = prompts.AcquirePoint(jigOpts);
            Point3d positionTemp = res.Value;
            this.endPoint = positionTemp;
          
            if (res.Status == PromptStatus.Cancel || res.Status == PromptStatus.None)
                return SamplerStatus.Cancel;
            else
                return SamplerStatus.OK;  
        }

        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            try
            {
                Update();
                if(mline != null)
                {
                    draw.Geometry.Draw(mline);
                }
            }
            catch(System.Exception ex)
            {
                ex.ToString();
                return false;
            }
            return true;
        }

        private void Update()
        {
            mline.MoveVertexAt(1, this.endPoint);
        }

        private void AppendEntity()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                Update();
                tr.Commit();
            }  

        }

        public bool StartDraw(out bool status)
        {
            status = false;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            if (this.startPoint == null)
            {
                
                PromptPointOptions opt = new PromptPointOptions("\n 选择管道的初始点");
                PromptPointResult res = ed.GetPoint(opt);
                if(res.Status == PromptStatus.Cancel)
                {
                    status = true;
                }
                if (res.Status != PromptStatus.OK)
                {
                    return false;
                }
                this.startPoint = res.Value;
            }
            mline = new Mline();
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary mlineStyleDic = (DBDictionary)tr.GetObject(db.MLStyleDictionaryId, OpenMode.ForRead);
                mline.Style = mlineStyleDic.GetAt("MLINEJIG");
            }
            mline.Normal = Vector3d.ZAxis;
            mline.Scale = PipeLine.GetScale();
            mline.AppendSegment((Point3d)this.startPoint);
            mline.AppendSegment((Point3d)this.startPoint);
            PromptResult res2 = ed.Drag(this);
            if (res2.Status == PromptStatus.OK)
            {
                return true;
            }
            else if (res2.Status == PromptStatus.Cancel || res2.Status == PromptStatus.None)
            {
                if(res2.Status == PromptStatus.Cancel)
                {
                    status = true;
                }
                return false;
            }
            return true;
        }

        
    }
}
