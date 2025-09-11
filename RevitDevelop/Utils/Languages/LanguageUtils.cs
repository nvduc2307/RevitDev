using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.Languages
{
    public class LanguageUtils
    {
        private static void SetLanguage(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            try
            {
                var resource = new ResourceManager("DaiwaLease.Language.ResourceEN", Assembly.GetExecutingAssembly());
                if (app.Language == LanguageType.Japanese)
                {
                    resource = new ResourceManager("DaiwaLease.Language.ResourceJP", Assembly.GetExecutingAssembly());
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
