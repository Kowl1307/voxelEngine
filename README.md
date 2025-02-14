# Voxel Engine
This is a voxel engine in unity,
based on the tutorial series by Sunny Vally Studio (https://www.youtube.com/@SunnyValleyStudio).

Current Features:
- Terrain generation by chunks with Octave Noise and domain warping
  - Layer based generation following Chain of Command pattern
- Infinite terrain generation
  - multithreaded to improve performance and maintain playable state
- Chunk pooling
- Rough Biome generation based on temperature (with Perlin Noise)
- Structure generation (general framework exists)
  - Trees
- Modular list of Voxel Types
- Voxel Scaling
  - (There are rounding errors with some fractions like 2/5)
- Basic First Person controller to explore the generated world

## Screenshots

8 Chunk Render-Distance from player
![grafik](https://github.com/user-attachments/assets/5d43855c-08c5-448b-85a2-75b1a3aea2be)
![grafik](https://github.com/user-attachments/assets/9eb26f02-0ce4-4b53-9020-fdf501f2fc3c)

16 Chunk Render-Distance from player
![grafik](https://github.com/user-attachments/assets/f3f5afc4-bbf8-45c5-aa1a-1d6cf17dc594)
![grafik](https://github.com/user-attachments/assets/e1dc57fc-9876-4cbb-9399-cec3341ec0f6)
