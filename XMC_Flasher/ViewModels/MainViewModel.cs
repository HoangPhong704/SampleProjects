using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using XMC_Flasher.FrameWorks;
using XMC_Flasher.Managers;

namespace XMC_Flasher.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        public event EventHandler<EventArgs?>? OnEditSettingsRequested;
        public event EventHandler<EventArgs?>? OnCloseApplicationRequested;
        private List<KeyValuePair<string, string>>? _fixtures;
        public List<KeyValuePair<string, string>>? Fixtures { get => _fixtures; set { _fixtures = value; NotifyPropertyChanged(); } }

        private string? _selectedFixture;
        public string? SelectedFixture { get => _selectedFixture; set { _selectedFixture = value; NotifyPropertyChanged(); } }
        
        private string? _selectedBoard;
        public string? SelectedBoard { get => _selectedBoard; set { _selectedBoard = value; NotifyPropertyChanged(); } }
        
        private List<KeyValuePair<string, string>>? _boardFamilies;
        public List<KeyValuePair<string, string>>? BoardFamilies { get => _boardFamilies; set { _boardFamilies = value; NotifyPropertyChanged(); } }

        private string? _message;
        public string? Message { get => _message; set { _message = value; NotifyPropertyChanged(); } }

        private bool? _hasError;
        public bool? HasError { get => _hasError; set { _hasError = value; NotifyPropertyChanged(); } }
        private bool? _hasTestError;
        public bool? HasTestError { get => _hasTestError; set { _hasTestError = value; NotifyPropertyChanged(); } }

        private string? _testMessage;
        public string? TestMessage { get => _testMessage; set { _testMessage = value; NotifyPropertyChanged(); } }
        private bool _isFlashing;
        public bool IsFlashing { get => _isFlashing; set { _isFlashing = value; NotifyPropertyChanged(); } }

        private List<KeyValuePair<string, string>>? _firmwareFiles;
        public List<KeyValuePair<string, string>>? FirmwareFiles { get => _firmwareFiles; set { _firmwareFiles = value; NotifyPropertyChanged(); } }

        private string? _selectedFirmware;
        public string? SelectedFirmware { get => _selectedFirmware; set { _selectedFirmware = value; NotifyPropertyChanged(); } }
 
        public DelegateCommand<object?>? CommandFlash { get; set; }
        public DelegateCommand<object?>? CommandClose { get; set; }
        public DelegateCommand<object?>? CommandSettings { get; set; }
        public DelegateCommand<object?>? CommandFixtureSelected { get; set; }
        public DelegateCommand<object?>? CommandBoardSelected { get; set; }
        public DelegateCommand<object?>? CommandRefreshFirmware { get; set; }
        public DelegateCommand<object?>? CommandTestFirmware { get; set; }
        public MainViewModel()
        {
            IntializateCommands();  
        }

        private void IntializateCommands()
        {
            CommandFlash = new DelegateCommand<object?>(ExecuteFlash, CanExecuteFlash);
            CommandClose = new DelegateCommand<object?>(ExecuteClose);
            CommandSettings = new DelegateCommand<object?>(ExecuteEditSettings);
            CommandFixtureSelected = new DelegateCommand<object?>(ExecuteFixtureSelected);
            CommandBoardSelected = new DelegateCommand<object?>(ExecuteBoardSelected);
            CommandRefreshFirmware = new DelegateCommand<object?>(ExecuteRefreshFirmware, CanExecuteRefreshFirmware);
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

        private void ExecuteBoardSelected(object? obj)
        {
            GetFixtures();
        }

        private bool CanExecuteRefreshFirmware(object? obj)
        {
            return true;
        }

        private void ExecuteRefreshFirmware(object? obj)
        {
            GetSelectedFixtureFirmware();
        }

        private void ExecuteFixtureSelected(object? obj)
        {
            GetSelectedFixtureFirmware();
        }

        private void ExecuteEditSettings(object? obj)
        {
            OnEditSettingsRequested?.Invoke(null, null);
        }

        private void ExecuteClose(object? obj)
        {
            OnCloseApplicationRequested?.Invoke(null, null);
        }
         
        private bool CanExecuteFlash(object? obj)
        {
            return true;
        }

        private bool CanFlash()
        {
            return (!string.IsNullOrWhiteSpace(SelectedFixture) && !string.IsNullOrWhiteSpace(SelectedFirmware) && !IsFlashing);
        }

        private void ExecuteFlash(object? obj)
        {
            FlashFirmware();
        }

        private async void FlashFirmware()
        {
            if (!CanFlash()) return;
            var currentCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                IsFlashing = true;
                HasError = null;
                Message = "Flashing firmware...";
                var jLink = new JLinkWriter(SelectedBoard?.Contains("DMX_3") == true);
                var rdmTemplateFile = Path.Combine(SettingsManager.Instance.FirmwarePath ?? "", SelectedBoard ?? "", SelectedFixture ?? "", SelectedFirmware ?? "");
                if (!File.Exists(rdmTemplateFile)) throw new Exception($"RDM template file could not be located!");
                var rdmResult = await jLink.FlashAsync(rdmTemplateFile, true, true);
                if (!rdmResult.IsSuccessful) throw new Exception(rdmResult.Message);
                if (!jLink.HasValidRDMAddress())
                {
                    var rdmAddress = new MX2SContext().ReserveRDMAddress();
                    if (string.IsNullOrEmpty(rdmAddress) || rdmAddress == "00000000") throw new Exception("Failed to reserve RDM Address!");
                    jLink.SetRDMAddress(rdmAddress);
                }

                var firmwareFile = Path.Combine(SettingsManager.Instance.FirmwarePath ?? "", SelectedBoard ?? "", SelectedFixture ?? "", SelectedFirmware ?? "");
                if (!File.Exists(firmwareFile)) throw new Exception($"Firmware file could not be located!");
                var result = await jLink.FlashAsync(firmwareFile);
                if (!result.IsSuccessful) throw new Exception(result.Message);
                Message = "Successfully flashed firmware.";
                HasError = false;
            }
            catch (Exception ex)
            {
                Logger.LogManager.Instance.WriteLog("Failed to flash firmware.", ex);
                Message = $"Failed to flash firmware. {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsFlashing = false;
                Mouse.OverrideCursor = currentCursor;
            }
        }

        public async void GetBoardFamilies()
        {
            try
            { 
                SelectedBoard = null;
                var boardFamilies = new List<KeyValuePair<string, string>>();
                var directories = await Task.Run(() =>
                {
                    var directorInfo = new DirectoryInfo(SettingsManager.Instance.FirmwarePath ?? "");
                    if (!directorInfo.Exists)
                    {
                        throw new Exception("Could not get to firmware folder.  Please check network or file path!");
                    }
                    return directorInfo.GetDirectories().Select(r => r.Name).Where(r=>r.Contains("DMX")).ToList();
                });
                if (directories != null)
                {
                    var orderedFixtures = new List<int>();
                    foreach (var folder in directories)
                    {

                        boardFamilies.Add(new KeyValuePair<string, string>(folder, folder.Replace("_", " ")));
                    }
                    orderedFixtures.OrderBy(r => r);
                }
                BoardFamilies = boardFamilies;
                SelectedBoard = BoardFamilies.FirstOrDefault().Key;
                if (!string.IsNullOrWhiteSpace(SelectedBoard))
                {
                    GetFixtures();
                }
            }
            catch (Exception ex)
            {
                Logger.LogManager.Instance.WriteLog("Failed to get board family.", ex);
                MessageBoxManager.ShowError(ex.Message);
            }
        }
        public async void GetFixtures()
        {
            try
            {
                var filePath = Path.Combine(SettingsManager.Instance.FirmwarePath ?? "", SelectedBoard ?? "");
                SelectedFixture = null;
                var fixtures = new List<KeyValuePair<string, string>>(); 
                var directories = await Task.Run(()=>
                            { 
                                var directorInfo = new DirectoryInfo(filePath ?? "");
                                if (!directorInfo.Exists)
                                {
                                    throw new Exception("Could not get to firmware folder.  Please check network or file path!");
                                }
                                return directorInfo.GetDirectories().Select(r=> r.Name).ToList();
                            });
                if (directories != null)
                {
                    var orderedFixtures = new List<int>();
                    directories = directories.OrderBy(r=>r.Length).ThenBy(r=>r).ToList();
                    foreach (var folder in directories)
                    {

                        fixtures.Add(new KeyValuePair<string, string>(folder, folder.Replace("_", " ")));
                    }
                    orderedFixtures.OrderBy(r=>r);
                }
                Fixtures = fixtures;
                SelectedFixture = Fixtures.FirstOrDefault().Key;
            }
            catch (Exception ex)
            {
                Logger.LogManager.Instance.WriteLog("Failed to get fixtures.", ex);
                MessageBoxManager.ShowError(ex.Message);
            }
        }
        private async void GetSelectedFixtureFirmware()
        {
            try
            {
                SelectedFirmware = null;
                FirmwareFiles?.Clear();
                if (string.IsNullOrWhiteSpace(SelectedFixture)) return;
                List<string>? files = null;
                files = await Task.Run(() =>
                {
                    var path = Path.Combine(SettingsManager.Instance.FirmwarePath ?? "", SelectedBoard ?? "", SelectedFixture ?? "");
                    var directorInfo = new DirectoryInfo(path);
                    if (!directorInfo.Exists)
                    {
                        throw new Exception("Could not get to firmware files.  Please check network or file path!");
                    }
                    return directorInfo.GetFiles().OrderByDescending(r=>r.Name).Select(r => r.Name).ToList();
                });
                if (files == null) return;
                FirmwareFiles = files.Select(r=> new KeyValuePair<string,string>(r, r.Replace(".hex","").Replace("-",".").Replace("_"," "))).ToList();
                SelectedFirmware = FirmwareFiles.FirstOrDefault().Key;
            }
            catch (Exception ex)
            {
                Logger.LogManager.Instance.WriteLog("Failed to get firmware.", ex);
                MessageBoxManager.ShowError(ex.Message);
            }
        } 
          

        private string? _rmdAddress;
        public string? RDMAddress { get => _rmdAddress; set { _rmdAddress = value; NotifyPropertyChanged(); } }

        private string? _firmwareVersion;
        public string? FirmwareVersion { get => _firmwareVersion; set { _firmwareVersion = value; NotifyPropertyChanged(); } }
        private string? _wattage;
        public string? Wattage { get => _wattage; set { _wattage = value; NotifyPropertyChanged(); } }
        private bool _isTesting;
        public bool IsTesting { get => _isTesting; set { _isTesting = value; NotifyPropertyChanged(); } }
        public async void ValidateBoardFirmware()
        {
            if(IsTesting)
                return;
            IsTesting = true;
            bool hasTestError = false;
            string testMessage = "";
            try
            {
                HasTestError = null;
                RDMAddress = null;
                FirmwareVersion = null;
                Wattage = null;
                TestMessage = "Validating...";
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
                HasTestError = hasTestError;
                TestMessage = testMessage;
            }


        }
    }
}
