using System;
using System.Windows.Input;

namespace Sorzus.Wpf.Toolkit
{
    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
    /// <remarks></remarks>
    /// <example>
    /// <code>
    ///private RelayCommand _testCommand;
    ///
    ///public RelayCommand TestCommand
    ///{
    ///    get
    ///    {
    ///        return _testCommand ?? (_testCommand = new RelayCommand(parameter =>
    ///        {
    ///
    ///        }));
    ///     }
    ///}
    /// </code>
    /// </example>
    public class RelayCommand : ICommand
    {
        public delegate void ExecuteDelegate(object parameter);

        private readonly Func<bool> _canExecute;
        private readonly ExecuteDelegate _execute;

        public RelayCommand(ExecuteDelegate execute)
            : this(execute, null)
        {
        }

        public RelayCommand(ExecuteDelegate execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }

    /// <summary>
    ///     A generic command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
    /// <remarks></remarks>
    /// <example>
    /// <code>
    ///private RelayCommand&lt;string> _testCommand;
    ///
    ///public RelayCommand&lt;string> TestCommand
    ///{
    ///    get
    ///    {
    ///        return _testCommand ?? (_testCommand = new RelayCommand&lt;string>(parameter =>
    ///        {
    ///
    ///        }));
    ///     }
    ///}
    /// </code>
    /// </example>
    public class RelayCommand<T> : ICommand
    {
        public delegate void ExecuteDelegate(T parameter);

        private readonly Func<bool> _canExecute;
        private readonly ExecuteDelegate _execute;

        public RelayCommand(ExecuteDelegate execute)
            : this(execute, null)
        {
        }

        public RelayCommand(ExecuteDelegate execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke();
        }

        void ICommand.Execute(object parameter)
        {
            _execute.Invoke((T) parameter);
        }

        public void Execute(T parameter)
        {
            _execute.Invoke(parameter);
        }
    }
}