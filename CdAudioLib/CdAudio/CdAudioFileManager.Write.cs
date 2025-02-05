/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Abstraction;
using CdAudioLib.Model;
using CdAudioLib.Utils;

namespace CdAudioLib.CdAudio
{
    public partial class CdAudioFileManager
    {
        public bool AddWavExtension = true;

        public bool IsCanceled = false;

        public List<ITrAudio> Write(string fileName, IList<ITrAudio> trAudios, TrAudioConverter converter, Action<int>? setProgress = null, Action<StatusType>? setStatus = null)
        {
            var results = new List<ITrAudio>();
            MsAdpcm.Encoder encoder = new MsAdpcm.Encoder();
            string? fileDir = Path.GetDirectoryName(fileName);
            if (fileDir == null)
                throw new ArgumentNullException(nameof(fileDir));

            string fileTemp = fileDir + "\\temp.wad";
            string name;

            int factValue = 0;
            int duration = 0;

            long oldPos, newPos;
            if (File.Exists(fileTemp))
            {
                File.SetAttributes(fileTemp, FileAttributes.Normal);
                File.Delete(fileTemp);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Create(fileTemp)))
            {
                File.SetAttributes(fileTemp, File.GetAttributes(fileTemp) | FileAttributes.Hidden);
                for (int i = 0; i < CdAudioConstants.FIRST_WAV_POS; i++)
                    writer.Write('\0');

                for (int i = 0; i < CdAudioConstants.CD_AUDIO_CAPACITY; i++)
                {
                    if (IsCanceled)
                        throw new OperationCanceledException();

                    setProgress?.Invoke(i);


                    var trAudio = trAudios[i];
                    if (trAudio.Name == null)
                    {
                        results.Add(new TrAudio());
                        continue;
                    };

                    oldPos = writer.BaseStream.Position;

                    try
                    {
                        if (trAudio.offset != 0)
                        {
                            using (BinaryReader reader = new BinaryReader(File.OpenRead(trAudio.FilePath)))
                            {
                                reader.BaseStream.Position = trAudio.offset;
                                writer.Write(reader.ReadBytes(trAudio.fileSize));
                                reader.Close();
                                duration = trAudio.duration;
                            }
                        }
                        else if (trAudio.IsOriginalTrAudio)
                        {
                            using (BinaryReader reader = new BinaryReader(File.OpenRead(trAudio.FilePath)))
                            {
                                writer.Write(reader.ReadBytes(trAudio.fileSize));
                                reader.Close();
                            }
                            duration = trAudio.duration;
                        }
                        else
                        {
                            converter.ConvertForCdAudio(trAudio.FilePath, writer, out factValue);
                            duration = (int)(factValue / CdAudioConstants.TR_AUDIO_SAMPLE_RATE);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If an error occurs, write an empty entry to the Collection and restore the writer state
                        results.Add(new TrAudio());

                        int numUnusableBytes = (int)(writer.BaseStream.Position - oldPos);
                        writer.BaseStream.Position = oldPos;
                        for (int j = 0; j < numUnusableBytes; j++)
                            writer.Write('\0');
                        writer.BaseStream.Position = oldPos;
                        LogError(ex);
                        continue;
                    }

                    if (AddWavExtension && !trAudio.Name.EndsWith(".wav"))
                        name = trAudio.Name + ".wav";
                    else name = trAudio.Name;

                    newPos = writer.BaseStream.Position;

                    writer.BaseStream.Position = i * CdAudioConstants.CD_AUDIO_ENTRY_LEN;
                    writer.Write(name.ToCharArray());

                    writer.BaseStream.Position = i * CdAudioConstants.CD_AUDIO_ENTRY_LEN + CdAudioConstants.CD_AUDIO_NAME_LEN;
                    writer.Write((uint)(newPos - oldPos));
                    writer.Write((uint)oldPos);

                    writer.BaseStream.Position = newPos;

                    // Write the new Meta Data back into the Collection
                    results.Add(new TrAudio
                    {
                        Name = name,
                        FilePath = fileName,
                        offset = (int)oldPos,
                        fileSize = (int)(newPos - oldPos),
                        channels = CdAudioConstants.TR_AUDIO_NUM_CHANNELS,
                        sampleRate = (int)CdAudioConstants.TR_AUDIO_SAMPLE_RATE,
                        duration = duration,
                        IsOriginalTrAudio = true,
                    });
                }
                writer.Close();
            }
            if (IsCanceled)
            {
                File.SetAttributes(fileTemp, FileAttributes.Normal);
                File.Delete(fileTemp);
                IsCanceled = false;
                throw new OperationCanceledException();
            }
            if (File.Exists(fileName))
                File.SetAttributes(fileName, FileAttributes.Normal);

            File.Delete(fileName);
            File.Copy(fileTemp, fileName, true);
            File.SetAttributes(fileName, FileAttributes.Normal);
            File.SetAttributes(fileTemp, FileAttributes.Normal);
            File.Delete(fileTemp);

            return results;
        }

        private void LogError(Exception ex)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorLog.txt");
            string logMessage = $"{DateTime.Now}: {ex}\n";

            File.AppendAllText(logFilePath, logMessage);
        }
    }
}
