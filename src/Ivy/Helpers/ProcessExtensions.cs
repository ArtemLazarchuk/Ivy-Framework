using System.Diagnostics;

namespace Ivy.Helpers;

public static class ProcessExtensions
{
    /// <summary>
    /// Waits for the process to exit within the specified timeout.
    /// If the timeout expires, kills the entire process tree.
    /// </summary>
    /// <returns>true if the process exited within the timeout; false if it was killed.</returns>
    public static bool WaitForExitOrKill(this Process? process, int timeoutMs)
    {
        if (process is null) return true;
        if (!process.WaitForExit(timeoutMs))
        {
            try { process.Kill(true); } catch { /* already exited */ }
            return false;
        }
        return true;
    }
}
