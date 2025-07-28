

using CdAudioLib.Abstraction;
using CdAudioLib.Extensions;
using CdAudioLib.Model;

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CdAudioLib.ViewModel
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        private string _exportPath = AppDomain.CurrentDomain.BaseDirectory;
        private IFileService? _fileService;

        public ObservableCollection<TrAudioName> TrAudioNames { get; set; } = new();
        public List<int> ResultList { get; set; } = new();

        public string ExportPath
        {
            get => _exportPath;
            set
            {
                if (_exportPath != value)
                {
                    _exportPath = value;
                    OnPropertyChanged(nameof(ExportPath));
                }
            }
        }

        private ExportAudioFormat _selectedFormat;

        public ExportAudioFormat SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                if (_selectedFormat != value)
                {
                    _selectedFormat = value;
                    OnPropertyChanged(nameof(SelectedFormat));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        private RelayCommand? _okCommand;
        public RelayCommand OkCommand =>
            _okCommand ??= new RelayCommand(OnOK);

        private void OnOK(object? parameterTuple)
        {
            if (parameterTuple is (IView view, ICollection ResultCollection))
            {
                foreach (var result in ResultCollection)
                    if (result is TrAudioName SelectedEntry)
                        ResultList.Add(SelectedEntry.slot);
                    else throw new InvalidCastException("Element is no TrAudioName");

                view.DialogResult = true;
                view.Close();
            }
        }

        private RelayCommand<IView>? _cancelCommand;
        public RelayCommand<IView> CancelCommand =>
            _cancelCommand ??= new RelayCommand<IView>(OnCancel);

        private void OnCancel(IView view)
        {
            view.DialogResult = false;
            view.Close();
        }

        private RelayCommand? _selectFolderCommand;
        public RelayCommand SelectFolderCommand => _selectFolderCommand ??= new RelayCommand(SelectFolder);

        private void SelectFolder()
        {
            if (_fileService == null) throw new InvalidOperationException("File service not set");
            if (_fileService.SelectFolderDialog() == true)
            {
                ExportPath = _fileService.SelectedPath + "\\";
            }
        }

        public ExportViewModel(IFileService fileService, IEnumerable<TrAudioName> trAudioNames, string selectedPath)
        {
            _fileService = fileService;
            TrAudioNames = new ObservableCollection<TrAudioName>(trAudioNames);
            ExportPath = selectedPath;
        }
    }
}
