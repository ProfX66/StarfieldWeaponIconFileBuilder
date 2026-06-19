using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml.Templates;
using StarfieldWeaponIconFileBuilder.Models;
using StarfieldWeaponIconFileBuilder.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace StarfieldWeaponIconFileBuilder.Extensions;

public static class FileSystemExtensions
{
    #region Methods

    /// <summary>
    /// Resolves relative pathing
    /// </summary>
    /// <param name="InputPath"></param>
    /// <returns></returns>
    public static string ResolvePath(this string InputPath)
    {
        if (InputPath.IsRegexMatch(@"^%") || !InputPath.IsRegexMatch(@"\\\.\.\\")) return InputPath;
        string ReplacePattern = $@"{Environment.CurrentDirectory.RegexEscape()}\\?";
        return Path.GetFullPath(InputPath).RegexReplace(ReplacePattern, "").NormalizePath();
    }

    /// <summary>
    /// Normalizes the slashes in the passed path
    /// </summary>
    /// <param name="InputPath"></param>
    /// <returns></returns>
    public static string NormalizePath(this string InputPath)
    {
        if (InputPath.IsNullOrEmptyOrWhiteSpace()) return InputPath;
        char separator = Path.DirectorySeparatorChar;
        char alternate = separator == '/' ? '\\' : '/';
        return InputPath.Replace(alternate, separator);
    }

    /// <summary>
    /// Converts a full file path to a FileInfo
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static FileInfo? ToFileInfo(this string FilePath)
    {
        if (FilePath.IsNullOrEmpty()) return null;
        if (!FilePath.PathExists().Exist) return null;
        return new FileInfo(FilePath);
    }

    /// <summary>
    /// Converts a full file path to a FileInfo
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static DirectoryInfo? ToDirectoryInfo(this string DirectoryPath)
    {
        if (DirectoryPath.IsNullOrEmpty()) return null;
        if (!DirectoryPath.PathExists().Exist) return null;
        return new DirectoryInfo(DirectoryPath);
    }

    /// <summary>
    /// Gets the directory name of the passed path
    /// </summary>
    /// <param name="path"></param>
    /// <returns>string</returns>
    public static string GetDirectoryName(this string path)
    {
        string ret = path;

        try
        {
            var dirName = Path.GetDirectoryName(path);
            if (!dirName.IsNullOrEmpty())
            {
                ret = dirName;
            }
        }
        catch { }

        return ret;
    }

    /// <summary>
    /// Gets the file name of the passed path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileName(this string path)
    {
        string ret = path;

        try
        {
            ret = Path.GetFileName(path);
        }
        catch { }

        return ret;
    }

    /// <summary>
    /// Gets the file name with out extension of the passed path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileNameWithoutExtension(this string path)
    {
        string ret = path;

        try
        {
            ret = Path.GetFileNameWithoutExtension(path);
        }
        catch { }

        return ret;
    }

    /// <summary>
    /// Gets the file extension of the passed path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileExtension(this string path)
    {
        string ret = path;

        try
        {
            ret = Path.GetExtension(path);
        }
        catch { }

        return ret;
    }

    /// <summary>
    /// Gets the full file path but with out the extension
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string? GetFullPathWithoutExtension(this string path)
    {
        string? ret = path;

        try
        {
            var directory = path.GetDirectoryName();
            var filenameWithoutExt = path.GetFileNameWithoutExtension();
            if (!directory.IsNullOrEmptyOrWhiteSpace() && !filenameWithoutExt.IsNullOrEmptyOrWhiteSpace())
            {
                ret = Path.Combine(directory, filenameWithoutExt);
            }
            else
            {
                ret = filenameWithoutExt;
            }
        }
        catch { }

        return ret;
    }

    /// <summary>
    /// Validates if the path exists and if its a File or Directory
    /// </summary>
    /// <param name="ItemPath"></param>
    /// <returns>FileSystemData</returns>
    public static FileSystemData PathExists(this string ItemPath)
    {
        FileSystemData fsd = new();

        try
        {
            fsd.Exist = File.Exists(ItemPath) || Directory.Exists(ItemPath);
            if (!ItemPath.IsRegexMatch(@"^\w:\\")) { return fsd; }
            if (File.Exists(ItemPath)) { fsd.Type = typeof(File); }
            if (Directory.Exists(ItemPath)) { fsd.Type = typeof(Directory); }
        }
        catch (Exception Ex)
        {
            Message.ShowError($"Unable to validate {fsd.Type.GetTypeString()} path: {ItemPath}", exception: Ex).Wait();
        }

        return fsd;
    }

    /// <summary>
    /// Creates the passed directory if it doesnt exist.
    /// Will trim leading/trailing spaces for a directory path to bypass a c# bug where a directory path with trailing spaces fails to be created but doesnt thow an exception.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isFile"></param>
    /// <param name="hidden"></param>
    /// <param name="temp"></param>
    /// <param name="silent"></param>
    /// <param name="NoTrim"></param>
    public static void TryCreateDirectory(this string? path, bool isFile = false, bool? hidden = null, bool? temp = null, bool silent = false, bool NoTrim = false)
    {
        if (path.IsNullOrEmpty()) return;
        string? resolvedPath = path.ExpandVariables();
        if (!isFile && !NoTrim)
        {
            resolvedPath = resolvedPath!.Trim();
            if (resolvedPath.EndsWith('.')) resolvedPath = resolvedPath.Trim('.');
        }

        FileSystemData exists = resolvedPath!.PathExists();
        if (exists.Type == typeof(File) || isFile) exists = resolvedPath!.GetDirectoryName().PathExists();

        if (!exists.Exist)
        {
            if (!silent) Logging.Advisory($"Creating directory: {resolvedPath}");
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(resolvedPath!);
                if (hidden.GetValueOrDefault()) di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (Exception Ex)
            {
                Message.ShowError($"Unable to create directory: {resolvedPath}", exception: Ex).Wait();
            }
        }
        else
        {
            if (hidden.HasValue)
            {
                try
                {
                    DirectoryInfo di = new(resolvedPath!);
                    if (hidden.Value)
                    {
                        di.Attributes |= FileAttributes.Hidden;
                    }
                    else
                    {
                        di.Attributes &= ~FileAttributes.Hidden;
                    }
                }
                catch (Exception Ex)
                {
                    Message.ShowError($"Unable to modify attributes of directory: {resolvedPath}", exception: Ex).Wait();
                }
            }
        }

        if (temp.GetValueOrDefault() && resolvedPath!.PathExists().Exist)
        {
            _ = resolvedPath!.DeleteOnReboot();
        }
    }

    /// <summary>
    /// Queue the specified file or folder for deletion on next system reboot.
    /// </summary>
    /// <param name="fileName">name of file or folder to delete at next system reboot</param>
    /// <returns>true if delete request was successfully queued, false if error</returns>
    public static bool DeleteOnReboot(this string path)
    {
        if (path.IsNullOrEmptyOrWhiteSpace()) return false;
        return FileSystem.DeleteOnReboot(path);
    }

    /// <summary>
    /// Queue the specified FileInfo for deletion on next system reboot.
    /// </summary>
    /// <param name="InputObject"></param>
    /// <returns></returns>
    public static bool DeleteOnReboot(this FileInfo InputObject)
    {
        if (InputObject.IsNullOrEmpty()) return false;
        return FileSystem.DeleteOnReboot(InputObject.FullName);
    }

    /// <summary>
    /// Queue the specified DirectoryInfo for deletion on next system reboot.
    /// </summary>
    /// <param name="InputObject"></param>
    /// <returns></returns>
    public static bool DeleteOnReboot(this DirectoryInfo InputObject)
    {
        if (InputObject.IsNullOrEmpty()) return false;
        return FileSystem.DeleteOnReboot(InputObject.FullName);
    }

    /// <summary>
    /// Iterates over Path to find if an file name exists along it
    /// </summary>
    /// <param name="Executable"></param>
    /// <param name="Substitutions"></param>
    /// <returns>FileInfo</returns>
    public static FileInfo? FindExecutable(this string? Executable, Dictionary<string, string>? Substitutions = null)
    {
        Executable = Executable!.ExpandVariables(Substitutions);
        if (Executable.IsNullOrEmptyOrWhiteSpace()) return null;
        if (!OperatingSystem.IsWindows())
        {
            string extension = Executable.GetFileExtension();
            if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) || extension.Equals(".bat", StringComparison.OrdinalIgnoreCase) || extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                Executable = Executable.GetFileNameWithoutExtension();
            }
        }

        Logging.Verbose($"[FindExecutable] Looking for executable: {Executable}");
        if (Path.IsPathFullyQualified(Executable!))
        {
            if (Executable.PathExists().Exist)
                return new FileInfo(Path.GetFullPath(Executable));
            else
                return null;
        }

        char Separator = Path.PathSeparator;
        string[] Paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Separator);
        foreach (string FoundPath in Paths)
        {
            string TrimmedPath = FoundPath.Trim();
            if (TrimmedPath.IsNullOrEmptyOrWhiteSpace()) continue;
            string FullPath = TrimmedPath.AppendPath(Executable!);

            if (OperatingSystem.IsWindows())
            {
                Logging.Verbose($"[FindExecutable::Windows] Path: {FullPath}");

                if (Path.HasExtension(FullPath))
                {
                    if (FullPath.PathExists().Exist)
                        return new FileInfo(Path.GetFullPath(FullPath));
                }
                else
                {
                    string[] ValidExtensions = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".exe;.bat;.cmd").Split(';');
                    foreach (string Ext in ValidExtensions)
                    {
                        string Candidate = FullPath + Ext;
                        if (Candidate.PathExists().Exist)
                            return new FileInfo(Path.GetFullPath(Candidate));
                    }
                }
            }
            else
            {
                Logging.Verbose($"[FindExecutable] Path: {FullPath}");
                if (FullPath.PathExists().Exist && (new FileInfo(FullPath).Attributes & FileAttributes.Directory) == 0)
                {
                    return new FileInfo(FullPath);
                }
                else
                {
                    string? caseinsens = FullPath.FindFileIgnoreCase();
                    if (!caseinsens.IsNullOrEmpty())
                        return new FileInfo(caseinsens);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Iterates over the directory for the passed path to find matching files while ignoring case
    /// </summary>
    /// <param name="InputPath"></param>
    /// <returns></returns>
    public static string? FindFileIgnoreCase(this string InputPath)
    {
        string? directory = InputPath.GetDirectoryName();
        string? fileName = InputPath.GetFileName();
        if (directory.IsNullOrEmptyOrWhiteSpace() || fileName.IsNullOrEmptyOrWhiteSpace() || !directory.PathExists().Exist)
        {
            return null;
        }

        string? ret = null;
        try
        {
            ret = Directory.EnumerateFileSystemEntries(directory).FirstOrDefault(x => string.Equals(Path.GetFileName(x), fileName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception Ex)
        {
            Message.ShowError($"Unable to find file: {InputPath}", exception: Ex).Wait();
        }

        return ret;
    }

    /// <summary>
    /// Moves the File or Directory to the provided destination full name
    /// </summary>
    /// <param name="SourcePath"></param>
    /// <param name="DestinationPath"></param>
    public static void TryMovePath(this string SourcePath, string DestinationPath)
    {
        string TempSourcePath = SourcePath.ResolvePath();
        string TempDestinationPath = DestinationPath.ResolvePath();

        FileSystemData fsd = TempSourcePath.PathExists();

        if (fsd.Exist)
        {
            try
            {
                if (fsd.Type == typeof(File)) { File.Move(TempSourcePath, TempDestinationPath); }
                if (fsd.Type == typeof(Directory)) { Directory.Move(TempSourcePath, TempDestinationPath); }
            }
            catch (Exception Ex)
            {
                Message.ShowError($"Unable to move {fsd.Type?.GetType()} path [ {TempSourcePath} ] to: {TempDestinationPath}", exception: Ex).Wait();
            }
        }
    }

    /// <summary>
    /// Moves the File or Directory to the provided destination full name asynchronously
    /// </summary>
    /// <param name="SourcePath"></param>
    /// <param name="DestinationPath"></param>
    /// <returns></returns>
    public static async Task TryMovePathAsync(this string SourcePath, string DestinationPath)
    {
        string TempSourcePath = SourcePath.ResolvePath();
        string TempDestinationPath = DestinationPath.ResolvePath();

        await Task.Run(() =>
        {
            TryMovePath(SourcePath, DestinationPath);
        });
    }

    /// <summary>
    /// Deletes the File or Directory if it exists
    /// </summary>
    /// <param name="ItemPath"></param>
    public static void TryDeletePath(this string ItemPath)
    {
        string TempPath = ItemPath.ResolvePath();
        FileSystemData fsd = TempPath.PathExists();

        if (fsd.Exist)
        {
            try
            {
                if (fsd.Type == typeof(File)) { File.Delete(TempPath); }
                if (fsd.Type == typeof(Directory)) { Directory.Delete(TempPath, true); }
            }
            catch (Exception Ex)
            {
                Message.ShowError($"Unable to delete {fsd.Type?.GetType()} path: {TempPath}", exception: Ex).Wait();
            }
        }
    }

    /// <summary>
    /// Deletes the File or Directory if it exists asynchronously
    /// </summary>
    /// <param name="ItemPath"></param>
    /// <returns></returns>
    public static async Task TryDeletePathAsync(this string ItemPath)
    {
        string TempPath = ItemPath.ResolvePath();
        await Task.Run(() =>
        {
            TryDeletePath(ItemPath);
        });
    }

    /// <summary>
    /// Returns a list of FileInfo from the passed path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static List<FileInfo> GetFiles(this string path, string pattern = "*.*", SearchOption option = SearchOption.TopDirectoryOnly)
    {
        List<FileInfo> ret = [];
        if (!path.PathExists().Exist) return ret;

        try
        {
            ret.AddRange(Directory.EnumerateFiles(path, pattern, option).Select(file => new FileInfo(file)));
        }
        catch (Exception Ex)
        {
            Message.ShowError($"Unable to enumerate files in: {path}", exception: Ex).Wait();
        }

        return ret;
    }

    /// <summary>
    /// Gets the passed file path content and returns it as a string
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static string? GetFileContent(this string Path)
    {
        if (Path.IsNullOrEmptyOrWhiteSpace()) return null;
        if (!Path.PathExists().Exist) return null;

        string? content = null;
        try
        {
            using var reader = new StreamReader(Path);
            content = reader.ReadToEnd();
        }
        catch (Exception Ex)
        {
            Message.ShowError($"Unable to read: {Path}", exception: Ex).Wait();
        }

        return content;
    }

    /// <summary>
    /// Gets the passed file info content and returns it as a string
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static string? GetFileContent(this FileInfo Path)
    {
        if (Path.IsNullOrEmpty()) return null;
        return GetFileContent(Path.FullName);
    }

    /// <summary>
    /// Replaces regex patterns in a file with passed text
    /// </summary>
    /// <param name="Path"></param>
    /// <param name="Replacements"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void RegExReplaceInFile(this string Path, Dictionary<string, string> Replacements, bool EscapeKeys = false)
    {
        if (!Path.PathExists().Exist)
            throw new FileNotFoundException($"File [ {Path} ] was not found!");

        if (Replacements.IsNullOrEmpty() || Replacements.Count == 0)
            return;

        Encoding encoding;
        string content;
        using (var reader = new StreamReader(Path, detectEncodingFromByteOrderMarks: true))
        {
            encoding = reader.CurrentEncoding;
            content = reader.ReadToEnd();
        }

        foreach (var kvp in Replacements)
        {
            string pattern = kvp.Key;
            if (!pattern.IsRegexMatch("^RegEx:")) { if (EscapeKeys) pattern = pattern.RegexEscape(); }
            else { pattern = pattern.RegexReplace(@"^RegEx:", ""); }
            string replacement = kvp.Value ?? string.Empty;
            content = Regex.Replace(content, pattern, replacement, RegexOptions.Multiline);
        }

        using var writer = new StreamWriter(Path, false, encoding);
        writer.Write(content);
    }

    /// <summary>
    /// Writes a string to the passed file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="path"></param>
    /// <param name="append"></param>
    public static void OutFile(this string content, string path, bool append = true)
    {
        string? resolvedPath = path.ExpandVariables();

        try
        {
            Path.GetDirectoryName(resolvedPath).TryCreateDirectory();
            using StreamWriter sw = new(resolvedPath!, append);
            sw.WriteLine(content);
        }
        catch (Exception Ex)
        {
            Message.ShowError($"Unable to write content to: {resolvedPath}", exception: Ex).Wait();
        }
    }

    /// <summary>
    /// Writes a string collection to the passed file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="path"></param>
    /// <param name="append"></param>
    public static void OutFile(this IEnumerable<string> content, string path, bool append = true)
    {
        foreach (string line in content)
        {
            line.OutFile(path, append);
        }
    }

    /// <summary>
    /// Writes a string to the passed file asynchronously
    /// </summary>
    /// <param name="content"></param>
    /// <param name="path"></param>
    /// <param name="append"></param>
    /// <returns></returns>
    public static async Task OutFileAsync(this string content, string path, bool append = true)
    {
        string? resolvedPath = path.ExpandVariables();

        try
        {
            Path.GetDirectoryName(resolvedPath).TryCreateDirectory();
            using StreamWriter sw = new(resolvedPath!, append);
            await sw.WriteLineAsync(content);
        }
        catch (Exception Ex)
        {
            Message.ShowError($"Unable to write content to: {resolvedPath}", exception: Ex).Wait();
        }
    }

    /// <summary>
    /// Writes a string collection to the passed file asynchronously
    /// </summary>
    /// <param name="content"></param>
    /// <param name="path"></param>
    /// <param name="append"></param>
    /// <returns></returns>
    public static async Task OutFileAsync(this IEnumerable<string> content, string path, bool append = true)
    {
        foreach (string line in content)
        {
            await line.OutFileAsync(path, append);
        }
    }

#endregion
}