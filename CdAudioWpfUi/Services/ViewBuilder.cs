

using CdAudioLib.Abstraction;
using CdAudioLib.ViewModel;
using CdAudioWpfUi.View;

using System.ComponentModel;

namespace CdAudioWpfUi.Services
{
    class ViewBuilder
    {
        private readonly Dictionary<Type, Func<INotifyPropertyChanged, IView>> _viewCreators =
                     new Dictionary<Type, Func<INotifyPropertyChanged, IView>>
    {
        { typeof(MainViewModel),     viewModel => new MainWindow((MainViewModel)viewModel) },
        { typeof(ProgressViewModel), viewModel => new ProgressWindow((ProgressViewModel)viewModel) },
        { typeof(SettingsViewModel), viewModel => new SettingsWindow((SettingsViewModel)viewModel) },
        { typeof(ExportViewModel),   viewModel => new ExportWindow((ExportViewModel)viewModel) }
    };

        public IView CreateView(INotifyPropertyChanged viewModel)
        {
            if (_viewCreators.TryGetValue(viewModel.GetType(), out var viewCreator))
                return viewCreator(viewModel);
            else
                throw new ArgumentException("No View registered for this ViewModelType.");
        }
    }
}
