using Andy.TUI.Terminal;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a system monitor similar to 'top' command with colored output.
/// </summary>
public class SystemMonitorExample
{
    private static readonly CpuUsageTracker _cpuTracker = new();
    public static void Run()
    {
        Console.WriteLine("=== System Monitor Example ===");
        Console.WriteLine("A simple 'top'-like system monitor");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();
        
        // Create input handler for exit
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        inputHandler.KeyPressed += (_, e) =>
        {
            if (e.Key == ConsoleKey.Escape || e.Key == ConsoleKey.Q)
                exit = true;
        };
        inputHandler.Start();
        
        // Update interval
        var lastUpdate = DateTime.Now;
        var updateInterval = TimeSpan.FromSeconds(1);
        
        // Initial CPU tracking update
        _cpuTracker.Update();
        
        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 10; // Low FPS for system monitor
        
        // Animation render function
        Action? renderFrame = null;
        renderFrame = () =>
        {
            if (exit)
                return;
                
            var now = DateTime.Now;
            if (now - lastUpdate >= updateInterval)
            {
                lastUpdate = now;
                
                // Update CPU tracking
                _cpuTracker.Update();
                
                renderingSystem.Clear();
                
                DrawHeader(renderingSystem, now);
                DrawSystemInfo(renderingSystem);
                DrawProcessList(renderingSystem);
                DrawFooter(renderingSystem);
            }
            
            // Queue next frame
            renderingSystem.Scheduler.QueueRender(renderFrame);
        };
        
        // Start animation
        renderingSystem.Scheduler.QueueRender(renderFrame);
        
        // Wait for exit
        while (!exit)
        {
            Thread.Sleep(100);
        }
        
        inputHandler.Stop();
        inputHandler.Dispose();
        
        Console.Clear();
        Console.WriteLine("\nSystem Monitor closed.");
    }
    
    private static void DrawHeader(RenderingSystem renderingSystem, DateTime now)
    {
        // Title bar
        renderingSystem.DrawBox(0, 0, renderingSystem.Terminal.Width, 3, 
            Style.Default.WithForegroundColor(Color.Cyan), BoxStyle.Double);
        
        var title = " SYSTEM MONITOR ";
        renderingSystem.WriteText((renderingSystem.Terminal.Width - title.Length) / 2, 0, title, 
            Style.Default.WithForegroundColor(Color.White).WithBold());
        
        // Current time
        var timeStr = now.ToString("HH:mm:ss");
        renderingSystem.WriteText(renderingSystem.Terminal.Width - timeStr.Length - 2, 1, timeStr, 
            Style.Default.WithForegroundColor(Color.Yellow));
        
        // Uptime
        var uptime = GetUptime();
        renderingSystem.WriteText(2, 1, $"Uptime: {uptime}", 
            Style.Default.WithForegroundColor(Color.Green));
    }
    
    private static void DrawSystemInfo(RenderingSystem renderingSystem)
    {
        var y = 4;
        
        // CPU Usage
        var cpuUsage = GetCpuUsage();
        renderingSystem.WriteText(2, y, "CPU Usage: ", Style.Default.WithForegroundColor(Color.White));
        DrawProgressBar(renderingSystem, 14, y, 30, cpuUsage, GetCpuColor(cpuUsage));
        renderingSystem.WriteText(46, y, $"{cpuUsage:F1}%", Style.Default.WithForegroundColor(GetCpuColor(cpuUsage)));
        
        // Memory Usage
        y++;
        var (memUsed, memTotal, memPercent) = GetMemoryUsage();
        renderingSystem.WriteText(2, y, "Memory:    ", Style.Default.WithForegroundColor(Color.White));
        DrawProgressBar(renderingSystem, 14, y, 30, memPercent, GetMemoryColor(memPercent));
        renderingSystem.WriteText(46, y, $"{memUsed:F1}/{memTotal:F1} GB ({memPercent:F1}%)", 
            Style.Default.WithForegroundColor(GetMemoryColor(memPercent)));
        
        // Process Count
        y++;
        var processCount = Process.GetProcesses().Length;
        renderingSystem.WriteText(2, y, $"Processes: {processCount}", 
            Style.Default.WithForegroundColor(Color.Cyan));
        
        // Thread Count
        var threadCount = Process.GetCurrentProcess().Threads.Count;
        renderingSystem.WriteText(25, y, $"Threads: {threadCount}", 
            Style.Default.WithForegroundColor(Color.Cyan));
    }
    
    private static void DrawProcessList(RenderingSystem renderingSystem)
    {
        var y = 9;
        
        // Header
        renderingSystem.DrawBox(0, y - 1, renderingSystem.Terminal.Width, renderingSystem.Terminal.Height - y - 2,
            Style.Default.WithForegroundColor(Color.DarkGray), BoxStyle.Single);
        
        renderingSystem.WriteText(2, y, " TOP PROCESSES BY CPU ", 
            Style.Default.WithForegroundColor(Color.White).WithBold());
        
        y += 2;
        
        // Column headers
        var headerStyle = Style.Default.WithForegroundColor(Color.Yellow).WithUnderline();
        renderingSystem.WriteText(2, y, "PID", headerStyle);
        renderingSystem.WriteText(10, y, "Process Name", headerStyle);
        renderingSystem.WriteText(40, y, "CPU %", headerStyle);
        renderingSystem.WriteText(50, y, "Memory MB", headerStyle);
        renderingSystem.WriteText(62, y, "Status", headerStyle);
        
        y++;
        
        // Get top processes
        var processes = GetTopProcesses(10);
        
        foreach (var proc in processes)
        {
            if (y >= renderingSystem.Terminal.Height - 4) break;
            
            // PID
            renderingSystem.WriteText(2, y, proc.Id.ToString().PadRight(7), 
                Style.Default.WithForegroundColor(Color.DarkGray));
            
            // Process name
            var name = proc.ProcessName.Length > 28 ? proc.ProcessName.Substring(0, 25) + "..." : proc.ProcessName;
            renderingSystem.WriteText(10, y, name.PadRight(29), 
                Style.Default.WithForegroundColor(Color.White));
            
            // CPU (simulated)
            var cpu = GetProcessCpu(proc);
            var cpuColor = GetCpuColor(cpu);
            renderingSystem.WriteText(40, y, cpu.ToString("F1").PadLeft(5), 
                Style.Default.WithForegroundColor(cpuColor));
            
            // Memory
            var memory = proc.WorkingSet64 / (1024.0 * 1024.0);
            var memColor = memory > 1000 ? Color.Red : memory > 500 ? Color.Yellow : Color.Green;
            renderingSystem.WriteText(50, y, memory.ToString("F0").PadLeft(9), 
                Style.Default.WithForegroundColor(memColor));
            
            // Status
            var status = proc.Responding ? "Running" : "Not Resp";
            var statusColor = proc.Responding ? Color.Green : Color.Red;
            renderingSystem.WriteText(62, y, status, 
                Style.Default.WithForegroundColor(statusColor));
            
            y++;
        }
    }
    
    private static void DrawFooter(RenderingSystem renderingSystem)
    {
        var y = renderingSystem.Terminal.Height - 2;
        renderingSystem.WriteText(2, y, "Press ESC or Q to exit | Updates every 1 second", 
            Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private static void DrawProgressBar(RenderingSystem renderingSystem, int x, int y, int width, 
        double percentage, Color color)
    {
        var filled = (int)(width * percentage / 100.0);
        var style = Style.Default.WithForegroundColor(color);
        
        renderingSystem.Buffer.SetCell(x, y, '[', Style.Default.WithForegroundColor(Color.DarkGray));
        
        for (int i = 0; i < width; i++)
        {
            if (i < filled)
            {
                renderingSystem.Buffer.SetCell(x + 1 + i, y, '█', style);
            }
            else
            {
                renderingSystem.Buffer.SetCell(x + 1 + i, y, '░', Style.Default.WithForegroundColor(Color.DarkGray));
            }
        }
        
        renderingSystem.Buffer.SetCell(x + width + 1, y, ']', Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private static Color GetCpuColor(double cpu)
    {
        if (cpu > 80) return Color.Red;
        if (cpu > 50) return Color.Yellow;
        return Color.Green;
    }
    
    private static Color GetMemoryColor(double percent)
    {
        if (percent > 90) return Color.Red;
        if (percent > 70) return Color.Yellow;
        return Color.Green;
    }
    
    private static string GetUptime()
    {
        try
        {
            TimeSpan uptime;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, use system uptime
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // On Linux, read from /proc/uptime
                var uptimeStr = File.ReadAllText("/proc/uptime").Split(' ')[0];
                if (double.TryParse(uptimeStr, out var seconds))
                    uptime = TimeSpan.FromSeconds(seconds);
                else
                    uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            }
            else
            {
                // On macOS and others, use process uptime as fallback
                uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            }
            
            if (uptime.TotalDays >= 1)
                return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
            else if (uptime.TotalHours >= 1)
                return $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
            else
                return $"{uptime.Minutes}m {uptime.Seconds}s";
        }
        catch
        {
            return "Unknown";
        }
    }
    
    private static double GetCpuUsage()
    {
        return _cpuTracker.GetTotalCpuUsage();
    }
    
    private static (double used, double total, double percent) GetMemoryUsage()
    {
        // Get total system memory
        var totalBytes = SystemInfo.GetTotalPhysicalMemory();
        var total = totalBytes / (1024.0 * 1024.0 * 1024.0); // GB
        
        // Calculate used memory (this is still an approximation)
        var availableMemory = GC.GetTotalMemory(false);
        var process = Process.GetCurrentProcess();
        var processMemory = process.WorkingSet64;
        
        // Get all processes memory usage for better approximation
        long totalUsed = 0;
        try
        {
            foreach (var p in Process.GetProcesses())
            {
                try { totalUsed += p.WorkingSet64; } catch { }
            }
        }
        catch { totalUsed = processMemory * 10; } // Fallback
        
        var used = totalUsed / (1024.0 * 1024.0 * 1024.0); // GB
        var percent = (used / total) * 100;
        
        return (used, total, percent);
    }
    
    private static List<Process> GetTopProcesses(int count)
    {
        try
        {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                .Select(p => new { Process = p, Cpu = _cpuTracker.GetProcessCpuUsage(p.Id) })
                .OrderByDescending(p => p.Cpu)
                .ThenByDescending(p => p.Process.WorkingSet64)
                .Take(count)
                .Select(p => p.Process)
                .ToList();
            
            return processes;
        }
        catch
        {
            return new List<Process>();
        }
    }
    
    private static double GetProcessCpu(Process process)
    {
        return _cpuTracker.GetProcessCpuUsage(process.Id);
    }
}