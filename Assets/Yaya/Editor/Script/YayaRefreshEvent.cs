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
			CreateYayaMetaFile();
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
			foreach (var file in Util.GetFilesIn(Const.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					World.EditorOnly_ForAllBlocks(file.FullName, entity: (id, pos) => {
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
			Game.SaveMeta(new eCheckAltar.CheckPointMeta() { CPs = cpList.ToArray(), });
		}


		private void CreateYayaMetaFile () {
			try {
				var yaya = Object.FindObjectOfType<Yaya>();
				if (yaya == null) return;
				Game.SaveMeta(Util.GetFieldValue(yaya, "m_YayaMeta") as YayaMeta);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


	}


}