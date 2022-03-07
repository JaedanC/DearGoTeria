using System;
using System.Collections.Generic;

public static class ImGuiLog
{
    public enum LogType
    {
        Debug,
        Info,
        Warning,
        Critical
    }

    public struct LogData
    {
        public DateTime TimeStamp;
        public LogType Type;
        public string Message;
    }

    private static readonly List<LogData> Entries;

    static ImGuiLog()
    {
        Entries = new List<LogData>();
    }

    public static List<LogData> GetEntries()
    {
        lock (Entries)
        {
            return Entries;
        }
    }

    private static void AddEntry(LogType type, string message)
    {
        lock (Entries)
        {
            Entries.Add(new LogData
            {
                TimeStamp = DateTime.Now,
                Type = type,
                Message = message,
            });
        }
    }

    public static void Debug(string s)
    {
        AddEntry(LogType.Debug, s + "\n");
    }
    
    public static void Info(string s)
    {
        AddEntry(LogType.Info, s);
    }
    
    public static void Warning(string s)
    {
        AddEntry(LogType.Warning, s);
    }
    
    public static void Critical(string s)
    {
        AddEntry(LogType.Critical, s);
    }

    public static void Clear()
    {
        lock (Entries)
        {
            Entries.Clear();
        }
    }
}
