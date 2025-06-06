using Autodesk.Revit.UI;
using System.Diagnostics;

namespace RevitDevelop.Updaters
{
    public class DocumentChangedUpdater
    {
        public static void Init(UIControlledApplication application)
        {
            try
            {
                application.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;
            }
            catch (Exception)
            {
            }
        }

        private static void ControlledApplication_ApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
        }

        public static void Dispose(UIControlledApplication application)
        {
            try
            {
                application.ControlledApplication.DocumentChanged -= Application_DocumentChanged;
            }
            catch (Exception)
            {
            }
        }

        private static void Application_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            try
            {
                var document = e.GetDocument();
                var docDiff = document.GetChangedElements(new Guid("5bdf87c8-b799-4aaa-ad9d-071853764491"));
                var modifiedElementIds = docDiff.GetModifiedElementIds();
                Debug.WriteLine(modifiedElementIds.FirstOrDefault());
            }
            catch (Exception)
            {
            }
        }
    }
}
