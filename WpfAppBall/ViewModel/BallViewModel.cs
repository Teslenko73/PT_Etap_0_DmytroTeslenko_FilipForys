using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppBall.ViewModel
{

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
                if(_x == value) return;
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
                if(_y == value) return;
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
                if (_diameter == value) return; // NIE powiadamiaj, jeśli wartość jest taka sama
                _diameter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanvasLeft));
                OnPropertyChanged(nameof(CanvasTop));
            }
        }


        public double CanvasLeft => X - Diameter / 2.0;

 
        public double CanvasTop => Y - Diameter / 2.0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}