using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliaFramework;
using AngeliaFramework.Editor;
using UnityEngine;
using Moenen.Standard;


namespace Yaya.Editor {


	public class CheckPointArtworkExtension : IArtworkEvent {


		public string Message => "Yaya Artwork Events";


		public void OnArtworkSynced () {
			CreateCheckPointMetaFile();
			CreateMetaFilesFromDefault();
		}


		private void CreateCheckPointMetaFile () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;

			var world = new World();
			int TARGET_ID = typeof(eCheckPoint).AngeHash();

			// Get All Cp Positions
			var allCpPool = new HashSet<Vector2Int>();
			foreach (var file in Util.GetFilesIn(game.Universe.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(file.FullName)) continue;
					for (int i = 0; i < Const.MAP_SIZE * Const.MAP_SIZE; i++) {
						if (world.Entities[i] != TARGET_ID) continue;
						allCpPool.Add(new Vector2Int(
							world.WorldPosition.x * Const.MAP_SIZE + i % Const.MAP_SIZE,
							world.WorldPosition.y * Const.MAP_SIZE + i / Const.MAP_SIZE
						));
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Write Position File
			try {
				var cpList = new List<YayaMeta.Data>();
				foreach (var pos in allCpPool) {
					if (allCpPool.Contains(new(pos.x, pos.y - 1))) continue;
					for (int i = 1; i < Const.MAP_SIZE; i++) {
						if (AngeEditorUtil.TryGetMapSystemNumber(pos.x, pos.y + i, out int cpIndex)) {
							cpList.Add(new() {
								Index = cpIndex,
								X = pos.x,
								Y = pos.y,
								IsAltar = allCpPool.Contains(new(pos.x, pos.y + 1)) && !allCpPool.Contains(new(pos.x, pos.y - 1)),
							});
							break;
						}
					}
				}
				game.SaveMeta(new YayaMeta() { CPs = cpList.ToArray(), });
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		private void CreateMetaFilesFromDefault () {
			var yaya = Object.FindObjectOfType<Yaya>();
			if (yaya == null) return;
			yaya.SaveMeta(Util.GetFieldValue(yaya, "m_DefaultPhysicsMeta") as PhysicsMeta);
		}


	}


}