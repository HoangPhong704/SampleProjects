using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XMC_Flasher.FrameWorks;

namespace XMC_Flasher.Managers
{

    /// <summary>
    /// Connect to J-Link with J-Link Commander using Process to flash hex file
    /// J-Link installer with J-Link.exe can be downloaded at: https://www.segger.com/downloads/jlink/
    /// Reference to J-Link commands at: co
    /// </summary>
    internal class JLinkWriter
    {
        const string DEVICE_NAME = "XMC1402-0128";
        const string CONNECTION_INTERFACE_SPEED = "4000";
        const string CONNECTION_INTERFACE_SWD = "s";
        const string RDM_ID_ADDRESS_DMX3 = "200018F0";
        const string DIRTY_VALUE_ADDRESS_DMX3 = "20001908";
        const string DIRTY_SETTINGS_ADDRESS_DMX3 = "20001BEC";
        const string RDM_ID_ADDRESS_DMX4 = "200037CC";
        const string DIRTY_VALUE_ADDRESS_DMX4 = "200037E4";
        const string DIRTY_SETTINGS_ADDRESS_DMX4 = "20003AC8";
        const string DEFAULT_RDM_ID = "31323334";
        const string EMPTY_ADDRESS_VALUE = "00000000";

        public string RDM_ID_Memmory_Address { get; set; }

        public string Dirty_Value_Memmory_Address { get; set; }
        public string Dirty_Settings_Memmory_Address { get; set; }
        public string JLinkFileName { get; set; }
        public string TemplateFileName { get; private set; }
        public JLinkWriter(bool isDMX3)
        {
            JLinkFileName = GetJLinkProgramFileFullName();
            if (isDMX3)
            {
                RDM_ID_Memmory_Address = RDM_ID_ADDRESS_DMX3;
                Dirty_Value_Memmory_Address = DIRTY_VALUE_ADDRESS_DMX3;
                Dirty_Settings_Memmory_Address = DIRTY_SETTINGS_ADDRESS_DMX3;
                TemplateFileName = "dmx3_template.hex";
            }
            else
            {
                RDM_ID_Memmory_Address = RDM_ID_ADDRESS_DMX4;
                Dirty_Value_Memmory_Address = DIRTY_VALUE_ADDRESS_DMX4;
                Dirty_Settings_Memmory_Address = DIRTY_SETTINGS_ADDRESS_DMX4;
                TemplateFileName = "dmx4_template.hex";
            }
        }
         /// <summary>
         /// Upload firmware file to xmc using J-Link command line
         /// </summary>
         /// <param name="firmware"></param>
         /// <param name="setBMI"></param>
         /// <param name="silenceUI"></param>
         /// <returns></returns>
        public Task<(bool IsSuccessful, string Message)> FlashAsync(string firmware, bool setBMI = false, bool silenceUI = false)
        {
            if (!File.Exists(JLinkFileName)) return Task.FromResult((false, "JLink program is not installed on this computer.  Please install it and try again."));
            return Task.Run(() =>
            {
                string lastMessage = "";
                try
                {
                    string parameter = silenceUI ? "-NoGui 1" : "";
                    var p = JLinkFileName.StartNewProcess(parameter);
                    if (setBMI) p.StandardInput.Write($"setbmi 3 {Environment.NewLine}");
                    p.StandardInput.Write($"connect {Environment.NewLine}");                        //start J-Link connection
                    p.StandardInput.Write($"{DEVICE_NAME} {Environment.NewLine}");                  //connect to device
                    p.StandardInput.Write($"{CONNECTION_INTERFACE_SWD} {Environment.NewLine}");     //select interface
                    p.StandardInput.Write($"{CONNECTION_INTERFACE_SPEED} {Environment.NewLine}");   //select interface speed
                    p.StandardInput.Write($"r {Environment.NewLine}");                              //reset CPU
                    p.StandardInput.Write($"h {Environment.NewLine}");                              //halt CPU
                    p.StandardInput.Write($"loadfile \"{firmware}\" {Environment.NewLine}");        //load firmware
                    p.StandardInput.Write($"g {Environment.NewLine}");                              //start CPU 
                    p.StandardInput.Write($"Exit {Environment.NewLine}");                           //must exit J-Link in order to grab output 
                    var messages = p.StandardOutput.ReadToEnd().Split(Environment.NewLine).Where(r => r != "J-Link>").Select(r => r).ToList();
                    lastMessage = messages[messages.Count - 2];
                    p.StandardInput.Flush();
                    p.Close();
                }
                catch (Exception ex)
                {
                    lastMessage = ex.Message;
                }
                return Task.FromResult((lastMessage == "O.K.", lastMessage ?? ""));
            });
        }
        //L:\Software\LEDFirmware\RDMTemplates
        /// <summary>
        /// Determine if existing device already have valid device ID
        /// </summary>
        /// <returns></returns>
        public bool HasValidRDMAddress()
        {
            var p = JLinkFileName.StartNewProcess();
            p.StandardInput.Write($"connect {Environment.NewLine}");                        //start J-Link connection
            p.StandardInput.Write($"{DEVICE_NAME} {Environment.NewLine}");                  //connect to device
            p.StandardInput.Write($"{CONNECTION_INTERFACE_SWD} {Environment.NewLine}");     //select interface
            p.StandardInput.Write($"{CONNECTION_INTERFACE_SPEED} {Environment.NewLine}");   //select interface speed
            p.StandardInput.Write($"r {Environment.NewLine}");                              //reset CPU                                 //
            Thread.Sleep(500);
            p.StandardInput.Write($"g {Environment.NewLine}");                              //reset CPU                             //
            Thread.Sleep(1000);
            p.StandardInput.Write($"Mem32 0x{RDM_ID_Memmory_Address}, 1 {Environment.NewLine}");       //Read current RDM ID   
            p.StandardInput.Write($"Exit {Environment.NewLine}");                           //must exit J-Link in order to grab output 
            var outputMessage = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var messages = outputMessage.Split(Environment.NewLine).ToList();
            var rdmAddress = messages[messages.Count - 2].Split("=")[1].Trim();
            p.StandardInput.Flush();
            p.Close();
            if (string.IsNullOrEmpty(rdmAddress) || rdmAddress == EMPTY_ADDRESS_VALUE) return false;
            return rdmAddress != DEFAULT_RDM_ID;
        }

        /// <summary>
        /// Set device ID to memory address so firmware to write to EEPROM
        /// </summary>
        /// <param name="rdmAddress"></param>
        /// <exception cref="Exception"></exception>
        public void SetRDMAddress(string rdmAddress)
        {
            var p = JLinkFileName.StartNewProcess();
            p.StandardInput.Write($"connect {Environment.NewLine}");                                    //start J-Link connection
            p.StandardInput.Write($"{DEVICE_NAME} {Environment.NewLine}");                              //connect to device
            p.StandardInput.Write($"{CONNECTION_INTERFACE_SWD} {Environment.NewLine}");                 //select interface
            p.StandardInput.Write($"{CONNECTION_INTERFACE_SPEED} {Environment.NewLine}");               //select interface speed  
            p.StandardInput.Write($"W4 0x{RDM_ID_Memmory_Address} 0x{rdmAddress} {Environment.NewLine}");        //set new RDM ID value 
            p.StandardInput.Write($"W4 0x{Dirty_Value_Memmory_Address} 0x00000001 {Environment.NewLine}");       //set set rdm value to dirty  
            p.StandardInput.Write($"W4 0x{Dirty_Settings_Memmory_Address} 0x00000001 {Environment.NewLine}");    //set settings to dirty 
            p.StandardInput.Write($"Exit {Environment.NewLine}");                                       //must exit J-Link in order to grab output 
            var outputMessage = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var messages = outputMessage.Split(Environment.NewLine).ToList();
            var lastMessage = messages[messages.Count - 4].Replace("J-Link>Writing ", "");
            if ($"{rdmAddress} -> {RDM_ID_Memmory_Address}" != lastMessage) throw new Exception("Failed to set RDM Address!");
            Thread.Sleep(250);
            p.StandardInput.Flush();
            p.Close();
        }




        private string GetJLinkProgramFileFullName()
        {
            var fileLocation = Path.Combine("SEGGER", "JLink", "JLink.exe");
            string fileName = string.Empty;
            if (Environment.Is64BitOperatingSystem)
            {
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), fileLocation);

                if (!File.Exists(fileName))
                    fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), fileLocation);
            }
            else
            {
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), fileLocation);
            }
            return fileName;
        }

    }
}
