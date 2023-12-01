using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace AngeliaFramework {


	public enum MapChannel { BuiltIn, User, }


	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnMapChannelChangedAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class BeforeLevelRenderedAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class AfterLevelRenderedAttribute : System.Attribute { }


	public sealed class WorldSquad {




		#region --- VAR ---


		// Const
		private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL = new(10) {
			{ typeof(Number0).AngeHash(), 0 },
			{ typeof(Number1).AngeHash(), 1 },
			{ typeof(Number2).AngeHash(), 2 },
			{ typeof(Number3).AngeHash(), 3 },
			{ typeof(Number4).AngeHash(), 4 },
			{ typeof(Number5).AngeHash(), 5 },
			{ typeof(Number6).AngeHash(), 6 },
			{ typeof(Number7).AngeHash(), 7 },
			{ typeof(Number8).AngeHash(), 8 },
			{ typeof(Number9).AngeHash(), 9 },
		};
		private static readonly int ENTITY_CODE = "Entity".AngeHash();

		// Event
		public static event System.Action<MapChannel> OnMapChannelChanged;
		public static event System.Action BeforeLevelRendered;
		public static event System.Action AfterLevelRendered;

		// Api
		public static WorldSquad Front { get; set; } = null;
		public static WorldSquad Behind { get; set; } = null;
		public static MapChannel Channel { get; private set; } = MapChannel.BuiltIn;
		public static MapLocation CurrentLocation {
			get {
				if (Front == null) return MapLocation.Unknown;
				var world = Front.Worlds[1, 1];
				return world != null ? world.LoadedLocation : MapLocation.Unknown;
			}
		}
		public static string MapRoot { get; private set; } = "";
		public static bool Enable { get; set; } = true;
		public static bool SpawnEntity { get; set; } = true;
		public static byte BehindAlpha { get; set; } = Const.SQUAD_BEHIND_ALPHA;
		public static bool SaveBeforeReload { get; set; } = false;
		public World this[int i, int j] => Worlds[i, j];

		// Data
		private static string[] ProcedureGeneratorRoots = null;
		private readonly World[,] Worlds = new World[3, 3] { { new(), new(), new() }, { new(), new(), new() }, { new(), new(), new() }, };
		private readonly World[,] WorldBuffer = new World[3, 3];
		private readonly World[] WorldBufferAlt = new World[9];
		private bool RequireReload = false;
		private int LoadedZ = int.MinValue;
		private int BackgroundBlockSize = Const.CEL;
		private RectInt CullingCameraRect = default;
		private RectInt ParallaxRect = default;
		private RectInt CameraRect = default;


		#endregion




		#region --- API ---


		[OnGameInitialize(-128)]
		public static void OnGameInitialize () {
			ProcedureGeneratorRoots = Util.EnumerateFolders(AngePath.ProcedureMapRoot, true, "*").ToArray() ?? new string[0];
			Front = new WorldSquad();
			Behind = new WorldSquad();
			Util.LinkEventWithAttribute<OnMapChannelChangedAttribute>(typeof(WorldSquad), nameof(OnMapChannelChanged));
			Util.LinkEventWithAttribute<BeforeLevelRenderedAttribute>(typeof(WorldSquad), nameof(BeforeLevelRendered));
			Util.LinkEventWithAttribute<AfterLevelRenderedAttribute>(typeof(WorldSquad), nameof(AfterLevelRendered));
			SetMapChannel(MapChannel.BuiltIn);
		}


		[OnGameUpdate]
		public static void OnGameUpdate () {
			Front.DrawAllBlocks(false);
			Behind.DrawAllBlocks(true);
		}


		[OnSlotChanged]
		public static void OnSlotChanged () {
			Front.ForceReloadDelay();
			Behind.ForceReloadDelay();
		}


		private void DrawAllBlocks (bool isBehind) {

			if (!Enable) return;

			int z = isBehind ? Stage.ViewZ + 1 : Stage.ViewZ;
			Vector4Int cullingPadding = CellRenderer.CameraShaking || FrameTask.IsTasking<TeleportTask>() ? new Vector4Int(Const.CEL * 4, Const.CEL * 4, Const.CEL * 4, Const.CEL * 4) : Vector4Int.zero;
			if (isBehind) {
				CellRenderer.SetLayerToBehind();
			} else {
				CellRenderer.SetLayerToDefault();
			}
			var viewPos = Stage.SpawnRect.CenterInt();
			RectInt unitRect_Entity;
			RectInt unitRect_Level;
			CameraRect = CellRenderer.CameraRect;
			CullingCameraRect = CameraRect.Expand(cullingPadding);
			const float PARA_01 = Const.SQUAD_BEHIND_PARALLAX / 1000f;

			if (!isBehind) {
				// Current
				unitRect_Entity = Stage.SpawnRect.ToUnit();
				unitRect_Level = unitRect_Entity.Expand(Const.LEVEL_SPAWN_PADDING_UNIT);
			} else {
				// Behind
				BackgroundBlockSize = (Const.CEL / PARA_01).CeilToInt();
				var parallax = ((Vector2)CameraRect.size * ((PARA_01 - 1f) / 2f)).CeilToInt();
				ParallaxRect = CameraRect.Expand(parallax.x, parallax.x, parallax.y, parallax.y);
				var parallaxUnitRect = ParallaxRect.Expand(cullingPadding).ToUnit();
				parallaxUnitRect.width += 2;
				parallaxUnitRect.height += 2;
				parallaxUnitRect.x--;
				parallaxUnitRect.y--;
				unitRect_Entity = parallaxUnitRect.Expand(1);
				unitRect_Level = parallaxUnitRect;
			}

			// View & World
			var midZone = new RectInt(
				(Vector2Int)Worlds[1, 1].WorldPosition * Const.MAP * Const.CEL,
				new Vector2Int(Const.MAP * Const.CEL, Const.MAP * Const.CEL)
			).Expand(Const.MAP * Const.HALF);

			if (RequireReload || !midZone.Contains(viewPos) || z != LoadedZ) {
				// Reload All Worlds in Squad
				if (Channel == MapChannel.User && SaveBeforeReload && !isBehind) {
					SaveToFile();
				}
				LoadSquadFromDisk(
					viewPos.x.UDivide(Const.MAP * Const.CEL),
					viewPos.y.UDivide(Const.MAP * Const.CEL),
					z,
					RequireReload
				);
				RequireReload = false;
			}

			// BG-Level
			if (!isBehind) BeforeLevelRendered?.Invoke();
			for (int worldI = 0; worldI < 3; worldI++) {
				for (int worldJ = 0; worldJ < 3; worldJ++) {
					var world = Worlds[worldI, worldJ];
					var worldUnitRect = new RectInt(
						world.WorldPosition.x * Const.MAP,
						world.WorldPosition.y * Const.MAP,
						Const.MAP,
						Const.MAP
					);
					if (!worldUnitRect.Overlaps(unitRect_Level)) continue;
					int l = Mathf.Max(unitRect_Level.x, worldUnitRect.x);
					int r = Mathf.Min(unitRect_Level.xMax, worldUnitRect.xMax);
					int d = Mathf.Max(unitRect_Level.y, worldUnitRect.y);
					int u = Mathf.Min(unitRect_Level.yMax, worldUnitRect.yMax);
					for (int j = d; j < u; j++) {
						int index = (j - worldUnitRect.y) * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							var bg = world.Background[index];
							if (bg != 0) {
								if (isBehind) {
									Draw_Behind(bg, i, j, false);
								} else {
									DrawBackgroundBlock(bg, i, j);
								}
							}
							var lv = world.Level[index];
							if (lv != 0) {
								if (isBehind) {
									Draw_Behind(lv, i, j, false);
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
			for (int worldI = 0; worldI < 3; worldI++) {
				for (int worldJ = 0; worldJ < 3; worldJ++) {
					var world = Worlds[worldI, worldJ];
					var worldUnitRect = new RectInt(
						world.WorldPosition.x * Const.MAP,
						world.WorldPosition.y * Const.MAP,
						Const.MAP,
						Const.MAP
					);
					if (!worldUnitRect.Overlaps(unitRect_Entity)) continue;
					int l = Mathf.Max(unitRect_Entity.x, worldUnitRect.x);
					int r = Mathf.Min(unitRect_Entity.xMax, worldUnitRect.xMax);
					int d = Mathf.Max(unitRect_Entity.y, worldUnitRect.y);
					int u = Mathf.Min(unitRect_Entity.yMax, worldUnitRect.yMax);
					for (int j = d; j < u; j++) {
						int index = (j - worldUnitRect.y) * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							var entityID = world.Entity[index];
							if (entityID == 0) continue;
							if (!isBehind) {
								DrawEntity(entityID, i, j, z);
							} else if (Stage.RequireDrawEntityBehind(entityID, i, j, z)) {
								Draw_Behind(entityID, i, j, true);
							}
						}
					}
				}
			}

			if (isBehind) {
				CellRenderer.SetLayerToDefault();
			}

		}


		public void ForceReloadDelay () => RequireReload = true;


		public void SaveToFile () {
			for (int i = 0; i < 3; i++) {
				for (int j = 0; j < 3; j++) {
					var world = Worlds[i, j];
					if (
						world.WorldPosition.x != int.MinValue &&
						world.WorldPosition.y != int.MinValue &&
						world.WorldPosition.z != int.MinValue
					) {
						world.SaveToDisk(MapRoot);
					}
				}
			}
		}


		public static void SetMapChannel (MapChannel newChannel) {
#if UNITY_EDITOR
			if (SaveBeforeReload) Front.SaveToFile();
#else
			if (Channel == MapChannel.User && SaveBeforeReload) SaveToFile();
#endif
			MapRoot = newChannel == MapChannel.BuiltIn ? AngePath.BuiltInMapRoot : AngePath.UserMapRoot;
			Channel = newChannel;
			var viewPos = Stage.SpawnRect.CenterInt();
			Front.LoadSquadFromDisk(
				viewPos.x.UDivide(Const.MAP * Const.CEL),
				viewPos.y.UDivide(Const.MAP * Const.CEL),
				Stage.ViewZ,
				Front.RequireReload
			);
			Behind.LoadSquadFromDisk(
				viewPos.x.UDivide(Const.MAP * Const.CEL),
				viewPos.y.UDivide(Const.MAP * Const.CEL),
				Stage.ViewZ,
				Behind.RequireReload
			);
			Front.RequireReload = false;
			Behind.RequireReload = false;

			OnMapChannelChanged?.Invoke(newChannel);
		}


		// Get Set Block
		public Vector3Int GetTriBlockAt (int unitX, int unitY) {
			var position00 = Worlds[0, 0].WorldPosition;
			int worldX = unitX.UDivide(Const.MAP) - position00.x;
			int worldY = unitY.UDivide(Const.MAP) - position00.y;
			if (!worldX.InRange(0, 2) || !worldY.InRange(0, 2)) return default;
			var world = Worlds[worldX, worldY];
			int localX = unitX - world.WorldPosition.x * Const.MAP;
			int localY = unitY - world.WorldPosition.y * Const.MAP;
			return new Vector3Int(
				world.Entity[localY * Const.MAP + localX],
				world.Level[localY * Const.MAP + localX],
				world.Background[localY * Const.MAP + localX]
			);
		}


		public int GetBlockAt (int unitX, int unitY) {
			int id = GetBlockAt(unitX, unitY, BlockType.Entity);
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
			return type switch {
				BlockType.Entity => world.Entity[localY * Const.MAP + localX],
				BlockType.Level => world.Level[localY * Const.MAP + localX],
				BlockType.Background => world.Background[localY * Const.MAP + localX],
				_ => throw new System.NotImplementedException(),
			};
		}


		public void SetBlockAt (int unitX, int unitY, int entityID, int levelID, int backgroundID) {
			var position00 = Worlds[0, 0].WorldPosition;
			int worldX = unitX.UDivide(Const.MAP) - position00.x;
			int worldY = unitY.UDivide(Const.MAP) - position00.y;
			if (!worldX.InRange(0, 2) || !worldY.InRange(0, 2)) return;
			var world = Worlds[worldX, worldY];
			int localX = unitX - world.WorldPosition.x * Const.MAP;
			int localY = unitY - world.WorldPosition.y * Const.MAP;
			world.Entity[localY * Const.MAP + localX] = entityID;
			world.Level[localY * Const.MAP + localX] = levelID;
			world.Background[localY * Const.MAP + localX] = backgroundID;
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
					world.Entity[localY * Const.MAP + localX] = newID;
					break;
				case BlockType.Level:
					world.Level[localY * Const.MAP + localX] = newID;
					break;
				case BlockType.Background:
					world.Background[localY * Const.MAP + localX] = newID;
					break;
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


		// Draw
		private void DrawBackgroundBlock (int id, int unitX, int unitY) {
			var rect = new RectInt(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
			if (CullingCameraRect.Overlaps(rect)) {
				CellRenderer.Draw(id, rect);
			}
			// Collider for Oneway
			if (CellRenderer.TryGetSprite(id, out var sp) && CellRenderer.TryGetMeta(id, out var meta) && AngeUtil.IsOnewayTag(meta.Tag)) {
				CellPhysics.FillBlock(
					PhysicsLayer.LEVEL, id,
					new RectInt(
						unitX * Const.CEL,
						unitY * Const.CEL,
						Const.CEL,
						Const.CEL
					).Shrink(
						sp.GlobalBorder.left, sp.GlobalBorder.right, sp.GlobalBorder.down, sp.GlobalBorder.up
					),
					true, meta.Tag
				);
			}
		}


		private void DrawLevelBlock (int id, int unitX, int unitY, bool behind) {
			var rect = new RectInt(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
			if (CullingCameraRect.Overlaps(rect)) {
				CellRenderer.Draw(id, rect);
			}
			if (!behind) {
				// Collider
				if (!CellRenderer.TryGetSprite(id, out var sp)) return;
				bool isTrigger = false;
				int tag = 0;
				if (CellRenderer.TryGetMeta(id, out var meta)) {
					isTrigger = meta.IsTrigger;
					tag = meta.Tag;
					if (meta.Tag == Const.DAMAGE_TAG) {
						CellPhysics.FillBlock(PhysicsLayer.DAMAGE, id, rect.Expand(1), true, 1);
					}
				}
				rect = rect.Shrink(
					sp.GlobalBorder.left, sp.GlobalBorder.right, sp.GlobalBorder.down, sp.GlobalBorder.up
				);
				CellPhysics.FillBlock(PhysicsLayer.LEVEL, id, rect, isTrigger, tag);
			}
		}


		private void DrawEntity (int id, int unitX, int unitY, int unitZ) {
			if (SpawnEntity) {
				// Spawn Entity
				var entity = Stage.SpawnEntityFromWorld(id, unitX, unitY, unitZ);
				if (entity is Character ch) {
					ch.X += ch.Width / 2;
				}
			} else {
				// Draw Entity
				var rect = new RectInt(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
				if (!CullingCameraRect.Overlaps(rect)) return;
				if (
					CellRenderer.TryGetSprite(id, out var sprite) ||
					CellRenderer.TryGetSpriteFromGroup(id, 0, out sprite)
				) {
					rect = rect.Fit(
						sprite.GlobalWidth, sprite.GlobalHeight,
						sprite.PivotX, sprite.PivotY
					);
					CellRenderer.Draw(sprite.GlobalID, rect);
				} else {
					CellRenderer.Draw(ENTITY_CODE, rect);
				}
			}
		}


		private void Draw_Behind (int id, int unitX, int unitY, bool fixRatio) {
			var cameraRect = CameraRect;
			var rect = new RectInt(
				Util.RemapUnclamped(ParallaxRect.xMin, ParallaxRect.xMax, cameraRect.xMin, cameraRect.xMax, unitX * Const.CEL),
				Util.RemapUnclamped(ParallaxRect.yMin, ParallaxRect.yMax, cameraRect.yMin, cameraRect.yMax, unitY * Const.CEL),
				BackgroundBlockSize, BackgroundBlockSize
			);
			if (
				!CellRenderer.TryGetSprite(id, out var sprite) &&
				!CellRenderer.TryGetSpriteFromGroup(id, 0, out sprite)
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
				CellRenderer.SkyTintBottom, CellRenderer.SkyTintTop,
				Mathf.InverseLerp(cameraRect.yMin, cameraRect.yMax, rect.y + rect.height / 2)
			);
			tint.a = BehindAlpha;
			CellRenderer.Draw(sprite.GlobalID, rect, tint, 0);
		}


		// System Number
		public bool ReadSystemNumber (int globalUnitX, int globalUnitY, out int number) {
			number = 0;
			bool hasH = ReadSystemNumber(globalUnitX, globalUnitY, Direction4.Right, out int numberH);
			bool hasV = ReadSystemNumber(globalUnitX, globalUnitY, Direction4.Down, out int numberV);
			if (!hasH && !hasV) return false;
			if (hasH == hasV) {
				number = Mathf.Max(numberH, numberV);
				return true;
			} else {
				number = hasH ? numberH : numberV;
				return true;
			}
		}
		public bool ReadSystemNumber (int globalUnitX, int globalUnitY, Direction4 direction, out int number) {

			number = 0;
			int digitCount = 0;
			int x = globalUnitX;
			int y = globalUnitY;
			var delta = direction.Normal();

			// Find Start
			int left = int.MinValue;
			int down = int.MinValue;
			while (HasSystemNumber(x, y)) {
				left = x;
				down = y;
				x -= delta.x;
				y -= delta.y;
			}
			if (left == int.MinValue) return false;

			// Get Number
			x = left;
			y = down;
			while (digitCount < 9 && TryGetSystemNumber(x, y, out int digit)) {
				number *= 10;
				number += digit;
				digitCount++;
				x += delta.x;
				y += delta.y;
			}
			return digitCount > 0;
		}


		public bool HasSystemNumber (int unitX, int unitY) {
			int id = GetBlockAt(unitX, unitY, BlockType.Entity);
			return id != 0 && SYSTEM_NUMBER_POOL.ContainsKey(id);
		}


		public bool TryGetSystemNumber (int unitX, int unitY, out int digitValue) {
			digitValue = 0;
			int id = GetBlockAt(unitX, unitY, BlockType.Entity);
			return id != 0 && SYSTEM_NUMBER_POOL.TryGetValue(id, out digitValue);
		}


		// Pose
		public FittingPose GetEntityPose (Entity entity, bool horizontal) => GetEntityPose(entity.TypeID, entity.X, entity.Y, horizontal);
		public FittingPose GetEntityPose (int typeID, int globalX, int globalY, bool horizontal) {
			int unitX = globalX.UDivide(Const.CEL);
			int unitY = globalY.UDivide(Const.CEL);
			bool n = GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
			bool p = GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
			return
				n && p ? FittingPose.Mid :
				!n && p ? FittingPose.Left :
				n && !p ? FittingPose.Right :
				FittingPose.Single;
		}
		public FittingPose GetEntityPose (Entity entity, bool horizontal, int mask, out Entity left_down, out Entity right_up, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			left_down = null;
			right_up = null;
			int unitX = entity.X.UDivide(Const.CEL);
			int unitY = entity.Y.UDivide(Const.CEL);
			int typeID = entity.TypeID;
			bool n = GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
			bool p = GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
			if (n) {
				left_down = CellPhysics.GetEntity(typeID, entity.Rect.Edge(horizontal ? Direction4.Left : Direction4.Down), mask, entity, mode, tag);
			}
			if (p) {
				right_up = CellPhysics.GetEntity(typeID, entity.Rect.Edge(horizontal ? Direction4.Right : Direction4.Up), mask, entity, mode, tag);
			}
			return
				n && p ? FittingPose.Mid :
				!n && p ? FittingPose.Left :
				n && !p ? FittingPose.Right :
				FittingPose.Single;
		}


		#endregion




		#region --- LGC ---


		private void LoadSquadFromDisk (int worldX, int worldY, int worldZ, bool forceLoad) {

			// Clear Buffer
			for (int i = 0; i < 9; i++) {
				WorldBuffer[i / 3, i % 3] = null;
				WorldBufferAlt[i] = null;
			}

			if (!forceLoad && LoadedZ == worldZ) {
				// Worlds >> Buffer
				int alt = 0;
				for (int j = 0; j < 3; j++) {
					for (int i = 0; i < 3; i++) {
						var world = Worlds[i, j];
						int localX = world.WorldPosition.x - worldX;
						int localY = world.WorldPosition.y - worldY;
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
					var pos = new Vector3Int(worldX + i - 1, worldY + j - 1, worldZ);
					if (!forceLoad && world.WorldPosition == pos) continue;
					// Load from Current Channel
					world.LoadFromDisk(
						MapRoot, Channel.GetLocation(), pos.x, pos.y, pos.z,
						clearExistData: true
					);
					// Load from Procedure
					foreach (var root in ProcedureGeneratorRoots) {
						world.LoadFromDisk(
							root, MapLocation.Procedure, pos.x, pos.y, pos.z,
							clearExistData: false
						);
					}
				}
			}
			LoadedZ = worldZ;

		}


		#endregion




	}
}
