using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewModemBoudrateSwitcher.Helpers
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;
        private readonly Action _execute1;


        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
          : this(execute, null)
        {
        }
        public DelegateCommand(Action execute)
         : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public DelegateCommand(Action execute, Predicate<object> canExecute)
        {
            _execute1 = execute;
            _canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        void ICommand.Execute(object parameter)
        {
            _execute?.Invoke(parameter);
            _execute1?.Invoke();
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute(parameter);
        }
    }
}
