using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	public enum MapChannel { BuiltIn, User, Procedure, Download, }

	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnMapChannelChangedAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class BeforeLevelRenderedAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class AfterLevelRenderedAttribute : System.Attribute { }

	public sealed class WorldSquad : IBlockSquad {




		#region --- VAR ---


		// Const
		private static readonly int ENTITY_CODE = typeof(Entity).AngeHash();

		// Api
		public static WorldSquad Front { get; set; } = null;
		public static WorldSquad Behind { get; set; } = null;
		public static IBlockSquad FrontBlockSquad => Front;
		public static MapChannel Channel { get; private set; } = MapChannel.BuiltIn;
		public static string MapRoot { get; private set; } = "";
		public static bool Enable { get; set; } = true;
		public static bool SpawnEntity { get; set; } = true;
		public static bool ShowElement { get; set; } = false;
		public static byte BehindAlpha { get; set; } = Const.SQUAD_BEHIND_ALPHA;
		public static bool SaveBeforeReload { get; set; } = false;
		public World this[int i, int j] => Worlds[i, j];

		// Data
		private readonly World[,] Worlds = new World[3, 3] { { new(), new(), new() }, { new(), new(), new() }, { new(), new(), new() }, };
		private readonly World[,] WorldBuffer = new World[3, 3];
		private readonly World[] WorldBufferAlt = new World[9];
		private static event System.Action<MapChannel> OnMapChannelChanged;
		private static event System.Action BeforeLevelRendered;
		private static event System.Action AfterLevelRendered;
		private bool RequireReload = false;
		private int LoadedZ = int.MinValue;
		private int BackgroundBlockSize = Const.CEL;
		private IRect CullingCameraRect = default;
		private IRect ParallaxRect = default;
		private IRect CameraRect = default;


		#endregion




		#region --- API ---


		[OnGameInitialize(-128)]
		public static void OnGameInitialize () {
			Front = new WorldSquad();
			Behind = new WorldSquad();
			Util.LinkEventWithAttribute<OnMapChannelChangedAttribute>(typeof(WorldSquad), nameof(OnMapChannelChanged));
			Util.LinkEventWithAttribute<BeforeLevelRenderedAttribute>(typeof(WorldSquad), nameof(BeforeLevelRendered));
			Util.LinkEventWithAttribute<AfterLevelRenderedAttribute>(typeof(WorldSquad), nameof(AfterLevelRendered));
			SetMapChannel(MapChannel.BuiltIn);
		}


		[OnGameUpdate(-64)]
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
			if (isBehind) {
				CellRenderer.SetLayerToBehind();
			} else {
				CellRenderer.SetLayerToDefault();
			}
			var viewPos = Stage.SpawnRect.CenterInt();
			IRect unitRect_Entity;
			IRect unitRect_Level;
			CameraRect = CellRenderer.CameraRect;
			var cullingPadding = Stage.GetCameraCullingPadding();
			CullingCameraRect = CameraRect.Expand(cullingPadding);
			const float PARA_01 = Const.SQUAD_BEHIND_PARALLAX / 1000f;

			if (!isBehind) {
				// Current
				unitRect_Entity = Stage.SpawnRect.ToUnit();
				unitRect_Level = unitRect_Entity.Expand(Const.LEVEL_SPAWN_PADDING_UNIT);
			} else {
				// Behind
				BackgroundBlockSize = (Const.CEL / PARA_01).CeilToInt();
				var parallax = ((Float2)CameraRect.size * ((PARA_01 - 1f) / 2f)).CeilToInt();
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
			var midZone = new IRect(
				(Int2)Worlds[1, 1].WorldPosition * Const.MAP * Const.CEL,
				new Int2(Const.MAP * Const.CEL, Const.MAP * Const.CEL)
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
									Draw_Behind(bg, i, j, false);
								} else {
									DrawBackgroundBlock(bg, i, j);
								}
							}
							var lv = world.Levels[index];
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

			// Entity & Element
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

					// Entity
					for (int j = d; j < u; j++) {
						int localY = j - worldUnitRect.y;
						int index = localY * Const.MAP + (l - worldUnitRect.x);
						for (int i = l; i < r; i++, index++) {
							var entityID = world.Entities[index];
							if (entityID != 0) {
								if (!isBehind) {
									DrawEntity(entityID, i, j, z);
								} else if (Stage.RequireDrawEntityBehind(entityID, i, j, z)) {
									Draw_Behind(entityID, i, j, true);
								}
							}

						}
					}

					// Element
					if (ShowElement && !isBehind) {
						for (int j = d; j < u; j++) {
							int localY = j - worldUnitRect.y;
							int index = localY * Const.MAP + (l - worldUnitRect.x);
							for (int i = l; i < r; i++, index++) {
								var elementID = world.Elements[index];
								if (elementID != 0) {
									DrawElement(elementID, i, j);
								}
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


		public static void SetMapChannel (MapChannel newChannel, string folderName = "") {

			if (SaveBeforeReload) Front.SaveToFile();

			MapRoot = newChannel switch {
				MapChannel.BuiltIn => AngePath.BuiltInMapRoot,
				MapChannel.User => AngePath.UserMapRoot,
				MapChannel.Procedure => Util.CombinePaths(AngePath.ProcedureMapRoot, folderName),
				MapChannel.Download => Util.CombinePaths(AngePath.DownloadMapRoot, folderName),
				_ => AngePath.BuiltInMapRoot,
			};
			Channel = newChannel;

			var viewPos = Stage.SpawnRect.CenterInt();
			Front.LoadSquadFromDisk(
				viewPos.x.UDivide(Const.MAP * Const.CEL),
				viewPos.y.UDivide(Const.MAP * Const.CEL),
				Stage.ViewZ,
				true
			);
			Behind.LoadSquadFromDisk(
				viewPos.x.UDivide(Const.MAP * Const.CEL),
				viewPos.y.UDivide(Const.MAP * Const.CEL),
				Stage.ViewZ,
				true
			);
			Front.RequireReload = false;
			Behind.RequireReload = false;

			OnMapChannelChanged?.Invoke(newChannel);
		}


		// Get Set Block
		public Int4 GetTriBlockAt (int unitX, int unitY) {
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
			return type switch {
				BlockType.Entity => world.Entities[localY * Const.MAP + localX],
				BlockType.Level => world.Levels[localY * Const.MAP + localX],
				BlockType.Background => world.Backgrounds[localY * Const.MAP + localX],
				BlockType.Element => world.Elements[localY * Const.MAP + localX],
				_ => throw new System.NotImplementedException(),
			};
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


		public void SetBlockAt (int unitX, int unitY, int entityID, int levelID, int backgroundID, int elementID) {
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
				CellRenderer.Draw(id, rect);
			}
			// Collider for Oneway
			if (CellRenderer.TryGetSprite(id, out var sp) && AngeUtil.IsOnewayTag(sp.Tag)) {
				CellPhysics.FillBlock(
					PhysicsLayer.LEVEL, id,
					new IRect(
						unitX * Const.CEL,
						unitY * Const.CEL,
						Const.CEL,
						Const.CEL
					).Shrink(
						sp.GlobalBorder.left, sp.GlobalBorder.right, sp.GlobalBorder.down, sp.GlobalBorder.up
					),
					true, sp.Tag
				);
			}
		}


		private void DrawLevelBlock (int id, int unitX, int unitY, bool behind) {
			var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
			if (CullingCameraRect.Overlaps(rect)) {
				CellRenderer.Draw(id, rect);
			}
			if (!behind) {
				// Collider
				if (!CellRenderer.TryGetSprite(id, out var sp)) return;
				if (sp.Tag == SpriteTag.DAMAGE_TAG) {
					CellPhysics.FillBlock(PhysicsLayer.DAMAGE, id, rect.Expand(1), true, 1);
				}
				rect = rect.Shrink(
					sp.GlobalBorder.left, sp.GlobalBorder.right, sp.GlobalBorder.down, sp.GlobalBorder.up
				);
				CellPhysics.FillBlock(PhysicsLayer.LEVEL, id, rect, sp.IsTrigger, sp.Tag);
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
				var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
				if (!CullingCameraRect.Overlaps(rect)) return;
				if (
					CellRenderer.TryGetSprite(id, out var sprite) ||
					CellRenderer.TryGetSpriteFromGroup(id, 0, out sprite)
				) {
					rect = rect.Fit(sprite, sprite.PivotX, sprite.PivotY);
					CellRenderer.Draw(sprite, rect);
				} else {
					CellRenderer.Draw(ENTITY_CODE, rect);
				}
			}
		}


		private void DrawElement (int id, int unitX, int unitY) {
			var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
			if (!CullingCameraRect.Overlaps(rect)) return;
			if (
				CellRenderer.TryGetSprite(id, out var sprite) ||
				CellRenderer.TryGetSpriteFromGroup(id, 0, out sprite)
			) {
				rect = rect.Fit(sprite, sprite.PivotX, sprite.PivotY);
				CellRenderer.Draw(sprite.GlobalID, rect);
			} else {
				CellRenderer.Draw(ENTITY_CODE, rect);
			}
		}


		private void Draw_Behind (int id, int unitX, int unitY, bool fixRatio) {
			var cameraRect = CameraRect;
			var rect = new IRect(
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
			var tint = Byte4.LerpUnclamped(
				Skybox.SkyTintBottomColor, Skybox.SkyTintTopColor,
				Util.InverseLerp(cameraRect.yMin, cameraRect.yMax, rect.y + rect.height / 2)
			);

			tint.a = BehindAlpha;
			CellRenderer.Draw(sprite, rect, tint, 0);
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
					var pos = new Int3(worldX + i - 1, worldY + j - 1, worldZ);
					if (!forceLoad && world.WorldPosition == pos) continue;
					world.LoadFromDisk(MapRoot, pos.x, pos.y, pos.z);
				}
			}
			LoadedZ = worldZ;

		}


		#endregion




	}
}
