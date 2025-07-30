#!/bin/bash
echo "Starting Andy.TUI Declarative Input Demo..."
echo ""
echo "Instructions:"
echo "- Use Tab/Shift+Tab to navigate between fields"
echo "- Type to enter text in the focused field"
echo "- Use arrow keys to move cursor within a field"
echo "- Press Enter or Space on a button to activate it"
echo "- Press Ctrl+C to exit"
echo ""
echo "Press any key to start..."
read -n 1

cd examples/Andy.TUI.Examples.Input
dotnet run