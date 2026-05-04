using System.Windows;
using System.Windows.Controls;
using WpfAppBall.ViewModel;

namespace WpfAppBall
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void GameCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is Canvas canvas)
            {
                vm.UpdateBoardSize(canvas.ActualWidth, canvas.ActualHeight);
            }
        }
    }
}