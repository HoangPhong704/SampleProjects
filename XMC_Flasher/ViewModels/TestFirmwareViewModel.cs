using System;
using System.Threading.Tasks;
using XMC_Flasher.FrameWorks;
using XMC_Flasher.Managers;

namespace XMC_Flasher.ViewModels
{
    public class TestFirmwareViewModel : BaseViewModel
    {

        private bool? _hasError;
        public bool? HasError { get => _hasError; set { _hasError = value; NotifyPropertyChanged(); } }

        private string? _testMessage;
        public string? Message { get => _testMessage; set { _testMessage = value; NotifyPropertyChanged(); } }

        private string? _rmdAddress;
        public string? RDMAddress { get => _rmdAddress; set { _rmdAddress = value; NotifyPropertyChanged(); } }

        private string? _firmwareVersion;
        public string? FirmwareVersion { get => _firmwareVersion; set { _firmwareVersion = value; NotifyPropertyChanged(); } }
        private string? _wattage;
        public string? Wattage { get => _wattage; set { _wattage = value; NotifyPropertyChanged(); } }
        private bool _isTesting;
        public bool IsTesting { get => _isTesting; set { _isTesting = value; NotifyPropertyChanged(); } }

        public DelegateCommand<object?>? CommandTestFirmware { get; set; }

        public TestFirmwareViewModel()
        {
            CommandTestFirmware = new DelegateCommand<object?>(ExecuteTestFirmware, CanExecuteTestFirmware);
        }

        private void ExecuteTestFirmware(object? obj)
        {
            ValidateBoardFirmware();
        }

        private bool CanExecuteTestFirmware(object? obj)
        {
            return true;
        }

        /// <summary>
        /// Test firmware and print label
        /// </summary>
        public async void ValidateBoardFirmware()
        {
            if (IsTesting)
                return;
            IsTesting = true;
            bool hasTestError = false;
            string testMessage = "";
            try
            {
                HasError = null;
                RDMAddress = null;
                FirmwareVersion = null;
                Wattage = null;
                Message = "Validating...";
                await Task.Delay(500);
                if (string.IsNullOrWhiteSpace(SettingsManager.Instance.ComPort))
                    throw new Exception("Cannot be completed, a COM port for DMX controller is required!");
                var result = new DMXPythonManager(SettingsManager.Instance.ComPort).GetFirmwareData();
                RDMAddress = result.Address;
                FirmwareVersion = result.FirmwareVersion;
                Wattage = result.Wattage;
                var barcode = new BarCodeManager();
                var labelItem = barcode.GenerateBarCode(FirmwareVersion, RDMAddress, Wattage);
                if (string.IsNullOrWhiteSpace(SettingsManager.Instance.Printer))
                    throw new Exception("Cannot be completed, a printer is required!");
                var printerManager = new PrintManager(SettingsManager.Instance.Printer, labelItem);
                printerManager.Print();
                testMessage = "Validation was completed succesffuly, a label is being printed.";
            }
            catch (Exception ex)
            {
                hasTestError = true;
                testMessage = $"There was an issue with test.  {ex.Message}";
            }
            finally
            {
                IsTesting = false;
                HasError = hasTestError;
                Message = testMessage;
            }

        }
    }
}
