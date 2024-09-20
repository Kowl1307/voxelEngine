# Voxel Engine
This is a voxel engine in unity, following the tutorial series by Sunny Vally Studio (https://www.youtube.com/@SunnyValleyStudio).

Current Features:
- Terrain generation by chunks with Octave Noise and domain warping
  - Layer based generation following Chain of Command pattern
- Infinte terrain generation
  - multithreaded to improve performance and maintain playable state
- Chunk pooling
- Rough Biome generation based on temperature (with Perlin Noise)
- Structure generation (general framework exists)
  - Trees
- Modular list of Voxel Types
- Basic First Person controller to explore the generated world
