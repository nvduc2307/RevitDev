using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;

namespace RevitDevelop.Utils.RevGrids
{
    public static class RevGridUtils
    {
        public static void ConfigGridOnViewPlan(this List<Grid> grids, ViewPlan viewPlan)
        {
            try
            {
                var extent = 20.MmToFoot();
                var document = viewPlan.Document;
                var viewTemplate = document.GetElement(viewPlan.ViewTemplateId) as View;
                var scale = viewTemplate == null
                    ? viewPlan.Scale
                    : viewTemplate.Scale;
                var gridsGroup = grids
                    .Select(x => x)
                    .GroupBy(x => x, new CompareGrid(viewPlan))
                    .Select(x => x.ToList().Distinct(new CompareGridOverLap(viewPlan)).ToList())
                    .OrderByDescending(x => x.Count)
                    .ToList();
                var qGridGroup = gridsGroup.Count;
                if (qGridGroup < 2) return;
                var grs1 = gridsGroup[0].Select(x => x as DatumPlane).ToList();
                var grs2 = gridsGroup[1].Select(x => x as DatumPlane).ToList();
                var gr1Dir = grs1.FirstOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Direction();
                var gr2Dir = grs2.FirstOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Direction();

                var ps1Sort = grs1
                    .OrderBy(x =>
                    {
                        var mp = x.GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Mid();
                        return Math.Round(mp.DotProduct(gr2Dir).FootToMm(), 0);
                    });
                var ps2Sort = grs2
                    .OrderBy(x =>
                    {
                        var mp = x.GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Mid();
                        return Math.Round(mp.DotProduct(gr1Dir).FootToMm(), 0);
                    });
                var ps1SortStartFace = new FaceCustom(gr2Dir, ps1Sort.FirstOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Mid());
                var ps1SortEndFace = new FaceCustom(gr2Dir, ps1Sort.LastOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Mid());
                var ps2SortStartFace = new FaceCustom(gr1Dir, ps2Sort.FirstOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Mid());
                var ps2SortEndFace = new FaceCustom(gr1Dir, ps2Sort.LastOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0).Mid());

                foreach (var gr in grs1)
                {
                    var lcr = gr.GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0);

                    var p1 = lcr.Mid().RayPointToFace(gr1Dir, ps2SortEndFace) + gr1Dir * extent * scale;
                    var p2 = lcr.Mid().RayPointToFace(gr1Dir, ps2SortStartFace) - gr1Dir * extent * scale;

                    var l = Line.CreateBound(p1, p2);
                    gr.SetCurveInView(DatumExtentType.ViewSpecific, viewPlan, l);
                }
                foreach (var gr in grs2)
                {
                    var lcr = gr.GetCurvesInView(DatumExtentType.ViewSpecific, viewPlan).ElementAt(0);

                    var p1 = lcr.Mid().RayPointToFace(gr2Dir, ps1SortEndFace) + gr2Dir * extent * scale;
                    var p2 = lcr.Mid().RayPointToFace(gr2Dir, ps1SortStartFace) - gr2Dir * extent * scale;

                    var l = Line.CreateBound(p1, p2);
                    gr.SetCurveInView(DatumExtentType.ViewSpecific, viewPlan, l);
                }

            }
            catch (Exception)
            {
            }
        }
        public static void MultiSegmentGridToGrid(this Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<MultiSegmentGrid> multiGrids = collector
                .OfClass(typeof(MultiSegmentGrid))
                .Cast<MultiSegmentGrid>();
            try
            {
            if (collector.Count() > 0)
            {
                using (Transaction tx = new Transaction(doc, "Convert MultiSegmentGrid to Grid"))
                {
                    tx.Start();
                    List<ElementId> multiGridIds = multiGrids.Select(g => g.Id).ToList();

                    // Sau đó xóa
                    //foreach (var id in multiGridIds)
                    //{
                    //    doc.Delete(id);
                    //}

                    List<string> LstNameGrids = new List<string>();
                    List<Grid> LstnewGrid = new List<Grid>();
                    foreach (var ids in multiGridIds)
                    {
                        try
                        {
                            // Lấy Segment đầu tiên (nếu có)
                            MultiSegmentGrid aaa = doc.GetElement(ids) as MultiSegmentGrid;
                            var segments = aaa.GetGridIds();
                            List<Curve> curves = new List<Curve>();


                            List<ElementId> list = segments.ToList();
                            var G1 = (doc.GetElement(list.First()) as Grid);
                            XYZ P1 = G1.Curve.GetEndPoint(1);
                            XYZ P10 = G1.Curve.GetEndPoint(0);

                            var G2 = (doc.GetElement(list.Last()) as Grid);
                            XYZ P2 = G2.Curve.GetEndPoint(0);
                            XYZ P21 = G2.Curve.GetEndPoint(1);

                            if (Math.Round( P1.Y,3) == Math.Round(P2.Y,3))
                            {
                                XYZ[] points = new XYZ[] { P1, P10, P2, P21 };

                                XYZ maxXPoint = points.OrderByDescending(p => p.X).First();
                                XYZ minXPoint = points.OrderBy(p => p.X).First();
                                Line line = Line.CreateBound(minXPoint, maxXPoint);

                                Grid newGrid = Grid.Create(doc, line);
                                newGrid.SetVerticalExtents(G1.GetExtents().MinimumPoint.Z, G1.GetExtents().MaximumPoint.Z); 
                                string nameGr = aaa.Name;
                                doc.Delete(ids);
                                newGrid.Name = nameGr;
                            }
                            else if (Math.Round(P1.X, 3) == Math.Round(P2.X, 3))
                            {
                                XYZ[] points = new XYZ[] { P1, P10, P2, P21 };

                                XYZ maxXPoint = points.OrderByDescending(p => p.Y).First();
                                XYZ minXPoint = points.OrderBy(p => p.Y).First();
                                Line line = Line.CreateBound(minXPoint, maxXPoint);

                                Grid newGrid = Grid.Create(doc, line);
                                newGrid.SetVerticalExtents(G1.GetExtents().MinimumPoint.Z, G1.GetExtents().MaximumPoint.Z);
                                string nameGr = aaa.Name;
                                doc.Delete(ids);
                                newGrid.Name = nameGr;
                            }
                                




                        }
                            catch { }

                    }
          
                    tx.Commit();
                }

            }

            }
            catch { }
        }

    }
}
