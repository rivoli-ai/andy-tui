# Immediate Next Steps

Based on the [Declarative Implementation Plan](docs/DECLARATIVE_IMPLEMENTATION_PLAN.md), here are the immediate tasks to continue the implementation:

## 🔥 Current Priority: Complete Layout System

### 1. Update Existing ViewInstances (In Progress)
The new layout system with LayoutBox and flexbox properties needs to be applied to all existing components:

- [x] TextInstance - Updated with LayoutBox
- [ ] HStackInstance - Needs conversion from manual positioning to flexbox
- [ ] VStackInstance - Needs conversion from manual positioning to flexbox  
- [ ] ButtonInstance - Needs layout calculation
- [ ] TextFieldInstance - Needs layout calculation
- [ ] DropdownInstance - Needs layout calculation

### 2. Fix Compilation Issues
After updating ViewInstance base class, all derived instances need to implement:
- `protected override LayoutBox PerformLayout(LayoutConstraints constraints)`
- `protected override VirtualNode RenderWithLayout(LayoutBox layout)`

### 3. Test Integration
Once components are updated:
- Run the input example to verify layout works correctly
- Check that focus management still functions
- Ensure dropdown positioning is correct

## 📝 Example Migration

Here's how to update a ViewInstance to the new system:

```csharp
// Old style
public override VirtualNode Render()
{
    return Element("text")
        .WithProp("x", 0)
        .WithProp("y", 0)
        .Build();
}

// New style
protected override LayoutBox PerformLayout(LayoutConstraints constraints)
{
    return new LayoutBox
    {
        Width = constraints.ConstrainWidth(desiredWidth),
        Height = constraints.ConstrainHeight(desiredHeight)
    };
}

protected override VirtualNode RenderWithLayout(LayoutBox layout)
{
    return Element("text")
        .WithProp("x", layout.AbsoluteX)
        .WithProp("y", layout.AbsoluteY)
        .Build();
}
```

## 🚀 After Layout System is Complete

The next major tasks will be:
1. **Core Components** - TextArea, SelectInput, Table, etc.
2. **Hook System** - UseState, UseEffect, UseMemo
3. **Animation System** - Property animations with easing

See the full [implementation plan](docs/DECLARATIVE_IMPLEMENTATION_PLAN.md) for detailed breakdowns of each phase.