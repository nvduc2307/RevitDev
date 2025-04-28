using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.NumberUtils;

namespace RevitDevelop.Utils.Geometries
{
    public static class GeometrySupport
    {
        private const int EPS_DIGITS = 6;
        private const double EPS_DECIMAL = 1.0e-6;
        public static XYZ EditX(this XYZ p, double x)
        {
            return new XYZ(x, p.Y, p.Z);
        }
        public static XYZ EditY(this XYZ p, double y)
        {
            return new XYZ(p.X, y, p.Z);
        }
        public static XYZ EditZ(this XYZ p, double z)
        {
            return new XYZ(p.X, p.Y, z);
        }
        public static bool IsSameDirection(this XYZ vt1, XYZ vt2)
        {
            var ispall = vt1.IsParallel(vt2);
            return !ispall ? false : !vt1.DotProduct(vt2).IsSmaller(0);
        }
        public static bool IsParallel(this XYZ vt1, XYZ vt2)
        {
            return CompareInstances.IsAlmostEqual(Math.Abs(vt1.DotProduct(vt2)), 1);
        }
        public static XYZ RayIntersectPlane(this XYZ point, XYZ vecRay, PlanarFace planarFace)
        {
            XYZ p = null;

            var ori = planarFace.Origin;
            var normal = planarFace.FaceNormal;
            if (vecRay.DotProduct(normal) != 0)
            {
                var dist = (ori - point).DotProduct(normal) / (vecRay.DotProduct(normal));
                p = point.Add(dist * vecRay.Normalize());
            }
            return p;
        }
        public static XYZ RayIntersectPlane(this XYZ point, XYZ vecRay, Plane plane)
        {
            XYZ p = null;
            var ori = plane.Origin;
            var normal = plane.Normal.Normalize();
            if (vecRay.DotProduct(normal) != 0)
            {
                var dist = (ori - point).DotProduct(normal) / (vecRay.DotProduct(normal));
                p = point.Add(dist * vecRay.Normalize());
            }
            return p;
        }
        public static XYZ RayIntersectLine2D(this XYZ point, XYZ vecRay, Line line)
        {
            XYZ p = null;
            if (vecRay.DotProduct(line.Direction.CrossProduct(XYZ.BasisZ).Normalize()) != 0)
            {
                var plane = Plane.CreateByNormalAndOrigin(line.Direction.CrossProduct(XYZ.BasisZ).Normalize(), line.GetEndPoint(0));
                p = point.RayIntersectPlane(vecRay, plane);
            }
            return p;
        }
        public static XYZ RayIntersectLineBound2D(this XYZ point, XYZ vecRay, Line line)
        {
            XYZ p = null;
            //Check can ray to line
            var canRay = false;
            var cross = vecRay.CrossProduct(XYZ.BasisZ).Normalize();
            var dot1 = cross.DotProduct(line.GetEndPoint(0) - point);
            var dot2 = cross.DotProduct(line.GetEndPoint(1) - point);
            if (dot1 * dot2 < 0) canRay = true;

            if (vecRay.DotProduct(line.Direction.CrossProduct(XYZ.BasisZ).Normalize()) != 0 && canRay)
            {
                var plane = Plane.CreateByNormalAndOrigin(line.Direction.CrossProduct(XYZ.BasisZ).Normalize(), line.GetEndPoint(0));
                p = point.RayIntersectPlane(vecRay, plane);
            }
            return p;
        }
        public static XYZ IntersectLines2D(Line l1, Line l2, double zElevation = 0)
        {
            var p1s = l1.GetEndPoint(0);
            var p1e = l1.GetEndPoint(1);
            var p2s = l2.GetEndPoint(0);
            var p2e = l2.GetEndPoint(1);
            p1s = new XYZ(p1s.X, p1s.Y, zElevation);
            p1e = new XYZ(p1e.X, p1e.Y, zElevation);
            p2s = new XYZ(p2s.X, p2s.Y, zElevation);
            p2e = new XYZ(p2e.X, p2e.Y, zElevation);
            var dir1 = p1e - p1s;
            var dir2 = p2e - p2s;
            if (!dir1.IsParallel(dir2))
            {
                var plane = Plane.CreateByNormalAndOrigin(dir1.CrossProduct(XYZ.BasisZ).Normalize(), p1s);
                return p2s.RayIntersectPlane(dir2.Normalize(), plane);
            }
            else
            {
                return null;
            }
        }
        public static Line TrimLineToLine2D(Line lineBeTrim, Line lineReference)
        {
            var planeRefer = Plane.CreateByNormalAndOrigin(lineReference.Direction.CrossProduct(XYZ.BasisZ), lineReference.GetEndPoint(0));
            var p1 = lineBeTrim.GetEndPoint(0);
            var p2 = lineBeTrim.GetEndPoint(1);
            var p1OnPlane = p1.RayIntersectPlane(lineBeTrim.Direction.Normalize(), planeRefer);
            var p2OnPlane = p2.RayIntersectPlane(lineBeTrim.Direction.Normalize(), planeRefer);
            return p1.DistanceTo(p1OnPlane) < p2.DistanceTo(p2OnPlane) ? Line.CreateBound(p1OnPlane, p2) : Line.CreateBound(p1, p2OnPlane);
        }
        public static bool IsIntersectWithLineSegmentOnPlane(this Line lineSegment1, Line lineSegment2)
        {
            bool r;

            //Line 1
            var u1 = lineSegment1.Direction.Normalize();
            var n1 = u1.CrossProduct(XYZ.BasisZ).Normalize();
            var start1 = lineSegment1.GetEndPoint(0);
            var end1 = lineSegment1.GetEndPoint(1);

            //Line 2
            var u2 = lineSegment2.Direction.Normalize();
            var n2 = u2.CrossProduct(XYZ.BasisZ).Normalize();
            var start2 = lineSegment2.GetEndPoint(0);
            var end2 = lineSegment2.GetEndPoint(1);

            //If # parallel
            if (!u1.IsParallel(u2))
            {
                bool conditon1;
                bool conditon2;

                var dot11 = CompareInstances.Round((start2 - start1).DotProduct(n1), EPS_DIGITS);
                var dot12 = CompareInstances.Round((end2 - start1).DotProduct(n1), EPS_DIGITS);
                conditon1 = dot11 * dot12 <= 0;

                var dot21 = CompareInstances.Round((start1 - start2).DotProduct(n2), EPS_DIGITS);
                var dot22 = CompareInstances.Round((end1 - start2).DotProduct(n2), EPS_DIGITS);
                conditon2 = dot21 * dot22 <= 0;

                r = conditon1 && conditon2;
            }
            else //If parallel
            {
                //Linesegment lie same Line
                if (CompareInstances.IsAlmostEqual(start1.DotProduct(n1), start2.DotProduct(n1), EPS_DECIMAL))
                {
                    bool cond1;
                    bool cond2;
                    bool cond3;

                    var dot1 = CompareInstances.Round((start2 - start1).DotProduct(n1), EPS_DIGITS);
                    var dot2 = CompareInstances.Round((end2 - start1).DotProduct(n1), EPS_DIGITS);
                    cond1 = dot1 * dot2 <= 0;

                    var dot3 = CompareInstances.Round((start2 - end1).DotProduct(n1), EPS_DIGITS);
                    var dot4 = CompareInstances.Round((end2 - end1).DotProduct(n1), EPS_DIGITS);
                    cond2 = dot3 * dot4 <= 0;

                    var dot5 = CompareInstances.Round((start2 - start1).DotProduct(n1), EPS_DIGITS);
                    var dot6 = CompareInstances.Round((end2 - start1).DotProduct(n1), EPS_DIGITS);
                    var dot7 = CompareInstances.Round((end1 - start1).DotProduct(n1), EPS_DIGITS);
                    cond3 = (dot5 <= dot7) && (dot6 <= dot7);

                    r = cond1 || cond2 || cond3;
                }
                else
                {
                    r = false;
                }
            }

            return r;
        }
        public static List<XYZ> FixZ(this List<XYZ> xyzs, double z = 0)
        {
            return xyzs.Select(p => new XYZ(p.X, p.Y, z)).ToList();
        }
        public static XYZ FixZ(this XYZ xyz, double z = 0)
        {
            return new XYZ(xyz.X, xyz.Y, z);
        }
        public static Line FixZ(this Line line, double z = 0)
        {
            Line r = null;
            try
            {
                r = Line.CreateBound(line.GetEndPoint(0).FixZ(z), line.GetEndPoint(1).FixZ(z));
            }
            catch { }
            return r;
        }
        public static bool IsCounterClockWise(this List<XYZ> polygons)
        {
            bool r = false;
            double sum = 0;
            for (int i = 0; i < polygons.Count - 1; i++)
            {
                var x1 = polygons[i].X;
                var x2 = polygons[i + 1].X;
                var y1 = polygons[i].Y;
                var y2 = polygons[i + 1].Y;
                sum += (x2 - x1) * (y2 + y1);
            }
            var sp = polygons[0];
            var ep = polygons[polygons.Count - 1];
            sum += (sp.X - ep.X) * (sp.Y + ep.Y);
            if (sum < 0)
            {
                r = true;
            }
            return r;
        }
        public static bool IsPointInPolygon(this XYZ point, List<XYZ> polygon)
        {
            if (point is null) return false;
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}
