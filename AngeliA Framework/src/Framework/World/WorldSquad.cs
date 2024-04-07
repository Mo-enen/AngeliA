using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum MapChannel { General, Procedure, }

[System.AttributeUsage(System.AttributeTargets.Method)] public class OnMapFolderChangedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class BeforeLevelRenderedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class AfterLevelRenderedAttribute : System.Attribute { }

public class WorldSquad : IBlockSquad {




	#region --- VAR ---


	// Api
	public static bool Enable { get; set; } = true;
	public static WorldSquad Front { get; set; } = null;
	public static WorldSquad Behind { get; set; } = null;
	public static IBlockSquad FrontBlockSquad => Front;
	public static MapChannel Channel { get; private set; } = MapChannel.General;
	public static string MapRoot => Stream.MapRoot;

	// Data
	private static readonly WorldStream Stream = new();
	private static event System.Action OnMapFolderChanged;
	private static event System.Action BeforeLevelRendered;
	private static event System.Action AfterLevelRendered;
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
		SwitchToCraftedMode(forceOperate: true);
		Reset();
	}


	[OnGameRestart]
	public static void OnGameRestart () => Enable = true;


	[OnGameUpdate(-64)]
	public static void OnGameUpdate () {
		if (!Enable) return;
		Front.Render();
		Behind.Render();
	}


	private void Render () {

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
		int worldL = unitRect_Level.xMin.UDivide(Const.MAP);
		int worldR = unitRect_Level.xMax.CeilDivide(Const.MAP);
		int worldD = unitRect_Level.yMin.UDivide(Const.MAP);
		int worldU = unitRect_Level.yMax.CeilDivide(Const.MAP);
		for (int worldI = worldL; worldI < worldR; worldI++) {
			for (int worldJ = worldD; worldJ < worldU; worldJ++) {
				if (!Stream.TryGetWorld(worldI, worldJ, z, out var world)) continue;
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
		worldL = unitRect_Entity.xMin.UDivide(Const.MAP);
		worldR = unitRect_Entity.xMax.CeilDivide(Const.MAP);
		worldD = unitRect_Entity.yMin.UDivide(Const.MAP);
		worldU = unitRect_Entity.yMax.CeilDivide(Const.MAP);
		for (int worldI = worldL; worldI < worldR; worldI++) {
			for (int worldJ = worldD; worldJ < worldU; worldJ++) {
				if (!Stream.TryGetWorld(worldI, worldJ, z, out var world)) continue;
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
		Channel = newChannel;
		string mapRoot = newChannel switch {
			MapChannel.General => UniverseSystem.CurrentUniverse.MapRoot,
			MapChannel.Procedure => Util.CombinePaths(UniverseSystem.CurrentUniverse.ProcedureMapRoot, folderName),
			_ => UniverseSystem.CurrentUniverse.MapRoot,
		};
		Stream.Load(mapRoot, @readonly: true);
		OnMapFolderChanged?.Invoke();
	}


	public static void Reset () => Stream.Clear();


	// Get Set Block
	public Int4 GetBlocksAt (int unitX, int unitY) {
		Stream.GetBlocksAt(unitX, unitY, Stage.ViewZ, out int e, out int l, out int b, out int ele);
		return new Int4(e, l, b, ele);
	}


	public int GetBlockAt (int unitX, int unitY) {
		int id = GetBlockAt(unitX, unitY, BlockType.Element);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Entity);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Element);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Level);
		if (id == 0) id = GetBlockAt(unitX, unitY, BlockType.Background);
		return id;
	}


	public int GetBlockAt (int unitX, int unitY, BlockType type) => Stream.GetBlockAt(unitX, unitY, Stage.ViewZ, type);


	public bool FindBlock (int id, int unitX, int unitY, Direction4 direction, BlockType type, out int resultX, out int resultY, int maxDistance = Const.MAP) {
		resultX = default;
		resultY = default;
		int l = unitX - maxDistance;
		int r = unitX + maxDistance;
		int d = unitY - maxDistance;
		int u = unitY + maxDistance;
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


	int IBlockSquad.GetBlockAt (int unitX, int unitY, int z, BlockType type) => GetBlockAt(unitX, unitY, type);


	void IBlockSquad.SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID) { }


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




}
