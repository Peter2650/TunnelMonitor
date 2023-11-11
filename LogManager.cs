using System;
using System.IO;
using static TunnelMonitor.MainWindow;

namespace TunnelMonitor
{
    public class LogManager
    {
        private static string logDirectoryPath = @"C:\TunnelMonitor\";
        private static string logFileName = "tunnel.log";

        public LogManager(
            string path = ""
        )
        {
            logDirectoryPath = path;
        }

        // Log en persons indgang til tunnelen
        public string LogPath
        {
            get => logDirectoryPath;
            set
            {
                logDirectoryPath = value;
                logFileName = "tunnel.log";
            }
        }
        public void LogEntry(PersonEntry entry)
        {
            string entryLog = $"ENTRY: {entry.ToString()}";
            File.AppendAllText(
                Path.Combine(logDirectoryPath, logFileName),
                entryLog + Environment.NewLine
            );
        }

        // Log en persons udgang fra tunnelen og tilføj til historik
        public void LogExit(PersonEntry entry)
        {
            string exitLog = $"EXIT: {entry.ToString()}";
            File.AppendAllText(
                Path.Combine(logDirectoryPath, logFileName),
                exitLog + Environment.NewLine
            );
        }
    }
}
