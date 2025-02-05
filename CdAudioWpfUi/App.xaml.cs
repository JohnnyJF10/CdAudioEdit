/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Abstraction;
using CdAudioLib.ViewModel;
using CdAudioWpfUi.Services;

using System.Windows;

using Wpf.Ui.Controls;
using Wpf.Ui;

using Application = System.Windows.Application;

namespace CdAudioWpfUi
{
    public partial class App : Application
    {
        private readonly ViewBuilder _viewBuilder = new();

        private SnackbarService _snackbarService = new();

        private MessageService? _messageService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _messageService = new MessageService(_snackbarService);

            var mainViewModel = new MainViewModel(
                getViewCallback: _viewBuilder.CreateView,
                showMessageCallback: _messageService.ShowMessage,
                saveChangesCallback: SaveChanges,
                cdAudioFileService: new CdAudioFileDialog(),
                clipboardService: new ClipboardService()
                );
            MainWindow = (Window)_viewBuilder.CreateView(mainViewModel);

            if (MainWindow is Window window && window.FindName("msg_snackbar") is SnackbarPresenter snackbarPresenter)
            {
                _snackbarService.SetSnackbarPresenter(snackbarPresenter);
                window.Show();
            }
            else
            {
                throw new InvalidOperationException("Die erstellte View ist kein Fenster.");
            }
        }

        protected YesNoCancel SaveChanges()
        {
            var result = System.Windows.Forms.MessageBox.Show
                ("Do you want to save the current file?", 
                "Save current file", 
                MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes: return YesNoCancel.Yes;
                case DialogResult.No: return YesNoCancel.No;
                case DialogResult.Cancel: return YesNoCancel.Cancel;
                default: return YesNoCancel.Cancel;
            }
        }
    }
}
