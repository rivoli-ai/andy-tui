# BubbleTea Examples Parity Plan for Andy.TUI

Date: 2025-08-09

Goal: Achieve feature and UX parity with Charmbracelet BubbleTea examples, keeping folder and example names identical, implemented in C# with Andy.TUI's declarative style.

## Scope (examples)
- [x] altscreen-toggle
- [x] chat
- [x] exec
- [x] focus-blur
- [x] fullscreen
- [x] autocomplete
- [x] list-simple
- [x] list-default
- [x] list-fancy
- [x] cellbuffer
- [x] composable-views
- [x] credit-card-form
- [x] debounce
- [x] file-picker
- [x] glamour
- [x] help
- [x] http
- [x] mouse

## TODO checklist (by area)

### UX parity
- [ ] Autocomplete: highlight matched substrings (e.g., bold or color)
- [ ] File picker: breadcrumbs in header (..), toggle hidden files (h/H), sorting toggle (name/date/size), directory indicators, Enter to cd, Backspace to go up
- [ ] Lists: sticky header, footer with paginator (n/m), page up/down behavior, total/selected counts, consistent highlight/indicator styles
- [ ] Chat: input history (Up/Down), timestamps, scrollback beyond 10 lines, Ctrl+L to clear
- [ ] Help: keymap box per example; Esc to close overlays

### TEA compatibility layer
- [ ] Create `tea-shim` (Model, Msg, Update, View) adapter to run BubbleTea-like programs
- [ ] Implement Cmd helpers (timer tick, debounced input, HTTP, file IO) mirroring BubbleTea names
- [ ] Convert 2-3 examples using the shim to validate ergonomics

### Visuals and theming
- [ ] Theme variables (colors, borders, indicators) and default theme approximating BubbleTea look
- [ ] Spinner styles: verify frames and cadence against BubbleTea, add missing styles if any
- [ ] Markdown renderer (for Glamour): headings, emphasis, lists, code, links; basic theming

### Input and mouse
- [ ] Enable mouse reporting (SGR) where supported; parse sequences and route to components
- [ ] Lists and file picker: hover highlight, click to select, wheel scroll, drag selection (where applicable)

### Performance and rendering
- [ ] Virtualize long lists (render only visible rows)
- [x] ANSI renderer optimization: smarter style diffing and cross-line coalescing
- [x] Terminal resize handling (SIGWINCH): recalc constraints, re-render immediately

### Reliability and async patterns
- [ ] Standard async command pattern with cancellation tokens and error surfacing in UI
- [ ] Debounce: cancel pending work on new input; show "typing…" state
- [ ] HTTP: progress indicator (bytes), retry/backoff, friendly error messages

### API ergonomics
- [ ] List/Select templating: item template, custom indicator, footer hooks (page info, hints)
- [ ] Form helpers: labeled inputs, validation, masking (credit-card), error summaries

### Cross-platform + CI
- [ ] Verify macOS/Linux/Windows behavior (input, rendering, colors)
- [ ] CI matrix job for examples build + snapshot tests
- [ ] Snapshot tests with mock renderer for selected examples (layout + content)

### Documentation and demos
- [ ] Per-example README with keys/features and short screencast GIF
- [ ] Top-level mapping table: BubbleTea example → Andy.TUI example + notes on differences

## Phased execution
- Phase 1 (UX polish): autocomplete highlighting; file-picker breadcrumbs + hidden toggle; list footer/pagination; chat scrollback/history; help keymap
- Phase 2 (input/mouse + performance): mouse routing/UX; list virtualization; resize handling; ANSI optimizations
- Phase 3 (TEA shim + visuals): tea-shim + cmds; spinner cadence; markdown renderer; theming
- Phase 4 (reliability + docs + CI): async patterns; HTTP progress; docs and GIFs; CI snapshots

## Status
- Current: initial implementations landed for all examples; started UX polish (http spinner/disabled, chat wrapping, autocomplete placeholders, list iconography)
- Next: implement Phase 1 tasks; update this checklist as items complete