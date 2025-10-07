using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.DirectContext3DServer.VirtualCurve
{
    public class CurveDirectContext3DServer : IDirectContext3DServer
    {
        private readonly Guid _guid;

        private readonly Color _color;
        public Document Document { get; set; }
        public CurveDirectContext3DServer(Document doc, Color color = null)
        {
            _guid = Guid.NewGuid();
            _color = color;
            Document = doc;
        }
        public Guid GetServerId() => this._guid;

        public string GetVendorId() => "STSO";

        public ExternalServiceId GetServiceId()
        => ExternalServices.BuiltInExternalServices.DirectContext3DService;

        public virtual string GetName() => "";

        public virtual string GetDescription() => "";

        public string GetApplicationId() => "";

        public string GetSourceId() => "";

        public bool UsesHandles() => false;

        public virtual bool CanExecute(View view) => view.Document.Equals(Document);

        public virtual Outline GetBoundingBox(View view) => null;

        public bool UseInTransparentPass(View view) => true;

        public virtual void RenderScene(View view, DisplayStyle displayStyle)
        {
        }
        public virtual List<Curve> PrepareProfile() => new List<Curve>();


        private void ProcessEdges(RenderingPassBufferStorage bufferStorage)
        {
            
        }
    }
}
