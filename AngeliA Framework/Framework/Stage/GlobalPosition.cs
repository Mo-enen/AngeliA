using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace AngeliaFramework {
	public interface IGlobalPosition {




		#region --- SUB ---


		public class Position {
			[JsonProperty("i")] public int ID;
			[JsonProperty("x")] public int UnitPositionX;
			[JsonProperty("y")] public int UnitPositionY;
			[JsonProperty("z")] public int UnitPositionZ;
		}


		public class GlobalPositionMeta {
			public List<Position> Positions = new();
		}


		#endregion




		#region --- VAR ---


		// Data
		private static readonly EchoDictionary<int, Int3> IdPosEcho = new();
		private static readonly Dictionary<Int3, List<Position>> WorldPosPool = new();
		private static readonly HashSet<int> AllGlobalPositionID = new();
		private static readonly GlobalPositionMeta Meta = new();
		private static string LoadedMapRoot = string.Empty;
		private static bool IsDirty = false;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-1024)]
		public static void OnGameInitialize () {
			AllGlobalPositionID.Clear();
			foreach (var type in typeof(IGlobalPosition).AllClassImplemented()) {
				AllGlobalPositionID.TryAdd(type.AngeHash());
			}
		}


		[OnProjectOpen]
		public static void OnProjectOpen () => LoadFromDisk(WorldSquad.MapRoot);


		[OnMapChannelChanged]
		public static void OnMapChannelChanged (MapChannel _) => LoadFromDisk(WorldSquad.MapRoot);


		#endregion




		#region --- API ---


		public static bool IsGlobalPositionEntity (int id) => AllGlobalPositionID.Contains(id);


		public static bool HasID (int id) => IdPosEcho.ContainsKey(id);


		public static bool HasPosition (Int3 pos) => IdPosEcho.ContainsValue(pos);


		public static bool TryGetPositionFromID (int id, out Int3 unitPosition) => IdPosEcho.TryGetValue(id, out unitPosition);


		public static bool TryGetIdFromPosition (Int3 unitPosition, out int id) => IdPosEcho.TryGetKey(unitPosition, out id);


		public static bool TryGetPositionList (Int3 worldPos, out List<Position> list) {
			bool hasValue = WorldPosPool.TryGetValue(worldPos, out list);
			return hasValue && list != null;
		}


		public static void SetPosition (int id, Int3 unitPosition) {

			if (IdPosEcho.ContainsPair(id, unitPosition)) return;

			var newWorldPos = new Int3(unitPosition.x.UDivide(Const.MAP), unitPosition.y.UDivide(Const.MAP), unitPosition.z);
			bool requireFillIntoList = false;

			// Overriding ID
			if (IdPosEcho.TryGetKey(unitPosition, out int idFromNewPos)) {
				IdPosEcho.Remove(idFromNewPos);
				var _overrideWorldPos = new Int3(unitPosition.x.UDivide(Const.MAP), unitPosition.y.UDivide(Const.MAP), unitPosition.z);
				if (WorldPosPool.TryGetValue(_overrideWorldPos, out var overrideList)) {
					for (int i = 0; i < overrideList.Count; i++) {
						if (overrideList[i].ID == idFromNewPos) {
							overrideList.RemoveAt(i);
							break;
						}
					}
				}
			}

			// Remove Old World Pos
			if (IdPosEcho.TryGetValue(id, out var oldUnitPos)) {

				// ID Already Exists
				var oldWorldPos = new Int3(oldUnitPos.x.UDivide(Const.MAP), oldUnitPos.y.UDivide(Const.MAP), oldUnitPos.z);
				if (newWorldPos == oldWorldPos) {
					// Change World Cache
					if (WorldPosPool.TryGetValue(oldWorldPos, out var oldList)) {
						for (int i = 0; i < oldList.Count; i++) {
							var data = oldList[i];
							if (data.ID == id) {
								data.UnitPositionX = unitPosition.x;
								data.UnitPositionY = unitPosition.y;
								data.UnitPositionZ = unitPosition.z;
								break;
							}
						}
					}
				} else {
					// Remove from Old List
					if (WorldPosPool.TryGetValue(oldWorldPos, out var oldList)) {
						for (int i = 0; i < oldList.Count; i++) {
							if (oldList[i].ID == id) {
								oldList.RemoveAt(i);
								break;
							}
						}
					}
					// Fill into New List
					requireFillIntoList = true;
				}
			} else {
				// ID Not Exists
				requireFillIntoList = true;
			}

			// Fill World Pos Cache into New List
			if (requireFillIntoList) {
				if (!WorldPosPool.TryGetValue(newWorldPos, out var newList)) {
					WorldPosPool.Add(newWorldPos, newList = new());
				}
				newList.Add(new Position() {
					ID = id,
					UnitPositionX = unitPosition.x,
					UnitPositionY = unitPosition.y,
					UnitPositionZ = unitPosition.z,
				});
			}

			// Set Value
			IdPosEcho[id] = unitPosition;

			IsDirty = true;
		}


		public static void RemoveID (int id) {
			if (!IdPosEcho.Remove(id, out var pos)) return;
			var worldPos = new Int3(pos.x.UDivide(Const.MAP), pos.y.UDivide(Const.MAP), pos.z);
			if (WorldPosPool.TryGetValue(worldPos, out var list)) {
				for (int i = 0; i < list.Count; i++) {
					if (list[i].ID == id) {
						list.RemoveAt(i);
						break;
					}
				}
			}
		}


		public static void RemovePosition (Int3 unitPosition) {
			if (!IdPosEcho.Remove(unitPosition, out int id)) return;
			var worldPos = new Int3(unitPosition.x.UDivide(Const.MAP), unitPosition.y.UDivide(Const.MAP), unitPosition.z);
			if (WorldPosPool.TryGetValue(worldPos, out var list)) {
				for (int i = 0; i < list.Count; i++) {
					if (list[i].ID == id) {
						list.RemoveAt(i);
						break;
					}
				}
			}
		}


		// File
		public static void LoadFromDisk (string rootFolder) {
			if (string.IsNullOrEmpty(rootFolder) || LoadedMapRoot == rootFolder) return;
			LoadedMapRoot = rootFolder;
			IsDirty = false;
			// File >> Meta
			Meta.Positions.Clear();
			JsonUtil.OverrideJson(rootFolder, Meta);
			// Meta >> Pool
			IdPosEcho.Clear();
			WorldPosPool.Clear();
			foreach (var pos in Meta.Positions) {
				var unitPos = new Int3(pos.UnitPositionX, pos.UnitPositionY, pos.UnitPositionZ);
				IdPosEcho.TryAdd(pos.ID, unitPos);
				var worldPos = new Int3(unitPos.x.UDivide(Const.MAP), unitPos.y.UDivide(Const.MAP), unitPos.z);
				if (!WorldPosPool.TryGetValue(worldPos, out var list)) {
					WorldPosPool.Add(worldPos, list = new());
				}
				list.Add(pos);
			}
		}


		public static void SaveToDisk (string rootFolder) {
			if (string.IsNullOrEmpty(rootFolder)) return;
			// Pool >> Meta
			if (IsDirty) {
				IsDirty = false;
				Meta.Positions.Clear();
				foreach (var (id, pos) in IdPosEcho) {
					Meta.Positions.Add(new Position() {
						ID = id,
						UnitPositionX = pos.x,
						UnitPositionY = pos.y,
						UnitPositionZ = pos.z,
					});
				}
			}
			// Meta >> File
			JsonUtil.SaveJson(Meta, rootFolder);
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
