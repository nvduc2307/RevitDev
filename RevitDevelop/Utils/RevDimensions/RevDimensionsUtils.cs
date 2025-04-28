using Autodesk.Revit.DB;

namespace RevitDevelop.Utils.RevDimensions
{
    public static class RevDimensionsUtils
    {
        public const string DIM_TYPE_NAME_BASE = "1.5 ㎜";
        public static void ReMoveZero(this Dimension dim)
        {
            var document = dim.Document;
            var l = dim.Curve as Line;
            var segments = dim.Segments;
            var refs = dim.References;
            var refA = new ReferenceArray();
            var c = 0;
            foreach (DimensionSegment item in segments)
            {
                try
                {
                    if (c == 0) refA.Append(refs.get_Item(c));
                    else
                    {
                        if (c == segments.Size - 1)
                        {
                            if (segments.get_Item(c).ValueString != "0")
                            {
                                if (segments.get_Item(c - 1).ValueString == "0")
                                    refA.Append(refs.get_Item(c + 1));
                                else
                                {
                                    refA.Append(refs.get_Item(c));
                                    refA.Append(refs.get_Item(c + 1));
                                }
                            }
                            else refA.Append(refs.get_Item(c));
                        }
                        else
                        {
                            if (segments.get_Item(c).ValueString != "0")
                            {
                                if (segments.get_Item(c - 1).ValueString != "0")
                                    refA.Append(refs.get_Item(c));
                            }
                            else refA.Append(refs.get_Item(c));
                        }
                    }
                }
                catch (Exception)
                {
                }
                c++;
            }
            document.Create.NewDimension(document.ActiveView, l, refA);
            document.Delete(dim.Id);
        }
    }
}
