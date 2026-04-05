using CsvHelper;
using CsvHelper.Configuration;
using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static partial class Flash
{
    #region Properties

    public static string? FfdecPath { get; set; } = @"C:\Program Files (x86)\FFDec\ffdec.jar";
    public static string? JavaPath { get; set; } = "Java.exe";
    public static List<string> ExpectedExtensions { get; set; } = ["swf", "gfx"];

    [GeneratedRegex(@"dynamic\ class\s+(?<value>.*?)\s+extends\ MovieClip")]
    private static partial Regex ASClassName();

    #endregion

    #region Methods

    #region Export

    /// <summary>
    /// Exports all scripts from the passed flash file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public static FlashResult? ExportScripts(string path, string? destination = null)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        bool UseTemp = false;
        if (destination.IsNullOrEmptyOrWhiteSpace())
        {
            destination = Path.GetTempPath().AppendPath($"SwfExport_{Guid.NewGuid():N}", path.GetFileName().RegexReplace(@"\.", "_"));
            UseTemp = true;
        }
        destination.TryCreateDirectory();

        string fileName = path.GetFileName();
        string workDir = destination!;
        if (UseTemp) workDir = destination!.AppendPath(fileName.RegexReplace(@"\.", "_"));
        workDir.TryCreateDirectory(temp: UseTemp);
        Logging.Informational($"Exporting scripts from [ {path} ] to: {workDir}");
        ProcessData? result = InvokeFfdec($"-format script:as -export script \"{workDir}\" \"{path}\"");
        
        return new FlashResult
        {
            Action = Logging.GetCurrentMethodName(),
            SourcePath = path,
            DestinationPath = workDir,
            ExportedPath = workDir.AppendPath("scripts"),
            Result = result?.RC == 0
        };
    }

    /// <summary>
    /// Exports the symbol-class mapping from the passed flash file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public static FlashResult? ExportSymbolClassMapping(string path, string? destination = null)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        bool UseTemp = false;
        if (destination.IsNullOrEmptyOrWhiteSpace())
        {
            destination = Path.GetTempPath().AppendPath($"SwfExport_{Guid.NewGuid():N}", path.GetFileName().RegexReplace(@"\.", "_"));
            UseTemp = true;
        }
        destination.TryCreateDirectory();

        string fileName = path.GetFileName();
        string workDir = destination!;
        if (UseTemp) workDir = destination!.AppendPath(fileName.RegexReplace(@"\.", "_"));
        workDir.TryCreateDirectory(temp: UseTemp);
        Logging.Informational($"Exporting Symbol-Class mapping from [ {path} ] to: {workDir}");
        ProcessData? result = InvokeFfdec($"-export symbolClass \"{workDir}\" \"{path}\"");
        
        return new FlashResult
        {
            Action = Logging.GetCurrentMethodName(),
            SourcePath = path,
            DestinationPath = workDir,
            ExportedPath = workDir.AppendPath("symbols.csv"),
            Result = result?.RC == 0
        };
    }

    #endregion

    #region Import

    /// <summary>
    /// Imports scripts into the passed flash file from the passed script path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="scriptPath"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public static FlashResult? ImportScripts(string path, string scriptPath, string? destination = null)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (scriptPath.IsNullOrEmptyOrWhiteSpace() || !scriptPath.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {scriptPath.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        bool UseTemp = false;
        if (destination.IsNullOrEmptyOrWhiteSpace())
        {
            destination = Path.GetTempPath().AppendPath($"SwfExport_{Guid.NewGuid():N}", path.GetFileName());
            UseTemp = true;
        }
        destination.TryCreateDirectory();

        string fileName = Path.GetFileNameWithoutExtension(path);
        string fileExt = Path.GetExtension(path);
        string workDir = destination!;
        if (UseTemp) workDir = destination!.AppendPath(fileName.RegexReplace(@"\.", "_"));
        workDir.TryCreateDirectory(temp: UseTemp);
        string tempFile = workDir.AppendPath($"{fileName}-TEMP{fileExt}");

        Logging.Informational($"Importing scripts from [ {scriptPath} ] into: {tempFile}");
        ProcessData? result = InvokeFfdec($"-importScript \"{path}\" \"{tempFile}\" \"{scriptPath}\"");

        return new FlashResult
        {
            Action = Logging.GetCurrentMethodName(),
            SourcePath = path,
            DestinationPath = workDir,
            ExportedPath = tempFile,
            Result = result?.RC == 0
        };
    }

    /// <summary>
    /// Imports symbol-class mapping into the passed flash file from the passed mapping path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="importPath"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public static FlashResult? ImportSymbolClassMapping(string path, string importPath, string destination)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (importPath.IsNullOrEmptyOrWhiteSpace() || !importPath.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {importPath.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (destination.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory($"Parameter [ Destination ] was null or empty - Unable to continue...");
            return null;
        }

        if (!destination.IsRegexMatch($@"\.({ExpectedExtensions.JoinString("|")})$"))
        {
            Logging.Advisory($"DestinationFile Path [ {destination} ] does not match expected extensions [ {ExpectedExtensions.JoinString(", ").ToUpper()} ] - Unable to continue...");
            return null;
        }
        destination.TryCreateDirectory(true);

        Logging.Informational($"Importing Symbol-Class mapping from [ {importPath} ] into: {destination}");
        ProcessData? result = InvokeFfdec($"-importSymbolClass \"{path}\" \"{destination}\" \"{importPath}\"");

        return new FlashResult
        {
            Action = Logging.GetCurrentMethodName(),
            SourcePath = path,
            DestinationPath = destination.GetDirectoryName(),
            ExportedPath = destination,
            Result = result?.RC == 0
        };
    }

    #endregion

    #region Edit

    /// <summary>
    /// Replaces symbol and class names in the passed flash file with the passed replacements dictionary
    /// </summary>
    /// <param name="path"></param>
    /// <param name="replacements"></param>
    /// <param name="destination"></param>
    /// <param name="workingPath"></param>
    /// <returns></returns>
    public static FlashResult? ReplaceSymbol(string path, Dictionary<string, string> replacements, string? destination = null, string? workingPath = null)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (replacements.IsNullOrEmpty() || replacements.Count == 0)
        {
            Logging.Advisory($"Parameter [ replacements ] is either null or empty - Unable to continue...");
            return null;
        }

        if (workingPath.IsNullOrEmptyOrWhiteSpace())
        {
            workingPath = Path.GetTempPath().AppendPath($"SwfReplaceSymbol_{Guid.NewGuid():N}", path.GetFileName().RegexReplace(@"\.", "_"));
            workingPath.TryCreateDirectory(temp: true);
        }
        else
        {
            workingPath.TryCreateDirectory();
        }

        if (destination.IsNullOrEmptyOrWhiteSpace()) destination = path;
        else
        {
            if (destination!.GetFileExtension().IsNullOrEmptyOrWhiteSpace())
            {
                destination = destination.AppendPath(path.GetFileName().ExpandVariables(replacements)!);
            }
        }
        destination.TryCreateDirectory(true);

        FlashResult returnObject = new()
        {
            Action = Logging.GetCurrentMethodName(),
            SourcePath = path,
            DestinationPath = destination!.GetDirectoryName(),
            ExportedPath = null,
            Result = false
        };

        Logging.Informational($"Performing [ {replacements.Count} ] string replacements on symbols and scripts from [ {path} ] to: {destination}");
        FlashResult? scriptExport = ExportScripts(path, workingPath);
        scriptExport!.LogObjectProperties("SCRIPT EXPORT", NewLine: true);

        if (!scriptExport!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to export scripts"));
            return returnObject;
        }

        FlashResult? symbolExport = ExportSymbolClassMapping(path, scriptExport.DestinationPath);
        symbolExport!.LogObjectProperties("SYMBOL-CLASS EXPORT", NewLine: true);
        if (!symbolExport!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to export symbol-class mapping"));
            return returnObject;
        }

        List<FlashSymbolMap>? symbols = LoadCsvFile(symbolExport.ExportedPath!);
        if (symbols.IsNullOrEmpty())
        {
            Logging.Exception(new NullReferenceException("Failed to load symbol mapping CSV"));
            return returnObject;
        }

        foreach (FlashSymbolMap item in symbols)
        {
            string? newSymbolName = item.Symbol?.ExpandVariables(replacements);
            Logging.Informational($"Finding scripts matching [ {item.Symbol}.as ] in: {scriptExport.ExportedPath}");
            List<FileInfo> scriptFiles = scriptExport.ExportedPath!.GetFiles($"{item.Symbol}.as", SearchOption.AllDirectories);
            if (!scriptFiles.IsNullOrEmpty() && scriptFiles.Count > 0)
            {
                Logging.Informational($"Found [ {scriptFiles.Count} ] matching scripts...");
                foreach (FileInfo file in scriptFiles)
                {
                    string fileExt = file.FullName.GetFileExtension();
                    string newName = $"{newSymbolName}{fileExt}";
                    Logging.Informational($"Renaming script [ {file.FullName} ] to [ {newName} ] and updating class name inside...");
                    file.FullName.RegExReplaceInFile(replacements, true);
                }
            }
            else
            {
                Logging.Advisory("No scripts found - Skipping...");
            }

            Logging.Informational($"Renaming symbol/class [ {item.Symbol} ] to [ {newSymbolName} ] in: {symbolExport.ExportedPath}");
            item.Symbol = newSymbolName!;
            Logging.None();
        }
        SaveCsvFile(symbolExport.ExportedPath!, symbols);

        FlashResult? importscripts = ImportScripts(scriptExport.SourcePath!, scriptExport.ExportedPath!, scriptExport.DestinationPath);
        importscripts!.LogObjectProperties("SCRIPT IMPORT", NewLine: true);
        if (!importscripts!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to import scripts"));
            return returnObject;
        }

        string? destFileName = destination!.GetFileNameWithoutExtension().ExpandVariables(replacements);
        string destFileExt = destination!.GetFileExtension();
        string finalDestPath = destination!.GetDirectoryName().AppendPath($"{destFileName}{destFileExt}");

        FlashResult? importsymbols = ImportSymbolClassMapping(importscripts.ExportedPath!, symbolExport.DestinationPath!, finalDestPath);
        importsymbols!.LogObjectProperties("SYMBOL-CLASS IMPORT", NewLine: true);
        if (!importsymbols!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to import symbol-class mapping"));
            return returnObject;
        }

        returnObject.ExportedPath = finalDestPath;
        returnObject.Result = finalDestPath.PathExists().Exist;
        return returnObject;
    }

    /// <summary>
    /// Replaces the passed shape id with the passed svg path in the passed flash file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <param name="svgPath"></param>
    /// <param name="shapeId"></param>
    /// <param name="resetBounds"></param>
    /// <returns></returns>
    public static FlashResult? ReplaceShape(string path, string? destination = null, string? svgPath = null, int shapeId = 1, bool resetBounds = true)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (destination.IsNullOrEmptyOrWhiteSpace()) destination = path;
        else
        {
            if (destination!.GetFileExtension().IsNullOrEmptyOrWhiteSpace())
            {
                destination = destination.AppendPath(path.GetFileName());
            }
        }
        destination.TryCreateDirectory(true);

        if (svgPath.IsNullOrEmptyOrWhiteSpace() || !svgPath!.PathExists().Exist)
        {
            Logging.Advisory($"SVG path [ {svgPath.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (!svgPath!.GetFileExtension().IsRegexMatch("SVG"))
        {
            Logging.Advisory($"File [ {svgPath.ToFriendlyNull()} ] is not a SVG file - Unable to continue...");
            return null;
        }

        Logging.Informational($"Replacing shape [ #{shapeId} ] with [ {svgPath} ] in: {destination}");
        string arguments = $"-replace \"{path}\" \"{destination}\" \"{shapeId}\" \"{svgPath}\"";
        if (resetBounds) arguments += " nofill";

        ProcessData? result = InvokeFfdec(arguments);
        return new FlashResult
        {
            Action = Logging.GetCurrentMethodName(),
            SourcePath = path,
            DestinationPath = destination,
            ExportedPath = destination,
            Result = result?.RC == 0
        };
    }

    /// <summary>
    /// Appends either a prefix, suffix, or both to symbol and class names in the passed flash file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    /// <param name="workingPath"></param>
    /// <returns></returns>
    public static bool? AppendToSymbol(string path, string? destination = null, string? prefix = null, string? suffix = null, string? workingPath = null)
    {
        if (path.IsNullOrEmptyOrWhiteSpace() || !path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (workingPath.IsNullOrEmptyOrWhiteSpace())
        {
            workingPath = Path.GetTempPath().AppendPath($"SwfAppendSymbol_{Guid.NewGuid():N}", path.GetFileName().RegexReplace(@"\.", "_"));
            workingPath.TryCreateDirectory(temp: true);
        }
        else
        {
            workingPath.TryCreateDirectory();
        }

        if (destination.IsNullOrEmptyOrWhiteSpace()) destination = path;
        destination.TryCreateDirectory(true);

        string SanitizedPrefix = ActionScript.SanitizeClassName(prefix!);
        string SanitizedSuffix = ActionScript.SanitizeClassName(suffix!);
        if (!SanitizedPrefix.IsNullOrEmptyOrWhiteSpace() && !SanitizedSuffix.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Informational($"Appending prefix [ {SanitizedPrefix} ] and suffix [ {SanitizedSuffix} ] to symbols and scripts from [ {path} ] to: {destination}");
        }
        else
        {
            if (!SanitizedPrefix.IsNullOrEmptyOrWhiteSpace())
                Logging.Informational($"Appending prefix [ {SanitizedPrefix} ] to symbols and scripts from [ {path} ] to: {destination}");
            if (!SanitizedSuffix.IsNullOrEmptyOrWhiteSpace())
                Logging.Informational($"Appending suffix [ {SanitizedSuffix} ] to symbols and scripts from [ {path} ] to: {destination}");
        }

        FlashResult? scriptExport = ExportScripts(path, workingPath);
        scriptExport!.LogObjectProperties("SCRIPT EXPORT", NewLine: true);
        if (!scriptExport!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to export scripts"));
            return false;
        }

        FlashResult? symbolExport = ExportSymbolClassMapping(path, scriptExport.DestinationPath);
        symbolExport!.LogObjectProperties("SYMBOL-CLASS EXPORT", NewLine: true);
        if (!symbolExport!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to export symbol-class mapping"));
            return false;
        }

        List<FlashSymbolMap>? symbols = LoadCsvFile(symbolExport.ExportedPath!);
        if (symbols.IsNullOrEmpty())
        {
            Logging.Exception(new NullReferenceException("Failed to load symbol mapping CSV"));
            return false;
        }

        foreach (FlashSymbolMap item in symbols)
        {
            string newSymbolName = item.Symbol!;
            if (!SanitizedPrefix.IsNullOrEmptyOrWhiteSpace()) newSymbolName = $"{SanitizedPrefix}{newSymbolName}";
            if (!SanitizedSuffix.IsNullOrEmptyOrWhiteSpace()) newSymbolName = $"{newSymbolName}{SanitizedSuffix}";

            Logging.Informational($"Finding scripts matching [ {item.Symbol}.as ] in: {scriptExport.ExportedPath}");
            List<FileInfo> scriptFiles = scriptExport.ExportedPath!.GetFiles($"{item.Symbol}.as", SearchOption.AllDirectories);
            if (!scriptFiles.IsNullOrEmpty() && scriptFiles.Count > 0)
            {
                Logging.Informational($"Found [ {scriptFiles.Count} ] matching scripts...");
                foreach (FileInfo file in scriptFiles)
                {
                    string fileExt = file.FullName.GetFileExtension();
                    string newName = $"{newSymbolName}{fileExt}";
                    Logging.Informational($"Renaming script [ {file.FullName} ] to [ {newName} ] and updating class name inside...");
                    Dictionary<string, string> replacements = new()
                    {
                        { item.Symbol!, newSymbolName! }
                    };
                    file.FullName.RegExReplaceInFile(replacements, true);
                }
            }
            else
            {
                Logging.Advisory("No scripts found - Skipping...");
            }

            Logging.Informational($"Renaming symbol/class [ {item.Symbol} ] to [ {newSymbolName} ] in: {symbolExport.ExportedPath}");
            item.Symbol = newSymbolName;
            Logging.None();
        }
        SaveCsvFile(symbolExport.ExportedPath!, symbols);

        FlashResult? importscripts = ImportScripts(scriptExport.SourcePath!, scriptExport.ExportedPath!, scriptExport.DestinationPath);
        importscripts!.LogObjectProperties("SCRIPT IMPORT", NewLine: true);
        if (!importscripts!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to import scripts"));
            return false;
        }

        string destFileName = destination!.GetFileNameWithoutExtension();
        string destFileExt = destination!.GetFileExtension();
        if (!SanitizedPrefix.IsNullOrEmptyOrWhiteSpace()) destFileName = $"{SanitizedPrefix}{destFileName}";
        if (!SanitizedSuffix.IsNullOrEmptyOrWhiteSpace()) destFileName = $"{destFileName}{SanitizedSuffix}";
        string finalDestPath = destination!.GetDirectoryName().AppendPath($"{destFileName}{destFileExt}");
        
        FlashResult? importsymbols = ImportSymbolClassMapping(importscripts.ExportedPath!, symbolExport.DestinationPath!, finalDestPath);
        importsymbols!.LogObjectProperties("SYMBOL-CLASS IMPORT", NewLine: true);
        if (!importsymbols!.Result.GetValueOrDefault())
        {
            Logging.Exception(new Exception("Failed to import symbol-class mapping"));
            return false;
        }

        return finalDestPath.PathExists().Exist;
    }

    #endregion

    #region CSV

    /// <summary>
    /// Reads the symbol-class mapping CSV file and returns a list of FlashSymbolMap objects representing the mapping
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<FlashSymbolMap>? LoadCsvFile(string path)
    {
        Logging.Informational($"Loading CSV file: {path}");
        try
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";",
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using StreamReader reader = new(path);
            using CsvReader csv = new(reader, config);
            csv.Context.RegisterClassMap<MappedFlashSymbols>();
            return [.. csv.GetRecords<FlashSymbolMap>()];
        }
        catch (Exception ex)
        {
            Logging.Exception(ex);
        }

        return null;
    }

    /// <summary>
    /// Saves the symbol-class mapping FlashSymbolMap list to the passed CSV file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool SaveCsvFile(string path, List<FlashSymbolMap> data)
    {
        Logging.Informational($"Saving CSV file: {path}");
        try
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using StreamWriter writer = new(path);
            using CsvWriter csv = new(writer, config);
            csv.Context.RegisterClassMap<MappedFlashSymbols>();
            csv.WriteRecords(data);
            return true;
        }
        catch (Exception ex)
        {
            Logging.Exception(ex);
        }
        return false;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Runs FFDec with the passed arguments and returns the results
    /// </summary>
    /// <param name="ArgumentString"></param>
    /// <param name="ArgumentArray"></param>
    /// <returns></returns>
    public static ProcessData? InvokeFfdec(string? ArgumentString = null, string[]? ArgumentArray = null)
    {
        if (!FfdecPath!.PathExists().Exist)
        {
            Logging.Advisory($"FFDec path [ {FfdecPath.ToFriendlyNull()} ] does not exist - Unable to continue...");
            return null;
        }

        if (ArgumentString.IsNullOrEmptyOrWhiteSpace() && ArgumentArray.IsNullOrEmpty())
        {
            Logging.Advisory($"Parameter [ ArgumentString ] and [ ArgumentArray ] are both null - Please provide one of them...");
            return null;
        }

        string FinalArguments = ArgumentString!;
        if (!ArgumentArray.IsNullOrEmpty() && ArgumentArray?.Length > 0)
        {
            FinalArguments = ArgumentArray.JoinString();
        }
        if (FinalArguments.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory($"Java arguments could not be resolved - Unable to continue...");
            return null;
        }

        FinalArguments = $"-jar \"{FfdecPath}\" {FinalArguments}";

        try
        {
            return Processes.RunAndWait(JavaPath!, FinalArguments);
        }
        catch (Exception Ex)
        {
            Logging.Exception(Ex);
        }

        return null;
    }

    /// <summary>
    /// Gets the main script class name from the passed ActionScript file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string? GetScriptClassName(string path)
    {
        if (path.IsNullOrEmptyOrWhiteSpace()) return null;
        if (!path.PathExists().Exist)
        {
            Logging.Advisory($"Path [ {path} ] does not exist - Unable to continue...");
            return null;
        }

        string content = path.GetFileContent()!;
        if (content.IsNullOrEmptyOrWhiteSpace()) return null;

        Match match = ASClassName().Match(content);
        if (match.Success)
            return match.Groups["value"].Value.Trim();

        return null;
    }

    #endregion

    #endregion
}
