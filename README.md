# Voxel Engine
This is a voxel engine in unity,
based on the tutorial series by Sunny Vally Studio (https://www.youtube.com/@SunnyValleyStudio).

Current Features:
- Terrain generation by chunks with Octave Noise and domain warping
  - Layer based generation following Chain of Command pattern
- Infinite terrain generation
  - multithreaded to improve performance and maintain playable state
- Chunk pooling
- Modular Biome Selection
  - Rough Biome generation based on temperature (with Perlin Noise)
  - Begin of Biome Refining algorithm
- Structure generation (examples below)
  - Trees
- Decoration (/GameObject) generation (examples below)
  - Grass
- Modular list of Voxel Types
- Voxel Scaling
- Save/Load System
  - Save world settings in a .world file
  - Save each chunk in a .chunk file if it contained modified voxels. Only modified voxels are saved for obvious space reasons.
- Basic First Person controller to explore the generated world

## Screenshots

<img width="1919" height="1079" alt="grafik" src="https://github.com/user-attachments/assets/bef53a45-56d0-408a-8eb2-ddffc7fb3b70" />


8 Chunk Render-Distance from player
![grafik](https://github.com/user-attachments/assets/5d43855c-08c5-448b-85a2-75b1a3aea2be)
![grafik](https://github.com/user-attachments/assets/9eb26f02-0ce4-4b53-9020-fdf501f2fc3c)

16 Chunk Render-Distance from player
![grafik](https://github.com/user-attachments/assets/f3f5afc4-bbf8-45c5-aa1a-1d6cf17dc594)
![grafik](https://github.com/user-attachments/assets/e1dc57fc-9876-4cbb-9399-cec3341ec0f6)




