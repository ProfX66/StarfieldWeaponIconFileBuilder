using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static class Processes
{
    #region Methods

    /// <summary>
    /// Runs the passed program and waits for return with Standard Output/Error and file exists protection and timeout
    /// </summary>
    /// <param name="FileName"></param>
    /// <param name="FileArgs"></param>
    /// <param name="WorkingDir"></param>
    /// <param name="ShowWindow"></param>
    /// <param name="UseTimer"></param>
    /// <param name="Timeout"></param>
    /// <param name="EnvSubstitutions"></param>
    /// <returns></returns>
    public static ProcessData RunAndWait(string FileName, string FileArgs, string? WorkingDir = null, bool ShowWindow = false, bool UseTimer = false, TimeSpan? Timeout = null, Dictionary<string, string>? EnvSubstitutions = null)
    {
        Logging.Informational(string.Format("Launching [ {0} {1} ] from [ {2} ]", FileName, FileArgs, WorkingDir.ToFriendlyNull()));
        StringBuilder StandardOutputBuilder = new();
        StringBuilder StandardErrorBuilder = new();
        int rcCode = 999;
        bool timedout = false;
        string? strStdo = null;
        string? strStde = null;
        string? exFileName = FileName.ExpandVariables(EnvSubstitutions);

        if (!exFileName!.IsRegexMatch(@"^[A-Z]:\\|\\\\"))
        {
            if (!WorkingDir.IsNullOrEmpty())
            {
                exFileName = Path.Combine(WorkingDir!, exFileName!);
            }
            if (!File.Exists(exFileName))
            {
                FileInfo? fileInfo = exFileName.FindExecutable(EnvSubstitutions!);
                if (!fileInfo.IsNullOrEmpty())
                {
                    exFileName = fileInfo.FullName;
                }
            }
        }

        if (!File.Exists(exFileName))
        {
            throw new FileNotFoundException(string.Format("Unable to find file '{0}' either directly or in PATH", exFileName), exFileName);
        }

        using (Process dProc = new())
        {
            if (!WorkingDir.IsNullOrEmpty()) { dProc.StartInfo.WorkingDirectory = WorkingDir; }
            dProc.StartInfo.FileName = exFileName;
            dProc.StartInfo.RedirectStandardError = true;
            dProc.StartInfo.RedirectStandardOutput = true;
            dProc.StartInfo.UseShellExecute = false;

            if (UseTimer)
            {
                dProc.OutputDataReceived += (sender, eventArgs) => StandardOutputBuilder.AppendLine(eventArgs.Data);
                dProc.ErrorDataReceived += (sender, eventArgs) => StandardErrorBuilder.AppendLine(eventArgs.Data);
            }

            dProc.StartInfo.Arguments = FileArgs;
            dProc.StartInfo.CreateNoWindow = (!ShowWindow);
            dProc.Start();

            Logging.Informational(string.Format("PID: {0}", dProc.Id.ToString()));
            if (!UseTimer)
            {
                strStdo = dProc.StandardOutput.ReadToEnd();
                strStde = dProc.StandardError.ReadToEnd();
            }

            if (!UseTimer) { dProc.WaitForExit(); }
            else
            {
                dProc.BeginOutputReadLine();
                dProc.BeginErrorReadLine();

                if (!Timeout.HasValue)
                {
                    Logging.Informational("- Waiting for process to complete -".PadToLength(Logging.SeparatorLineLength, '-'));
                }
                else
                {
                    Logging.Informational("- Waiting for process to complete or timeout has elapsed -".PadToLength(Logging.SeparatorLineLength, '-'));
                    Logging.Informational(string.Format("Timeout: {0}", Timeout.ToFriendlyTime()));
                    Logging.Informational("-".PadToLength(Logging.SeparatorLineLength, '-'));
                }

                bool doLoop = true;
                int count = 0;
                double interval = 5;

                while (doLoop)
                {
                    count++;
                    Timers.Wait((int)interval, true);
                    double etotaldur = (count * interval) / 60;
                    Logging.Informational(string.Format("[Process] Total Elapsed: {0:N2} Minute(s) [Loop: {1}]", etotaldur, count));
                    Logging.Verbose(string.Format("Has Exited: {0}", dProc.HasExited.ToString()));
                    if (Timeout.HasValue && etotaldur >= Timeout.Value.TotalMinutes)
                    {
                        Logging.Informational("-".PadToLength(Logging.SeparatorLineLength, '-'));
                        Logging.Advisory(string.Format("[Process] Timeout has been reached! [{0:N2} Minute(s) >= {1:N2} Minute(s)]", etotaldur, Timeout.Value.TotalMinutes));
                        dProc.Kill();
                        timedout = true;
                        doLoop = false; break;
                    }
                    if (dProc.HasExited) { doLoop = false; break; }
                }

                strStdo = StandardOutputBuilder.ToString();
                strStde = StandardErrorBuilder.ToString();
            }

            Logging.Informational("- Output -".PadToLength(Logging.SeparatorLineLength, '-'));
            Logging.Informational(strStdo!.RegexReplace(@"\x00", "", RegexOptions.IgnoreCase));
            if (!strStde.IsNullOrEmpty())
            {
                Logging.Informational("- Error -".PadToLength(Logging.SeparatorLineLength, '-'));
                Logging.Informational(strStde!);
            }
            Logging.Informational("-".PadToLength(Logging.SeparatorLineLength, '-'));

            rcCode = dProc.ExitCode;
            if (timedout) { rcCode = 888; }
            Logging.Informational(string.Format("RC: {0}", rcCode.ToString()));
        }

        return new ProcessData { RC = rcCode, StandardOutput = strStdo!, StandardError = strStde! };
    }

    #endregion
}
