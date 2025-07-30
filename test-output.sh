#!/bin/bash
cd examples/Andy.TUI.Examples.Input
dotnet run &
PID=$!
sleep 2
kill $PID 2>/dev/null
echo "Example completed"