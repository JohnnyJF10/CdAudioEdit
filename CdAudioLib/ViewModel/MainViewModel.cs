/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Model;
using CdAudioLib.Extensions;
using CdAudioLib.Abstraction;
using CdAudioLib.CdAudio;
using CdAudioLib.Utils;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace CdAudioLib.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string _cdAudioFilePath = "";
        private bool _isTextBoxFocused;
        private int _selectedIndex;
        private ObservableTrAudio _selectedTrAudio = new();

        private readonly Func<INotifyPropertyChanged, IView> _getViewCallback;
        private readonly Action<TrAudioMessage, object?> _showMessageCallback;
        private readonly Func<YesNoCancel> _saveChangesCallback;

        private readonly IFileService _cdAudioFileService;
        private readonly IClipboardService _clipboardService;

        private readonly UndoRedoManager _undoRedoManager = new();
        private readonly CdAudioFileManager _cdAudioFileManager = new();
        private readonly AudioPlayer _audioPlayer = new();
        private readonly FileAdmissionManager _fileAdmissionManager = new();
        private readonly TrAudioHelper _trAudioHelper = new();

        private readonly TrAudioConverter _converter;
        private readonly SettingsViewModel _settingsViewModel;

        #endregion

        #region Collections

        public ObservableCollection<ListableTrAudio> TrAudios { get; set; } = new();

        #endregion

        #region Properties

        public string ExportFolder
        {
            get => _converter.ExportDir;
            set => _converter.ExportDir = value;
        }

        public bool AddWavExtension
        {
            get => _cdAudioFileManager.AddWavExtension;
            set => _cdAudioFileManager.AddWavExtension = value;
        }

        public bool StripExtensionDuringImport
        {
            get => _fileAdmissionManager.StripExtension;
            set => _fileAdmissionManager.StripExtension = value;
        }

        public int ResamplingQuality
        {
            get => _converter.resamplingQuality;
            set => _converter.resamplingQuality = value;
        }

        public int NumOfAdpcmTestSamples
        {
            get => _converter.numOfAdpcmTestSamples;
            set => _converter.numOfAdpcmTestSamples = value;
        }

        public bool FileHasChanges => _undoRedoManager.CanUndo;

        public bool IsRegularTrAudio => !(SelectedTrAudio?.Name is null);

        public bool IsTextBoxFocused
        {
            get => _isTextBoxFocused;
            set
            {
                if (_isTextBoxFocused != value)
                {
                    _isTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTextBoxFocused));
                }
            }
        }

        public ObservableTrAudio SelectedTrAudio
        {
            get => _selectedTrAudio;
            set
            {
                if (!_trAudioHelper.Equals(value,_selectedTrAudio))
                {
                    _selectedTrAudio = value;
                    OnPropertyChanged(nameof(SelectedTrAudio));
                }
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value != _selectedIndex)
                {
                    if (value < 0 || value >= CdAudioConstants.CD_AUDIO_CAPACITY)
                        return;
                    _selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                    RefreshSelectedTrAudio();
                }
            }
        }

        #endregion

        #region Commands and Public Methods & Tasks

        private RelayCommand<ITrAudio>? _playCommand;
        public RelayCommand<ITrAudio> PlayCommand
            => _playCommand ??= new RelayCommand<ITrAudio>(PlayFromSystem);
        public void PlayFromSystem(ITrAudio trAudio)
            => _audioPlayer.PlayFromSystem(trAudio);


        private RelayCommand? _newCommand;
        public RelayCommand NewCommand
            => _newCommand ??= new RelayCommand(async _ => await New());
        public async Task New()
        {
            if (_undoRedoManager.CanUndo)
            {
                YesNoCancel result = _saveChangesCallback();
                switch (result)
                {
                    case YesNoCancel.Yes:
                        await Save(inputTrAudios: TrAudios.Select(x => _trAudioHelper.CloneTrAudio(x)).ToList(),
                            TrAudiosToUpdate: false);
                        break;
                    case YesNoCancel.No: break;
                    case YesNoCancel.Cancel: return;
                }
            }

            for (int i = 0; i < CdAudioConstants.CD_AUDIO_CAPACITY; i++)
                _trAudioHelper.RenewTrAudio(TrAudios[i]);
            _undoRedoManager.Clear();
            PasteCommand.RaiseCanExecuteChanged();
        }


        private RelayCommand? _openCommand;
        public RelayCommand OpenCommand
            => _openCommand ??= new RelayCommand(async _ => await Open());
        public async Task Open(string? fileName = null)
        {
            if (_undoRedoManager.CanUndo)
            {
                YesNoCancel result = _saveChangesCallback();
                switch (result)
                {
                    case YesNoCancel.Yes:
                        await Save(inputTrAudios: TrAudios.Select(x => _trAudioHelper.CloneTrAudio(x)).ToList(),
                            TrAudiosToUpdate: false);
                        break;
                    case YesNoCancel.No: break;
                    case YesNoCancel.Cancel: return;
                }
            }

            if (_cdAudioFileService.OpenFileDialog(FileTypes.CdAudioWad) == true && _cdAudioFileService.SelectedPath is string filePath)
            {
                var OpenList = new List<ListableTrAudio>();
                try
                {
                    var openListTask = Task.Run(() => _cdAudioFileManager.Read<ListableTrAudio>(filePath));
                    OpenList = await openListTask;
                }
                catch (FileLoadException ex)
                {
                    ShowMessage(TrAudioMessage.FileLoadExceptionMessage);
                    LogError(ex);
                    return;
                }
                catch (Exception ex)
                {
                    ShowMessage(TrAudioMessage.FileLoadOtherExceptionMessage);
                    LogError(ex);
                    return;
                }
                UpdateTrAudios(OpenList);

                ShowMessage(TrAudioMessage.FileLoadSuccessMessage);
                _cdAudioFilePath = filePath;
                _undoRedoManager.Clear();
                PasteCommand.RaiseCanExecuteChanged();
                RefreshSelectedTrAudio();
            }
        }


        private RelayCommand? _saveCommand;
        public RelayCommand SaveCommand
            => _saveCommand ??= new RelayCommand(async _ => await Save(_cdAudioFilePath));

        private RelayCommand? _saveAsCommand;
        public RelayCommand SaveAsCommand
            => _saveAsCommand ??= new RelayCommand(async _ => await Save());
        public async Task Save(string? fileName = null, bool TrAudiosToUpdate = true, IList<ITrAudio>? inputTrAudios = null)
        {
            if (String.IsNullOrEmpty(fileName))
                if (_cdAudioFileService.SaveFileDialog(FileTypes.CdAudioWad) == true && _cdAudioFileService.SelectedPath is string filePath)
                    fileName = filePath;
                else
                    return;

            var errorEntryList = new List<int>();
            var progressViewModel =
                new ProgressViewModel
                (
                    maxP: CdAudioConstants.CD_AUDIO_CAPACITY,
                    cancelCallback: () => _cdAudioFileManager.IsCanceled = true
                );
            var progessView = _getViewCallback(progressViewModel);
            progessView.Show();

            inputTrAudios ??= TrAudios.Select(x => _trAudioHelper.CloneTrAudio(x)).ToList();
            List<ITrAudio> resList = new();

            try
            {
                var GetSaveFileBuiltTask = Task.Run(() => resList = _cdAudioFileManager.Write
                (
                    fileName: fileName,
                    trAudios: inputTrAudios,
                    converter: _converter,
                    setProgress: value => progressViewModel.progress = value
                ));
                await GetSaveFileBuiltTask;
            }
            catch (OperationCanceledException ex)
            {
                ShowMessage(TrAudioMessage.FileSaveOperationCancelledMessage);
                LogError(ex);
                return;
            }
            catch (Exception ex)
            {
                ShowMessage(TrAudioMessage.FileSaveOtherExceptionMessage);
                LogError(ex);
                return;
            }


            if (TrAudiosToUpdate)
            {
                for (int i = 0; i < CdAudioConstants.CD_AUDIO_CAPACITY; i++)
                {
                    if (TrAudios[i].Name != null && resList[i] == null)
                        errorEntryList.Add(i);
                    else
                        _trAudioHelper.UpdateTrAudioFromOther(TrAudios[i], resList[i]);
                }
            }

            if (errorEntryList.Count > 0)
                ShowMessage(TrAudioMessage.FileSavePartiallySuccessMessage, errorEntryList);
            else
                ShowMessage(TrAudioMessage.FileSaveSuccessMessage);

            _cdAudioFilePath = fileName;

            _undoRedoManager.Clear();
            PasteCommand.RaiseCanExecuteChanged();

            RefreshSelectedTrAudio();

            progessView.Hide();
            await Task.Delay(1000);
            progessView.Close();
        }


        private RelayCommand? _undoCommand;
        public RelayCommand UndoCommand
            => _undoCommand ?? (_undoCommand = new RelayCommand(
                 Undo, _ => _undoRedoManager.CanUndo));
        public void Undo() => _undoRedoManager.Undo();


        private RelayCommand? _redoCommand;
        public RelayCommand RedoCommand
            => _redoCommand ?? (_redoCommand = new RelayCommand(
                Redo, _ => _undoRedoManager.CanRedo));
        public void Redo() => _undoRedoManager.Redo();


        private RelayCommand? _cutCommand;
        public RelayCommand CutCommand
            => _cutCommand ??= new RelayCommand(Cut, _ => IsRegularTrAudio);
        public void Cut()
        {
            _clipboardService.SetData("TrAudio", SelectedTrAudio);
            ExecuteUndoRedoChange(_selectedIndex, _trAudioHelper.CloneTrAudio(SelectedTrAudio), new TrAudio());
            OnPropertyChanged(nameof(IsRegularTrAudio));
        }


        private RelayCommand? _copyCommand;
        public RelayCommand CopyCommand
            => _copyCommand ??= new RelayCommand(Copy, _ => IsRegularTrAudio);
        public void Copy()
        {
            _clipboardService.SetData("TrAudio", SelectedTrAudio);
        }


        private RelayCommand? _pasteCommand;
        public RelayCommand PasteCommand
            => _pasteCommand ??= new RelayCommand(Paste);
        public void Paste()
        {
            if (_clipboardService.GetData("TrAudio") is ITrAudio trAudioFromClipboard)
            {
                ExecuteUndoRedoChange(_selectedIndex, _trAudioHelper.CloneTrAudio(SelectedTrAudio), trAudioFromClipboard);
                OnPropertyChanged(nameof(IsRegularTrAudio));
            }
        }


        private RelayCommand? _pasteSwapCommand;
        public RelayCommand PasteSwapCommand
            => _pasteSwapCommand ??= new RelayCommand(PasteSwap);
        public void PasteSwap()
        {
            if (_clipboardService.GetData("TrAudio") is ITrAudio trAudioFromClipboard)
            {
                _clipboardService.SetData("TrAudio", SelectedTrAudio);
                ExecuteUndoRedoChange(_selectedIndex, _trAudioHelper.CloneTrAudio(SelectedTrAudio), trAudioFromClipboard);
                OnPropertyChanged(nameof(IsRegularTrAudio));
            }
        }


        private RelayCommand? _deleteCommand;
        public RelayCommand DeleteCommand
            => _deleteCommand ??= new RelayCommand(Delete, _ => IsRegularTrAudio);
        public void Delete()
        {
            ExecuteUndoRedoChange(_selectedIndex, _trAudioHelper.CloneTrAudio(SelectedTrAudio), new TrAudio());
            OnPropertyChanged(nameof(IsRegularTrAudio));
        }


        private RelayCommand? _exportSelectedCommand;
        public RelayCommand ExportSelectedCommand
            => _exportSelectedCommand ??= new RelayCommand(ExportSelected);
        public void ExportSelected()
        {
            if (_cdAudioFileService.SaveFileDialog(
                    types: FileTypes.Wav | FileTypes.Mp3, 
                    InitDir: ExportFolder,
                    Title: "Export as..."  ) == true 
                    && _cdAudioFileService.SelectedPath is string filePath)
            {
                try
                {
                    _converter.Export(TrAudios[SelectedIndex], OutputFilePath: filePath);
                }
                catch (NotSupportedException)
                {
                    ShowMessage(TrAudioMessage.ExportNotSupportedMessage);
                    return;
                }
                catch (NotImplementedException)
                {
                    ShowMessage(TrAudioMessage.ExportNotImplementedMessage);
                    return;
                }
                catch (Exception ex)
                {
                    ShowMessage(TrAudioMessage.ExportOtherExceptionMessage);
                    LogError(ex);
                    return;
                }
                ShowMessage(TrAudioMessage.ExportSuccessMessage);
            }
        }


        private RelayCommand? _importSelectedCommand;
        public RelayCommand ImportSelectedCommand
            => _importSelectedCommand ??= new RelayCommand(ImportSelected);
        public void ImportSelected()
        {
            if (_cdAudioFileService.OpenFileDialog(
                    types: FileTypes.Wav | FileTypes.Mp3 | FileTypes.Ogg, 
                    Title: "Select Audio File") == true 
                    && _cdAudioFileService.SelectedPath is string filePath)
            {
                ReplaceSelectedTrAudio(filePath);
            }
        }


        private RelayCommand<string[]>? _fileDropCommand;
        public RelayCommand<string[]> FileDropCommand
            => _fileDropCommand ??= new RelayCommand<string[]>(
                filepaths => ReplaceSelectedTrAudio(filepaths[0]));
        public void ReplaceSelectedTrAudio(string DropFilePath)
        {
            string Extension = Path.GetExtension(DropFilePath);
            TrAudio admittedTrAudio;

            try
            {
                admittedTrAudio = _fileAdmissionManager.GetTrAudioFromFile(DropFilePath);
            }
            catch (NotSupportedException)
            {
                ShowMessage(TrAudioMessage.ImportNotSupportedMessage, Extension);
                return;
            }
            catch (Exception ex)
            {
                ShowMessage(TrAudioMessage.ImportOtherExceptionMessage, Extension);
                LogError(ex);
                return;
            }
            ExecuteUndoRedoChange(_selectedIndex, _trAudioHelper.CloneTrAudio(SelectedTrAudio), admittedTrAudio);
            OnPropertyChanged(nameof(IsRegularTrAudio));
        }


        private RelayCommand<(int, DragDropMode)>? _intrinsicDropCommand;
        public RelayCommand<(int, DragDropMode)> IntrinsicDropCommand
            => _intrinsicDropCommand ??= new RelayCommand<(int, DragDropMode)>(
                 parameters => ReplaceSelectedTrAudio(parameters.Item1, dragDropMode: parameters.Item2));
        public void ReplaceSelectedTrAudio(int sourceTrAudioIndex, int targetTrAudioIndex = -1, DragDropMode dragDropMode = DragDropMode.Swap)
        {
            if (targetTrAudioIndex == -1) targetTrAudioIndex = _selectedIndex;
            if (sourceTrAudioIndex == targetTrAudioIndex || TrAudios[sourceTrAudioIndex].Name == null) return;

            var sourceChangedTrAudio = dragDropMode switch
            {
                DragDropMode.Move => new TrAudio(),
                DragDropMode.Copy => _trAudioHelper.CloneTrAudio(TrAudios[sourceTrAudioIndex]),
                DragDropMode.Swap => _trAudioHelper.CloneTrAudio(TrAudios[targetTrAudioIndex]),
                _ => throw new ArgumentOutOfRangeException(nameof(dragDropMode))
            };

            ExecuteUndoRedoChange(
                _selectedIndex,
                _trAudioHelper.CloneTrAudio(TrAudios[targetTrAudioIndex]),
                _trAudioHelper.CloneTrAudio(TrAudios[sourceTrAudioIndex]),
                sourceTrAudioIndex,
                _trAudioHelper.CloneTrAudio(TrAudios[sourceTrAudioIndex]),
                sourceChangedTrAudio);
        }



        private RelayCommand<(int, DragDropMode)>? _intrinsicQuickConverCommand;
        public RelayCommand<(int, DragDropMode)> IntrinsicQuickConvertCommand
            => _intrinsicQuickConverCommand ??= new RelayCommand<(int, DragDropMode)>(
                parameters => ExportSingle(TrAudios[parameters.Item1]));
        public void ExportSingle(ITrAudio trAudio)
        {
            EnsureExportFolderExists();
            _converter.Export(trAudio);
        }


        private RelayCommand<string[]>? _quickConvertCommand;
        public RelayCommand<string[]> QuickConvertCommand
            => _quickConvertCommand ??= new RelayCommand<string[]>(async filepaths 
                => await QuickCovert(filepaths));
        public async Task QuickCovert(string[] filepaths)
        {
            EnsureExportFolderExists();
            bool isCancelled = false;
            var progressViewModel =
                new ProgressViewModel(filepaths.Length, () => isCancelled = true);
            var progressView = _getViewCallback(progressViewModel);
            List<string> failedFiles = new();

            progressView.Show();
            await Task.Run(() =>
            {
                foreach (var item in filepaths)
                {
                    try
                    {
                        if (isCancelled) throw new OperationCanceledException();
                        _converter.QuickConvert(item);
                        progressViewModel.progress++;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        failedFiles.Add(Path.GetFileName(item));
                        LogError(ex);
                        progressViewModel.progress++;
                    }
                }
            });

            if (failedFiles.Count > 0)
                ShowMessage(TrAudioMessage.QuickConvertPartiallySuccessMessage, failedFiles);
            else
                ShowMessage(TrAudioMessage.QuickConvertSuccessMessage);

            progressView.Hide();
            await Task.Delay(1000);
            progressView.Close();
        }


        private RelayCommand? _openExportFolderCommand;
        public RelayCommand OpenExportFolderCommand
            => _openExportFolderCommand ??= new RelayCommand(OpenExportFolder);
        public void OpenExportFolder()
        {
            EnsureExportFolderExists();
            Process.Start(new ProcessStartInfo("explorer.exe", ExportFolder) { UseShellExecute = true });
        }


        private RelayCommand? _exportCommand;
        public RelayCommand ExportCommand
            => _exportCommand ??= new RelayCommand(async _ => await ExportWithDialog());
        public async Task ExportWithDialog()
        {
            var exportViewModel = new ExportViewModel(
                fileService: _cdAudioFileService,
                trAudioNames: TrAudios
                    .Select((audio, index) => new { audio.Name, IsIncluded = Path.GetExtension(audio.FilePath) == ".wad", Index = index })
                    .Where(x => x.Name != null && x.IsIncluded)
                    .Select(x => new TrAudioName(Name: x.Name, slot: x.Index)),
                selectedPath: ExportFolder
                );
            var exportView = _getViewCallback(exportViewModel);
            if (exportView.ShowDialog() == true)
            {
                EnsureExportFolderExists();
                await ExportList(exportViewModel.ResultList, exportViewModel.SelectedFormat);
            }
        }
        public async Task ExportList(IList<int> slots, ExportAudioFormat format)
        {
            bool isCancelled = false;
            List<int> failedSlots = new();
            var progressViewModel =
                new ProgressViewModel(
                    maxP: slots.Count,
                    cancelCallback: () => isCancelled = true);
            var progressView = _getViewCallback(progressViewModel);

            progressView.Show();
            await Task.Run(() =>
            {
                foreach (var item in slots)
                {
                    try
                    {
                        if (isCancelled) throw new OperationCanceledException();
                        _converter.Export(TrAudios[item], format);
                        progressViewModel.progress++;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        failedSlots.Add(item);
                        LogError(ex);
                        progressViewModel.progress++;
                    }
                }
            });

            if (failedSlots.Count > 0)
                ShowMessage(TrAudioMessage.ExportListPartiallySuccessMessage, failedSlots);
            else
                ShowMessage(TrAudioMessage.ExportListSuccessMessage);

            progressView.Hide();
            await Task.Delay(1000);
            progressView.Close();
        }


        private RelayCommand? settingsCommand;
        public RelayCommand SettingsCommand =>
            settingsCommand ??= new RelayCommand(Settings);
        public void Settings()
        {
            var _settingsView = _getViewCallback(_settingsViewModel);
            if (_settingsView.ShowDialog() == true)
            {
                StripExtensionDuringImport = _settingsViewModel.StripExtension;
                AddWavExtension = _settingsViewModel.AddWavExtension;
                ResamplingQuality = _settingsViewModel.nAudioResamplingQuality;
                NumOfAdpcmTestSamples = _settingsViewModel.numOfAdpcmEncoderTestSamples;
                ExportFolder = _settingsViewModel.ExportFolder;
            }
        }

        #endregion

        #region Private Methods

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (IsTextBoxFocused
                && sender is ObservableTrAudio item
                && e.PropertyName == nameof(item.Name))
                ExecuteUndoRedoChange(
                    _selectedIndex, 
                    _trAudioHelper.CloneTrAudio(TrAudios[_selectedIndex]), 
                    _trAudioHelper.CloneTrAudio(_selectedTrAudio));
        }

        // Important: parameters ListableTrAudio originalCloned and ListableTrAudio newCloned must be new cloned instances, otherwise the undo/redo will not work correctly
        private void ExecuteUndoRedoChange(int index, ITrAudio originalCloned, ITrAudio newCloned)
            => _undoRedoManager.ExecuteChange(
                () => UndoRedoApplyChange(index, newCloned),  // Redo action
                () => UndoRedoApplyChange(index, originalCloned)); // Undo action

        private void ExecuteUndoRedoChange(int index1, ITrAudio originalCloned1, ITrAudio newCloned1, int index2, ITrAudio originalCloned2, ITrAudio newCloned2)
            => _undoRedoManager.ExecuteChange(
                () => UndoRedoApplyChange(index1, newCloned1),  // Redo action1
                () => UndoRedoApplyChange(index1, originalCloned1), // Undo action1
                () => UndoRedoApplyChange(index2, newCloned2),  // Redo action2
                () => UndoRedoApplyChange(index2, originalCloned2)); // Undo action2


        private void OnUndoRedoManagerStateChanged()
        {
            UndoCommand?.RaiseCanExecuteChanged();
            RedoCommand?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(FileHasChanges));
        }

        private void UndoRedoApplyChange(int indexOfChanged, ITrAudio ChangedTrAudio)
        {
            _trAudioHelper.UpdateTrAudioFromOther(TrAudios[indexOfChanged], ChangedTrAudio);
            RefreshSelectedTrAudio();
        }

        private void LogError(Exception ex)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorLog.txt");
            string logMessage = $"{DateTime.Now}: {ex}\n";

            File.AppendAllText(logFilePath, logMessage);
        }

        private void RefreshSelectedTrAudio()
        {
            SelectedTrAudio.UpdateFromOther(TrAudios[_selectedIndex]);
            CustomCommandManager.InvalidateRequerySuggested();
            OnPropertyChanged(nameof(IsRegularTrAudio));
        }

        private void UpdateTrAudios<T>(IList<T> OtherTrAudios) where T : ITrAudio
        {
            for (int i = 0; i < CdAudioConstants.CD_AUDIO_CAPACITY; i++)
                _trAudioHelper.UpdateTrAudioFromOther(TrAudios[i], OtherTrAudios[i]);
        }

        private void EnsureExportFolderExists()
        {
            if (!Directory.Exists(ExportFolder))
            {
                Directory.CreateDirectory(ExportFolder);
            }
        }

        private void DeleteExistingTempFiles()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var tempFiles = Directory.GetFiles(appDirectory, "*~temp.wav", SearchOption.TopDirectoryOnly)
                                     .Where(f => (File.GetAttributes(f) & FileAttributes.Hidden) == FileAttributes.Hidden);

            foreach (var tempFile in tempFiles)
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        private void ShowMessage(TrAudioMessage message, object? parameter = null)
        {
            _showMessageCallback(message, parameter);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Constructors

        public MainViewModel(
            Func<INotifyPropertyChanged, IView> getViewCallback,
            Action<TrAudioMessage, object?> showMessageCallback,
            Func<YesNoCancel> saveChangesCallback,
            IFileService cdAudioFileService,
            IClipboardService clipboardService)
        {
            _getViewCallback = getViewCallback;
            _saveChangesCallback = saveChangesCallback;
            //_messageManager = new MessageManager(showMessageCallback);
            _showMessageCallback = showMessageCallback;
            _cdAudioFileService = cdAudioFileService;
            _clipboardService = clipboardService;
            _settingsViewModel = new SettingsViewModel(_cdAudioFileService);
            _converter = new TrAudioConverter(_settingsViewModel.ExportFolder);

            for (int i = 0; i < CdAudioConstants.CD_AUDIO_CAPACITY; i++)
                TrAudios.Add(new ListableTrAudio());

            SelectedTrAudio.PropertyChanged += Item_PropertyChanged;


#if DEBUG
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\cdaudio.wad"))
            {
                _cdAudioFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\cdaudio.wad";
                var DebugLoadList = _cdAudioFileManager.Read<ListableTrAudio>(_cdAudioFilePath);
                PasteCommand.RaiseCanExecuteChanged();
            }
#endif
            _undoRedoManager.StateChanged += OnUndoRedoManagerStateChanged;
        }

        #endregion

    }
}

