# Design Rules

## General

- Choose the simplest implementation that solves the current problem, preserves required behavior, and protects user data. Optimize for clarity and maintenance, not architectural completeness.
- Justify every added mechanism with a concrete problem: an observed bug, a race demonstrated by current call paths, normal usage, or a meaningful risk to user data. Theoretical failures and possible future requirements are not sufficient reasons to add complexity.
- Before adding state, tasks, synchronization, or recovery behavior, explain what happens without it, whether a simpler response is acceptable, and why the benefit outweighs the permanent complexity. If the case is unclear, leave it out.
- Accept explicit capability limits when recovery by retry, refresh, reinitialization, or restart is safe. Do not turn every limit into an automatic recovery protocol.
- Fix responsibility boundaries that cause the current problem, while keeping the change scoped. A concern about technical debt does not by itself justify redesigning the module.
- Prefer fewer concepts and types when they express the requirements clearly. Every new model, wrapper, or layer needs a concrete responsibility and consumer; similar features do not automatically need identical sets of classes.
- Before introducing a type, explain what the existing types cannot express and where the new type belongs, down to its project and file. Consider whether an existing model, a small internal index, or a targeted read already meets the requirement.
- Abstract actual variation and current consumers. Do not create a public abstraction or extension point solely for a possible second implementation.
- Preserve established names and responsibilities unless changing them solves a concrete problem. Do not rename DTOs or add parallel `Entry`/`Snapshot` models merely to follow a pattern.

## State and concurrency

- Minimize shared mutable state before adding locks. Prefer clear ownership and serialized state updates where they simplify the flow; avoid making readers track mutual exclusion across many methods.
- Default to one primary serialization mechanism per service. Keep it at a narrow business boundary and check existing library guarantees before adding synchronization; do not wrap already safe operations in redundant locks.
- Prefer straightforward serialization to parallelism that requires additional coordination protocols. Introduce more complex concurrency only when measurements show that the simpler boundary is a real bottleneck.
- Give background work stable input rather than concurrent access to changing UI properties. A snapshot does not automatically require a new public model or a duplicate of an existing DTO.

## Performance and validation

- Optimize expensive work first. Accept cheap repeated computation and idempotent updates when they avoid permanent bookkeeping; require evidence before adding caches, indexes, or incremental computation.
- Test required behavior, known bugs, and meaningful failure cases. Do not expand the implementation to satisfy a speculative test matrix.

## Planning

Classify architecture plans by implementation priority:

- **MUST**: the minimum changes needed to solve current, demonstrated problems and protect required behavior or user data.
- **LATER**: potentially useful improvements without enough evidence to justify implementing them now.
- **DO NOT IMPLEMENT NOW**: mechanisms whose complexity exceeds their current value.

Develop only MUST into an implementation plan. Keep deferred items brief, state accepted capability limits, and do not design their future implementation. Favor changes to existing code and minimize new types, state, tasks, locks, caches, and cross-layer protocols.

## Models and ViewModels

- Wire contracts belong in `Euterpe.Contracts`. App-internal DTOs in `Euterpe.Models` may combine remote data, local state, and computed display properties; this is a valid responsibility and does not require another model layer.
- A manage service owns data and business operations; a page ViewModel combines service data, filtering, sorting, and page interaction. Add an item ViewModel only when an individual item has actual page-specific state or behavior, not just to forward DTO properties.
- A filter ViewModel owns live control input. Separate fixed filter criteria only when background evaluation or another real consumer needs them; do not split types solely to create another layer.
- Place types according to their actual consumers and the project map in `AGENTS.md`. Feature-specific filter criteria can stay in that feature; private implementation records stay nested with their owner. Promote a type to a shared project only when a real cross-project boundary requires it.

## Multi-game state and lifetime

- Reuse the same views, ViewModel types, and data formats across games. Each game has its own ViewModel and service instances, with separate caches and editable state.
- Create each game scope once and retain it for the application session. Switching games selects the existing instances for the target game; do not dispose or recreate the outgoing game's scope, ViewModels, caches, or subscriptions.
- Switching away and back must preserve in-memory edits and interaction state, including filters, sorting, and selection. A game switch must not reset state or reload data over the user's edits.
- Game-scoped page ViewModels and their reactive subscriptions intentionally stay alive for the application session. Do not add `IDisposable`, subscription bags, or teardown logic solely for page navigation or game switching. Short-lived resources still follow their actual ownership and disposal boundaries.
- Keep game-specific state and services game-scoped. Use `[AppSingleton]` only for instances shared across all games.
