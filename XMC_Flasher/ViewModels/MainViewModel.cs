using System;
using XMC_Flasher.FrameWorks;

namespace XMC_Flasher.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        public event EventHandler<EventArgs?>? OnEditSettingsRequested;
        public event EventHandler<EventArgs?>? OnCloseApplicationRequested;

        public TestFirmwareViewModel ViewModelTest { get; set; }
        public FlashFirmwareViewModel ViewModelFlash { get; set; }

        public DelegateCommand<object?>? CommandClose { get; set; }
        public DelegateCommand<object?>? CommandSettings { get; set; }
        public MainViewModel()
        {
            ViewModelTest = new TestFirmwareViewModel();
            ViewModelFlash = new FlashFirmwareViewModel();
            CommandClose = new DelegateCommand<object?>(ExecuteClose);
            CommandSettings = new DelegateCommand<object?>(ExecuteEditSettings);
        }



        private void ExecuteEditSettings(object? obj)
        {
            OnEditSettingsRequested?.Invoke(null, null);
        }

        private void ExecuteClose(object? obj)
        {
            OnCloseApplicationRequested?.Invoke(null, null);
        }


    }
}
