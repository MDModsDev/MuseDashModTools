# Design Rules

## General

- Design the change, don't just complete the feature: a working implementation that leaves tech debt is not acceptable. If every viable approach leaves debt, the module itself is due for a redesign — surface that instead of bolting on.
- Build along known extension axes. Ask "what varies when the next game/variant arrives" before hardcoding.

## Multi-game state and lifetime

- Reuse the same views, ViewModel types, and data formats across games. Each game has its own ViewModel and service instances, with separate caches and editable state.
- Create each game scope once and retain it for the application session. Switching games selects the existing instances for the target game; do not dispose or recreate the outgoing game's scope, ViewModels, caches, or subscriptions.
- Switching away and back must preserve in-memory edits and interaction state, including filters, sorting, and selection. A game switch must not reset state or reload data over the user's edits.
- Game-scoped page ViewModels and their reactive subscriptions intentionally stay alive for the application session. Do not add `IDisposable`, subscription bags, or teardown logic solely for page navigation or game switching. Short-lived resources still follow their actual ownership and disposal boundaries.
- Keep game-specific state and services game-scoped. Use `[AppSingleton]` only for instances shared across all games.
