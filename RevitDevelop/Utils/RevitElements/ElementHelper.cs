using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.RevitElements
{
    public static class ElementHelper
    {
        public static Element GetElement(this Reference reference, Document doc)
        {
            return doc.GetElement(reference);
        }
    }
}
