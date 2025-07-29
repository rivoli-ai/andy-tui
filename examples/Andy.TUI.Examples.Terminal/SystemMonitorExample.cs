using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a system monitor similar to 'top' command with colored output.
/// </summary>
public class SystemMonitorExample
{
    public static void Run()
    {
        Console.WriteLine("=== System Monitor Example ===");
        Console.WriteLine("A simple 'top'-like system monitor");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        using var terminal = new AnsiTerminal();
        var renderer = new TerminalRenderer(terminal);
        
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
        
        while (!exit)
        {
            var now = DateTime.Now;
            if (now - lastUpdate >= updateInterval)
            {
                lastUpdate = now;
                
                renderer.BeginFrame();
                renderer.Clear();
                
                DrawHeader(renderer, now);
                DrawSystemInfo(renderer);
                DrawProcessList(renderer);
                DrawFooter(renderer);
                
                renderer.EndFrame();
            }
            
            Thread.Sleep(100); // Small delay to reduce CPU usage
        }
        
        inputHandler.Stop();
        inputHandler.Dispose();
        
        Console.Clear();
        Console.WriteLine("\nSystem Monitor closed.");
    }
    
    private static void DrawHeader(TerminalRenderer renderer, DateTime now)
    {
        // Title bar
        renderer.DrawBox(0, 0, renderer.Width, 3, BorderStyle.Double, 
            Style.Default.WithForegroundColor(Color.Cyan));
        
        var title = " SYSTEM MONITOR ";
        renderer.DrawText((renderer.Width - title.Length) / 2, 0, title, 
            Style.Default.WithForegroundColor(Color.White).WithBold());
        
        // Current time
        var timeStr = now.ToString("HH:mm:ss");
        renderer.DrawText(renderer.Width - timeStr.Length - 2, 1, timeStr, 
            Style.Default.WithForegroundColor(Color.Yellow));
        
        // Uptime
        var uptime = GetUptime();
        renderer.DrawText(2, 1, $"Uptime: {uptime}", 
            Style.Default.WithForegroundColor(Color.Green));
    }
    
    private static void DrawSystemInfo(TerminalRenderer renderer)
    {
        var y = 4;
        
        // CPU Usage
        var cpuUsage = GetCpuUsage();
        renderer.DrawText(2, y, "CPU Usage: ", Style.Default.WithForegroundColor(Color.White));
        DrawProgressBar(renderer, 14, y, 30, cpuUsage, GetCpuColor(cpuUsage));
        renderer.DrawText(46, y, $"{cpuUsage:F1}%", Style.Default.WithForegroundColor(GetCpuColor(cpuUsage)));
        
        // Memory Usage
        y++;
        var (memUsed, memTotal, memPercent) = GetMemoryUsage();
        renderer.DrawText(2, y, "Memory:    ", Style.Default.WithForegroundColor(Color.White));
        DrawProgressBar(renderer, 14, y, 30, memPercent, GetMemoryColor(memPercent));
        renderer.DrawText(46, y, $"{memUsed:F1}/{memTotal:F1} GB ({memPercent:F1}%)", 
            Style.Default.WithForegroundColor(GetMemoryColor(memPercent)));
        
        // Process Count
        y++;
        var processCount = Process.GetProcesses().Length;
        renderer.DrawText(2, y, $"Processes: {processCount}", 
            Style.Default.WithForegroundColor(Color.Cyan));
        
        // Thread Count
        var threadCount = Process.GetCurrentProcess().Threads.Count;
        renderer.DrawText(25, y, $"Threads: {threadCount}", 
            Style.Default.WithForegroundColor(Color.Cyan));
    }
    
    private static void DrawProcessList(TerminalRenderer renderer)
    {
        var y = 9;
        
        // Header
        renderer.DrawBox(0, y - 1, renderer.Width, renderer.Height - y - 2, BorderStyle.Single,
            Style.Default.WithForegroundColor(Color.DarkGray));
        
        renderer.DrawText(2, y, " TOP PROCESSES BY CPU ", 
            Style.Default.WithForegroundColor(Color.White).WithBold());
        
        y += 2;
        
        // Column headers
        var headerStyle = Style.Default.WithForegroundColor(Color.Yellow).WithUnderline();
        renderer.DrawText(2, y, "PID", headerStyle);
        renderer.DrawText(10, y, "Process Name", headerStyle);
        renderer.DrawText(40, y, "CPU %", headerStyle);
        renderer.DrawText(50, y, "Memory MB", headerStyle);
        renderer.DrawText(62, y, "Status", headerStyle);
        
        y++;
        
        // Get top processes
        var processes = GetTopProcesses(10);
        
        foreach (var proc in processes)
        {
            if (y >= renderer.Height - 4) break;
            
            // PID
            renderer.DrawText(2, y, proc.Id.ToString().PadRight(7), 
                Style.Default.WithForegroundColor(Color.DarkGray));
            
            // Process name
            var name = proc.ProcessName.Length > 28 ? proc.ProcessName.Substring(0, 25) + "..." : proc.ProcessName;
            renderer.DrawText(10, y, name.PadRight(29), 
                Style.Default.WithForegroundColor(Color.White));
            
            // CPU (simulated)
            var cpu = GetProcessCpu(proc);
            var cpuColor = GetCpuColor(cpu);
            renderer.DrawText(40, y, cpu.ToString("F1").PadLeft(5), 
                Style.Default.WithForegroundColor(cpuColor));
            
            // Memory
            var memory = proc.WorkingSet64 / (1024.0 * 1024.0);
            var memColor = memory > 1000 ? Color.Red : memory > 500 ? Color.Yellow : Color.Green;
            renderer.DrawText(50, y, memory.ToString("F0").PadLeft(9), 
                Style.Default.WithForegroundColor(memColor));
            
            // Status
            var status = proc.Responding ? "Running" : "Not Resp";
            var statusColor = proc.Responding ? Color.Green : Color.Red;
            renderer.DrawText(62, y, status, 
                Style.Default.WithForegroundColor(statusColor));
            
            y++;
        }
    }
    
    private static void DrawFooter(TerminalRenderer renderer)
    {
        var y = renderer.Height - 2;
        renderer.DrawText(2, y, "Press ESC or Q to exit | Updates every 1 second", 
            Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private static void DrawProgressBar(TerminalRenderer renderer, int x, int y, int width, 
        double percentage, Color color)
    {
        var filled = (int)(width * percentage / 100.0);
        var style = Style.Default.WithForegroundColor(color);
        
        renderer.DrawChar(x, y, '[', Style.Default.WithForegroundColor(Color.DarkGray));
        
        for (int i = 0; i < width; i++)
        {
            if (i < filled)
            {
                renderer.DrawChar(x + 1 + i, y, '█', style);
            }
            else
            {
                renderer.DrawChar(x + 1 + i, y, '░', Style.Default.WithForegroundColor(Color.DarkGray));
            }
        }
        
        renderer.DrawChar(x + width + 1, y, ']', Style.Default.WithForegroundColor(Color.DarkGray));
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
            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
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
        // Simplified CPU usage - in real implementation you'd track over time
        var process = Process.GetCurrentProcess();
        var startTime = DateTime.UtcNow;
        var startCpuUsage = process.TotalProcessorTime;
        
        Thread.Sleep(100);
        
        var endTime = DateTime.UtcNow;
        var endCpuUsage = process.TotalProcessorTime;
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        
        return Math.Min(100, cpuUsageTotal * 100);
    }
    
    private static (double used, double total, double percent) GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        var used = process.WorkingSet64 / (1024.0 * 1024.0 * 1024.0); // GB
        
        // Get total system memory (approximation)
        var total = Environment.WorkingSet / (1024.0 * 1024.0 * 1024.0) * 4; // Rough estimate
        if (total < used) total = used * 2; // Ensure total is reasonable
        
        var percent = (used / total) * 100;
        return (used, total, percent);
    }
    
    private static List<Process> GetTopProcesses(int count)
    {
        try
        {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                .OrderByDescending(p => p.WorkingSet64)
                .Take(count)
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
        // Simulated CPU usage per process
        // In a real implementation, you'd track CPU time over intervals
        Random rand = new Random(process.Id);
        return rand.NextDouble() * 20; // 0-20% simulated
    }
}