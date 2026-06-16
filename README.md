# Losten Memory

Unity 2D project for a diploma game prototype.

## Unity version

Use Unity `6000.4.5f1`.

## Scenes

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Game.unity`
- `Assets/Scenes/GameScene2.unity`
- `Assets/Scenes/GameScene3.unity`

## Current gameplay loop

1. Start from `MainMenu`.
2. Pick up the key in `Game`.
3. Open the door to `GameScene2`.
4. Talk to the NPC through choice dialogue.
5. Move to `GameScene3` and solve the tile puzzle.

## Git workflow for beginners

Before starting work:

```powershell
git pull
```

After finishing a small change:

```powershell
git status
git add Assets ProjectSettings Packages README.md .gitignore
git commit -m "Describe what changed"
git push
```

Rules:

- Do not commit `Library`, `Temp`, `Logs`, `Build`, `obj`, `.vs`, or ready-made `.zip` builds.
- Commit `.meta` files together with Unity assets.
- Work in small commits: one feature or fix per commit.
- If Git reports conflicts in a Unity scene, stop and ask before editing the conflict manually.
