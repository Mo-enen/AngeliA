# Open World Sandbox

This section provides a brief overview of map and level-related concepts. For more in-depth usage instructions, please refer to the following chapters.



### Map Logic

The engine supports seamless stitching of multiple maps, allowing you to create a smooth open-world experience. Only map tiles within the player’s visible range are rendered. Entities that enter the screen are dynamically loaded, while those leaving the screen are automatically unloaded.

There are several types of map tiles, each serving a distinct purpose:

- **Background Tiles**: Visual only; they do not participate in collision detection.
- **Level Tiles**: Have collision enabled by default. You can add a "OneWay" tag to make them block movement in only certain directions.
- **Entity Tiles**: Used to load instances with custom logic, typically representing interactive or behavior-driven objects.
- **Helper Tiles**: Exist only in backend data. They are neither rendered nor interactive, and are used for tagging or internal logic purposes.

<img src="../../images/OpenWorldSandbox-Map.png" width="61.8%"/>



### Map Layers

Players can travel between adjacent map layers using doors or portals (as shown below). The engine displays nearby background layers in real time. Use the method `TeleportTask.TeleportFromDoor` to teleport the player character.

<img src="../../images/GoThroughLayer.gif" width="61.8%"/>



### Map File Format

Map files use the `.ibb` format, which stands for `int byte byte`. Each `.ibb` file represents a map area of 128 × 128 tiles. The filename corresponds to the coordinates of the lower-left corner of that square region in the format `x_y_z`.

Each file stores a flat list of values structured as triples: `int, byte, byte` (e.g., `7126723, 0, 0, -467174, 1, 0, 892135, 2, 1, ...`).  
- The `int` represents the block's `AngeHash` (unique identifier).
- The first `byte` is the local X coordinate within the map.
- The second `byte` is the local Y coordinate.

If a coordinate value exceeds 127, subtract 128 to retrieve the actual data.

Block types are determined by the ranges of the coordinates:
- `x < 128`, `y < 128`: Entity tile  
- `x < 128`, `y ≥ 128`: Helper tile  
- `x ≥ 128`, `y < 128`: Level tile  
- `x ≥ 128`, `y ≥ 128`: Background tile

<img src="../../images/OpenWorldSandbox-MapFile.png" width="61.8%"/>
