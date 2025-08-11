# Immediate Next Steps

This focuses on the current rendering architecture work and related reliability items.

## 🔥 Highest Priority: Renderer Unification and Correctness

- [ ] Unify `VirtualDomRenderer` to a single rendering path (choose visitor or element-based) and remove duplicate flows
- [ ] Fix incremental patch application: ensure old regions are cleared for move/resize, remove duplication artifacts
- [ ] Make clipping behavior consistent within the unified renderer
- [ ] Standardize child-adding APIs across the stack (collection-initializer for declarative, builder/`AddChild` in VDOM)

## 🧭 Z-Index and Spatial Index Integration

- [ ] Finalize absolute z-index propagation from `ViewInstance` and adopt in renderer
- [ ] Integrate `Enhanced3DRTree` for spatial queries in dirty-region rendering
- [ ] Implement occlusion-aware rendering: skip fully occluded elements; mark revealed areas on movement

## 🧪 Testing & Diagnostics

- [ ] Expand movement/resize test matrix in `DiffEngineMovementTests` and `VirtualDomRendererTests`
- [ ] Enable comprehensive logging in failing tests to capture render cycles and patch details
  - Use `ComprehensiveLoggingInitializer.BeginTestSession(testName)`

## 📦 Examples & Docs

- [ ] Update examples to avoid fighting the scheduler; prefer `QueueRender`
- [ ] Document the unified renderer behavior and migration notes

For long-term roadmap, see `docs/DECLARATIVE_IMPLEMENTATION_PLAN.md`, `docs/Z_INDEX_ARCHITECTURE.md`, and `docs/SPATIAL_INDEX_DESIGN.md`.