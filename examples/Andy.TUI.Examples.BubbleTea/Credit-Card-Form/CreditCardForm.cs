namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class CreditCardForm
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new CardFormApp();
        renderer.Run(() => app.Render());
    }

    private class CardFormApp
    {
        private string _name = "";
        private string _number = "";
        private string _exp = "";
        private string _cvc = "";
        private bool _submitted = false;

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("Credit Card Form (Ctrl+C to quit)").Bold(),
                new HStack(spacing: 2) { new Text("Name:"), new TextField("Cardholder name", new Binding<string>(() => _name, v => _name = v)) },
                new HStack(spacing: 2) { new Text("Number:"), new TextField("1234 5678 9012 3456", new Binding<string>(() => _number, v => _number = v)) },
                new HStack(spacing: 2) { new Text("Exp:"), new TextField("MM/YY", new Binding<string>(() => _exp, v => _exp = v)) },
                new HStack(spacing: 2) { new Text("CVC:"), new TextField("123", new Binding<string>(() => _cvc, v => _cvc = v)) },
                new Button("Submit", OnSubmit),
                new Text(_submitted ? "Submitted!" : "").Color(Color.Green)
            };
        }

        private void OnSubmit()
        {
            _submitted = true;
        }
    }
}
