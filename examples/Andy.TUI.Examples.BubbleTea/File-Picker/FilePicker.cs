namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class FilePicker
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new FilePickerApp();
        renderer.Run(() => app.Render());
    }

    private class FilePickerApp
    {
        private string _cwd = Environment.CurrentDirectory;
        private Optional<string> _selection = Optional<string>.None;

        public ISimpleComponent Render()
        {
            var entries = Directory.EnumerateFileSystemEntries(_cwd)
                .Select(path => Path.GetFileName(path) + (Directory.Exists(path) ? "/" : ""))
                .OrderBy(name => name)
                .ToList();

            return new VStack(spacing: 1) {
                new Text($"File Picker: {_cwd}").Bold(),
                new SelectInput<string>(entries, new Binding<Optional<string>>(() => _selection, v => OnPick(v)), s => s, 12, "Empty")
                    .VisibleItems(12)
            };
        }

        private void OnPick(Optional<string> selected)
        {
            _selection = selected;
            if (!_selection.HasValue) return;
            var name = _selection.Value;
            var full = Path.Combine(_cwd, name.TrimEnd('/'));
            if (Directory.Exists(full))
            {
                _cwd = full;
                _selection = Optional<string>.None;
            }
        }
    }
}
