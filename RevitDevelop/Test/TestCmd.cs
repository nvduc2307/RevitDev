using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Test.viewModels;
using RevitDevelop.Test.views;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : ExternalCommand
    {
        private TestView _view;
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    var viewModel = new TestViewModel() { TextTest = "Text" };
                    _view = new TestView() { DataContext = viewModel };
                    _view.Loaded += View_Loaded;
                    _view.ShowDialog();
                    //--------
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception)
                {
                    tsg.RollBack();
                }
            }
        }

        private void View_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var allCheckBoxes = FindVisualChildren<System.Windows.Controls.CheckBox>(_view);
            var texblock1 = new TextBlock() { Text = "(Anh Yeu Em)", Foreground = new SolidColorBrush(Colors.Red) };
            var texblock2 = new TextBlock() { Text = "Tho" };
            var stackP = new StackPanel();
            stackP.Children.Add(texblock2);
            stackP.Children.Add(texblock1);
            stackP.Orientation = Orientation.Horizontal;
            foreach (var cb in allCheckBoxes)
            {
                cb.Content = stackP;
            }
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
        {
            if (root == null) yield break;

            var stack = new Stack<DependencyObject>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child is T t) yield return t;
                    stack.Push(child);
                }
            }
        }
    }
}
