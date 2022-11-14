using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using XMC_Flasher.FrameWorks;

namespace XMC_Flasher.ViewModels
{
    internal class SettingsViewModel : BaseViewModel
    {
        public event EventHandler<EventArgs?>? OnSettingsSaved;
        private string? _firmwarePath;
        public string? FirmwarePath { get => _firmwarePath; set { _firmwarePath = value; NotifyPropertyChanged(); } }

        private string? _printerName;
        public string? PrinterName { get => _printerName; set { _printerName = value; NotifyPropertyChanged(); } }

        private string? _comPort;
        public string? COMPort { get => _comPort; set { _comPort = value; NotifyPropertyChanged(); } }

        private string? _dbConnection;
        public string? DB_Connection { get => _dbConnection; set { _dbConnection = value; NotifyPropertyChanged(); } }

        private List<string?> _availableCOMPorts;
        public List<string?> AvailablePorts { get => _availableCOMPorts; set { _availableCOMPorts = value; NotifyPropertyChanged(); } }

        private List<string?> _availablePrinters;
        public List<string?> AvailablePrinters { get => _availablePrinters; set { _availablePrinters = value; NotifyPropertyChanged(); } }
        public DelegateCommand<object?> CommandSave { get; set; }
        public DelegateCommand<object?> CommandBrowseDirectory { get; set; }
        public SettingsViewModel()
        {
            CommandSave = new DelegateCommand<object?>(ExecuteSave, CanExecuteSave);
            CommandBrowseDirectory = new DelegateCommand<object?>(ExecuteBroseDirectory);
            Initialize();
        }

        private void ExecuteBroseDirectory(object? obj)
        {
            var result = new FileDialogManager().BrowseDirectory();
            if (result.IsSelected)
                FirmwarePath = result.SelectedDirectory;
        }

        private void Initialize()
        { 
            AvailablePorts = SerialPort.GetPortNames().Select(r => (string?)r).ToList();
            var printers = new List<string?>();
            foreach (var printer in PrinterSettings.InstalledPrinters)
            {
                if (printer == null)
                    continue;
                printers.Add(printer.ToString());
            }
            AvailablePrinters = printers;
        }
        /// <summary>
        /// All settings must have values to save changes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecuteSave(object? obj)
        { 
            return !string.IsNullOrWhiteSpace(FirmwarePath) && !string.IsNullOrWhiteSpace(DB_Connection) && !string.IsNullOrWhiteSpace(PrinterName) && !string.IsNullOrWhiteSpace(COMPort) && DataChanged;
        }

        private void ExecuteSave(object? obj)
        {
            try
            {
                if (!Directory.Exists(FirmwarePath)) throw new Exception("Firmware folder does not exist. Please verify and try again!");
                if (string.IsNullOrWhiteSpace(PrinterName)) throw new Exception("A printer must be selected!");
                if (string.IsNullOrWhiteSpace(COMPort)) throw new Exception("A valid COM port must be selected!");
                if (string.IsNullOrWhiteSpace(DB_Connection)) throw new Exception("A valid database connection is required!");
                SettingsManager.Instance.Set(FirmwarePath, COMPort ?? "", PrinterName ?? "", DB_Connection ?? "");
                new DataContext().Validate();
                SettingsManager.Instance.Save();
                OnSettingsSaved?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                SettingsManager.Instance.Load();
                MessageBoxManager.ShowError(ex.Message);
            }

        }

        internal void GetSettings()
        { 
            FirmwarePath = SettingsManager.Instance.FirmwarePath; 
            PrinterName = SettingsManager.Instance.Printer;
            COMPort = SettingsManager.Instance.ComPort;
            DB_Connection = SettingsManager.Instance.DB_Connection;
            DataChanged = false;
        }
    }
}
