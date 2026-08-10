# Spike A — Setup

Everything here is code-driven on purpose (see PLAN.md §12.1) — there is no hand-built prefab to wire up. The Canvas, cards, buttons, and reveal panels all build themselves at runtime from `GameLoopController`.

## Status: fully scaffolded

- Unity 6000.0.81f1 (LTS) installed, with iOS + Android build support.
- Project created at `UnityProject/`, packages resolved (`com.unity.ugui` added for uGUI/EventSystems), scripts compile clean.
- `Assets/Scenes/Bootstrap.unity` already exists with a `GameLoop` GameObject carrying `GameLoopController` — generated via `Assets/Editor/BootstrapSceneBuilder.cs` (menu: `LearnAIGame -> Build Bootstrap Scene`, re-runnable any time the scene needs regenerating).

## To run it

1. Open Unity Hub -> Add project -> select `UnityProject/` (or it may already be open).
2. In the Project window, double-click `Assets/Scenes/Bootstrap.unity` if it isn't already the open scene.
3. Hit Play.

## What you're testing (§12.1 success criteria)

1. Does the swipe burst feel fun on its own, before any AI framing is explained?
2. Does the video-stub/checkpoint feel connected to what was just played, or does it feel like an ad interrupting a game?

Get a few external people to play it, not just yourself — self-testing can't validate either question.

## Known stub points (expected, not bugs)

- The Feynman "video" is a text panel, not a real video — Spike B (Grok HITL pipeline + S3) replaces this later.
- The shareable result card and skill-tree payoff are placeholder text screens, not final art.
- Streak is tracked locally via `PlayerPrefs` (see `StreakTracker.cs`) — no backend yet (§10.3).
