/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

namespace CdAudioLib.Abstraction
{

    public enum TrAudioMessage
    {
        FileLoadExceptionMessage,
        FileLoadOtherExceptionMessage,
        FileLoadSuccessMessage,

        FileSaveOperationCancelledMessage,
        FileSaveOtherExceptionMessage,
        FileSavePartiallySuccessMessage,
        FileSaveSuccessMessage,

        ExportNotSupportedMessage,
        ExportNotImplementedMessage,
        ExportOtherExceptionMessage,
        ExportSuccessMessage,

        ExportListPartiallySuccessMessage,
        ExportListSuccessMessage,

        ImportNotSupportedMessage,
        ImportNotImplementedMessage,
        ImportOtherExceptionMessage,
        ImportSuccessMessage,

        QuickConvertPartiallySuccessMessage,
        QuickConvertSuccessMessage,
    }

    public enum YesNoCancel
    {
        Yes,
        No,
        Cancel
    }   

    public enum StatusType
    {
        Ready,
        Convering,
        Writing,
        Reading
    }

    public enum DragDropMode
    {
        Move,
        Copy,
        Swap,
        None
    }

    public enum ExportAudioFormat
    {
        WAV,
        MP3,
        OGG
    }
}
