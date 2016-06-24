/* Include the ObjectARX inc file's dlls 
 * 绘制楼房，ESC键自动闭合
 */
/**
* this is shit coding.!!!!!!!@!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
* it has bug but I haven't find it!
*/
using System;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.GraphicsInterface;


using MgdAcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;
using HeatSource.Model;
using Autodesk.AutoCAD.Colors;

namespace HeatSource.Utils
{
    public class PolyLineJig : DrawJig
    {
        #region Fields

        private Point3dCollection mAllVertexes = new Point3dCollection();
        private Point3d mLastVertex = new Point3d();

        #endregion

        #region Constructors

        public PolyLineJig()
        {
        }

        ~PolyLineJig()
        {
        }

        #endregion

        #region Properties

        public Point3d LastVertex
        {
            get { return mLastVertex; }
            set { mLastVertex = value; }
        }

        private Editor Editor
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        public Matrix3d UCS
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            }
        }

        #endregion

        #region Overrides

        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                Point3dCollection tempPts = new Point3dCollection();
                foreach (Point3d pt in mAllVertexes)
                {
                    tempPts.Add(pt);
                }
                
                tempPts.Add(mLastVertex);
                if (tempPts.Count > 0)
                {
                    try
                    {
                        Database db = HostApplicationServices.WorkingDatabase;
                        Autodesk.AutoCAD.DatabaseServices.Polyline line = new Autodesk.AutoCAD.DatabaseServices.Polyline();
                        for (int i = 0; i < tempPts.Count; i++)
                        {
                            Point3d pt3d = tempPts[i];
                            Point2d pt2d = new Point2d(pt3d.X, pt3d.Y);
                            line.AddVertexAt(i, pt2d, 0, db.Plinewid, db.Plinewid);
                        }
                        Point2d final = new Point2d(tempPts[0].X, tempPts[0].Y);
                        line.AddVertexAt(tempPts.Count, final, 0, db.Plinewid, db.Plinewid);
                        line.ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                        line.Color = Color.FromRgb(255, 0, 0);
                        draw.Geometry.Draw(line);
                    }
                    catch(System.Exception e)
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
                    }
                    }
            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\n 选择下一点");
            prOptions1.UseBasePoint = false;
            prOptions1.UserInputControls =
                UserInputControls.NullResponseAccepted | UserInputControls.Accept3dCoordinates |
                UserInputControls.GovernedByUCSDetect | UserInputControls.GovernedByOrthoMode |
                UserInputControls.AcceptMouseUpAsPoint;

            PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
            if (prResult1.Status == PromptStatus.Cancel || prResult1.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;
            Point3d tempPt = prResult1.Value.TransformBy(UCS.Inverse());
            mLastVertex = tempPt;
            return SamplerStatus.OK;
        }

        #endregion

        #region Commands

        public static PolyLineJig jigger;

        public static ObjectId DrawPolyLine(out bool status)
        {
            status = false;
            using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                ObjectId res = ObjectId.Null;
                try
                {
                    Database db = HostApplicationServices.WorkingDatabase;
                    jigger = new PolyLineJig();
                    PromptResult jigRes;

                    //最初版本，勿删
                    do
                    {
                        jigRes = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.Drag(jigger);
                        if (jigger.mAllVertexes.Count > 0 && (jigger.mAllVertexes[jigger.mAllVertexes.Count - 1] - jigger.mLastVertex).Length == 0)
                        {

                        }
                        else if(jigRes.Status == PromptStatus.OK)
                        {
                            jigger.mAllVertexes.Add(jigger.mLastVertex);
                        }
                    } while (jigRes.Status == PromptStatus.OK);
                    
                    if(jigRes.Status == PromptStatus.None)
                    {
                        status = true;
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                            Autodesk.AutoCAD.DatabaseServices.Polyline ent = new Autodesk.AutoCAD.DatabaseServices.Polyline();
                            //ent.SetDatabaseDefaults();
                            if (jigger.mAllVertexes.Count > 2)
                            {
                                for (int i = 0; i < jigger.mAllVertexes.Count; i++)
                                {
                                    Point3d pt3d = jigger.mAllVertexes[i];
                                    Point2d pt2d = new Point2d(pt3d.X, pt3d.Y);
                                    ent.AddVertexAt(i, pt2d, 0, db.Plinewid, db.Plinewid);

                                }
                                    int final = jigger.mAllVertexes.Count;
                                    Point3d mPt3d = jigger.mAllVertexes[0];
                                    Point2d mPt2d = new Point2d(mPt3d.X, mPt3d.Y);
                                    ent.AddVertexAt(final, mPt2d, 0, db.Plinewid, db.Plinewid);
                                //ent.TransformBy(jigger.UCS);
                                ent.ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                                res = btr.AppendEntity(ent);
                                tr.AddNewlyCreatedDBObject(ent, true);
                                tr.Commit();
                            }
                        }
                    }
                    else
                    {
                        status = false;
                    }
                }
                catch (System.Exception ex)
                {
                    MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
                }
                return res;
            }
        }
        #endregion

    }
}
