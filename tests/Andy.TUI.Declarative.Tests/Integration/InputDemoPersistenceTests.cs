using System;
using System.Linq;
using System.Threading;
using Xunit;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class InputDemoPersistenceTests
{
    private static string BufferToString(Andy.TUI.Terminal.Buffer buffer)
    {
        var width = buffer.Width;
        var height = buffer.Height;
        var chars = new char[width * height + height];
        var idx = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                chars[idx++] = buffer[x, y].Character;
            }
            chars[idx++] = '\n';
        }
        return new string(chars);
    }

    private static (RenderingSystem rs, DeclarativeRenderer renderer, TestInputHandler input, MockTerminal term) Setup()
    {
        var terminal = new MockTerminal(120, 40);
        var rs = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(rs, input);
        rs.Initialize();
        return (rs, renderer, input, terminal);
    }

    private static ISimpleComponent CreateUI(Func<string> getName, Action<string> setName,
                                             Func<string> getPass, Action<string> setPass,
                                             Func<string> getCountry, Action<string> setCountry)
    {
        var countries = new[] { "United States", "Canada", "United Kingdom", "Germany", "France" };
        return new VStack(spacing: 1) {
            new Text("🚀 Andy.TUI Input Components Demo").Title().Color(Color.Cyan),
            " ",
            new HStack(spacing: 2) {
                new Text("  Name:").Bold().Color(Color.White),
                new TextField("Enter your name...", new Andy.TUI.Declarative.State.Binding<string>(getName, setName))
            },
            new HStack(spacing: 2) {
                new Text("  Pass:").Bold().Color(Color.White),
                new TextField("Enter password...", new Andy.TUI.Declarative.State.Binding<string>(getPass, setPass)).Secure()
            },
            new HStack(spacing: 2) {
                new Text("Country:").Bold().Color(Color.White),
                new Dropdown<string>("Select a country...", countries, new Andy.TUI.Declarative.State.Binding<string>(getCountry, setCountry))
                    .Color(Color.White)
                    .PlaceholderColor(Color.Gray)
            },
            " ",
            new HStack(spacing: 3) {
                new Button("Submit", () => { }).Primary(),
                new Button("Cancel", () => { }).Secondary()
            },
            " ",
            new Text("Use [Tab] to navigate between fields, [Enter] to submit").Color(Color.Yellow),
            new Text("Press Ctrl+C to exit").Color(Color.DarkGray)
        };
    }

    [Fact]
    public void Buttons_ShouldPersist_DuringTabNavigation()
    {
        string name = string.Empty, pass = string.Empty, country = string.Empty;
        var (rs, renderer, input, term) = Setup();

        ISimpleComponent Root() => CreateUI(() => name, v => name = v, () => pass, v => pass = v, () => country, v => country = v);
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        Thread.Sleep(200);
        string buf0 = BufferToString(rs.Buffer.GetFrontBuffer());
        Assert.Contains("Submit", buf0);
        Assert.Contains("Cancel", buf0);

        for (int i = 0; i < 6; i++)
        {
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(60);
            var buf = BufferToString(rs.Buffer.GetFrontBuffer());
            Assert.Contains("Submit", buf);
            Assert.Contains("Cancel", buf);
        }

        rs.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void StatusTexts_ShouldPersist_DuringTabNavigation()
    {
        string name = string.Empty, pass = string.Empty, country = string.Empty;
        var (rs, renderer, input, term) = Setup();

        ISimpleComponent Root() => CreateUI(() => name, v => name = v, () => pass, v => pass = v, () => country, v => country = v);
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        Thread.Sleep(200);
        string buf0 = BufferToString(rs.Buffer.GetFrontBuffer());
        Assert.Contains("Use [Tab] to navigate", buf0);
        Assert.Contains("Press Ctrl+C to exit", buf0);

        for (int i = 0; i < 6; i++)
        {
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(60);
            var buf = BufferToString(rs.Buffer.GetFrontBuffer());
            Assert.Contains("Use [Tab] to navigate", buf);
            Assert.Contains("Press Ctrl+C to exit", buf);
        }

        rs.Shutdown();
        thread.Join(100);
    }
}
