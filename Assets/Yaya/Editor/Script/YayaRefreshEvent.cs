using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using AngeliaFramework.Editor;
using UnityEngine;


namespace Yaya.Editor {
	public class YayaArtworkExtension : IRefreshEvent {


		public string Message => "Yaya Refresh Events";


		public void Refresh () {

			CreateTeleporterMetaFile();
		}


		private void CreateTeleporterMetaFile () {

			// Entity ID
			var targetHash = new HashSet<int>();
			foreach (var type in typeof(eCheckAltar).AllChildClass()) {
				targetHash.TryAdd(type.AngeHash());
			}

			// Get All tele Positions
			var list = new List<eCheckAltar.AltarMeta.Position>();
			foreach (var filePath in Util.EnumerateFiles(Const.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!World.GetWorldPositionFromName(Util.GetNameWithoutExtension(filePath), out var worldPos)) continue;
					foreach (var (id, x, y) in World.EditorOnly_ForAllEntities(filePath)) {
						if (!targetHash.Contains(id)) continue;
						list.Add(new eCheckAltar.AltarMeta.Position() {
							EntityID = id,
							X = worldPos.x * Const.MAP + x,
							Y = worldPos.y * Const.MAP + y,
							Z = worldPos.z,
						});
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			// Write Position File
			AngeUtil.SaveMeta(new eCheckAltar.AltarMeta() { Positions = list.ToArray(), });
		}


	}
}