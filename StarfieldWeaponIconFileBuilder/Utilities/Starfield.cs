using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static class Starfield
{
    #region Properties

    public static string? TemplatePath { get; set; }
    public static string? TemplateClassName { get; set; } = "CustomWeaponTemplate";
    public static string? FilePrefix { get; set; } = "CCSUP";

    #endregion

    #region Methods

    /// <summary>
    /// Creates a new Starfield Weapon icon file at the passed destiation using the passed linkage name
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Destination"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static bool? NewWeaponIconFile(string Name, string Destination, string? IconPath = null, bool AutoResize = true)
    {
        if (TemplatePath.IsNullOrEmptyOrWhiteSpace())
        {
            throw new NullReferenceException("TemplatePath property is null or empty");
        }

        if (!TemplatePath!.PathExists().Exist)
        {
            throw new FileNotFoundException($"Path [ {TemplatePath} ] was not found");
        }

        if (Name.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory("Parameter [ Name ] was null or empty - Unable to continue...");
            return null;
        }

        if (Destination.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory("Parameter [ Destination ] was null or empty - Unable to continue...");
            return null;
        }

        string tempFileName = TemplatePath!.GetFileName().RegexReplace(TemplateClassName!.RegexEscape(), Name);
        if (!tempFileName.IsRegexMatch($"^{FilePrefix!.RegexEscape()}"))
        {
            Name = $"{FilePrefix}_{Name}";
        }

        Logging.Informational($"Creating new Starfield Weapon icon file [ {tempFileName} ] in: {Destination}");
        Dictionary<string, string> replacements = new()
        {
            { TemplateClassName!, Name }
        };

        Logging.Informational($"Autosize? {AutoResize}");
        if (!AutoResize)
        {
            replacements.TryAdd("bAutoSize:Boolean = true;", "bAutoSize:Boolean = false;");
        }

        FlashResult? result = Flash.ReplaceSymbol(TemplatePath!, replacements, Destination);
        if (result.IsNullOrEmpty()) return false;

        bool currentResult = result.Result.GetValueOrDefault();
        if (!currentResult) return false;
        
        if (!IconPath.IsNullOrEmptyOrWhiteSpace())
        {
            FlashResult? shapeResult = Flash.ReplaceShape(result.ExportedPath!, result.ExportedPath!, IconPath!);
            if (shapeResult.IsNullOrEmpty()) return false;
            currentResult = shapeResult.Result.GetValueOrDefault();
        }

        return currentResult;
    }

    /// <summary>
    /// Clones the passed source SWF to a new name in the passed destination folder
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Source"></param>
    /// <param name="Destination"></param>
    /// <returns></returns>
    public static bool? CloneWeaponIconFile(string Name, string Source, string Destination)
    {
        if (Name.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory("Parameter [ Name ] was null or empty - Unable to continue...");
            return null;
        }

        if (Source.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory("Parameter [ Source ] was null or empty - Unable to continue...");
            return null;
        }

        if (Destination.IsNullOrEmptyOrWhiteSpace())
        {
            Logging.Advisory("Parameter [ Destination ] was null or empty - Unable to continue...");
            return null;
        }

        if (!Name.IsRegexMatch($"^{FilePrefix!.RegexEscape()}"))
        {
            Name = $"{FilePrefix}_{Name}";
        }

        Destination = Destination.AppendPath($"{Name}.swf");
        Logging.Informational($"Cloning Starfield Weapon icon file [ {Source} ] to: {Destination}");
        Dictionary<string, string> replacements = new()
        {
            { Source.GetFileNameWithoutExtension(), Name }
        };

        FlashResult? result = Flash.ReplaceSymbol(Source, replacements, Destination);
        if (result.IsNullOrEmpty()) return false;

        bool currentResult = result.Result.GetValueOrDefault();
        if (!currentResult) return false;

        return currentResult;
    }

    #endregion
}
