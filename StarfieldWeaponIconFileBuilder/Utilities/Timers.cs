using StarfieldWeaponIconFileBuilder.Extensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static class Timers
{
    #region Methods

    /// <summary>
    /// Sleeps the thread for the passed duration
    /// </summary>
    /// <param name="Duration">Seconds to wait</param>
    /// <param name="silent"></param>
    public static void Wait(int Duration, bool silent = false)
    {
        if (!silent) Logging.Informational(string.Format("Waiting for {0} seconds...", Duration));
        Stopwatch sw = new();
        sw.Start();
        while (sw.Elapsed < TimeSpan.FromSeconds(Duration))
        {
            if (!silent) Logging.Verbose(string.Format("Elapsed: {0} [{1}]", sw.Elapsed.ToFriendlyTime(), TimeSpan.FromSeconds(Duration).ToFriendlyTime()));
            Thread.Sleep(1000);
        }
        sw.Stop();
        sw.Reset();
    }

    /// <summary>
    /// Sleeps the thread for the passed duration unless stopped by in bool
    /// </summary>
    /// <param name="Duration"></param>
    /// <param name="BreakLoop"></param>
    /// <param name="silent"></param>
    public static void Wait(int Duration, in bool BreakLoop, bool silent = false)
    {
        if (!silent) Logging.Informational(string.Format("Waiting for {0} seconds...", Duration));
        Stopwatch sw = new();
        sw.Start();
        while (sw.Elapsed < TimeSpan.FromSeconds(Duration))
        {
            if (!silent) Logging.Verbose(string.Format("Elapsed: {0} [{1}]", sw.Elapsed.ToFriendlyTime(), TimeSpan.FromSeconds(Duration).ToFriendlyTime()));
            Thread.Sleep(1000);
            if (BreakLoop) break;
        }
        sw.Stop();
        sw.Reset();
    }

    /// <summary>
    /// Sleeps the thread for the passed duration
    /// </summary>
    /// <param name="Duration">TimeSpan to wait</param>
    /// <param name="silent"></param>
    public static void Wait(TimeSpan? Duration = null, bool silent = false)
    {
        TimeSpan RealDuration = new(0, 0, 5);
        if (Duration.HasValue) { RealDuration = Duration.Value; }
        if (!silent) Logging.Informational(string.Format("Waiting for {0}", RealDuration.ToFriendlyTime()));
        Stopwatch sw = new();
        sw.Start();
        while (sw.Elapsed < RealDuration)
        {
            if (!silent) Logging.Verbose(string.Format("Elapsed: {0} [{1}]", sw.Elapsed.ToFriendlyTime(), RealDuration.ToFriendlyTime()));
            Thread.Sleep(1000);
        }
        sw.Stop();
        sw.Reset();
    }

    /// <summary>
    /// Sleeps the thread for the passed duration unless stopped by in bool
    /// </summary>
    /// <param name="BreakLoop"></param>
    /// <param name="Duration"></param>
    /// <param name="silent"></param>
    public static void Wait(in bool BreakLoop, TimeSpan? Duration = null, bool silent = false)
    {
        TimeSpan RealDuration = new(0, 0, 5);
        if (Duration.HasValue) { RealDuration = Duration.Value; }
        if (!silent) Logging.Informational(string.Format("Waiting for {0}", RealDuration.ToFriendlyTime()));
        Stopwatch sw = new();
        sw.Start();
        while (sw.Elapsed < RealDuration)
        {
            if (!silent) Logging.Verbose(string.Format("Elapsed: {0} [{1}]", sw.Elapsed.ToFriendlyTime(), RealDuration.ToFriendlyTime()));
            Thread.Sleep(1000);
            if (BreakLoop) break;
        }
        sw.Stop();
        sw.Reset();
    }

    /// <summary>
    /// Sleeps the thread for the passed duration asynchronously
    /// </summary>
    /// <param name="Duration">Seconds to wait</param>
    /// <param name="silent"></param>
    /// <returns></returns>
    public static async Task WaitAsync(int Duration, bool silent = false)
    {
        await Task.Run(() => {
            Wait(Duration, silent);
        });
    }

    /// <summary>
    /// Sleeps the thread for the passed duration asynchronously
    /// </summary>
    /// <param name="Duration">TimeSpan to wait</param>
    /// <param name="silent"></param>
    /// <returns></returns>
    public static async Task WaitAsync(TimeSpan? Duration = null, bool silent = false)
    {
        await Task.Run(() => {
            Wait(Duration, silent);
        });
    }

    /// <summary>
    /// Waits indefinitely until the in bool is false
    /// </summary>
    /// <param name="BreakLoop"></param>
    /// <param name="silent"></param>
    public static void WaitUntil(in bool Loop, bool silent = false)
    {
        if (!silent) Logging.Informational("Waiting until reference value becomes false...");
        Stopwatch sw = new();
        sw.Start();
        while (Loop)
        {
            if (!silent) Logging.Verbose(string.Format("Elapsed: {0} [{1}]", sw.Elapsed.ToFriendlyTime(), Loop));
            Thread.Sleep(1000);
        }
        sw.Stop();
        sw.Reset();
    }

    #endregion
}
