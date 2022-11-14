using System;
using System.IO;
using System.Linq;

namespace Logger
{
    public delegate void LogEntryAddedHandler(object sender, LogEntryEventArgs e);


    /// <summary>
    /// A singleton class that is thread safe for logging to a log file in a folder where the application is running from.
    /// The log file has a maximum size and when reached the file is renamed.
    /// An unlimited number of archive files will be created. If DeleteLogFile property is set then
    /// log files are not archived but are deleted. Logging starts again on an empty file.
    /// </summary>
    public class LogManager
    {

        #region Constants and Members

        public const string StartStopLogFileDelimeter = "**************************************************";
        private const string LogFileFolderName = "Log";
        //private const int MaxLogFileSizeBytes = 10000; // = 10 KB can use for testing
        private const int MaxLogFileSizeBytes = 5242880; // = 5 MB
        private const int MaxNumberOfLogFiles = 20;

        private const string LogFileExtension = ".txt";
        private const string LogDateTimeFormat = "MM/dd/yyyy HH:mm:ss:fff";
        private const string ArchiveFileName = "yyyy-MM-dd-HH-mm-ss-fff";

        private static LogManager loggerInstance;
        private static object threadLockObject = new object();

        /// <summary>
        /// The default is a log file name that is the name of the exe.
        /// </summary>
        public static string LogFileName { get; private set; }
        /// <summary>
        /// Path is where the exe is running from.
        /// </summary>
        public static string LogFilePath { get; set; }

        public static string StartStopLogFileName { get; private set; }

        /// <summary>
        /// Set the name and extension of the log file for custom logging that should not go to the exe log file.
        /// Use this with the CustomWriteLog(). If the property is not set then the default LogFileName is used.
        /// </summary>
        public static string CustomLogFileName { get; set; }

        public static bool DeleteLogFile { get; set; }

        /// <summary>
        /// Raised when a new message is saved to the event log file.
        /// </summary>
        public event LogEntryAddedHandler LogEntryAdded;

        /// <summary>
        /// Indicates the type of log message.
        /// </summary>
        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Indicates application start or stop.
        /// </summary>
        public enum StartStopState
        {
            Start,
            Stop,
            Running
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Private constructor as callers use LoggerInstance() to get an instance of the singleton class.
        /// </summary>
        private LogManager()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the instance of the Logger class as it is a singleton. It is thread safe.
        /// </summary>
        /// <returns></returns>
        public static LogManager Instance
        {
            get
            {
                if (loggerInstance == null)
                {
                    lock (threadLockObject)
                    {
                        if (loggerInstance == null)
                        {
                            //create log instance
                            loggerInstance = new LogManager();
                            IntializeLogger();
                        }
                    }
                }

                return loggerInstance;
            }
        }

        /// <summary>
        /// Writes message as a new line to the log file with a default LogType of Info.
        /// </summary>
        /// <param name="message">The message to save to the log file.</param>
        public void WriteLog(string message)
        {
            SaveLog(message, LogType.Info);
        }

        /// <summary>
        /// Writes message as a new line to the log file with a desired LogType.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        public void WriteLog(string message, LogType logType)
        {
            SaveLog(message, logType);
        }

        /// <summary>
        /// Writes a message and the exception as a new line to the log file with a type of Error.
        /// </summary>
        /// <param name="message">A friendly sentence outlining issue.</param>
        /// <param name="exception">The exception object.</param>
        public void WriteLog(string message, Exception exception)
        {
            string result = string.Format("{0} {1}", message, exception.ToString());
            SaveLog(result, LogType.Error);
        }

        /// <summary>
        /// Use to write a log to an alternate log file rather than the exe log file. The log file name is set with the CustomLogFileName property.
        /// The CustomLogFileName property should be set for each call to this method as it is static and could be set by other threads or assemblies.
        /// </summary>
        /// <param name="message">The message to save to the log file.</param>
        public void CustomWriteLog(string message)
        {
            //uses the same lock object in case the custom log file name is not set then
            //the default is used
            lock (threadLockObject)
            {
                try
                {
                    CheckLogFileSize(true);
                    string path = string.Empty;

                    //if CustomLogFileName has not been set then the default is used.
                    if (string.IsNullOrEmpty(CustomLogFileName))
                    {
                        path = Path.Combine(LogFilePath, LogFileName);
                    }
                    else
                    {
                        path = Path.Combine(LogFilePath, CustomLogFileName);
                    }

                    string logMessage = string.Format("{0}     {1}{2}", DateTime.Now.ToString(LogDateTimeFormat), message, Environment.NewLine);

                    System.IO.File.AppendAllText(path, logMessage);
                    if (LogEntryAdded != null)
                    {
                        LogEntryAdded(this, new LogEntryEventArgs() { LogMessage = message });
                    }
                }
                catch (Exception)
                {
                    //empty as logging is not working
                }
            }
        }

        public void AppStartStopWriteLog(string message, StartStopState state)
        {
            string filename = string.Format("StartStop{0}", StartStopLogFileName);
            string path = Path.Combine(LogFilePath, filename);

            switch (state)
            {
                case StartStopState.Start:
                    System.IO.File.AppendAllText(path, string.Format("{0}{1}", Environment.NewLine, StartStopLogFileDelimeter));
                    System.IO.File.AppendAllText(path, string.Format("{0}{1}     Application started. {2}", Environment.NewLine, DateTime.Now.ToString(LogDateTimeFormat), message));
                    break;
                case StartStopState.Stop:
                    System.IO.File.AppendAllText(path, string.Format("{0}{1}     Application closing. {2}", Environment.NewLine, DateTime.Now.ToString(LogDateTimeFormat), message));
                    System.IO.File.AppendAllText(path, string.Format("{0}{1}", Environment.NewLine, StartStopLogFileDelimeter));
                    break;
                case StartStopState.Running:
                    System.IO.File.AppendAllText(path, string.Format("{0}{1}     Application running. {2}", Environment.NewLine, DateTime.Now.ToString(LogDateTimeFormat), message));
                    break;
            }
        }

        #endregion

        #region Private Methods

        private static void IntializeLogger()
        {
            //set log file name
            LogFileName = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", LogFileExtension);
            //LogFileName = "MX2SLogFile";
            //set start\stop log file name
            StartStopLogFileName = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", string.Format("Log{0}", LogFileExtension));

            //set log file root path
            LogFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, LogFileFolderName);
            //LogFilePath = "C:\\temp\\MX2";
            //set default log delete behaviour
            DeleteLogFile = false;

            //make sure EcsLog folder exists and create if needed
            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }
            //if (!Directory.Exists(LogFilePath))
            //{               
            //    Directory.CreateDirectory(LogFilePath);
            //}
             

        }

        private void SaveLog(string message, LogType logType)
        {
            lock (threadLockObject)
            {
                try
                {
                    CheckLogFileSize(false);
                    string path = Path.Combine(LogFilePath, LogFileName);
                    string logMessage = string.Format("{0}     {1}     {2}{3}", DateTime.Now.ToString(LogDateTimeFormat), logType.ToString().PadRight(12, ' '), message, Environment.NewLine);

                    if (!Directory.Exists(LogFilePath))
                    {
                        Directory.CreateDirectory(LogFilePath);
                    }
                    System.IO.File.AppendAllText(path, logMessage);
                    if (LogEntryAdded != null)
                    {
                        LogEntryAdded(this, new LogEntryEventArgs() { LogMessage = message });
                    }
                }
                catch (Exception)
                {
                    //empty as logging is not working
                }
            }
        }

        /// <summary>
        /// The log file is deleted if size in bytes is larger than MaxLogFileSizeBytes.
        /// </summary>
        /// <param name="checkCustomLogFile">Indicates which log file to check.</param>
        private static void CheckLogFileSize(bool checkCustomLogFile)
        {
            string path = string.Empty;
            if (checkCustomLogFile)
            {
                path = Path.Combine(LogFilePath, CustomLogFileName);
            }
            else
            {
                path = Path.Combine(LogFilePath, LogFileName);
            }

            if (File.Exists(path))
            {
                FileInfo logFile = new FileInfo(path);
                if (logFile.Length > MaxLogFileSizeBytes)
                {
                    ManageLogFile(path);
                }
            }
        }

        /// <summary>
        /// A log file is deleted or it is renamed for archival purpose. The default is renamed.
        /// </summary>
        /// <param name="logFilePath"></param>
        private static void ManageLogFile(string logFilePath)
        {
            if (DeleteLogFile)
            {
                File.Delete(logFilePath);
            }
            else
            {
                //rename file so a new one will be created
                string archiveName = DateTime.Now.ToString(ArchiveFileName);
                FileInfo file = new FileInfo(logFilePath);
                File.Move(logFilePath, Path.Combine(file.DirectoryName, string.Format("{0}{1}", archiveName, file.Name)));
                ManageLogFileNumber(logFilePath);
            }
        }

        /// <summary>
        /// Makes sure there are only MaxNumberOfLogFiles being saved. This will limit the number of log files created.
        /// </summary>
        private static void ManageLogFileNumber(string logFilePath)
        {
            try
            {
                FileInfo file = new FileInfo(logFilePath);
                string logFileSearchString = string.Format("*{0}", file.Name);
                FileInfo[] files = file.Directory.GetFiles(logFileSearchString);
                int numberOfFiles = files.Length;

                if (numberOfFiles > MaxNumberOfLogFiles)
                {
                    //get the oldest file based on modified date
                    //uses linq
                    var oldestFile = files
                        .Select(x => x)
                        .OrderBy(x => x.LastWriteTime)
                        .Take(1)
                        .ToArray();

                    //delete the oldest file
                    if (oldestFile.Length > 0) oldestFile[0].Delete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Issue managing the number number of log files. {0}", ex.ToString());
            }
        }

        #endregion

    }
}
