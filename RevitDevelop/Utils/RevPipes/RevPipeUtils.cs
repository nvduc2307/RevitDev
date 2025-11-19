using Autodesk.Revit.DB.Plumbing;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;

namespace RevitDevelop.Utils.RevPipes
{
    public static class RevPipeUtils
    {
        public static Pipe CreateNew(this Pipe pipeBase, XYZ sp, XYZ ep)
        {
            Pipe result = null;
            try
            {
                var id = pipeBase.Copy(XYZ.BasisX * 1).FirstOrDefault();
                if (id == null)
                    return result;
                var document = pipeBase.Document;
                var pNew = document.GetElement(id) as Pipe;
                var location = pNew.Location as LocationCurve;
                location.Curve = Line.CreateBound(sp, ep);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static void CreateConnect(this Pipe p1, Pipe p2)
        {
            try
            {
                var connectType = GetConnectorLocationType(p1, p2, out Pipe main, out Pipe branch);
                if (connectType == ConnectorLocationType.None)
                    return;
                else if (connectType == ConnectorLocationType.EndPoint)
                    CreateConnetAtEndPoint(main, branch);
                else
                    CreateConnectAtBody(main, branch);
            }
            catch (Exception)
            {
            }
        }
        public static ConnectorLocationType GetConnectorLocationType(this Pipe p1, Pipe p2, out Pipe main, out Pipe branch)
        {
            main = p1;
            branch = p2;
            try
            {
                var l1 = (p1.Location as LocationCurve).Curve;
                var l2 = (p2.Location as LocationCurve).Curve;
                //check type endpoint
                var ds = new List<double>()
                {
                    l1.GetEndPoint(0).DistanceTo(l2.GetEndPoint(0)).FootToMm(),
                    l1.GetEndPoint(0).DistanceTo(l2.GetEndPoint(1)).FootToMm(),
                    l1.GetEndPoint(1).DistanceTo(l2.GetEndPoint(0)).FootToMm(),
                    l1.GetEndPoint(1).DistanceTo(l2.GetEndPoint(1)).FootToMm()
                };
                if (ds.Any(x=> x <= 50)) return ConnectorLocationType.EndPoint;
                //check type body
                var normal = l1.Direction().CrossProduct(l2.Direction());
                var normal1 = normal.CrossProduct(l1.Direction());
                var face1 = new FaceCustom(normal1, l1.Mid());
                var p1Check = l2.Mid().RayPointToFace(l2.Direction(), face1);
                if ((p1Check - l2.GetEndPoint(0)).DotProduct((p1Check - l2.GetEndPoint(1))) < 0)
                {
                    main = p2;
                    branch = p1;
                    return ConnectorLocationType.Body;
                }
                var normal2 = normal.CrossProduct(l2.Direction());
                var face2 = new FaceCustom(normal2, l2.Mid());
                var p2Check = l1.Mid().RayPointToFace(l1.Direction(), face2);
                if ((p2Check - l1.GetEndPoint(0)).DotProduct((p2Check - l1.GetEndPoint(1))) < 0)
                {
                    main = p1;
                    branch = p2;
                    return ConnectorLocationType.Body;
                }
            }
            catch (Exception)
            {
            }
            return ConnectorLocationType.None;
        }
        public static FamilyInstance CreateConnectAtBody(this Pipe pMain, Pipe pBranch)
        {
            FamilyInstance result = null;
            try
            {
                var document = pMain.Document;
                var mainEnds = pMain.ConnectorManager.Connectors
                    .Cast<Connector>()
                    .Where(c => c.ConnectorType == ConnectorType.End)
                    .OrderBy(c => c.Origin.X + c.Origin.Y + c.Origin.Z)
                    .ToList();
                if (mainEnds.Count < 2) return null;
                var mainCurve = (pMain.Location as LocationCurve).Curve;
                var branchCon = pBranch.ConnectorManager.Connectors
                    .Cast<Connector>()
                    .OrderBy(c => mainCurve.Distance(c.Origin))
                    .FirstOrDefault();
                if (branchCon == null) return null;
                //lấy điểm tại kết nối
                var teePoint = mainCurve.Project(branchCon.Origin).XYZPoint;
                //ngắt ống chính thành 2 tại điểm kết nối
                var pNew = document.GetElement(PlumbingUtils.BreakCurve(document, pMain.Id, teePoint)) as Pipe;
                result = document.Create.NewTeeFitting(mainEnds[0], mainEnds[1], branchCon);
                var connectSp = CreateConnectAtBody(pNew, pBranch);
                document.Delete(connectSp.Id);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static void CreateConnetAtEndPoint(this Pipe p1, Pipe p2)
        {
            try
            {
                var document = p1.Document;
                Connector c1a, c1b, c2a, c2b;
                GetPipeEndConnectors(p1, out c1a, out c1b);
                GetPipeEndConnectors(p2, out c2a, out c2b);
                var pairs = new[]
                {
                    (c1a, c2a), (c1a, c2b),
                    (c1b, c2a), (c1b, c2b),
                }
                .Where(p => p.Item1 != null && p.Item2 != null)
                .OrderBy(p => p.Item1.Origin.DistanceTo(p.Item2.Origin))
                .ToList();
                if (!pairs.Any()) return;
                var (conA, conB) = pairs.First();
                // Nếu hai connector gần trùng nhau & cùng hướng -> dùng union; nếu không -> elbow
                bool nearlySamePoint = conA.Origin.DistanceTo(conB.Origin) < 1e-3; // mm -> tuỳ units
                bool colinearOpposite = conA.CoordinateSystem.BasisZ.IsParallel(conB.CoordinateSystem.BasisZ);
                var connect = colinearOpposite 
                    ? document.Create.NewUnionFitting(conA, conB)
                    : document.Create.NewElbowFitting(conA, conB);
            }
            catch (Exception)
            {

            }
        }
        public static void GetPipeEndConnectors(Pipe pipe, out Connector end1, out Connector end2)
        {
            end1 = null; end2 = null;
            var cm = pipe.ConnectorManager;
            if (cm == null) return;

            var ends = cm.Connectors
                .Cast<Connector>()
                .Where(c => c.ConnectorType == ConnectorType.End)
                .ToList();

            if (ends.Count >= 1) end1 = ends[0];
            if (ends.Count >= 2) end2 = ends[1];
        }
    }
    public enum ConnectorLocationType
    {
        None = -1,
        EndPoint = 0,
        Body = 1
    }
}
