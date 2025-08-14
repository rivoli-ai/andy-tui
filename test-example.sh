#!/bin/bash
cd examples/Andy.TUI.Examples.Input
echo "Testing example 1..."
# Send input 1 and then Ctrl+C after a delay
( echo "1"; sleep 2; echo -e "\x03" ) | dotnet run 2>&1 | head -50