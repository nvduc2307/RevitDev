using System.Windows.Input;

namespace RevitDevelop.TestDamper.commands
{
    public class ExternalCommandCustom : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            CanExecuteChanged?.Invoke(parameter, new EventArgs());
        }
    }
}
