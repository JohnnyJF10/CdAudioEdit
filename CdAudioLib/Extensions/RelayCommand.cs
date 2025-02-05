using System.Windows.Input;

namespace CdAudioLib.Extensions
{
    public class RelayCommand : ICommand
    {
        private Action<object?> execute; 
        private Predicate<object?>? canExecute; 

        public event EventHandler? CanExecuteChanged 
        {
            add => CustomCommandManager.RequerySuggested += value;
            remove => CustomCommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public RelayCommand(Action execute, Predicate<object?>? canExecute = null)
            :
            this(_ => execute(), canExecute)
        { }

        public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter);
        public void Execute(object? parameter) => this.execute(parameter);
        public void RaiseCanExecuteChanged() => CustomCommandManager.InvalidateRequerySuggested();

    }
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null)
                return (default(T) == null) 
                    ? _canExecute(default!) 
                    : throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            return _canExecute((T)parameter);
        }

        public void Execute(object? parameter)
        {
            if (parameter is null && default(T) is not null)
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            _execute((T)parameter!);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CustomCommandManager.RequerySuggested += value;
            remove => CustomCommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged() => CustomCommandManager.InvalidateRequerySuggested();
    }
}
