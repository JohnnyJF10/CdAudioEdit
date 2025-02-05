/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

namespace CdAudioLib.Model
{
    public class TrAudioName
    {
        public TrAudioName(string? Name, int slot)
        {
            this.Name = Name;
            this.slot = slot;
        }

        public string? Name { get; }

        public int slot { get; }
    }
}
