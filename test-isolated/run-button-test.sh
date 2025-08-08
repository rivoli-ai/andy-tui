#!/bin/bash
# Run the button focus test with debug logging enabled

export ANDY_TUI_DEBUG=Debug
echo "Running button focus test with debug logging..."
echo "Debug logs will be written to: $(dirname $(mktemp -u))/andy-tui-debug/$(date +%Y%m%d_%H%M%S)"
echo ""
echo "Instructions:"
echo "- Press TAB to switch focus between buttons"
echo "- Focused button should show with cyan background"
echo "- Press ENTER to click the focused button"
echo "- Watch for 'Button clicked' messages in console"
echo "- Press Ctrl+C to exit"
echo ""

dotnet run --project test-isolated/test-focus.csproj