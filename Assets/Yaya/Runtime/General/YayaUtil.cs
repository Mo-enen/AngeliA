using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaUtil {


		public static void DrawSegmentHealthBar (
			int x, int y,
			int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode,
			int hp, int maxHP, int prevHP = int.MinValue
		) {

			const int SIZE = Const.HALF;
			const int COLUMN = 4;
			const int MAX = 8;

			int maxHp = Mathf.Min(maxHP, MAX);
			int left = x - SIZE * COLUMN / 4;

			// Draw Hearts
			var rect = new RectInt(0, 0, SIZE / 2, SIZE);
			bool isLeft = true;
			for (int i = 0; i < maxHp; i++) {
				rect.x = left + (i % COLUMN) * SIZE / 2;
				rect.y = y - (i / COLUMN + 1) * SIZE;
				if (i < hp) {
					// Heart
					CellRenderer.Draw(isLeft ? heartLeftCode : heartRightCode, rect).Z = 0;
				} else {
					// Empty Heart
					CellRenderer.Draw(isLeft ? emptyHeartLeftCode : emptyHeartRightCode, rect).Z = 0;
					// Spawn Drop Particle
					if (i < prevHP) {
						eYayaDroppingHeart heart;
						if (isLeft) {
							heart = Game.Current.SpawnEntity<eYayaDroppingHeartLeft>(rect.x, rect.y);
						} else {
							heart = Game.Current.SpawnEntity<eYayaDroppingHeartRight>(rect.x, rect.y);
						}
						if (heart != null) {
							heart.Width = rect.width + 8;
							heart.Height = rect.height + 16;
						}
					}
				}
				isLeft = !isLeft;
			}
		}


		public static Vector2Int GetFlyingFormation (Vector2Int pos, int column, int instanceIndex) {

			int sign = instanceIndex % 2 == 0 ? -1 : 1;
			int _row = instanceIndex / 2 / column;
			int _column = (instanceIndex / 2 % column + 1) * sign;
			int rowSign = (_row % 2 == 0) == (sign == 1) ? 1 : -1;

			int instanceOffsetX = _column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
			int instanceOffsetY = _row * Const.CEL + Const.CEL - _column.Abs() * Const.HALF / 3;

			// Result
			//var pos = OwnerPosTrail[(OwnerPosTrailIndex + 1).UMod(OwnerPosTrail.Length)];
			return new(pos.x + instanceOffsetX, pos.y + instanceOffsetY);
		}


		// Meta
		public static void CreateCameraScrollMetaFile (string mapRoot) {

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
			foreach (var filePath in Util.EnumerateFiles(mapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!World.GetWorldPositionFromName(Util.GetNameWithoutExtension(filePath), out var worldPos)) continue;
					foreach (var (id, x, y) in World.ForAllEntities(filePath)) {
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
			}, mapRoot, false);

		}


	}
}