using CdAudioLib.Abstraction;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace CdAudioWpfUi.View
{
    /// <summary>
    /// Interaktionslogik für ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : FluentWindow, IView
    {
        public ExportWindow(INotifyPropertyChanged ViewModel)
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
            => TrAudioListView.UnselectAll();

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
            => TrAudioListView.SelectAll();
    }
}
