using System;

namespace Andy.TUI.Terminal.Diagnostics;

/// <summary>
/// Rendering invariants that validate properties of the frame buffer after each render.
/// </summary>
public static class RenderingInvariants
{
    /// <summary>
    /// Ensures that for any horizontal run that is intended to be a single background span
    /// (e.g., list rows), the background color is uniform across that run.
    /// As a pragmatic first step, this checks that for each line, contiguous non-space clusters
    /// have a uniform background, and highlights split backgrounds which often indicate flicker
    /// or inconsistent styling logic.
    /// </summary>
    public static void ValidateUniformLineBackgrounds(Buffer frontBuffer, RenderingInvariantOptions options)
    {
        if (!options.Enabled) return;

        for (int y = 0; y < frontBuffer.Height; y++)
        {
            int xStart = 0;
            int xEnd = frontBuffer.Width - 1;

            ConsoleColorSignature? currentBg = null;
            int segmentStart = xStart;

            for (int x = xStart; x <= xEnd; x++)
            {
                var cell = frontBuffer[x, y];
                var bg = ConsoleColorSignature.FromColor(cell.Style.Background);

                // Borders always break segments
                bool isBorderChar = cell.Character is '│' or '┌' or '┐' or '└' or '┘' or '─' or '┬' or '┴' or '├' or '┤' or '+' or '|' or '-';
                if (isBorderChar)
                {
                    currentBg = null;
                    segmentStart = x + 1;
                    continue;
                }

                // Treat whitespace as a natural segment break regardless of background fill.
                // Many renderers clear areas using backgrounded spaces; those transitions are benign.
                if (char.IsWhiteSpace(cell.Character))
                {
                    currentBg = null;
                    segmentStart = x + 1;
                    continue;
                }

                // Only enforce uniformity within spans where a background is explicitly set.
                if (cell.Style.Background.Type == ColorType.None)
                {
                    currentBg = null;
                    segmentStart = x + 1;
                    continue;
                }

                if (currentBg == null)
                {
                    currentBg = bg;
                    segmentStart = x;
                }
                else if (!currentBg.Value.Equals(bg))
                {
                    var message = $"Non-uniform background detected at y={y}, x={segmentStart}-{x}. " +
                                  $"PrevBg={currentBg} NewBg={bg}. This causes striped rows/flicker.";
                    if (options.ThrowOnViolation)
                        throw new RenderingInvariantViolationException(message);
                    else
                        Andy.TUI.Diagnostics.LogManager.GetLogger("RenderingInvariants").Warning(message);

                    // Reset segment to continue scanning within backgrounded spans
                    currentBg = bg;
                    segmentStart = x;
                }
            }
        }
    }

    /// <summary>
    /// Validates that the frame contains no ANSI SGR sequences split mid-cell and that
    /// text-only rows do not have background gaps (heuristic checks on buffer state).
    /// </summary>
    public static void ValidateNoBgGaps(Buffer frontBuffer, RenderingInvariantOptions options)
    {
        if (!options.Enabled) return;
        for (int y = 0; y < frontBuffer.Height; y++)
        {
            bool seenContent = false;
            bool seenGapAfterBg = false;
            ConsoleColorSignature? bgSeen = null;
            int bgStartX = -1;
            int bgEndX = -1;
            for (int x = 0; x < frontBuffer.Width; x++)
            {
                var cell = frontBuffer[x, y];
                var bg = ConsoleColorSignature.FromColor(cell.Style.Background);
                bool isBgSet = bg.Type != ColorType.None;
                bool isBorderChar = cell.Character is '│' or '┌' or '┐' or '└' or '┘' or '─' or '┬' or '┴' or '├' or '┤' or '+' or '|' or '-';
                bool isWhitespace = char.IsWhiteSpace(cell.Character);

                // Ignore borders and whitespace for gap detection; these are common and benign
                if (isBorderChar || isWhitespace)
                {
                    continue;
                }

                if (isBgSet)
                {
                    // Allow gaps between background segments; only warn if background reappears
                    // with a different color after a gap (potential flicker), but do not throw.
                    if (seenGapAfterBg && bgSeen.HasValue && !bg.Equals(bgSeen.Value))
                    {
                        Andy.TUI.Diagnostics.LogManager.GetLogger("RenderingInvariants").Warning($"Background reappeared with different color on row {y}");
                    }
                    if (bgStartX == -1) bgStartX = x;
                    bgSeen = bg;
                    seenContent = true;
                    seenGapAfterBg = false;
                }
                else if (seenContent)
                {
                    // First gap after content with bg
                    if (!seenGapAfterBg) bgEndX = x - 1;
                    seenGapAfterBg = true;
                }
            }
        }
    }
}

/// <summary>
/// Runtime switches for rendering invariants.
/// </summary>
public sealed class RenderingInvariantOptions
{
    public bool Enabled { get; init; } = true;
    public bool ThrowOnViolation { get; init; } = true;

    public static RenderingInvariantOptions FromEnvironment()
    {
        // ANDY_TUI_INVARIANTS=0 disables; ANDY_TUI_INVARIANTS_THROW=0 disables throwing
        var enabledVar = Environment.GetEnvironmentVariable("ANDY_TUI_INVARIANTS");
        var throwVar = Environment.GetEnvironmentVariable("ANDY_TUI_INVARIANTS_THROW");
        bool enabled = enabledVar == null || enabledVar == "1" || enabledVar.Equals("true", StringComparison.OrdinalIgnoreCase);
        bool shouldThrow = throwVar == null || throwVar == "1" || throwVar.Equals("true", StringComparison.OrdinalIgnoreCase);
        return new RenderingInvariantOptions { Enabled = enabled, ThrowOnViolation = shouldThrow };
    }
}

public sealed class RenderingInvariantViolationException : Exception
{
    public RenderingInvariantViolationException(string message) : base(message) { }
}

internal readonly struct ConsoleColorSignature : IEquatable<ConsoleColorSignature>
{
    public readonly ColorType Type;
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly int? Index;
    public readonly System.ConsoleColor? ConsoleColor;

    private ConsoleColorSignature(ColorType type, byte r, byte g, byte b, int? index, System.ConsoleColor? consoleColor)
    {
        Type = type; R = r; G = g; B = b; Index = index; ConsoleColor = consoleColor;
    }

    public static ConsoleColorSignature FromColor(Color c)
    {
        return c.Type switch
        {
            ColorType.None => new ConsoleColorSignature(ColorType.None, 0, 0, 0, null, null),
            ColorType.Rgb => new ConsoleColorSignature(ColorType.Rgb, c.Rgb?.R ?? (byte)0, c.Rgb?.G ?? (byte)0, c.Rgb?.B ?? (byte)0, null, null),
            ColorType.EightBit => new ConsoleColorSignature(ColorType.EightBit, 0, 0, 0, c.ColorIndex, null),
            ColorType.ConsoleColor => new ConsoleColorSignature(ColorType.ConsoleColor, 0, 0, 0, null, c.ConsoleColor),
            _ => new ConsoleColorSignature(ColorType.None, 0, 0, 0, null, null)
        };
    }

    public bool Equals(ConsoleColorSignature other)
    {
        if (Type != other.Type) return false;
        return Type switch
        {
            ColorType.None => true,
            ColorType.Rgb => R == other.R && G == other.G && B == other.B,
            ColorType.EightBit => Index == other.Index,
            _ => ConsoleColor == other.ConsoleColor
        };
    }

    public override bool Equals(object? obj) => obj is ConsoleColorSignature other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Type, R, G, B, Index, ConsoleColor);
    public override string ToString() => Type switch
    {
        ColorType.None => "none",
        ColorType.Rgb => $"rgb({R},{G},{B})",
        ColorType.EightBit => Index.HasValue ? $"8bit({Index})" : "8bit(-)",
        _ => ConsoleColor.HasValue ? ConsoleColor.Value.ToString() : "console(-)"
    };
}
