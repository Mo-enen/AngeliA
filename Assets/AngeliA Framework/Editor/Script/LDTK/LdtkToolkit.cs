using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;
using Moenen.Standard;


namespace LdtkToAngeliA {
	public static class LdtkToolkit {


		private static string LdtkRoot => Util.CombinePaths(Util.GetParentPath(Application.dataPath), "_Level");


		[MenuItem("AngeliA/Command/Save Texture for LDTK")]
		public static void SaveTextureForLDTK () {
			string rootPath = Util.CombinePaths(LdtkRoot, "Texture");
			Util.CreateFolder(rootPath);
			foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(SpriteSheet)}")) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var sheet = AssetDatabase.LoadAssetAtPath<SpriteSheet>(path);
				if (sheet.Texture == null || !sheet.Texture.isReadable) continue;
				Util.ByteToFile(
					sheet.Texture.EncodeToPNG(),
					Util.CombinePaths(rootPath, sheet.Texture.name + ".png")
				);
			}
		}


		public static void ReloadAllLevels () {

			// Delete Old Maps
			var mapRoot = Util.CombinePaths(Application.dataPath, "Resources", "Map");
			Util.DeleteFolder(mapRoot);
			Util.CreateFolder(mapRoot);
			AssetDatabase.Refresh();

			// Get Sprite Pool
			var tilesetPool = new Dictionary<string, Dictionary<Vector2Int, int>>();
			foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(SpriteSheet)}")) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var sheet = AssetDatabase.LoadAssetAtPath<SpriteSheet>(path);
				if (sheet.Texture == null || !sheet.Texture.isReadable) continue;
				var tPath = AssetDatabase.GetAssetPath(sheet.Texture);
				var spritePool = new Dictionary<Vector2Int, int>();
				int tHeight = sheet.Texture.height;
				foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(tPath)) {
					if (obj is Sprite sp) {
						var _rect = sp.rect.ToRectInt();
						var _pos = _rect.position;
						_pos.y = tHeight - (_pos.y + _rect.height - 1) - 1;
						spritePool.TryAdd(_pos, sp.name.ACode());
					}
				}
				tilesetPool.TryAdd(sheet.Texture.name, spritePool);
			}

			// Load Levels
			int successCount = 0;
			int errorCount = 0;
			foreach (var file in Util.GetFilesIn(LdtkRoot, false, "*.ldtk")) {
				try {
					var json = Util.FileToText(file.FullName);
					var ldtk = JsonUtility.FromJson<LdtkProject>(json);
					bool success = LoadLdtkLevel(ldtk, tilesetPool);
					if (success) successCount++;
				} catch (System.Exception ex) {
					Debug.LogException(ex);
					errorCount++;
				}
			}

			// Dialog
			if (successCount + errorCount == 0) {
				EditorUtility.DisplayDialog("Done", "No Level Processesed.", "OK");
			} else {
				string message = "All Maps Reloaded. ";
				if (successCount > 0) {
					message += successCount + " success, ";
				}
				if (errorCount > 0) {
					message += errorCount + " failed.";
				}
				if (errorCount > 0) {
					Debug.LogWarning(message);
				}
			}
		}


		private static bool LoadLdtkLevel (LdtkProject project, Dictionary<string, Dictionary<Vector2Int, int>> spritePool) {
			// Ldtk >> Custom Data Pool
			var customDataPool = new Dictionary<int, (bool trigger, int tag)>();
			foreach (var tileset in project.defs.tilesets) {
				string tName = Util.GetNameWithoutExtension(tileset.relPath);
				if (!spritePool.ContainsKey(tName)) continue;
				var sPool = spritePool[tName];
				int gridX = tileset.__cWid;
				int gridY = tileset.__cHei;
				int space = tileset.spacing;
				int padding = tileset.padding;
				int gSize = tileset.tileGridSize;
				foreach (var data in tileset.customData) {
					int id = data.tileId;
					var tilePos = new Vector2Int(
						padding + (id % gridX) * (gSize + space),
						padding + (id / gridY) * (gSize + space)
					);
					if (sPool.TryGetValue(tilePos, out int blockID)) {
						var lines = data.data.Split('\n');

						// Is Trigger
						bool isTrigger = false;
						if (lines.Length > 0 && bool.TryParse(lines[0], out bool value)) {
							isTrigger = value;
						}

						// Tag String
						int tag = 0;
						if (lines.Length > 1) {
							tag = lines[1].ACode();
						}

						customDataPool.TryAdd(blockID, (isTrigger, tag));
					}
				}
			}

			// Ldtk >> World Pool
			var worldPool = new Dictionary<(int x, int y), World>();
			foreach (var level in project.levels) {
				int levelPosX = level.worldX;
				int levelPosY = level.worldY;
				foreach (var layer in level.layerInstances) {
					int gridSize = layer.__gridSize;
					int offsetX = levelPosX + layer.__pxTotalOffsetX;
					int offsetY = levelPosY + layer.__pxTotalOffsetY;
					bool isLevel = IsLevelBlock(layer.__identifier);
					var tName = Util.GetNameWithoutExtension(layer.__tilesetRelPath);
					if (!spritePool.ContainsKey(tName) && layer.__type != "Entities") continue;
					TileInstance[] tiles = null;
					EntityInstance[] entities = null;
					switch (layer.__type) {
						case "Tiles":
							tiles = layer.gridTiles;
							break;
						case "IntGrid":
						case "AutoLayer":
							tiles = layer.autoLayerTiles;
							break;
						case "Entities":
							entities = layer.entityInstances;
							break;
					}
					if (tiles != null) {
						var sPool = spritePool[tName];
						foreach (var tile in tiles) {
							var srcPos = new Vector2Int(tile.src[0], tile.src[1]);
							if (sPool.ContainsKey(srcPos)) {
								ForLdtkTile(
									tile.px[0] + offsetX,
									tile.px[1] + offsetY,
									(_localX, _localY, world) => SetBlock(world, _localX, _localY, srcPos)
								);
							}
						}
					} else if (entities != null) {
						foreach (var entity in entities) {
							ForLdtkTile(
								entity.px[0] - (int)(entity.__pivot[0] * entity.width) + offsetX,
								entity.px[1] - (int)(entity.__pivot[1] * entity.height) + offsetY,
								(_localX, _localY, world) => {
									ref var e = ref world.Entities[
										_localY * Const.WORLD_MAP_SIZE + _localX
									];
									e.TypeID = entity.__identifier.ACode();
								}
							);
						}
					}
					// Func
					void ForLdtkTile (int pixelX, int pixelY, System.Action<int, int, World> action) {
						int globalX = pixelX * Const.CELL_SIZE / gridSize;
						int globalY = -pixelY * Const.CELL_SIZE / gridSize - Const.CELL_SIZE;
						int unitX = globalX.AltDivide(Const.CELL_SIZE);
						int unitY = globalY.AltDivide(Const.CELL_SIZE);
						int worldX = unitX.AltDivide(Const.WORLD_MAP_SIZE);
						int worldY = unitY.AltDivide(Const.WORLD_MAP_SIZE);
						if (!worldPool.ContainsKey((worldX, worldY))) {
							worldPool.Add((worldX, worldY), new());
						}
						action(
							unitX.AltMode(Const.WORLD_MAP_SIZE),
							unitY.AltMode(Const.WORLD_MAP_SIZE),
							worldPool[(worldX, worldY)]
						);
					}
					void SetBlock (World world, int _localX, int _localY, Vector2Int srcPos) {
						var blocks = isLevel ? world.Level : world.Background;
						ref var block = ref blocks[_localY * Const.WORLD_MAP_SIZE + _localX];
						block.TypeID = spritePool[tName][srcPos];
						if (customDataPool.TryGetValue(block.TypeID, out (bool _isT, int _tag) _value)) {
							block.IsTrigger = _value._isT;
							block.Tag = _value._tag;
						} else {
							block.IsTrigger = false;
							block.Tag = 0;
						}
					}
				}
			}

			// World Pool >> Maps (add into, no replace)
			var mapPool = new Dictionary<(int x, int y), MapObject>();
			foreach (var pair in worldPool) {
				var (x, y) = pair.Key;
				var world = pair.Value;
				MapObject mapObj;
				if (mapPool.ContainsKey((x, y))) {
					mapObj = mapPool[(x, y)];
				} else {
					mapObj = ScriptableObject.CreateInstance<MapObject>();
					mapPool.Add((x, y), mapObj);
				}
				world.EditorOnly_SaveToDisk(mapObj, false);
			}
			foreach (var pair in mapPool) {
				var (x, y) = pair.Key;
				string mapName = $"{x}_{y}";
				AssetDatabase.CreateAsset(pair.Value, Util.CombinePaths("Assets", "Resources", "Map", mapName + ".asset"));
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return true;
		}


		private static bool IsLevelBlock (string id) => id.StartsWith("level", System.StringComparison.CurrentCultureIgnoreCase);


	}
}
