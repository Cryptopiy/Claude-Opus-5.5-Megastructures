# Nocturne (Megastructures, game 6)

A first-person walk through a rain-soaked night megacity that has no ground. The walk runs 1 500 m through seven beats:
alley, boulevard canyon, the Edge, skybridge, hanging garden, wall of signs and Heart Square. It lasts 600 s at the
cinematic 2.5 m/s and about 1 000 s at the walking 1.5 m/s, and ends at one warm window under a 900 m hologram koi. The
game was built procedurally in Unity 6000.6.0f1 (URP 17.6, D3D11) for a YouTube video. The only imported assets are CC0
textures (D-027).

**Status (2026-11-16): in development, Phase 4 (beat production).** The game plays from start to goal in the built
player. Look v2 is complete: dark neutral palette and textured facades and surfaces. Per-beat content (tram, trains,
abyss traffic, garden lanterns, the screen wall) and the score are being built. See `FEATURE_MATRIX.md` for an honest
beat-by-beat status and `STATE.md` for the current work.

## Run
- Player: `Builds/Nocturne/Nocturne.exe` (Windows 10/11, a D3D11 GPU; 1080p uses about 0.7 GB VRAM, 4K about 1.6 GB).
- The game opens on a black fade-in. Walk forward to reach the goal; the run ends with a fade.

## Controls
| Input | Action |
|---|---|
| W A S D / arrow keys, gamepad left stick | walk (1.5 m/s) |
| Shift (hold), gamepad shoulder buttons | brisk walk, 2.5 m/s (the cinematic speed) |
| Mouse, gamepad right stick | look |
| C | toggle the cinematic autowalk (constant 2.5 m/s with the authored gaze) |
| Esc, gamepad Start | pause menu |
| Enter / Space, gamepad A | menu select |
| F11, Alt+Enter | toggle full screen / windowed |

## Command-line flags (one value per flag)
| Flag | Effect |
|---|---|
| `-cinematic` | start in the cinematic autowalk: 2.5 m/s constant, authored gaze, no input needed (for recording) |
| `-res 3840x2160` | window or full-screen size |
| `-rain 0..1` | rain density override (0 turns the rain pass off) |
| `-tier 0/1/2` | full / simplified / graybox fallback tiers |
| `-nofade` | skip the opening fade |
| `-quit-after <s>` | quit after that many seconds |
| `-beat <N>` | start at beat N (testing) |
| `-autotest`, `-autotest -cinematic`, `-wallbot`, `-soak <s>` | QA runs (completability, wall-hugging bot, soak) |
| `-capture <dir> -capture-set <set>` | QA captures (see `Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs` for the sets) |
| `-audio-dump <file.wav>` | record the in-game audio mix to a WAV |
| `-facade-v`, `-surf-v`, `-foliage-v`, `-relief-v`, `-facade-prepass` | Look v2 render-path switches (defaults are the accepted paths) |

## Recording the video
1. Run `Builds/Nocturne/Nocturne.exe -cinematic -res 3840x2160` (or 2560x1440) in full screen.
2. Record with OBS (or similar) at 60 fps with a high bitrate or a near-lossless codec. The game's dither is tuned
   for x264 CRF 23 at 1440p and above (MASTER_PROMPT 14.7 H1), so upload at 1440p or 2160p.
3. The cinematic walk takes 600 s from the fade-in to the goal, plus about 17 s of ending.

## For developers
- Spec: `MASTER_PROMPT.md` (Part 14 is the game). Architecture, commands and every decision: `DEVELOPMENT.md`.
  Current state and resume notes: `STATE.md`.
- Unity runs only through `Tools/unity-queue.ps1`; players only through `Tools/run-player.ps1` / `Tools/lookdev.ps1`.
  The command list is in `STATE.md` "Commands".
- QA: `QA/` (captures in `QA/captures/`, git-ignored; reports in `QA/reports/`; critiques `QA/critique-*.md`).
- Metrics (time, tokens, API-equivalent cost): `python -I Tools/metrics.py --md QA/reports/metrics.md --json QA/reports/metrics.json`.
