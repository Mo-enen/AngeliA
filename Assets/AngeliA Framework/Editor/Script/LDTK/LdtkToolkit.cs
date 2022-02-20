using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;


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
					bool success = LoadLdtkLevel(ldtk, mapRoot, tilesetPool);
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
			string mapRoot,
			Dictionary<string, Dictionary<Vector2Int, int>> spritePool
		) {

			// Ldtk >> Pool
			var worldPool = new Dictionary<Vector2Int, World>();
			foreach (var level in project.levels) {
				foreach (var layer in level.layerInstances) {
					var tName = Util.GetNameWithoutExtension(layer.__tilesetRelPath);
					if (!spritePool.ContainsKey(tName)) continue;
					var sPool = spritePool[tName];
					foreach (var tile in layer.autoLayerTiles) {
						if (!sPool.ContainsKey(new(tile.src[0], tile.src[1]))) {

							Debug.LogWarning(tName + " " + new Vector2Int(tile.src[0], tile.src[1]));

						}
					}
				}
			}

			// Pool >> Maps (add into, no replace)



			return true;
		}


	}
}
