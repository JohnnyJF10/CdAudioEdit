/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Model;
using System.Diagnostics.CodeAnalysis;

namespace CdAudioLib.Utils
{
    public class TrAudioHelper : IEqualityComparer<ITrAudio>
    {
        public void UpdateTrAudioFromOther<T1, T2>(T1 trAudioToUpdate, T2 otherTrAudio)
            where T1 : ITrAudio
            where T2 : ITrAudio
        {
            trAudioToUpdate.Name = otherTrAudio.Name;
            trAudioToUpdate.FilePath = otherTrAudio.FilePath;
            trAudioToUpdate.offset = otherTrAudio.offset;
            trAudioToUpdate.fileSize = otherTrAudio.fileSize;
            trAudioToUpdate.duration = otherTrAudio.duration;
            trAudioToUpdate.sampleRate = otherTrAudio.sampleRate;
            trAudioToUpdate.channels = otherTrAudio.channels;
            trAudioToUpdate.IsOriginalTrAudio = otherTrAudio.IsOriginalTrAudio;
        }

        public ITrAudio CloneTrAudio(ITrAudio trAudioToClone)
            => CloneTrAudio<TrAudio>(trAudioToClone);

        public T CloneTrAudio<T>(ITrAudio trAudioToClone) where T : ITrAudio, new()
        {
            T trAudio = new();
            trAudio.Name = trAudioToClone.Name;
            trAudio.FilePath = trAudioToClone.FilePath;
            trAudio.offset = trAudioToClone.offset;
            trAudio.fileSize = trAudioToClone.fileSize;
            trAudio.duration = trAudioToClone.duration;
            trAudio.sampleRate = trAudioToClone.sampleRate;
            trAudio.channels = trAudioToClone.channels;
            trAudio.IsOriginalTrAudio = trAudioToClone.IsOriginalTrAudio;
            return trAudio;
        }

        public void RenewTrAudio<T>(T trAudio) where T : ITrAudio
        {
            trAudio.Name = null;
            trAudio.FilePath = string.Empty;
            trAudio.offset = 0;
            trAudio.fileSize = 0;
            trAudio.duration = 0;
            trAudio.sampleRate = 0;
            trAudio.channels = 0;
            trAudio.IsOriginalTrAudio = false;
        }

        public bool Equals(ITrAudio? x, ITrAudio? y)
        {
            return x != null && y != null
                && String.Equals(x.Name, y.Name)
                && String.Equals(x.FilePath, y.FilePath)
                && x.offset == y.offset
                && x.fileSize == y.fileSize
                && x.duration == y.duration
                && x.sampleRate == y.sampleRate
                && x.channels == y.channels
                && x.IsOriginalTrAudio == y.IsOriginalTrAudio;
        }

        public int GetHashCode([DisallowNull] ITrAudio obj)
        {
            return obj.GetHashCode();
        }
    }
}
