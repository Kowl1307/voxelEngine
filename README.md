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
