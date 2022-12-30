using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class JFLog
{
    public enum LogTag
    {
        UNKNOWN,
        UI,
        IAP,
        ADS,
        AUTHEN,
        STORAGE,
        CONFIG,
        GAMEPLAY,
        LOCALIZE,
        RESOURCE,
        NETWORK,
        SERVICE,
        LOCALSAVE,
        OTHER
    }

    internal enum LogLevel
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3
    }

    static readonly private string _logFilePath = Application.persistentDataPath + "/JuiceFruit.log";

    static private bool _isInitialized = false;

    static public string LogPath => _logFilePath;

    static private void Init()
    {
        _isInitialized = true;
        string runTime = "\n==========" + DateTime.Now.ToString("yyyy-MM-dd") + "==========\n";
        WriteFile(_logFilePath, runTime, false);
    }

    static private void WriteFile(string path, string data, bool append = true)
    {
        if (!_isInitialized) Init();
        StreamWriter writer = new StreamWriter(path, append);
        writer.Write(data);
        writer.Close();
    }

    static private void WriteLog(LogLevel logLevel, string logTag, string msg, params object[] param)
    {
        Color levelColor = GetLogColor(logLevel);

        string log = msg;
        if (param.Length > 0)
        {
            log = string.Format(msg, param);
        }

        StackTrace stackTrace = new StackTrace(true);
        StackFrame stackFrame = stackTrace.GetFrame(2);
        string tag = $"[{stackFrame.GetFileName().Split('\\').Last()}:{stackFrame.GetFileLineNumber()}:{stackFrame.GetFileColumnNumber()}][{logLevel}][{logTag}]: ";
        log = tag + log;
        UnityEngine.Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(levelColor.r * 255f), (byte)(levelColor.g * 255f), (byte)(levelColor.b * 255f), log));

        WriteFile(_logFilePath, $"[{DateTime.Now.ToString("hh:mm:ss")}]{log}{Environment.NewLine}");
    }

    static private Color GetLogColor(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.DEBUG:
                return Color.white;
            case LogLevel.INFO:
                return Color.green;
            case LogLevel.WARN:
                return Color.yellow;
            case LogLevel.ERROR:
                return Color.red;
            default:
                return Color.black;
        }
    }

    static public void Debug(LogTag logTag, string msg, params object[] param)
    {
#if _DEBUG
        WriteLog(LogLevel.DEBUG, logTag.ToString(), msg, param);
#endif
    }

    static public void Info(LogTag logTag, string msg, params object[] param)
    {
        WriteLog(LogLevel.INFO, logTag.ToString(), msg, param);
    }

    static public void Warn(LogTag logTag, string msg, params object[] param)
    {
        WriteLog(LogLevel.WARN, logTag.ToString(), msg, param);
    }

    static public void Error(LogTag logTag, string msg, params object[] param)
    {
        WriteLog(LogLevel.ERROR, logTag.ToString(), msg, param);
#if _SHOW_ERROR_PRODUCTION
        var log = msg;
        if (param.Length > 0)
        {
            log = string.Format(msg, param);
        }
        UnityEngine.Debug.LogError($"[{logTag}] {log}");
#endif
    }

    #region Overloading
    static public void Debug(string logTag, string msg, params object[] param)
    {
#if _DEBUG
        WriteLog(LogLevel.DEBUG, logTag, msg, param);
#endif
    }

    static public void Info(string logTag, string msg, params object[] param)
    {
        WriteLog(LogLevel.INFO, logTag, msg, param);
    }

    static public void Warn(string logTag, string msg, params object[] param)
    {
        WriteLog(LogLevel.WARN, logTag, msg, param);
    }

    static public void Error(string logTag, string msg, params object[] param)
    {
        WriteLog(LogLevel.ERROR, logTag, msg, param);
#if _SHOW_ERROR_PRODUCTION
        var log = msg;
        if (param.Length > 0)
        {
            log = string.Format(msg, param);
        }
        UnityEngine.Debug.LogError($"[{logTag}] {log}");
#endif
    }
    #endregion
}
