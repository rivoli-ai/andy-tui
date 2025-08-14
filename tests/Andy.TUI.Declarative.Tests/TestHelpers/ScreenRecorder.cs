using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

/// <summary>
/// Records screen frames and debug logs for test analysis.
/// </summary>
public class ScreenRecorder
{
    private readonly List<ScreenFrame> _frames = new();
    private readonly List<LogEntry> _logs = new();
    private readonly int _width;
    private readonly int _height;
    private readonly string _testName;
    private int _frameNumber = 0;

    public ScreenRecorder(string testName, int width, int height)
    {
        _testName = testName;
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Records a frame from the terminal buffer.
    /// </summary>
    public void RecordFrame(MockTerminal terminal, string action, Dictionary<string, object>? metadata = null)
    {
        var frame = new ScreenFrame
        {
            FrameNumber = _frameNumber++,
            Timestamp = DateTime.UtcNow,
            Action = action,
            Width = _width,
            Height = _height,
            Content = CaptureScreen(terminal),
            Metadata = metadata ?? new Dictionary<string, object>()
        };

        _frames.Add(frame);
    }

    /// <summary>
    /// Records a debug log entry.
    /// </summary>
    public void RecordLog(string level, string message, Dictionary<string, object>? context = null)
    {
        _logs.Add(new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Context = context ?? new Dictionary<string, object>(),
            FrameNumber = _frameNumber - 1 // Associate with last frame
        });
    }

    /// <summary>
    /// Captures the current screen content with character-level detail including colors.
    /// </summary>
    private ScreenContent CaptureScreen(MockTerminal terminal)
    {
        var lines = new List<ScreenLine>();
        
        for (int y = 0; y < _height; y++)
        {
            var line = terminal.GetLine(y);
            var chars = new List<CharInfo>();
            
            // Capture full width to get color info even for spaces
            for (int x = 0; x < _width; x++)
            {
                var (character, foreground, background) = terminal.GetCharAt(x, y);
                chars.Add(new CharInfo
                {
                    Char = character,
                    X = x,
                    Y = y,
                    ForegroundColor = foreground.ToString(),
                    BackgroundColor = background.ToString()
                });
            }
            
            lines.Add(new ScreenLine
            {
                Y = y,
                Text = line.TrimEnd(),
                Characters = chars,
                // Track if this line has any colored backgrounds (highlights)
                HasHighlight = chars.Any(c => c.BackgroundColor != "Black" && c.BackgroundColor != "DarkGray")
            });
        }

        return new ScreenContent
        {
            Lines = lines,
            FullText = string.Join("\n", lines.Select(l => l.Text))
        };
    }

    /// <summary>
    /// Analyzes the recording for issues.
    /// </summary>
    public AnalysisReport Analyze()
    {
        var issues = new List<RenderingIssue>();
        
        // Analyze frame transitions
        for (int i = 1; i < _frames.Count; i++)
        {
            var prev = _frames[i - 1];
            var curr = _frames[i];
            
            // Check for disappearing text
            var prevNonEmpty = prev.Content.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Text)).ToList();
            var currNonEmpty = curr.Content.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Text)).ToList();
            
            if (prevNonEmpty.Count > currNonEmpty.Count + 2) // Allow some clearing
            {
                issues.Add(new RenderingIssue
                {
                    Type = "TextDisappeared",
                    FrameNumber = curr.FrameNumber,
                    Description = $"Text disappeared after action '{curr.Action}'. Lines went from {prevNonEmpty.Count} to {currNonEmpty.Count}",
                    Severity = "High"
                });
            }
            
            // Check for corruption (null chars, weird symbols)
            foreach (var line in curr.Content.Lines)
            {
                if (line.Text.Contains('\0'))
                {
                    issues.Add(new RenderingIssue
                    {
                        Type = "NullCharacter",
                        FrameNumber = curr.FrameNumber,
                        Description = $"Null character found at line {line.Y}",
                        Severity = "High"
                    });
                }
                
                if (line.Text.Contains('�'))
                {
                    issues.Add(new RenderingIssue
                    {
                        Type = "CorruptedCharacter",
                        FrameNumber = curr.FrameNumber,
                        Description = $"Corrupted character '�' found at line {line.Y}",
                        Severity = "High"
                    });
                }
            }
            
            // Check for multiple highlights in dropdown
            var highlightLines = curr.Content.Lines.Where(l => l.Text.Contains("▶")).ToList();
            if (highlightLines.Count > 1)
            {
                issues.Add(new RenderingIssue
                {
                    Type = "MultipleHighlights",
                    FrameNumber = curr.FrameNumber,
                    Description = $"Multiple dropdown highlights found: {highlightLines.Count} lines with '▶'",
                    Severity = "High",
                    Details = highlightLines.Select(l => $"Line {l.Y}: {l.Text}").ToList()
                });
            }
            
            // Check for partial backgrounds (looking for inconsistent patterns)
            foreach (var line in curr.Content.Lines)
            {
                if (line.Text.Contains("│") && line.Text.Contains("▶"))
                {
                    // Check if the line has consistent formatting
                    var parts = line.Text.Split('│');
                    if (parts.Length >= 2)
                    {
                        var content = parts[1];
                        // Look for mixed content that might indicate rendering issues
                        if (content.Count(c => c == '▶') > 1)
                        {
                            issues.Add(new RenderingIssue
                            {
                                Type = "MixedContent",
                                FrameNumber = curr.FrameNumber,
                                Description = $"Line {line.Y} has mixed content indicators",
                                Severity = "Medium"
                            });
                        }
                    }
                }
            }
            
            // Check for overlapping elements
            CheckForOverlaps(curr, issues);
            
            // Check for color rendering issues
            CheckForColorIssues(curr, issues);
        }
        
        return new AnalysisReport
        {
            TestName = _testName,
            TotalFrames = _frames.Count,
            TotalLogs = _logs.Count,
            Issues = issues,
            Frames = _frames,
            Logs = _logs
        };
    }

    private void CheckForOverlaps(ScreenFrame frame, List<RenderingIssue> issues)
    {
        // Look for patterns that indicate overlapping rendering
        foreach (var line in frame.Content.Lines)
        {
            // Check for dropdown borders appearing in wrong places
            if (line.Text.Contains("┌") || line.Text.Contains("└"))
            {
                // Check if there's text before or after that shouldn't be there
                var borderIndex = line.Text.IndexOfAny(new[] { '┌', '└' });
                if (borderIndex > 0)
                {
                    var before = line.Text.Substring(0, borderIndex).Trim();
                    if (!string.IsNullOrEmpty(before) && !before.All(c => c == ' '))
                    {
                        issues.Add(new RenderingIssue
                        {
                            Type = "OverlappingBorder",
                            FrameNumber = frame.FrameNumber,
                            Description = $"Border character overlaps with text at line {line.Y}",
                            Severity = "Medium",
                            Details = new List<string> { $"Line content: [{line.Text}]" }
                        });
                    }
                }
            }
        }
    }

    private void CheckForColorIssues(ScreenFrame frame, List<RenderingIssue> issues)
    {
        // Check for multiple highlighted lines (should only be one)
        var highlightedLines = frame.Content.Lines.Where(l => l.HasHighlight).ToList();
        if (highlightedLines.Count > 1)
        {
            issues.Add(new RenderingIssue
            {
                Type = "MultipleHighlightedLines",
                FrameNumber = frame.FrameNumber,
                Description = $"Multiple lines have highlight backgrounds: {highlightedLines.Count} lines",
                Severity = "High",
                Details = highlightedLines.Select(l => $"Line {l.Y}: {l.Text}").ToList()
            });
        }

        // Check for partial highlights (background not covering full line)
        foreach (var line in frame.Content.Lines)
        {
            if (line.HasHighlight && line.Characters.Count > 0)
            {
                // Check if the highlight is consistent across the line
                var highlightedChars = line.Characters.Where(c => 
                    c.BackgroundColor != "Black" && c.BackgroundColor != "DarkGray").ToList();
                
                if (highlightedChars.Count > 0)
                {
                    // Find gaps in highlighting
                    var firstHighlight = highlightedChars.First().X;
                    var lastHighlight = highlightedChars.Last().X;
                    var expectedCount = lastHighlight - firstHighlight + 1;
                    
                    if (highlightedChars.Count < expectedCount)
                    {
                        issues.Add(new RenderingIssue
                        {
                            Type = "PartialHighlight",
                            FrameNumber = frame.FrameNumber,
                            Description = $"Line {line.Y} has partial background highlighting",
                            Severity = "High",
                            Details = new List<string> 
                            { 
                                $"Highlighted chars: {highlightedChars.Count}, Expected: {expectedCount}",
                                $"Line text: [{line.Text}]"
                            }
                        });
                    }
                    
                    // Check for mixed background colors on the same line
                    var bgColors = highlightedChars.Select(c => c.BackgroundColor).Distinct().ToList();
                    if (bgColors.Count > 1)
                    {
                        issues.Add(new RenderingIssue
                        {
                            Type = "MixedBackgroundColors",
                            FrameNumber = frame.FrameNumber,
                            Description = $"Line {line.Y} has mixed background colors",
                            Severity = "High",
                            Details = new List<string> 
                            { 
                                $"Colors found: {string.Join(", ", bgColors)}",
                                $"Line text: [{line.Text}]"
                            }
                        });
                    }
                }
            }
        }

        // Check for incorrect color combinations (e.g., black on black)
        foreach (var line in frame.Content.Lines)
        {
            foreach (var ch in line.Characters)
            {
                if (ch.Char != ' ' && ch.Char != '\0')
                {
                    // Check for invisible text (same foreground and background)
                    if (ch.ForegroundColor == ch.BackgroundColor)
                    {
                        issues.Add(new RenderingIssue
                        {
                            Type = "InvisibleText",
                            FrameNumber = frame.FrameNumber,
                            Description = $"Character at ({ch.X},{ch.Y}) has same foreground and background color",
                            Severity = "High",
                            Details = new List<string> 
                            { 
                                $"Character: '{ch.Char}'",
                                $"Color: {ch.ForegroundColor}"
                            }
                        });
                        break; // Only report once per line
                    }
                    
                    // Check for poor contrast
                    if (IsPoorContrast(ch.ForegroundColor, ch.BackgroundColor))
                    {
                        issues.Add(new RenderingIssue
                        {
                            Type = "PoorContrast",
                            FrameNumber = frame.FrameNumber,
                            Description = $"Poor color contrast at line {line.Y}",
                            Severity = "Medium",
                            Details = new List<string> 
                            { 
                                $"Foreground: {ch.ForegroundColor}, Background: {ch.BackgroundColor}",
                                $"Character: '{ch.Char}' at position {ch.X}"
                            }
                        });
                        break; // Only report once per line
                    }
                }
            }
        }
    }

    private bool IsPoorContrast(string foreground, string background)
    {
        // Simple check for known bad combinations
        var badCombos = new[]
        {
            ("DarkGray", "Black"),
            ("Gray", "DarkGray"),
            ("Yellow", "White"),
            ("Cyan", "White"),
            ("DarkBlue", "Black"),
            ("DarkGreen", "Black")
        };
        
        return badCombos.Any(combo => 
            (combo.Item1 == foreground && combo.Item2 == background) ||
            (combo.Item2 == foreground && combo.Item1 == background));
    }

    /// <summary>
    /// Saves the recording to a file for analysis.
    /// </summary>
    public string SaveToFile(string? directory = null)
    {
        directory ??= Path.GetTempPath();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var filename = Path.Combine(directory, $"screen_recording_{_testName}_{timestamp}.json");
        
        var report = Analyze();
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        
        File.WriteAllText(filename, json);
        
        // Also create a human-readable summary
        var summaryFile = Path.ChangeExtension(filename, ".txt");
        WriteSummary(summaryFile, report);
        
        return filename;
    }

    private void WriteSummary(string filename, AnalysisReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Screen Recording Analysis: {_testName}");
        sb.AppendLine(new string('=', 60));
        sb.AppendLine($"Total Frames: {report.TotalFrames}");
        sb.AppendLine($"Total Logs: {report.TotalLogs}");
        sb.AppendLine($"Issues Found: {report.Issues.Count}");
        sb.AppendLine();
        
        if (report.Issues.Any())
        {
            sb.AppendLine("ISSUES:");
            sb.AppendLine("-------");
            foreach (var issue in report.Issues.OrderBy(i => i.FrameNumber))
            {
                sb.AppendLine($"Frame {issue.FrameNumber}: [{issue.Severity}] {issue.Type}");
                sb.AppendLine($"  {issue.Description}");
                if (issue.Details?.Any() == true)
                {
                    foreach (var detail in issue.Details)
                    {
                        sb.AppendLine($"    - {detail}");
                    }
                }
                sb.AppendLine();
            }
        }
        
        sb.AppendLine("\nFRAME SEQUENCE:");
        sb.AppendLine("---------------");
        foreach (var frame in report.Frames)
        {
            sb.AppendLine($"Frame {frame.FrameNumber}: {frame.Action}");
            sb.AppendLine($"  Dimensions: {frame.Width}x{frame.Height}");
            
            // Show ALL non-empty lines for complete capture
            var nonEmptyLines = frame.Content.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.Text))
                .ToList();
            
            sb.AppendLine($"  Content ({nonEmptyLines.Count} lines):");
            foreach (var line in nonEmptyLines)
            {
                // Show color info for each character segment with different colors
                var lineDesc = $"  [{line.Y:D2}]: {line.Text}";
                
                if (line.HasHighlight)
                {
                    lineDesc += " [HIGHLIGHTED]";
                    
                    // Analyze which characters have what colors
                    var colorSegments = new List<string>();
                    var chars = line.Characters.Where(c => c.X < line.Text.Length).ToList();
                    
                    if (chars.Any())
                    {
                        var bgGroups = chars.GroupBy(c => c.BackgroundColor);
                        foreach (var group in bgGroups.Where(g => g.Key != "Black"))
                        {
                            var positions = string.Join(",", group.Select(c => c.X));
                            colorSegments.Add($"bg:{group.Key}@[{positions}]");
                        }
                        
                        if (colorSegments.Any())
                        {
                            lineDesc += $" Colors: {string.Join("; ", colorSegments)}";
                        }
                    }
                }
                
                sb.AppendLine(lineDesc);
            }
            
            // Highlight color issues in this frame
            var frameColorIssues = report.Issues
                .Where(i => i.FrameNumber == frame.FrameNumber && 
                       (i.Type.Contains("Highlight") || i.Type.Contains("Color") || i.Type.Contains("Contrast")))
                .ToList();
                
            if (frameColorIssues.Any())
            {
                sb.AppendLine("  ⚠️ Color Issues in this frame:");
                foreach (var issue in frameColorIssues)
                {
                    sb.AppendLine($"    - {issue.Type}: {issue.Description}");
                }
            }
            
            sb.AppendLine();
        }
        
        File.WriteAllText(filename, sb.ToString());
    }
}

// Data structures for recording
public class ScreenFrame
{
    public int FrameNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public ScreenContent Content { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ScreenContent
{
    public List<ScreenLine> Lines { get; set; } = new();
    public string FullText { get; set; } = "";
}

public class ScreenLine
{
    public int Y { get; set; }
    public string Text { get; set; } = "";
    public List<CharInfo> Characters { get; set; } = new();
    public bool HasHighlight { get; set; }
}

public class CharInfo
{
    public char Char { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string ForegroundColor { get; set; } = "";
    public string BackgroundColor { get; set; } = "";
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "";
    public string Message { get; set; } = "";
    public Dictionary<string, object> Context { get; set; } = new();
    public int FrameNumber { get; set; }
}

public class RenderingIssue
{
    public string Type { get; set; } = "";
    public int FrameNumber { get; set; }
    public string Description { get; set; } = "";
    public string Severity { get; set; } = "";
    public List<string>? Details { get; set; }
}

public class AnalysisReport
{
    public string TestName { get; set; } = "";
    public int TotalFrames { get; set; }
    public int TotalLogs { get; set; }
    public List<RenderingIssue> Issues { get; set; } = new();
    public List<ScreenFrame> Frames { get; set; } = new();
    public List<LogEntry> Logs { get; set; } = new();
}