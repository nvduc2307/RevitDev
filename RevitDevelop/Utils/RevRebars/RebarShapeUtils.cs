using Autodesk.Revit.DB.Structure;

namespace RevitDevelop.Utils.RevRebars
{
    public static class RebarShapeUtils
    {
        public static List<string> GetSegmentParamNames(this RebarShape rebarShape)
        {
            var segmentParamNames = new List<string>();
            var def = rebarShape.GetRebarShapeDefinition();

            if (def is RebarShapeDefinitionBySegments)
            {
                if (rebarShape.GetRebarShapeDefinition() is RebarShapeDefinitionBySegments defBySegment)
                {
                    for (var i = 0; i < defBySegment.NumberOfSegments; i++)
                    {
                        var rss = defBySegment.GetSegment(i);
                        if (rss.GetConstraints() is List<RebarShapeConstraint> constraints)
                        {
                            foreach (var rsc in constraints)
                            {
                                if (rsc is not RebarShapeConstraintSegmentLength)
                                    continue;
                                var paramId = rsc.GetParamId();
                                if ((paramId == ElementId.InvalidElementId))
                                    continue;
                                foreach (Parameter p in rebarShape.Parameters)
                                {
                                    if (p.Id.ToString() == paramId.ToString())
                                    {
                                        segmentParamNames.Add(p.Definition.Name);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return segmentParamNames;
        }

        public static List<Tuple<string, Guid>> GetSegmentParamNameAndGuids(this RebarShape rebarShape)
        {
            var segmentParamNames = new List<Tuple<string, Guid>>();
            var def = rebarShape.GetRebarShapeDefinition();

            if (def is RebarShapeDefinitionBySegments)
            {
                if (rebarShape.GetRebarShapeDefinition() is RebarShapeDefinitionBySegments defBySegment)
                {
                    for (var i = 0; i < defBySegment.NumberOfSegments; i++)
                    {
                        var rss = defBySegment.GetSegment(i);
                        if (rss.GetConstraints() is List<RebarShapeConstraint> constraints)
                        {
                            foreach (var rsc in constraints)
                            {
                                if (rsc is not RebarShapeConstraintSegmentLength)
                                    continue;
                                var paramId = rsc.GetParamId();
                                if ((paramId == ElementId.InvalidElementId))
                                    continue;
                                foreach (Parameter p in rebarShape.Parameters)
                                {
                                    if (p.Id.GetId() == paramId.GetId())
                                    {
                                        segmentParamNames.Add(new Tuple<string, Guid>(p.Definition.Name, p.GUID));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return segmentParamNames;
        }
    }
}
