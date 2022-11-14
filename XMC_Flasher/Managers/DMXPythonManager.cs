using XMC_Flasher.FrameWorks;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace XMC_Flasher.Managers
{
    internal class DMXPythonManager
    {
        string PYTHON = "Python.exe";
        string SUBFOLDER = "PythonScripts";
        string SCRIPT_NAME = "ValidateDMXBoard.py";
        string _comPort;
        public DMXPythonManager(string comPort)
        {
            _comPort = comPort;
        }

        public (string Address, string FirmwareVersion, string Wattage) GetFirmwareData()
        { 
            var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", SUBFOLDER);
            var sourceFile = Path.Combine(directory, SCRIPT_NAME);
            var p = PYTHON.StartNewProcess($"{sourceFile} {_comPort}");  
            var messages = p.StandardOutput.ReadToEnd().Split(Environment.NewLine).Select(r => r).ToList();
            p.StandardInput.Flush();
            p.Close(); 
            if (messages.Count() > 2)
            { 
                var errorCode = int.Parse(messages[0]);
                switch(errorCode)
                {
                    case 0:
                        return (messages[1], messages[2].Replace("\a", "").Replace("\b", ""), ""); 
                    case 1:
                        throw new Exception("Cannot find RDM address.");
                    case 2:
                        throw new Exception("Failed to get firmware version.");
                }
                
            }
            else
            { 
                throw new Exception("Please verify setup and try again.");
            }
            return("","","");
        }

    }
}
