using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class WorldSquad : IBlockSquad {




	#region --- VAR ---


	// Api
	public static bool Enable { get; set; } = true;
	public static bool SquadReady { get; private set; } = false;
	public static WorldStream Stream { get; private set; } = null;
	public static WorldSquad Front { get; set; } = null;
	public static WorldSquad Behind { get; set; } = null;
	public static string MapRoot => Stream?.MapRoot;
	public static bool DontSaveChangesToFile { get; private set; } = false;

	// Data
	private static event System.Action BeforeLevelRendered;
	private static event System.Action AfterLevelRendered;
	private static readonly Dictionary<int, int> LevelToEntityRedirect = new();
	private static readonly Int3[] WorldPosInViewCache = new Int3[128];
	private int BackgroundBlockSize = Const.CEL;
	private IRect CullingCameraRect = default;
	private IRect ParallaxRect = default;
	private IRect CameraRect = default;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static TaskResult OnGameInitialize () {
		if (!Renderer.IsReady) return TaskResult.Continue;
		bool useProceduralMap = Universe.BuiltInInfo.UseProceduralMap;
		DontSaveChangesToFile = !useProceduralMap;
		Front = new WorldSquad();
		Behind = new WorldSquad();
		Util.LinkEventWithAttribute<BeforeLevelRenderedAttribute>(typeof(WorldSquad), nameof(BeforeLevelRendered));
		Util.LinkEventWithAttribute<AfterLevelRenderedAttribute>(typeof(WorldSquad), nameof(AfterLevelRendered));
		Stream = WorldStream.GetOrCreateStreamFromPool(useProceduralMap ? Universe.BuiltIn.SlotUserMapRoot : Universe.BuiltIn.MapRoot);
		SquadReady = true;
		// Level to Entity Redirect
		foreach (var (type, att) in Util.AllClassWithAttribute<EntityAttribute.SpawnFromLevelBlock>()) {
			int levelID = att.LevelID;
			int entityID = type.AngeHash();
			if (Renderer.TryGetSpriteGroup(levelID, out var group)) {
				LevelToEntityRedirect.TryAdd(group.ID, entityID);
			} else if (Renderer.TryGetSprite(levelID, out var sprite, true)) {
				LevelToEntityRedirect.TryAdd(levelID, entityID);
				if (sprite.Group != null) {
					LevelToEntityRedirect.TryAdd(sprite.Group.ID, entityID);
				}
			}
		}
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
		if (DontSaveChangesToFile) return;
		Stream?.SaveAllDirty();
		Stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.SlotUserMapRoot);
		Stream.ClearWorldPool();
		SquadReady = true;
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () {
		if (!DontSaveChangesToFile) {
			Stream?.SaveAllDirty();
		}
	}


	[OnGameRestart]
	public static void OnGameRestart () => Enable = true;


	[OnGameUpdate(-64)]
	public static void OnGameUpdate () {
		if (!Enable) return;
		// Render
		Front.RenderCurrentFrame();
		Behind.RenderCurrentFrame();
		// Auto Save
		if (!DontSaveChangesToFile && Game.GlobalFrame % 3600 == 0 && Stream.IsDirty) {
			Stream.SaveAllDirty();
		}
	}


	#endregion




	#region --- API ---


	public static void DiscardAllChangesInMemory () => Stream.DiscardAllChanges();


	public static Int3[] ForAllWorldInRange (IRect overlapRange, int z, out int count) {
		int left = overlapRange.xMin.ToUnit().UDivide(Const.MAP);
		int right = (overlapRange.xMax.ToUnit() + 1).UDivide(Const.MAP);
		int down = overlapRange.yMin.ToUnit().UDivide(Const.MAP);
		int up = (overlapRange.yMax.ToUnit() + 1).UDivide(Const.MAP);
		int index = 0;
		int maxCount = WorldPosInViewCache.Length;
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				WorldPosInViewCache[index] = new Int3(i, j, z);
				index++;
				if (index >= maxCount) {
					goto _END_;
				}
			}
		}
		_END_:;
		count = index;
		return WorldPosInViewCache;
	}


	// Get Block
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


	public void GetBlocksAt (int unitX, int unitY, int z, out int entity, out int level, out int background, out int element) => Stream.GetBlocksAt(unitX, unitY, z, out entity, out level, out background, out element);


	public int GetBlockAt (int unitX, int unitY, BlockType type) => Stream.GetBlockAt(unitX, unitY, Stage.ViewZ, type);
	public int GetBlockAt (int unitX, int unitY, int z, BlockType type) => Stream.GetBlockAt(unitX, unitY, Stage.ViewZ, type);


	// Set Block
	public void SetBlockAt (int unitX, int unitY, BlockType type, int newID) => SetBlockAt(unitX, unitY, Stage.ViewZ, type, newID);
	public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID) => Stream.SetBlockAt(unitX, unitY, z, type, newID);


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
							} else if (LevelToEntityRedirect.TryGetValue(lv, out int redirectEntityID)) {
								if (unitRect_Entity.Contains(i, j)) {
									DrawEntity(redirectEntityID, i, j, z);
								}
							} else {
								DrawLevelBlock(lv, i, j, isBehind);
							}
						}
					}
				}
			}
		}
		if (!isBehind) AfterLevelRendered?.Invoke();

		// Entity
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
				var eSpan = new System.ReadOnlySpan<int>(world.Entities);
				var eleSpan = new System.ReadOnlySpan<int>(world.Elements);
				if (!isBehind) {
					// Front Block
					for (int j = d; j < u; j++) {
						int localY = j - worldUnitRect.y;
						int index = localY * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							// Entity
							int entityID = eSpan[index];
							if (entityID != 0) {
								DrawEntity(entityID, i, j, z);
							}
							// Element
							int eleID = eleSpan[index];
							if (eleID != 0) {
								int localID = MapGenerationSystem.STARTER_ID + 8 - eleID;
								if (localID >= 0 && localID <= 8) {
									var startPoint = new Int3(i, j, z);
									if (!MapGenerationSystem.IsGenerating(startPoint)) {
										if (localID == 0) {
											MapGenerationSystem.GenerateMap(startPoint, true);
										} else {
											MapGenerationSystem.GenerateMap(startPoint, (Direction8)(localID - 1), true);
										}
									}
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
							if (entityID != 0 && Stage.RequireDrawEntityBehind(entityID)) {
								DrawBehind(entityID, i, j, true);
							}
							// Element
							int eleID = eleSpan[index];
							if (eleID != 0) {
								int localID = MapGenerationSystem.STARTER_ID + 8 - eleID;
								if (localID >= 0 && localID <= 8) {
									var startPoint = new Int3(i, j, z);
									if (!MapGenerationSystem.IsGenerating(startPoint)) {
										if (localID == 0) {
											MapGenerationSystem.GenerateMap(startPoint, true);
										} else {
											MapGenerationSystem.GenerateMap(startPoint, (Direction8)(localID - 1), true);
										}
									}
								}
							}
						}
					}
				}

			}
		}

		// Final
		if (isBehind) Renderer.SetLayerToDefault();
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
			if (sp.Tag.HasAny(TagUtil.AllDamages)) {
				Physics.FillBlock(PhysicsLayer.DAMAGE, id, rect.Expand(1), true, sp.Tag);
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

		if (
			!Renderer.TryGetSprite(id, out var sprite) &&
			!Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) return;

		var cameraRect = CameraRect;
		var rect = new IRect(
			Util.RemapUnclamped(ParallaxRect.xMin, ParallaxRect.xMax, cameraRect.xMin, cameraRect.xMax, unitX * Const.CEL),
			Util.RemapUnclamped(ParallaxRect.yMin, ParallaxRect.yMax, cameraRect.yMin, cameraRect.yMax, unitY * Const.CEL),
			BackgroundBlockSize, BackgroundBlockSize
		);

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
