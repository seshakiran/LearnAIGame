# Spike A — Setup

Everything here is code-driven on purpose (see PLAN.md §12.1) — there is no hand-built scene or prefab to wire up. This avoids hand-authoring Unity's binary/YAML scene files outside the Editor, which is fragile.

## One-time project creation (once Unity Editor 6000.0.81f1 finishes installing)

From a terminal, with the Unity Editor installed:

```bash
/Applications/Unity/Hub/Editor/6000.0.81f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -createProject "/Users/itsthematrix/Downloads/Projects/LearnAIGame/UnityProject" -quit
```

This generates `ProjectSettings/` and `Packages/manifest.json` alongside the `Assets/` folder that's already here — Unity will pick up the existing scripts and data on first open.

## First open (2 minutes, one-time, needs the Editor GUI)

1. Open Unity Hub -> Add project -> select `UnityProject/`.
2. Open it. Let it import (first import compiles all scripts — can take a minute).
3. File -> New Scene (empty/basic scene is fine).
4. Create an empty GameObject (GameObject -> Create Empty), rename it `GameLoop`.
5. Drag `Assets/Scripts/Gameplay/GameLoopController.cs` onto it as a component.
6. Save the scene as `Assets/Scenes/Bootstrap.unity`.
7. Hit Play.

Everything else — Canvas, cards, buttons, reveal panels — builds itself at runtime from `GameLoopController`.

## What you're testing (§12.1 success criteria)

1. Does the swipe burst feel fun on its own, before any AI framing is explained?
2. Does the video-stub/checkpoint feel connected to what was just played, or does it feel like an ad interrupting a game?

Get a few external people to play it, not just yourself — self-testing can't validate either question.

## Known stub points (expected, not bugs)

- The Feynman "video" is a text panel, not a real video — Spike B (Grok HITL pipeline + S3) replaces this later.
- The shareable result card and skill-tree payoff are placeholder text screens, not final art.
- Streak is tracked locally via `PlayerPrefs` (see `StreakTracker.cs`) — no backend yet (§10.3).
