using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PixelJelly.Editor {
	public class JellyConfig : ScriptableObject {



		// SUB
		[System.Serializable]
		public struct SlotPair {
			public string Type;
			public string Namespace;
			public string Assembly;
			public string Slot;
			public SlotPair (string assembly, string @namespace, string type, string slot) {
				Assembly = assembly;
				Namespace = @namespace;
				Type = type;
				Slot = slot;
			}
		}


		[System.Serializable]
		public struct FavoriteBehaviourData {
			public string Type;
			public string Namespace;
			public string Assembly;
			public string Group;
			public FavoriteBehaviourData (string assembly, string @namespace, string type, string group) {
				Type = type;
				Namespace = @namespace;
				Assembly = assembly;
				Group = group;
			}
		}


		// Const
		public const string TITLE = "Pixel Jelly";
		public const string ROOT_NAME = "Pixel Jelly";

		// Api 
		public static JellyConfig Main {
			get {
				if (_Main == null) {
					string root = EditorUtil.GetRootPath(ROOT_NAME, "");
					if (string.IsNullOrEmpty(root)) {
						root = "Assets/Pixel Jelly Data";
					}
					string configPath = Util.FixPath(Util.CombinePaths(root, "Data", "Config.asset"));
					if (!Util.FileExists(configPath)) {
						var config = CreateInstance<JellyConfig>();
						Util.CreateFolder(Util.GetParentPath(configPath));
						AssetDatabase.CreateAsset(config, configPath);
						AssetDatabase.Refresh();
						AssetDatabase.SaveAssets();
						_Main = config;
					} else {
						_Main = AssetDatabase.LoadAssetAtPath<JellyConfig>(configPath);
					}
				}
				return _Main;
			}
		}
		public List<Color32> PixelEditorPalette {
			get {
				if (m_PixelEditorPalette == null || m_PixelEditorPalette.Count == 0) {
					m_PixelEditorPalette = GetDefaultPalette();
					EditorUtility.SetDirty(this);
					AssetDatabase.SaveAssets();
				}
				return m_PixelEditorPalette;
			}
			set => m_PixelEditorPalette = value;
		}
		public int FavoriteBehCount => m_FavoriteBehs.Count;

		// Short
		private static string BehaviourAssetRoot {
			get {
				if (string.IsNullOrEmpty(_BehaviourAssetRoot)) {
					string root = EditorUtil.GetRootPath(ROOT_NAME, "");
					if (string.IsNullOrEmpty(root)) {
						root = "Assets/Pixel Jelly Data";
					}
					_BehaviourAssetRoot = Util.FixPath(
						Util.CombinePaths(root, "Data", "Behaviour")
					);
				}
				return _BehaviourAssetRoot;
			}
		}
		private static string _BehaviourAssetRoot = "";

		// Ser
		[SerializeField] List<SlotPair> m_DataSlots = new();
		[SerializeField] List<Color32> m_PixelEditorPalette = new();
		[SerializeField] List<FavoriteBehaviourData> m_FavoriteBehs = new();

		// Data
		private static JellyConfig _Main = null;
		private static readonly Dictionary<(string _assembly, string _namespace, string _type), string> SlotMap = new();



		// API
		public void SaveChanges () {
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}


		// Fav
		public IEnumerator<FavoriteBehaviourData> GetFavoriteEnumerator () => m_FavoriteBehs.GetEnumerator();


		public (string _assembly, string _namespace, string _type) GetFavoriteBeh (int index) {
			var data = m_FavoriteBehs[index];
			return (data.Assembly, data.Namespace, data.Type);
		}


		public void RemoveFavoriteBeh (string _assembly, string _namespace, string _type) {
			for (int i = 0; i < m_FavoriteBehs.Count; i++) {
				var beh = m_FavoriteBehs[i];
				if (beh.Assembly == _assembly && beh.Namespace == _namespace && beh.Type == _type) {
					m_FavoriteBehs.RemoveAt(i);
					break;
				}
			}
		}


		public void AddFavoriteBeh (string _assembly, string _namespace, string _type, string _group) {
			m_FavoriteBehs.Add(new FavoriteBehaviourData(_assembly, _namespace, _type, _group));
			m_FavoriteBehs.Sort((a, b) => {
				int result = a.Group.CompareTo(b.Group);
				if (result == 0) {
					result = a.Assembly.CompareTo(b.Assembly);
				}
				if (result == 0) {
					result = a.Namespace.CompareTo(b.Namespace);
				}
				if (result == 0) {
					result = a.Type.CompareTo(b.Type);
				}
				return result;
			});
		}


		// Slot
		public string GetDataSlot (System.Type type) => GetDataSlot(type.Assembly.GetName().Name, type.Namespace, type.Name);
		public string GetDataSlot (string _assembly, string _namespace, string _type) {
			if (string.IsNullOrEmpty(_assembly) || string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(_type)) { return "Default"; }
			var param = (_assembly, _namespace, _type);
			if (SlotMap.ContainsKey(param)) {
				return SlotMap[param];
			}
			string slot = "Default";
			bool inData = false;
			foreach (var pair in m_DataSlots) {
				if (pair.Assembly == _assembly && pair.Namespace == _namespace && pair.Type == _type) {
					slot = pair.Slot;
					inData = true;
					break;
				}
			}
			if (!inData) {
				string rootFolder = GetAssetFolder(_assembly, _namespace, _type);
				if (Util.FolderExists(rootFolder)) {
					var enu = Util.EnumerateFiles(rootFolder, "*.asset");
					using var enumerator = enu.GetEnumerator();
					if (enumerator.MoveNext()) {
						slot = Util.GetNameWithoutExtension(enumerator.Current);
					}
				}
				m_DataSlots.Add(new SlotPair(_assembly, _namespace, _type, slot));
			}
			SlotMap.Add(param, slot);
			return slot;
		}


		public void SetDataSlot (System.Type type, string slot) => SetDataSlot(type.Assembly.GetName().Name, type.Namespace, type.Name, slot);
		public void SetDataSlot (string _assembly, string _namespace, string _type, string slot) {
			if (string.IsNullOrEmpty(_assembly) || string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(_type)) { return; }
			var param = (_assembly, _namespace, _type);
			SlotMap.SetOrAdd(param, slot);
			bool done = false;
			for (int i = 0; i < m_DataSlots.Count; i++) {
				var pair = m_DataSlots[i];
				if (pair.Assembly == _assembly && pair.Namespace == _namespace && pair.Type == _type) {
					pair.Slot = slot;
					m_DataSlots[i] = pair;
					done = true;
					break;
				}
			}
			if (!done) {
				m_DataSlots.Add(new SlotPair(_assembly, _namespace, _type, slot));
			}
		}


		public void ResetDataSlotCache (System.Type type) => ResetDataSlotCache(type.Assembly.GetName().Name, type.Namespace, type.Name);
		public void ResetDataSlotCache (string _assembly, string _namespace, string _type) {
			if (string.IsNullOrEmpty(_assembly) || string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(_type)) { return; }
			var param = (_assembly, _namespace, _type);
			if (SlotMap.ContainsKey(param)) {
				SlotMap.Remove(param);
			}
		}


		public void ClearUnusedDataSlot () {
			var usingHash = new HashSet<(string, string, string)>();
			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(JellyBehaviour)));
				foreach (var type in types) {
					string typeName = type.Name;
					string nameSpace = !string.IsNullOrEmpty(type.Namespace) ? type.Namespace : "Default";
					string assemblyName = assembly.GetName().Name;
					usingHash.TryAdd((assemblyName, nameSpace, typeName));
				}
			}
			for (int i = 0; i < m_DataSlots.Count; i++) {
				var pair = m_DataSlots[i];
				if (!usingHash.Contains((pair.Assembly, pair.Namespace, pair.Type))) {
					m_DataSlots.RemoveAt(i);
					i--;
				}
			}
			SlotMap.Clear();
		}


		// LGC
		private string GetAssetFolder (string _assembly, string _namespace, string _type) {
			string typeName = Util.SanitizeFileName(_type);
			string nameSpace = !string.IsNullOrEmpty(_namespace) ?
				Util.SanitizeFileName(_namespace) : "Default";
			string assemblyName = Util.SanitizeFileName(_assembly);
			return Util.CombinePaths(BehaviourAssetRoot, assemblyName, nameSpace, typeName);
		}


		private List<Color32> GetDefaultPalette () => new() {

				new Color32(000,000,000,255),new Color32(85, 85, 85,255),new Color32(170, 170, 170,255),new Color32(255,255,255,255),
				new Color32(50, 50, 50,255),new Color32(93, 93, 93,255),new Color32(125, 125, 125,255),new Color32(190, 190, 190,255),

				new Color32(77, 77, 77,255),new Color32(142, 144, 144,255),new Color32(197, 203, 205,255),new Color32(237, 241, 245,255),
				new Color32(94, 88, 88,255),new Color32(138, 129, 127,255),new Color32(184, 172, 167,255),new Color32(240, 230, 218,255),

				new Color32(168, 35, 66,255),new Color32(199, 58, 74,255),new Color32(240, 86, 86,255),new Color32(255, 125, 102,255),
				new Color32(117, 59, 78,255),new Color32(150, 75, 84,255),new Color32(199, 104, 99,255),new Color32(255, 147, 120,255),

				new Color32(146, 85, 73,255),new Color32(177, 122, 102,255),new Color32(208, 158, 131,255),new Color32(239, 194, 160,255),
				new Color32(140, 84, 101,255),new Color32(170, 108, 114,255),new Color32(200, 132, 128,255),new Color32(231, 165, 146,255),

				new Color32(120, 50, 24,255),new Color32(153, 80, 24,255),new Color32(207, 123, 60,255),new Color32(245, 169, 83,255),
				new Color32(115, 64, 55,255),new Color32(140, 86, 70,255),new Color32(191, 133, 92,255),new Color32(232, 184, 111,255),

				new Color32(143, 98, 55,255),new Color32(209, 136, 60,255),new Color32(255, 165, 50,255),new Color32(252, 195, 81,255),
				new Color32(114, 89, 51,255),new Color32(172, 129, 59,255),new Color32(225, 171, 48,255),new Color32(252, 213, 74,255),

				new Color32(157, 139, 65,255),new Color32(191, 174, 60,255),new Color32(232, 216, 42,255),new Color32(255, 255, 0,255),
				new Color32(69, 97, 53,255),new Color32(94, 115, 59,255),new Color32(153, 166, 58,255),new Color32(245, 231, 83,255),

				new Color32(53, 97, 66,255),new Color32(59, 115, 61,255),new Color32(81, 166, 58,255),new Color32(151, 245, 83,255),
				new Color32(53, 97, 84,255),new Color32(60, 115, 96,255),new Color32(58, 166, 105,255),new Color32(83, 245, 113,255),

				new Color32(23, 101, 104,255),new Color32(10, 143, 134,255),new Color32(9, 181, 161,255),new Color32(0, 255, 204,255),
				new Color32(37, 44, 53,255),new Color32(42, 61, 74,255),new Color32(59, 106, 118,255),new Color32(77, 189, 189,255),

				new Color32(41, 46, 92,255),new Color32(44, 63, 130,255),new Color32(47, 86, 164,255),new Color32(52, 139, 216,255),
				new Color32(72, 102, 155,255),new Color32(87, 130, 199,255),new Color32(86, 163, 217,255),new Color32(80, 207, 252,255),

				new Color32(29, 29, 46,255),new Color32(39, 38, 60,255),new Color32(50, 47, 74,255),new Color32(87, 79, 105,255),
				new Color32(46, 40, 62,255),new Color32(55, 44, 74,255),new Color32(77, 58, 100,255),new Color32(111, 82, 131,255),

				new Color32(67, 6, 105,255),new Color32(98, 8, 138,255),new Color32(168, 39, 194,255),new Color32(236, 87, 225,255),
				new Color32(43, 20, 87,255),new Color32(64, 26, 115,255),new Color32(115, 56, 161,255),new Color32(176, 94, 196,255),

				new Color32(119, 95, 117,255),new Color32(164, 114, 155,255),new Color32(222, 142, 203,255),new Color32(244, 185, 223,255),
				new Color32(84, 70, 79,255),new Color32(110, 86, 97,255),new Color32(154, 126, 134,255),new Color32(187, 162, 161,255),

				new Color32(44, 43, 43,255),new Color32(54, 47, 47,255),new Color32(80, 59, 59,255),new Color32(139, 92, 92,255),
				new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),

				new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),
				new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),
				new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),
				new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),new Color32(122, 98, 103,255),
			};


	}
}
