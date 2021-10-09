using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace AngeliaFramework {



	public class TestPrefabA : Prefab {
		public override void BeforeSave () { }
		public override void OnLoaded () { }
	}


	public class TestPrefabB : Prefab {
		public override void BeforeSave () { }
		public override void OnLoaded () { }
	}



	public class TestPrefabC : Prefab {
		public override void BeforeSave () { }
		public override void OnLoaded () { }
	}


	public abstract class Prefab {


		// VAR
		public ushort GlobalID = 0;
		public string RootPath = "";


		// MSG
		public abstract void OnLoaded ();
		public abstract void BeforeSave ();


	}


	public static class PrefabStream {




		#region --- SUB ---


		[System.Serializable]
		private class PrefabInfo {
			public string Type;
			public ushort GLobalID;
		}


		#endregion




		#region --- VAR ---


		// Data
		private static Dictionary<string, System.Type> TypeMap = new Dictionary<string, System.Type>();
		private static Dictionary<ushort, Prefab> Pool = new Dictionary<ushort, Prefab>();


		#endregion




		#region --- API ---


		// Pool
		public static void LoadPool (string prefabRootPath) {
			// Type Map
			TypeMap.Clear();
			foreach (var type in Util.GetAllClass(typeof(Prefab))) {
				TypeMap.TryAdd(GetTypeString(type), type);
			}
			// Pool
			Pool.Clear();
			foreach (var folder in Util.GetFoldersIn(prefabRootPath, true)) {
				var prefab = LoadPrefab(folder.FullName);
				if (prefab != null) {
					Pool.TryAdd(prefab.GlobalID, prefab);
				}
			}
		}


		public static void ClearPool () => Pool.Clear();


		public static ushort CreateEmptyPrefab (System.Type type) {




			return 0;
		}


		// Prefab and File
		public static Prefab LoadPrefab (string path) {
			Prefab prefab = null;
			try {
				string json = Util.FileToText(Util.CombinePaths(path, "Info.json"));
				var info = JsonUtility.FromJson<PrefabInfo>(json);
				if (info == null || !TypeMap.ContainsKey(info.Type)) { return null; }
				prefab = System.Activator.CreateInstance(TypeMap[info.Type]) as Prefab;
				prefab.RootPath = path;
				prefab.GlobalID = info.GLobalID;
				prefab.OnLoaded();
			} catch { }
			return prefab;
		}


		public static void SavePrefab (Prefab prefab, string path) {
			try {
				prefab.RootPath = path;
				prefab.BeforeSave();
				var info = new PrefabInfo() {
					GLobalID = prefab.GlobalID,
					Type = GetTypeString(prefab.GetType()),
				};
				Util.TextToFile(
					JsonUtility.ToJson(info, true),
					Util.CombinePaths(path, "Info.json")
				);
			} catch { }
		}


		#endregion




		#region --- LGC ---


		private static string GetTypeString (System.Type type) => $"{type.Assembly.GetName().Name}-{type.Namespace}-{type.Name}";


		#endregion




	}
}
