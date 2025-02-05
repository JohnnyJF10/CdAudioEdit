using CdAudioLib.Abstraction;
using System.ComponentModel;
using Wpf.Ui.Controls;

namespace CdAudioWpfUi.View
{
    /// <summary>
    /// Interaktionslogik für ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : FluentWindow, IView
    {
        public ProgressWindow(INotifyPropertyChanged ViewModel)
        {
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
