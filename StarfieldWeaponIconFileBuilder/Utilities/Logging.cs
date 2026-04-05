using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static class Logging
{
    #region Properties

    #region AppInfo

    public static string? AppName { get; set; }
    public static string? AppVersionString { get; set; }
    public static Version? AppVersion { get; set; }

    #endregion

    #region Constants

    private const string LogExtension = ".log";
    private const string InformationalId = " INF ";
    private const string AdvisoryId = " ADV ";
    private const string ErrorId = " ERR ";
    private const string VerboseId = " VBS ";
    private const string DebugId = " DBG ";
    private const string IWhatIfId = " WIF ";
    private const string NoneId = "-----";

    #endregion

    #region Logging Properties

    public static string LineFormat => "{0} |{1}| {2}";
    public static string RollFormat => "{0}.{1}{2}";
    public static string DateTimeFormat => "yyyy/MM/dd HH:mm:ss.ff";
    public static string FirstLinePattern => "Log file for: {0} v{1}";
    public static LoggingData DefaultLogLevel => new() { Informational = true };
    public static int SeparatorLineLength => 169;
    public static string? PreferredEventSource { get; set; }
    public static string? LogFilePath { get; set; }
    public static string? LogDirectory { get; set; }
    public static int MaxLogRoll { get; set; }
    public static int? AppProcessId { get; set; }
    public static bool AllowDynamicRoll { get; set; } = false;
    public static bool IsInitialized { get; set; }
    public static bool IsPsInitialized { get; set; }
    public static bool IsConsoleAvailable { get; set; }
    public static LoggingData? LogConfig { get; set; }
    private static readonly SemaphoreSlim LogSemaphore = new(1);

    #endregion

    #endregion

    #region Methods

    #region Initialization

    /// <summary>
    /// Initializes logging for the assembly
    /// </summary>
    /// <param name="LogggingPath"></param>
    /// <param name="Level"></param>
    /// <param name="FileNameAppend"></param>
    /// <param name="InstanceRoll"></param>
    /// <param name="MaxCount"></param>
    /// <param name="MaxSize"></param>
    /// <param name="EventSource"></param>
    /// <returns></returns>
    public static async Task Initialize(LoggingData LogSettings)
    {
        if (IsInitialized) return;
        LogConfig = LogSettings;
        AppName = AppName.IsNullOrEmpty() ? Assembly.GetExecutingAssembly().GetName().Name : AppName;
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersionString = AppVersionString.IsNullOrEmpty() ? (version?.ToString() ?? "0.0.0.0") : AppVersionString;
        AppVersion = AppVersion ?? version ?? new Version(0, 0, 0, 0);
        AppProcessId = AppProcessId.HasValue ? AppProcessId : GetAppProcessId();
        PreferredEventSource = PreferredEventSource.IsNullOrEmpty() ? LogConfig.EventSource : PreferredEventSource;
        LogDirectory = LogDirectory.IsNullOrEmpty() ? LogConfig.Path : LogDirectory;
        LogConfig.FileName = GetLogFileName(LogConfig.FileName!);
        LogFilePath = Path.Combine(LogConfig.Path!, LogConfig.FileName);
        bool DidSizeRoll = false;

        if (LogConfig.Interactive.GetValueOrDefault() && LogConfig.InteractiveDetection.GetValueOrDefault())
        {
            if (!IsConsoleAvailable) LogConfig.Interactive = false;
        }

        LogDirectory.TryCreateDirectory(silent: true);
        if (File.Exists(LogFilePath))
        {
            if (LogConfig.InstanceRoll.GetValueOrDefault()) { await InstanceRollLogFiles(LogConfig.MaxCount); }
            else { DidSizeRoll = await SizeRollLogFiles(LogConfig.MaxCount, LogConfig.MaxSize!.ToRawSizeLong()); }
        }
        IsInitialized = true;

        if (!DidSizeRoll && File.Exists(LogFilePath))
        {
            await NoneAsync();
            await NoneAsync("---[ New Instance ]-".PadToLength(SeparatorLineLength));
            await NoneAsync();
        }

        string RollMethod = "Each Instance";
        if (!LogConfig.InstanceRoll.GetValueOrDefault()) { RollMethod = $"Size >= {LogConfig.MaxSize!.ToUpper()}"; }

        await InformationalAsync(string.Format(FirstLinePattern, AppName, AppVersionString), !LogConfig.InstanceRoll.GetValueOrDefault());
        await InformationalAsync($"PID: {AppProcessId}");
        await InformationalAsync($"Interactive? {LogConfig.Interactive}");
        await InformationalAsync($"Log Roll Method: {RollMethod}");
        await VerboseAsync($"Verbose? {LogConfig.Verbose}");
        await DebugAsync($"Debug? {LogConfig.Debug}");
        await WhatIfAsync($"WhatIf? {LogConfig.WhatIf}");
        await NoneAsync();
        await InformationalAsync($"AppPath: {"%AppPath%".ExpandVariables()}");
        await InformationalAsync($"LogFilePath: {LogFilePath}");
        await NoneAsync();

        AllowDynamicRoll = true;
    }

    /// <summary>
    /// Returns the file name for the log file
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns>string</returns>
    private static string GetLogFileName(string FileName)
    {
        if (FileName.IsNullOrEmpty())
        {
            return string.Concat(AppName, LogConfig?.FileNameAppend, LogExtension);
        }

        if (FileName.IsRegexMatch(LogExtension.RegexEscape()))
        {
            string fileNameAppend = LogConfig?.FileNameAppend ?? string.Empty;
            return FileName.RegexReplace($"({LogExtension.RegexEscape()})?$", string.Concat(fileNameAppend, LogExtension), RegexOptions.IgnoreCase);
        }

        return string.Concat(FileName, LogConfig?.FileNameAppend, LogExtension);
    }

    /// <summary>
    /// Rolls the log files up to the passed max count
    /// </summary>
    /// <param name="MaxCount"></param>
    /// <returns></returns>
    public static async Task InstanceRollLogFiles(int? MaxCount = null)
    {
        MaxCount ??= LogConfig?.MaxCount;
        if (string.IsNullOrEmpty(LogDirectory) || string.IsNullOrEmpty(LogFilePath))
        {
            throw new InvalidOperationException("LogDirectory and LogFilePath must not be null or empty.");
        }
        string PrimaryLogWithoutExtension = Path.Combine(LogDirectory, Path.GetFileNameWithoutExtension(LogFilePath));
        string MaxLogFileName = string.Format(RollFormat, PrimaryLogWithoutExtension, MaxCount.GetValueOrDefault().ToString("D2"), LogExtension);
        string MinLogFileName = string.Format(RollFormat, PrimaryLogWithoutExtension, 1.ToString("D2"), LogExtension);
        await MaxLogFileName.TryDeletePathAsync();

        for (int i = MaxCount.GetValueOrDefault() - 1; i >= 1; i--)
        {
            string ThisLogFile = string.Format(RollFormat, PrimaryLogWithoutExtension, i.ToString("D2"), LogExtension);
            string NextLogFile = string.Format(RollFormat, PrimaryLogWithoutExtension, (i + 1).ToString("D2"), LogExtension);
            await ThisLogFile.TryMovePathAsync(NextLogFile);
        }

        await LogFilePath.TryMovePathAsync(MinLogFileName);
    }

    /// <summary>
    /// Validates the log size and rolls up to passed max count when size exceeds passed max size
    /// </summary>
    /// <param name="MaxCount"></param>
    /// <param name="MaxSize"></param>
    /// <returns>Bool</returns>
    public static async Task<bool> SizeRollLogFiles(int? MaxCount = null, long? MaxSize = null)
    {
        bool returnValue = false;
        MaxCount = MaxCount ?? LogConfig?.MaxCount;
        MaxSize = MaxSize ?? LogConfig?.MaxSize!.ToRawSizeLong();
        var fi = LogFilePath?.ToFileInfo();
        if (fi.IsNullOrEmpty()) return returnValue;

        if (fi.Length >= MaxSize)
        {
            IsInitialized = true;
            await NoneAsync();
            await NoneAsync("-".PadToLength(SeparatorLineLength, '-'));
            await AdvisoryAsync($"Max log size [ {fi.Length.ToFriendlySize()} >= {MaxSize.ToFriendlySize()} ] exceeded - Rolling log...");
            await NoneAsync("-".PadToLength(SeparatorLineLength, '-'));
            await InstanceRollLogFiles(MaxCount);
            returnValue = true;
        }

        return returnValue;
    }

    /// <summary>
    /// Gets this executing assembly Process Identifier
    /// </summary>
    /// <returns>Nullable Int</returns>
    private static int? GetAppProcessId()
    {
        try { return Environment.ProcessId; }
        catch (Exception Ex)
        {
            EventException(new Exception("Exception attempting to get the current Process ID", Ex), 66);
        }
        return null;
    }

    #endregion

    #region Log Level

    /// <summary>
    /// Formats the log line with date and type identifiers
    /// </summary>
    /// <param name="Line"></param>
    /// <param name="Type"></param>
    /// <param name="Append"></param>
    /// <returns>LogEntryData</returns>
    private static LogEntryData LineData(string Line, string Type, bool Append = true)
    {
        Line = string.Format(LineFormat, DateTime.Now.ToString(DateTimeFormat), Type, Line);
        return new LogEntryData { Text = Line.ToFriendlyNull(), Append = Append };
    }

    /// <summary>
    /// Writes an Informational log line
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void Informational(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        Write(LineData(Text, InformationalId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Informational log line asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task InformationalAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        await WriteAsync(LineData(Text, InformationalId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Advisory log line
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void Advisory(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        Write(LineData(Text, AdvisoryId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Advisory log line asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task AdvisoryAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        await WriteAsync(LineData(Text, AdvisoryId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Error log line
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void Error(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        Write(LineData(Text, ErrorId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Error log line asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task ErrorAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        await WriteAsync(LineData(Text, ErrorId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Verbose log line if Verbose is true
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void Verbose(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!LogConfig!.Verbose.GetValueOrDefault()) { return; }
        Write(LineData(Text, VerboseId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Verbose log line if Verbose is true asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task VerboseAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!LogConfig!.Verbose.GetValueOrDefault()) { return; }
        await WriteAsync(LineData(Text, VerboseId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Debug log line if Debug is true
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void Debug(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!LogConfig!.Debug.GetValueOrDefault()) { return; }
        Write(LineData(Text, DebugId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an Debug log line if Debug is true asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task DebugAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!LogConfig!.Debug.GetValueOrDefault()) { return; }
        await WriteAsync(LineData(Text, DebugId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an WhatIf log line if WhatIf is true
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void WhatIf(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!LogConfig!.WhatIf.GetValueOrDefault()) { return; }
        Write(LineData(Text, IWhatIfId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes an WhatIf log line if WhatIf is true asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task WhatIfAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!LogConfig!.WhatIf.GetValueOrDefault()) { return; }
        await WriteAsync(LineData(Text, IWhatIfId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes a blank log line
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void None(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        Write(LineData(Text, NoneId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes a blank log line asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task NoneAsync(string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        await WriteAsync(LineData(Text, NoneId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes a blank log line if passed bool is true
    /// </summary>
    /// <param name="Allow"></param>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    public static void None(bool Allow, string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!Allow) { return; }
        Write(LineData(Text, NoneId, Append), UseSemaphore);
    }

    /// <summary>
    /// Writes a blank log line if passed bool is true asynchronously
    /// </summary>
    /// <param name="Allow"></param>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task NoneAsync(bool Allow, string Text = "", bool Append = true, bool UseSemaphore = true)
    {
        if (!IsInitialized) { return; }
        if (!Allow) { return; }
        await WriteAsync(LineData(Text, NoneId, Append), UseSemaphore);
    }

    #endregion

    #region Exception

    /// <summary>
    /// Builds a list of log lines with all passed exception data
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="ExType"></param>
    /// <returns>List<string></returns>
    public static List<string> BuildExceptionLogLines(System.Exception Ex, string ExType = "Exception")
    {
        List<string> ReturnObj =
        [
            $">>>[ {ExType} Details ]>".PadToLength(SeparatorLineLength, '>'),
            $"Message   : {Ex.Message.ToFriendlyNull().RegexReplace(@"\r\n+", " => ")}"
        ];

        if (!Ex.GetType().FullName.IsNullOrEmpty()) { ReturnObj.Add($"Type      : {Ex.GetType().FullName}"); }
        if (!Ex.Source.IsNullOrEmpty()) { ReturnObj.Add($"Source    : {Ex.Source}"); }
        if (!Ex.TargetSite.IsNullOrEmpty()) { ReturnObj.Add($"TargetSite: {Ex.TargetSite}"); }
        if (!Ex.StackTrace.IsNullOrEmpty())
        {
            foreach (string trace in Ex.StackTrace!.Split('\r', '\n'))
            {
                if (!trace.Trim().IsNullOrWhiteSpace()) ReturnObj.Add($"StackTrace: {trace.Trim()}");
            }
        }

        if (Ex is ArgumentException argumentException)
        {
            ReturnObj.Add($"Invalid Argument: {argumentException.ParamName}");
        }
        else if (Ex is InvalidOperationException invalidOperationException)
        {
            ReturnObj.Add($"Invalid Operation: {invalidOperationException.Message}");
        }

        if (!Ex.InnerException.IsNullOrEmpty())
        {
            List<string> TempObj = BuildExceptionLogLines(Ex.InnerException, "Inner Exception");
            ReturnObj.AddRange(TempObj);
        }
        else { ReturnObj.Add("<".PadToLength(SeparatorLineLength, '<')); }

        return ReturnObj;
    }

    /// <summary>
    /// Builds a list of log lines with all passed exception data asynchronously
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="ExType"></param>
    /// <returns>List<string></returns>
    public static async Task<List<string>> BuildExceptionLogLinesAsync(System.Exception Ex, string ExType = "Exception")
    {
        List<string> ReturnObj = [];

        await Task.Run(() => {
            ReturnObj = BuildExceptionLogLines(Ex, ExType);
        });

        return ReturnObj;
    }

    /// <summary>
    /// Writes the built exception data to the log
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="Append"></param>
    public static void Exception(System.Exception Ex, bool Append = true)
    {
        if (!IsInitialized) { return; }
        List<string> ExceptionLogLines = BuildExceptionLogLines(Ex);
        foreach (string Line in ExceptionLogLines)
        {
            Error(Line, Append);
        }
    }

    /// <summary>
    /// Writes the built exception data to the log asynchronously
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="ExType"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task ExceptionAsync(System.Exception Ex, bool Append = true)
    {
        if (!IsInitialized) { return; }
        List<string> ExceptionLogLines = await BuildExceptionLogLinesAsync(Ex);
        foreach (string Line in ExceptionLogLines)
        {
            await ErrorAsync(Line, Append);
        }
    }

    /// <summary>
    /// Writes the built exception data to the event log
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="Id"></param>
    /// <param name="Log"></param>
    /// <param name="Source"></param>
    public static void EventException(System.Exception Ex, int Id = 1, string Log = "Application", string Source = "")
    {
        List<string> ExceptionLines = BuildExceptionLogLines(Ex);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Source.IsNullOrEmpty()) { Source = PreferredEventSource!; }
            StringBuilder ExceptionStrings = new();
            ExceptionStrings.AppendLineList(ExceptionLines);
            Event(ExceptionStrings.ToString(), Id, EventLogEntryType.Error, Log, Source);
        }
        else
        {
            WriteToFallbackLog(ExceptionLines);
        }
    }

    /// <summary>
    /// Writes the built exception data to the event log asynchronously
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="Id"></param>
    /// <param name="Log"></param>
    /// <param name="Source"></param>
    /// <returns></returns>
    public static async Task EventExceptionAsync(System.Exception Ex, int Id = 1, string Log = "Application", string Source = "")
    {
        List<string> ExceptionLines = await BuildExceptionLogLinesAsync(Ex);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Source.IsNullOrEmpty()) { Source = PreferredEventSource!; }
            StringBuilder ExceptionStrings = new();
            ExceptionStrings.AppendLineList(ExceptionLines);
            await EventAsync(ExceptionStrings.ToString(), Id, EventLogEntryType.Error, Log, Source);
        }
        else
        {
            WriteToFallbackLog(ExceptionLines);
        }
    }

    #endregion

    #region File Write

    /// <summary>
    /// Writes the actual log file to the file system
    /// </summary>
    /// <param name="Line"></param>
    private static void Write(LogEntryData Line, bool UseSemaphore = true)
    {
        if (LogConfig!.Interactive.GetValueOrDefault()) { Console.WriteLine(Line.Text); }
        if (UseSemaphore) LogSemaphore.Wait();

        try
        {
            using StreamWriter sw = new(LogFilePath!, Line.Append.GetValueOrDefault());
            sw.WriteLine(Line.Text);
        }
        catch (Exception Ex)
        {
            StringBuilder sb = new();
            sb.AppendLine($"Exception writing log line: {Line.Text}");
            sb.AppendLineList(BuildExceptionLogLines(Ex));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Event(sb.ToString(), 1, EventLogEntryType.Error, silent: true);
            }
            else
            {
                WriteToFallbackLog(sb.ToString());
            }
        }
        finally
        {
            if (UseSemaphore) LogSemaphore.Release();
        }
    }

    /// <summary>
    /// Writes the actual log file to the file system asynchronously
    /// </summary>
    /// <param name="Line"></param>
    /// <returns></returns>
    private static async Task WriteAsync(LogEntryData Line, bool UseSemaphore = true)
    {
        if (LogConfig!.Interactive.GetValueOrDefault()) { Console.WriteLine(Line.Text); }
        if (UseSemaphore) LogSemaphore.Wait();

        try
        {
            using StreamWriter sw = new(LogFilePath!, Line.Append.GetValueOrDefault());
            await sw.WriteLineAsync(Line.Text);
        }
        catch (Exception Ex)
        {
            StringBuilder sb = new();
            sb.AppendLine($"Exception writing log line: {Line.Text}");
            sb.AppendLineList(BuildExceptionLogLines(Ex));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await EventAsync(sb.ToString(), 1, EventLogEntryType.Error, silent: true);
            }
            else
            {
                WriteToFallbackLog(sb.ToString());
            }
        }
        finally
        {
            if (UseSemaphore) LogSemaphore.Release();
        }
    }

    #endregion

    #region Event Log Write

    /// <summary>
    /// Writes the passed message to the event log
    /// </summary>
    /// <param name="Message"></param>
    /// <param name="Id"></param>
    /// <param name="Type"></param>
    /// <param name="Log"></param>
    /// <param name="Source"></param>
    public static void Event(string Message, int Id = 0, EventLogEntryType? Type = null, string Log = "Application", string? Source = null, bool silent = false)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Source.IsNullOrEmpty()) { Source = PreferredEventSource; }
            if (Type.IsNullOrEmpty()) { Type = EventLogEntryType.Information; }
            if (!EventLog.SourceExists(Source))
            {
                if (!silent) Advisory($"Attempting to create EventLog source [ {Source} ] in log [ {Log} ]");
                try { EventLog.CreateEventSource(Source, Log); }
                catch (Exception Ex) { Exception(Ex); }
            }

            string LogMessage = Message;
            if (LogMessage.IsRegexMatch(@"\n")) { LogMessage = string.Concat("\n", Message); }

            switch (Type)
            {
                case EventLogEntryType.Information:
                    if (!silent) Informational($"Writing [ {LogMessage} ] with Source [ {Source} ] and ID [ {Id} ] to EventLog [ {Log} ]");
                    break;
                case EventLogEntryType.Warning:
                    if (!silent) Advisory($"Writing [ {LogMessage} ] with Source [ {Source} ] and ID [ {Id} ] to EventLog [ {Log} ]");
                    break;
                case EventLogEntryType.Error:
                    if (!silent) Error($"Writing [ {LogMessage} ] with Source [ {Source} ] and ID [ {Id} ] to EventLog [ {Log} ]");
                    break;
            }

            try
            {
                using EventLog eventLog = new(Log);
                eventLog.Source = Source;
                eventLog.WriteEntry(Message, Type.GetValueOrDefault(), Id);
            }
            catch (Exception Ex) { Exception(Ex); }
        }
        else
        {
            if (!silent)
            {
                Advisory("Event logging is only supported on Windows platforms.");
            }
        }
    }

    /// <summary>
    /// Writes the passed message to the event log asynchronously
    /// </summary>
    /// <param name="Message"></param>
    /// <param name="Id"></param>
    /// <param name="Type"></param>
    /// <param name="Log"></param>
    /// <param name="Source"></param>
    /// <returns></returns>
    public static async Task EventAsync(string Message, int Id = 0, EventLogEntryType? Type = null, string Log = "Application", string? Source = null, bool silent = false)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Source.IsNullOrEmpty()) { Source = PreferredEventSource; }
            if (Type.IsNullOrEmpty()) { Type = EventLogEntryType.Information; }
            if (!EventLog.SourceExists(Source))
            {
                if (!silent) await AdvisoryAsync($"Attempting to create EventLog source [ {Source} ] in log [ {Log} ]");
                try { EventLog.CreateEventSource(Source, Log); }
                catch (Exception Ex) { Exception(Ex); }
            }

            string LogMessage = Message;
            if (LogMessage.IsRegexMatch(@"\n")) { LogMessage = string.Concat("\n", Message); }

            switch (Type)
            {
                case EventLogEntryType.Information:
                    if (!silent) await InformationalAsync($"Writing [ {LogMessage} ] with Source [ {Source} ] and ID [ {Id} ] to EventLog [ {Log} ]");
                    break;
                case EventLogEntryType.Warning:
                    if (!silent) await AdvisoryAsync($"Writing [ {LogMessage} ] with Source [ {Source} ] and ID [ {Id} ] to EventLog [ {Log} ]");
                    break;
                case EventLogEntryType.Error:
                    if (!silent) await ErrorAsync($"Writing [ {LogMessage} ] with Source [ {Source} ] and ID [ {Id} ] to EventLog [ {Log} ]");
                    break;
            }

            try
            {
                await Task.Run(() => {
#pragma warning disable CA1416 // Validate platform compatibility
                    using EventLog eventLog = new(Log);
                    eventLog.Source = Source;
                    eventLog.WriteEntry(Message, Type.GetValueOrDefault(), Id);
#pragma warning restore CA1416 // Validate platform compatibility
                });
            }
            catch (Exception Ex) { await ExceptionAsync(Ex); }
        }
        else
        {
            if (!silent)
            {
                await AdvisoryAsync("Event logging is only supported on Windows platforms.");
            }
        }
    }

#endregion

    #region Dynamic

    /// <summary>
    /// Writes a dynamic typed log line
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="LogData"></param>
    /// <param name="Append"></param>
    public static void Dynamic(string Text = "", LoggingData? LogData = null, bool Append = true)
    {
        bool WroteLine = false;
        LogData ??= new LoggingData { Informational = true };
        if (Text.IsNullOrEmptyOrWhiteSpace()) { None(); WroteLine = true; }
        if (LogData.Informational.GetValueOrDefault() && !WroteLine) { Informational(Text, Append); WroteLine = true; }
        if (LogData.Advisory.GetValueOrDefault() && !WroteLine) { Advisory(Text, Append); WroteLine = true; }
        if (LogData.Error.GetValueOrDefault() && !WroteLine) { Error(Text, Append); WroteLine = true; }
        if (LogData.Verbose.GetValueOrDefault() && !WroteLine) { Verbose(Text, Append); WroteLine = true; }
        if (LogData.Debug.GetValueOrDefault() && !WroteLine) { Debug(Text, Append); WroteLine = true; }
        if (LogData.WhatIf.GetValueOrDefault() && !WroteLine) { WhatIf(Text, Append); }
    }

    /// <summary>
    /// Writes a dynamic typed log line asynchronously
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task DynamicAsync(string Text = "", LoggingData? LogData = null, bool Append = true)
    {
        bool WroteLine = false;
        LogData ??= new LoggingData { Informational = true };
        if (Text.IsNullOrEmptyOrWhiteSpace()) { await NoneAsync(); WroteLine = true; }
        if (LogData.Informational.GetValueOrDefault() && !WroteLine) { await InformationalAsync(Text, Append); WroteLine = true; }
        if (LogData.Advisory.GetValueOrDefault() && !WroteLine) { await AdvisoryAsync(Text, Append); WroteLine = true; }
        if (LogData.Error.GetValueOrDefault() && !WroteLine) { await ErrorAsync(Text, Append); WroteLine = true; }
        if (LogData.Verbose.GetValueOrDefault() && !WroteLine) { await VerboseAsync(Text, Append); WroteLine = true; }
        if (LogData.Debug.GetValueOrDefault() && !WroteLine) { await DebugAsync(Text, Append); WroteLine = true; }
        if (LogData.WhatIf.GetValueOrDefault() && !WroteLine) { await WhatIfAsync(Text, Append); }
    }

    /// <summary>
    /// Writes a dynamic typed log line only after the passed time has elapsed
    /// </summary>
    /// <param name="Left"></param>
    /// <param name="Right"></param>
    /// <param name="Interval"></param>
    /// <param name="Text"></param>
    /// <param name="LogData"></param>
    /// <param name="Append"></param>
    public static void Dynamic(TimeSpan Left, TimeSpan Right, TimeSpan Interval, string Text = "", LoggingData? LogData = null, bool Append = true)
    {
        if (Left.GetPositiveTimeSpanDifference(Right) >= Interval) return;
        bool WroteLine = false;
        LogData ??= new LoggingData { Informational = true };
        if (Text.IsNullOrEmptyOrWhiteSpace()) { None(); WroteLine = true; }
        if (LogData.Informational.GetValueOrDefault() && !WroteLine) { Informational(Text, Append); WroteLine = true; }
        if (LogData.Advisory.GetValueOrDefault() && !WroteLine) { Advisory(Text, Append); WroteLine = true; }
        if (LogData.Error.GetValueOrDefault() && !WroteLine) { Error(Text, Append); WroteLine = true; }
        if (LogData.Verbose.GetValueOrDefault() && !WroteLine) { Verbose(Text, Append); WroteLine = true; }
        if (LogData.Debug.GetValueOrDefault() && !WroteLine) { Debug(Text, Append); WroteLine = true; }
        if (LogData.WhatIf.GetValueOrDefault() && !WroteLine) { WhatIf(Text, Append); }
    }

    /// <summary>
    /// Writes a dynamic typed log line only after the passed time has elapsed asynchronously
    /// </summary>
    /// <param name="Left"></param>
    /// <param name="Right"></param>
    /// <param name="Interval"></param>
    /// <param name="Text"></param>
    /// <param name="LogData"></param>
    /// <param name="Append"></param>
    /// <returns></returns>
    public static async Task DynamicAsync(TimeSpan Left, TimeSpan Right, TimeSpan Interval, string Text = "", LoggingData? LogData = null, bool Append = true)
    {
        if (Left.GetPositiveTimeSpanDifference(Right) >= Interval)
        {
            bool WroteLine = false;
            LogData ??= new LoggingData { Informational = true };
            if (Text.IsNullOrEmptyOrWhiteSpace()) { await NoneAsync(); WroteLine = true; }
            if (LogData.Informational.GetValueOrDefault() && !WroteLine) { await InformationalAsync(Text, Append); WroteLine = true; }
            if (LogData.Advisory.GetValueOrDefault() && !WroteLine) { await AdvisoryAsync(Text, Append); WroteLine = true; }
            if (LogData.Error.GetValueOrDefault() && !WroteLine) { await ErrorAsync(Text, Append); WroteLine = true; }
            if (LogData.Verbose.GetValueOrDefault() && !WroteLine) { await VerboseAsync(Text, Append); WroteLine = true; }
            if (LogData.Debug.GetValueOrDefault() && !WroteLine) { await DebugAsync(Text, Append); WroteLine = true; }
            if (LogData.WhatIf.GetValueOrDefault() && !WroteLine) { await WhatIfAsync(Text, Append); }
        }
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Writes the final log lines before exiting
    /// </summary>
    public static void Dispose(int code = 0)
    {
        None();
        Informational("Disposing objects...");
        Informational($"Exiting with RC [ {code} ]");
    }

    /// <summary>
    /// Writes the final log lines before exiting asynchronously
    /// </summary>
    /// <returns></returns>
    public static async Task DisposeAsync(int code = 0)
    {
        await NoneAsync();
        await InformationalAsync("Disposing objects...");
        await InformationalAsync($"Exiting with RC [ {code} ]");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Returns the calling method name
    /// </summary>
    /// <param name="memberName"></param>
    /// <returns></returns>
    public static string GetCurrentMethodName([CallerMemberName] string memberName = "")
    {
        return memberName;
    }

    /// <summary>
    /// Writes the passed string to a fallback log file in a guid temp folder
    /// </summary>
    /// <returns></returns>
    public static void WriteToFallbackLog(string InputString)
    {
        string fallbackPath = Path.GetTempPath().AppendPath($"{AppName}_{Guid.NewGuid():N}");
        string fallbackFilePath = fallbackPath.AppendPath($"{AppName}-LogFailure.log");
        fallbackPath.TryCreateDirectory();
        fallbackPath.DeleteOnReboot();
        try
        {
            File.AppendAllText(fallbackFilePath, InputString + Environment.NewLine);
        }
        catch
        {
            Console.WriteLine($"Failed to write to fallback log at [ {fallbackFilePath} ] with lines:\n\n{InputString}");
        }
    }

    /// <summary>
    /// Writes the passed list of strings to a fallback log file in a guid temp folder
    /// </summary>
    /// <param name="InputObject"></param>
    public static void WriteToFallbackLog(IEnumerable<string> InputObject)
    {
        string fallbackPath = Path.GetTempPath().AppendPath($"{AppName}_{Guid.NewGuid():N}");
        string fallbackFilePath = fallbackPath.AppendPath($"{AppName}-LogFailure.log");
        string content = string.Join(Environment.NewLine, InputObject);
        fallbackPath.TryCreateDirectory();
        fallbackPath.DeleteOnReboot();
        try
        {
            File.AppendAllText(fallbackFilePath, content);
        }
        catch
        {
            Console.WriteLine($"Failed to write to fallback log at [ {fallbackFilePath} ] with lines:\n\n{content}");
        }
    }

    #endregion

    #endregion
}
