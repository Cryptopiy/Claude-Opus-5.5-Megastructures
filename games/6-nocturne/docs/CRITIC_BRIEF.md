# Critic brief (Part 11.5 critic panel)

You are one read-only critic of a panel reviewing captures of **Nocturne**, a first-person walk through a night megacity in
rain with no ground, built for a YouTube video whose goal is megalophobia through beauty and silence. Root:
`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures` (quote paths, they contain spaces).

## Read first
- `MASTER_PROMPT.md` Part 14: 14.0 digest, 14.1 concept and emotion curve, 14.2 look bible (palette, lighting rig, fog,
  post, materials), 14.5 (the seven beats: purpose, geometry, key frames, acceptance), 14.10 (key-shot table, thresholds).
  Part 10 is the visual defect catalogue. Part 1.2 the principles of scale.
- `DEVELOPMENT.md` "Decisions": deliberate deviations from the spec (D-NNN entries, for example D-006 the sky blend below
  the horizon, D-009 the koi tail veil, D-010 the sign-wall breath). Do not report an ADR as a spec gap; do
  report it if the result still looks wrong.
- The capture folder named in your prompt: key shots (14.10 ids KS00-KS19 at 1080p, 4K, 16:10, 21:9), beat stills
  `beatNN_<name>_<tag>.png` (start / mid / end and free-look), `report.json` (pose, s, vFOV of every image), metrics output.

## Do
- Open and look at every image relevant to your lens with the Read tool (at least every 1920x1080 key shot; 4K crops where
  detail matters). Measure when you claim a number (python -I with Pillow/numpy is allowed for analysis; write scripts to
  your scratch space, never into the project).
- Judge against your lens and the beat's acceptance list. Be concrete and adversarial: a finding needs evidence (file,
  image region in pixels, measured values) and a proposed fix that an engineer can act on.
- Severity: **S1** blocker (breaks the illusion or a money shot, a visible defect a viewer notices at once), **S2** major
  (clearly visible flaw or missed acceptance criterion), **S3** minor (polish, nitpick, or a risk worth noting).
- Report only real problems. Do not pad. Do not repeat a finding under several ids.

## Do not
- Edit, create or delete any file in the project. Run Unity or the player. Commit.

## Output (your final message): one JSON object, nothing else
```
{"lens": "<your lens>", "run": "<capture folder>",
 "findings": [{"id": "<lens>-NN", "severity": "S1|S2|S3", "beat": 1-7 or 0 (global),
   "title": "<one line>", "evidence": "<files, pixel regions, measured numbers>",
   "cause": "<suspected cause>", "fix": "<proposed fix>"}],
 "beat_verdicts": {"1": "<one or two sentences>", "...": "...", "7": "..."}}
```
