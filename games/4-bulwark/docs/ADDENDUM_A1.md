# ADDENDUM A1 — The kraken and the new ending (binding)

Synthesis of two design studies (cinematic lens and technical lens) for the user's redesign order of 2026-11-19
(`STATE.md`, "USER REDESIGN ORDER"). Every number below was recomputed for this document with throwaway scripts from the
14.2-14.12 constants (`calc` notes in brackets). Coordinates are true world coordinates of 14.4 (Y up, +Z north, +X east =
sea, heading/yaw 0 = north, 90 = east). Sizes in px are at 3840 x 2160 (4K) on the centre column, 1080p in brackets;
vFOV 64 gives 33.75 px/deg, vFOV 66 gives 32.73 px/deg. "t" is the time from the start of the walk; "t at 2.5 / 1.5" is
cinematic / default walking speed. T is the fog transmittance along the line of sight.

---

## 00. LEAD DECISIONS 2026-11-20 (win over every section below; the review in Logs/a1_review.json is resolved by them)
Deadline: the user wants the finished game within 10 hours (by about 21:30 local on 2026-11-20). Every builder works to that.
1. ENDING (replaces the inclined fin ride of section 3): at the end of the lee ledge a glass EXPRESS ELEVATOR cabin runs VERTICALLY down the outside of the lee face (x about -8 to -12, no buttress) from the ledge to the foot in about 75 s (smooth S-curve, peak about 110 m/s, comfortable accelerations). The cabin glass faces west (open view down to the lowland and the sunset) with glass floor panels so the look-down shots work from the walker's fixed spot. The sunset happens during the descent (section 3 numbers: lower limb touches at 5467 m, centre at 4388 m, gone at 3421 m altitude above the lowland). At the foot the same cabin couples to a horizontal maglev SHUTTLE and runs WEST about 3.5 km in about 35 s while the camera turns to look BACK at the dam (the wall recedes and its whole height appears). It stops at the LANTERN PLATFORM (a small lit plaza with lanterns at the edge of the lowland town lights, about 3.5 km from the face); the doors open; walk about 40 m to the lantern goal; the final look back shows the WHOLE dam face from its foot to the crest in the dusk haze (crest elevation atan(6000/3500) = 59.7 deg: frame it with pitch about 28 deg at vFOV 66), the upper wall still in alpenglow, outlet jets, gallery lights; then the title/ending. The fin is NOT built. Shared state: Runtime/Core/EndingRide.cs (writer: the ending owner).
2. KRAKEN MOTION: slow and heavy everywhere: visible parts at most 8 m/s, reveals over tens of seconds, nothing quick; it must read as enormous (Part 1.2).
3. KRAKEN SOUND: every appearance has a CLEARLY audible, terrifying sound on any speaker: energy in 80-800 Hz growl harmonics, grinding and creaking textures plus the infrasonic body; the beds duck by 6-10 dB under a call; sound may precede sight by 2-6 s. Shared bus: Runtime/Core/KrakenCalls.cs (writer: kraken choreography; reader: audio).
4. Geometry claims must be consistent (cameras never inside concrete); fix numbers as you build, with arithmetic in your report.

## 0. Status, precedence and what this supersedes

1. **Authority.** This addendum is part of the game specification. It wins over `MASTER_PROMPT.md` Part 14 wherever they
   conflict (Part 1.6 order becomes: Part 2.0 > the user's redesign order in `STATE.md` > **this addendum** > Part 14 >
   Parts 1-13, 15). It never overrides Part 2.0, Part 15.3, or the visible-quality rules of Part 1.6.2 (no relaxing of
   banding, aliasing, shimmer, popping, clipping, exposure steps, seams).
2. **User-ordered exceptions** (log each as a row in `DEVELOPMENT.md`): (a) the Tripo3D kraken models are external assets
   (Part 6); (b) an express inclined lift is machinery (14.1 "no machinery, no sci-fi"; it is built as real engineering
   pushed to the limit, see 3.1); (c) the run time grows from 600 s to 816 s to the goal at 2.5 m/s.
3. **Superseded in Part 14** (the replacement is named in brackets):
   - 14.0 digest: money shots (3) and (4), beat list, runtime, constants (section 4.1).
   - 14.1 arc table and runtime (4.2).
   - 14.2.3 / 14.2.4 / 14.2.5 rows for the new beats 8-9 and the end sequence (4.3).
   - 14.3 row V (whale call), S6, S8 and the master targets (section 2, 4.4).
   - 14.4 path table, beat table, "fixed origin" deviation (section 3.3, 3.8, 4.5).
   - 14.5.6 whale, survey lamp keys and gaze (section 1.8); 14.5.7 lantern and KS21 (section 3.4); new beats 8 and 9
     (section 3).
   - 14.6 transitions 7 -> 8 -> 9 -> end (4.6); 14.7 H6 (1.5), H4 (3.7, 3.11), H3 additions (1.8).
   - 14.8 cut order (section 6); 14.10.1-14.10.4 (4.7-4.10); 14.11 ending (3.10); 14.12 constants (4.11).
4. **Unchanged**: the walked path s 0-1500 and every number derived from it (all T-PATH values of s <= 1500, KS01-KS16,
   KS18-KS20 poses), beats 1-5 geometry, H1/H2/H5/H7/H8, the dither, the title card.
5. **Conflicts between the two source designs and how they were resolved** (arithmetic in the named sections):

   | Topic | Design A (cinematic) | Design B (technical) | Decision |
   |---|---|---|---|
   | Ending vehicle | vertical glass cab on the face, walk 200 m out | inclined lift on a buttress fin, foot 3.4 km out | **inclined lift** (3.1): from 200 m the crest is at 88.1 deg and can never share a frame with the ground; from 3510 m it is at 59.7 deg |
   | Ride | 90 s, 3.5 m/s^2, 93 m/s | 165 s, 1.2 m/s^2, 60 m/s | **142.3 s, 1.5 m/s^2, jerk 1.0, 75 m/s** (3.5): comfort-class acceleration, 23 s shorter than B |
   | Ride in s | s advances at 2.5 during the ride | s frozen, second coordinate `ss` | **one coordinate**: the ride is a leg of `BwPath` whose s is driven by the car clock at 2.5 m/s at every walking speed (3.3) |
   | Foot path | loop around a terrace, lantern ahead | straight 160 m, camera turns back | **straight + 180 deg terrace loop**, lantern ahead, scripted tilt to the crest (3.9, 3.10) |
   | End climax | last light leaves the crest | crest disc fraction 0.81 frozen | **both**: KS28 frozen at 0.81, then the sun sinks and the crest goes dark at t_g 14.0 (3.10) |
   | First sighting | far "islet that moves" (1.46 km) | dark shadow 226 m away | **islet first** (K1), shadow later at the spillway gates (K3): distance falls monotonically |
   | Size ruler | arm beside the 80 m tower; head 622 m with eyes | arm 300 m at the gates; crown 1250 m | **arm vs tower** (K2, 455 m), **head rising behind the islets** at 598 m (K4) |
   | Beat 6 order | eye first, suckers later | suckers first, eye at pane 6, strike | **suckers on pane 5 (lamp A) -> eyes vanish behind the pier -> eye lit in pane 6 (lamp B) -> strike** (1.8) |
   | Eye | 20 m eye, 48 deg tall | 10 m glowing iris | **20 m eyeball, 12 m iris**: 1296 px at KS17 (60 % of the frame), 1728 px at the closest moment |
   | Phase keying | per appearance s keys | smax(progress, time at 1.5 m/s) + follower | **latched smax(progress, time at 1.5 m/s) on the crest; progress only + idle layer inside** (1.5) |
   | Visual stinger | cut (recommended) | optional crest drape, cut first | **not built**; sound-only stinger after the end text (1.9) |

---

## 1. The kraken

### 1.1 Look and creepiness rules

The user's references (`Logs/kraken/refs/`): (1) arms arching out of a dark sea, blood-red undersides with large pale
suckers, a mossy dome with small glowing eyes; (2) a crusted dome out of the water at night with a row of glowing
orange-red eyes in deep wrinkled sockets; (3) a towering animal with whip-thin tentacles many times its body, dwarfing a
bridge and helicopters; (4) under water, glowing orange eyes, sucker rows, a submarine for scale. Rules:

1. **Octopus-like, not a squid**: a bulbous, rock-like head/mantle; eight arms 6x the head height, tapering to whips.
2. **Skin**: mantle top crusted with barnacles (1-4 m, 20-30 % cover) and moss/algae (grey-green patches); 6-10 kelp
   streamers per side (20-60 m long, 2-4 m wide) hanging from the mantle rim, moved as damped strings by the current;
   dark blue-black to grey-green; a wet film (roughness 0.12 wet, 0.30 on crust) with water sheeting off emerged parts
   for 6-10 s; arms dark on top, **blood-red/rust underneath**; scars as lighter streaks.
3. **Eyes**: per side one main eye (20 m) in a deep wrinkled socket plus three lesser eyes (8, 6, 4 m) on an arc
   above-behind it; the 6 m one is milky and never glows. Emission orange-red (linear 1.0, 0.12, 0.02 = `#FF6127`),
   pulsing +-10 % at 0.2 Hz. Horizontal slit pupil in light, round in the dark; the pupil contracts in 1.2 s when a lamp
   lights the eye (only the lit eye reacts). Slow lid blink every 20-40 s (lid 1.2 s, hold 0.6 s, open 1.5 s).
4. **Suckers**: pale bone-pink (`#D8C2B0`) with darker rims, two alternating rows on the underside, each with a ring of
   10 inward-curved pale hooks. On glass they flatten and blanch.
5. **Never the whole animal**: the most ever in one frame is the head plus two arm arcs (K4, 598 m away). Close up only
   an eye, a socket, or a stretch of arm.
6. **Never static** (1.3): every visible part moves at least 0.3 m/s or 0.02 deg/s on screen; no two arms in phase.
7. **It watches**: once an eye is visible it tracks the camera (eyeball yaw/pitch at most 10 deg/s, 1.5 s latency) and
   the head turns toward the walker.
8. **Heavy and slow**: no motion faster than 8 m/s at an arm tip on screen; the body never jerks; huge things ease in
   and out (Part 1.2.5).
9. **No cartoon cues**: no teeth, no beak shown, no roaring mouth; horror comes from size, the eyes, the suckers, the
   hooks, the sound and its attention.
10. **Palette tokens** (append-only in `BulwarkPalette.cs`, kraken owner): `KrakenSkinDark #1E242B`, `KrakenMoss #3D4A3A`,
    `KrakenUnderRed #7A2418`, `KrakenSucker #D8C2B0`, `KrakenEyeGlow #FF6127`, `KrakenEyeMilky #B9C2BE`.

### 1.2 Scale and anatomy

Kraken local frame (as `Tools/kraken/kr_common.py`): origin at the centre of the arm crown, +Y up through the head,
+Z = forward (eyes side), `MODEL_SCALE` = 220 m per glTF unit. The table fixes the lead's numbers; the kraken builder
measures the eye anchors from the model's eye vertices and keeps them within 5 %.

| Part | Value | Note |
|---|---|---|
| Head/mantle height above the arm crown | 190 m (0.87 u) | "head ~220 m" class |
| Head length (front-back) / width | 320 m / 154 m | from the Tripo layout |
| Arm crown (web) radius | 66 m | |
| Eye centres | +88 m up, +-72 m lateral, +60 m forward | main eyes |
| Arms | 8, each **1300 m** from the crown centre | 5.9 x head height (6.8 x for the visible free arm) |
| Arm diameter, Tripo part | 44 m at u 55 (web), 35 at 110, 31 at 165, 24 at 220, **21 at the splice u 264** | u = arc length from the crown centre |
| Arm diameter, procedural part | `d(u) = max(0.6, 21 (1 - (u - 264)/1036)^1.43)`: 14.5 at 500, 12.0 at 600, 9.6 at 700, 5.4 at 900, 2.0 at 1100, 0.6 at the tip | loft along a GPU spline, textured from the Tripo tentacle |
| Suckers | diameter `0.28 d(u)` clamped 0.15-6.0 m (5.9 m near the root, 3.4 at u 600, 1.5 at u 900); pitch 1.15 diameters per row; about 1100-1200 per arm | instanced only within 200 m of the camera; normal-mapped beyond |
| Hooks | 10 per sucker, length 0.18 x sucker diameter (>= 0.05 m) | beat 6 only (geometry); a mask elsewhere |
| Main eye | eyeball 20 m, lid aperture 16 x 11 m, iris 12 m, pupil slit 8 x 1.6 m (round 7 m in the dark), socket wrinkle ring 6 m | |
| Lesser eyes | 8, 6 (milky), 4 m per side | |
| Eye emission | linear (1.0, 0.12, 0.02) x E; E = 4.0 in air, 6.0 under water (before water attenuation); HDR clamp 3.0 after exposure, bloom clamp 40 | |

Why the eye colour works under water (water model 14.12, c = (0.34, 0.085, 0.050)): through r metres of water the
red/green ratio is `8.33 exp(-0.255 r)`: 5.0 at 2 m (red-orange), 1.8 at 6 m (orange), 0.65 at 10 m (yellow), 0.18 at
15 m, 0.01 at 25 m (green). Far eyes read as green points that warm to orange as they approach; nothing else glows.

### 1.3 Motion vocabulary

| Motion | Rule |
|---|---|
| Reach | a bend travels base to tip with a bell-shaped speed profile (Gutfreund 1996); a full traverse takes 32-44 s (a 0.4 m octopus arm takes about 0.75 s; scaled by sqrt(length)); each arm has its own period (19-37 s idle cycle) |
| Slide | arms move along their own length like a rope (lengthwise drag is about 1/50 of the sideways drag); lateral sweeps at most 2 m/s |
| Curl | the distal 15 % coils into a spiral over 8-12 s and uncoils; one tip curls on its own while the rest is quiet |
| Twist | axial roll up to 120 deg over 10 s to show the red underside |
| Breath | mantle volume +-4 % with a 12 s period; each exhalation near the surface pushes a boil (Breach strength +0.2 for 3 s) |
| Passing cloud | dark bands sweep over mantle and arms at 25 m/s, 0.15 Hz, +-15 % albedo (How et al. 2017; below the 1 Hz photosensitivity limit) |
| Eyes | track the camera (<= 10 deg/s, 1.5 s latency), blink every 20-40 s, pupil round <-> slit in 1.2 s, glow +-10 % at 0.2 Hz |
| Sucker wave | rims open and close in a wave along the arm; on glass attach/release at about 2 per second |
| Water | sheeting 6-10 s off emerged surfaces, drip strings from arm undersides, foam collars at every waterline (SeaDisturbances) |

Speeds: visible parts 0.3-8 m/s; deep transit (top of the body >= 40 m below the surface, nothing published on the bus)
at most 25 m/s at 2.5 m/s walking (0.11 head lengths per second), with 15 s accelerate/decelerate ramps.

### 1.4 Habitat, the reef channel and the route

- The kraken lives east of the dam. Its body (the arm crown and head) **never enters the reef footprint**
  (z 704-1040 and z >= 1560 for x < 1600: the armour units stand on a shoal) except through a **deep channel at
  x 900-1100, z 704-1040** that this addendum adds to the reef: `ReefUnitAt` returns no unit for cells whose centre lies
  in the channel; `BwSea` shows open water there (beat 4 builder, Wave C). Arms may lie across the reef and drape over
  islets (clearance >= 2 m from unit hulls).
- **Never in the walkable corridor**: in beats 2-4 every kraken vertex stays >= 140 m east of the parapet plane x 6.0;
  in beat 5 >= 1 m outside the porthole outer faces; in beat 6 >= 0.3 m water-side of the outer glass plane
  (axis + 8.4 m) and never inside a pier.
- It moves north with the walker, from the open sea of beats 1-3 through the channel to the cove (z 1040-1560, open
  deep water) in front of the window gallery: the walker approaches its lair. Route of the arm-crown centre:

| s | Position (x, z) | Depth / pose | Leg speed at 2.5 m/s |
|---|---|---|---|
| 0-120 | (1600, 300) | deep, horizontal | - |
| 136-180 (K1) | (1400, 600) | upright, crown depth 200 -> 120 -> 220 m (head top -10 -> +70 -> -30) | rises in place |
| 180-352 | to (560, 400) | horizontal, body top >= 40 m deep | 12.5 m/s |
| 352-410 (K2) | (560, 400) | horizontal, body top 30-40 m deep; one arm out at (330, 690) | 0 |
| 410-458 | to (240, 380) | horizontal, deep | 17 m/s |
| 458-516 (K3) | (240, 380) -> (225, 560) -> (220, 620), heading 355 | body top 40 -> 15 -> 15 -> 60 m | 9.5 m/s |
| 516-744 | (220, 620) -> (1000, 680) -> channel -> (1000, 1080) -> (500, 1120) | horizontal, body top >= 60 m deep | 18.5 avg, 22 peak |
| 744-840 (K4, K5) | (500, 1120), facing 236 (the walker) | upright, crown depth 12 m at full rise (head top +178, eyes +76) | 0 |
| 840-990 | to (120, 1120) | sinks; horizontal 100 m deep | 3 m/s |
| 996-1020 (K6) | (120, 1100) | 100 m deep; one arm along the sea face past porthole 3 | 0 |
| 1100-1345 (K7) | the cove at the gallery: (120, 1150) -> (60, 1200) -> eye at the glass (1.8) | mantle-down (body pitched 50-70 deg, arm crown 10-20 m above the eyes, >= 15 m below the surface) | <= 3 m/s |
| > 1345 | (300, 1300) | 300 m deep, idling | - |

At 1.5 m/s every leg is 1.67x slower; the K-windows are progress-keyed (1.5).

### 1.5 Progress keying (H6 for the kraken)

`KrakenChoreography` is a pure function `Pose(s, t)`. Each appearance k has a window [s_a, s_b] and a phase:

- **Crest appearances K1-K5** (outdoors): `phi_k = latch( smax( sat((s - s_a)/(s_b - s_a)), sat(1.5 (t - t_a)/(s_b - s_a)) ) )`,
  `t_a` = first time s >= s_a, smax softness 0.02 (phase units), `latch` = never decreases. At 2.5 m/s the progress
  branch wins; at 1.5 m/s both coincide; a player who stops still sees the appearance complete at the 1.5 m/s rate,
  then the kraken goes deep and circles at radius 300 m, 4 m/s.
- **Interior appearances K6-K7** (porthole, window gallery): `phi_k = latch(sat((s - s_a)/(s_b - s_a)))`, progress only,
  so the encounter waits for the player (lamps are keyed to s as well). A time-driven idle layer (breath, eye tracking,
  sucker wave, curl, a 0.5 m/s drift along the current pose) keeps it alive when the player stops.
- No critically damped follower on the phase (it would shift live poses 6.25 m of s away from the captured ones).
  Transit legs are C1 (trapezoid speed profiles); teleports (`-progress`) re-evaluate the pure function.
- **Capture mode**: `phi_k = progress branch`, idle layers on the frozen ambient clock: every still is reproducible.
- **Tests**: the choreography root (excluding idle layers) at a given s is identical at 1.5 and 2.5 m/s within 1.0 m and
  1.0 deg; no root step > 0.5 m per 1/60 s (30 m/s).

### 1.6 Choreography table

All appearances keep the existing gaze holds except where a gaze change is listed (1.7, 1.8). "Visible" = what stands
above water or behind glass. Haze: beat 2-3 storm slab `T = exp(-6.565e-4 d)`, beat 4 `T = exp(-1.03e-4 d)`.

| ID | s window (t at 2.5 / 1.5) | Where (x, z), heading, depth | Pose and what is visible | SeaDisturbances published | Distance, T | Size 4K (1080p) |
|---|---|---|---|---|---|---|
| K0 | 62-80 (25.05-32.25 / 41.58-53.58) | (1600, 300), deep | nothing; heard through the gallery wall; lamps sway +-0.6 deg at s 66 **without** a wave boom | none | - | - |
| K1 "the islet that moves" | 136-180 (54.65-72.25 / 90.92-120.25); KS04 hold 148-176 | (1400, 600), heading 250, upright | the top of the mantle rises like an islet (head top -10 -> +70 m over s 136-152), breathes +-3 m over s 152-164, sinks over s 164-180; no eyes (they stay 32 m under). Visible fraction: top 37 % of the head height | Breach at the waterline: HalfAlong 75, HalfAcross 60, heading 250, strength smoothstep(height 0 -> 30 m), HeightAboveSea = height | bearing 72.5 from KS04, 1464 m, T 0.383 | +70 m: 2.74 deg = 92 px tall (46), 150 m wide = 198 px (99); KS04 rows 764-849, 7.5 deg left of centre; paler but taller than the real islets at bearings 33-40 (39-60 px) |
| Kf far slap (sound) | 236 (94.65 / 157.58) | (1100, 520) | not seen (the gaze is on the lowland) | Slap HalfAlong 20 (invisible from here) | 1.2 km behind-right | - |
| K2 "taller than the tower" | 352-410 (141.05-164.25 / 234.92-273.58); hold 364-392 | crown (560, 400), body 30-40 m deep; the arm leaves the water at **(330, 690)** | one arm rises 180 m out of the water over s 356-370 (bend travelling up), holds 370-392 with the tip curling, slides back down along its length 392-408 with water cascading. Waterline diameter 14.5 m (u 500), curl 6.4 m (u 850). Fraction of one arm: 27 % | TentacleRise at (330, 690), HalfAlong = HalfAcross = 7.25, heading = lean direction 315, HeightAboveSea 0 -> 180 -> 0 | bearing 46.7 at s 378, 455 m, T 0.742; tower 542 m dead ahead | arm 21.7 deg = 733 px (367) tall, 62 px (31) thick at the waterline, curl 27 px (14); tower 284 px (142): 2.6x taller than the 80 m tower at nearly the same distance |
| K3 "the shadow at the gates" | 458-516 (183.45-206.65 / 305.58-344.25); gate hold 470-500 | path (240, 380) -> (225, 560) -> (220, 620), heading 355 | nothing above water: an enormous dark mass with six writhing tendrils slides north under the storm sea in front of the gates, with a glassy slick, a 0.5 m swell and a V-wake; sinks 500-516. The sun break follows at s 520 (relief) | Shadow HalfAlong 160, HalfAcross 70, strength 1 at body-top depth 15 m, 0 at 60 m; Wake HalfAlong 110, HalfAcross 60 at the same centre, strength 0.6 x Shadow | bearings 56-120 (centre 102 -> 86 -> 75) from the gate eye, 230-310 m, T 0.84 | a band 50 px tall (25) at KS07 rows 952-1001, 60-90 deg wide |
| Kc far call (sound) | 655 (262.25 / 436.92) | in the channel (1000, 900) | not seen | none | 1.04 km | - |
| K4a "arm under the swell" (cut first) | 724-752 (289.85-301.05 / 482.92-501.58) | an arm along x 200 from z 860 to 930, sliding north 8 m/s | a dark ribbon just under the reef swell | Shadow HalfAlong 120, HalfAcross 8, heading 0 | 242 m, bearing 55 | ribbon about 50 px tall |
| K4 "the ruler" | 744-840 (297.85-336.25 / 496.25-560.25); reef hold 752-796 | crown (500, 1120) in the cove, facing 236, upright | 744-772: the dome rises behind islet (180, 905) to +150 m with water sheeting off; 772-782 the head reaches +178 and the closed eyes (+76 m) come over the islet's silhouette; 780-787.5 the lids open (3 s); 770-800 one arm rises north-east of islet (260, 830), arcs 80 m above the sea over the pile and drapes down its south-west side, twisting to show the red underside; a second arm rises and curls near the head (cut third); 812-840 the head sinks (unseen, gaze ahead). Visible: head 94 % of its height + 2 arm arcs | Breach (500, 1120) HalfAlong 90, HalfAcross 80, HeightAboveSea 0 -> 178; TentacleRise (300, 880) and (225, 790), r 8, heights 80; TentacleRise (430, 1150), r 10, height 120 (second arm) | head 598 m bearing 56.3 (from s 788), T 0.940; draped arm at the islet 260 m | head 14.8 deg wide = 499 px (250); above the islet silhouette (5.5 deg) to its top (15.7 deg): 344 px (172); eyes 20 m = 65 px (32) at 6.3 deg elevation, >= 1.0 deg above the islet silhouette, bloom >= 30 px; draped arm 15.8 m = 117 px (58), suckers 4.4 m = 33 px (16), arc top 15.1 deg |
| K5 "the slap" | 812-830 (325.05-332.25 / 541.58-553.58) | whip tip (u ~1050, d 2.6 m) of an arm from (500, 1120) | a whip rises 60 m out of the belt edge at **(160, 960)** (812-818), slaps down at s 820; a 50 m spray column behind the KS11 wall plume | TentacleRise (160, 960) r 1.3, height 60; then Slap HalfAlong 25 (fast attack 0.2 s, decay 4 s) | bearing 49.5 from KS11, 209 m, T 0.98 | column 13.6 deg = 445 px (222) |
| K6 "the porthole" | 996-1020 (398.65-408.25 / 664.25-680.25) | an arm (u ~600, d 12 m, suckers 3.4 m) sliding north at 1.5 m/s along the sea face past porthole 3 (landing 11, s 1008, eye depth 29.9 m) | the porthole's cyan disc and its cyan spill on the landing darken over 2.5 s (luma 98 -> <= 20), a pale sucker rim with hooks crosses the disc (s 1005-1009), then cyan again over 2.5 s; seen fully only by a player looking along the tube | none | 1-4 m beyond the porthole | one sucker fills the 0.6 m disc |
| K7 "the window gallery" | 1100-1345 (440.25-538.25 / 733.58-896.92) | section 1.8 | section 1.8 | none (nothing reaches the surface visibly) | 2-48 m behind 2.4 m of glass | iris 1296-1728 px |
| Stinger | t_g 32.5 after the goal | - | sound only (1.9) | - | - | - |

Gaps between kraken moments at 2.5 m/s: K0 -> K1 30 s, -> Kf 22 s, -> K2 46 s, -> K3 19 s, -> Kc 56 s, -> K4 36 s,
-> K5 0 s, -> K6 66 s (tower, atrium, stairs: compression), -> K7 32 s. The size is revealed in this order: an "islet"
paler and taller than the real islets (K1); an arm 2.6x the 80 m tower (K2); a body as long as the spillway under the
surface right in front of the gates (K3); a head 178 m tall whose eyes look over a pile of 40 m armour units, an arm
thicker than a unit's arm draped over the pile with its 1.1 m gauge rails (K4); a slap 209 m away (K5); one sucker filling
a porthole (K6); the light of the sky going out, suckers on a 20 x 36 m pane, an eye wider than your view (K7).

### 1.7 Gaze changes on the crest and stairs (cinematic; keys are world (s, yaw, pitch))

Peak rates are 1.875 x (change / key interval); all <= 18 deg/s yaw, 13 deg/s pitch.

- **Beat 2** (K2): replace `(360,0,3)` with `(348,0,3) (364,22,4) (392,22,4) (408,0,2)`; keep `(330,-12,1)` and `(420,0,2)`.
  Peak 6.4 deg/s. At yaw 22 the tower is at -21 deg and the arm at +25 deg: both inside 16:10 with the 3 % margin.
- **Beat 1** (K0), **beat 2** (K1), **beat 3** (K3), **beat 4** (K4, K5): existing holds unchanged
  (`(60,60,4)(72,60,4)`, `(150,80,-8)(166,80,-8)`, `(470,70,-6)(500,70,-6)`, `(752,78,-3)(796,78,-3)`,
  `(814,25,6)(832,25,6)`).
- **Beat 5** (K6): replace `(1000,3,-24)` with `(997,3,-24) (1006,33,-10) (1010,33,-10) (1019,3,-16)`; keep `(1034,3,-8)`.
  Peak 15.6 deg/s.

### 1.8 The window-gallery encounter (replaces the whale; 14.5.6 "Moving", lamps, gaze, KS17)

Geometry facts (14.4): walker axis `x(s) = 4.243 + 0.015009 (s - 1070)`, eye y = -54 - 0.12 (s - 1070) + 1.7, inner
glass at axis + 6.0, outer glass face at axis + 8.4; pane k centred at s 1085 + 30 k (20 m), pier k|k+1 at
s 1095 + 30 k .. 1105 + 30 k. Pane 5 = s 1225-1245, pier 5|6 = 1245-1255, pane 6 = 1255-1275, pier 6|7 = 1275-1285.
Survey lamps sit in the pier reveals at s 1220 (A, pier 4|5) and 1250 (B, pier 5|6). [calc: an eye centre 24.4 m out
at z 1268 is visible through pane 6 for walker z 1250.8-1277.3; eyes 33-48 m out at z 1262-1265 are visible through
pane 5 for walker z 1216-1242; between them the pier 5|6 hides them for about 9 m of s.]

**Timeline** (t from the hall entry at 2.5 m/s; sound IDs in section 2):

| s | t | Event |
|---|---|---|
| 1070-1100 | 0-12 | S5 hush, glass, veil, falling snow (unchanged). KS15 at 1115 unchanged |
| 1100-1124 | 12-21.6 | from s 1104 the snow drifts sideways (bulk flow 0 -> 0.3 m/s toward +z over s 1104-1112, decaying to 0 by s 1160: something huge displaces the water); far voice through the glass (KV-water far) |
| 1122-1160 | 20.8-36 | **the light goes out**: an arm crosses above panes 1-3 at 28-34 m depth (y -36..-42), 10-25 m beyond the glass, sliding south; the shafts die pane by pane (shaft occlusion from a kraken depth map, below), the upper veil dims 30-45 %, exposure does not compensate; light returns over s 1150-1165. Body swell (KB) and a low creak of the hall concrete |
| 1166 | 38.4 | **KS16**: the arm's underside as a soft dark band against the bright upper veil at yaw 55-64, pitch 54-58, slant 26-30 m (contrast G 0.08-0.12, B 0.22-0.29), a row of sucker bumps along its edge; about 3 s |
| 1176-1196 | 42-50 | breathing only (KB, 8 s swells); a long dark shape slides past far out in panes 3-4 |
| 1190-1245 | 48-70 | **suckers on the glass (pane 5)**: an arm (u ~600, d 12 m) rises diagonally out of the black and presses its underside flat on the outer face of pane 5, sliding up at 1-3 m/s; suckers attach and release in a travelling wave (about 2/s), rims blanch, hooks scrape. **Lamp A** clicks at s 1204, ramps to full by 1213, lights the arm with grazing light along the glass: the red underside, pale hooked suckers 3-4 m wide. The glass groans (creak rate 6 -> 30 /s) and the 4 % reflection wobbles +-0.05 deg at 0.6 Hz. From s 1236 the suckers release tip to base in 1.5 s (pops) and the arm peels away into the black; lamp A ramps off 1236-1245 |
| 1228-1241 | 63-68.4 | **eyes in the dark**: behind the arm, through pane 5, 3-5 dim **green** points of unequal size at 33-48 m approach and warm toward yellow; the underwater voice (KV-water close) begins at s 1228 |
| 1241-1250 | 68.4-72 | the pier 5|6 hides them: nothing; silence except breathing; lamp B relay clicks at s 1247 and starts its warm-up |
| 1251-1256 | 72.4-74.4 | **it is there, lit**: the eye reappears in pane 6 at **6 m off the outer glass** (centre x = axis + 24.4 = 31.6, z 1268, y -75), orange (R/G 1.8), the wrinkled socket, barnacles, kelp moving in the current; the pupil snaps from round to a horizontal slit in 1.2 s as lamp B reaches full (s 1256). Glass groan + close voice |
| 1268 | 79.2 | **KS17**: the eye abeam, 44.6 deg tall (iris 1296 px), piers at both frame edges, the rail and a 0.9 m bollard lamp in the foreground; its glow lights the snow amber within 7 m |
| 1272-1280 | 80.8-84 | slow blink at s 1272; the eye moves in to **2 m off the glass** (centre x = axis + 20.4): iris 1728 px (80 % of the frame), red-orange (R/G 5.0); the eyeball turns up to 40 deg to follow the walker who passes it |
| 1278-1287 | 83.2-86.8 | the pier 6|7 hides it from s 1278; lamp B ramps off 1278-1287 |
| 1287-1300 | 86.8-92 | the eye withdraws into the black (2 -> 30 m), fading to green, gone; 2 s of near silence |
| 1300 | 92 | **the strike**: an arm slams the dam face ahead (z 1330, y -70): a 24/31 Hz blow through the structure, dust falls from the ceiling joints in the bollard light, the snow swirls, the glass reflection wobbles, a glass crack is heard; **no crack is ever visible** |
| 1306-1345 | 94.4-110 | **KS18 (1320)**: below, the eye cluster sinks along a 30 deg slope from (34, -100, 1334) to (48, -125, 1340) and on to -160, amber -> green -> nothing by s 1338; the deepest groan (with its octave-below subharmonic) at s 1312, then silence |
| 1346-1370 | 110.4-120 | shafts turn amber; exit into the tunnel (unchanged) |

**Lamps** (replace 14.12 "Whale ... lamps"; ramps are log-linear in luminance so no area over 10 % of the frame changes
by over 15 % within 0.33 s; 9 m of s = 3.6 s at 2.5 m/s, 6.0 s at 1.5):

| Lamp | Mount | Aim | Click | Ramp up | Full | Ramp down |
|---|---|---|---|---|---|---|
| A | pier 4|5 reveal, s 1220 | yaw 20, pitch +5 (grazing along the outer face of pane 5) | 1204 | 1204-1213 | 1213-1236 | 1236-1245 |
| B | pier 5|6 reveal, s 1250 | yaw 45, pitch -8 (the eye hold at 24.8 m) | 1247 | 1247-1256 | 1256-1278 | 1278-1287 |

Cone half-angle 28 deg and range 45 m unchanged; both are `LocalLightPool` spots outside the glass (they light the
creature only: `BwCreature` reads them; `BwWaterPortal` adds their in-scatter).

**Gaze keys (beat 6, replace 14.5.6)**: `(1070,0,0) (1082,40,6) (1100,90,10) (1140,90,12) (1166,70,40) (1190,50,6)
(1214,58,4) (1232,66,4) (1246,58,2) (1256,64,2) (1268,90,2) (1278,112,0) (1292,100,-4) (1320,60,-24) (1348,25,-4)
(1370,0,0)`. Peak 10.3 deg/s yaw, 6.6 deg/s pitch. The keys at 1256-1278 follow the eye exactly (bearing 63.7, 89.6,
112.1 deg at s 1256, 1268, 1278).

**Glass interaction rules**: creature vertices clamped >= 0.3 m water-side of the outer plane; contact = sucker discs
flattened to the glass with blanched rims and a darker water film; the 4 % analytic reflection gets a +-0.05 deg
sway while anything presses or strikes; snow is pushed away from moving parts (bulk flow field, exact wrap preserved);
the creature is drawn at Geometry+20 behind the portal so no pixel shows inside a pier (T6).

**Shaft occlusion**: the kraken is rendered once per frame into a 512 x 512 orthographic depth map along the refracted
sun (n 1.33); each of the 12 shaft samples of H3 does one fetch; cost <= 0.15 ms at 4K. Fallback: a veil dimming keyed by
s (section 1.10).

**Body orientation in beat 6**: mantle-down (pitched 50-70 deg, mantle in the black below and behind), so the arm crown
stays 10-20 m above the eyes and >= 15 m below the surface; nothing breaks the surface during beat 6.

### 1.9 Stinger

After the end text, in black, at t_g 32.5 (3.10): one far groan rolling down the wall (KV-wall, low-passed at 140 Hz,
-46 dBFS RMS, 4 s plus a 4 s reverb tail). No picture. The visual crest-drape stinger was rejected: at 6.9 km slant it is
51 x 4 px, competes with the light leaving the crest, and implies the dam is breached, against the closing line.

### 1.10 Fallback tiers (14.8 rules)

| Tier | Kraken |
|---|---|
| Full | as specified |
| Simplified | no lesser eyes; no kelp; arms with 32 spline nodes; suckers instanced only in beat 6 (normal-mapped elsewhere); K4a and K6 cut; beat 6 shaft occlusion replaced by a veil dim of -25 % keyed by s 1122-1165; the strike as sound + dust only |
| Graybox | `KrakenProxy` (mantle ellipsoid + 8 x 8 capsules) on the same choreography, eyes as emissive spheres |

---

## 2. Sound design

All synthesised (`BwSynth`, `BwDsp`, 48 kHz), no audio files. Kraken events are pure functions of (s, t) from
`KrakenChoreography`, scheduled as `BulwarkScore` events so `BulwarkScore.Render(speed)` renders them offline (T-AUDIO).

### 2.1 Principles

1. **Sound before sight**: the voice arrives 3-8 s before each visual event (K1, K2, K4, K7 eyes).
2. **Physical delay for impacts**: slaps, cascades and water roar arrive d/343 s after the picture (K2 1.33 s, K4 1.75 s,
   K5 0.61 s, Kf 3.5 s). Under water and through concrete the delay is ignored (1500 m/s, metres away).
3. **Roughness = danger**: the voice carries 35-60 Hz amplitude modulation, the roughness band of screams (Arnal 2015),
   plus non-linear bursts (period doubling, chaos), which raise arousal.
4. **Infrasound with harmonics**: 17-19 Hz partials are always doubled by 34-76 Hz harmonics so laptops and phones still
   "hear" them; the 17-19 Hz unease evidence is tentative, the harmonics carry the effect.
5. **Silence is the setup**: the hall's S5 hush, the 9 m of silence behind the pier, the 2 s before the strike.

### 2.2 Distance model and spatialisation

- Gain `-20 log10(d / 50 m)` relative to the 50 m level, then the per-event calibration of 2.4 (art-directed levels win).
- Air low-pass `fc = max(250, 8000 / (1 + d/150))` Hz (743 Hz at 1.46 km, 1.6 kHz at 600 m, 3.4 kHz at 200 m).
- Pan by bearing relative to the camera yaw (equal power); sources behind get an extra 1-pole low-pass at 2.5 kHz and
  -2 dB. Wall variant: low-pass 200-250 Hz, no pan (from everywhere). Water variant (beat 6): low-pass 450 Hz into the
  hall FDN (5.5 s, HF ratio 0.35), pan by bearing.
- Reverb: outdoor events add a 0.7-0.9 s outdoor FDN at -12 dB; beat 6 events use the hall reverb.

### 2.3 Recipes

| ID | Sound | Synthesis | Band | Duration |
|---|---|---|---|---|
| KV | the voice | glottal pulse train, f0 rising 24 -> 30 Hz in 1 s then falling to 21 Hz by 7 s, jitter 6-8 %, shimmer +-3 dB, period doubling on 30 % of periods; three gliding formants (SVF band-pass, Q 6) "oo -> aa -> oo": F1 220 -> 420 -> 200, F2 560 -> 950 -> 480, F3 1150 -> 1500 -> 1000 Hz; roughness AM 40-60 Hz random walk, depth 0.35 -> 0.6 at the peak; at the peak a 1.5 s chaotic segment (+-30 % f0 jitter) with an octave-below subharmonic fading in; breath = pink noise through the same formants at -18 dB; envelope attack 1.5 s, sustain to 5 s, release 3 s. Variants: **KV-air** (distance model), **KV-wall** (LP 200-250 Hz, no pan), **KV-water** (LP 450 Hz, hall reverb, plus Minnaert bubble chirps 150 -> 600 Hz, 6-12/s, -20 dB) | 21-1500 Hz | 7-8 s |
| KB | body swell / dread bed | sines 19 (-6 dB), 38 (0), 57 (-6), 76 Hz (-12) + brown noise LP 50 Hz; 6 s swells (attack 3 s, release 3 s) | 19-80 Hz | event length |
| KW | water roar, cascades | brown LP 600 Hz + pink band-pass 1.8 kHz (Q 0.8), random 0.5-2 s swells, gain proportional to emerging speed x wetted height; a sheet rumble brown LP 120 Hz | 40-3000 Hz | 4-12 s |
| KC | creak, groan | stick-slip impulse trains accelerating 6 -> 30 /s (+-15 % jitter) into modal resonators: flesh 90/210/470 Hz (Q 8), glass 230/517/940/1460 Hz (Q 40), concrete 60/130/310 Hz (Q 12), steel 140/390/1100 Hz (Q 30) | 60-1500 Hz | 2-20 s |
| KS | sucker release "thock" | 3 ms click (LP 3 kHz) + 150 -> 70 Hz noise burst 35 ms + 70 Hz thump decaying 60 ms + wet hiss 250 ms (pink HP 2 kHz) | 60-6000 Hz | 0.3 s each, 0.8-2.5 /s in a travelling wave |
| KH | hook scrape | 20 ms grains of 600-2400 Hz band-passed noise, 25 /s | 600-2400 Hz | while sliding |
| KL | slap | sines 28 + 44 Hz decaying 2.5 s + 3 ms crack (pink HP 1 kHz) + spray hiss (pink BP 2 kHz, 1.5 s) + brown LP 300 Hz tail 3 s | 28-4000 Hz | 3-4 s |
| KX | the strike on the dam (beat 6) | sines 24 + 31 Hz decaying 4 s, brown LP 150 Hz rumble 3 s, concrete modes 60/130/310 Hz ringing 2 s, pane boom 31 Hz 1.8 s with glass ring 230/517 Hz 2.5 s, dust trickle (sparse 2-6 kHz grains, 4 s, -46 dBFS) | 24-6000 Hz | 4-5 s |
| KG | glass crack (heard, never seen) | 2 ms click through resonators 2.1 and 3.4 kHz (Q 30), decay 0.4 s, at KX + 0.4 s | 2-4 kHz | 0.4 s |
| KR | relay clicks (lamps A, B) | 2 ms clicks as 14.3 (-34 dBFS) | - | - |

### 2.4 Event schedule and levels

Levels are 3 s RMS at the listener unless "peak"; true peak always <= -1 dBTP.

| s | Event | Layers | Level |
|---|---|---|---|
| 62-70 | K0 groan through the gallery wall; lamps sway at 66 with no boom | KV-wall, KB | KV -42 (peak -30), KB -44 |
| 132-150 | K1 far voice (precedes the rise at 140) | KV-air (1464 m, fc 743 Hz, pan by bearing 72.5) | -34 |
| 164-182 | K1 sinking swell | KB + KW (low) | -38 / -40 |
| 236 (+3.5 s) | Kf far slap behind the walker | KL (1.2 km, behind) | -36 |
| 346-360 | K2 voice (from the body, 600 m) | KV-air | -30 |
| 356-380 (+1.33 s) | K2 rise: water cascading, creak of the arm | KW, KC flesh | -32 / -40 |
| 392-410 (+1.33 s) | K2 slide-back | KW | -34 |
| 452-516 | K3 under the sea: infrasonic bed and a muffled underwater voice; F and S duck 6 dB | KB, KV (LP 300 Hz) | -30 / -30 |
| 655 | Kc far call | KV-air (1 km) | -38 |
| 738-760 | K4 voice (arrives before the rise) | KV-air (600 m) | -27 |
| 744-786 (+1.75 s) | K4 water sheeting off the head and arm | KW, KB | -28 / -36 |
| 820 (+0.61 s) | K5 slap | KL (209 m) | peak -8, RMS -22 |
| 1000-1012 | K6 scrape and thocks through the stair wall | KH, KS (LP 800 Hz) | -40 / -36 |
| 1100-1124 | K7 far voice through the glass | KV-water (LP 200 Hz) | -44 |
| 1122-1165 | K7 the light goes out | KB, KC concrete | -40 / -42 |
| 1176-1196 | breathing | KB (8 s swells) | -40 |
| 1190-1245 | suckers on the glass | KS, KH, KC glass (6 -> 30 /s), KR at 1204 | -34 / -42 / -36 -> -32 |
| 1228-1240 | the voice under water (precedes the green points) | KV-water close | -28 |
| 1247 | relay B | KR | peak -34 |
| 1251-1280 | the lit eye: glass groan, breath | KC glass, KB, KV-water (second call at 1262) | -34 / -40 / -26 |
| 1300 | the strike + crack | KX, KG | KX peak <= -3 before the limiter, RMS -20; KG peak -12 |
| 1312 | the deepest groan | KV-water with the subharmonic | -30 |
| t_g 32.5 | stinger | KV-wall (LP 140 Hz) | -46 |

### 2.5 Levels against 14.3 (event windows)

Outside the windows below the 14.3 targets hold (+-1.5 dB, same per s at both speeds). Inside a window the 3 s RMS may
rise to the ceiling; the per-s comparison between speeds is replaced by event peak levels within +-1 dB.

| Beat (target) | Window s | Ceiling (3 s RMS) |
|---|---|---|
| 1 (-34) | 62-82 | -32 |
| 2 (-24) | 132-184, 236-244, 344-414 | -22.5, -23, -21 |
| 3 (-18) | 452-520 (F, S ducked 6 dB inside), 652-664 | -16.5, -17 |
| 4 (-23) | 736-842 | -19 |
| 5 (-32) | 994-1022 | -29 |
| 6 (-38) | 1100-1186, 1186-1250, 1250-1296, 1296-1312, 1312-1345 | -35, -30, -26, -19, -31 |
| 7 (-25 -> -35) | none | - |
| 8 Descent (new) | target -40 at rest and doors (s 1500-1510, 1866-1880), -33.5 at cruise (motor -36, air -38, joints -46, hum -50: power sum -33.5) | +-1.5 |
| 9 Foot (new) | target -46 (wind -48, jets -52, crickets -56: power sum -46.1); car departure 1880-1905 | -40 |
| End | fade with the picture; stinger t_g 32.5-40.5 | -44 |

### 2.6 Ducking

Other layers (W, S, B, F, P, ride, foot) duck 6 dB starting 1.5 s before every KV close call (K4, K7 at 1228, 1262,
1312), KL at K5 and KX, and release over 3 s. In beat 6 P and B duck 4 dB for the whole of s 1190-1300. The pause duck
(12 dB, 0.4 s) applies to the kraken bus too.

### 2.7 Infrasound, peaks, steps

- All partials >= 17 Hz (the master 20 Hz DC blocker costs about 4 dB at 17 Hz). A 2nd-order high-pass at 15 Hz on the
  kraken bus. Content below 20 Hz <= -30 dBFS RMS in any 3 s window and never louder than its audible harmonics.
- The kraken bus has its own look-ahead limiter (5 ms, ceiling -6 dBFS) before the master limiter (asymptote -1.42 dBFS);
  true peak measured with 4x oversampling <= -1.0 dBTP.
- Sample step <= 0.25 except KG, the KX crack and the relay clicks (each <= 0.5, listed in `BwAudioSpec`).

### 2.8 Ride and foot

| ID | Sound | Synthesis | Level |
|---|---|---|---|
| RD | doors close/open | 45 Hz thud 0.4 s + latch click; seal hiss 1.0 s (pink HP 3 kHz) | peak -24 |
| RM | linear motor | whine at f = v / 0.5 m pole pitch (0 -> 150 Hz at 75 m/s) + 2nd and 3rd harmonics; gain 0.3 + 0.7 |a| / 1.5 | -36 at cruise |
| RA | air rush | pink BP 400 Hz (Q 0.7), gain proportional to (v / 75)^2 | -38 at cruise |
| RJ | track joints | 2 ms clicks LP 2 kHz every 24 m of travel (3.1 Hz at cruise) | -46 |
| RC | cabin | FDN 0.4 s; the tungsten lamp hum 100/200 Hz | -50 |
| RS | sunset hush | from tau 27 to 64 every ride layer except RA drops 6 dB | - |
| RP | arrival | latch clunk, 1.8 s pressure hiss, "ear pop" = master LP 1.2 kHz opening to full over 0.4 s at tau 142.5 | peak -26 |
| FW, FJ, FC | foot | wind pink BP 380 Hz (-48), distant jets brown LP 250 Hz (-52), crickets: 4.6 kHz chirps (3 x 30 ms every 1.2-3 s, 4-6 individuals, -56) | -46 total |
| FU | the car leaves upward | pink BP 300 -> 900 Hz rising over 20 s, panned behind, -40 -> -60 | -40 |
| - | lanterns | silent (14.3 S8 kept) | - |

---

## 3. The ending

### 3.1 Decision: an inclined express lift on a buttress fin

The user's order: descend to the foot of the dam, walk away to the lights, turn back and see the true size. The crest
elevation seen from the ground at distance D is `atan(6000 / D)` [calc]: 88.1 deg at 200 m, 85.2 at 500 m, 80.5 at 1 km,
71.6 at 2 km, 59.7 at 3.51 km. With vFOV 66 the whole wall and the ground fit in one level frame (horizon above the
frame bottom, crest below the top with a 3 % margin) only for D >= 3460 m. Walking 3.5 km takes 23 minutes, so the
distance must come from the descent: a lift that runs **down a buttress fin at 60.16 deg** delivers the walker 3390 m
from the face. A vertical cab can only offer a tilt up a wall that has no top. Engineering precedents (real, smaller):
the Krasnoyarsk dam's inclined ship lift (a rack railway with a 9 m gauge carrying ships over the dam), Marcel Breuer's
glass inclined elevator running down the face of Grand Coulee's third powerplant, the Stoosbahn funicular (47.7 deg,
10 m/s, level cylindrical cabins), the Shanghai Tower lift (20.5 m/s record). 75 m/s is 3.7x the record: engineering
pushed to the limit; log it as the user-ordered exception.

### 3.2 Structure

| Beat | Name | s | Length | What happens |
|---|---|---|---|---|
| 7 | Sluice tunnel and lee ledge (changed) | 1370-1500 | 130 | unchanged walk; the lantern is gone from the ledge; the ledge ends at the lift car standing on the fin crown deck (the old lantern spot); the walker stops in the car at s 1500 |
| 8 | Descent (new) | 1500-1880 | 380 of s (152.0 s at every speed) | doors close, the car plunges down the fin: the plunge, the sunset made by the descent, the lit wall above, the lanterns below, arrival |
| 9 | Foot (new) | 1880-2040 | 160 | out of the car, west along a causeway of lanterns, a 180 deg loop round a terrace, the Keeper's Lantern ahead, the dam behind it; at the goal the camera tilts up the wall to the crest as the last light leaves it |

### 3.3 Path (BwPath changes; lead)

- **s 0-1500 unchanged** (geometry, lanes, T-PATH values). The walker comes to rest at s 1500 at (-9.199, -90 floor,
  1497.114) with the existing stop ramp (0.5 s, loses 0.25 s).
- **Ride leg s 1500.000-1880.000**: `BwPath.World(s)` is the car reference point (the walker's foot point); s advances
  at **2.5 m/s of s driven by the car clock at every walking speed**; movement input is ignored (mouse look free,
  Part 1.2.7); lane = [0, 0]; `EyeY(s) = FloorY(s) + 1.7` exactly (no kernel). With `tauL = (s - 1500) / 2.5`:
  doors close tauL 0-3 (s 1500-1507.5), seal 3-4 (to 1510), motion tau = tauL - 4 from 0 to T_R = 142.343
  (s 1510-1865.858), settle to tauL 149 (s 1872.5), doors open 149-152 (s 1880). Plan point
  `P = (-9.199, -90, 1497.114) + D(tau) (-3390, -5910, 0) / 6813.237`, `D(tau)` = the profile of 3.5.
  `HeadingDeg` = 0 in the leg (the walker faces north as he entered).
- **Foot leg s 1880.000-2040.000** (a second `FilletPath`): from (-3399.199, 1497.114) heading 270, straight 122.584 m
  (to s 2002.584), arc r 10 right 270 -> 0 (15.708 m, to 2018.292), arc r 10 right 0 -> 90 (15.708 m, to 2034.000),
  straight 6.000 m to the rest point **(-3515.783, 1517.114)**. Corners (-3531.783, 1497.114) and (-3531.783, 1517.114),
  both r 10, with a zero-length straight between them (FilletPath must accept it); both arcs share the centre
  (-3521.783, 1507.114). Floor y -6000, eye -5998.3.
- **Lanes**: car west door 4.0 m (lateral +-1.65) for s 1880-1886, smootherstep to the causeway +-3.65 by s 1896
  (edge slope <= 0.6 m/m), +-3.65 on the causeway and the loop, end cap at s 2040.
- `PATH_LENGTH = 2040.000`; `BEAT_S = 0, 120, 420, 700, 920, 1070, 1370, 1500, 1880, 2040`; goal trigger **s 2038.5**;
  walked plan length 1660.000; `CinematicLateral` 0 beyond s 1500.

### 3.4 The lift: fin, track, car, stations

- **Fin** (a gravity buttress of the lee face): centre plane z 1497.114; flat crown deck at y -90 from x -6.0 to -14.2,
  40 m thick (z 1477.114-1517.114), merging with the north end of the ledge slab; ridge from (-14.2, -90) to
  (-3404.2, -6000); half-thickness `w(y) = 20 + 100 (-90 - y) / 5910` (240 m at the base: base/height 0.57); concrete
  `ConcreteDark` with formwork, gallery lines continued; a lit service stair line along its north flank (lamps every
  24 m, `SodiumWarm` x1.5, soft glows >= 1.5 px).
- **Track**: two rails on the ridge, 6 m gauge, parallel to the motion line, rail top 3.0 m below the car floor line;
  rails continuous (no periodic fixings shorter than 6 m: at 75 m/s and 60 fps the car moves 1.25 m per frame, so a
  period under 2.5 m would strobe); the ridge surface in 24 m modules (formwork 3.0 / 1.5 / 0.6 m `fwidth`-faded).
- **Car**: interior 7.3 m along x (x -13.85..-6.55 at the top, 0.55 m clear of the face plane x -6.0; the walker
  stands 4.65 m from the west wall, 2.65 m from the east wall) x 6.0 m (z 1494.114-1500.114) x 3.2 m; level floor;
  glazing on the west, north and south walls and the roof (`BwGlass`, 4 % analytic reflection), a 1.2 m glass floor
  strip along the west wall, 1.1 m handrail, mullions 0.12 m, one tungsten ceiling lamp (Tungsten x1.2, a
  `LocalLightPool` anchor); south door (boarding) and west door (exit), 4.0 x 2.4 m; about 15 k triangles. The east
  wall is solid; the look back up goes through the roof glass.
- **Top station**: the fin crown deck with a low glass canopy (3.6 m, no walls) over the car; the ledge rail continues
  round the deck. The car's warm interior replaces the old lantern as the destination glow in KS20 (column 2792); the
  north vanishing point stays readable through the car glazing.
- **Bottom station**: open canopy 30 (x) x 24 (z) x 10 m over x -3396..-3426, plaza to x -3440, warm lamps.
- **The empty car returns up**: 8 s after the walker steps out it departs upward (FU); its lit interior is a moving warm
  point climbing the fin line during the turn-back and the end hold (min 1.5 px).

### 3.5 Ride profile

Track 3390 m horizontal x 5910 m vertical: L = 6813.237 m at 60.161 deg. Jerk-limited trapezoid: v_max 75 m/s
(270 km/h), a_max 1.5 m/s^2 (0.15 g, comfort class), jerk 1.0 m/s^3. `T_R = L/v + v/a + a/j = 90.843 + 50.0 + 1.5 =
142.343 s`. Acceleration phase 51.5 s (1931.3 m), cruise 39.343 s, braking from tau 90.843 (altitude 1677 m). At
cruise: 65.0 m/s down, 37.3 m/s west. The car floor never rolls or pitches.

### 3.6 Ride timeline and gaze (tau = motion time, s = 1510 + 2.5 tau)

| tau | s | Altitude (eye) | Event | Gaze key (yaw, pitch) |
|---|---|---|---|---|
| -4 (tauL 0) | 1500 | 5911.7 | doors close behind; cabin hum; reverb ledge 0.5 -> cabin 0.4 over 2 s, hard edge at the seal | (0, -1) |
| 0 | 1510 | 5911.7 | launch (thump, motor rising) | |
| 8 | 1530 | 5877 | **KS22 ride_drop**: down the track, rails converging 6 km down, the lowland 5.9 km below | (270, -30) |
| 20 | 1560 | 5671 | | (270, -30) |
| 26.9 | 1577.3 | 5466 | the sun's lower limb touches the horizon: the descent is setting the sun | |
| 28 | 1580 | 5440 | | (300, -3) |
| 29.9 | 1584.8 | 5360 | first origin shift (y) | |
| 49.15 | 1632.9 | 4388 | **KS23 ride_sunset**: sun centre on the horizon; the car crosses into the Earth's shadow: the face beside it goes from amber to violet while the wall above stays lit | (298, -4) at 50 |
| 64.0 | 1670 | 3421 | the upper limb is gone | (296, -5) |
| 82.8 | 1717 | 2200 | the car passes the level of the mist rivers (900-2200 m): clouds at eye level | |
| 84-100 | 1720-1760 | 2150-1130 | **KS24 ride_lookback (tau 92)**: through the roof, the wall above, the lit band from 4584 m up (alpenglow), the tiny top station at the crest line, the fin ridge rising | (90, +38) |
| 90.8 | 1737.1 | 1677 | braking begins | |
| 104.4 | 1771 | 900 | below the mist level | |
| 120 | 1810 | 305 | | (270, -40) |
| 122 | 1815 | 252 | **KS25 ride_lanterns**: the causeway lanterns and the Keeper's Lantern 310-370 m below, the station canopy | (270, -40) to 130 |
| 142.343 | 1865.9 | 1.7 | arrival: latch, hiss, ear pop (RP) | (270, -8) at 142 |
| tauL 149-152 | 1872.5-1880 | - | west doors open | |

Turns: 0 -> 270 over 12 s (14.1 deg/s), 296 -> 450 over 20 s (14.4), 90 -> 270 via south over 20 s (16.9); pitch
peaks 7.3 deg/s. The full wall is never framed during the ride (the look back shows only the wall above, its top at
60.6 deg): the whole dam is saved for KS28.

### 3.7 Sun, Earth shadow, sky and haze

- Horizon dip `acos(R / (R + h))` (geometric, as BwFarField; refraction ignored): 2.467 deg at the ledge.
- **Sun** az 305 throughout. Elevation: beat 7 unchanged to -2.10 at s 1500; ride `el = -2.10 - 0.10 F` (F = travelled
  fraction of L); foot `el = -2.20 - 0.15 (s - 1880) / 158.5` (-2.35 at the goal); after the goal
  `el = -2.35 - 0.47 smootherstep((t_g - 9) / 7)` (-2.82 at t_g 16).
- **Sunset from the car** where dip(h) = |el|: lower limb touches at tau 26.9 (5466 m), centre at tau 49.15 (4388 m),
  upper limb gone at tau 64.0 (3421 m).
- **Lit fraction** of a point at altitude h = the fraction of the 0.53 deg disc above its own horizon. Shadow-line
  altitude (centre) `h = R (1/cos(el) - 1)`: 4386 m at -2.126, 4584 m at -2.173, 5363 m at -2.35; the penumbra is about
  2.4 km tall (the alpenglow effect: the high wall stays lit by sunlight grazing the ground far to the west). Crest
  fraction 0.81 at el -2.35; the crest goes dark at el -2.752, i.e. **t_g 14.0**.
- **Alpenglow colour** on the face: `KeyB7 #FF9A4A x 0.9 x fraction x exp(-k (M(h_t) - M_crest))`, k = (0.12, 0.19, 0.32),
  `M = 38 exp(-h_t / 8400)`, `h_t = h - R el^2 / 2` (the tangent-point altitude of the sun ray).
- **Haze** `s0`: 4.5e-5 to F 0.5, smootherstep to **2.0e-4** at F 1.0, 2.0e-4 at the foot. At KS28 the wall base has
  T 0.496 and the slant to the crest 0.644: the foot of the wall dissolves, the rim stays crisp.
- **Mist rivers**: the slab (900-2200 m) is rendered as a volume (entry/exit of the two shells, 8 samples x the plan
  mask) so it is correct from inside and below. Plan masks keep out of the KS28 sight wedge: no mist river within
  x > -3600, |z - 1517| < 3000 except the spillway plume (bearing 105 from the foot).
- **City lights** `N(s)` stays 1 after s 1475.

### 3.8 Floating origin (`BwFrame`, lead)

- **Why**: URP has no camera-relative rendering; at |x| ~ 6000 m float32 steps are 0.49 mm and camera and vertices round
  independently: about 2 mrad at the car mullions 0.5 m away, 3.5 px of jitter at 4K (Part 10 #17).
- **Rule**: per axis x and y (z never shifts), when `|cam_true - O| > 640 m`, set `O = 768 round(cam_true / 768)`. O = 0
  until the doors are sealed (s 1507.5), so beats 1-7 stay bit-identical. Shifts happen at tau 29.9, 45.8, 57.8, 69.6,
  81.5, 93.3, 107.6 (y) and 41.9, 63.2, 83.8, 106.7 (x): 11 shifts. At the foot **O = (-3072, -5376, 0)**; the Unity eye
  at the rest point is about (-443.8, -622.3, 1517.1) (step 61 um).
- **Execution**: one `LateUpdate` at order 90 (after `PlayerController` -30 and `PathGuide` -20, before `BackdropRig` 150
  and `ProgressDirector` 500). Shifted: every root registered with `BwFrame` (Ledge, Tunnel7 until off, Descent, Foot,
  kraken root if enabled), the player rig (CharacterController disabled, moved, enabled), `PathGuide` colliders,
  `LocalLightPool` anchors, `InstancedBatch` / `InstancedField` matrices and bounds, `BackdropRig`, capture poses
  (`ShotPlan` works in true coordinates and converts through `BwFrame.ToUnity`).
- **Shader globals**: `_BwOriginTrue` (float3, exact multiples of 768). Altitude for fog, far field and sun visibility =
  `6000 + positionWS.y + _BwOriginTrue.y`. Pattern lookups use `BwPatternPos = positionWS + _BwOriginTrue` (true
  coordinates: no seam at a shift, Part 10 #33); footprints come from `ddx/ddy(positionWS)`, never from the sum.

### 3.9 The foot

- **Ground**: `BwGround` near disc, radius 800 m, using BwFarField's own field function (fields, macro tone); the
  curvature drop at its rim is 5 cm (0.1 px). Beyond: BwFarField with the true eye.
- **Causeway**: 8 m wide, z 1493.114-1501.114, from the plaza (x -3440) to the terrace rim (x -3505.8), gravel with a
  0.6 m kerb on each side; **five lanterns** on the kerb tops at x -3445, -3460, -3475, -3490, -3505, alternating
  z 1501.4 / 1492.8: the old lantern model (iron post 1.4 m, glass drum 0.30, cage) at Lantern x2.0.
- **Terrace**: circular slab radius 16 m centred (-3521.783, 1507.114), 0.6 m kerb, a central stone plinth radius 5 m,
  0.9 m high (a scale cue inside the loop).
- **The Keeper's Lantern** (the goal object of 14.11, moved here): (-3513.883, -6000, 1517.114), post 1.4 m, glass 0.30,
  Lantern x6, `LocalLightPool` point 5 m / 2.0, flicker 3 % (0.31 / 0.77 / 0.97 Hz). The walker rests 1.9 m short of it,
  facing east with the dam beyond it.
- **Light**: sky ambient (dusk), the lanterns, the canopy lamps; no sun key at the ground (it has set).
- **Gaze** (cinematic): `(1880,270,-4) (1892,270,0) (1990,270,1) (2038.5,90,0)` (yaw 270 -> 450 through north: 180 deg
  over 48.5 m = 19.4 s, peak 17.4 deg/s). For 44 s the walker looks west at the lanterns and the afterglow: the
  compression before the release; the wall swings into view during the loop, its base across the frame, its top out of
  it.

### 3.10 The look-back money shot and the end sequence (replaces 14.11 "Ending")

The goal trigger is s >= 2038.5 (`t_g` = time since the trigger). In both modes the input freezes at the trigger
(as the 3.0 s dwell of 14.11 already did) and the end camera runs:

| t_g | What |
|---|---|
| 0-1.5 | the walker comes to rest (stop ramp); the camera eases to (90, 0) (in manual mode at <= 18 deg/s, the sequence waits until it arrives) |
| 1.5 | **KS27 keeper**: the lantern in the lower middle (glass centre row 1474), the base of the wall across the whole frame, the wall rising out of the top edge |
| 1.5-8.5 | tilt (90, 0) -> (90, +29), smootherstep, peak 7.8 deg/s: the lantern drops out of the bottom, the light rows sink through the frame, the lit band and the crest enter at the top |
| 8.5 | **KS28 dam_reveal**: crest a straight horizontal line at row 94, horizon row 2003 (the wall fills 88 % of the frame height), the alpenglow band (half-lit line at 5363 m, row 204), the fin ridge rising from the station to row 106, jets at bearings 84, 71, 56, 42, the spillway plume at the right edge, the climbing car light |
| 9.0-16.0 | the sun sinks (3.7): the amber band slides up the wall and the crest goes dark at t_g 14.0; the light rows and the station stay lit |
| 16.0-20.0 | picture and audio fade to black (smootherstep); the lantern flickers until the fade ends |
| 20.0-21.5 | black |
| 21.5-31.5 | the line **"Someone held the sea back, so the land could stay dry."** (14.11 typography): in 2.0, hold 6.0, out 2.0 |
| 32.5-40.5 | stinger (1.9) |
| 41.0 | ending done; the game stays on black with Esc available |

### 3.11 Far-field additions (BwFarField, dusk owner)

- The fin as an analytic 3-plane wedge beyond 400 m from the camera (crossover with the near fin: dE <= 2, Part 10 #34).
- **Inspection-gallery lines** on the lee face every 250 m of altitude (23 rows plus the crest): faint horizontal
  lines (min 1.5 px, energy-preserving), spacing 137 px near the base to 44 px near the top at KS28.
- **Gallery lamps**: `SodiumWarm` glows every 60 m along every gallery row, an energy-preserving density field (never a
  point), switched on with N(s); each glow >= 1.5 px.
- The returning car light on the fin line; the bottom station canopy glow as a proxy beyond 1.5 km.
- `BwFarField` accepts any true eye position (altitude 0-6000 m, x down to -3600) through `_BwOriginTrue`.

### 3.12 Near geometry, sets, pre-warm

- Ride: near = the car, the fin and track treadmill modules within +-240 m (24 m period), the Ledge/top-station set
  (frustum-culled, swapped to a far-field proxy once >= 1.5 km away, at about tau 35). Sets off at s 1507.5 (doors sealed):
  `Sea` (the dam hides the sea from the whole lee side), `Crest`, `Spillway`, `Reef`, `Interior5`, `Hall6`, kraken root;
  `Tunnel7` off at the proxy swap.
- Foot: the ground disc, the station, causeway, lanterns, terrace, the last 400 m of fin and track.
- Pre-warm (14.6) adds `Descent` (car poses tau 0 and tau 120) and `Foot` (KS28 pose) and the kraken at its K4 and
  KS17 poses, behind the black load fade.

---

## 4. Changes to the Part 14 tables

### 4.1 14.0 Digest (replace the affected bullets)

- **Game**: ... the walk ends at a ledge where an express lift runs down a buttress fin to the foot of the dam; a short
  walk to a lantern; turning back shows the whole 6 km wall. Path s 0-2040: walked 1660 m plus a 380 m ride leg whose s
  the car drives at 2.5 m/s. Goal trigger s 2038.5 at 816.15 s (2.5 m/s) / 1258.42 s (1.5 m/s).
- **Beats**: 1-7 unchanged; 8 Descent s 1500-1880 (152 s at every speed); 9 Foot s 1880-2040 (64 s at 2.5).
- **Money shots**: (1) KS03/KS04 unchanged; (2) KS08 unchanged; (3) **KS17 the kraken's eye at the glass** (KS15-KS18);
  (4) KS20 lee ledge at dusk with the lift car as the destination glow; (5) **KS28 the whole dam from its foot**.
  Kraken key moments: KS04 islet, KK1 arm vs tower, KS10/KK2 head over the islets.
- **Constants**: vFOV 66 in beats 8-9; far plane 12000; dither 2.0 in 8-9; `BwFrame` origin step 768 m after s 1507.5.
- **Cut order**: section 6.

### 4.2 14.1 Arc and runtime

| Beat | Emotion | Intensity |
|---|---|---|
| 1 Wave gallery | dread; something answers the waves | 3 -> 5 |
| 2 Crest | vertigo, wonder, a wrong islet, an arm taller than the tower | 5 -> 8 |
| 3 Spillway bridge | a shadow under the gates, then awe, relief, gold | 8 -> 9 |
| 4 Armour slope | the head over the islets: it looks at you | 7 -> 9 |
| 5 Tower and descent | calm, the porthole goes dark | 3 -> 5 |
| 6 Window gallery | pressure, silence, the eye, the strike | 6 -> 10 |
| 7 Tunnel and ledge | thunder, release, peace | 6 -> 9 -> 5 |
| 8 Descent | vertigo, the sun set by the fall, the wall above | 5 -> 7 |
| 9 Foot | smallness, then the whole wall | 3 -> 10 -> 2 |

| Beat | Length (m of s) | Cum. t at 1.5 | Cum. t at 2.5 |
|---|---|---|---|
| 1-7 | 1500 | 1000.50 (at rest in the car) | 600.50 |
| 8 | 380 | 1152.50 | 752.50 |
| 9 | 160 (trigger at 158.5) | 1258.42 (trigger) | 816.15 (trigger) |
| End | +41.0 s | 1299.42 | 857.15 |

Compress-release pairs: 1 -> 2, 5 -> 6, 7 tunnel -> ledge, 9 causeway -> the turn back.

### 4.3 14.2.3 / 14.2.4 / 14.2.5 rows (added)

| Beat | Sun az / el | Key | Haze s0 | Storm | Sky | vFOV | EV | Bloom thr / int / scatter | Sat | Cont | WB | Vig | Dither |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 8 | 305 / -2.10 - 0.10 F | #FF9A4A x0.9 x disc fraction at the car, no shadows | 4.5e-5 -> 2.0e-4 (F 0.5-1.0) | 0 | dusk | 66 | +0.3 -> +0.9 (F 0.2-0.7) -> +1.1 (F 1.0) | 0.9 / 0.30 / 0.75 | +8 | 0 | +4 | 0.14 | 2.0 |
| 9 | 305 / -2.20 - 0.15 u (u = (s-1880)/158.5), then 3.7 | none at the ground; alpenglow on the face only | 2.0e-4 | 0 | dusk | 66 | +1.1 | 0.9 / 0.30 / 0.75 | +8 | 0 | +4 | 0.14 | 2.0 |

EV slopes: the ride's EV rise is 0.6 EV over 52 s (0.012 EV/s). Rule P: the beat 7 values at s 1500 equal the beat 8
start values (no crossfade needed). Interior ambient of the car: Tungsten x 0.06.

### 4.4 14.3 Sound bible (changes)

- Row V (whale call) is replaced by the kraken events of 2.3-2.4; S6 becomes "s 1228 the voice under the glass"; S8:
  "fade with the picture at t_g 16; the stinger at t_g 32.5; the lantern is silent".
- New rows: ride R* (2.8) in beat 8, foot F* in beat 9.
- Master: true peak <= -1 dBTP (4x oversampled); 3 s RMS targets B1 -34, B2 -24, B3 -18, B4 -23, B5 -32, B6 -38,
  B7 -25 -> -35, **B8 -40 / -33.5 / -40, B9 -46**, with the event windows of 2.5; reverb: cabin 0.4 s, station canopy
  0.8 s, foot 0.5 s.

### 4.5 14.4 Layout and timeline (changes)

Path table, appended rows:

| Segment | s start | s end | Length | Heading | End (x, z) | Floor y |
|---|---|---|---|---|---|---|
| Ledge to the car (was "to the lantern") | 1436.321 | 1500 | 63.679 | 0 | (-9.199, 1497.114) | -90 |
| R ride leg (car clock) | 1500 | 1880 | 380 (s) | 0 (fixed) | (-3399.199, 1497.114) | -90 - 5910 F |
| F1 causeway | 1880 | 2002.584 | 122.584 | 270 | (-3521.783, 1497.114) | -6000 |
| F2 arc r 10 | 2002.584 | 2018.292 | 15.708 | 270 -> 0 | (-3531.783, 1507.114) | -6000 |
| F3 arc r 10 | 2018.292 | 2034.000 | 15.708 | 0 -> 90 | (-3521.783, 1517.114) | -6000 |
| F4 to the lantern | 2034.000 | 2040.000 | 6.000 | 90 | (-3515.783, 1517.114) | -6000 |

Beat table, appended rows: 8 Descent, s 1500, 380, 1880, ride leg, 152.00 s at 1.5 and at 2.5; 9 Foot, s 1880, 160,
2040, F1-F4, 106.67 s at 1.5, 64.00 s at 2.5. Key positions: lantern (-9.2, -90, 1499.0) **removed**; Keeper's Lantern
(-3513.883, -6000, 1517.114); car (3.4); fin (3.4); terrace centre (-3521.783, 1507.114) r 16; reef channel x 900-1100,
z 704-1040. The "fixed origin" deviation of 14.4 holds for s <= 1507.5 only (3.8).

### 4.6 14.6 Transitions (added)

| Boundary | Change and how it is hidden | Blends |
|---|---|---|
| 7 -> 8 (s 1490-1510) | the walker enters the car (south door); doors close s 1500-1507.5; `BwFrame` starts at s 1507.5; sets of 3.12 off at s 1507.5 (all hidden by the dam) | reverb ledge 0.5 -> cabin 0.4 over 2 s with a 0.6 s hard edge at the seal; EV, vFOV continuous |
| in 8 | the near top station swaps to its far proxy at about tau 35 (>= 1.5 km, dE <= 2); treadmill modules recycle outside the frustum | - |
| 8 -> 9 (s 1866-1896) | doors open s 1872.5-1880; the foot set is enabled from s 1800 (pre-warmed) | cabin 0.4 -> canopy 0.8 over 2 s -> open 0.5 by s 1900 |
| 9 -> end | goal s >= 2038.5; 3.10 | - |

Continuity test (EditMode): sweep s 0-2040 in 0.1 m steps (Rule P limits of 14.6) and t_g 0-41 in 1/30 s steps.

### 4.7 14.7 Hard problems (additions)

- **H6**: section 1.5 replaces the whale formula; lamps, snow drift, shaft occlusion, `el(s)`, `N(s)` are keyed by s;
  the ride by s (car clock); the end by t_g.
- **H3**: eye emission through water (1.2), shaft occlusion map (1.8), lamp spots outside the glass, the glass sway.
- **H4**: any eye altitude, the fin wedge, galleries and lamps, the mist slab as a volume, the KS28 sight wedge (3.7, 3.11).
- **H9 (new) Precision far from the origin**: `BwFrame` (3.8).

### 4.8 14.10.1 Numeric checks (added or replaced)

| ID | Check | Expected |
|---|---|---|
| T-PATH2 | path | s 0-1500 as T-PATH; ride leg 380.000, s rate 2.5 at both speeds; foot leg 160.000 +-0.01 (122.584, 15.708, 15.708, 6.000), end (-3515.783, 1517.114); `World`, `FloorY`, `EyeY` continuous at s 1500 and 1880; beats at 0, 120, 420, 700, 920, 1070, 1370, 1500, 1880; goal 2038.5 |
| T-RIDE | profile | L 6813.237 +-0.01, theta 60.161 +-0.01; v_max 75.000, |a| <= 1.500, |j| <= 1.000; T_R 142.343 +-0.005; identical at both speeds; floor roll and pitch 0.000; the eye's second difference at 30 fps equals a(t) sin(theta) dt^2 within 0.05 mm (no jitter); camera-to-car offset jitter <= 0.1 mm |
| T-FRAME | floating origin | every shift a multiple of 768 m on x or y only; |camera Unity x, y| <= 1408; 11 shifts in the ride; a forced shift with a static camera changes the image by <= 1 level (mean <= 0.1); T-PREC at s 2035 for 120 frames <= 1 level; lantern-rail centroid jitter under a 0.1 px/frame pan < 0.1 px |
| T-SUN8 | sun | sunset centre at altitude 4388 +-50 m (tau 49.15 +-0.5); lower limb at tau 26.9 +-0.5; KS28 crest disc fraction 0.81 +-0.03, half-lit altitude 5363 +-50; crest dark at t_g 14.0 +-0.3 |
| T-KRAKEN | choreography | root pose at a fixed s equal at 1.5 and 2.5 m/s within 1.0 m and 1.0 deg; no root step > 0.5 m per 1/60 s; lingering (stop at s 160 for 120 s): K1 completes, visible parts keep moving >= 0.3 m/s; clearances of 1.4 (>= 140 m east of x 6.0 for s <= 920; >= 1 m outside portholes; >= 0.3 m outside the outer glass for s 1070-1373; >= 2 m from islet hulls; body outside the reef footprint except the channel); bus <= 8 entries; eye px at KS17 1296 +-80 |
| T-AUDIO+ | kraken and ride audio | impact arrivals = picture + d/343 +-0.05 s (K2 1.33, K4 1.75, K5 0.61); the voice precedes K1, K2, K4, K7-eyes by 3-8 s; true peak <= -1 dBTP (4x); steps <= 0.25 except the listed transients (<= 0.5); RMS below 20 Hz <= -30 dBFS per 3 s; event-window ceilings of 2.5; outside them 14.3 targets +-1.5 dB at both speeds |
| T-NUM | gaze | all new keys <= 18 / 13 deg/s (peaks: beat 6 10.3 / 6.6; ride 16.9 / 7.3; foot 17.4; end tilt 7.8) |

### 4.9 14.10.2 Key shots (changed and added; 1080p, 4K, 16:10, 21:9; vFOV of the schedule)

| ID | Tag | s (or t_g) | Eye (x, y, z) true | Yaw / pitch | vFOV | Must be visible (4K) |
|---|---|---|---|---|---|---|
| KS04 | crest_sea (+K1) | 160 | as 14.10.2 | 80 / -8 | 64 | + kraken crown at bearing 72.5, 92 x 198 px, rows 764-849, >= 6 levels darker than the fog-only render in its mask |
| KS07 | spill_gates (+K3) | 492 | as 14.10.2 | 70 / -6 | 66 | + the shadow: band at rows 952-1001, >= 6 levels darker than the sea without the kraken; the slick (block std of the sea texture <= 0.6 x outside) |
| KS10 | reef_wide (+K4) | 776 | as 14.10.2 | 78 / -3 | 64 | + the dome rising behind islet (180, 905) (eyes still hidden); every 14.5.4 acceptance unchanged; no islet occluded by the kraken |
| KK1 | arm_tower | 378 | (-1.5, 1.7, 378.0) | 22 / +4 | 64 | arm 733 +-40 px tall at bearing 46.7, tower 284 +-15 px at bearing 0.8, both inside all aspects with 3 % margin; sea-side lamp post z 400 in the foreground |
| KK2 | head_eyes | 788 | (3.0, 1.7, 788.0) | 78 / -3 | 64 | head 499 px wide at bearing 56.3, its top at 15.7 deg; both main eyes glowing (luma >= 200, R >= 1.5 G, bloom >= 30 px) >= 1.0 deg above the islet silhouette; the draped arm over islet (260, 830): 117 px thick, >= 8 suckers of >= 30 px, red underside visible; a gauge platform within 200 m |
| KS11 | reef_spray (+K5) | 824 | as 14.10.2 | 25 / +6 | 64 | + slap column 445 +-40 px at bearing 49.5 |
| KK3 | porthole | 1008 | (2.318, -37.86, 1007.900) | 93 / -2 | 60 | the porthole disc darkened (luma <= 20) with a pale sucker rim crossing it |
| KS16 | win_up (+K7) | 1166 | as 14.10.2 | 70 / +40 | 64 | + the arm band (yaw 55-64, pitch 54-58) >= 6 levels darker than the no-kraken render, soft edges (no line) |
| KK4 | arm_glass | 1232 | (6.674, -71.74, 1231.842) | 66 / +4 | 64 | lamp A full; >= 6 suckers with hooks, each >= 300 px; red underside (R >= 1.3 G in the lit arm mask); 3-5 green points (G >= 1.5 R) beyond |
| KS17 | win_eye (replaces win_whale) | 1268 | (7.215, -76.06, 1267.838) | 90 / +2 | 64 | iris 1296 +-80 px inside the frame at all aspects; R/G >= 1.5 at the iris; horizontal slit pupil (aspect >= 3:1); 0 creature pixels in the pier mask; own luma band 30-110 (14.10.5 ruling: the lit eye fills 60 % of the frame); clipped <= 0.5 % (eye glow cores excluded as lamp cores) |
| KS18 | win_down (+K7) | 1320 | as 14.10.2 | 60 / -24 | 64 | + >= 2 green eye points (G >= 2 R) near yaw 62 pitch -31; the luma table holds outside the eye mask |
| KS20 | ledge_world (re-audit) | 1452 | as 14.10.2 | -27.5 / -7 | 66 | the fin's south face in the lower part (centre column below row ~1786); the lit car at columns about 2580-2915 with the boarding axis (old lantern spot) at column 2792 +-20; the vanishing point readable through the car glazing (edge contrast >= 8 levels); >= 200 light blobs outside the fin mask at s 1470 |
| KS21 | station (replaces beacon) | 1490 | (-9.2, -88.3, 1487.2) | 0 / -1 | 66 | the car 6.9 m ahead (columns about 800-2560), its interior lamp glow >= 40 px; the fin ridge falling away at the left |
| KS22 | ride_drop | 1530 (tau 8) | (-28.88, -122.62, 1497.114) | 270 / -30 | 66 | horizon row 213 +-8, track at row 2046 +-12, the lowland 5.9 km below |
| KS23 | ride_sunset | 1632.875 (tau 49.15) | (-883.44, -1612.43, 1497.114) | 298 / -4 | 66 | sun centre on the horizon at column 2124 +-10, row 1026 +-8, inside all aspects; a window mullion |
| KS24 | ride_lookback | 1740 (tau 92) | (-2481.33, -4398.11, 1497.114) | 90 / +38 | 66 | the face to the crest (row 388 +-12); lit band from row 717 up; the top station; the fin ridge; roof glass edge |
| KS25 | ride_lanterns | 1815 (tau 122) | (-3255.87, -5748.43, 1497.114) | 270 / -40 | 66 | causeway lanterns at rows 1150-1485, the Keeper's Lantern brighter, the station canopy |
| KS26 | foot_causeway | 1905 | (-3424.199, -5998.3, 1497.114) | 270 / 0 | 66 | five lanterns leading west, the terrace, the western afterglow; mean luma 20-70 |
| KS27 | keeper | t_g 1.5 (s 2040) | (-3515.783, -5998.3, 1517.114) | 90 / 0 | 66 | the lantern glass at row 1474 +-20, glow radius >= 40 px; the wall base across the full width; no crest in frame |
| KS28 | dam_reveal | t_g 8.5 (s 2040) | (-3515.783, -5998.3, 1517.114) | 90 / +29 | 66 | crest a straight horizontal line at row 94 +-8 (straightness <= 1.5 px per 100 px); horizon row 2003 +-8; half-lit line row 204 +-20; >= 20 gallery lines; fin ridge from the bottom to row <= 115; >= 3 jets; mean luma 25-70 |

Capture flags: ride shots by s (no new flag); KS27/KS28 need `-end-t T` (t_g for the end camera and the sun, capture
mode). KS15 unchanged (the arm crossing starts at s 1122).

### 4.10 14.10.3 thresholds and 14.10.4 autoplay

| Beat | Mean luma | Clipped | Banding region | Aliasing region |
|---|---|---|---|---|
| 8 | 20-80 | <= 1.0 % outside a 2.5 deg disc round the sun | dusk sky, face gradient | rails, mullions, ridge edges |
| 9 | 20-70 | <= 0.5 % (lantern cores excluded) | sky, the wall's alpenglow | causeway kerbs, lantern cages, crest line |

Sequences (30 fps): `seq_k1` (s 136-184), `seq_k2` (344-414), `seq_k3` (452-520), `seq_k4` (738-842), `seq_k6`
(994-1022), `seq_k7` (1100-1345, 1080p), `seq_ride_depart` (1500-1560), `seq_ride_sunset` (1570-1680),
`seq_ride_lookback` (1700-1770), `seq_ride_arrive` (1840-1890), `seq_origin` (+-1 s around every shift: activation
difference <= 0.2 level), `seq_turnback` (s 1985-2040 then t_g 0-16). Gates: luma step <= 1.2 levels per frame; no
area over 10 % of the frame changes by over 15 % within 0.33 s (lamp ramps, porthole, shaft occlusion, the sunset);
activation difference <= 0.5. The encode test adds KS23 and KS28.

**Autoplay** (`-autotest`): at 2.5 m/s (`-cinematic`) the goal trigger (s 2038.5) at **816.15 +-3 s**, ending done at
**857.15 +-3**, exit 0 by **865 s**; at 1.5 m/s trigger at **1258.42 +-4 s**, done **1299.42 +-4**, exit 0 by **1307 s**.
Intermediate: at rest in the car 600.50 / 1000.50, departure 604.50 / 1004.50, arrival 746.84 / 1146.84, walk restarts
752.50 / 1152.50. Beats trigger once in order at s 0, 120, 420, 700, 920, 1070, 1370, 1500, 1880. Progress speed 2.50
(1.50) on walked legs, 2.50 in the ride leg at both speeds; the ground-speed check excludes the ride leg. The wall bot
covers s 0-1500 and 1880-2040 (both lane edges 0.40 m inside), skips the ride leg. Roll 0.00 +-0.01 deg manual,
<= 0.3 cinematic, also in the car.

### 4.11 14.12 Constants (added or changed; mirrored in `BulwarkConst`)

| Name | Value |
|---|---|
| PATH_LENGTH / BEAT_S | 2040.000 / 0, 120, 420, 700, 920, 1070, 1370, 1500, 1880, 2040; goal trigger 2038.5, rest 2040 |
| Ride leg | s 1500-1880; s rate 2.5 (car clock); doors close 3.0 s, seal 1.0, settle 2.657, doors open 3.0; leg 152.000 s |
| Ride | top (-9.199, -90, 1497.114), delta (-3390, -5910, 0), L 6813.237, theta 60.161, v 75, a 1.5, j 1.0, T_R 142.343 |
| Fin | z 1497.114, crown deck x -6.0..-14.2 y -90, ridge (-14.2, -90) -> (-3404.2, -6000), w(y) = 20 + 100 (-90 - y)/5910 |
| Car | interior 7.3 x 6.0 x 3.2 (x -13.85..-6.55 at the top, z 1494.114-1500.114), doors 4.0 x 2.4, glass floor strip 1.2 |
| Foot | start (-3399.199, 1497.114) heading 270; 122.584 + arc r 10 (15.708) + arc r 10 (15.708) + 6.000; end (-3515.783, 1517.114); terrace (-3521.783, 1507.114) r 16; causeway 8 m, lanterns x -3445..-3505 step 15; Keeper's Lantern (-3513.883, -6000, 1517.114); floor -6000 |
| Origin | step 768, threshold 640, active from s 1507.5; final O (-3072, -5376, 0) |
| Sun | beat 8 el -2.10 - 0.10 F; beat 9 el -2.20 - 0.15 u; end el -2.35 - 0.47 smootherstep((t_g - 9)/7); az 305 |
| Haze | s0 4.5e-5 -> 2.0e-4 over F 0.5-1.0; foot 2.0e-4 |
| EV | beat 8 +0.3 -> +0.9 (F 0.2-0.7) -> +1.1; beat 9 +1.1 |
| End | dwell 1.5, tilt 1.5-8.5 to pitch +29, sun 9-16, fade 16-20, black 20-21.5, text 21.5-31.5, stinger 32.5, done 41.0 |
| Kraken | scale 220 m/u; arm 1300 m, splice u 264 (d 21), taper 1.43, tip 0.6; sucker 0.28 d (0.15-6.0); eye 20 / iris 12; emission (1.0, 0.12, 0.02) x 4.0 air / 6.0 water; corridor clearance 140; glass clearance 0.3; transit <= 25 m/s; windows K0 62-80, K1 136-180, K2 352-410, K3 458-516, K4a 724-752, K4 744-840, K5 812-830, K6 996-1020, K7 1100-1345 |
| Reef channel | x 900-1100, z 704-1040, no units |
| Beat 6 lamps | A s 1220 aim (20, +5): click 1204, up 1204-1213, full to 1236, down 1236-1245; B s 1250 aim (45, -8): click 1247, up 1247-1256, full to 1278, down 1278-1287; ramps log-linear |
| Removed | whale (all `Whale*`), lamp keys `LampA*`/`LampB*` of the whale, lantern on the ledge (`Lantern*` position moves to the foot) |

---

## 5. Implementation plan (next wave)

### 5.1 Order

1. **L0, lead, serial, first**: `BwPath` + `FilletPath` (ride leg, foot leg, zero-length straight between fillets, beat
   table), `BulwarkConst` (4.11, key shots, lamps), `Core/BwFrame.cs`, `Systems/RideState.cs` (pure function of tau:
   distance, speed, acceleration, doors), `PlayerController` / `CameraRig` / `AutoWalker` / `WalkPlanner` hooks (car clock
   in the ride leg, input ignored, stop ramp at 1500, start ramp at 1880, end camera at the goal), `BeatDirector`
   (9 beats, goal 2038.5, publishes `t_g`), `GameBootstrap` / `WarmupLoader` (sets `Descent`, `Foot`), `Instanced*`,
   `BackdropRig` and `LocalLightPool` shift listeners, contract `KrakenProxy` (mantle ellipsoid + 8 x 8 capsules,
   for audits), `IsletHull` contract (stand-in cones from `BulwarkConst` until Wave C), stubs so every builder compiles;
   `PathTests` (T-PATH2), `RideTests`, `FrameTests`.
2. **Parallel builders on disjoint files** (5.2).
3. **Lead integration**: RebuildAll, Validate, EditMode, build.
4. **Gate**: smoke `-autotest -speed 25`, autotests at 2.5 (about 865 s) and 1.5 (about 1307 s), soak 1800 s, wall bot,
   perf at 1080p and 4K, captures of every KS/KK and sequence.
5. **Six-lens critic panel** on the contact sheets, fix rounds.
6. **Wave C** (beats 1, 4, 5): K0 in the gallery, the reef channel and islet hulls for K4/K5, the K6 porthole; their
   kraken acceptance runs then.

### 5.2 Builders, ownership, acceptance

| Builder | Owns | Acceptance |
|---|---|---|
| **kraken2** (continues the A1 kraken builder) | `Runtime/Creature/**` (`KrakenRig`, `KrakenChoreography` pure (s, t), `KrakenDirector`: SeaDisturbances writer, audio event list, key-shot subjects, `KrakenShadowRT` shaft-occlusion map), `Shaders/BwCreature*` + includes (skin, crust, kelp, passing cloud, eyes with lids and pupils, suckers with hooks, wet film, glass contact blanching), `Assets/_Game/Creature/**`, `Beats/QA_KrakenRig/**`, `Tests/EditMode/{KrakenTests,KrakenChoreoTests}.cs`, `Kraken*` palette tokens | 1.1-1.6, 1.8 creature parts; T-KRAKEN; KS04/KS07/KS10/KK1/KK2/KS11/KS16/KK4/KS17/KS18 subject numbers (on the graybox where beats 1, 4, 5 are not built); <= 60 k visible triangles and <= 0.5 ms (4K) far; <= 460 k and <= 2.0 ms near in beat 6; spline skinning 8 arms x 64 nodes <= 0.1 ms CPU, 0 GC |
| **voice** | `Runtime/Audio/**` (`KrakenVoice`, `RideAudio`, `FootAudio`, `BulwarkScore` rows: whale removed, K events, windows, ducking; `BwAudioSpec` targets B8, B9 and transient exceptions), `Tests/EditMode/AudioTests.cs`, `QA/audio-trims.json` | section 2; T-AUDIO and T-AUDIO+ at both speeds; the player's `OnAudioFilterRead` dump within 1 dB RMS of the offline render; a listening pass logged (each event audible on laptop speakers through its harmonics) |
| **hall** | `Beats/Beat06_WindowGallery/**` (whale removed; lamps re-keyed and re-aimed; snow bulk drift; glass sway; dust at the strike), `Shaders/BwWaterPortal.shader` + include (shaft occlusion fetch, eye glow and lamp in-scatter through water), `Tests/EditMode/Beat06Tests.cs` | 1.8 timeline at both speeds; KS15-KS18, KK4 numbers; 14.5.6 acceptance items (2), (4), (5) unchanged; T6 pier mask 0 px; lamp-ramp sequences pass the 10 %/15 %/0.33 s gate |
| **ending** | `Beats/Beat07_SluiceLedge/**` (lantern removed, top station, fin crown deck), `Beats/Beat08_Descent/**` (car, glass, treadmill fin and track, car light, returning car), `Beats/Beat09_Foot/**` (ground disc, station, causeway, lanterns, terrace, Keeper's Lantern), `Shaders/{BwGround,BwGlass}.shader`, `Tests/EditMode/{Beat07Tests,Beat08Tests,Beat09Tests}.cs` | 3.2-3.4, 3.9, 3.12; KS21-KS27; T-RIDE car parts; T-FRAME image tests; no strobing in `seq_ride_*`; near/far fin crossover dE <= 2 at 400 m; ride frame <= 8 ms at 4K, foot <= 9 ms |
| **dusk** | `Shaders/{BwFarField,BwSky*}.shader`, `Include/BulwarkAtmosphere.hlsl` (`_BwOriginTrue`, disc fraction at any altitude, alpenglow, mist slab volume), `Beats/World_Sky/**`, `Systems/{AtmosphereDriver,BulwarkSchedule,ProgressDirector}.cs` (beats 8-9 rows, el schedules, end sun by t_g, EV, haze), `Core/BwCurvature.cs`, `Tests/EditMode/{AtmosphereTests,FarFieldTests}.cs` | 3.7, 3.11; T-SUN8; KS20 re-audit, KS22-KS24, KS28; flicker std <= 1.5 on the lowland and the far wall; city lights and lamp density fields without shimmer |
| **sea** | `Beats/Beat02_Crest/**`, `Shaders/BwSea.shader`, `Include/BwSeaWaves.hlsl`, `Systems/SeaState.cs`, sea tests | the disturbance sizes and strengths of 1.6 read at their distances (K1 Breach at 1464 m inside the clipmap, K2 TentacleRise at 455 m, K3 Shadow + Wake at 230-310 m); no sparkle (H1 tests unchanged with the bus active) |
| **mist** | `Rendering/MistVolume*.cs`, `Shaders/BwMist*`, `Include/BwMistVolume.hlsl`, mist tests | kraken sprays for K1, K2, K4, K5 in the wall-spray slots with kraken priority; K5 column 50 m; no MSAA halos (T10) |
| **integrate** | `Runtime/QA/**` (`ShotPlan` new shots and `-end-t`, sequences, `VisibilityAudit` kraken subjects via `KrakenProxy`, `AutoTestRunner` times and beats, `WallBot` legs), `Player/{PathGuide,GazeSchedule,WalkPlanner,AutoWalker}.cs` (new keys, foot lanes, colliders shifting with `BwFrame`), `Core/CliArgs.cs` (`-end-t`), lit includes (`BwPatternPos`, altitude with `_BwOriginTrue`), `Rendering/LocalLightPool.cs` (shift, outside-glass spots), `Runtime/UI/**` (end timeline 3.10), `Tools/qa/**` (regions and bands for KS/KK shots, beats 8-9), `DEVELOPMENT.md` rows (0.2, 3.1, 3.8, 2.5, the reef channel, the end duration), `docs/FRAMEWORK.md` | 4.7-4.10; T-NUM gaze; autotests at both speeds within the 4.10 tolerances; visibility audit 0 failures for all new shots at three aspects |

Shared contracts stay lead-owned: `SeaDisturbances` (unchanged, Max 8), `WaveImpacts`, `BwFrame`, `RideState`,
`KrakenProxy`, `IsletHull`.

### 5.3 Performance budgets (RTX 5070, Part 11.7 targets unchanged)

| Item | Triangles | GPU 4K | GPU 1080p |
|---|---|---|---|
| Kraken far LOD, beats 2-4 (mantle 20 k + 8 x 3 k), <= 12 draws | <= 60 k | <= 0.5 ms | <= 0.2 ms |
| SeaDisturbances loop, 8 entries | - | <= 0.3 ms | <= 0.1 ms |
| Kraken near, beat 6 (mantle 150 k, arms 8 x 24 k, suckers <= 120 k) + hall 210 k | <= 670 k | <= 2.0 ms | <= 0.7 ms |
| Shaft occlusion map | - | <= 0.15 ms | <= 0.05 ms |
| Ride frame total | <= 300 k | <= 8 ms | <= 3.5 ms |
| Foot frame total | <= 300 k | <= 9 ms | <= 4 ms |
| Origin shift (CPU, 11 times) | - | <= 1.5 ms each, 0 GC | - |

---

## 6. Risks and cut order

### 6.1 Risks

| Risk | Mitigation |
|---|---|
| Float jitter or pattern seams at origin shifts (Part 10 #17, #33) | `BwFrame` rule, true-coordinate patterns, `seq_origin`, T-FRAME |
| The fin changes KS20 (and shows at KS05's right edge) | accepted (a leading line into the abyss); re-audit KS20 and KS05 with the new numbers |
| Strobing in the ride (track, face lamps, gallery lines) | continuous rails, >= 6 m periods, density fields, footprint filtering; `seq_ride_*` |
| Thin far parts of the kraken alias (curl tips at 455 m: 27 px, fine; whips in K5: 2.6 m at 209 m = 24 px) | minimum 1.5 px width with energy preservation on the far LOD; fog-matched fades |
| Flashes in beat 6 (lamps, eye glow, shaft loss, strike) | log-linear ramps >= 3.6 s, glow pulse 0.2 Hz +-10 %, shaft loss over >= 6 s; the 0.33 s gate |
| Arm-islet or arm-glass interpenetration (Part 10 #28) | `IsletHull` + 2 m offset; glass clamp 0.3 m; tests |
| Pier leaks of the creature (T6) | portal queues; pier mask test at KS17 and over `seq_k7` |
| The voice masked by the fall roar (beat 3) or the storm (beat 2) | ducking windows; harmonics in the 100-1500 Hz formant band |
| Infrasound pumping the limiter | 15 Hz HP, <= -30 dBFS below 20 Hz, a kraken bus pre-limiter |
| Runtime grows to 13.6 min (2.5 m/s) | accepted per the user's order; if a shorter cut is wanted, raise v_max to 90 m/s with a 2.0 m/s^2 and jerk 1.5 (T_R 122.0 s; v_max alone saves only 5 s) before touching any appearance |
| A manual player lingers or turns back | latched phases, idle layers, the progress-only interior encounter; the end camera waits for the player's view |
| Capture determinism | phases = progress branch, idle layers on the frozen clock, ride by s, the end by `-end-t` |

### 6.2 Cut order (first cut first; merges with 14.8)

1. K4a arm under the swell.
2. K6 porthole event.
3. K4's second arm (keep the draped arm).
4. Lesser eyes and kelp streamers.
5. Beat 6 shaft occlusion -> veil dim keyed by s (1.10).
6. K5 slap column (keep the sound).
7. Gallery lamps on the far face (keep the gallery lines); the returning car light.
8. Mist slab as a volume (fallback 2D, mist rivers kept out of the ride's altitude band).
9. Treadmill fin surfaces (keep the rails).
10. Then the 14.8 list in its order (beat 4 islets and detail, snow, city detail and jets, crest props, conduit spray,
    foam detail, portholes); "whale lamps, then the whale" is removed from it.

**Never cut**: K1, K2, K3, K4 head with the eyes and the draped arm, the beat 6 suckers on the glass, the eye and the
strike, the kraken voice, the ride with the sunset, `BwFrame`, the foot walk, KS28 with the tilt and the light leaving
the crest; plus 14.8's never-cut list (far field, sea, atmosphere, dither, door, window portal and veil, plume with a bow,
ledge, QA). The Keeper's Lantern replaces "ledge and lantern" in that list.

---

## Sources

- Leviathan staging, sound before sight: [feedme.design on Subnautica](https://feedme.design/designing-with-depth-how-subnautica-turns-the-ocean-into-its-greatest-threat), [The Geek Wave: Subnautica and sound](https://thegeekwave.com/2020/06/subnautica-and-sound), [PC Gamer: encountering leviathans](https://pcgamer.com/great-moments-in-pc-gaming-encountering-leviathans-in-subnautica)
- The kraken mistaken for islands (Pontoppidan): [Biodiversity Heritage Library blog](https://blog.biodiversitylibrary.org/2014/10/release-kraken)
- Octopus arm bend propagation and drag: [Yekutieli et al. 2005](https://cris.huji.ac.il/en/publications/dynamic-model-of-the-octopus-arm-i-biomechanics-of-the-octopus-re/), [Gutfreund et al. 1996](https://cris.huji.ac.il/en/publications/organization-of-octopus-arm-movements-a-model-system-for-studying/), [JEB: octopus arm movements under constraint](https://cob.silverchair.com/jeb/article-split/218/7/1069/14549/Octopus-arm-movements-under-constrained-conditions)
- Octopus pupil (round in the dark, horizontal slit, only the lit eye reacts): [Frontiers 2020 (PMC)](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC7530272/)
- Passing-cloud skin displays: [How et al. 2017](https://www.frontiersin.org/articles/10.3389/fphys.2017.00393/pdf)
- Roughness of screams (30-150 Hz): [AIP Inside Science](https://www.aip.org/inside-science/why-screams-are-rough-on-the-brain), [Neuroscience News on Arnal et al. 2015](https://neurosciencenews.com/acoustic-properties-fear-scream-2257/); non-linear sound and arousal: [Kingston University](https://researchinnovation.kingston.ac.uk/en/publications/the-sound-of-arousal-in-music-is-context-dependent-2/)
- Infrasound and unease (tentative): [IFLScience on the 17 Hz concert experiment](https://www.iflscience.com/the-infrasonic-17-hz-tone-experiment-induced-revulsion-and-fear-in-participants-77093), [Vic Tandy (19 Hz)](https://en.wikipedia.org/wiki/Vic_Tandy); slowed ocean sounds: [Skeptoid on the Bloop](https://skeptoid.com/episodes/177)
- Lift speed record 20.5 m/s: [Mitsubishi Electric](https://mitsubishielectric.com/news/2016/0510.html); comfort ranges: [Peters Research](https://download.peters-research.com/library/Lift_Performance_Time.pdf), [Elevator World](https://elevatorworld.com/article/lift-performance-time); rope limits: [KONE UltraRope](https://constructionenquirer.com/?p=71555)
- Inclined lifts and dams: [Stoosbahn](https://en.wikipedia.org/wiki/Stoosbahn), [Krasnoyarsk ship lift](https://en.wikipedia.org/wiki/Krasnoyarsk_ship_lift), [Breuer's Grand Coulee powerhouse and its inclined glass elevator (Docomomo US)](https://docomomo-us.org/news/powerhouse-marcel-breuer-at-grand-coulee)
- Horizon dip and alpenglow: [SDSU: dip of the horizon](https://aty.sdsu.edu/explain/atmos_refr/dip.html), [Arizona-Sonora Desert Museum: light after sunset](https://www.desertmuseum.org/books/nhsd_light.php?print=y)
- Gravity dam proportions: [VSSUT lecture notes](https://www.vssut.ac.in/lecture_notes/lecture1530778597.pdf)
