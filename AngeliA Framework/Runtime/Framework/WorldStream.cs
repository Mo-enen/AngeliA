using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public sealed class WorldStream : System.IDisposable {





		#region --- VAR ---


		// Data
		private readonly Dictionary<Vector3Int, World> Pool = new();
		private readonly string MapRoot;
		private readonly MapLocation Location;
		private readonly bool Readonly;


		#endregion




		#region --- API ---


		public WorldStream (string mapRoot, MapLocation location, bool @readonly) {
			Pool.Clear();
			MapRoot = mapRoot;
			Location = location;
			Readonly = @readonly;
		}


		public void Clear () => Pool.Clear();


		public void Dispose () => Pool.Clear();


		// Block
		public void GetBlocksAt (int unitX, int unitY, int z, out int entity, out int level, out int background) {
			entity = 0;
			level = 0;
			background = 0;
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorld(worldX, worldY, z, out var world)) {
				int index = unitY.UMod(Const.MAP) * Const.MAP + unitX.UMod(Const.MAP);
				entity = world.Entity[index];
				level = world.Level[index];
				background = world.Background[index];
			}
		}


		public int GetBlockAt (int unitX, int unitY, int z, BlockType type) {
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorld(worldX, worldY, z, out var world)) {
				int localX = unitX.UMod(Const.MAP);
				int localY = unitY.UMod(Const.MAP);
				return type switch {
					BlockType.Entity => world.Entity[localY * Const.MAP + localX],
					BlockType.Level => world.Level[localY * Const.MAP + localX],
					BlockType.Background => world.Background[localY * Const.MAP + localX],
					_ => 0,
				};
			}
			return 0;
		}


		public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int value) {
			if (Readonly) {
#if UNITY_EDITOR
				Debug.LogError("Can not write block data when the world stream is readonly.");
#endif
				return;
			}
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorld(worldX, worldY, z, out var world, createNew: true)) {
				int localX = unitX.UMod(Const.MAP);
				int localY = unitY.UMod(Const.MAP);
				switch (type) {
					case BlockType.Entity:
						world.Entity[localY * Const.MAP + localX] = value;
						break;
					case BlockType.Level:
						world.Level[localY * Const.MAP + localX] = value;
						break;
					case BlockType.Background:
						world.Background[localY * Const.MAP + localX] = value;
						break;
				}
			}
		}


		public void SetBlocksAt (int unitX, int unitY, int z, int entity, int level, int background) {
			if (Readonly) {
#if UNITY_EDITOR
				Debug.LogError("Can not write block data when the world stream is readonly.");
#endif
				return;
			}
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorld(worldX, worldY, z, out var world, createNew: true)) {
				int index = unitY.UMod(Const.MAP) * Const.MAP + unitX.UMod(Const.MAP);
				world.Entity[index] = entity;
				world.Level[index] = level;
				world.Background[index] = background;
			}
		}


		public bool TryGetWorld (int worldX, int worldY, int worldZ, out World world, bool createNew = false) {
			var pos = new Vector3Int(worldX, worldY, worldZ);
			if (!Pool.TryGetValue(pos, out world)) {
				world = new World(pos);
				bool loaded = world.LoadFromDisk(MapRoot, Location, worldX, worldY, worldZ, true);
				if (!loaded && !createNew) world = null;
				Pool.Add(pos, world);
				if (createNew) world.SaveToDisk(MapRoot);
			}
			return world != null;
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}