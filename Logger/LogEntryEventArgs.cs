using System;

namespace Logger
{
    public class LogEntryEventArgs : EventArgs
    {
        public string LogMessage { get; set; }
    }
}
