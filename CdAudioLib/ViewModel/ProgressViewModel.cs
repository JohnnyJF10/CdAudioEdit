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
    /// <summary>
    /// ViewModel for progress views
    /// </summary>
    public class ProgressViewModel : INotifyPropertyChanged
    {
        private int _progress = 0;

        /// <summary>
        /// Gets or sets the current progress value.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the maximum progress value.
        /// </summary>
        public int maxProgress
        {
            get => _maxProgress;
            set
            {
                _maxProgress = value;
                OnPropertyChanged(nameof(maxProgress));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressViewModel"/> class.
        /// </summary>
        /// <param name="maxP">The maximum progress value.</param>
        /// <param name="cancelCallback">The callback action to execute when cancel is requested.</param>
        public ProgressViewModel(int maxP, Action cancelCallback)
        {
            maxProgress = maxP;
            _cancelCommand = new RelayCommand(cancelCallback);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private RelayCommand _cancelCommand;

        /// <summary>
        /// Gets the command to cancel the operation.
        /// </summary>
        public RelayCommand CancelCommand => _cancelCommand;
    }
}
