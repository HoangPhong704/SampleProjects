using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XMC_Flasher.Models
{
    [Serializable]
    public class BaseModel : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;
       
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            HasChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));           
        }
        private bool _hasChanged;
        public bool HasChanged {
            get => _hasChanged;
            set { _hasChanged = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasChanged")); } }

        public int UpdateBy { get; set; }
    }
}
