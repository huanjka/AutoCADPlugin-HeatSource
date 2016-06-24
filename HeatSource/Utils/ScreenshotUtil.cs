using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Utils
{
    /// <summary>
    /// 该类提供了创建截图的方法
    /// </summary>
    class ScreenshotUtil
    {
        //
        public static Bitmap CreateScreenShot(Rectangle rect)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return null;
            var ed = doc.Editor;
            Point2d screenSize = (Point2d)Application.GetSystemVariable("SCREENSIZE");
            using (var bmp = doc.CapturePreviewImage(Convert.ToUInt32(screenSize.X), Convert.ToUInt32(screenSize.Y)))
            {
                Bitmap target = bmp.Clone(rect, bmp.PixelFormat);
                return target;    
            }
        }

        //
        public static Bitmap CreateScreenShot()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return null;
            var ed = doc.Editor;
            Point2d screenSize = (Point2d)Application.GetSystemVariable("SCREENSIZE");
            using (var bmp = doc.CapturePreviewImage(Convert.ToUInt32(screenSize.X), Convert.ToUInt32(screenSize.Y)))
            {
                Rectangle t = new Rectangle();
                t.X = 0;
                t.Y = 0;
                t.Width = (int)screenSize.X;
                t.Height = (int)screenSize.Y;
                Bitmap target = bmp.Clone(t, bmp.PixelFormat);
                return target;
            }
        }

        //获得截图区域
        public static bool SelectCropArea(out Rectangle rect)
        {
            rect = new Rectangle();
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;

            var ed = doc.Editor;
            PromptPointResult ppr = ed.GetPoint("\n 选择区域的一角");
            if (ppr.Status != PromptStatus.OK)
                return false;
            Point3d first = ppr.Value;
            ppr = ed.GetCorner("\n 选择区域的另一角", first);
            if (ppr.Status != PromptStatus.OK)
                return false;
            Point3d second = ppr.Value;
            System.Windows.Point pt1, pt2;
            short vp = (short)Application.GetSystemVariable("CVPORT");
            IntPtr hWnd = doc.Window.Handle;
            pt1 = ScreenFromDrawingPoint(ed, first, vp);
            pt2 = ScreenFromDrawingPoint(ed, second, vp);
            rect.X = (int)Math.Min(pt1.X, pt2.X);
            rect.Y = (int)Math.Min(pt1.Y, pt2.Y);
            rect.Width = (int)Math.Abs(pt2.X - pt1.X);
            rect.Height = (int)Math.Abs(pt2.Y - pt1.Y);
            if(rect.Width == 0 || rect.Height == 0)
            {
                System.Windows.MessageBox.Show("您的截图区域面积为0，请重新生成文档", "Error");
                return false;
            }
            return true;
        }

        //坐标系转换
        private static System.Windows.Point ScreenFromDrawingPoint(Editor ed, Point3d ucsPt, short viewportNum)
        {
            Point3d wcsPt = ucsPt.TransformBy(ed.CurrentUserCoordinateSystem);
            System.Windows.Point res = ed.PointToScreen(wcsPt, viewportNum);
            return res;
        }
    }
}
