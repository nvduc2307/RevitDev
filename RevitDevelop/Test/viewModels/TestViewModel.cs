using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Test.viewModels
{
    public partial class TestViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _textTest;
    }
}
