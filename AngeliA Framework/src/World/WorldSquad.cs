using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Core system that renders the level/background blocks and load the world data into stage
/// </summary>
public sealed class WorldSquad : IBlockSquad {




	#region --- VAR ---


	// Api
	/// <summary>
	/// True if the system is required
	/// </summary>
	public static bool Enable { get; set; } = true;
	/// <summary>
	/// True if the system is ready to use
	/// </summary>
	public static bool SquadReady { get; private set; } = false;
	/// <summary>
	/// Squad that handles the front side map
	/// </summary>
	public static WorldSquad Front { get; set; } = null;
	/// <summary>
	/// Squad that handles the behind side map
	/// </summary>
	public static WorldSquad Behind { get; set; } = null;

	// Data
	[BeforeLevelRendered] internal static System.Action BeforeLevelRendered;
	[AfterLevelRendered] internal static System.Action AfterLevelRendered;
	[OnWorldCreatedBySquad_World] internal static System.Action<World> OnWorldCreated;
	[OnWorldLoadedBySquad_World] internal static System.Action<World> OnWorldLoaded;
	private static readonly Dictionary<int, int> BlockRedirect = [];
	private static WorldStream Stream = null;
	private static byte WorldBehindAlpha;
	private static int WorldBehindParallax;
	private static bool ReadonlyMap = false;
	private IRect CullingCameraRect = default;
	private IRect CameraRect = default;
	private Int2 ParaCenter = default;
	private float ReversePara01 = 1f;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static TaskResult OnGameInitialize () {

		if (!Renderer.IsReady) return TaskResult.Continue;

		var info = Universe.BuiltInInfo;
		WorldBehindAlpha = info.WorldBehindAlpha;
		WorldBehindParallax = info.WorldBehindParallax;
		ReadonlyMap = info.ReadonlyMap;
		Front = new WorldSquad();
		Behind = new WorldSquad();
		WorldStream.OnWorldCreated += _OnWorldCreated;
		WorldStream.OnWorldLoaded += _OnWorldLoaded;
		static void _OnWorldCreated (WorldStream stream, World world) {
			if (stream != Stream) return;
			OnWorldCreated?.Invoke(world);
		}
		static void _OnWorldLoaded (WorldStream stream, World world) {
			if (stream != Stream) return;
			OnWorldLoaded?.Invoke(world);
		}
		Stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.SlotUserMapRoot);
		Stream.UseBuiltInAsFailback = !Universe.BuiltInInfo.UseProceduralMap;
		SquadReady = true;

		return TaskResult.End;
	}


	[BeforeSavingSlotChanged]
	internal static void BeforeSavingSlotChanged () {
		SquadReady = false;
		Stage.SetViewZ(Stage.ViewZ);
		Stage.DespawnAllNonUiEntities();
	}


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () {
		BlockRedirect.Clear();
		// Reset Stream
		if (!ReadonlyMap) {
			Stream?.SaveAllDirty();
		}
		Stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.SlotUserMapRoot);
		Stream.UseBuiltInAsFailback = !Universe.BuiltInInfo.UseProceduralMap;
		Stream.ClearWorldPool();
		SquadReady = true;
	}


	[OnGameQuitting(4096)]
	internal static void OnGameQuitting () {
		if (!ReadonlyMap) {
			Stream?.SaveAllDirty();
		}
	}


	[OnGameRestart]
	internal static void OnGameRestart () => Enable = true;


	[OnGameUpdate(-64)]
	internal static void OnGameUpdate () {
		if (!Enable) return;
		// Render
		Front.RenderCurrentFrame();
		Behind.RenderCurrentFrame();
		// Auto Save
		if (!ReadonlyMap && Game.GlobalFrame % 3600 == 0 && Stream.IsDirty) {
			Stream.SaveAllDirty();
		}
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Reset the internal world instance pool
	/// </summary>
	public static void ClearStreamWorldPool () => Stream?.ClearWorldPool();


	/// <summary>
	/// Make all existing user maps require reload from built-in maps without deleting the user map folder
	/// </summary>
	public static void ResetStreamFailbackCopying () => Stream?.ResetFailbackCopying();


	/// <summary>
	/// True if world exists at given world position
	/// </summary>
	/// <param name="worldPos">In world space (1 world space = 256 * 128 global space)</param>
	public bool WorldExists (Int3 worldPos) => Stream.WorldExists(worldPos);


	// Get Block
	/// <summary>
	/// Get all ID of blocks at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Z position</param>
	public (int level, int bg, int entity, int element) GetAllBlocksAt (int unitX, int unitY, int z) => Stream.GetAllBlocksAt(unitX, unitY, z);


	/// <summary>
	/// Get block ID at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="type">Type of the block</param>
	public int GetBlockAt (int unitX, int unitY, BlockType type) => Stream.GetBlockAt(unitX, unitY, Stage.ViewZ, type);


	/// <summary>
	/// Get block ID at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="type">Type of the block</param>
	public int GetBlockAt (int unitX, int unitY, int z, BlockType type) => Stream.GetBlockAt(unitX, unitY, z, type);


	// Set Block
	/// <summary>
	/// Set block ID at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="type">Type of the block</param>
	/// <param name="newID">ID of the block</param>
	public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID) => Stream.SetBlockAt(unitX, unitY, z, type, newID);


	// Redirect
	/// <summary>
	/// Add a pair of redirect ID to make all "fromID" block rendered with "toID"
	/// </summary>
	public static void AddBlockRedirect (int fromID, int toID) => BlockRedirect[fromID] = toID;


	/// <summary>
	/// Remove the redirect pair
	/// </summary>
	/// <returns>True if the pair removed</returns>
	public static bool RemoveBlockRedirect (int fromID) => BlockRedirect.Remove(fromID);


	#endregion




	#region --- LGC ---


	private void RenderCurrentFrame () {

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
		bool procedural = Universe.BuiltInInfo.UseProceduralMap;

		if (!isBehind) {
			// Current
			unitRect_Entity = Stage.SpawnRect.ToUnit();
			unitRect_Level = unitRect_Entity.Expand(Const.LEVEL_SPAWN_PADDING_UNIT);
		} else {
			// Behind
			float para01 = WorldBehindParallax / 1000f;
			ReversePara01 = 1f / para01;
			ParaCenter = CameraRect.CenterInt();
			var parallax = ((Float2)CameraRect.size * ((para01 - 1f) / 2f)).CeilToInt();
			var parallaxUnitRect = CameraRect.Expand(
				cullingPadding.left + parallax.x,
				cullingPadding.right + parallax.x,
				cullingPadding.down + parallax.y,
				cullingPadding.up + parallax.y
			).ToUnit();
			parallaxUnitRect.width += 2;
			parallaxUnitRect.height += 2;
			parallaxUnitRect.x--;
			parallaxUnitRect.y--;
			unitRect_Entity = parallaxUnitRect.Expand(1);
			unitRect_Level = parallaxUnitRect;
		}

		// BG-Level
		if (!isBehind) {
			BeforeLevelRendered?.Invoke();
		}
		int worldL = unitRect_Level.xMin.UDivide(Const.MAP);
		int worldR = unitRect_Level.xMax.CeilDivide(Const.MAP);
		int worldD = unitRect_Level.yMin.UDivide(Const.MAP);
		int worldU = unitRect_Level.yMax.CeilDivide(Const.MAP);
		for (int worldI = worldL; worldI < worldR; worldI++) {
			for (int worldJ = worldD; worldJ < worldU; worldJ++) {
				// Get World
				World world;
				if (procedural) {
					world = Stream.GetOrCreateWorld(worldI, worldJ, z);
				} else {
					if (!Stream.TryGetWorld(worldI, worldJ, z, out world)) continue;
				}
				// Draw World
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
				var bgSpan = new System.ReadOnlySpan<int>(world.Backgrounds);
				var lvSpan = new System.ReadOnlySpan<int>(world.Levels);
				for (int j = d; j < u; j++) {
					int index = (j - worldUnitRect.y) * Const.MAP + (l - worldUnitRect.x);
					for (int i = l; i < r; i++, index++) {
						// BG
						int bg = bgSpan[index];
						if (bg != 0) {
							if (isBehind) {
								DrawBehind(bg, i, j, false);
							} else {
								DrawBackgroundBlock(bg, i, j);
							}
						}
						// Level
						int lv = lvSpan[index];
						if (lv != 0) {
							if (isBehind) {
								DrawBehind(lv, i, j, false);
							} else {
								DrawLevelBlock(lv, i, j);
							}
						}
					}
				}
			}
		}
		if (!isBehind) {
			AfterLevelRendered?.Invoke();
		}

		// Entity-Element
		worldL = unitRect_Entity.xMin.UDivide(Const.MAP);
		worldR = unitRect_Entity.xMax.CeilDivide(Const.MAP);
		worldD = unitRect_Entity.yMin.UDivide(Const.MAP);
		worldU = unitRect_Entity.yMax.CeilDivide(Const.MAP);
		for (int worldI = worldL; worldI < worldR; worldI++) {
			for (int worldJ = worldD; worldJ < worldU; worldJ++) {
				// Get World
				World world;
				if (procedural) {
					world = Stream.GetOrCreateWorld(worldI, worldJ, z);
				} else {
					if (!Stream.TryGetWorld(worldI, worldJ, z, out world)) continue;
				}
				// Draw World
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
				var eSpan = new System.ReadOnlySpan<int>(world.Entities);
				var eleSpan = new System.Span<int>(world.Elements);
				if (!isBehind) {
					// Front Block
					for (int j = d; j < u; j++) {
						int localY = j - worldUnitRect.y;
						int index = localY * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							// Entity
							int entityID = eSpan[index];
							if (entityID != 0) {
								int ele = eleSpan[index];
								if (ele != 0 && FrameworkUtil.TryGetRepositionElementDelta(ele, out int deltaX, out int deltaY)) {
									eleSpan[index] = 0;
									Stage.SpawnEntityFromWorld(
										entityID, i.ToGlobal(), j.ToGlobal(), z,
										reposDeltaX: deltaX, reposDeltaY: deltaY
									);
								} else {
									Stage.SpawnEntityFromWorld(entityID, i.ToGlobal(), j.ToGlobal(), z);
								}
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
							int entityID = eSpan[index];
							if (entityID != 0 && Stage.IsEntityRequireDrawBehind(entityID)) {
								DrawBehind(entityID, i, j, true);
							}
						}
					}
				}

			}
		}

		// Final
		if (isBehind) {
			Renderer.SetLayerToDefault();
		}
	}


	// Draw
	private void DrawBackgroundBlock (int id, int unitX, int unitY) {
		if (BlockRedirect.TryGetValue(id, out int newID)) id = newID;
		if (!Renderer.TryGetSprite(id, out var sp, false)) return;
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		// Shift Pivot
		if (sp.PivotX != 0) {
			rect.x -= rect.width * sp.PivotX / 1000;
		}
		if (sp.PivotY != 0) {
			rect.y -= rect.height * sp.PivotY / 1000;
		}
		// Draw
		if (CullingCameraRect.Overlaps(rect)) {
			Renderer.Draw(sp, rect);
		}
	}


	private void DrawLevelBlock (int id, int unitX, int unitY) {
		if (BlockRedirect.TryGetValue(id, out int newID)) id = newID;
		if (!Renderer.TryGetSprite(id, out var sp, false)) return;
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		// Shift Pivot
		if (sp.PivotX != 0) {
			rect.x -= rect.width * sp.PivotX / 1000;
		}
		if (sp.PivotY != 0) {
			rect.y -= rect.height * sp.PivotY / 1000;
		}
		// Draw
		if (CullingCameraRect.Overlaps(rect)) {
			Renderer.Draw(sp, rect);
		}
		// Collider
		if (sp.Tag.HasAny(TagUtil.AllDamages)) {
			Physics.FillBlock(PhysicsLayer.DAMAGE, id, rect.Expand(1), true, sp.Tag);
		}
		rect = rect.Shrink(
			sp.GlobalBorder.left, sp.GlobalBorder.right, sp.GlobalBorder.down, sp.GlobalBorder.up
		);
		Physics.FillBlock(PhysicsLayer.LEVEL, id, rect, sp.IsTrigger, sp.Tag);
	}


	private void DrawBehind (int id, int unitX, int unitY, bool fixRatio) {

		if (BlockRedirect.TryGetValue(id, out int newID)) id = newID;

		if (
			!Renderer.TryGetSprite(id, out var sprite) &&
			!Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) return;

		var cameraRect = CameraRect;
		var rect = new IRect(
			unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL
		).ScaleFrom(ReversePara01, ParaCenter.x, ParaCenter.y);

		// Ratio
		if (fixRatio) {
			if ((sprite.GlobalWidth != Const.CEL || sprite.GlobalHeight != Const.CEL)) {
				int width = sprite.GlobalWidth * rect.width / Const.CEL;
				int height = sprite.GlobalHeight * rect.height / Const.CEL;
				rect.x -= Util.RemapUnclamped(0, 1000, 0, width - rect.width, sprite.PivotX);
				rect.y -= Util.RemapUnclamped(0, 1000, 0, height - rect.height, sprite.PivotY);
				rect.width = width;
				rect.height = height;
			}
		} else {
			// Shift Pivot
			if (sprite.PivotX != 0) {
				rect.x -= rect.width * sprite.PivotX / 1000;
			}
			if (sprite.PivotY != 0) {
				rect.y -= rect.height * sprite.PivotY / 1000;
			}
		}

		// Color
		var tint = Color32.LerpUnclamped(
			Sky.SkyTintBottomColor, Sky.SkyTintTopColor,
			Util.InverseLerp(cameraRect.yMin, cameraRect.yMax, rect.y + rect.height / 2)
		);
		tint.a = WorldBehindAlpha;

		// Draw
		Renderer.Draw(sprite, rect, tint, 0);
	}


	#endregion




}
