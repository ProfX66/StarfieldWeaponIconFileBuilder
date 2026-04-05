using StarfieldWeaponIconFileBuilder.Models;
using StarfieldWeaponIconFileBuilder.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StarfieldWeaponIconFileBuilder.Extensions;

public static class ObjectExtensions
{
    #region Methods

    /// <summary>
    /// Gets a positive difference value between two timespans
    /// </summary>
    /// <param name="firstTimeSpan"></param>
    /// <param name="secondTimeSpan"></param>
    /// <returns></returns>
    public static TimeSpan GetPositiveTimeSpanDifference(this TimeSpan firstTimeSpan, TimeSpan secondTimeSpan)
    {
        return (firstTimeSpan > secondTimeSpan) ? firstTimeSpan - secondTimeSpan : secondTimeSpan - firstTimeSpan;
    }

    /// <summary>
    /// Casts IDictionary to IEnumerable<DictionaryEntry>
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns>IEnumerable<DictionaryEntry></returns>
    public static IEnumerable<DictionaryEntry> CastDict(this IDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            yield return entry;
        }
    }

    /// <summary>
    /// Enumerates dictionary entries and logs their values
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="Type"></param>
    public static void LogEntries(this Dictionary<string, object> dict, string Prefix = "Item", LoggingData? Type = null)
    {
        if (dict.IsNullOrEmpty() || dict.Count == 0) { return; }
        Type ??= Logging.DefaultLogLevel;

        foreach (KeyValuePair<string, object> kvp in dict)
        {
            if (kvp.Value.GetType() == typeof(Dictionary<string, object>))
            {
                IDictionary dictionary = (IDictionary)kvp.Value;
                Dictionary<string, string> newDictionary = dictionary.CastDict()
                    .Where(entry => entry.Key is string && entry.Value != null)
                    .ToDictionary(
                        entry => (string)entry.Key,
                        entry => entry.Value?.ToString() ?? string.Empty
                    );

                foreach (KeyValuePair<string, string> kvpp in newDictionary)
                {
                    Logging.Dynamic($"[{Prefix}] {kvp.Key.ToFriendlyNull()} ({kvp.Value.GetType()}) => [Sub{Prefix}] {kvpp.Key.ToFriendlyNull()}: {kvpp.Value.ToFriendlyNull()} ({kvpp.Value.GetType()})", Type);
                }
            }
            else if (kvp.Value.IsArray())
            {
                object[] newObj = (object[])kvp.Value;
                foreach (var item in newObj)
                {
                    Logging.Dynamic($"[{Prefix}] {kvp.Key.ToFriendlyNull()} ({kvp.Value.GetType()}) => [Sub{Prefix}] {item.ToFriendlyNull()} ({item.GetType()})", Type);
                }
            }
            else
            {
                Logging.Dynamic($"[{Prefix}] {kvp.Key.ToFriendlyNull()}: {kvp.Value.ToFriendlyNull()} ({kvp.Value.GetType()})", Type);
            }
        }
    }

    /// <summary>
    /// Write object properties to the log (collection aware)
    /// </summary>
    /// <param name="InputObject"></param>
    /// <param name="Caller"></param>
    /// <param name="Prefix"></param>
    /// <param name="SubPrefix"></param>
    /// <param name="Type"></param>
    /// <param name="NewLine"></param>
    public static void LogObjectProperties(this object InputObject, string Caller, string Prefix = "Property", string SubPrefix = "SubProperty", LoggingData? Type = null, bool NewLine = false)
    {
        if (InputObject.IsNullOrEmpty()) return;

        Caller = Caller ?? nameof(InputObject);
        Type = Type ?? Logging.DefaultLogLevel;

        Type ObjectType = InputObject.GetType();
        Logging.Dynamic($"Object [ {Caller} ] properties:", Type);

        foreach (PropertyInfo pi in ObjectType.GetProperties())
        {
            object? value = null;

            try
            {
                value = pi.GetValue(InputObject, null)!;

                if (value is IEnumerable enumerable && value is not string)
                {
                    if (value is IEnumerable<string> stringEnum)
                    {
                        value = stringEnum.ToList();
                    }
                    else
                    {
                        value = enumerable.Cast<object>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Exception(new Exception($"[{Prefix}] {pi.Name} could not be read...", ex));
                continue;
            }

            if (value.IsNullOrEmpty() || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                Logging.Dynamic($"[{Prefix}] {pi.Name.ToFriendlyNull()}: {value.ToFriendlyNull()}", Type);
                continue;
            }

            if (value is ICollection collection && collection.Count == 0)
            {
                Logging.Dynamic($"[{Prefix}] {pi.Name}: (empty collection)", Type);
                continue;
            }

            switch (value)
            {
                case Dictionary<string, object> dictObj:
                    dictObj.LogEntries(SubPrefix, Type);
                    break;

                case Dictionary<string, string> dictStr:
                    dictStr.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value).LogEntries(SubPrefix, Type);
                    break;

                case List<string> stringList:
                    Logging.Dynamic($"[{Prefix}] {pi.Name.ToFriendlyNull()}: {string.Join(", ", stringList)}", Type);
                    break;

                case IEnumerable<object> objEnumerable:
                    var items = objEnumerable.ToList();
                    Logging.Dynamic($"[{Prefix}] {pi.Name.ToFriendlyNull()}: {string.Join(", ", items)}", Type);
                    break;

                default:
                    Logging.Dynamic($"[{Prefix}] {pi.Name.ToFriendlyNull()}: {value.ToFriendlyNull()}", Type);
                    break;
            }
        }

        if (NewLine) Logging.None();
    }

    #endregion
}