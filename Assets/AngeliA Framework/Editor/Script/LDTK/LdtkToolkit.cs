using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;
using Moenen.Standard;


namespace LdtkToAngeliA {
	public static class LdtkToolkit {


		[MenuItem("AngeliA/Command/Save Texture for LDTK")]
		private static void ReloadSheetAssets () {
			string rootPath = Util.CombinePaths(Util.GetParentPath(Application.dataPath), "_Level", "Texture");
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
			// Delete Maps
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
			foreach (var file in Util.GetFilesIn(Util.GetParentPath(Application.dataPath), false, "*.ldtk")) {
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
				EditorUtility.DisplayDialog("Done", message, "OK");
			}
		}


		private static bool LoadLdtkLevel (
			LdtkProject project,
			Dictionary<string, Dictionary<Vector2Int, int>> spritePool
		) {
			// Ldtk >> World Pool
			var worldPool = new Dictionary<(int x, int y), World>();
			foreach (var level in project.levels) {
				int levelPosX = level.worldX;
				int levelPosY = level.worldY;
				foreach (var layer in level.layerInstances) {
					int gridSize = layer.__gridSize;
					int offsetX = levelPosX + layer.__pxTotalOffsetX;
					int offsetY = levelPosY + layer.__pxTotalOffsetY;
					int blockLayerIndex = (int)LdtkLayerID_to_BlockLayer(layer.__identifier);
					var tName = Util.GetNameWithoutExtension(layer.__tilesetRelPath);
					if (!spritePool.ContainsKey(tName) && layer.__type != "Entities") continue;
					switch (layer.__type) {
						case "Tiles": {
							var sPool = spritePool[tName];
							foreach (var tile in layer.gridTiles) {
								var srcPos = new Vector2Int(tile.src[0], tile.src[1]);
								if (sPool.ContainsKey(srcPos)) {
									ForLdtkTile(
										tile.px[0] + offsetX,
										tile.px[1] + offsetY,
										(_localX, _localY, world) => {
											ref var block = ref world.Blocks[
												blockLayerIndex * Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE +
												_localY * Const.WORLD_MAP_SIZE + _localX
											];
											block.TypeID = sPool[srcPos];
										}
									);
								}
							}
							break;
						}

						case "IntGrid":
						case "AutoLayer": {
							var sPool = spritePool[tName];
							foreach (var tile in layer.autoLayerTiles) {
								var srcPos = new Vector2Int(tile.src[0], tile.src[1]);
								if (sPool.ContainsKey(srcPos)) {
									ForLdtkTile(
										tile.px[0] + offsetX,
										tile.px[1] + offsetY,
										(_localX, _localY, world) => {
											ref var block = ref world.Blocks[
												blockLayerIndex * Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE +
												_localY * Const.WORLD_MAP_SIZE + _localX
											];
											block.TypeID = sPool[srcPos];
										}
									);
								}
							}
							break;
						}

						case "Entities":
							foreach (var entity in layer.entityInstances) {
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
							break;
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


		private static BlockLayer LdtkLayerID_to_BlockLayer (string id) {
			for (int i = 0; i < Const.BLOCK_LAYER_COUNT; i++) {
				if (id.StartsWith(((BlockLayer)i).ToString())) {
					return (BlockLayer)i;
				}
			}
			return BlockLayer.Background;
		}


	}
}
