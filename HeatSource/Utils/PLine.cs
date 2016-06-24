using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;


namespace HeatSource.Utils
{
    public class MyPlineCmds
    {
        class PlineJig : EntityJig
        {
            // Maintain a list of vertices...
            // Not strictly necessary, as these will be stored in the
            // polyline, but will not adversely impact performance
            Point3dCollection m_pts;
            // Use a separate variable for the most recent point...
            // Again, not strictly necessary, but easier to reference
            Point3d m_tempPoint;
            Plane m_plane;

            public PlineJig(Matrix3d ucs)
              : base(new Polyline())
            {
                // Create a point collection to store our vertices
                m_pts = new Point3dCollection();

                // Create a temporary plane, to help with calcs
                Point3d origin = new Point3d(0, 0, 0);
                Vector3d normal = new Vector3d(0, 0, 1);
                normal = normal.TransformBy(ucs);
                m_plane = new Plane(origin, normal);

                // Create polyline, set defaults, add dummy vertex
                Polyline pline = Entity as Polyline;
                pline.SetDatabaseDefaults();
                pline.Normal = normal;
                pline.ConstantWidth = HeatSource.HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                pline.Closed = true;
                pline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                JigPromptPointOptions jigOpts =
                  new JigPromptPointOptions();
                jigOpts.UserInputControls =
                  (UserInputControls.Accept3dCoordinates |
                  UserInputControls.NullResponseAccepted |
                  UserInputControls.NoNegativeResponseAccepted
                  );

                if (m_pts.Count == 0)
                {
                    // For the first vertex, just ask for the point
                    jigOpts.Message =
                      "\nStart point of polyline: ";
                }
                else if (m_pts.Count > 0)
                {
                    // For subsequent vertices, use a base point
                    jigOpts.BasePoint = m_pts[m_pts.Count - 1];
                    jigOpts.UseBasePoint = true;
                    jigOpts.Message = "\nPolyline vertex: ";
                }
                else // should never happen
                    return SamplerStatus.Cancel;

                // Get the point itself
                PromptPointResult res =
                  prompts.AcquirePoint(jigOpts);

                // Check if it has changed or not
                // (reduces flicker)
                if (m_tempPoint == res.Value)
                {
                    return SamplerStatus.NoChange;
                }
                else if (res.Status == PromptStatus.OK)
                {
                    m_tempPoint = res.Value;
                    return SamplerStatus.OK;
                }
                return SamplerStatus.Cancel;
            }

            protected override bool Update()
            {
                // Update the dummy vertex to be our
                // 3D point projected onto our plane
                Polyline pline = Entity as Polyline;
                
                pline.ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                pline.SetPointAt(
                  pline.NumberOfVertices - 1,
                  m_tempPoint.Convert2d(m_plane)
                );
                
                return true;
            }

            public Entity GetEntity()
            {
                return Entity;
            }

            public void AddLatestVertex()
            {
                // Add the latest selected point to
                // our internal list...
                // This point will already be in the
                // most recently added pline vertex
                m_pts.Add(m_tempPoint);
                Polyline pline = Entity as Polyline;
                pline.ConstantWidth = HeatSourceLayoutApp.globalProperty.BuildingOutlineWidth;
                // Create a new dummy vertex...
                // can have any initial value
                pline.AddVertexAt(
                  pline.NumberOfVertices,
                  new Point2d(0, 0),
                  0, 0, 0
                );
            }

            public void RemoveLastVertex()
            {
                // Let's remove our dummy vertex
                Polyline pline = Entity as Polyline;
                if (m_pts.Count != 0)
                {
                    pline.RemoveVertexAt(m_pts.Count);
                }
            }
        }
       
        public static ObjectId MyPolyJig(out bool status)
        {
            using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                ObjectId oid = ObjectId.Null;
                Document doc =
                  Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;

                // Get the current UCS, to pass to the Jig
                Matrix3d ucs =
                  ed.CurrentUserCoordinateSystem;

                // Create our Jig object
                PlineJig jig = new PlineJig(ucs);

                // Loop to set the vertices directly on the polyline
                bool bSuccess = true, bComplete = false;
                do
                {
                    PromptResult res = ed.Drag(jig);
                    bSuccess =
                      (res.Status == PromptStatus.OK);
                    // A new point was added
                    if (bSuccess)
                        jig.AddLatestVertex();
                    // Null input terminates the command
                    bComplete =
                      (res.Status == PromptStatus.None);
                    if (bComplete)
                        // Let's clean-up the polyline before adding it
                        jig.RemoveLastVertex();
                } while (bSuccess && !bComplete);

                // If the jig completed successfully, add the polyline
                if (bComplete)
                {
                    if(((Polyline)jig.GetEntity()).NumberOfVertices <= 2)
                    {
                        status = true;
                        return ObjectId.Null;
                    }
                    // Append entity
                    Database db = doc.Database;
                    Transaction tr =
                      db.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        BlockTable bt =
                          (BlockTable)tr.GetObject(
                            db.BlockTableId,
                            OpenMode.ForRead,
                            false
                          );
                        BlockTableRecord btr =
                          (BlockTableRecord)tr.GetObject(
                            bt[BlockTableRecord.ModelSpace],
                            OpenMode.ForWrite,
                            false
                          );
                        oid = btr.AppendEntity(jig.GetEntity());
                        tr.AddNewlyCreatedDBObject(jig.GetEntity(), true);
                        tr.Commit();
                    }
                    status = true;
                }
                else
                {
                    status = false;
                }
                return oid;
            }
        }
    }
}