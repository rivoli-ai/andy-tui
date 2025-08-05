#!/bin/bash
cd examples/Andy.TUI.Examples.Input
export ANDY_TUI_DEBUG=1
echo -e "\n9\n" | timeout 5 dotnet run 2>&1 | tee /tmp/textwrap_debug.log || true
echo "=== Debug log location ==="
ls -la /var/folders/*/T/andy-tui-debug/* 2>/dev/null | tail -1