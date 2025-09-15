using System;
using System.Windows.Input;

namespace Awd.MVVM
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }


        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public virtual void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
    public class RelayCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;
        // private Action<System.Windows.Media.Visual> action;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }


        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        //public RelayCommand(Action<System.Windows.Media.Visual> action)
        //{
        //    // TODO: Complete member initialization
        //    this.action = action;
        //}

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            try
            {
                _execute((T)parameter);
            }
            catch (Exception ex)
            {
                try
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                catch { }
            }
        }
    }
}