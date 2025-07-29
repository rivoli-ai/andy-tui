using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Andy.TUI.Terminal;

/// <summary>
/// Provides system information utilities.
/// </summary>
public static class SystemInfo
{
    /// <summary>
    /// Gets the total physical memory in bytes.
    /// </summary>
    public static long GetTotalPhysicalMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsTotalMemory();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxTotalMemory();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOSTotalMemory();
        }
        
        // Fallback - estimate based on current process
        return GC.GetTotalMemory(false) * 10;
    }
    
    private static long GetWindowsTotalMemory()
    {
        try
        {
            var output = ExecuteCommand("wmic", "computersystem get TotalPhysicalMemory /value").Trim();
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("TotalPhysicalMemory="))
                {
                    var value = line.Substring("TotalPhysicalMemory=".Length).Trim();
                    if (long.TryParse(value, out var bytes))
                        return bytes;
                }
            }
        }
        catch { }
        
        return 8L * 1024 * 1024 * 1024; // Default 8GB
    }
    
    private static long GetLinuxTotalMemory()
    {
        try
        {
            var meminfo = File.ReadAllLines("/proc/meminfo");
            foreach (var line in meminfo)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kb))
                        return kb * 1024;
                }
            }
        }
        catch { }
        
        return 8L * 1024 * 1024 * 1024; // Default 8GB
    }
    
    private static long GetMacOSTotalMemory()
    {
        try
        {
            var output = ExecuteCommand("sysctl", "-n hw.memsize").Trim();
            if (long.TryParse(output, out var bytes))
                return bytes;
        }
        catch { }
        
        return 8L * 1024 * 1024 * 1024; // Default 8GB
    }
    
    private static string ExecuteCommand(string command, string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
}

/// <summary>
/// Tracks CPU usage for processes.
/// </summary>
public class CpuUsageTracker
{
    private readonly Dictionary<int, ProcessCpuInfo> _processInfo = new();
    private DateTime _lastUpdate = DateTime.UtcNow;
    
    private class ProcessCpuInfo
    {
        public TimeSpan LastTotalProcessorTime { get; set; }
        public DateTime LastCheckTime { get; set; }
        public double CpuUsage { get; set; }
    }
    
    /// <summary>
    /// Updates CPU usage for all processes.
    /// </summary>
    public void Update()
    {
        var currentTime = DateTime.UtcNow;
        var timeDiff = (currentTime - _lastUpdate).TotalSeconds;
        
        if (timeDiff < 0.1) // Don't update too frequently
            return;
            
        var processes = Process.GetProcesses();
        var newProcessInfo = new Dictionary<int, ProcessCpuInfo>();
        
        foreach (var process in processes)
        {
            try
            {
                if (process.HasExited)
                    continue;
                    
                var currentCpuTime = process.TotalProcessorTime;
                
                if (_processInfo.TryGetValue(process.Id, out var info))
                {
                    var cpuTimeDiff = (currentCpuTime - info.LastTotalProcessorTime).TotalSeconds;
                    var realTimeDiff = (currentTime - info.LastCheckTime).TotalSeconds;
                    
                    if (realTimeDiff > 0)
                    {
                        var cpuUsage = (cpuTimeDiff / realTimeDiff) * 100.0 / Environment.ProcessorCount;
                        info.CpuUsage = Math.Min(100, Math.Max(0, cpuUsage));
                    }
                    
                    info.LastTotalProcessorTime = currentCpuTime;
                    info.LastCheckTime = currentTime;
                }
                else
                {
                    info = new ProcessCpuInfo
                    {
                        LastTotalProcessorTime = currentCpuTime,
                        LastCheckTime = currentTime,
                        CpuUsage = 0
                    };
                }
                
                newProcessInfo[process.Id] = info;
            }
            catch
            {
                // Process might have exited
            }
        }
        
        _processInfo.Clear();
        foreach (var kvp in newProcessInfo)
        {
            _processInfo[kvp.Key] = kvp.Value;
        }
        
        _lastUpdate = currentTime;
    }
    
    /// <summary>
    /// Gets the CPU usage for a specific process.
    /// </summary>
    public double GetProcessCpuUsage(int processId)
    {
        return _processInfo.TryGetValue(processId, out var info) ? info.CpuUsage : 0;
    }
    
    /// <summary>
    /// Gets total system CPU usage.
    /// </summary>
    public double GetTotalCpuUsage()
    {
        return _processInfo.Values.Sum(p => p.CpuUsage);
    }
}