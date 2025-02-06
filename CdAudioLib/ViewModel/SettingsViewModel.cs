/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Abstraction;
using CdAudioLib.Extensions;

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace CdAudioLib.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private const string SettingsFilePath = "settings.json";

        private bool _stripExtension = true;
        public bool StripExtension
        {
            get => _stripExtension;
            set
            {
                if (value != _stripExtension)
                {
                    _stripExtension = value;
                    OnPropertyChanged(nameof(StripExtension));
                }
            }
        }

        private bool _addWavExtension = true;
        public bool AddWavExtension
        {
            get => _addWavExtension;
            set
            {
                if (value != _addWavExtension)
                {
                    _addWavExtension = value;
                    OnPropertyChanged(nameof(AddWavExtension));
                }
            }
        }

        private string _exportFolder = AppDomain.CurrentDomain.BaseDirectory + "Export\\";
        public string ExportFolder
        {
            get => _exportFolder;
            set
            {
                if (value != _exportFolder)
                {
                    _exportFolder = value;
                    OnPropertyChanged(nameof(ExportFolder));
                }
            }
        }

        private int _nAudioResamplingQuality = 36;
        public int nAudioResamplingQuality
        {
            get => _nAudioResamplingQuality;
            set
            {
                if (value < 1) value = 1;
                else if (value > 60) value = 60;

                if (value != _nAudioResamplingQuality)
                {
                    _nAudioResamplingQuality = value;
                    OnPropertyChanged(nameof(nAudioResamplingQuality));
                }
            }
        }

        private int _numOfAdpcmEncoderTestSamples = 42;
        public int numOfAdpcmEncoderTestSamples
        {
            get => _numOfAdpcmEncoderTestSamples;
            set
            {
                if (value < 1) value = 1;
                else if (value > 60) value = 60;
                if (value != _numOfAdpcmEncoderTestSamples)
                {
                    _numOfAdpcmEncoderTestSamples = value;
                    OnPropertyChanged(nameof(numOfAdpcmEncoderTestSamples));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private RelayCommand<IView>? _okCommand;
        [JsonIgnore]
        public RelayCommand<IView> OkCommand =>
            _okCommand ??= new RelayCommand<IView>(OnOK);

        private RelayCommand<IView>? _cancelCommand;
        [JsonIgnore]
        public RelayCommand<IView> CancelCommand =>
            _cancelCommand ??= new RelayCommand<IView>(OnCancel);

        private void OnCancel(IView view)
        {
            view.DialogResult = false;
            view.Close();
        }

        private void OnOK(IView view)
        {
            SaveSettings();
            view.DialogResult = true;
            view.Close();
        }

        public void LoadSettings()
        {
            try
            {
                var json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<SettingsViewModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (settings != null)
                {
                    ExportFolder = settings.ExportFolder;
                    nAudioResamplingQuality = settings.nAudioResamplingQuality;
                    numOfAdpcmEncoderTestSamples = settings.numOfAdpcmEncoderTestSamples;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsFilePath, json);
        }

        private IFileService? _cdAudioFileManager;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public SettingsViewModel() { }

        public SettingsViewModel(IFileService cdAudioFileManager)
        {
            _cdAudioFileManager = cdAudioFileManager;
            LoadSettings();
        }

        private RelayCommand? _selectExportFolderCommand;
        [JsonIgnore]
        public ICommand SelectExportFolderCommand =>
            _selectExportFolderCommand ??= new RelayCommand(SelectExportFolder);

        private void SelectExportFolder()
        {
            if (_cdAudioFileManager != null && _cdAudioFileManager.SelectFolderDialog() == true)
                ExportFolder = _cdAudioFileManager.SelectedPath + "\\";
        }

        private void LogError(Exception ex)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorLog.txt");
            string logMessage = $"{DateTime.Now}: {ex}\n";

            File.AppendAllText(logFilePath, logMessage);
        }
    }
}
