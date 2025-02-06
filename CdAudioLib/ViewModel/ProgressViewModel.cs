/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Extensions;
using CdAudioLib.Utils;

using System.ComponentModel;

namespace CdAudioLib.ViewModel
{
    public class ProgressViewModel : INotifyPropertyChanged
    {
        private int _progress = 0;

        public int progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(progress));
            }
        }

        private int _maxProgress = CdAudioConstants.CD_AUDIO_CAPACITY;

        public int maxProgress
        {
            get => _maxProgress;
            set
            {
                _maxProgress = value;
                OnPropertyChanged(nameof(maxProgress));
            }
        }

        public ProgressViewModel(int maxP, Action cancelCallback)
        {
            maxProgress = maxP;
            _cancelCommand = new RelayCommand(cancelCallback);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private RelayCommand _cancelCommand;

        public RelayCommand CancelCommand => _cancelCommand;
    }
}
