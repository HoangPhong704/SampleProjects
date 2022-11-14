using System;

namespace XMC_Flasher.FrameWorks
{
    public class SettingsManager
    {
        public string? FirmwarePath { get; set; }
        public string? ComPort { get; set; }
        public string? Printer { get; set; }
        public string? DB_Connection { get; set; }

        private static SettingsManager? _instance;
        private static object _instanceLock = new object();

        private SettingsManager()
        {
            Load();
        }
        public static SettingsManager Instance {
            get { 
                    lock(_instanceLock)
                    {
                        if (_instance == null)
                            _instance = new SettingsManager();
                        return _instance;
                    } 
                }
        }

        public void Set(string filePath, string comPort, string printer, string db_connection)
        {
            FirmwarePath = filePath;
            ComPort = comPort;
            Printer = printer;
            DB_Connection = db_connection;
        }

        public void Save()
        {
            Properties.Settings.Default.FirmwarePath = FirmwarePath;
            Properties.Settings.Default.COMPort = ComPort;
            Properties.Settings.Default.Printer = Printer;
            Properties.Settings.Default.DB_Connection = DB_Connection;
            Properties.Settings.Default.Save();
        }
        public void Load()
        {
            FirmwarePath = Properties.Settings.Default.FirmwarePath;
            ComPort = Properties.Settings.Default.COMPort;
            Printer = Properties.Settings.Default.Printer;
            DB_Connection = Properties.Settings.Default.DB_Connection;
        }
    }
}
