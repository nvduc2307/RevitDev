using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.DemoST.Command3.models
{
    public partial class LevelModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [ObservableProperty]
        private bool _isSelected;
    }
}
