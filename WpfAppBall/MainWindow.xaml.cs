using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfAppBall
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Upewnij się, że nazwa po prawej stronie jest identyczna 
            // z nazwą Twojej klasy w folderze ViewModel
            this.DataContext = new WpfAppBall.ViewModel.ViewModel();
        }
    }
}