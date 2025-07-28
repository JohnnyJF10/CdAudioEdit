

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
