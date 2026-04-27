using System;
using System.Windows.Input;

namespace WpfAppBall.ViewModel
{
    /// <summary>
    /// Implementacja ICommand (wzorzec MVVM - RelayCommand).
    /// Umożliwia bindowanie przycisków do metod ViewModelu bez code-behind.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Przeciążenie dla wygody (Action bez parametru)
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(_ => execute(), canExecute == null ? (Func<object, bool>)null : _ => canExecute())
        { }

        public bool CanExecute(object parameter)
            => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter)
            => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged()
            => CommandManager.InvalidateRequerySuggested();
    }
}