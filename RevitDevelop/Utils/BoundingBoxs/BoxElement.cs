using Autodesk.Revit.DB.Structure;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.Solids;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.BoundingBoxs
{
    public class BoxElement
    {
        public int Id { get; }
        public string UniqueId { get; }
        public XYZ VTX { get; set; }
        public XYZ VTY { get; set; }
        public XYZ VTZ { get; set; }
        public Element Element { get; }
        public List<Solid> Solids { get; }
        public List<Curve> Curves { get; }
        public Outline Outline { get; set; }
        public Line LineBox { get; private set; }
        public Line LineBoxTop { get; private set; }
        public Line LineBoxBot { get; private set; }
        public Line LineBoxMid { get; private set; }
        public BoxElementPoint BoxElementPoint { get; set; }
        public BoxElement(Element ele)
        {
            Element = ele;
            Id = int.Parse(Element.Id.ToString());
            UniqueId = ele.UniqueId;
            Solids = GetSolids();
            Curves = GetCurves();
            VTX = GetVTX();
            VTY = !VTX.IsParallel(XYZ.BasisZ) ? VTX.CrossProduct(XYZ.BasisZ).Normalize() : VTX.CrossProduct(XYZ.BasisX).Normalize();
            VTZ = VTX.CrossProduct(VTY).Normalize();
            Outline = GetOutLine(out BoxElementPoint boxElementPoint, out Line linebox);
            LineBox = linebox;
            var z = Outline != null ? (Outline.MinimumPoint.Z + Outline.MaximumPoint.Z) / 2 : 0;
            LineBoxMid = Outline != null ? Line.CreateBound(Outline.MinimumPoint.EditZ(z), Outline.MaximumPoint.EditZ(z)) : null;
            LineBoxTop = Outline != null ? Line.CreateBound(Outline.MinimumPoint.EditZ(Outline.MaximumPoint.Z), Outline.MaximumPoint) : null;
            LineBoxBot = Outline != null ? Line.CreateBound(Outline.MinimumPoint, Outline.MaximumPoint.EditZ(Outline.MinimumPoint.Z)) : null;
            BoxElementPoint = boxElementPoint;
        }
        private List<Solid> GetSolids()
        {
            var results = new List<Solid>();
            try
            {
                results = Element.GetSolids();

            }
            catch (Exception)
            {
            }
            return results;
        }
        private Outline GetOutLine(out BoxElementPoint boxElementPoint, out Line lineBox)
        {
            lineBox = null;
            boxElementPoint = new BoxElementPoint();
            try
            {
                var ps = Curves
                    .Where(x => x is Line)
                    .Select(x => new List<XYZ>() { x.GetEndPoint(0), x.GetEndPoint(1) })
                    .Aggregate((a, b) => a.Concat(b).ToList())
                    .ToList();
                var pxs = ps.OrderBy(x => x.DotProduct(VTX)).ToList();
                var pys = ps.OrderBy(x => x.DotProduct(VTY)).ToList();
                var pzs = ps.OrderBy(x => x.DotProduct(VTZ)).ToList();

                if (pxs.Count <= 0) return null;
                if (pys.Count <= 0) return null;
                if (pzs.Count <= 0) return null;

                var fxStart = new FaceCustom(VTX, pxs.FirstOrDefault());
                var fxEnd = new FaceCustom(VTX, pxs.LastOrDefault());
                var fyStart = new FaceCustom(VTY, pys.FirstOrDefault());
                var fyEnd = new FaceCustom(VTY, pys.LastOrDefault());
                var fzStart = new FaceCustom(VTZ, pzs.FirstOrDefault());
                var fzEnd = new FaceCustom(VTZ, pzs.LastOrDefault());

                var minBotX = pxs.FirstOrDefault().RayPointToFace(VTZ, fzStart);
                var maxBotX = pxs.LastOrDefault().RayPointToFace(VTZ, fzStart);

                var pb1 = minBotX.RayPointToFace(fyStart.Normal, fyStart);
                var pb2 = minBotX.RayPointToFace(fyEnd.Normal, fyEnd);
                var pb3 = maxBotX.RayPointToFace(fyEnd.Normal, fyEnd);
                var pb4 = maxBotX.RayPointToFace(fyStart.Normal, fyStart);

                boxElementPoint.P1 = pb1;
                boxElementPoint.P2 = pb2;
                boxElementPoint.P3 = pb3;
                boxElementPoint.P4 = pb4;

                var minTopX = pxs.FirstOrDefault().RayPointToFace(VTZ, fzEnd);
                var maxTopX = pxs.LastOrDefault().RayPointToFace(VTZ, fzEnd);

                var pt1 = minTopX.RayPointToFace(fyStart.Normal, fyStart);
                var pt2 = minTopX.RayPointToFace(fyEnd.Normal, fyEnd);
                var pt3 = maxTopX.RayPointToFace(fyEnd.Normal, fyEnd);
                var pt4 = maxTopX.RayPointToFace(fyStart.Normal, fyStart);

                boxElementPoint.P5 = pt1;
                boxElementPoint.P6 = pt2;
                boxElementPoint.P7 = pt3;
                boxElementPoint.P8 = pt4;
                lineBox = Line.CreateBound(pb1, pt3);

                return new Outline(pb1, pt3);
            }
            catch (Exception)
            {
                return null;
            }
        }
        private List<Curve> GetCurves()
        {
            var results = new List<Curve>();
            try
            {
                if (Element is AssemblyInstance ass)
                {
                    var eles = ass.GetMemberIds().Select(x => Element.Document.GetElement(x)).ToList();
                    foreach (var ele in eles)
                    {
                        var crs = GetCurvesFromElement(ele);
                        results.AddRange(crs);
                    }
                }
                else
                {
                    var crs = GetCurvesFromElement(Element);
                    results.AddRange(crs);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine(Element.Id);
            }
            return results;
        }
        private List<Curve> GetCurvesFromElement(Element ele)
        {
            var results = new List<Curve>();
            if (ele is Rebar rb)
            {
                var crs = rb
                    .GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeAllMultiplanarCurves, 0)
                    .ToList();
                results.AddRange(crs);
            }
            else
            {
                try
                {
                    var crs = ele.GetSolids()
                        .Select(x => x.GetFacesFromSolid())
                        .Aggregate((a, b) => a.Concat(b).ToList())
                        .Select(x => x.GetFirstCurveLoop().ToList())
                        .Select(x => x)
                        .Aggregate((a, b) => a.Concat(b).ToList());
                    results.AddRange(crs);
                }
                catch (Exception)
                {
                }
            }
            return results;
        }
        private XYZ GetVTX()
        {
            var result = new XYZ();
            try
            {
                var l = Curves
                    .Where(x => x is Line)
                    .OrderBy(x => x.Length)
                    .LastOrDefault();
                if (l == null) throw new Exception();
                result = l.Direction();

            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
