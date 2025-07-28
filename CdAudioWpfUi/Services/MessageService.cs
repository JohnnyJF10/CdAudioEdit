using CdAudioLib.Abstraction;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace CdAudioWpfUi.Services
{
    public class MessageService
    {
        private readonly SnackbarService snackbarService;

        public MessageService(SnackbarService snackbarService)
        {
            this.snackbarService = snackbarService;
        }

        public void ShowMessage(TrAudioMessage trAudioMessage, object? MessageArgs = null)
        {
            switch (trAudioMessage)
            {
                case TrAudioMessage.FileLoadExceptionMessage:
                    snackbarService.Show(
                        title: "File Load Exception",
                        message: "An exception occurred while loading the file. Wrong file format?",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.FileLoadOtherExceptionMessage:
                    snackbarService.Show(
                        title: "File Load Unknown Exception",
                        message: "An exception occurred while loading the file. Find ErrorLog.txt for more details",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.FileLoadSuccessMessage:
                    snackbarService.Show(
                        title: "File Loaded",
                        message: "File loaded successfully.",
                        appearance: ControlAppearance.Success,
                        icon: new SymbolIcon(SymbolRegular.ApprovalsApp24),
                        timeout: TimeSpan.FromSeconds(3));
                    break;
                case TrAudioMessage.FileSaveOperationCancelledMessage:
                    snackbarService.Show(
                        title: "File Save Cancelled",
                        message: "The file save operation was cancelled.",
                        appearance: ControlAppearance.Info,
                        icon: new SymbolIcon(SymbolRegular.Info24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.FileSaveOtherExceptionMessage:
                    snackbarService.Show(
                        title: "File Save Unknown Exception",
                        message: "An exception occurred while saving the file. Find ErrorLog.txt for more details",
                        appearance: ControlAppearance.Caution,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.FileSavePartiallySuccessMessage:
                    if (MessageArgs is List<int> errorEntryList)
                        snackbarService.Show(
                            title: "File Save Partially Successful",
                            message: "The file was saved with some issues. The following entries could not be saved: " +
                                string.Join(", ", errorEntryList) +
                                ". Please finde more Information in the ErrorLog.txt",
                            appearance: ControlAppearance.Caution,
                            icon: new SymbolIcon(SymbolRegular.Warning24),
                            timeout: TimeSpan.FromSeconds(10));
                    else
                        snackbarService.Show(
                            title: "File Save Partially Successful",
                            message: "The file was saved with some issues. Please finde more Information in the ErrorLog.txt",
                            appearance: ControlAppearance.Caution,
                            icon: new SymbolIcon(SymbolRegular.Warning24),
                            timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.FileSaveSuccessMessage:
                    snackbarService.Show(
                        title: "File Saved",
                        message: "The file was saved successfully.",
                        appearance: ControlAppearance.Success,
                        icon: new SymbolIcon(SymbolRegular.ApprovalsApp24),
                        timeout: TimeSpan.FromSeconds(3));
                    break;
                case TrAudioMessage.ExportNotSupportedMessage:
                    snackbarService.Show(
                        title: "Export Not Supported",
                        message: "Exporting is not supported for this file.",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ExportNotImplementedMessage:
                    snackbarService.Show(
                        title: "Export Not Implemented",
                        message: "Exporting is not implemented for ogg files.",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ExportOtherExceptionMessage:
                    snackbarService.Show(
                        title: "Export Unknown Exception",
                        message: "An exception occurred while exporting the file. Find ErrorLog.txt for more details",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ExportSuccessMessage:
                    snackbarService.Show(
                        title: "Export Successful",
                        message: "The file was exported successfully.",
                        appearance: ControlAppearance.Success,
                        icon: new SymbolIcon(SymbolRegular.ApprovalsApp24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ExportListPartiallySuccessMessage:
                    if (MessageArgs is List<int> failedSlots)
                        snackbarService.Show(
                            title: "Export List Partially Successful",
                            message: "The file list was exported with some issues. The following slots could not be exported: " +
                                string.Join(", ", failedSlots) +
                                ". Please finde more Information in the ErrorLog.txt",
                            appearance: ControlAppearance.Caution,
                            icon: new SymbolIcon(SymbolRegular.Warning24),
                            timeout: TimeSpan.FromSeconds(10));
                    else
                        snackbarService.Show(
                            title: "Export List Partially Successful",
                            message: "The file list was exported with some issues. Please finde more Information in the ErrorLog.txt",
                            appearance: ControlAppearance.Caution,
                            icon: new SymbolIcon(SymbolRegular.Warning24),
                            timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ExportListSuccessMessage:
                    snackbarService.Show(
                        title: "Export List Successful",
                        message: "The file list was exported successfully.",
                        appearance: ControlAppearance.Success,
                        icon: new SymbolIcon(SymbolRegular.ApprovalsApp24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ImportNotSupportedMessage:
                    snackbarService.Show(
                        title: "Import Not Supported",
                        message: "Importing is not supported for this file.",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ImportNotImplementedMessage:
                    snackbarService.Show(
                        title: "Import Not Implemented",
                        message: "Importing is not implemented for ogg files.",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ImportOtherExceptionMessage:
                    snackbarService.Show(
                        title: "Import Unknown Exception",
                        message: "An exception occurred while importing the file. Find ErrorLog.txt for more details",
                        appearance: ControlAppearance.Danger,
                        icon: new SymbolIcon(SymbolRegular.ErrorCircle24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.ImportSuccessMessage:
                    snackbarService.Show(
                        title: "Import Successful",
                        message: "The file was imported successfully.",
                        appearance: ControlAppearance.Success,
                        icon: new SymbolIcon(SymbolRegular.ApprovalsApp24),
                        timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.QuickConvertPartiallySuccessMessage:
                    if (MessageArgs is List<string> failedFiles)
                        snackbarService.Show(
                            title: "Quick Convert Partially Successful",
                            message: "The file list was converted with some issues. The following slots could not be converted: " +
                                string.Join(", ", failedFiles) +
                                ". Please finde more Information in the ErrorLog.txt",
                            appearance: ControlAppearance.Caution,
                            icon: new SymbolIcon(SymbolRegular.Warning24),
                            timeout: TimeSpan.FromSeconds(10));
                    else
                        snackbarService.Show(
                            title: "Quick Convert Partially Successful",
                            message: "The file list was converted with some issues. Please finde more Information in the ErrorLog.txt",
                            appearance: ControlAppearance.Caution,
                            icon: new SymbolIcon(SymbolRegular.Warning24),
                            timeout: TimeSpan.FromSeconds(10));
                    break;
                case TrAudioMessage.QuickConvertSuccessMessage:
                    snackbarService.Show(
                        title: "Quick Convert Successful",
                        message: "The file list was converted successfully.",
                        appearance: ControlAppearance.Success,
                        icon: new SymbolIcon(SymbolRegular.ApprovalsApp24),
                        timeout: TimeSpan.FromSeconds(3));
                    break;
                default:
                    break;
            }
        }
    }
}
