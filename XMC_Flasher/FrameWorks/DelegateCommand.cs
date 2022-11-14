using System;
using System.Windows.Input;

namespace XMC_Flasher.FrameWorks

{
    public class DelegateCommand<T> : ICommand
    {
        readonly Action<T?>? _execute;
        readonly Predicate<T?>? _canExecute;
        private Action? executeSave;
        private bool canExecuteSave;
        private DelegateCommand<object>? commandSelectFilterCategory;

        public string? CommandName { get; set; }
        public DelegateCommand(Action<T?> execute) : this(execute, null) { }
        public DelegateCommand(Action<T?> execute, Predicate<T?>? canExecute, string commandName = "")
        {
            _execute = execute;
            _canExecute = canExecute;
            CommandName = commandName;
        }

        public DelegateCommand(Action executeSave, bool canExecuteSave)
        {
            this.executeSave = executeSave;
            this.canExecuteSave = canExecuteSave;
        }

        public DelegateCommand(DelegateCommand<object> commandSelectFilterCategory)
        {
            this.commandSelectFilterCategory = commandSelectFilterCategory;
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                if (_canExecute != null) CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null) CommandManager.RequerySuggested -= value;
            }

        }
        public void RaiseCanExecuteChanged()
        {
            canExecuteSave = false;
            CommandManager.InvalidateRequerySuggested();
        }
        public bool CanExecute(object? parameter)
        {

            return _canExecute == null ? true : _canExecute((T?)parameter);
        }

        public void Execute(object? parameter)
        { 
            if (_execute != null)
                _execute((T?)parameter);
        }
    }
}

