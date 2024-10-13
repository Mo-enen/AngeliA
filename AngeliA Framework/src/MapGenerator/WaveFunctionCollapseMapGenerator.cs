using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class WaveFunctionCollapseMapGenerator (int typeID) : MapGenerator(typeID) {




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(WaveFunctionCollapseMapGenerator).AngeHash();
	public const int WAITING_COLLAPSE_ID = -1;

	// Api
	public static bool WaveFunctionCollapseEnable { get; set; } = true;

	// Data
	private static readonly Int4[] CollapseCache = new Int4[(Const.MAP + 2) * (Const.MAP + 2)];


	#endregion




	#region --- MSG ---



	[OnWorldCreated]
	internal static void OnWorldCreated (World world) {
		if (!WaveFunctionCollapseEnable) return;
		world.Elements.FillWithValue(WAITING_COLLAPSE_ID);
	}


	[OnWorldLoaded]
	internal static void OnWorldLoaded (World world) {
		if (!WaveFunctionCollapseEnable) return;
		if (MapGenerationSystem.IsGenerating(world.WorldPosition)) return;
		MapGenerationSystem.GenerateMap(TYPE_ID, world.WorldPosition, async: true);
	}


	public override MapGenerationResult GenerateMap (Int3 worldPos) {
		if (!WaveFunctionCollapseEnable) return MapGenerationResult.Skipped;
		var stream = WorldSquad.Stream;
		FillCollapseCache(worldPos, stream);



		// TODO


		return MapGenerationResult.Success;
	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---


	private static void FillCollapseCache (Int3 worldPos, WorldStream stream) {

		const int SIZE = Const.MAP + 2;
		var cacheSpan = CollapseCache.GetSpan();
		var defaultBlockValue = new Int4(0, 0, 0, WAITING_COLLAPSE_ID);

		// Mid
		if (stream.TryGetWorld(worldPos, out var world)) {
			var lvs = world.Levels.GetReadOnlySpan();
			var bgs = world.Backgrounds.GetReadOnlySpan();
			var ens = world.Entities.GetReadOnlySpan();
			var els = world.Elements.GetReadOnlySpan();
			int index = 0;
			for (int j = 0; j < Const.MAP; j++) {
				int cacheIndex = (j + 1) * SIZE + 1;
				for (int i = 0; i < Const.MAP; i++) {
					cacheSpan[cacheIndex] = new Int4(lvs[index], bgs[index], ens[index], els[index]);
					cacheIndex++;
					index++;
				}
			}
		} else {
			cacheSpan.Fill(defaultBlockValue);
			for (int j = 0; j < Const.MAP; j++) {
				int cacheIndex = (j + 1) * SIZE + 1;
				for (int i = 0; i < Const.MAP; i++) {
					cacheIndex++;
				}
			}
		}

		// Left
		if (stream.TryGetWorld(new(worldPos.x - 1, worldPos.y, worldPos.z), out world)) {
			var lvs = world.Levels.GetReadOnlySpan();
			var bgs = world.Backgrounds.GetReadOnlySpan();
			var ens = world.Entities.GetReadOnlySpan();
			var els = world.Elements.GetReadOnlySpan();
			int index = Const.MAP - 1;
			int cacheIndex = SIZE;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[cacheIndex] = new Int4(lvs[index], bgs[index], ens[index], els[index]);
				index += Const.MAP;
				cacheIndex += SIZE;
			}
		} else {
			int cacheIndex = SIZE;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[cacheIndex] = defaultBlockValue;
				cacheIndex += SIZE;
			}
		}

		// Right
		if (stream.TryGetWorld(new(worldPos.x + 1, worldPos.y, worldPos.z), out world)) {
			var lvs = world.Levels.GetReadOnlySpan();
			var bgs = world.Backgrounds.GetReadOnlySpan();
			var ens = world.Entities.GetReadOnlySpan();
			var els = world.Elements.GetReadOnlySpan();
			int index = 0;
			int cacheIndex = SIZE + SIZE - 1;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[cacheIndex] = new Int4(lvs[index], bgs[index], ens[index], els[index]);
				index += Const.MAP;
				cacheIndex += SIZE;
			}
		} else {
			int cacheIndex = SIZE + SIZE - 1;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[cacheIndex] = defaultBlockValue;
				cacheIndex += SIZE;
			}
		}

		// Down
		if (stream.TryGetWorld(new(worldPos.x, worldPos.y - 1, worldPos.z), out world)) {
			var lvs = world.Levels.GetReadOnlySpan();
			var bgs = world.Backgrounds.GetReadOnlySpan();
			var ens = world.Entities.GetReadOnlySpan();
			var els = world.Elements.GetReadOnlySpan();
			int index = (Const.MAP - 1) * Const.MAP;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[i + 1] = new Int4(lvs[index], bgs[index], ens[index], els[index]);
				index++;
			}
		} else {
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[i + 1] = defaultBlockValue;
			}
		}

		// Up
		if (stream.TryGetWorld(new(worldPos.x, worldPos.y + 1, worldPos.z), out world)) {
			var lvs = world.Levels.GetReadOnlySpan();
			var bgs = world.Backgrounds.GetReadOnlySpan();
			var ens = world.Entities.GetReadOnlySpan();
			var els = world.Elements.GetReadOnlySpan();
			int cacheIndex = SIZE * SIZE - SIZE + 1;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[cacheIndex] = new Int4(lvs[i], bgs[i], ens[i], els[i]);
				cacheIndex++;
			}
		} else {
			int cacheIndex = SIZE * SIZE - SIZE + 1;
			for (int i = 0; i < Const.MAP; i++) {
				cacheSpan[cacheIndex] = defaultBlockValue;
				cacheIndex++;
			}
		}

		// Top Left
		if (stream.TryGetWorld(new(worldPos.x - 1, worldPos.y + 1, worldPos.z), out world)) {
			int index = Const.MAP - 1;
			var blocks = new Int4(world.Levels[index], world.Backgrounds[index], world.Entities[index], world.Elements[index]);
			cacheSpan[(SIZE - 1) * SIZE] = blocks;
		} else {
			cacheSpan[(SIZE - 1) * SIZE] = defaultBlockValue;
		}

		// Top Right
		if (stream.TryGetWorld(new(worldPos.x + 1, worldPos.y + 1, worldPos.z), out world)) {
			var blocks = new Int4(world.Levels[0], world.Backgrounds[0], world.Entities[0], world.Elements[0]);
			cacheSpan[^1] = blocks;
		} else {
			cacheSpan[^1] = defaultBlockValue;
		}

		// Bottom Left
		if (stream.TryGetWorld(new(worldPos.x - 1, worldPos.y - 1, worldPos.z), out world)) {
			int index = Const.MAP * Const.MAP - 1;
			var blocks = new Int4(world.Levels[index], world.Backgrounds[index], world.Entities[index], world.Elements[index]);
			cacheSpan[0] = blocks;
		} else {
			cacheSpan[0] = defaultBlockValue;
		}

		// Bottom Right
		if (stream.TryGetWorld(new(worldPos.x + 1, worldPos.y - 1, worldPos.z), out world)) {
			int index = Const.MAP * (Const.MAP - 1);
			var blocks = new Int4(world.Levels[index], world.Backgrounds[index], world.Entities[index], world.Elements[index]);
			cacheSpan[SIZE - 1] = blocks;
		} else {
			cacheSpan[SIZE - 1] = defaultBlockValue;
		}

	}


	#endregion




}
