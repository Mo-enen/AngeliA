using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework;

public enum MapChannel { General, Procedure, }

[System.AttributeUsage(System.AttributeTargets.Method)] public class OnMapFolderChangedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class BeforeLevelRenderedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class AfterLevelRenderedAttribute : System.Attribute { }

public sealed class WorldSquad : IBlockSquad {




	#region --- VAR ---


	// Api
	public static bool Enable { get; set; } = true;
	public static WorldSquad Front { get; set; } = null;
	public static WorldSquad Behind { get; set; } = null;
	public static IBlockSquad FrontBlockSquad => Front;
	public static MapChannel Channel { get; private set; } = MapChannel.General;
	public static string MapRoot { get; private set; } = "";
	public World this[int i, int j] => Worlds[i, j];

	// Data
	private static readonly WorldPathPool PathPool = new();
	private readonly World[,] Worlds = new World[3, 3] { { new(), new(), new() }, { new(), new(), new() }, { new(), new(), new() }, };
	private readonly World[,] WorldBuffer = new World[3, 3];
	private readonly World[] WorldBufferAlt = new World[9];
	private static event System.Action OnMapFolderChanged;
	private static event System.Action BeforeLevelRendered;
	private static event System.Action AfterLevelRendered;
	private bool RequireReload = false;
	private int LoadedZ = int.MinValue;
	private int BackgroundBlockSize = Const.CEL;
	private IRect CullingCameraRect = default;
	private IRect ParallaxRect = default;
	private IRect CameraRect = default;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	public static void OnGameInitialize () {
		Front = new WorldSquad();
		Behind = new WorldSquad();
		Util.LinkEventWithAttribute<OnMapFolderChangedAttribute>(typeof(WorldSquad), nameof(OnMapFolderChanged));
		Util.LinkEventWithAttribute<BeforeLevelRenderedAttribute>(typeof(WorldSquad), nameof(BeforeLevelRendered));
		Util.LinkEventWithAttribute<AfterLevelRenderedAttribute>(typeof(WorldSquad), nameof(AfterLevelRendered));
	}


	[OnUniverseOpen]
	public static void OnUniverseOpen () {
		ResetWorldPathPool(UniverseSystem.CurrentUniverse.MapRoot);
		SwitchToCraftedMode(forceOperate: true);
		Front.ForceReloadDelay();
		Behind.ForceReloadDelay();
	}


	[OnGameRestart]
	public static void OnGameRestart () => Enable = true;


	[OnGameUpdate(-64)]
	public static void OnGameUpdate () {
		if (!Enable) return;
		Front.Update_Data(Stage.ViewRect, Stage.ViewZ);
		Front.Update_Rendering();
		Behind.Update_Data(Stage.ViewRect, Stage.ViewZ + 1);
		Behind.Update_Rendering();
	}


	private void Update_Data (IRect viewRect, int z) {

		var center = viewRect.CenterInt();

		// View & World
		var midZone = new IRect(
			(Int2)Worlds[1, 1].WorldPosition * Const.MAP * Const.CEL,
			new Int2(Const.MAP * Const.CEL, Const.MAP * Const.CEL)
		).Expand(Const.MAP * Const.HALF);

		if (RequireReload || !midZone.Contains(center) || z != LoadedZ) {
			// Reload All Worlds in Squad
			LoadSquadFromDisk(
				center.x.UDivide(Const.MAP * Const.CEL),
				center.y.UDivide(Const.MAP * Const.CEL),
				z,
				RequireReload
			);
			RequireReload = false;
		}
	}


	private void Update_Rendering () {

		bool isBehind = this == Behind;
		int z = isBehind ? Stage.ViewZ + 1 : Stage.ViewZ;
		if (isBehind) {
			Renderer.SetLayerToBehind();
		} else {
			Renderer.SetLayerToDefault();
		}

		IRect unitRect_Entity;
		IRect unitRect_Level;
		CameraRect = Renderer.CameraRect;
		var cullingPadding = Stage.GetCameraCullingPadding();
		CullingCameraRect = CameraRect.Expand(cullingPadding);
		float para01 = Game.WorldBehindParallax / 1000f;

		if (!isBehind) {
			// Current
			unitRect_Entity = Stage.SpawnRect.ToUnit();
			unitRect_Level = unitRect_Entity.Expand(Const.LEVEL_SPAWN_PADDING_UNIT);
		} else {
			// Behind
			BackgroundBlockSize = (Const.CEL / para01).CeilToInt();
			var parallax = ((Float2)CameraRect.size * ((para01 - 1f) / 2f)).CeilToInt();
			ParallaxRect = CameraRect.Expand(parallax.x, parallax.x, parallax.y, parallax.y);
			var parallaxUnitRect = ParallaxRect.Expand(cullingPadding).ToUnit();
			parallaxUnitRect.width += 2;
			parallaxUnitRect.height += 2;
			parallaxUnitRect.x--;
			parallaxUnitRect.y--;
			unitRect_Entity = parallaxUnitRect.Expand(1);
			unitRect_Level = parallaxUnitRect;
		}

		// BG-Level
		if (!isBehind) BeforeLevelRendered?.Invoke();
		for (int worldI = 0; worldI < 3; worldI++) {
			for (int worldJ = 0; worldJ < 3; worldJ++) {
				var world = Worlds[worldI, worldJ];
				var worldUnitRect = new IRect(
					world.WorldPosition.x * Const.MAP,
					world.WorldPosition.y * Const.MAP,
					Const.MAP,
					Const.MAP
				);
				if (!worldUnitRect.Overlaps(unitRect_Level)) continue;
				int l = System.Math.Max(unitRect_Level.x, worldUnitRect.x);
				int r = System.Math.Min(unitRect_Level.xMax, worldUnitRect.xMax);
				int d = System.Math.Max(unitRect_Level.y, worldUnitRect.y);
				int u = System.Math.Min(unitRect_Level.yMax, worldUnitRect.yMax);
				for (int j = d; j < u; j++) {
					int index = (j - worldUnitRect.y) * Const.MAP + (l - worldUnitRect.x);
					for (int i = l; i < r; i++, index++) {
						var bg = world.Backgrounds[index];
						if (bg != 0) {
							if (isBehind) {
								DrawBehind(bg, i, j, false);
							} else {
								DrawBackgroundBlock(bg, i, j);
							}
						}
						var lv = world.Levels[index];
						if (lv != 0) {
							if (isBehind) {
								DrawBehind(lv, i, j, false);
							} else {
								DrawLevelBlock(lv, i, j, isBehind);
							}
						}
					}
				}
			}
		}
		if (!isBehind) AfterLevelRendered?.Invoke();

		// Entity & Element & Global Pos
		for (int worldI = 0; worldI < 3; worldI++) {
			for (int worldJ = 0; worldJ < 3; worldJ++) {
				var world = Worlds[worldI, worldJ];
				var worldUnitRect = new IRect(
					world.WorldPosition.x * Const.MAP,
					world.WorldPosition.y * Const.MAP,
					Const.MAP,
					Const.MAP
				);
				if (!worldUnitRect.Overlaps(unitRect_Entity)) continue;
				int l = System.Math.Max(unitRect_Entity.x, worldUnitRect.x);
				int r = System.Math.Min(unitRect_Entity.xMax, worldUnitRect.xMax);
				int d = System.Math.Max(unitRect_Entity.y, worldUnitRect.y);
				int u = System.Math.Min(unitRect_Entity.yMax, worldUnitRect.yMax);

				if (!isBehind) {
					// Entity
					for (int j = d; j < u; j++) {
						int localY = j - worldUnitRect.y;
						int index = localY * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							// Entity
							var entityID = world.Entities[index];
							if (entityID != 0) {
								DrawEntity(entityID, i, j, z);
							}
							// Global Pos
							if (IUnique.TryGetIdFromPosition(new Int3(i, j, z), out int gID)) {
								DrawEntity(gID, i, j, z);
							}
						}
					}
				} else {
					// Behind
					for (int j = d; j < u; j++) {
						int localY = j - worldUnitRect.y;
						int index = localY * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							// Entity
							var entityID = world.Entities[index];
							if (entityID != 0 && Stage.RequireDrawEntityBehind(entityID, i, j, z)) {
								DrawBehind(entityID, i, j, true);
							}
							// Global Pos
							if (
								IUnique.TryGetIdFromPosition(new Int3(i, j, z), out int gID) &&
								Stage.RequireDrawEntityBehind(gID, i, j, z)
							) {
								DrawBehind(gID, i, j, true);
							}
						}
					}
				}

			}
		}

		// Final
		if (isBehind) Renderer.SetLayerToDefault();
	}


	#endregion




	#region --- API ---


	public static void SwitchToCraftedMode (bool forceOperate = false) => SetMode(string.Empty, MapChannel.General, forceOperate);
	public static void SwitchToProcedureMode (string folderName, bool forceOperate = false) => SetMode(folderName, MapChannel.Procedure, forceOperate);
	private static void SetMode (string folderName, MapChannel newChannel, bool forceOperate = false) {

		if (!forceOperate && newChannel == Channel) return;

		MapRoot = newChannel switch {
			MapChannel.General => UniverseSystem.CurrentUniverse.MapRoot,
			MapChannel.Procedure => Util.CombinePaths(UniverseSystem.CurrentUniverse.ProcedureMapRoot, folderName),
			_ => UniverseSystem.CurrentUniverse.MapRoot,
		};
		Channel = newChannel;

		var center = Stage.ViewRect.CenterInt();
		Front.LoadSquadFromDisk(
			center.x.UDivide(Const.MAP * Const.CEL),
			center.y.UDivide(Const.MAP * Const.CEL),
			Stage.ViewZ,
			true
		);
		Behind.LoadSquadFromDisk(
			center.x.UDivide(Const.MAP * Const.CEL),
			center.y.UDivide(Const.MAP * Const.CEL),
			Stage.ViewZ,
			true
		);
		Front.RequireReload = false;
		Behind.RequireReload = false;

		OnMapFolderChanged?.Invoke();
	}


	public static void ResetWorldPathPool (string newMapRoot = null) => PathPool.SetMapRoot(newMapRoot ?? PathPool.MapRoot);


	public void UpdateWorldDataImmediately (IRect viewRect, int z) => Update_Data(viewRect, z);


	public void ForceReloadDelay () => RequireReload = true;


	// Get Set Block
	public Int4 GetBlocksAt (int unitX, int unitY) {
		var position00 = Worlds[0, 0].WorldPosition;
		int worldX = unitX.UDivide(Const.MAP) - position00.x;
		int worldY = unitY.UDivide(Const.MAP) - position00.y;
		if (!worldX.InRange(0, 2) || !worldY.InRange(0, 2)) return default;
		var world = Worlds[worldX, worldY];
		int localX = unitX - world.WorldPosition.x * Const.MAP;
		int localY = unitY - world.WorldPosition.y * Const.MAP;
		return new Int4(
			world.Entities[localY * Const.MAP + localX],
			world.Levels[localY * Const.MAP + localX],
			world.Backgrounds[localY * Const.MAP + localX],
			world.Elements[localY * Const.MAP + localX]
		);
	}


	public int GetBlockAt (int unitX, int unitY) {
		int id = GetBlockAt(unitX, unitY, BlockType.Element);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Entity);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Level);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Background);
		return id;
	}


	public int GetBlockAt (int unitX, int unitY, BlockType type) {
		var position00 = Worlds[0, 0].WorldPosition;
		int worldX = unitX.UDivide(Const.MAP) - position00.x;
		int worldY = unitY.UDivide(Const.MAP) - position00.y;
		if (!worldX.InRange(0, 2) || !worldY.InRange(0, 2)) return 0;
		var world = Worlds[worldX, worldY];
		int localX = unitX - world.WorldPosition.x * Const.MAP;
		int localY = unitY - world.WorldPosition.y * Const.MAP;


		try {




			return type switch {
				BlockType.Entity => world.Entities[localY * Const.MAP + localX],
				BlockType.Level => world.Levels[localY * Const.MAP + localX],
				BlockType.Background => world.Backgrounds[localY * Const.MAP + localX],
				BlockType.Element => world.Elements[localY * Const.MAP + localX],
				_ => throw new System.NotImplementedException(),
			};
		} catch {

			Util.LogWarning(world.Entities.Length);
			Util.LogWarning(localY * Const.MAP + localX + " " + localY + " " + localX);
			return 0;
		}
	}


	public bool FindBlock (int id, int unitX, int unitY, Direction4 direction, BlockType type, out int resultX, out int resultY) {
		resultX = default;
		resultY = default;
		var position00 = Worlds[0, 0].WorldPosition;
		int l = position00.x * Const.MAP;
		int r = (position00.x + 3) * Const.MAP - 1;
		int d = position00.y * Const.MAP;
		int u = (position00.y + 3) * Const.MAP - 1;
		var delta = direction.Normal();
		while (unitX >= l && unitX <= r && unitY >= d && unitY <= u) {
			int _id = GetBlockAt(unitX, unitY, type);
			if (_id == id) {
				resultX = unitX;
				resultY = unitY;
				return true;
			}
			unitX += delta.x;
			unitY += delta.y;
		}
		return false;
	}


	public void SetBlocksAt (int unitX, int unitY, int entityID, int levelID, int backgroundID, int elementID) {
		var position00 = Worlds[0, 0].WorldPosition;
		int worldX = unitX.UDivide(Const.MAP) - position00.x;
		int worldY = unitY.UDivide(Const.MAP) - position00.y;
		if (!worldX.InRange(0, 2) || !worldY.InRange(0, 2)) return;
		var world = Worlds[worldX, worldY];
		int localX = unitX - world.WorldPosition.x * Const.MAP;
		int localY = unitY - world.WorldPosition.y * Const.MAP;
		world.Entities[localY * Const.MAP + localX] = entityID;
		world.Levels[localY * Const.MAP + localX] = levelID;
		world.Backgrounds[localY * Const.MAP + localX] = backgroundID;
		world.Elements[localY * Const.MAP + localX] = elementID;
	}


	public void SetBlockAt (int unitX, int unitY, BlockType type, int newID) {
		var position00 = Worlds[0, 0].WorldPosition;
		int worldX = unitX.UDivide(Const.MAP) - position00.x;
		int worldY = unitY.UDivide(Const.MAP) - position00.y;
		if (!worldX.InRange(0, 2) || !worldY.InRange(0, 2)) return;
		var world = Worlds[worldX, worldY];
		int localX = unitX - world.WorldPosition.x * Const.MAP;
		int localY = unitY - world.WorldPosition.y * Const.MAP;
		switch (type) {
			default: throw new System.NotImplementedException();
			case BlockType.Entity:
				world.Entities[localY * Const.MAP + localX] = newID;
				break;
			case BlockType.Level:
				world.Levels[localY * Const.MAP + localX] = newID;
				break;
			case BlockType.Background:
				world.Backgrounds[localY * Const.MAP + localX] = newID;
				break;
			case BlockType.Element:
				world.Elements[localY * Const.MAP + localX] = newID;
				break;
		}
	}


	int IBlockSquad.GetBlockAt (int unitX, int unitY, int z, BlockType type) => z == Stage.ViewZ ? GetBlockAt(unitX, unitY, type) : 0;


	void IBlockSquad.SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID) {
		if (z != Stage.ViewZ) return;
		SetBlockAt(unitX, unitY, type, newID);
	}


	// Draw
	private void DrawBackgroundBlock (int id, int unitX, int unitY) {
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		if (CullingCameraRect.Overlaps(rect)) {
			Renderer.Draw(id, rect);
		}
	}


	private void DrawLevelBlock (int id, int unitX, int unitY, bool ignoreCollider) {
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		if (CullingCameraRect.Overlaps(rect)) {
			Renderer.Draw(id, rect);
		}
		if (!ignoreCollider) {
			// Collider
			if (!Renderer.TryGetSprite(id, out var sp)) return;
			if (sp.Tag == SpriteTag.DAMAGE_TAG) {
				Physics.FillBlock(PhysicsLayer.DAMAGE, id, rect.Expand(1), true, 1);
			}
			rect = rect.Shrink(
				sp.GlobalBorder.left, sp.GlobalBorder.right, sp.GlobalBorder.down, sp.GlobalBorder.up
			);
			Physics.FillBlock(PhysicsLayer.LEVEL, id, rect, sp.IsTrigger, sp.Tag);
		}
	}


	private void DrawEntity (int id, int unitX, int unitY, int unitZ) {
		var entity = Stage.SpawnEntityFromWorld(id, unitX, unitY, unitZ);
		if (entity is Character ch) {
			ch.X += ch.Width / 2;
		}
	}


	private void DrawBehind (int id, int unitX, int unitY, bool fixRatio) {
		var cameraRect = CameraRect;
		var rect = new IRect(
			Util.RemapUnclamped(ParallaxRect.xMin, ParallaxRect.xMax, cameraRect.xMin, cameraRect.xMax, unitX * Const.CEL),
			Util.RemapUnclamped(ParallaxRect.yMin, ParallaxRect.yMax, cameraRect.yMin, cameraRect.yMax, unitY * Const.CEL),
			BackgroundBlockSize, BackgroundBlockSize
		);

		if (
			!Renderer.TryGetSprite(id, out var sprite) &&
			!Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) return;

		if (
			fixRatio &&
			(sprite.GlobalWidth != Const.CEL || sprite.GlobalHeight != Const.CEL)
		) {
			int width = sprite.GlobalWidth * rect.width / Const.CEL;
			int height = sprite.GlobalHeight * rect.height / Const.CEL;
			rect.x -= Util.RemapUnclamped(0, 1000, 0, width - rect.width, sprite.PivotX);
			rect.y -= Util.RemapUnclamped(0, 1000, 0, height - rect.height, sprite.PivotY);
			rect.width = width;
			rect.height = height;
		}
		var tint = Color32.LerpUnclamped(
			Sky.SkyTintBottomColor, Sky.SkyTintTopColor,
			Util.InverseLerp(cameraRect.yMin, cameraRect.yMax, rect.y + rect.height / 2)
		);

		tint.a = Game.WorldBehindAlpha;
		Renderer.Draw(sprite, rect, tint, 0);
	}


	#endregion




	#region --- LGC ---


	private void LoadSquadFromDisk (int centerWorldX, int centerWorldY, int z, bool forceLoad) {

		// Clear Buffer
		for (int i = 0; i < 9; i++) {
			WorldBuffer[i / 3, i % 3] = null;
			WorldBufferAlt[i] = null;
		}

		if (!forceLoad && LoadedZ == z) {
			// Worlds >> Buffer
			int alt = 0;
			for (int j = 0; j < 3; j++) {
				for (int i = 0; i < 3; i++) {
					var world = Worlds[i, j];
					int localX = world.WorldPosition.x - centerWorldX;
					int localY = world.WorldPosition.y - centerWorldY;
					if (localX >= -1 && localX <= 1 && localY >= -1 && localY <= 1) {
						WorldBuffer[localX + 1, localY + 1] = world;
					} else {
						WorldBufferAlt[alt] = world;
						alt++;
					}
				}
			}

			// Buffer >> Worlds
			alt = 0;
			for (int j = 0; j < 3; j++) {
				for (int i = 0; i < 3; i++) {
					var buffer = WorldBuffer[i, j];
					if (buffer != null) {
						Worlds[i, j] = buffer;
					} else {
						Worlds[i, j] = WorldBufferAlt[alt];
						alt++;
					}
				}
			}

			// Clear Buffer
			for (int i = 0; i < 9; i++) {
				WorldBuffer[i / 3, i % 3] = null;
				WorldBufferAlt[i] = null;
			}
		}

		// Load
		for (int j = 0; j < 3; j++) {
			for (int i = 0; i < 3; i++) {
				var world = Worlds[i, j];
				var pos = new Int3(centerWorldX + i - 1, centerWorldY + j - 1, z);
				if (!forceLoad && world.WorldPosition == pos) continue;
				if (PathPool.TryGetPath(pos, out string path)) {
					world.LoadFromDisk(path, pos.x, pos.y, pos.z);
				} else {
					world.Clear(pos);
				}
			}
		}
		LoadedZ = z;

	}


	#endregion




}
