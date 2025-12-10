using RevitDevelop.Utils.Messages;
using System.Windows.Input;

namespace RevitDevelop.TestDamper.commands
{
    public class CancelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            IO.ShowInfo("CancelCommand");
        }
    }
}
