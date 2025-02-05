using CdAudioLib.ViewModel;

namespace CdAudioLib.Abstraction
{
    public interface IProgressService
    {
        public void Open(MainViewModel mainViewModel);

        public void Close();
    }
}
