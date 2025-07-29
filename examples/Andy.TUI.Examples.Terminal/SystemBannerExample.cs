using Andy.TUI.Terminal;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a system information banner similar to Ubuntu's MOTD.
/// </summary>
public class SystemBannerExample
{
    public static void Run()
    {
        Console.WriteLine("=== System Banner Example ===");
        Console.WriteLine("Displays a system information banner");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
        
        using var terminal = new AnsiTerminal();
        var renderer = new TerminalRenderer(terminal);
        
        renderer.BeginFrame();
        renderer.Clear();
        
        DrawBanner(renderer);
        
        // Draw exit instruction at the bottom
        renderer.DrawText(2, renderer.Height - 2, "Press any key to exit...", 
            Style.Default.WithForegroundColor(Color.DarkGray));
        
        renderer.EndFrame();
        
        Console.ReadKey(true);
    }
    
    private static void DrawBanner(TerminalRenderer renderer)
    {
        var y = 2;
        
        // Welcome header with ASCII art
        DrawAsciiLogo(renderer, 2, y);
        y += 6;
        
        // System information box
        renderer.DrawBox(2, y, renderer.Width - 4, 12, BorderStyle.Rounded,
            Style.Default.WithForegroundColor(Color.DarkGray));
        
        y += 1;
        
        // Welcome message
        var hostname = Environment.MachineName;
        var welcomeMsg = $"Welcome to {hostname}";
        renderer.DrawText(4, y, welcomeMsg, 
            Style.Default.WithForegroundColor(Color.Green).WithBold());
        
        y += 2;
        
        // System information
        DrawSystemInfo(renderer, 4, y);
        y += 5;
        
        // System usage
        DrawSystemUsage(renderer, 4, y);
        y += 3;
        
        // Updates and security
        y += 1;
        DrawUpdatesInfo(renderer, 2, y);
        y += 4;
        
        // Last login
        DrawLastLogin(renderer, 2, y);
    }
    
    private static void DrawAsciiLogo(TerminalRenderer renderer, int x, int y)
    {
        var logo = new[]
        {
            @"    _              _         _____ _   _ ___ ",
            @"   / \   _ __   __| |_   _  |_   _| | | |_ _|",
            @"  / _ \ | '_ \ / _` | | | |   | | | | | || | ",
            @" / ___ \| | | | (_| | |_| |   | | | |_| || | ",
            @"/_/   \_\_| |_|\__,_|\__, |   |_|  \___/|___|",
            @"                     |___/                    "
        };
        
        var logoStyle = Style.Default.WithForegroundColor(Color.Cyan);
        for (int i = 0; i < logo.Length; i++)
        {
            renderer.DrawText(x, y + i, logo[i], logoStyle);
        }
    }
    
    private static void DrawSystemInfo(TerminalRenderer renderer, int x, int y)
    {
        var labelStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        var valueStyle = Style.Default.WithForegroundColor(Color.White);
        
        // OS Information
        var os = RuntimeInformation.OSDescription;
        renderer.DrawText(x, y, "System: ", labelStyle);
        renderer.DrawText(x + 10, y, os, valueStyle);
        
        // Kernel/Runtime
        y++;
        renderer.DrawText(x, y, "Runtime: ", labelStyle);
        renderer.DrawText(x + 10, y, $".NET {Environment.Version}", valueStyle);
        
        // Architecture
        y++;
        renderer.DrawText(x, y, "Arch: ", labelStyle);
        renderer.DrawText(x + 10, y, RuntimeInformation.ProcessArchitecture.ToString(), valueStyle);
        
        // Uptime
        y++;
        var uptime = GetSystemUptime();
        renderer.DrawText(x, y, "Uptime: ", labelStyle);
        renderer.DrawText(x + 10, y, uptime, valueStyle);
    }
    
    private static void DrawSystemUsage(TerminalRenderer renderer, int x, int y)
    {
        var labelStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        
        // CPU usage
        var cpuUsage = GetCpuUsage();
        renderer.DrawText(x, y, "CPU: ", labelStyle);
        DrawUsageBar(renderer, x + 10, y, 20, cpuUsage, "cpu");
        
        // Memory usage
        y++;
        var (memUsed, memTotal, memPercent) = GetMemoryUsage();
        renderer.DrawText(x, y, "Memory: ", labelStyle);
        DrawUsageBar(renderer, x + 10, y, 20, memPercent, "memory");
        renderer.DrawText(x + 32, y, $"{memUsed:F1}G / {memTotal:F1}G", 
            Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private static void DrawUsageBar(TerminalRenderer renderer, int x, int y, int width, 
        double percentage, string type)
    {
        var filled = (int)(width * percentage / 100.0);
        
        // Determine color based on usage
        Color barColor;
        if (percentage > 80)
            barColor = Color.Red;
        else if (percentage > 50)
            barColor = Color.Yellow;
        else
            barColor = Color.Green;
        
        // Draw the bar
        renderer.DrawText(x, y, "[", Style.Default.WithForegroundColor(Color.DarkGray));
        
        for (int i = 0; i < width; i++)
        {
            if (i < filled)
            {
                renderer.DrawChar(x + 1 + i, y, '=', 
                    Style.Default.WithForegroundColor(barColor));
            }
            else
            {
                renderer.DrawChar(x + 1 + i, y, '-', 
                    Style.Default.WithForegroundColor(Color.DarkGray));
            }
        }
        
        renderer.DrawText(x + width + 1, y, $"] {percentage:F0}%", 
            Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private static void DrawUpdatesInfo(TerminalRenderer renderer, int x, int y)
    {
        // Simulated update information
        renderer.DrawBox(x, y, renderer.Width - 4, 3, BorderStyle.Single,
            Style.Default.WithForegroundColor(Color.Yellow));
        
        renderer.DrawText(x + 2, y + 1, "💡 ", Style.Default);
        renderer.DrawText(x + 5, y + 1, "System is up to date. No updates available.", 
            Style.Default.WithForegroundColor(Color.Yellow));
    }
    
    private static void DrawLastLogin(TerminalRenderer renderer, int x, int y)
    {
        var lastLogin = DateTime.Now.AddHours(-2.5); // Simulated
        var loginInfo = $"Last login: {lastLogin:ddd MMM dd HH:mm:ss yyyy} from console";
        
        renderer.DrawText(x, y, loginInfo, 
            Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private static string GetSystemUptime()
    {
        try
        {
            // Get actual system uptime on different platforms
            TimeSpan uptime;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, use Environment.TickCount64
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // On Linux, try to read /proc/uptime
                try
                {
                    var uptimeStr = System.IO.File.ReadAllText("/proc/uptime");
                    var uptimeSeconds = double.Parse(uptimeStr.Split(' ')[0]);
                    uptime = TimeSpan.FromSeconds(uptimeSeconds);
                }
                catch
                {
                    // Fallback to a simulated uptime
                    uptime = TimeSpan.FromDays(7.5).Add(TimeSpan.FromHours(3.25));
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // On macOS, try to get actual uptime using 'uptime' command
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/uptime",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        // Parse uptime output, e.g., "18:45  up 52 mins, 3 users, load averages: 2.50 2.89 3.22"
                        // or "18:45  up 2 days, 3:24, 3 users, load averages: 2.50 2.89 3.22"
                        uptime = ParseMacOSUptime(output);
                    }
                    else
                    {
                        // Fallback if process fails
                        uptime = TimeSpan.FromMinutes(52);
                    }
                }
                catch
                {
                    // Fallback for any errors
                    uptime = TimeSpan.FromMinutes(52);
                }
            }
            else
            {
                // Fallback for other platforms
                uptime = TimeSpan.FromDays(5.5).Add(TimeSpan.FromHours(2.25));
            }
            
            if (uptime.TotalDays >= 1)
            {
                return $"{(int)uptime.TotalDays} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
            }
            else if (uptime.TotalHours >= 1)
            {
                return $"{(int)uptime.TotalHours} hours, {uptime.Minutes} minutes";
            }
            else
            {
                return $"{(int)uptime.TotalMinutes} minutes";
            }
        }
        catch
        {
            return "Unknown";
        }
    }
    
    private static double GetCpuUsage()
    {
        // Simulate realistic CPU usage for a system
        var random = new Random();
        var baseUsage = 15.0; // Base system usage
        var variation = random.NextDouble() * 10; // 0-10% variation
        var spikes = random.NextDouble() < 0.1 ? random.NextDouble() * 30 : 0; // Occasional spikes
        
        return Math.Min(100, baseUsage + variation + spikes);
    }
    
    private static TimeSpan ParseMacOSUptime(string uptimeOutput)
    {
        // Parse macOS uptime command output
        // Examples:
        // "18:45  up 52 mins, 3 users, load averages: 2.50 2.89 3.22"
        // "18:45  up 2:30, 3 users, load averages: 2.50 2.89 3.22"
        // "18:45  up 2 days, 3:24, 3 users, load averages: 2.50 2.89 3.22"
        // "18:45  up 15 days, 22:03, 3 users, load averages: 2.50 2.89 3.22"
        
        try
        {
            var parts = uptimeOutput.Split(',')[0].Split("up")[1].Trim().Split(' ');
            
            if (parts.Length == 2 && parts[1] == "mins")
            {
                // Format: "52 mins"
                return TimeSpan.FromMinutes(int.Parse(parts[0]));
            }
            else if (parts.Length == 1 && parts[0].Contains(':'))
            {
                // Format: "2:30" (hours:minutes)
                var timeParts = parts[0].Split(':');
                return TimeSpan.FromHours(int.Parse(timeParts[0])) + TimeSpan.FromMinutes(int.Parse(timeParts[1]));
            }
            else if (parts.Length >= 3 && (parts[1] == "day" || parts[1] == "days"))
            {
                // Format: "2 days, 3:24" or "15 days, 22:03"
                var days = int.Parse(parts[0]);
                if (parts.Length >= 3 && parts[2].Contains(':'))
                {
                    var timeParts = parts[2].Split(':');
                    var hours = int.Parse(timeParts[0]);
                    var minutes = int.Parse(timeParts[1]);
                    return TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
                }
                else
                {
                    return TimeSpan.FromDays(days);
                }
            }
            else
            {
                // Fallback for unexpected format
                return TimeSpan.FromMinutes(52);
            }
        }
        catch
        {
            // If parsing fails, return a reasonable default
            return TimeSpan.FromMinutes(52);
        }
    }
    
    private static (double used, double total, double percent) GetMemoryUsage()
    {
        // Try to get actual system memory
        double total = 16.0; // Default fallback
        double used = 8.0;
        
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, try to use WMIC command
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "wmic",
                        Arguments = "computersystem get TotalPhysicalMemory /value",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        // Parse "TotalPhysicalMemory=137438953472"
                        var match = System.Text.RegularExpressions.Regex.Match(output, @"TotalPhysicalMemory=(\d+)");
                        if (match.Success)
                        {
                            var bytes = long.Parse(match.Groups[1].Value);
                            total = bytes / (1024.0 * 1024.0 * 1024.0); // Convert to GB
                        }
                    }
                    
                    // Simulate used memory
                    var random = new Random();
                    used = total * (0.3 + random.NextDouble() * 0.3); // 30-60% usage
                }
                catch
                {
                    // Fallback to estimate
                    total = 16.0;
                    used = 8.0;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Try to read from /proc/meminfo
                try
                {
                    var meminfo = System.IO.File.ReadAllLines("/proc/meminfo");
                    var totalLine = meminfo.FirstOrDefault(l => l.StartsWith("MemTotal:"));
                    var availLine = meminfo.FirstOrDefault(l => l.StartsWith("MemAvailable:"));
                    
                    if (totalLine != null)
                    {
                        var totalKb = double.Parse(totalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        total = totalKb / (1024 * 1024); // Convert KB to GB
                    }
                    
                    if (availLine != null && totalLine != null)
                    {
                        var availKb = double.Parse(availLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        var totalKb = double.Parse(totalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        used = (totalKb - availKb) / (1024 * 1024); // Convert KB to GB
                    }
                }
                catch
                {
                    // Keep defaults
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // On macOS, use sysctl to get actual memory
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/sbin/sysctl",
                        Arguments = "hw.memsize",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        // Parse "hw.memsize: 137438953472" (bytes)
                        var parts = output.Split(':');
                        if (parts.Length == 2)
                        {
                            var bytes = long.Parse(parts[1].Trim());
                            total = bytes / (1024.0 * 1024.0 * 1024.0); // Convert to GB
                        }
                    }
                    
                    // Get memory pressure for used memory estimate
                    processInfo.Arguments = "-l 1 -c 1";
                    processInfo.FileName = "/usr/bin/vm_stat";
                    
                    using var vmProcess = Process.Start(processInfo);
                    if (vmProcess != null)
                    {
                        var output = vmProcess.StandardOutput.ReadToEnd();
                        vmProcess.WaitForExit();
                        
                        // Parse vm_stat output to estimate used memory
                        // This is simplified - real implementation would parse all values
                        var random = new Random();
                        used = total * (0.3 + random.NextDouble() * 0.3); // 30-60% usage
                    }
                }
                catch
                {
                    // Keep defaults
                }
            }
        }
        catch
        {
            // Keep defaults
        }
        
        // Ensure used is not greater than total
        if (used > total) used = total * 0.8;
        
        var percent = (used / total) * 100;
        return (used, total, percent);
    }
}