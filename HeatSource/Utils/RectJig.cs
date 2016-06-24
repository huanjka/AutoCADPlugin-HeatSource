using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeatSource.Utils;

namespace HeatSource.Utils
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.GraphicsInterface;
    using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;


    namespace UseDrawJig
    {
        class RecJig : DrawJig
        {

            public Polyline m_PolyLine2;

            private Point3d m_Pt1, m_Pt2;

            public RecJig(Point3d pt1, Polyline polyline2)
            {
                m_Pt1 = pt1;
                m_PolyLine2 = polyline2;
                m_PolyLine2.ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
            }

            protected override bool WorldDraw(WorldDraw draw)
            {
                draw.Geometry.Draw(m_PolyLine2);
                return true;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                Matrix3d mt = ed.CurrentUserCoordinateSystem;

                JigPromptPointOptions optJigPoint = new JigPromptPointOptions
                    ("\n请指定矩形的另一角点");
                optJigPoint.UserInputControls =
                    UserInputControls.Accept3dCoordinates
                    | UserInputControls.NoZeroResponseAccepted
                    | UserInputControls.NoNegativeResponseAccepted;


                optJigPoint.BasePoint = m_Pt1.TransformBy(mt);
                optJigPoint.UseBasePoint = true;

                PromptPointResult resJigPoint = prompts.AcquirePoint(optJigPoint);
                Point3d tempPt = resJigPoint.Value;

                if (resJigPoint.Status == PromptStatus.Cancel)
                {
                    return SamplerStatus.Cancel;
                }

                if (m_Pt2 != tempPt)
                {
                    m_Pt2 = tempPt;
                    Point3d ucsPt2 = m_Pt2.TransformBy(mt.Inverse());
                    try
                    {
                        m_PolyLine2.Normal = Vector3d.ZAxis;
                        m_PolyLine2.Elevation = 0;
                        m_PolyLine2.SetPointAt(0, new Point2d(m_Pt1.X, m_Pt1.Y));
                        m_PolyLine2.SetPointAt(1, new Point2d(ucsPt2.X, m_Pt1.Y));
                        m_PolyLine2.SetPointAt(2, new Point2d(ucsPt2.X, ucsPt2.Y));
                        m_PolyLine2.SetPointAt(3, new Point2d(m_Pt1.X, ucsPt2.Y));
                        m_PolyLine2.TransformBy(mt);
                    }
                    catch { }
                    
                    return SamplerStatus.OK;
                }
                else
                {
                    return SamplerStatus.NoChange;
                }
            }


        }
    }




}
