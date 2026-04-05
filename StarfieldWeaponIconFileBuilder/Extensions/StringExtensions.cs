using StarfieldWeaponIconFileBuilder.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StarfieldWeaponIconFileBuilder.Extensions;
public static class StringExtensions
{
    #region Methods

    /// <summary>
    /// Validates the passed object for null or empty
    /// </summary>
    /// <param name="Value"></param>
    /// <returns>Bool</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? value)
    {
        if (value == null) return true;
        return string.IsNullOrEmpty(value.ToString());
    }

    /// <summary>
    /// Validates the passed object for null or whitespace
    /// </summary>
    /// <param name="Value"></param>
    /// <returns>Bool</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this object? Value)
    {
        if (Value == null) return true;
        return string.IsNullOrWhiteSpace(Value.ToString());
    }

    /// <summary>
    /// Validates the passed object for null, empty or whitespace
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static bool IsNullOrEmptyOrWhiteSpace([NotNullWhen(false)] this object? Value)
    {
        if (Value == null) return true;
        if (string.IsNullOrEmpty(Value.ToString())) return true;
        return string.IsNullOrWhiteSpace(Value.ToString());
    }

    /// <summary>
    /// Returns an escaped string
    /// </summary>
    /// <param name="InputString"></param>
    /// <returns>string</returns>
    public static string RegexEscape(this string InputString)
    {
        return Regex.Escape(InputString);
    }

    /// <summary>
    /// RexEx replace shorthand extension
    /// </summary>
    /// <param name="InputString"></param>
    /// <param name="Pattern"></param>
    /// <param name="ReplaceString"></param>
    /// <param name="RxOptions"></param>
    /// <returns>String</returns>
    public static string RegexReplace(this string InputString, string Pattern, string ReplaceString, RegexOptions RxOptions = RegexOptions.IgnoreCase)
    {
        return Regex.Replace(InputString, Pattern, ReplaceString, RxOptions);
    }

    /// <summary>
    /// Just performs a standard regex ismatch
    /// </summary>
    /// <param name="InputString"></param>
    /// <param name="Pattern"></param>
    /// <param name="RxOptions"></param>
    /// <returns>bool</returns>
    public static bool IsRegexMatch(this object InputString, string Pattern, RegexOptions RxOptions = RegexOptions.IgnoreCase)
    {
        if (InputString.IsNullOrEmpty()) { return false; }
        if (InputString.GetType() != typeof(string)) { return false; }
        return Regex.IsMatch($"{InputString}", Pattern, RxOptions);
    }

    /// <summary>
    /// Sanitizes the passed string if it includes a NUL unicode character
    /// </summary>
    /// <param name="InputString"></param>
    /// <returns></returns>
    public static string SanitizeNul(this string InputString)
    {
        if (!InputString.IsRegexMatch(@"\x00")) return InputString;
        if (InputString.IsRegexMatch(@"\x00\\")) return InputString.RegexReplace(@"\x00\\", "");
        return InputString.RegexReplace(@"\x00", "");
    }

    /// <summary>
    /// Converts a nullable object to a friendly string
    /// </summary>
    /// <param name="Value"></param>
    /// <returns>String output of passed object</returns>
    public static string ToFriendlyNull(this object? Value, string FriendlyValue = "Null")
    {
        if (Value.IsNullOrEmpty()) { return FriendlyValue; }
        if (Value is string thisValue)
        {
            return thisValue.IsNullOrEmpty() ? FriendlyValue : thisValue.SanitizeNul();
        }
        return FriendlyValue;
    }

    /// <summary>
    /// Gets the type of the passed object as a string or a friendly null if the object is null
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="NullValue"></param>
    /// <returns></returns>
    public static string GetTypeString(this object? Value, string NullValue = "Null")
    {
        if (Value.IsNullOrEmpty()) return NullValue;
        return Value.GetType().ToString();
    }

    /// <summary>
    /// Adds characters after the string to the desired length
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="desiredLength"></param>
    /// <param name="PadChar"></param>
    /// <returns></returns>
    public static string PadToLength(this string inputString, int desiredLength, char? PadChar = null)
    {
        if (inputString.Length >= desiredLength)
        {
            return inputString;
        }
        else
        {
            int numSpacesToAdd = desiredLength - inputString.Length;
            if (!PadChar.HasValue) PadChar = inputString.ToCharArray().LastOrDefault();
            string paddedString = inputString + new string(PadChar.Value, numSpacesToAdd);
            return paddedString;
        }
    }

    /// <summary>
    /// Loads the StringBuilder with the List data one item per line
    /// </summary>
    /// <param name="SBuilder"></param>
    /// <param name="InputList"></param>
    /// <returns></returns>
    public static StringBuilder AppendLineList(this StringBuilder SBuilder, List<string> InputList)
    {
        if (SBuilder.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(SBuilder));
        }

        if (!InputList.IsNullOrEmpty() && InputList.Count > 0)
        {
            SBuilder.AppendLine(InputList[0].ToString());

            for (int i = 1; i < InputList.Count; i++)
            {
                SBuilder.AppendLine(InputList[i].ToString());
            }
        }

        return SBuilder;
    }

    /// <summary>
    /// Joines the passed collection object to string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static string JoinString<T>(this IEnumerable<T> Source, string Delimiter = " ")
    {
        if (Source.IsNullOrEmpty()) return string.Empty;
        return string.Join(Delimiter, Source);
    }

    /// <summary>
    /// Gets the last item of the passed collection object as a string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Source"></param>
    /// <param name="TrimReturn"></param>
    /// <returns></returns>
    public static string GetLastItem<T>(this IEnumerable<T> Source, bool TrimReturn = true)
    {
        if (Source.IsNullOrEmpty()) return string.Empty;

        var lastItem = Source.LastOrDefault();
        if (lastItem.IsNullOrEmpty()) return string.Empty;

        var result = lastItem.ToString() ?? string.Empty;
        return TrimReturn ? result.Trim() : result;
    }

    /// <summary>
    /// Gets the first item of the passed collection object as a string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Source"></param>
    /// <param name="TrimReturn"></param>
    /// <returns></returns>
    public static string GetFirstItem<T>(this IEnumerable<T> Source, bool TrimReturn = true)
    {
        if (Source.IsNullOrEmpty()) return string.Empty;

        var lastItem = Source.FirstOrDefault();
        if (lastItem.IsNullOrEmpty()) return string.Empty;

        var result = lastItem.ToString() ?? string.Empty;
        return TrimReturn ? result.Trim() : result;
    }

    /// <summary>
    /// Appends the passed path elements to the base path and returns the combined path as a string
    /// </summary>
    /// <param name="basePath"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string AppendPath(this string? InputPath, params string[]? ChildItems)
    {
        if (ChildItems.IsNullOrEmpty())
            return InputPath ?? string.Empty;

        if (InputPath.IsNullOrEmptyOrWhiteSpace())
            return Path.Combine(ChildItems);

        var AllPathElements = new string[ChildItems.Length + 1];
        AllPathElements[0] = InputPath!;
        ChildItems.CopyTo(AllPathElements, 1);

        return Path.Combine(AllPathElements);
    }

    /// <summary>
    /// Converts the object to a friendly size string
    /// </summary>
    /// <param name="inputObject"></param>
    /// <returns>String</returns>
    public static string ToFriendlySize(this object inputObject)
    {
        string[] unit = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
        int index = 0;
        double size = Convert.ToDouble(inputObject);
        while (size >= 1024)
        {
            size /= 1024;
            index++;
        }

        return $"{size:N2} {unit[index]}";
    }

    /// <summary>
    /// Expands environmental and passed variables in the string
    /// </summary>
    /// <param name="Item"></param>
    /// <param name="Substitutions"></param>
    /// <returns>Expanded String</returns>
    public static string? ExpandVariables(this string Item, Dictionary<string, string>? Substitutions = null)
    {
        if (Item.IsNullOrEmpty()) return Item;
        string stReturn = Environment.ExpandEnvironmentVariables(Item);
        if (!Substitutions.IsNullOrEmpty())
        {
            foreach (KeyValuePair<string, string> item in Substitutions)
            {
                stReturn = stReturn.Replace(item.Key, item.Value);
            }
        }

        if (AppConfig.InternalEnvVariables.Count > 0)
        {
            foreach (KeyValuePair<string, string> item in AppConfig.InternalEnvVariables)
            {
                stReturn = stReturn.Replace(item.Key, item.Value);
            }
        }

        return stReturn;
    }

    /// <summary>
    /// Formats a friendly time
    /// </summary>
    /// <param name="Label"></param>
    /// <param name="Total"></param>
    /// <param name="FormatPattern"></param>
    /// <returns></returns>
    private static string FormatFriendlyTime(string Label, double Total, string FormatPattern = "{0:0.00}")
    {
        string ret = string.Format(string.Concat(FormatPattern, $" {Label}"), Total);
        if (Total > 1) ret = $"{ret}s";
        return ret;
    }

    /// <summary>
    /// Converts the timespan to a friendly string value
    /// </summary>
    /// <param name="InputObject"></param>
    /// <param name="FormatPattern"></param>
    /// <returns>String</returns>
    public static string? ToFriendlyTime(this TimeSpan? InputObject, string FormatPattern = "{0:0.00}")
    {
        try
        {
            if (InputObject.IsNullOrEmpty()) { return null; }
            if (InputObject.Value.TotalSeconds < 60) { return FormatFriendlyTime("Second", InputObject.Value.TotalSeconds, FormatPattern); }
            if (InputObject.Value.TotalMinutes < 60) { return FormatFriendlyTime("Minute", InputObject.Value.TotalMinutes, FormatPattern); }
            if (InputObject.Value.TotalHours < 24) { return FormatFriendlyTime("Hour", InputObject.Value.TotalHours, FormatPattern); }
            if (InputObject.Value.TotalDays < 7) { return FormatFriendlyTime("Day", InputObject.Value.TotalDays, FormatPattern); }
            if (InputObject.Value.TotalDays < 30.44) { return FormatFriendlyTime("Week", InputObject.Value.TotalDays / 7, FormatPattern); }
            if (InputObject.Value.TotalDays < 365.25) { return FormatFriendlyTime("Month", InputObject.Value.TotalDays / 30.44, FormatPattern); }
            else { return FormatFriendlyTime("Year", InputObject.Value.TotalDays / 365.25, FormatPattern); }
        }
        catch { return null; }
    }

    /// <summary>
    /// Converts the timespan to a friendly string value
    /// </summary>
    /// <param name="InputObject"></param>
    /// <param name="FormatPattern"></param>
    /// <returns>String</returns>
    public static string? ToFriendlyTime(this TimeSpan InputObject, string FormatPattern = "{0:0.00}")
    {
        try
        {
            if (InputObject.TotalSeconds < 60) { return FormatFriendlyTime("Second", InputObject.TotalSeconds, FormatPattern); }
            if (InputObject.TotalMinutes < 60) { return FormatFriendlyTime("Minute", InputObject.TotalMinutes, FormatPattern); }
            if (InputObject.TotalHours < 24) { return FormatFriendlyTime("Hour", InputObject.TotalHours, FormatPattern); }
            if (InputObject.TotalDays < 7) { return FormatFriendlyTime("Day", InputObject.TotalDays, FormatPattern); }
            if (InputObject.TotalDays < 30.44) { return FormatFriendlyTime("Week", InputObject.TotalDays / 7, FormatPattern); }
            if (InputObject.TotalDays < 365.25) { return FormatFriendlyTime("Month", InputObject.TotalDays / 30.44, FormatPattern); }
            else { return FormatFriendlyTime("Year", InputObject.TotalDays / 365.25, FormatPattern); }
        }
        catch { return null; }
    }

    /// <summary>
    /// Converts a short hand friendly time string to long
    /// </summary>
    /// <param name="InputString"></param>
    /// <returns>String</returns>
    public static string FromFriendlyShortTime(this string InputString)
    {
        string rxPattern = @"^*[A-Za-z]\z";
        Regex rx = new(rxPattern, RegexOptions.IgnoreCase);
        Match rxMatch = rx.Match(InputString);
        int timeValue = Convert.ToInt32(Regex.Replace(InputString, rxPattern, "", RegexOptions.IgnoreCase));

        if (rxMatch.ToString().IsRegexMatch("^s")) { return string.Format("{0} Second(s)", timeValue); }
        if (rxMatch.ToString().IsRegexMatch("^m")) { return string.Format("{0} Minute(s)", timeValue); }
        if (rxMatch.ToString().IsRegexMatch("^h")) { return string.Format("{0} Hour(s)", timeValue); }
        if (rxMatch.ToString().IsRegexMatch("^d")) { return string.Format("{0} Day(s)", timeValue); }

        return $"{timeValue} Hours";
    }

    #endregion
}