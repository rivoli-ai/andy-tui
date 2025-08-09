namespace Andy.TUI.Examples.BubbleTea;

using System.Diagnostics;

public static class Exec
{
    public static void Run()
    {
        Console.WriteLine("Exec example: running 'uname -a' (press Enter to continue)");
        Console.ReadLine();

        var psi = new ProcessStartInfo
        {
            FileName = "bash",
            ArgumentList = { "-lc", "uname -a" },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi)!;
        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        Console.WriteLine("\n--- stdout ---\n" + stdout);
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            Console.WriteLine("\n--- stderr ---\n" + stderr);
        }
        Console.WriteLine("\nExit code: " + proc.ExitCode);
        Console.WriteLine("\nPress Enter to exit.");
        Console.ReadLine();
    }
}
