using CdAudioLib.Abstraction;
using System.ComponentModel;
using Wpf.Ui.Controls;

namespace CdAudioWpfUi.View
{
    public partial class MainWindow : FluentWindow, IView
    {
        public MainWindow(INotifyPropertyChanged mainViewModel)
        {
            DataContext = mainViewModel;
            InitializeComponent();

        }

        private void wnd_main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
