using CdAudioLib.Abstraction;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Navigation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace CdAudioWpfUi.View
{
    public partial class SettingsWindow : FluentWindow, IView
    {
        public SettingsWindow(INotifyPropertyChanged settingsViewModel)
        {
            DataContext = settingsViewModel;
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void ChangeThemeButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
            =>  ApplicationThemeManager.Apply(
                ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light 
                ? ApplicationTheme.Dark 
                : ApplicationTheme.Light);
    }
}
