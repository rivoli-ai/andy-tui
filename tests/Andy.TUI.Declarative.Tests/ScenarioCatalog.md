# UI Scenario Catalog (Draft)

This document outlines a broad catalog (~200) of realistic UI scenarios we should cover with automated integration tests. Each scenario is intended to run under the real render loop, with input simulation and buffer verification.

- Forms and Inputs
  - Single-field form; cursor, focus, typing, deletion
  - Two text fields + buttons; repeat tab cycles; ensure footer persists
  - Secure field masking; cursor moves; delete/backspace at edges
  - Long placeholder truncation and scroll while typing
  - Validation feedback rendering next to fields; toggling states
- Dropdowns/Selects
  - Closed trigger render; open/close; highlight movement; selection updates
  - Repeated open/select cycles; ensure backdrop/menu cleared; buttons visible
  - Long list (10+ items); scrolling highlight; z-index over form
  - Nested dropdowns in grids; overlapping regions
- TextArea
  - Multi-line typing; newlines; cursor; wrap on/off; borders stable
  - Scroll bar visualization; scroll with arrow keys; selection area remains
- Buttons
  - Primary/secondary styles; focus/hover (focus only) states
  - Press activation; action updates summary text
- Layout Containers
  - VStack/HStack spacing; gaps visible; no missing rows
  - ZStack overlapping labels; updates over time; no artifacts
  - Grid of text cells; updates in one column shift others; no ghosts
- Modals
  - Alert/Confirm/Prompt open/close; backdrop opacity; z-index above UI
  - Interactions while open route to modal only; on close, underlying UI restored
- Overlap and Movement
  - Text moving left/right/up/down with content shrinking/expanding; old area cleared
  - Multiple components moving simultaneously; merged dirty rects acceptable
- Clipping/Overflow
  - Content inside clipped region; outside should not render
  - Nested clipping; partial line visibility
- Colors/Styles
  - High-contrast theme visibility for focused inputs/buttons/dropdowns
  - Truecolor vs 256-color fallbacks
- Trees/Menus
  - Expand/collapse tree nodes; arrows; selection; ensure below content persists
  - Horizontal menus; active item highlight; movement; no clears on other lines
- Advanced/Stress
  - Rapid input and render requests; no tearing
  - Window resize events; layout reflows; no stale fragments
  - Mixed components: form + grid + dropdown + modal overlay interaction sequence

Implementation status: see ComplexUiStabilityTests.cs for initial coverage; more scenarios will be added iteratively until the full catalog is covered.
