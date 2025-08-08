#!/bin/bash
echo "Testing TabView keyboard navigation..."
echo "1. Run the TabView example"
echo "2. Try arrow keys to switch tabs"
echo ""
echo "Press Enter to start..."
read

# Run with debug logging
export ANDY_TUI_DEBUG=debug

# Launch in background and capture PID
echo "1" | dotnet run --project examples/Andy.TUI.Examples.ZIndex/Andy.TUI.Examples.ZIndex.csproj 2>tabview-test.log &
PID=$!

echo ""
echo "TabView is running (PID: $PID)"
echo "Try pressing arrow keys to switch tabs..."
echo "Press Enter to stop the test..."
read

# Kill the process
kill $PID 2>/dev/null

echo ""
echo "Test complete. Check tabview-test.log for debug output."