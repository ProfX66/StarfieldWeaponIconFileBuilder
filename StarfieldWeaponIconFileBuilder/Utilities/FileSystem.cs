using StarfieldWeaponIconFileBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static partial class FileSystem
{
    #region Properties

    public static readonly Dictionary<string, string> PlatformVariables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["%LOCALAPPDATA%"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify),
        ["%APPDATA%"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
        ["%USERPROFILE%"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify),
        ["%DESKTOP%"] = Environment.GetFolderPath(Environment.SpecialFolder.Desktop, Environment.SpecialFolderOption.DoNotVerify),
        ["%DOCUMENTS%"] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify),
        ["$HOME"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify),
        ["${HOME}"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify),
        ["%TEMP%"] = Path.GetTempPath()
    };

    #endregion

    #region Native Imports

    /// <summary>
    /// Impliment the native MoveFileEx method from kernel32.dll
    /// </summary>
    /// <param name="lpExistingFileName"></param>
    /// <param name="lpNewFileName"></param>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    [LibraryImport("kernel32.dll", EntryPoint = "MoveFileExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, int dwFlags);
    public const int MOVEFILE_REPLACE_EXISTING = 0x00000001;
    public const int MOVEFILE_COPY_ALLOWED = 0x00000002;
    public const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;
    public const int MOVEFILE_WRITE_THROUGH = 0x00000008;
    public const int MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020;

    #endregion

    #region Methods

    /// <summary>
    /// Queue the specified file or folder for deletion on next system reboot.
    /// </summary>
    /// <param name="fileName">name of file or folder to delete at next system reboot</param>
    /// <returns>true if delete request was successfully queued, false if error</returns>
    public static bool DeleteOnReboot(string path)
    {
        bool result = true;
        if (!path.IsNullOrEmpty())
        {
            try
            {
                string? expandedPath = path.ExpandVariables();
                if (!expandedPath.IsNullOrEmpty())
                {
                    string completeFileName = Path.GetFullPath(expandedPath!);
                    Logging.Debug($"Marking path [ {completeFileName} ] for deletion on reboot...");
                    if (completeFileName.PathExists().Exist)
                    {
                        result = MoveFileEx(completeFileName, null, MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_REPLACE_EXISTING);
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception Ex)
            {
                Message.ShowError($"Failed to flag path [ {path} ] for deletion on reboot", exception: Ex).Wait();
                result = false;
            }
        }
        return result;
    }

    /// <summary>
    /// Renames a file that is currently in use
    /// </summary>
    /// <param name="path"></param>
    /// <param name="newName"></param>
    /// <param name="deleteOnReboot"></param>
    /// <returns></returns>
    public static bool RenameInUseFile(string path, string newName, bool deleteOnReboot = true)
    {
        bool result = false;
        if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(newName))
        {
            try
            {
                string completeFileName = Path.GetFullPath(path);
                if (File.Exists(completeFileName) || Directory.Exists(completeFileName))
                {
                    string finalNewName = Path.GetFullPath(newName);
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(newName)))
                    {
                        string originalDirectory = path.GetDirectoryName();
                        finalNewName = Path.Combine(originalDirectory, newName);
                    }

                    result = MoveFileEx(path, finalNewName, MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH);
                    if (result && deleteOnReboot)
                    {
                        DeleteOnReboot(finalNewName);
                    }
                }
            }
            catch
            {
                result = false;
            }
        }
        return result;
    }

    #endregion
}
