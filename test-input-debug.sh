#!/bin/bash
export ANDY_TUI_DEBUG=debug
echo "1" | dotnet run --project examples/Andy.TUI.Examples.Input/Andy.TUI.Examples.Input.csproj 2>input-debug.log