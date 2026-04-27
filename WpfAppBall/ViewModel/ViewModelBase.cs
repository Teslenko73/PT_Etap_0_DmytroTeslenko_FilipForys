using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppBall.ViewModel
{
    /// <summary>
    /// Bazowa klasa ViewModelu - implementuje INotifyPropertyChanged.
    /// Wszystkie ViewModele dziedziczą z tej klasy.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}