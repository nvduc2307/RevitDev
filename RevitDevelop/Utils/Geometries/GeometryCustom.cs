using Autodesk.Revit.UI;
using RevitDevelop.Utils;
using RevitDevelop.Utils.Compares;

namespace RevitDevelop.Utils.Geometries
{
    public static class GeometryCustom
    {
        public static double Distance(this XYZ p, Line l)
        {
            var d = p.Distance(l.GetEndPoint(0));
            var dir = l.Direction;
            var vt = (l.GetEndPoint(0) - p).Normalize();
            if (dir.DotProduct(vt).IsAlmostEqual(0)) return p.Distance(l.GetEndPoint(0));
            if (Math.Abs(dir.DotProduct(vt)).IsAlmostEqual(1)) return 0;

            var angle = dir.DotProduct(vt) > 0
                ? vt.AngleTo(dir)
                : vt.AngleTo(-dir);
            d = Math.Sin(angle) * d;
            return d;
        }
        public static double Distance(this XYZ p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
        }
        public static double Distance(this XYZ p1, XYZ p2)
        {
            var x = p1.X - p2.X;
            var y = p1.Y - p2.Y;
            var z = p1.Z - p2.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public static XYZ VectorNormal(this XYZ vt)
        {
            return vt / vt.Distance();
        }
        public static XYZ MidPoint(this XYZ p1, XYZ p2)
        {
            var x = (p1.X + p2.X) * 0.5;
            var y = (p1.Y + p2.Y) * 0.5;
            var z = (p1.Z + p2.Z) * 0.5;
            return new XYZ(x, y, z);
        }
        public static double AngleTo(this XYZ vt1, XYZ vt2)
        {
            var cos = vt1.DotProduct(vt2) / (vt1.Distance() * vt2.Distance());
            return Math.Acos(cos);
        }
        public static XYZ PointToLine(this XYZ p, Line l)
        {
            var dir = (l.GetEndPoint(1) - l.GetEndPoint(0)).Normalize();
            var d = p.Distance(l.GetEndPoint(0));
            var vt = (l.GetEndPoint(0) - p).Normalize();
            if (dir.DotProduct(vt).IsAlmostEqual(0)) return l.GetEndPoint(0);
            if (Math.Abs(dir.DotProduct(vt)).IsAlmostEqual(1)) return p;

            var normal = dir.CrossProduct(vt);

            var vti = dir.CrossProduct(normal).DotProduct(vt) > 0
                ? dir.CrossProduct(normal).Normalize()
                : -dir.CrossProduct(normal).Normalize();

            var angle = dir.DotProduct(vt) > 0
                ? vt.AngleTo(dir)
                : vt.AngleTo(-dir);

            d = Math.Sin(angle) * d;

            return p + vti * d;
        }
        public static XYZ Mirror(this XYZ p, Line l)
        {
            var pm = p.PointToLine(l);
            return p.Mirror(pm);
        }
        public static XYZ Mirror(this XYZ p, XYZ pc)
        {
            return new XYZ(pc.X * 2 - p.X, pc.Y * 2 - p.Y, pc.Z * 2 - p.Z);
        }
        public static bool IsSame(this XYZ p1, XYZ p2, double tolerance = 1)
        {
            return p1.Distance(p2).FootToMm().IsSmallerEqual(tolerance);
        }
        public static XYZ CenterPoint(this List<XYZ> points)
        {
            points = points.Distinct(new ComparePoint()).ToList();
            if (!points.Any()) return null;
            if (points.Count == 1)
                return points.FirstOrDefault();
            var x = points.Select(a => a.X).ToList();
            var y = points.Select(a => a.Y).ToList();
            var z = points.Select(a => a.Z).ToList();
            var min = new XYZ(x.Min(), y.Min(), z.Min());
            var max = new XYZ(x.Max(), y.Max(), z.Max());
            var center = max.MidPoint(min);
            return center;
        }
        public static List<Curve> PointsToCurves(this List<XYZ> points, bool isClose = false)
        {
            var curves = new List<Curve>();
            var pc = points.Count;
            for (int i = 0; i < pc; i++)
            {
                if (isClose)
                {
                    var j = i == 0 ? pc - 1 : i - 1;
                    curves.Add(Line.CreateBound(points[j], (points[i])));
                }
                else
                {
                    if (i < pc - 1)
                    {
                        var sp = points[i];
                        var ep = points[i + 1];
                        curves.Add(Line.CreateBound(sp, ep));
                    }
                }
            }
            return curves;
        }
        public static XYZ GetModelCoordinatesAtCursor(this UIDocument uidoc)
        {
            UIView uiview = uidoc.GetOpenUIViews().FirstOrDefault();
            if (uiview is null) return XYZ.Zero;
            var view = uidoc.Document.ActiveView;
            Rectangle rect = uiview.GetWindowRectangle();
            var p = System.Windows.Forms.Cursor.Position;
            double u = (p.X - rect.Left) / (double)(rect.Right - rect.Left);
            double v = (p.Y - rect.Top) / (double)(rect.Bottom - rect.Top);
            XYZ R = view.RightDirection, U = view.UpDirection;
            double dx = (double)(p.X - rect.Left)
              / (rect.Right - rect.Left);

            double dy = (double)(p.Y - rect.Bottom)
              / (rect.Top - rect.Bottom);

            IList<XYZ> corners = uiview.GetZoomCorners();
            XYZ a = corners[0];
            XYZ b = corners[1];
            XYZ diag = b - a;
            double w = diag.DotProduct(R), hgt = diag.DotProduct(U);
            XYZ B = a + (u * w) * R + ((1 - v) * hgt) * U;
            return B;
        }
        public static bool TryGetRayUnderMouse(this UIDocument uidoc, out XYZ origin, out XYZ direction)
        {
            origin = null; direction = null;

            var view = uidoc.ActiveView as View3D;
            if (view == null) return false;
            var uiview = uidoc.GetOpenUIViews().FirstOrDefault(v => v.ViewId == view.Id);
            if (uiview == null) return false;
            var p = System.Windows.Forms.Cursor.Position;
            var rect = uiview.GetWindowRectangle();
            double u = (p.X - rect.Left) / (double)(rect.Right - rect.Left);
            double v = (p.Y - rect.Top) / (double)(rect.Bottom - rect.Top);
            XYZ R = view.RightDirection, U = view.UpDirection;
            double dx = (double)(p.X - rect.Left)
              / (rect.Right - rect.Left);

            double dy = (double)(p.Y - rect.Bottom)
              / (rect.Top - rect.Bottom);
            IList<XYZ> corners = uiview.GetZoomCorners();
            XYZ pMin = corners[0];
            XYZ pMax = corners[1];

            XYZ diag = pMax - pMin;
            double w = diag.DotProduct(R);
            double h = diag.DotProduct(U);

            // 4) Điểm “pNear” ứng với pixel trên mặt phẳng vuông góc ViewDirection
            XYZ pNear = pMin + (u * w) * R + ((1 - v) * h) * U;

            if (view.IsPerspective)
            {
                // Tạo ray từ mắt → pNear
                var cam = view.GetOrientation();
                origin = cam.EyePosition;
                direction = (pNear - origin).Normalize();
            }
            else
            {
                // Ray song song ViewDirection, gốc đặt ngay tại pNear
                origin = pNear;
                direction = view.ViewDirection.Normalize();
            }
            return true;
        }
    }
}
