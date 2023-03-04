using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using AngeliaFramework.Editor;
using UnityEngine;


namespace Yaya.Editor {
	public class YayaArtworkExtension : IRefreshEvent {


		public string Message => "Yaya Refresh Events";


		public void Refresh () {
			CreateCameraScrollMetaFile();
		}


		private void CreateCameraScrollMetaFile () {

			// Entity ID
			var targetPool = new Dictionary<int, Vector2Int>();
			foreach (var type in typeof(eCameraAutoScroll).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is eCameraAutoScroll instance) {
					targetPool.TryAdd(
						type.AngeHash(),
						new Vector2Int((int)instance.DirectionX, (int)instance.DirectionY)
					);
				}
			}

			// Get All Pos
			var allPos = new Dictionary<Vector3Int, Vector2Int>();
			foreach (var filePath in Util.EnumerateFiles(Const.BuiltInMapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!World.GetWorldPositionFromName(Util.GetNameWithoutExtension(filePath), out var worldPos)) continue;
					foreach (var (id, x, y) in World.EditorOnly_ForAllEntities(filePath)) {
						if (!targetPool.TryGetValue(id, out var direction)) continue;
						if (direction == Vector2Int.zero) continue;
						allPos.Add(
							new(worldPos.x * Const.MAP + x, worldPos.y * Const.MAP + y, worldPos.z),
							direction
						);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Get all Entrance
			var allEntrance = new List<Vector3Int>();
			foreach (var (pos, dir) in allPos) {

				if (CheckForPrevTarget(new(-1, -1), pos.z)) continue;
				if (CheckForPrevTarget(new(-1, 0), pos.z)) continue;
				if (CheckForPrevTarget(new(-1, 1), pos.z)) continue;
				if (CheckForPrevTarget(new(0, -1), pos.z)) continue;
				if (CheckForPrevTarget(new(0, 1), pos.z)) continue;
				if (CheckForPrevTarget(new(1, -1), pos.z)) continue;
				if (CheckForPrevTarget(new(1, 0), pos.z)) continue;
				if (CheckForPrevTarget(new(1, 1), pos.z)) continue;

				allEntrance.Add(pos);

				// Func
				bool CheckForPrevTarget (Vector2Int direction, int z) {
					if (direction == dir) return false;
					for (int i = 1; i < eCameraAutoScroll.MAX_LEN; i++) {
						var _pos = new Vector3Int(
							pos.x + direction.x * i,
							pos.y + direction.y * i,
							z
						);
						if (allPos.ContainsKey(_pos)) {
							return true;
						}
					}
					return false;
				}
			}

			// to File
			AngeUtil.SaveJson(new eCameraAutoScroll.CameraScrollMeta() {
				EntrancePositions = allEntrance.ToArray(),
			}, Const.MetaRoot, false);

		}


	}
}