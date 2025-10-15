using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevDuct;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        public List<Element> GetElementsByFittingToDistance(
            FamilyInstance fittingStart,
            List<Element> elementAlreadySort,
            double lengthMm,
            out Duct ductLast,
            out XYZ ductLastPointStart,
            out XYZ ductLastPointIntersect,
            out XYZ ductLastPointEnd)
        {
            ductLast = null;
            ductLastPointStart = null;
            ductLastPointEnd = null;
            ductLastPointIntersect = null;
            var result = new List<Element>();
            double totalLengthUseMm = 0;
            double totalLengthUseMmPrev = 0;
            foreach (var element in elementAlreadySort)
            {
                var ps = element.GetMepGeometry(out XYZ midPoint);
                var ls = ps.PointsToCurves();
                var lengthElementMm = ls.Sum(x => Math.Round(x.Length.FootToMm(), 0));
                totalLengthUseMm += lengthElementMm;
                if (totalLengthUseMm <= lengthMm)
                    result.Add(element);
                else
                {
                    if (totalLengthUseMmPrev >= lengthMm)
                        continue;
                    if (!(element is Duct duct))
                        continue;
                    if (ps.Count != 2)
                        continue;
                    var index = elementAlreadySort.IndexOf(element);
                    var geoFittingStart = fittingStart.GetMepGeometry(out XYZ midFitting);
                    var pCheck = index == 0
                        ? midFitting
                        : elementAlreadySort[index - 1].GetConnectors().Select(x=>x.Origin).ToList().CenterPoint();
                    var lengthIntersectMm = lengthMm - totalLengthUseMmPrev;
                    ps = ps
                        .OrderBy(x=>x.DistanceTo(pCheck))
                        .ToList();
                    ductLastPointStart = ps.FirstOrDefault();
                    ductLastPointEnd = ps.LastOrDefault();
                    var vt = (ductLastPointEnd - ductLastPointStart).Normalize();
                    ductLastPointIntersect = ductLastPointStart + vt * lengthIntersectMm.MmToFoot();
                    ductLast = duct;
                }
                totalLengthUseMmPrev = totalLengthUseMm;
            }
            return result;
        }
    }
}
