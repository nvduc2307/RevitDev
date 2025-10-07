﻿using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.Utils.Virture
{
    public class CurveVisualizationServer : ElementVisualizationServer
    {
        public List<Line> Lines {  get; set; }
        public CurveVisualizationServer(UIDocument uiDocument, List<Line> lines) : base(uiDocument)
        {
            Lines = lines;
        }
        public override void MapGeometryBuffer()
        {
            foreach (var l in Lines)
            {
                var render = new RenderingBufferStorage();
                RenderHelper.MapCurveBuffer(render, l.Tessellate());
                _edgeBuffers.Add(render);
            }
        }
    }
}
