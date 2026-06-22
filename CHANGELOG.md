# Changelog

All notable changes to Aeroflux are documented here.

## [1.0.0] – 2026-06-22

First public release — the original HoloLens experiment, cleaned up and expanded
into a presentable demo.

### Added
- **Exploded view** — every car panel slides outward at once, with a toggle to
  reassemble (`CarDissectionController`).
- **Dissect piece-by-piece** — lift out one component per press for close
  inspection, from the bodywork inward.
- **Reassemble** — animate every part back to its resting pose in one tap.
- **Summon Car** — reposition the car in front of the viewer at a comfortable
  height and orientation (`CarSummoner`).
- **Racing Mode** — a procedurally-generated race-track backdrop with asphalt,
  curbing, a checkered start/finish line and grandstands (`RacingEnvironment`).
- **Floating control panel** — a world-space uGUI panel, generated at runtime,
  that works with HoloLens hand input and a mouse in the Editor
  (`AerofluxControlPanel`).
- **One-click setup** — `Aeroflux ▸ Set Up Demo In Current Scene` Editor command
  that wires the whole demo together (`AerofluxSetup`).

### Notes
- Car parts are discovered at runtime from the model's mesh renderers, so the
  effects work with any car model, not just the bundled one.
- The racing backdrop is built entirely in code (meshes + textures) — no extra
  assets to import.
