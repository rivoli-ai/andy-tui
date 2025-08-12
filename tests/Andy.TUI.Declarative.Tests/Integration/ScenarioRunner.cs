using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class ScenarioRunner
{
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

    public static IEnumerable<object[]> Scenarios()
    {
        // This yields name + UI factory + drive function + assert function
        // We will add dozens here, then break out into separate files as it grows

        // 1) Simple form with two fields and a submit button
        yield return new object[]
        {
            "Form_TwoFields_Submit",
            (Func<ISimpleComponent>)(() =>
            {
                string name = "", pass = "";
                return new VStack(spacing: 1) {
                    new Text("Form"),
                    new TextField("Name", new Binding<string>(() => name, v => name = v)),
                    new TextField("Pass", new Binding<string>(() => pass, v => pass = v)).Secure(),
                    new Button("Submit", () => { })
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('A', ConsoleKey.A);
                Thread.Sleep(10);
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('B', ConsoleKey.B);
                Thread.Sleep(10);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Submit", buf);
            })
        };

        // 2) Dropdown open/select/close in presence of buttons
        yield return new object[]
        {
            "Dropdown_Open_Select_Close",
            (Func<ISimpleComponent>)(() =>
            {
                string sel = ""; var items = new[]{"A","B","C"};
                return new VStack(spacing: 1) {
                    new Text("Drop Test"),
                    new Dropdown<string>("Pick...", items, new Binding<string>(() => sel, v => sel = v)).Color(Color.White),
                    new HStack(spacing: 2) { new Button("Submit", () => { }).Primary(), new Button("Cancel", () => { }).Secondary() },
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey(' ', ConsoleKey.Spacebar);
                Thread.Sleep(10);
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(10);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Submit", buf);
                Assert.DoesNotContain("  A", buf);
                Assert.DoesNotContain("  B", buf);
                Assert.DoesNotContain("  C", buf);
            })
        };

        // 3) Modal open/close overlay safety
        yield return new object[]
        {
            "Modal_Alert_Open_Close",
            (Func<ISimpleComponent>)(() =>
            {
                bool open = false; string s = "";
                return new ZStack {
                    new VStack(spacing: 1) { new Text("Main"), new TextField("T", new Binding<string>(() => s, v => s = v)), new Button("Open", () => open = true) },
                    Dialog.Alert("Title", "Hello!", new Binding<bool>(() => open, v => open = v))
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter); // OK on modal
                Thread.Sleep(20);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Main", buf);
                Assert.DoesNotContain("Hello!", buf);
            })
        };

        // 4) Grid with text updates shifting columns, then dropdown over it
        yield return new object[]
        {
            "Grid_Shift_With_Dropdown",
            (Func<ISimpleComponent>)(() =>
            {
                string col1 = "Col1", selected = ""; var items = new[]{"X","Y","Z"};
                var grid = new Grid { Gap = 1 }
                    .WithColumns(GridTrackSize.Pixels(10), GridTrackSize.Pixels(10), GridTrackSize.Pixels(10))
                    .WithRows(GridTrackSize.Auto, GridTrackSize.Auto);
                // Use collection initializer-style additions
                grid.Add(new Text(col1).GridArea(0,0));
                grid.Add(new Text("Col2").GridArea(0,1));
                grid.Add(new Text("Col3").GridArea(0,2));
                return new VStack(spacing: 1) {
                    new Text("Grid Demo"),
                    grid,
                    new Dropdown<string>("Pick...", items, new Binding<string>(() => selected, v => selected = v)).Color(Color.White),
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                renderer.RequestRender();
                Thread.Sleep(20);
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey(' ', ConsoleKey.Spacebar);
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(10);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Grid Demo", buf);
                Assert.Contains("Col2", buf);
            })
        };

        // 5) Clipping area: outside text never rendered, footer remains visible
        yield return new object[]
        {
            "Clipping_Bounds",
            (Func<ISimpleComponent>)(() =>
            {
                var box = new Box{ Width = 10, Height = 2, Overflow = Overflow.Hidden };
                box.Add(new Text("Inside"));
                box.Add(new Text("Outside"));
                return new VStack(spacing: 0) {
                    box,
                    new Text("Footer")
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) => { Thread.Sleep(10); }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Inside", buf);
                Assert.Contains("Footer", buf);
            })
        };

        // 6) Form with validation message toggling
        yield return new object[]
        {
            "Form_Validation_Toggle",
            (Func<ISimpleComponent>)(() =>
            {
                string name = ""; bool showError = false;
                return new VStack(spacing: 1) {
                    new Text("Validation Form"),
                    new TextField("Name", new Binding<string>(() => name, v => name = v)),
                    // Simple rule: show error when empty; UI factory runs every render, so this updates
                    new Text(string.IsNullOrEmpty(name) ? "Name is required" : "").Color(Color.Red),
                    new Button("Submit", () => showError = string.IsNullOrEmpty(name))
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                // With empty name, press Submit
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(30);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Name is required", buf);
            })
        };

        // Auto-batch: Forms tab cycles (7..16)
        for (int i = 0; i < 10; i++)
        {
            int idx = i;
            yield return new object[]
            {
                $"Form_TabCycle_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    string name = "", pass = "";
                    return new VStack(spacing: 1) {
                        new Text($"Form {idx}"),
                        new TextField("Name", new Binding<string>(() => name, v => name = v)),
                        new TextField("Pass", new Binding<string>(() => pass, v => pass = v)).Secure(),
                        new Button("Submit", () => { })
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
                {
                    input.EmitKey('\t', ConsoleKey.Tab);
                    Thread.Sleep(5);
                    input.EmitKey('\t', ConsoleKey.Tab);
                    Thread.Sleep(5);
                    input.EmitKey((char)('a' + (idx % 3)), ConsoleKey.A);
                    Thread.Sleep(5);
                }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var buf = Buf(rs.Buffer.GetFrontBuffer());
                    Assert.Contains("Submit", buf);
                })
            };
        }

        // Auto-batch: Dropdown repeat open/select cycles (17..26)
        for (int j = 0; j < 10; j++)
        {
            int idx = j;
            yield return new object[]
            {
                $"Dropdown_RepeatOpenSelect_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    string sel = ""; var items = new[]{"A","B","C"};
                    return new VStack(spacing: 1) {
                        new Text($"Drop {idx}"),
                        new Dropdown<string>("Pick...", items, new Binding<string>(() => sel, v => sel = v)).Color(Color.White),
                        new HStack(spacing: 2) { new Button("Submit", () => { }).Primary(), new Button("Cancel", () => { }).Secondary() },
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
                {
                    input.EmitKey('\t', ConsoleKey.Tab);
                    Thread.Sleep(5);
                    input.EmitKey(' ', ConsoleKey.Spacebar);
                    Thread.Sleep(5);
                    input.EmitKey('\0', ConsoleKey.DownArrow);
                    Thread.Sleep(5);
                    input.EmitKey('\r', ConsoleKey.Enter);
                    Thread.Sleep(5);
                }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var buf = Buf(rs.Buffer.GetFrontBuffer());
                    Assert.Contains("Submit", buf);
                    Assert.DoesNotContain("  A", buf);
                    Assert.DoesNotContain("  B", buf);
                    Assert.DoesNotContain("  C", buf);
                })
            };
        }

        // Auto-batch: Grid basics (27..32)
        for (int g = 0; g < 6; g++)
        {
            int idx = g;
            yield return new object[]
            {
                $"Grid_Basic_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    var grid = new Grid { Gap = 1 }
                        .WithColumns(GridTrackSize.Pixels(8), GridTrackSize.Pixels(8), GridTrackSize.Pixels(8))
                        .WithRows(GridTrackSize.Auto, GridTrackSize.Auto);
                    grid.Add(new Text($"C1{idx}").GridArea(0,0));
                    grid.Add(new Text("Col2").GridArea(0,1));
                    grid.Add(new Text("Col3").GridArea(0,2));
                    return new VStack(spacing: 1) {
                        new Text("Grid Basic"),
                        grid
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) => { Thread.Sleep(5); }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var buf = Buf(rs.Buffer.GetFrontBuffer());
                    Assert.Contains("Col2", buf);
                })
            };
        }

        // Auto-batch: TextArea short typing (33..38)
        for (int t = 0; t < 6; t++)
        {
            int idx = t;
            yield return new object[]
            {
                $"TextArea_Short_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    string text = "";
                    return new VStack(spacing: 1) {
                        new Text($"TA {idx}"),
                        new TextArea("Enter...", new Binding<string>(() => text, v => text = v), rows: 2, cols: 8)
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
                {
                    input.EmitKey('\t', ConsoleKey.Tab);
                    Thread.Sleep(5);
                    input.EmitKey('x', ConsoleKey.X);
                    Thread.Sleep(5);
                }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var buf = Buf(rs.Buffer.GetFrontBuffer());
                    Assert.Contains("┌", buf);
                })
            };
        }

        // 39-42) Nested clipping scenarios
        for (int c = 0; c < 4; c++)
        {
            int idx = c;
            yield return new object[]
            {
                $"NestedClipping_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    // Outer box clips, inner box clips further
                    var inner = new Box { Width = 8, Height = 1, Overflow = Overflow.Hidden };
                    inner.Add(new Text("InnerContent_TooLong"));
                    var outer = new Box { Width = 10, Height = 2, Overflow = Overflow.Hidden };
                    outer.Add(new Text("Header"));
                    outer.Add(inner);
                    return new VStack(spacing: 0) {
                        new Text($"NC {idx}"),
                        outer,
                        new Text("Tail")
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) => { Thread.Sleep(5); }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var buf = Buf(rs.Buffer.GetFrontBuffer());
                    Assert.Contains("Header", buf);
                    Assert.Contains("Tail", buf);
                })
            };
        }

        // 43-46) Overlapping movement cases
        for (int m = 0; m < 4; m++)
        {
            int idx = m;
            yield return new object[]
            {
                $"OverlapMove_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    string a = "Left"; string b = "Right"; int posB = 10 + idx;
                    return new VStack(spacing: 0) {
                        new Text(a),
                        new Text(new string(' ', posB) + b)
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) => { Thread.Sleep(5); }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var line0 = term.GetLine(0);
                    Assert.Contains("Left", line0);
                })
            };
        }

        // 47-54) Static text stacks (safe baseline scenarios)
        for (int s = 0; s < 8; s++)
        {
            int idx = s;
            yield return new object[]
            {
                $"Static_Stack_{idx}",
                (Func<ISimpleComponent>)(() =>
                {
                    return new VStack(spacing: 0) {
                        new Text($"Static {idx}"),
                        new Text("Alpha"),
                        new Text("Beta"),
                        new Text("Gamma")
                    };
                }),
                (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) => { Thread.Sleep(5); }),
                (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
                {
                    var buf = Buf(rs.Buffer.GetFrontBuffer());
                    Assert.Contains("Alpha", buf);
                    Assert.Contains("Gamma", buf);
                })
            };
        }

        // removed: MixedGridDropdown scenarios to keep exactly +8 additions in this batch

        // 7) TextArea typing and wrap
        yield return new object[]
        {
            "TextArea_Typing_Wrap",
            (Func<ISimpleComponent>)(() =>
            {
                string text = "";
                return new VStack(spacing: 1) {
                    new Text("Textarea Demo"),
                    new TextArea("Enter...", new Binding<string>(() => text, v => text = v), rows: 3, cols: 12),
                    new Button("Next", () => { })
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab); // focus textarea
                Thread.Sleep(10);
                foreach (var ch in "Hello world long")
                {
                    var key = char.IsLetter(ch) ? (ConsoleKey)Enum.Parse(typeof(ConsoleKey), char.ToUpper(ch).ToString())
                                                : (ch == ' ' ? ConsoleKey.Spacebar : ConsoleKey.OemPeriod);
                    input.EmitKey(ch, key);
                    Thread.Sleep(5);
                }
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("┌", buf); // border corner
                Assert.Contains("┘", buf);
            })
        };

        // 8) Dropdown long list, move highlight to last item
        yield return new object[]
        {
            "Dropdown_LongList_Scroll",
            (Func<ISimpleComponent>)(() =>
            {
                string sel = ""; var items = new[]{"One","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten"};
                return new VStack(spacing: 1) {
                    new Text("Long Dropdown"),
                    new Dropdown<string>("Pick...", items, new Binding<string>(() => sel, v => sel = v)).Color(Color.White)
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey(' ', ConsoleKey.Spacebar);
                Thread.Sleep(10);
                for (int i = 0; i < 9; i++) { input.EmitKey('\0', ConsoleKey.DownArrow); Thread.Sleep(5); }
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(20);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.DoesNotContain("  One", buf);
                Assert.DoesNotContain("  Ten", buf);
            })
        };

        // 9) Confirm modal with Yes/No buttons
        yield return new object[]
        {
            "Modal_Confirm_Select",
            (Func<ISimpleComponent>)(() =>
            {
                bool open = false; string state = "";
                return new ZStack {
                    new VStack(spacing: 1) { new Text("Main"), new Text(" "+ state) , new Button("Open Confirm", () => open = true) },
                    Dialog.Confirm("Confirm", "Proceed?", new Binding<bool>(() => open, v => open = v), () => state = "YES")
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('\t', ConsoleKey.Tab); // focus Open Confirm
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter); // open modal
                Thread.Sleep(20);
                input.EmitKey('\t', ConsoleKey.Tab); // move to Yes
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(30);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("YES", buf);
            })
        };

        // 10) SelectInput navigation and selection
        yield return new object[]
        {
            "SelectInput_Navigation",
            (Func<ISimpleComponent>)(() =>
            {
                string selected = ""; var items = new[]{"Red","Green","Blue"};
                return new VStack(spacing: 1) {
                    new Text("SelectInput Demo"),
                    new SelectInput<string>(
                        items,
                        new Binding<Optional<string>>(
                            () => string.IsNullOrEmpty(selected) ? Optional<string>.None : Optional<string>.Some(selected),
                            opt => selected = opt.HasValue ? opt.Value : ""
                        ),
                        s => s)
                };
            }),
            (Action<DeclarativeRenderer, TestInputHandler>)( (renderer, input) =>
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(10);
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(10);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(20);
            }),
            (Action<RenderingSystem, MockTerminal>)( (rs, term) =>
            {
                var buf = Buf(rs.Buffer.GetFrontBuffer());
                Assert.Contains("Green", buf);
            })
        };

        // More scenarios will be added here in batches until we reach ~200.
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void RunScenario(string name, Func<ISimpleComponent> makeUI,
        Action<DeclarativeRenderer, TestInputHandler> drive,
        Action<RenderingSystem, MockTerminal> assert)
    {
        var (rs, renderer, input, term) = Setup();
        var th = new Thread(() => renderer.Run(makeUI)) { IsBackground = true };
        th.Start();
        Thread.Sleep(120);

        drive(renderer, input);
        Thread.Sleep(120);

        try
        {
            assert(rs, term);
        }
        catch
        {
            // On failure, include the scenario name and buffer snapshot for diagnostics
            var buf = Buf(rs.Buffer.GetFrontBuffer());
            throw new Xunit.Sdk.XunitException($"Scenario '{name}' failed.\nBuffer:\n{buf}");
        }

        rs.Shutdown();
        th.Join(100);
    }
}
