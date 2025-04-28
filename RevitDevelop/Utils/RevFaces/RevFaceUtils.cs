using RevitDevelop.Utils.NumberUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.RevFaces
{
    public static class RevFaceUtils
    {
        public static CurveLoop GetFirstCurveLoop(this Face face)
        {
            return (from x in face.GetEdgesAsCurveLoops()
                    orderby x.GetExactLength() descending
                    select x).FirstOrDefault();
        }

        public static List<XYZ> GetPoints(this Face face)
        {
            List<XYZ> list = new List<XYZ>();
            foreach (Curve item in (from x in face.GetEdgesAsCurveLoops()
                                    orderby x.GetExactLength() descending
                                    select x).FirstOrDefault())
            {
                list.Add(item.GetEndPoint(0));
            }

            return list;
        }

        public static double DistanceTo(this Face face, XYZ point)
        {
            if (face != null && point != null)
            {
                UV uv = UV.Zero;
                double distance = 0.0;
                face.GetSurface().Project(point, out uv, out distance);
                return distance;
            }

            return 0.0;
        }

        public static XYZ FaceCenter(this Face face)
        {
            IList<XYZ> vertices = face.Triangulate().Vertices;
            XYZ zero = XYZ.Zero;
            foreach (XYZ item in vertices)
            {
                zero += item;
            }

            return zero / vertices.Count;
        }


        public static bool IsOverlap(Face face1, Face face2)
        {
            if (face1 != null && face1 != null && face1.Intersect(face2) == FaceIntersectionFaceResult.NonIntersecting)
            {
                foreach (EdgeArray edgeLoop in face1.EdgeLoops)
                {
                    foreach (Edge item in edgeLoop)
                    {
                        Curve curve = item.AsCurve();
                        XYZ endPoint = curve.GetEndPoint(0);
                        XYZ endPoint2 = curve.GetEndPoint(1);
                        if (!face2.Project(endPoint).Distance.IsZero())
                        {
                            return false;
                        }

                        if (!face2.Project(endPoint2).Distance.IsZero())
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool IsParallel(Face face1, Face face2)
        {
            if (face1 != null && face1 != null && face1.Intersect(face2) == FaceIntersectionFaceResult.NonIntersecting)
            {
                return true;
            }

            return false;
        }

        public static bool IsIntersecting(Face face1, Face face2)
        {
            if (face1 != null && face1 != null && face1.Intersect(face2) == FaceIntersectionFaceResult.Intersecting)
            {
                return true;
            }

            return false;
        }

    }
}
