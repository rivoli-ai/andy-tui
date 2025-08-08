# Andy.TUI vs. Bubble Tea — Feature Comparison

This document compares the current implementation in this repository (Andy.TUI) with Charm’s Bubble Tea.

## Architecture

| Feature | Andy.TUI | Bubble Tea | Comparison | Best |
|---|---|---|---|---|
| Core paradigm | Retained-mode with view instances + VDOM | TEA (Model/Update/View) | Retained tree vs. functional immediate mode | Depends |
| State model | Component-local state, bindings (`StateProperty<T>`) | Central Model + Msg in `Update` | Local ergonomics vs. global predictability | Depends (Bubble Tea for predictability) |
| Effects/async | Implicit via event handlers/scheduler | First-class `Cmd`/`Sub` | TEA has a clear, testable effects model | Bubble Tea |
| Focus mgmt | Global `FocusManager` + `EventRouter` | Typically component-scoped (Bubbles) | Centralized focus vs. per-component patterns | Depends |
| Z-index/layers | Built-in z-index + occlusion | No native z-index (string composition) | True layering vs. manual composition | Andy.TUI |

## Rendering & Performance

| Feature | Andy.TUI | Bubble Tea | Comparison | Best |
|---|---|---|---|---|
| Render granularity | Dirty-cell diff with double buffer | Full view string render (diff/flush under the hood) | Fine-grained cells vs. simple frames | Andy.TUI (for hotspots) |
| Frame scheduling | `RenderScheduler` (target FPS, batching) | Message-driven renders (timers/ticks via `Cmd`) | Fixed-frame loop vs. message-triggered | Depends |
| VDOM diff | Yes (`DiffEngine`) | No (string-based view) | Tree/keyed diff vs. text render | Andy.TUI (for complex trees) |
| Partial updates | Cell-level dirty regions | Typically full re-render | More efficient local changes | Andy.TUI |
| Animations | Scheduler-driven | Ticks/timers via `Cmd` | Both capable via different models | Tie |

## Layout & Components

| Feature | Andy.TUI | Bubble Tea | Comparison | Best |
|---|---|---|---|---|
| Layout system | SwiftUI-like (`HStack`, `VStack`, `ZStack`, `Grid`, flex) | Lip Gloss sizing/padding; no general grid/flex | Rich layout engine vs. styled strings | Andy.TUI |
| Hit testing | Bounds + hit testing | Manual or component-provided | Built-in hit test vs. ad hoc | Andy.TUI |
| Component set | Built-in inputs, lists, table, modal, tabview, etc. | Rich ecosystem via Bubbles (table, list, textarea, viewport, etc.) | Integrated set vs. large ecosystem | Depends (Bubble Tea for breadth) |
| Styling | `Style` with ANSI/truecolor | Lip Gloss (very expressive) | Lip Gloss often more ergonomic/powerful | Bubble Tea |
| Markdown/format | Manual or custom | Ecosystem (e.g., Glamour) | Strong ecosystem for formatting | Bubble Tea |

## Input & Terminal

| Feature | Andy.TUI | Bubble Tea | Comparison | Best |
|---|---|---|---|---|
| Keyboard input | Enhanced `KeyInfo`, routing to focus | Msg-based events to `Update` | Central router vs. Msg stream | Depends |
| Mouse input | Mouse info, drag/wheel, routing | Mouse messages supported | Both support; patterns differ | Tie |
| Resize handling | Terminal `SizeChanged` -> buffer resize | `WindowSizeMsg` to `Update` | Both responsive | Tie |
| Alt screen/cursor | `AnsiTerminal` enter/exit alt screen | Built-in program options | Parity on core terminal control | Tie |
| Cross‑platform | .NET Console/ANSI | Go + termenv ecosystem | Both cross-platform | Tie |

## DX, Testing & Ecosystem

| Feature | Andy.TUI | Bubble Tea | Comparison | Best |
|---|---|---|---|---|
| Dev style | SwiftUI-like declarative API | TEA with Msg/Cmd | SwiftUI familiarity vs. TEA rigor | Depends |
| Testability | Instance/VDOM tests; some side effects local | Pure `Update` easy to unit test | TEA is highly testable | Bubble Tea |
| Learning curve | Easy if familiar with SwiftUI | Easy if familiar with TEA/Elm | Depends on background | Depends |
| Ecosystem | In-repo components/examples | Large community (Bubbles, Lip Gloss, examples) | Ecosystem maturity and breadth | Bubble Tea |
| Docs/examples | Solid in repo | Extensive in community | Broader references and patterns | Bubble Tea |

## Practical Takeaways

- Complex layouts, layers, and fine-grained updates: Andy.TUI shines (layout engine, z-index, dirty-cell rendering).
- Predictable state, async effects, testing, and styling ergonomics: Bubble Tea shines (TEA model, Cmd/Sub, Lip Gloss, ecosystem).
- Typical apps with standard components and strong community support: Bubble Tea likely fastest path.
- C#/.NET shops needing rich layout semantics and retained UI: Andy.TUI is a natural fit.

