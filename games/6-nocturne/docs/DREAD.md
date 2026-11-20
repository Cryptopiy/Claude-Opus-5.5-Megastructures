# DREAD - horror and megalophobia layer (user request 2026-11-19, decision D-034)

**Authority.** On 2026-11-19 the user asked what could raise the horror and the megalophobia, and accepted the whole list ("Да, всё").

**Tone.** The tone moves from "melancholic and beautiful" to "beautiful and uncanny". It is dread, never jump scares:
- No people.
- No real text.
- No flashing. Every light event passes the photosensitivity gate of D-010.
- No roll. The comfort limits of 14.0 hold.

**Construction.**
- Everything is procedural: MeshKit and MeshBuilder geometry, Noc shaders, procedural audio.
- No AI-generated or imported models (D-034 item 5).

Distances use the 14.4 layout. The speed of sound is 343 m/s.

## 1. Sound of scale (audio owner; highest effect per cost)

**1.1 Physical echo across the Gulf.** Distant one-shots are echoed off the Gulf faces:
- Sources: a horn, a metal strike, a structural groan.
- Each reflection is delayed by 2 d / 343 s, where d is the distance from the listener to the face plane (x_A = 215.3, x_B = 815.3 at the neck). Each is attenuated by 1/(1 + d/150) and low-passed with distance.
- Delays along the walk:

  | Position | x | Face A delay | Face B delay |
  |---|---|---|---|
  | Tongue tip | 375 | 0.93 s | 2.57 s |
  | Bridge middle | 535 | 1.87 s | 1.63 s |
  | Boulevard mouth | 215 | ~0 | 3.5 s, one long echo |

- Two or three echoes are spaced by the face-to-face path, 600 m = 1.75 s.
- **Source events**, keyed to s:
  - a horn far down the rift once in beat 3 (s 440) and once in beat 4 (s 700);
  - metal strikes inside the Gulf faces at random 25-40 s intervals.

**1.2 Pressure.**
- A sub-bass swell under the big reveals: KS04, KS09, KS10, KS13 and KS17.
- It sits at 35-55 Hz with harmonics up to about 160 Hz, so that laptops, phones and YouTube's encoder still carry it. True infrasound is inaudible on most playback.
- It rises over 6-10 s before the reveal and decays over 8 s, under the 14.3 RMS ceilings with no clipping.

**1.3 Structural groans.**
- Rare, low, ship-hull creaks and booms of the towers under load, every 20-60 s from s 120 on.
- They are built from resonant comb and filtered noise with a slow pitch bend, and placed in 3-D on the nearest large mass.

**1.4 Silence drop.**
- 2.0 s before KS09 and before KS17, rain and the city bed duck by 18-24 dB over 0.6 s, so that only breath and wind remain.
- They come back under the reveal over 1.5 s.

## 2. Something huge moves (new package DR, files Beats/Dread/**)

**2.1 The hull over the rift.**
- A colossal dark body crosses the rift in or just under the cloud deck while the player stands on the Edge (beats 3-4, s 430-640). It is a ribbed loft with a keel, fins and engine glows.
- Size and position: length 1 800 m, beam 260 m, belly at y 420-440, so its back is inside the deck at y 460.
- Course and speed: it crosses along z at 18 m/s, so the visible pass lasts about 60-90 s.
- How it reads:
  - It reads only as a silhouette that darkens the city-lit cloud glow. The cloud model of NocAtmosphere takes it as an analytic occluder: an elongated soft ellipse in the deck plane.
  - Rows of small red and white belly lights through NocStreak: pitch 40 m, 2.2 px minimum, the energy rule of 14.2.5.
  - A low engine glow at the stern.
- Sound: an infrasonic drone with a slow Doppler (1.2), plus 1.1 echoes.

**2.2 Elevators and gantries.**
- About 30 lit cabins (3 x 3 x 3.5 m) ride glass shafts on the boulevard towers and on the Gulf faces, at 3-8 m/s.
- A cabin needs 10-20 minutes for its shaft, so during the walk it climbs only a fraction of the tower: scale through slowness.
- Two maintenance gantry cranes slide along face B at 0.5 m/s.
- Visible cabins must not pop. Their positions are functions of time with exact wraps (Part 10 item 33).

## 3. Presence in the abyss (DR, with the abyss owner)

**3.1 The shadow below.**
- In beat 4 (s 600-860) a vast shape passes under the bridge in the haze: 900 m long, at y -500..-800, at 4-6 m/s.
- It shows only as:
  - a slow darkening of the haze glow and the H3 sheets (an analytic occluder in the abyss shader);
  - the lower traffic rivers of AB going dark where it passes;
  - a few dim, slow-moving cold lights along its back.
- It is seen over the balustrade and through the KS13 glass strip. The koi above is a hologram; this one is not, and it is never explained.

**3.2 The falling panel.**
- Once per walk (s 470, beat 3), a sign or cladding panel of 12 x 8 m tears off face B at y +150, tumbles with drag and falls.
- It falls about 600-800 m in 20 s and dissolves into the haze (T = 0.46 at -1 000 m straight down).
- It is visible for 15-20 s. No impact is ever heard; the only sound is the tearing creak at the start.

## 4. Camera (gaze schedule and CameraRig; constants owner)

**4.1 Long look-ups.**
- At KS05, KS09 and KS17 the gaze rises from about 0 to +55..+65 deg over 6-8 s, at 8-10 deg/s, inside the 18 deg/s pitch limit.
- Each holds 2 s at the top: the tower never ends; it disappears into the deck.
- In the walk mode, these are suggestions inside the player's free look.

**4.2 Vertigo at the plunge (KS10).**
- At the balustrade, vFOV widens slowly from 68 to 74 deg over 4 s while the eye leans 0.35 m forward and pitch eases down. It returns over 3 s.
- This is an exception to the 62-68 vFOV schedule, recorded in D-034. Motion comfort is checked in the walk and cinematic autotests: no rate above the 14.0 limits.

**4.3 Bridge sway.**
- In beat 4, a lateral translation of 2-3 cm at 0.12 Hz, with wind gusts (the 0.28 rad gust tilt of H5) that are also heard.
- No roll. Total sway stays at 0.3 deg or less.

## 5. The city notices you (facade, koi and screen owners)

**5.1 The darkness wave.**
- Twice in the walk (beat 2 at s 300; beat 7 at s 1 330), the lit windows of one named tower go dark floor by floor, from the top down: a front at 120-200 m/s over 20-30 s.
- It is a uniform analytic term in NocFacade (tower key, front y(t)). It keeps the 1.2 s window toggle ramp, so it never flickers.
- The windows come back slowly about 40 s later.

**5.2 The koi's eye.**
- In beat 7 the koi's eye spot turns its pupil toward the camera, slowly (at most 6 deg/s), and keeps following while the head is in view.

**5.3 The eye on the wall of signs.**
- In beat 6, within the three slow waves of 14.5.6, the screen wall once shows a single enormous eye opening over 6 s, holding and closing. It is soft-edged, with no text, and passes the D-010 photosensitivity gate.

**5.4 Sheet lightning in the deck.**
- Very dim, slow glows inside the clouds:
  - rise 0.4 s, decay 1.5 s;
  - at most +15 % cloud luminance in a region of about 20 % of the frame;
  - at most once every 25 s; none in beats 1 and 5.
- One of them, timed to 2.1, outlines the hull's silhouette.
- Every one passes D-010: no more than one luminance transition per second, and none above the flash area limits.

## 6. Traces of emptiness (tram, interior and tenement owners)

- **The empty tram (BV's tram T_A):** it arrives at the boulevard stop, opens its doors to a lit empty interior, waits 12 s, chimes, and leaves.
- **An elevator lobby on the boulevard terminus:** the doors open onto an empty lit hall and close again after 8 s.
- **Rooms near the path** (alley windows, the Old Quarter tenement): two or three are modelled. Each has a chair, a table and a cup, a lamp on, and nobody.
- **The warm goal window** stays the only warm window. Its occupant is never shown.

## 7. Ownership and order

| Area | Owner | Items |
|---|---|---|
| New objects | New package DR, `Beats/Dread/**` | the hull (2.1), elevators and gantries (2.2), the abyss shadow (3.1), the falling panel (3.2) |
| Facade | FA | the darkness wave (5.1) |
| Koi | HT / KO | the koi's eye (5.2) |
| Screen wall | SG | the eye on the wall (5.3) |
| Atmosphere | AT | cloud occluder and sheet lightning (2.1, 5.4) |
| Abyss | AB | abyss occluder (3.1) |
| Audio | AU | all of section 1, and every sound cue of sections 2-6 |
| Constants and gaze | lead | camera (section 4) |
| Tram, lobby, rooms | BV / HT | traces (section 6) |

**Order** (wave B, after wave A is accepted):
1. Section 1 (sound).
2. Then 2.1, 3.1, 4.1 and 4.2, and 5.1.
3. Then the rest.

**Acceptance.** Every item gets:
- captures, with a sequence for each moving item;
- the photosensitivity gate for every light event;
- the audio gates (RMS, peak, no clicks);
- the visibility audit, which keeps the key-shot subjects readable;
- a critic lens, "dread", added to the panel next to "wow": does the frame make the viewer feel small?
