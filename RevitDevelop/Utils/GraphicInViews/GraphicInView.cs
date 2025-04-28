namespace RevitDevelop.Utils.GraphicInViews
{
    public static class GraphicInView
    {
        public static void SetTransparenColorElement(this View view, Element ele, Color color, int transparency)
        {
            try
            {
                var document = view.Document;
                var graphic = new OverrideGraphicSettings();
                graphic.SetSurfaceTransparency(transparency);
                graphic.SetSurfaceBackgroundPatternColor(color);
                graphic.SetProjectionLineColor(color);
                graphic.SetCutLineColor(color);
                var fillPattern = new FilteredElementCollector(document)
                    .OfClass(typeof(FillPatternElement))
                    .Cast<FillPatternElement>()
                    .FirstOrDefault();
                var fillPattern_projectLine = new FilteredElementCollector(document)
                    .OfClass(typeof(LinePatternElement))
                    .Cast<LinePatternElement>()
                    .FirstOrDefault();
                if (fillPattern != null)
                {
                    graphic.SetSurfaceBackgroundPatternId(fillPattern.Id);
                    graphic.SetProjectionLinePatternId(fillPattern_projectLine.Id);
                    graphic.SetCutLinePatternId(fillPattern_projectLine.Id);
                }
                ;
                view.SetElementOverrides(ele.Id, graphic);
            }
            catch (Exception)
            {
            }
        }
    }
}
