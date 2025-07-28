

using CdAudioLib.Model;
using System.Diagnostics;

namespace CdAudioLib.Utils
{
    public class AudioPlayer
    {
        private string _tempAdpcmFilePath = "";

        public AudioPlayer()
        {
            DeleteExistingTempFiles();
        }

        public void PlayFromSystem(ITrAudio trAudio)
        {
            if (String.IsNullOrEmpty(trAudio.Name)) return;
            string? ext = Path.GetExtension(trAudio.FilePath);

            DeleteExistingTempFiles();

            if ((ext == ".wad" || ext == ".WAD") && trAudio.FilePath != null)
            {
                _tempAdpcmFilePath = AppDomain.CurrentDomain.BaseDirectory + trAudio.Name;
                if (Path.GetExtension(_tempAdpcmFilePath) != ".wav")
                    _tempAdpcmFilePath += "~temp.wav";
                else
                    _tempAdpcmFilePath = _tempAdpcmFilePath.Insert(_tempAdpcmFilePath.Length - 4, "~temp");

                try
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Create(_tempAdpcmFilePath)))
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(trAudio.FilePath)))
                    {
                        reader.BaseStream.Position += trAudio.offset;
                        for (int i = 0; i < trAudio.fileSize; i++)
                            writer.Write(reader.ReadByte());
                        reader.Close();
                        writer.Close();
                    }
                }
                catch (IOException) { return; }
                catch (UnauthorizedAccessException) { return; }

                File.SetAttributes(_tempAdpcmFilePath, File.GetAttributes(_tempAdpcmFilePath) | FileAttributes.Hidden);
                Process.Start(new ProcessStartInfo
                {
                    FileName = _tempAdpcmFilePath,
                    UseShellExecute = true
                });
            }
            else if (ext == ".mp3" || ext == ".ogg" || ext == ".OGG" || ext == ".wav" || ext == ".WAV")
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = trAudio.FilePath,
                    UseShellExecute = true
                });
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
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }
            }
        }
    }
}
