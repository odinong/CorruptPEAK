# CorruptPEAK (This readme was generated from gpt cus im lazy)

![CorruptPEAK Banner](https://github.com/odinong/CorruptPEAK/blob/main/CorruptPEAKUncropped.png?raw=true)  

## Overview

**CorruptPEAK** is a BepInEx plugin mod that introduces dynamic visual and audio corruption effects into the game environment. Using various randomized distortions on rendering, audio, particles, meshes, transforms, and lightmaps, the mod progressively increases its intensity during gameplay, creating a unique and chaotic experience.

---

## Features

- **Dynamic corruption intensity** that increases over time once the mod starts.
- Corrupts multiple aspects of the scene:
  - Materials and textures
  - Audio clips and sources
  - Particle systems with randomized parameters
  - Mesh vertices deformation
  - Transform position, rotation, and scale changes
  - Lightmaps color corruption
- Configurable toggles to enable/disable corruption of renderers, audio, particles, transforms, meshes, and lightmaps.
- Automatic initialization and corruption start when joining a multiplayer room (Photon).
- Safety feature to quit the game if corruption intensity exceeds a critical threshold to avoid crashes.

---

## Installation

1. Install **BepInEx** (version compatible with your game).
2. Download the latest release of CorruptPEAK.dll from the [Releases](https://github.com/yourusername/CorruptPEAK/releases) page.
3. Place `CorruptPEAK.dll` into the `BepInEx/plugins` folder of your game directory.
4. Launch the game and join a multiplayer room to see the corruption effects begin.

---

## Configuration

All settings are currently hardcoded in the plugin source but can be customized by modifying the following public variables in `Plugin.cs` before building:

| Variable               | Description                             | Default  |
|------------------------|-------------------------------------|----------|
| `CorruptionIntensity`  | Starting intensity of corruption     | 0.0001f  |
| `CorruptionIncreaseRate`| Rate at which corruption intensity grows per second | 0.0001f |
| `CorruptRenderers`     | Toggle corruption of materials/textures | true     |
| `CorruptAudio`         | Toggle corruption of audio sources   | true     |
| `CorruptParticles`     | Toggle corruption of particle systems| true     |
| `CorruptTransforms`    | Toggle corruption of transform data  | false    |
| `CorruptMeshes`        | Toggle mesh vertex corruption        | true     |
| `ShouldCorruptLightmaps` | Toggle lightmap corruption          | true     |

---

## How It Works

- When you join Photon room, corruption automatically begins.
- The corruption intensity gradually increases, amplifying the visual and audio distortions.
- Random elements in the scene are continuously selected and corrupted based on the current intensity.
- Corruption includes color shifts, texture distortions, mesh deformation, audio pitch/volume shifts, and particle effect randomness.
- The plugin sends a one-time notification message when corruption starts.
- If the corruption intensity becomes too high (indicative of memory issues), the game will automatically quit to prevent crashes.

---

## Dependencies

- [BepInEx](https://github.com/BepInEx/BepInEx) (plugin framework)

---

## Development

This project is open-source and welcomes contributions.
