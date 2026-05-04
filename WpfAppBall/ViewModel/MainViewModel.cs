using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfAppBall.Logic;

namespace WpfAppBall.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly LogicAbstractApi _logic;

        private int _ballCount = 5;
        private bool _isRunning = false;
        private double _boardWidth = 600;
        private double _boardHeight = 400;


        public ObservableCollection<BallViewModel> Balls { get; }
            = new ObservableCollection<BallViewModel>();



        public int BallCount
        {
            get => _ballCount;
            set
            {
                if (value < 1) value = 1;
                if (value > 500) value = 500;
                _ballCount = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotRunning));
            }
        }

        public bool IsNotRunning => !_isRunning;

        public double BoardWidth
        {
            get => _boardWidth;
            set { _boardWidth = value; OnPropertyChanged(); }
        }

        public double BoardHeight
        {
            get => _boardHeight;
            set { _boardHeight = value; OnPropertyChanged(); }
        }



        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }




        public MainViewModel() : this(LogicAbstractApi.CreateApi()) { }


        public MainViewModel(LogicAbstractApi logic)
        {
            _logic = logic ?? throw new ArgumentNullException(nameof(logic));


            _logic.Subscribe(OnBallsUpdated);

            StartCommand = new RelayCommand(
                execute: _ => StartSimulation(),
                canExecute: _ => !IsRunning && BallCount > 0
            );

            StopCommand = new RelayCommand(
                execute: _ => StopSimulation(),
                canExecute: _ => IsRunning
            );
        }




        public void UpdateBoardSize(double width, double height)
        {
            BoardWidth = width;
            BoardHeight = height;

            if (IsRunning)
                _logic.StartSimulation(BoardWidth, BoardHeight);
        }


        private void StartSimulation()
        {
            _logic.CreateBalls(BallCount, BoardWidth, BoardHeight);
            _logic.StartSimulation(BoardWidth, BoardHeight);
            IsRunning = true;
        }

        private void StopSimulation()
        {
            _logic.StopSimulation();
            IsRunning = false;
            Application.Current?.Dispatcher.Invoke(Balls.Clear);
        }


        private readonly Dictionary<int, BallViewModel> _ballMap = new Dictionary<int, BallViewModel>();

        private void OnBallsUpdated(System.Collections.Generic.IEnumerable<IBallDto> dtos)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                foreach (var dto in dtos)
                {
                    if (!_ballMap.TryGetValue(dto.Id, out var bvm))
                    {
                        bvm = new BallViewModel { Id = dto.Id };
                        _ballMap[dto.Id] = bvm;
                        Balls.Add(bvm);
                    }

                    bvm.X = dto.X;
                    bvm.Y = dto.Y;
                    bvm.Diameter = dto.Radius * 2.0;
                }
            });
        }
    }
}