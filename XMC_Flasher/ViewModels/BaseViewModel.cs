using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XMC_Flasher.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            DataChanged = true;
        }
        public bool IsInitialized { get; set; }
        public bool DataChanged { get; set; }
    }
}
