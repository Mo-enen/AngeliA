using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using AngeliaFramework.Editor;
using UnityEngine;


namespace Yaya.Editor {


	public class CheckPointArtworkExtension : IRefreshEvent {


		public string Message => "Yaya Refresh Events";


		public void Refresh () {
			CreateCheckPointMetaFile();
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
			foreach (var filePath in Util.EnumerateFiles(AngePath.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
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
			AngeUtil.SaveMeta(new eCheckAltar.CheckPointMeta() { CPs = cpList.ToArray(), });
		}


	}


}