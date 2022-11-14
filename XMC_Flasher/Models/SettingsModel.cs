using System;

namespace XMC_Flasher.Models
{
    [Serializable]
    internal class SettingsModel : BaseModel
    {
        private string? _firmwarePath;
        public string? FirmwareFilesPath { get => _firmwarePath; set { _firmwarePath = value; NotifyPropertyChanged(); } }
    }
}
