using System;
using System.Diagnostics;

/// <summary>
/// Convenience class to use a stopwatch with a debug log with frequent resetting.
/// </summary>
public class RBStopwatch
{
    /* Consts, Fields ========================================================================================================= */

    private string name;
    private Stopwatch stopwatch;

    /* Constructors, Enums ================================================================================================================== */

    /// <summary>
    /// Initializes a new instance of the <see cref="T:RBStopwatch"/> class.
    /// </summary>
    /// <param name="name">Name.</param>
    public RBStopwatch(string name)
    {
        this.name = name;
        this.stopwatch = new Stopwatch();
        this.stopwatch.Start();
    }

    /* Properties ============================================================================================================= */

    /* Methods ================================================================================================================ */

    /// <summary>
    /// Logs the time elapsed on the stopwatch since the last time it was constructed or Logged.
    /// </summary>
    /// <param name="logTag">Log tag.</param>
    public void LogLap(string logTag)
    {
        var logFormatString = string.Concat("[", name, "] ", logTag, " elapsed: {0} min, {1} sec, {2} milliseconds");
        TimeSpan ts = stopwatch.Elapsed;

        UnityEngine.Debug.LogFormat(null, logFormatString, ts.Minutes, ts.Seconds, ts.Milliseconds);

        this.stopwatch.Stop();
        this.stopwatch.Reset();
        this.stopwatch.Start();
    }

    /* Structs, Sub-Classes =================================================================================================== */
}