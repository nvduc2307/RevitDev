using Autodesk.Revit.UI;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.RevViews;

namespace RevitDevelop.Utils.Geometries
{
    public static class GeometryCustom
    {
        public static double Distance(this XYZ p, Line l)
        {
            var d = 0.0;
            try
            {
                d = p.Distance(l.GetEndPoint(0));
                var dir = l.Direction;
                var vt = (l.GetEndPoint(0) - p).Normalize();
                if (CompareInstances.IsAlmostEqual(dir.DotProduct(vt), 0)) return p.Distance(l.GetEndPoint(0));
                if (CompareInstances.IsAlmostEqual(Math.Abs(dir.DotProduct(vt)), 1)) return 0;

                var angle = dir.DotProduct(vt) > 0
                    ? vt.AngleTo(dir)
                    : vt.AngleTo(-dir);
                d = Math.Sin(angle) * d;
            }
            catch (Exception)
            {
                d = 0.0;
            }
            return d;
        }
        public static double Distance(this XYZ p)
        {
            var result = 0.0;
            try
            {
                result = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static double Distance(this XYZ p1, XYZ p2)
        {
            try
            {
                var x = p1.X - p2.X;
                var y = p1.Y - p2.Y;
                var z = p1.Z - p2.Z;
                return Math.Sqrt(x * x + y * y + z * z);
            }
            catch (Exception)
            {
            }
            return 0;
        }
        public static double Distance(this XYZ p, FaceCustom faceCad)
        {
            var result = 0.0;
            try
            {
                var d = p.Distance(faceCad.BasePoint);
                var vt = (faceCad.BasePoint - p).VectorNormal();
                var angle = faceCad.Normal.DotProduct(vt) >= 0
                    ? faceCad.Normal.AngleTo(vt)
                    : faceCad.Normal.AngleTo(-vt);
                result = Math.Cos(angle) * d;
            }
            catch (Exception)
            {
            }
            return result;
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
        public static XYZ RayPointToFace(this XYZ p, XYZ vtRay, FaceCustom faceCad)
        {
            XYZ result = p;
            try
            {
                var vt = (faceCad.BasePoint - p).VectorNormal();
                var normalFace = vt.DotProduct(faceCad.Normal) >= 0 ? faceCad.Normal : -faceCad.Normal;
                var angle1 = normalFace.AngleTo(vt);
                var angle2 = normalFace.AngleTo(vtRay);

                var angle1D = normalFace.AngleTo(vt) * 180 / Math.PI;
                var angle2D = normalFace.AngleTo(vtRay) * 180 / Math.PI;

                var dm = p.Distance(faceCad.BasePoint);

                var dd = p.Distance(faceCad);

                var d = Math.Cos(angle1) * p.Distance(faceCad.BasePoint) / Math.Cos(angle2);
                result = p + vtRay * d;
            }
            catch (Exception)
            {
                result = p;
            }
            return result;
        }
        public static XYZ LineIntersectFace(this Line line, FaceCustom faceCad)
        {
            XYZ result = null;
            try
            {
                var p1 = line.GetEndPoint(0);
                var p2 = line.GetEndPoint(1);
                var p = line.Mid().RayPointToFace(line.Direction, faceCad);
                var vt1 = p1 - p;
                var vt2 = p2 - p;
                if (vt1.DotProduct(vt2) < 0) result = p;
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static double AngleTo(this XYZ vt1, XYZ vt2)
        {
            var result = 0.0;
            try
            {
                var cos = vt1.DotProduct(vt2) / (vt1.Distance() * vt2.Distance());
                result = Math.Acos(cos);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static XYZ Rotate(this XYZ p, FaceCustom f1, FaceCustom f2)
        {
            //p phai thuoc mp f1
            var result = p;
            try
            {
                var axis = f1.FaceIntersectFace(f2);
                var angle = f1.Normal.AngleTo(f2.Normal);
                var f = new FaceCustom(axis.Direction, p);
                var pc = axis.BasePoint.RayPointToFace(axis.Direction, f);
                var l = f.FaceIntersectFace(f2);
                var vt = (p - pc).Normalize();
                result = vt.DotProduct(f2.Normal) <= 0
                    ? pc + l.Direction * pc.Distance(p)
                    : pc - l.Direction * pc.Distance(p);
            }
            catch (Exception)
            {
                result = p;
            }
            return result;
        }
        public static XYZ Rotate(this XYZ p, FaceCustom f1, FaceCustom f2, XYZ vtCheck)
        {
            //p phai thuoc mp f1
            var result = p;
            try
            {
                var axis = f1.FaceIntersectFace(f2);
                var f = new FaceCustom(axis.Direction, p);
                var pc = axis.BasePoint.RayPointToFace(axis.Direction, f);
                var l = f.FaceIntersectFace(f2);
                var vt = (p - pc).Normalize();
                result = l.Direction.DotProduct(vtCheck) <= 0
                    ? pc + l.Direction * pc.Distance(p)
                    : pc - l.Direction * pc.Distance(p);
            }
            catch (Exception)
            {
                result = p;
            }
            return result;
        }
        public static XYZ Rotate(this XYZ p, XYZ c, double degRad)
        {
            var pn = new XYZ(p.X, p.Y, 0);
            var cn = new XYZ(c.X, c.Y, 0);
            var x = cn.X + (pn.X - cn.X) * Math.Cos(degRad) - (pn.Y - cn.Y) * Math.Sin(degRad);
            var y = cn.Y + (pn.X - cn.X) * Math.Sin(degRad) + (pn.Y - cn.Y) * Math.Cos(degRad);
            return new XYZ(x, y, c.Z);
        }
        public static XYZ Round(this XYZ p, int n = 4)
        {
            return new XYZ(Math.Round(p.X, n), Math.Round(p.Y, n), Math.Round(p.Z, n));
        }
        public static XYZ PointToLine(this XYZ p, Line l)
        {
            var result = p;
            try
            {
                var dir = (l.GetEndPoint(1) - l.GetEndPoint(0)).Normalize();
                var d = p.Distance(l.GetEndPoint(0));
                var vt = (l.GetEndPoint(0) - p).Normalize();
                if (CompareInstances.IsAlmostEqual(dir.DotProduct(vt), 0)) return l.GetEndPoint(0);
                if (CompareInstances.IsAlmostEqual(Math.Abs(dir.DotProduct(vt)), 1, 0.00000001)) return p;

                var normal = dir.CrossProduct(vt);

                var vti = dir.CrossProduct(normal).DotProduct(vt) > 0
                    ? dir.CrossProduct(normal).Normalize()
                    : -dir.CrossProduct(normal).Normalize();

                var angle = dir.DotProduct(vt) > 0
                    ? vt.AngleTo(dir)
                    : vt.AngleTo(-dir);

                d = Math.Sin(angle) * d;

                result = p + vti * d;
            }
            catch (Exception)
            {
                result = p;
            }
            //ddang sai
            return result;
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
        public static LineCustom FaceIntersectFace(this FaceCustom f1, FaceCustom f2)
        {
            LineCustom result = null;
            try
            {
                if (f1.Normal.IsSame(f2.Normal)) throw new Exception();
                var lDir = f1.Normal.CrossProduct(f2.Normal);
                var lDir1 = lDir.CrossProduct(f1.Normal);
                var p1 = f1.BasePoint.RayPointToFace(lDir1, f2);
                result = new LineCustom(lDir, p1);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
        public static Line CreateLine(this XYZ p1, XYZ p2)
        {
            return Line.CreateBound(p1, p2);
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
            try
            {
                var pc = points.Count;
                for (int i = 0; i < pc; i++)
                {
                    if (isClose)
                    {
                        var j = i == 0 ? pc - 1 : i - 1;
                        curves.Add(points[j].CreateLine(points[i]));
                    }
                    else
                    {
                        if (i < pc - 1)
                        {
                            var sp = points[i];
                            var ep = points[i + 1];
                            curves.Add(sp.CreateLine(ep));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return curves;
        }
        public static XYZ GetModelCoordinatesAtCursor(this UIDocument uidoc)
        {
            UIView uiview = ViewHelper.GetActiveUIView(uidoc);
            var view = uidoc.Document.ActiveView;
            if (uiview is null) return XYZ.Zero;
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
