using System.Windows;
using System.Windows.Controls;
using WpfAppBall.ViewModel;

namespace WpfAppBall
{
    /// <summary>
    /// Code-behind MainWindow.
    ///
    /// ZASADA MVVM: Ten plik zawiera WYŁĄCZNIE:
    ///   1) Automatycznie generowany przez VS kod (InitializeComponent)
    ///   2) Obsługę SizeChanged Canvas - jedyną operację niemożliwą przez DataBinding,
    ///      bo Canvas.ActualWidth/Height nie są dostępne w designerze przy starcie.
    ///
    /// Cała logika biznesowa i sterowanie jest w MainViewModel.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Przekazuje aktualny rozmiar planszy do ViewModelu gdy Canvas zmienia rozmiar.
        /// To jedyne uzasadnione użycie code-behind poza InitializeComponent.
        /// </summary>
        private void GameCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is Canvas canvas)
            {
                vm.UpdateBoardSize(canvas.ActualWidth, canvas.ActualHeight);
            }
        }
    }
}