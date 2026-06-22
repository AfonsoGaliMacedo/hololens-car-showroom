# Aeroflux

An interactive **mixed-reality car showroom** for Microsoft HoloLens 2, built in
Unity with [MRTK3](https://learn.microsoft.com/windows/mixed-reality/mrtk-unity/mrtk3-overview/).

The mixed-reality showcase our STEM Racing team built for the Aeroflux car, used at the National Finals to let international judges explore the car hands-on in HoloLens — summon it in front of you, walk around it, explode the assembly, and strip it down part by part with hand and gaze input.

> **Platform:** HoloLens 2 (OpenXR) · **Engine:** Unity 2022.3 LTS · **Toolkit:** MRTK3

---

## Features

| Feature | What it does |
| --- | --- |
| **Summon Car** | Places the car a comfortable arm's length in front of you, at table height, facing you — wherever you happen to be looking. |
| **Exploded View** | Slides every panel outward at once so you can see how the whole car is assembled. Press again to pull it back together. |
| **Dissect, piece by piece** | Lifts out one component per press so you can inspect each part on its own — bodywork first, then in toward the chassis. |
| **Reassemble** | Smoothly animates every part home in one tap. |
| **Racing Mode** | Toggles a procedurally-built race-track backdrop (asphalt, curbing, start/finish line, grandstands) for a showroom feel. |

Every interaction is smoothly animated and driven from a floating control panel
that works with HoloLens hand input and with a mouse in the Editor.

---

## Quick start

You'll need **Unity 2022.3 LTS** with the *Universal Windows Platform* build
support module installed.

1. Clone the repo and open the folder in Unity Hub.
2. Open `Assets/Scenes/Aeroflux Pilot.unity`.
3. In the menu bar, choose **Aeroflux → Set Up Demo In Current Scene**.
   This finds the car model, adds the `AerofluxAppController`, and wires
   everything together.
4. Press **Play**. Use the floating panel (click in the Game view, or use the
   MRTK input simulator — hold *Space* / right-mouse to look, then click the
   buttons).

That's the whole setup — the control panel and racing backdrop are generated at
runtime, so there's nothing else to drag into place.

### Running on a HoloLens 2

1. **File → Build Settings → Universal Windows Platform** and switch platform.
2. Build to a folder, open the generated `.sln` in Visual Studio.
3. Deploy to the device (or the HoloLens 2 Emulator) as `Release / ARM64`.

---

## How it's built

The demo is deliberately **self-wiring** — the scripts discover the car's parts
and build their own UI, so you don't have to hand-place a panel per button.

```
Assets/Aeroflux/Scripts/
├── Runtime/
│   ├── AerofluxAppController.cs   # Entry point – ties everything together
│   ├── CarDissectionController.cs # Exploded view, dissect-by-part, reassemble
│   ├── CarPart.cs                 # Per-part record (home pose + explode vector)
│   ├── CarSummoner.cs             # "Bring the car in front of me"
│   ├── RacingEnvironment.cs       # Procedural track backdrop (no extra assets)
│   └── AerofluxControlPanel.cs    # Generates the floating uGUI button panel
└── Editor/
    └── AerofluxSetup.cs           # "Aeroflux ▸ Set Up Demo" menu command
```

A few design notes:

- **Parts are discovered, not hard-coded.** `CarDissectionController` scans the
  mesh renderers under the car and computes each part's outward direction from
  the model's centre, so the explode/dissect effects work with any car model you
  drop in — not just the one bundled here.
- **The control panel is plain uGUI on a world-space canvas.** MRTK3 drives
  standard Canvas buttons through its canvas interactor, so the same panel is
  pokable on HoloLens *and* clickable with a mouse in the Editor. No duplicate UI.
- **The racing environment is generated in code** (meshes + textures), so a fresh
  clone has no missing-asset surprises and the repo stays small.

---

## Controls (in the Editor)

MRTK3 ships with an input simulator so you can try everything without a headset:

| Input | Action |
| --- | --- |
| Hold **Right Mouse** + move | Look around |
| **W / A / S / D** | Move |
| **Left click** | Press a panel button |

---

## Project structure

```
Aeroflux/
├── Assets/
│   ├── Aeroflux/Scripts/        # All the code for this demo (see above)
│   ├── Scenes/Aeroflux Pilot.unity
│   ├── A1+PRO+Changed+wheels.fbx   # The car model
│   └── ...                      # MRTK, TextMesh Pro, XR plumbing
├── Packages/                    # Package manifest (MRTK3, OpenXR, …)
└── ProjectSettings/
```

`Library/`, `Temp/`, `Logs/` and other generated folders are intentionally left
out of version control and rebuilt by Unity on first open.

---

## Roadmap / ideas

- Per-part labels and a short spec sheet on selection
- Voice commands ("explode", "reassemble") via MRTK speech
- A paint-shop mode to recolour the body
- Spatialised engine audio

---

## Credits

- Built with [Unity](https://unity.com/) and the
  [Mixed Reality Toolkit 3](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity).
- Car model: `A1 PRO` FBX (bundled under `Assets/`).

## License

Released under the [MIT License](LICENSE). The bundled third-party car model and
the Mixed Reality Toolkit remain under their own respective licenses.
