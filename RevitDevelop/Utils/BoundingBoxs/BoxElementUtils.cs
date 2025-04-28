using Autodesk.Revit.DB.Structure;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.Solids;
using System.Diagnostics;

namespace RevitDevelop.Utils.BoundingBoxs
{
    public static class BoxElementUtils
    {
        public static void GenerateCoordinateBeam(this List<FamilyInstance> familyInstances, out XYZ vtxOut, out XYZ vtyOut, out XYZ vtzOut)
        {
            vtxOut = null;
            vtyOut = null;
            vtzOut = null;
            try
            {
                vtxOut = familyInstances
                    .Select(x =>
                    {
                        var transf = x.GetTransform();
                        return transf.OfVector(XYZ.BasisX);
                    })
                    .GroupBy(x => x, new ComparePoint())
                    .OrderBy(x => x.Count())
                    .Select(x => x.ToList())
                    .LastOrDefault()
                .FirstOrDefault()
                    .Normalize();
                vtyOut = familyInstances
                    .Select(x =>
                    {
                        var transf = x.GetTransform();
                        return transf.OfVector(XYZ.BasisY);
                    })
                    .GroupBy(x => x, new ComparePoint())
                    .OrderBy(x => x.Count())
                    .Select(x => x.ToList())
                    .LastOrDefault()
                    .FirstOrDefault()
                    .Normalize();
                vtzOut = vtxOut.CrossProduct(vtyOut).Normalize();
            }
            catch (Exception)
            {
            }
        }
    }
}
