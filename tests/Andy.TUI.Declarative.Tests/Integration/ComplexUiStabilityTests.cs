using System;
using System.Linq;
using System.Threading;
using Xunit;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Tests.Integration;

public class ComplexUiStabilityTests
{
    private static string Buf(Andy.TUI.Terminal.Buffer b)
    {
        var sb = new System.Text.StringBuilder();
        for (int y = 0; y < b.Height; y++)
        {
            for (int x = 0; x < b.Width; x++) sb.Append(b[x, y].Character);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static (RenderingSystem rs, DeclarativeRenderer renderer, TestInputHandler input, MockTerminal term)
        Setup(int w = 120, int h = 40)
    {
        var term = new MockTerminal(w, h);
        var rs = new RenderingSystem(term);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(rs, input);
        rs.Initialize();
        return (rs, renderer, input, term);
    }

    [Fact]
    public void Example1_FooterTexts_ShouldPersist_WhileCyclingFocus()
    {
        string name = "", pass = "", country = "";
        ISimpleComponent UI() => new VStack(spacing: 1) {
            new Text("🚀 Andy.TUI Input Components Demo").Title().Color(Color.Cyan),
            " ",
            new HStack(spacing: 2) { new Text("  Name:").Bold().Color(Color.White), new TextField("Enter your name...", new Binding<string>(() => name, v => name = v)) },
            new HStack(spacing: 2) { new Text("  Pass:").Bold().Color(Color.White), new TextField("Enter password...", new Binding<string>(() => pass, v => pass = v)).Secure() },
            new HStack(spacing: 2) { new Text("Country:").Bold().Color(Color.White), new Dropdown<string>("Select a country...", new[]{"United States","Canada","United Kingdom"}, new Binding<string>(() => country, v => country = v)).Color(Color.White).PlaceholderColor(Color.Gray) },
            " ",
            new HStack(spacing: 3) { new Button("Submit", () => { }).Primary(), new Button("Cancel", () => { }).Secondary() },
            " ",
            new Text("Use [Tab] to navigate between fields, [Enter] to submit").Color(Color.Yellow),
            new Text("Press Ctrl+C to exit").Color(Color.DarkGray)
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(200);

        // Initial assertions
        var buf0 = Buf(rs.Buffer.GetFrontBuffer());
        Assert.Contains("Submit", buf0);
        Assert.Contains("Cancel", buf0);
        Assert.Contains("Use [Tab] to navigate", buf0);
        Assert.Contains("Press Ctrl+C to exit", buf0);

        // Cycle focus many times
        for (int i = 0; i < 12; i++)
        {
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            var buf = Buf(rs.Buffer.GetFrontBuffer());
            Assert.Contains("Submit", buf);
            Assert.Contains("Cancel", buf);
            Assert.Contains("Use [Tab] to navigate", buf);
            Assert.Contains("Press Ctrl+C to exit", buf);
        }

        rs.Shutdown();
        th.Join(100);
    }

    [Fact]
    public void Dropdown_Backdrop_ShouldDisappear_AfterSelection_AndNotOccludeButtons()
    {
        string selected = "";
        var items = new[] { "United States", "Canada", "United Kingdom" };
        ISimpleComponent UI() => new VStack(spacing: 1) {
            new Text("Title"),
            new Dropdown<string>("Select a country...", items, new Binding<string>(() => selected, v => selected = v)).Color(Color.White),
            new HStack(spacing: 2) { new Button("Submit", () => { }).Primary(), new Button("Cancel", () => { }).Secondary() },
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(150);

        // Focus dropdown and open it (Space)
        input.EmitKey('\t', ConsoleKey.Tab); // focus dropdown
        Thread.Sleep(30);
        input.EmitKey(' ', ConsoleKey.Spacebar);
        Thread.Sleep(60);

        // Move highlight and select with Enter
        input.EmitKey('\0', ConsoleKey.DownArrow);
        Thread.Sleep(40);
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(80);

        var buf = Buf(rs.Buffer.GetFrontBuffer());
        // Backdrop must not be visible; buttons must be readable
        Assert.Contains("Submit", buf);
        Assert.Contains("Cancel", buf);
        Assert.DoesNotContain("  United States", buf); // menu lines should be gone
        Assert.DoesNotContain("  Canada", buf);
        Assert.DoesNotContain("  United Kingdom", buf);
    }

    [Fact]
    public void Dropdown_ToggleManyTimes_ShouldNotEraseButtons()
    {
        string selected = "";
        var items = new[] { "A", "B", "C", "D" };
        ISimpleComponent UI() => new VStack(spacing: 1) {
            new Text("Title"),
            new Dropdown<string>("Pick...", items, new Binding<string>(() => selected, v => selected = v)).Color(Color.White),
            new HStack(spacing: 2) { new Button("Submit", () => { }).Primary(), new Button("Cancel", () => { }).Secondary() },
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(150);

        input.EmitKey('\t', ConsoleKey.Tab); // focus dropdown
        Thread.Sleep(20);

        for (int i = 0; i < 6; i++)
        {
            input.EmitKey(' ', ConsoleKey.Spacebar); // open
            Thread.Sleep(40);
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(20);
            input.EmitKey('\r', ConsoleKey.Enter); // select
            Thread.Sleep(60);

            var buf = Buf(rs.Buffer.GetFrontBuffer());
            Assert.Contains("Submit", buf);
            Assert.Contains("Cancel", buf);
            Assert.DoesNotContain("  A", buf);
            Assert.DoesNotContain("  B", buf);
            Assert.DoesNotContain("  C", buf);
            Assert.DoesNotContain("  D", buf);
        }

        rs.Shutdown();
        th.Join(100);
    }

    [Fact]
    public void TextArea_Borders_ShouldPersist_WhileTypingAndCycling()
    {
        string val = "";
        ISimpleComponent UI() => new VStack(spacing: 1) {
            new Text("TextArea Demo"),
            new TextArea("Enter multi-line...", new Binding<string>(() => val, v => val = v), rows: 4, cols: 20),
            new Button("Next", () => { })
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(150);

        // Focus textarea
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(30);

        // Type characters and newlines
        foreach (var ch in "Hello\nWorld")
        {
            var key = char.IsLetter(ch) ? (ConsoleKey)Enum.Parse(typeof(ConsoleKey), ch.ToString().ToUpper()) : ConsoleKey.Enter;
            input.EmitKey(ch, key);
            Thread.Sleep(10);
        }

        // Cycle focus several times
        for (int i = 0; i < 5; i++)
        {
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(30);
            var buf = Buf(rs.Buffer.GetFrontBuffer());
            Assert.Contains("┌", buf);
            Assert.Contains("┐", buf);
            Assert.Contains("└", buf);
            Assert.Contains("┘", buf);
        }

        rs.Shutdown();
        th.Join(100);
    }

    [Fact]
    public void Overlapping_Texts_WithUpdates_ShouldNotLeaveArtifacts()
    {
        string a = "Left", b = "Right";
        var posB = 15;
        ISimpleComponent UI() => new VStack(spacing: 0) {
            new Text(a),
            // Position b by inserting enough leading spaces
            new Text(new string(' ', posB) + b)
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(120);

        // Expand A and shift B
        a = "Left_Expanded"; posB = 25;
        renderer.RequestRender();
        Thread.Sleep(120);

        // Shrink A and shift B back
        a = "L"; posB = 5;
        renderer.RequestRender();
        Thread.Sleep(120);

        var line0 = term.GetLine(0);
        Assert.Contains("L", line0);
        // Ensure no trailing leftovers from previous longer strings at earlier positions
        Assert.DoesNotContain("Expanded", line0);
        Assert.True(line0.IndexOf('L') <= line0.IndexOf('R'));

        rs.Shutdown();
        th.Join(100);
    }

    [Fact]
    public void PasswordField_ShouldNotDisappear_WhenCyclingAndTyping()
    {
        string name = "", pass = "";
        ISimpleComponent UI() => new VStack(spacing: 1) {
            new Text("Form"),
            new TextField("Name", new Binding<string>(() => name, v => name = v)),
            new TextField("Pass", new Binding<string>(() => pass, v => pass = v)).Secure(),
            new Button("Submit", () => { })
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(150);

        // Focus name, then password, type a character, cycle a few times
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(30);
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(30);
        input.EmitKey('x', ConsoleKey.X);
        Thread.Sleep(40);

        for (int i = 0; i < 6; i++)
        {
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(40);
            var buf = Buf(rs.Buffer.GetFrontBuffer());
            // Expect masked dot at least somewhere and the brackets
            Assert.Contains("•", buf);
        }

        rs.Shutdown();
        th.Join(100);
    }

    [Fact]
    public void ModalOverlay_ShouldNotPermanentlyOcclude_UnderlyingUi()
    {
        bool isOpen = false;
        string state = "";
        ISimpleComponent UI() => new ZStack {
            new VStack(spacing: 1) {
                new Text("Main UI"),
                new TextField("Enter", new Binding<string>(() => state, v => state = v)),
                new Button("Open", () => isOpen = true)
            },
            Dialog.Alert("Alert", "Hello!", new Binding<bool>(() => isOpen, v => isOpen = v))
        };

        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(UI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(150);

        // Open modal
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(30);
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(30);
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(80);

        // Close modal (Enter on OK)
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(120);

        var buf = Buf(rs.Buffer.GetFrontBuffer());
        Assert.Contains("Main UI", buf);
        Assert.DoesNotContain("Hello!", buf);
    }
}
