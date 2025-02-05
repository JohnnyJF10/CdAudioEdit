/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Abstraction;
using System.Text;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace CdAudioWpfUi.Services
{
    public class CdAudioFileDialog : IFileService
    {
        public string SelectedPath { get; set; } = "";
        public bool OpenFileDialog(FileTypes types, string? InitDir = null, string? Title = null)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = GetFilter(types);
            openFileDialog.DefaultExt = GetDefaultExt(types);
            openFileDialog.Title = Title ?? GetTitle(types);
            if (InitDir != null) openFileDialog.InitialDirectory = InitDir;
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPath = openFileDialog.FileName;
                return true;
            }

            return false;
        }

        public bool SaveFileDialog(FileTypes types, string? InitDir = null, string? Title = null)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = GetFilter(types);
            saveFileDialog.DefaultExt = GetDefaultExt(types);
            saveFileDialog.Title = Title ?? GetTitle(types);
            if (InitDir != null) saveFileDialog.InitialDirectory = InitDir;
            if (saveFileDialog.ShowDialog() == true)
            {
                SelectedPath = saveFileDialog.FileName;
                return true;
            }

            return false;
        }

        public bool SelectFolderDialog(string? InitDir = null, string? Title = null)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new();
            folderBrowserDialog.Description = Title ?? "Select a Folder";
            if (InitDir != null) folderBrowserDialog.SelectedPath = InitDir;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = folderBrowserDialog.SelectedPath;
                return true;
            }
            return false;
        }

        private static string GetFilter(FileTypes types)
        {
            StringBuilder filter = new StringBuilder();
            if (types.HasFlag(FileTypes.CdAudioWad))
                filter.Append("WAD Files (*.wad)|*.wad|");
            if (types.HasFlag(FileTypes.Wav))
                filter.Append("WAV Files (*.wav)|*.wav|");
            if (types.HasFlag(FileTypes.Mp3))
                filter.Append("MP3 Files (*.mp3)|*.mp3|");
            if (types.HasFlag(FileTypes.Ogg))
                filter.Append("OGG Files (*.ogg)|*.ogg|");
            filter.Append("All Files (*.*)|*.*");
            return filter.ToString();
        }

        private static string GetDefaultExt(FileTypes types)
        {
            var reducedTypes = ReduceToSingleSelection(types);
            switch (reducedTypes)
            {
                case FileTypes.CdAudioWad: return ".wad";
                case FileTypes.Wav: return ".wav";
                case FileTypes.Mp3: return ".mp3";
                case FileTypes.Ogg: return ".ogg";
                default:  return "";
            }
        }

        private static string GetTitle(FileTypes types)
        {
            var reducedTypes = ReduceToSingleSelection(types);
            switch (reducedTypes)
            {
                case FileTypes.CdAudioWad: return "Select a CdAudio.wad File";
                case FileTypes.Wav: return "Select a WAV File";
                case FileTypes.Mp3: return "Select a MP3 File";
                case FileTypes.Ogg: return "Select a OGG File";
                default: return "";
            }
        }

        private static FileTypes ReduceToSingleSelection(FileTypes types)
        {
            if (types == FileTypes.None) return FileTypes.None;

            foreach (FileTypes type in Enum.GetValues(typeof(FileTypes)).Cast<FileTypes>().Reverse())
                if (types.HasFlag(type)) return type;

            return FileTypes.None;
        }

    }
}
