using StarfieldWeaponIconFileBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static partial class ActionScript
{
    #region Properties

    [GeneratedRegex(@"[^A-Za-z0-9_]", RegexOptions.Compiled)]
    private static partial Regex InvalidCharPattern();

    private static readonly Regex InvalidChars = InvalidCharPattern();

    private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "class","package","public","private","protected","internal","extends","implements","import",
        "function","var","const","if","else","for","while","switch","case","default","return","new"
    };

    public static HashSet<string> ReplacementWords =
    [
        "Alpha","Bravo","Charlie","Delta","Echo","Falcon","Nova","Orion","Viper","Atlas"
    ];

    private static readonly Random Rand = new();

    #endregion

    #region Methods

    /// <summary>
    /// Replaces AS3 invalid characters and reserved tokens in the passed string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string SanitizeClassName(string input)
    {
        if (input.IsNullOrEmptyOrWhiteSpace()) return input;
        string sanitized = InvalidChars.Replace(input, GetRandomChar());

        if (char.IsDigit(sanitized[0]))
        {
            sanitized = $"§{sanitized}§";
        }

        if (ReservedWords.Contains(sanitized))
        {
            sanitized = GetRandomReplacement();
        }

        return sanitized;
    }

    /// <summary>
    /// Returns a random alpha-numeric character as a string
    /// </summary>
    /// <returns></returns>
    private static string GetRandomChar()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return chars[Rand.Next(chars.Length)].ToString();
    }

    /// <summary>
    /// Returns a random word from the ReplacementWords hashset
    /// </summary>
    /// <returns></returns>
    private static string GetRandomReplacement()
    {
        if (ReplacementWords.IsNullOrEmpty() || ReplacementWords.Count == 0)
            return "CLN";

        int index = Rand.Next(ReplacementWords.Count);
        foreach (var word in ReplacementWords)
        {
            if (index-- == 0)
                return word;
        }

        return "CLN";
    }

    #endregion
}
