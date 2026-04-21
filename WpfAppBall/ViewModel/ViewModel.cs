using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WpfAppBall.ViewModel
{
    public class ViewModel : viemodel
    {
        public ObservableCollection<Ball> Balls { get; set; }
        public ICommand StartCommand { get; }

        private int _ballCount;
        public int BallCount
        {
            get => _ballCount;
            set { _ballCount = value; OnPropertyChanged(); }
        }

        public ViewModel()
        {
            Balls = new ObservableCollection<Ball>();
            StartCommand = new RelayCommand(StartSimulation);
        }

        private void StartSimulation()
        {
            // Tu w przyszłości wywołasz logikę tworzenia kulek
            // Na razie dla testu:
            Balls.Clear();
            for (int i = 0; i < BallCount; i++)
            {
                Balls.Add(new Ball { X = 10 * i, Y = 10 * i });
            }
        }
    }
}