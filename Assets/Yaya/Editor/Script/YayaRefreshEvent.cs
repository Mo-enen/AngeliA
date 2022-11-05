using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using AngeliaFramework.Editor;
using Moenen.Standard;
using UnityEditor;
using UnityEngine;


namespace Yaya.Editor {


	public class CheckPointArtworkExtension : IRefreshEvent {


		public string Message => "Yaya Refresh Events";


		public void Refresh () {
			CreateCheckPointMetaFile();
			AddAlwaysIncludeShader();
		}


		private void CreateCheckPointMetaFile () {

			// Altar Entity ID
			var world = new World();
			var targetHash = new HashSet<int>();
			foreach (var type in typeof(eCheckAltar).AllChildClass()) {
				targetHash.TryAdd(type.AngeHash());
			}

			// Get All Cp Positions
			var cpList = new List<eCheckAltar.CheckPointMeta.Data>();
			foreach (var filePath in Util.EnumerateFiles(Const.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					World.EditorOnly_ForAllBlocks(filePath, entity: (id, pos) => {
						if (!targetHash.Contains(id)) return;
						cpList.Add(new eCheckAltar.CheckPointMeta.Data() {
							EntityID = id,
							X = world.WorldPosition.x * Const.MAP + pos.x,
							Y = world.WorldPosition.y * Const.MAP + pos.y,
							Z = world.WorldPosition.z,
						});
					});
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Write Position File
			Game.SaveMeta(
				new eCheckAltar.CheckPointMeta() { CPs = cpList.ToArray(), },
				"", "", false
			);
		}


		private void AddAlwaysIncludeShader () {
			foreach (var guid in AssetDatabase.FindAssets("t:shader")) {
				try {
					string path = AssetDatabase.GUIDToAssetPath(guid);
					var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
					if (shader == null) continue;
					if (shader.name.StartsWith("Yaya/", System.StringComparison.OrdinalIgnoreCase)) {
						EditorUtil.AddAlwaysIncludedShader(shader.name);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			AssetDatabase.SaveAssets();
		}


	}


}