using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppBall.ViewModel
{
    /// <summary>
    /// Model warstwy Prezentacja (MVVM).
    /// Przechowuje dane jednej kuli dla GUI.
    /// Odpowiada za skalowanie wartości z warstwy Logika do rozmiaru ekranu.
    /// </summary>
    public class BallViewModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private double _diameter;
        private int _id;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanvasLeft));
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanvasTop));
            }
        }

        public double Diameter
        {
            get => _diameter;
            set
            {
                _diameter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanvasLeft));
                OnPropertyChanged(nameof(CanvasTop));
            }
        }

        /// <summary>
        /// Pozycja lewego górnego rogu elipsy na Canvas (Canvas.Left).
        /// X to środek kuli, więc odejmujemy połowę średnicy.
        /// </summary>
        public double CanvasLeft => X - Diameter / 2.0;

        /// <summary>
        /// Pozycja lewego górnego rogu elipsy na Canvas (Canvas.Top).
        /// Y to środek kuli, więc odejmujemy połowę średnicy.
        /// </summary>
        public double CanvasTop => Y - Diameter / 2.0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}