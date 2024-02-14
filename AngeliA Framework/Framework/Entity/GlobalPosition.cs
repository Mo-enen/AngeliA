using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace AngeliA.Framework {
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


		public static void SetPosition (int id, Int3 unitPosition) {
			if (IdPosEcho.ContainsPair(id, unitPosition)) return;
			IdPosEcho[id] = unitPosition;
			IsDirty = true;
		}


		public static void RemoveID (int id) => IdPosEcho.Remove(id);


		public static void RemovePosition (Int3 unitPosition) => IdPosEcho.Remove(unitPosition);


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
			foreach (var pos in Meta.Positions) {
				IdPosEcho.TryAdd(pos.ID, new Int3(pos.UnitPositionX, pos.UnitPositionY, pos.UnitPositionZ));
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




	}
}
