using System;
using System.IO;
using System.Threading.Tasks;

namespace StarfieldWeaponIconFileBuilder.Extensions;
public static class BooleanExtensions
{
    #region Methods

    /// <summary>
    /// Converts object to Boolean
    /// </summary>
    /// <param name="Input"></param>
    /// <returns>Boolean</returns>
    public static bool ToBoolean(this object Input)
    {
        if (Input.IsNullOrEmpty()) return false;
        if (Input is bool bInput) return bInput;
        string strInput = (string)Input;

        if (strInput.IsRegexMatch(@"^[\d\.]{1}"))
        {
            return Convert.ToBoolean(Convert.ToInt32(strInput[..1]));
        }

        if (strInput.IsRegexMatch(@"^Yes"))
        {
            return true;
        }

        if (strInput.IsRegexMatch(@"^No"))
        {
            return false;
        }

        if (!strInput.IsRegexMatch(@"^(True|False)"))
        {
            return false;
        }

        bool bRet = Convert.ToBoolean(strInput);
        return bRet;
    }

    /// <summary>
    /// Converts object to Nullable Boolean
    /// </summary>
    /// <param name="Input"></param>
    /// <returns>Nullable Boolean</returns>
    public static bool? ToNullableBoolean(this object Input)
    {
        if (Input.IsNullOrEmpty()) return null;
        return Input.ToBoolean();
    }

    /// <summary>
    /// Gets the value or default of a nullable bool
    /// </summary>
    /// <param name="nullableBool"></param>
    /// <returns></returns>
    public static bool GetValueOrDefault(this bool? InputNullableBool)
    {
        return InputNullableBool ?? false;
    }

    /// <summary>
    /// Tests if the current user can write to the passed path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool TestWrite(this string path)
    {
        string dir = path;
        if (!Directory.Exists(path) && File.Exists(path))
        {
            dir = path.GetDirectoryName();
        }

        while (!dir.IsNullOrEmpty() && !Directory.Exists(dir))
        {
            dir = dir.GetDirectoryName();
        }

        if (dir.IsNullOrEmpty() || !Directory.Exists(dir)) return false;

        try
        {
            string testFile = Path.Combine(dir, ".write_test_" + Guid.NewGuid().ToString("N") + ".tmp");
            using (var fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
            {
                fs.Close();
            }

            testFile.DeleteOnReboot();
            testFile.TryDeletePath();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tests if the current user can write to the passed FileInfo path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool TestWrite(this FileInfo path)
    {
        return TestWrite(path.FullName);
    }

    /// <summary>
    /// Tests if the current user can write to the passed DirectoryInfo path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool TestWrite(this DirectoryInfo path)
    {
        return TestWrite(path.FullName);
    }

    /// <summary>
    /// Tests if the current user can write to the passed path asynchronously
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<bool> TestWriteAsync(this string path)
    {
        bool ret = false;
        await Task.Run(() =>
        {
            ret = TestWrite(path);
        });
        return ret;
    }

    /// <summary>
    /// Tests if the current user can write to the passed FileInfo path asynchronously
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<bool> TestWriteAsync(this FileInfo path)
    {
        return await TestWriteAsync(path.FullName); ;
    }

    /// <summary>
    /// Tests if the current user can write to the passed DirectoryInfo path asynchronously
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<bool> TestWriteAsync(this DirectoryInfo path)
    {
        return await TestWriteAsync(path.FullName); ;
    }

    /// <summary>
    /// Returns a bool if the passed object is an array
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>boolean</returns>
    public static bool IsArray(this object obj)
    {
        return !obj.IsNullOrEmpty() && obj.GetType().IsArray;
    }

    #endregion
}

