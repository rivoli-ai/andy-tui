#!/bin/bash
cd examples/Andy.TUI.Examples.Input
# Use expect or timeout if available, otherwise just try to run with a timeout
( echo "19"; sleep 2 ) | dotnet run 2>&1 | head -50