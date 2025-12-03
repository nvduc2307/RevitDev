using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.Compares
{
    public class CompareLevelById : IEqualityComparer<Level>
    {
        public bool Equals(Level x, Level y)
        {
            return x.Id.ToString() == y.Id.ToString();
        }

        public int GetHashCode(Level obj)
        {
            return 0;
        }
    }
}
