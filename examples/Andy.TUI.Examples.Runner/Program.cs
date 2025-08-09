using System.Diagnostics;
using System.Text;

static class ExampleRunner
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static readonly string ExamplesRoot = Path.Combine(RepoRoot, "examples");

    public static int Run(string[] args)
    {
        var exampleProjects = FindExampleProjects();
        if (exampleProjects.Count == 0)
        {
            Console.WriteLine("No example projects found under 'examples/'.");
            return 1;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Andy.TUI Examples");
            Console.WriteLine(new string('=', 24));
            for (int i = 0; i < exampleProjects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {exampleProjects[i].DisplayName}");
            }
            Console.WriteLine("0. Exit");
            Console.Write("Select an example to run: ");

            var input = Console.ReadLine();
            if (!int.TryParse(input, out int choice))
            {
                continue;
            }

            if (choice == 0)
            {
                return 0;
            }

            if (choice < 1 || choice > exampleProjects.Count)
            {
                continue;
            }

            var project = exampleProjects[choice - 1];
            Console.Clear();
            Console.WriteLine($"Running: {project.DisplayName}\n");

            int exit = RunDotnetProject(project.ProjectPath);

            Console.WriteLine($"\nProcess exited with code {exit}. Press Enter to return to menu...");
            Console.ReadLine();
        }
    }

    private static int RunDotnetProject(string projectPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList = { "run", "--project", projectPath },
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            return -1;
        }
        process.WaitForExit();
        return process.ExitCode;
    }

    private static List<ProjectInfo> FindExampleProjects()
    {
        var projects = Directory.EnumerateFiles(ExamplesRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !Path.GetFileName(p).Equals("Andy.TUI.Examples.Runner.csproj", StringComparison.OrdinalIgnoreCase))
            .Select(p => new ProjectInfo(p))
            .OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return projects;
    }

    private sealed class ProjectInfo
    {
        public string ProjectPath { get; }
        public string DisplayName { get; }

        public ProjectInfo(string projectPath)
        {
            ProjectPath = projectPath;
            DisplayName = MakeDisplayName(projectPath);
        }

        private static string MakeDisplayName(string projectPath)
        {
            var dir = Path.GetFileName(Path.GetDirectoryName(projectPath)) ?? projectPath;
            return dir;
        }
    }
}

internal static class Program
{
    public static int Main(string[] args) => ExampleRunner.Run(args);
}
