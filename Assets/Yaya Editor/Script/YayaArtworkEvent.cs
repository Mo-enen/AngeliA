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
			CreateCheckPointFile();
		}


		private void CreateCheckPointFile () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;

			var world = new World();
			var blockCheckingWorld = new World();
			int TARGET_ID = typeof(eCheckPoint).AngeHash();

			// Get All Cp Positions
			var allCpPool = new Dictionary<Vector2Int, int>();
			foreach (var file in Util.GetFilesIn(game.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(file.FullName)) continue;
					for (int i = 0; i < Const.MAP_SIZE * Const.MAP_SIZE; i++) {
						if (world.Entities[i] != TARGET_ID) continue;
						var cpPos = new Vector2Int(
							world.WorldPosition.x * Const.MAP_SIZE + i % Const.MAP_SIZE,
							world.WorldPosition.y * Const.MAP_SIZE + i / Const.MAP_SIZE
						);
						var cpBlockPos = cpPos;
						cpBlockPos.y--;
						var cpBlockWorldPos = cpBlockPos.UDivide(Const.MAP_SIZE);
						int bIndex = cpBlockPos.y.UMod(Const.MAP_SIZE) * Const.MAP_SIZE + cpBlockPos.x.UMod(Const.MAP_SIZE);
						int blockID = 0;
						if (cpBlockWorldPos == world.WorldPosition) {
							blockID = world.Level[bIndex];
						} else if (blockCheckingWorld.LoadFromDisk(game.MapRoot, cpBlockWorldPos.x, cpBlockWorldPos.y)) {
							blockID = blockCheckingWorld.Level[bIndex];
						} else continue;
						allCpPool.Add(cpPos, blockID);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Write Position File
			try {
				var cpList = new List<CheckPointConfig.Data>();
				foreach (var (pos, id) in allCpPool) {
					if (id == 0) continue;
					for (int i = 1; i < Const.MAP_SIZE; i++) {
						if (AngeEditorUtil.TryGetMapSystemNumber(pos.x, pos.y + i, out int cpIndex)) {
							cpList.Add(new() {
								Index = cpIndex,
								X = pos.x,
								Y = pos.y,
								IsAltar = allCpPool.ContainsKey(new(pos.x, pos.y + 1)) && !allCpPool.ContainsKey(new(pos.x, pos.y - 1)),
							});
							break;
						}
					}
				}
				var jsonData = new CheckPointConfig() { CPs = cpList.ToArray() };
				Util.TextToFile(
					JsonUtility.ToJson(jsonData, true),
					Util.CombinePaths(game.JsonConfigRoot, $"{nameof(CheckPointConfig)}.json")
				);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


	}


}