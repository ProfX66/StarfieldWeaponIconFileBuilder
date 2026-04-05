using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public partial class AppConfig
{
    #region Properties

    public static AppParams? ApplicationParameters { get; set; }
    public static AppState? ApplicationState { get; set; }
    public static LoggingData? LogConfig { get; set; }
    public static string? LocalPath { get; set; }
    public static int ExitCode { get; set; } = 0;
    public static Dictionary<string, string> InternalEnvVariables { get; set; } = [];

    [GeneratedRegex(";", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SplitColonRegEx();

    [GeneratedRegex("=", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SplitEqualsRegEx();

    #endregion

    #region Constructor

    /// <summary>
    /// Intialize the application configuration by parsing command line arguments and setting up logging based on the provided assembly and logging configuration.
    /// </summary>
    /// <param name="CommandLineArgs"></param>
    /// <param name="ThisAssembly"></param>
    /// <param name="LoggingConfig"></param>
    public AppConfig(string[] CommandLineArgs, Assembly? ThisAssembly, LoggingData? LoggingConfig)
    {
        ApplicationState = new AppState { Initializing = true } ?? null;
        if (!ThisAssembly.IsNullOrEmpty())
        {
            Logging.AppName = ThisAssembly.GetName().Name;
            var version = ThisAssembly.GetName().Version;
            Logging.AppVersionString = !version.IsNullOrEmpty() ? version.ToString() : "0.0.0.0";
            LocalPath = GetThisAppPath(ThisAssembly);
            InternalEnvVariables.TryAdd("%AppPath%", LocalPath ?? string.Empty);
        }
        ApplicationParameters = GetApplicationParams(CommandLineArgs);
        if (!LoggingConfig.IsNullOrEmpty())
        {
            LogConfig = LoggingConfig;
            LogConfig.MaxCount = GetParamValue("LogCount", LogConfig.MaxCount!)?.ToInt32();
            LogConfig.MaxSize = GetParamValue("LogMaxSize", LogConfig.MaxSize!)?.ToString();
            LogConfig.EventSource = GetParamValue("EventSource", "PXC")?.ToString();
            LogConfig.PathPrepend = GetParamValue("LogPathPrepend", LogConfig.PathPrepend!)?.ToString();
            LogConfig.PathAppend = GetParamValue("LogPathAppend", LogConfig.PathAppend!)?.ToString();
            LogConfig.FileName = GetParamValue("LogName", LogConfig.FileName!)?.ToString();
            LogConfig.FileNameAppend = GetParamValue("LogAppend", LogConfig.FileNameAppend!)?.ToString();

            string? DefaultLogPath = LocalPath;
            string? LocalAppData = "%LOCALAPPDATA%".ExpandVariables();
            if (!LogConfig.PathPrepend.IsNullOrEmpty()) { LocalAppData = LocalAppData.AppendPath(LogConfig.PathPrepend!); }
            LocalAppData = LocalAppData.AppendPath(Logging.AppName!);
            if (LogConfig.UseLocalAppData.GetValueOrDefault()) { DefaultLogPath = LocalAppData; }
            if (!LogConfig.Path.IsNullOrEmpty()) { DefaultLogPath = LogConfig.Path?.ExpandVariables(); }

            LogConfig.Path = GetParamValue("LogPath", DefaultLogPath!)?.ToString();
            if (!LogConfig.Path!.TestWrite()) LogConfig.Path = LocalAppData;
            if (!LogConfig.PathAppend.IsNullOrEmpty()) { LogConfig.Path = LogConfig.Path.AppendPath(LogConfig.PathAppend!); }

            if (!LogConfig.Verbose.GetValueOrDefault()) { LogConfig.Verbose = GetParamValue("Verbose", "false")?.ToBoolean(); }
            if (!LogConfig.Debug.GetValueOrDefault()) { LogConfig.Debug = GetParamValue("Debug", "false")?.ToBoolean(); }
            if (!LogConfig.WhatIf.GetValueOrDefault()) { LogConfig.WhatIf = GetParamValue("WhatIf", "false")?.ToBoolean(); }

            if (LogConfig.Debug.GetValueOrDefault()) { LogConfig.Verbose = true; }
            if (LogConfig.WhatIf.GetValueOrDefault()) { LogConfig.Verbose = true; LogConfig.Debug = true; }

            _ = Logging.Initialize(LogConfig);
            InternalEnvVariables.TryAdd("%LogPath%", LogConfig.Path ?? string.Empty);
        }

        if (!ApplicationParameters.FullCommandLine.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Informational($"FullCommandLine: {ApplicationParameters.FullCommandLine.ToFriendlyNull()}");
            Logging.Debug($"FullCommandLineRaw: {ApplicationParameters.FullCommandLineRaw.ToFriendlyNull()}");
            ApplicationParameters.ParameterTable?.LogEntries("Param", new LoggingData { Verbose = true });
            Logging.None(LogConfig!.Verbose.GetValueOrDefault());
        }

        Logging.Informational("Initialization completed!");
        Logging.None();
        if (!ApplicationState.IsNullOrEmpty()) ApplicationState.Initialized = true;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the currently executing directory with fallbacks to work with single-run embedded environment
    /// </summary>
    /// <param name="ThisAssembly"></param>
    /// <returns></returns>
    public static string GetThisAppPath(Assembly? ThisAssembly)
    {
        if (!Environment.ProcessPath.IsNullOrEmptyOrWhiteSpace())
        {
            return Environment.ProcessPath.GetDirectoryName()!;
        }

        using (Process process = Process.GetCurrentProcess())
        {
            if (!process.MainModule.IsNullOrEmpty())
                return process.MainModule.FileName.GetDirectoryName()!;
        }

        ThisAssembly ??= Assembly.GetExecutingAssembly() ?? Assembly.GetCallingAssembly();
        string? assemblyLocation = ThisAssembly.Location;
        if (!assemblyLocation.IsNullOrEmptyOrWhiteSpace())
        {
            return assemblyLocation.GetDirectoryName()!;
        }

        return AppContext.BaseDirectory;
    }

    /// <summary>
    /// Cache application command line and parameters into internal object
    /// </summary>
    /// <param name="CommandLineArgs"></param>
    /// <returns></returns>
    public static AppParams GetApplicationParams(string[] CommandLineArgs)
    {
        StringBuilder FullCommandLineParsed = new();
        Dictionary<string, object> dict = [];
        string switchPattern = @"^[-\/]";
        string paramNamePattern = @"(?<=[-\/])\S.*?(?=[=])";
        string? currentParam = null;
        string FullCommandLineRaw = string.Join(" ", CommandLineArgs.Skip(1)).Trim();

        IEnumerable<string> MatchedParams = CommandLineArgs.Where(a => Regex.IsMatch(a, switchPattern, RegexOptions.IgnoreCase));
        foreach (string param in MatchedParams)
        {
            Regex rx = new(paramNamePattern, RegexOptions.IgnoreCase);
            Match mp = rx.Match(param);

            if (mp.Success)
            {
                currentParam = mp.Value;
                string frontTrimPattern = string.Concat(switchPattern, mp.Value, "=");
                
                string finalValue = param.RegexReplace(frontTrimPattern, "");
                string frontExact = Regex.Match(param, frontTrimPattern, RegexOptions.IgnoreCase).Value;

                if (finalValue.IsRegexMatch(@"\{"))
                {
                    string extendedParamPattern = string.Concat(@"(?<=\", Regex.Escape(frontExact), @"\{).*?(?=\})");
                    Regex rxExtended = new(extendedParamPattern, RegexOptions.IgnoreCase);
                    Match mpExtended = rxExtended.Match(FullCommandLineRaw);
                    if (mpExtended.Success)
                    {
                        string Rebuilt = string.Concat(" ", frontExact, "{", mpExtended.Value, "} ");
                        FullCommandLineParsed.Append(Rebuilt);

                        Dictionary<string, object> secDict = [];
                        string[] SubParmSplit = SplitColonRegEx().Split(mpExtended.Value);
                        foreach (string item in SubParmSplit)
                        {
                            if (item.IsRegexMatch("="))
                            {
                                string[] NameValueSplit = SplitEqualsRegEx().Split(item);
                                object ParamValue = NameValueSplit.GetLastItem();
                                if (ParamValue.IsRegexMatch("true|false")) { ParamValue = ParamValue.ToBoolean(); }
                                if (ParamValue.IsRegexMatch("^[0-9]$")) { ParamValue = ParamValue.ToInt32(); }
                                secDict.Add(NameValueSplit.GetFirstItem(), ParamValue);
                            }
                        }
                        dict.Add(mp.Value, secDict);
                    }
                }
                else
                {
                    dict.Add(mp.Value, finalValue);
                    FullCommandLineParsed.Append(string.Concat(" /", mp.Value, "=\"", finalValue, "\""));
                }
            }
        }

        foreach (string sParam in CommandLineArgs.Skip(1).Where(p => p.IsRegexMatch(switchPattern) && !p.IsRegexMatch("=")))
        {
            FullCommandLineParsed.Append(string.Concat(" /", sParam.RegexReplace(switchPattern, ""), "=\"True\""));
            dict.TryAdd(sParam.RegexReplace(switchPattern, ""), "True");
        }

        return new AppParams
        {
            FullCommandLine = FullCommandLineParsed.ToString().Trim(),
            FullCommandLineRaw = FullCommandLineRaw,
            ParameterTable = dict
        };
    }

    /// <summary>
    /// Parameter retrieval method with optional default value. Logs the retrieved parameter value or the default if not found.
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="DefaultValue"></param>
    /// <returns></returns>
    public static object? GetParamValue(string Name, object DefaultValue)
    {
        object returnValue = DefaultValue;
        Dictionary<string, object>? ParameterTable = ApplicationParameters?.ParameterTable;
        if (ParameterTable.IsNullOrEmpty()) return returnValue;
        
        object aReturnData = ParameterTable!.FirstOrDefault(a => a.Key.IsRegexMatch($"^{Name}$")).Value;
        Logging.Verbose($"[Arg] {Name}: {aReturnData.ToFriendlyNull()}");
        if (!aReturnData.IsNullOrEmpty())
        {
            returnValue = aReturnData;
            Logging.Informational($"[Parameter] {Name}: {returnValue.ToFriendlyNull()}");
        }
        Logging.None(LogConfig!.Verbose.GetValueOrDefault());

        return returnValue;
    }

    /// <summary> 
    /// Dispose any objects and exit the application
    /// </summary>
    public static void DisposeAndExit(int code = 0)
    {
        if (ExitCode > code) { code = ExitCode; }
        if (!ApplicationState.IsNullOrEmpty()) ApplicationState.ContentRendered = false;
        Logging.Dispose(code);
        Environment.Exit(code);
    }

    /// <summary> 
    /// Dispose any objects and exit the application asynchronously
    /// </summary>
    public static async Task DisposeAndExitAsync(int code = 0)
    {
        if (ExitCode > code) { code = ExitCode; }
        if (!ApplicationState.IsNullOrEmpty() && ApplicationState.ContentRendered.GetValueOrDefault())
        {
            ApplicationState.ContentRendered = false;
            await Logging.DisposeAsync();
            Environment.Exit(code);
        }
    }

    #endregion
}

