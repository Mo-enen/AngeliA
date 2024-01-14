using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireSpriteFromField]
	public partial class SheetEditor : GlobalEditorUI {




		#region --- SUB ---


		private class AtlasRequirementComparer : IComparer<AtlasRequirement> {
			public static readonly AtlasRequirementComparer Instance = new();
			public int Compare (AtlasRequirement a, AtlasRequirement b) {
				int result = a.Order.CompareTo(b.Order);
				return result != 0 ? result : a.AtlasName.CompareTo(b.AtlasName);
			}
		}


		private class AtlasRequirement {
			public int ID;
			public string AtlasName = "";
			public bool Readonly;
			public int Order;
			public List<string> SpriteNames = new();
		}


		#endregion




		#region --- VAR ---


		// Const 
		private const int PANEL_WIDTH = 300;
		public static readonly int TYPE_ID = typeof(SheetEditor).AngeHash();
		private static readonly SpriteCode ICON_FILE = "Icon.File";
		private static readonly string[] STATIC_ATLAS_REQUIREMENTS = { "LevelFront", "LevelBack", "Background", };

		// Api
		public new static SheetEditor Instance => GlobalEditorUI.Instance as SheetEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private readonly Dictionary<int, AtlasRequirement> AtlasRequirements = new();
		private readonly List<AtlasRequirement> AllRequiredAtlasNames = new();
		private readonly Sheet CurrentAtlasSheet = new();
		private string CurrentAtlasName = "";


		#endregion




		#region --- MSG ---


		[OnGameInitialize]
		public static void CreateAtlasFiles () {
			string root = Project.CurrentProject.AtlasRoot;
			CreateFileIfNotExists(root, "LevelFront", AtlasType.Level, 16);
			CreateFileIfNotExists(root, "LevelBack", AtlasType.Level, -35);
			CreateFileIfNotExists(root, "Background", AtlasType.Background, -64);
			// Func
			static void CreateFileIfNotExists (string root, string targetAtlasName, AtlasType targetAtlasType, int targetAtlasZ) {
				string path = Util.CombinePaths(root, $"{targetAtlasName}.{AngePath.SHEET_FILE_EXT}");
				if (Util.FileExists(path)) return;
				var sheet = new Sheet(null, null, new AtlasInfo[] { new() {
					Name = targetAtlasName,
					Type = targetAtlasType,
					AtlasZ = targetAtlasZ,
				} }, null);
				sheet.SaveToDisk(path);
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			CurrentAtlasSheet.Clear();
			CurrentAtlasName = "";
			InitializeAtlas();
		}


		public override void OnInactivated () {
			base.OnInactivated();
			CurrentAtlasSheet.Clear();
			CurrentAtlasName = "";
			AtlasRequirements.Clear();
		}


		// Update
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceOffset(Unify(PANEL_WIDTH), 0);
		}


		public override void UpdateUI () {
			base.UpdateUI();

			// Panel Rect
			var panelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));
			if (string.IsNullOrEmpty(CurrentAtlasName)) {
				Update_FilePanel(panelRect);
			} else {
				Update_InspectorPanel(panelRect);
			}

			// 



		}


		private void Update_FilePanel (IRect panelRect) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: 0);

			// Atlas List




		}


		private void Update_InspectorPanel (IRect panelRect) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: 0);


		}


		#endregion




		#region --- LGC ---


		private void InitializeAtlas () {

			AtlasRequirements.Clear();
			AllRequiredAtlasNames.Clear();

			// Load Requirement From File
			foreach (var path in Util.EnumerateFiles(Project.CurrentProject.AtlasRoot, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				string atlasName = Util.GetNameWithoutExtension(path);
				int nameID = atlasName.AngeHash();
				if (!AtlasRequirements.ContainsKey(nameID)) {
					AtlasRequirements.Add(nameID, new AtlasRequirement() {
						AtlasName = atlasName,
						Readonly = false,
						Order = 2,
					});
				}
			}

			// Load Requirement From Code
			foreach (var (name, atlasName) in AngeUtil.ForAllSpriteNameRequirements()) {
				int id = atlasName.AngeHash();
				if (AtlasRequirements.TryGetValue(id, out var requirement)) {
					requirement.Readonly = true;
				} else {
					requirement = new AtlasRequirement() {
						AtlasName = atlasName,
						Readonly = true,
						Order = 0,
					};
				}
				requirement.SpriteNames.Add(name);
			}

			// Add Static
			foreach (var require in STATIC_ATLAS_REQUIREMENTS) {
				int id = require.AngeHash();
				if (!AtlasRequirements.ContainsKey(id)) {
					AtlasRequirements.Add(id, new AtlasRequirement() {
						AtlasName = require,
						Readonly = true,
						Order = 1,
					});
				}
			}

			// All Names
			AllRequiredAtlasNames.AddRange(AtlasRequirements.Values);
			AllRequiredAtlasNames.Sort(AtlasRequirementComparer.Instance);

		}


		private void LoadAtlasSheetFromDisk (Sheet atlasSheet, string atlasName) {
			// Load from Disk
			string path = Util.CombinePaths(
				Project.CurrentProject.AtlasRoot,
				$"{atlasName}.{AngePath.SHEET_FILE_EXT}"
			);
			bool loaded = atlasSheet.LoadFromDisk(path);
			// Create New Atlas if File Not Exists
			if (
				!loaded &&
				!Util.FileExists(path) &&
				AtlasRequirements.TryGetValue(atlasName.AngeHash(), out var require)
			) {
				atlasSheet.SetData(null, null, new AtlasInfo[] { new() {
					Name = atlasName,
					Type = AtlasType.General,
				} }, null);
			}
		}


		private void SaveAtlasSheetToDisk (Sheet atlasSheet, string atlasName) =>
			atlasSheet.SaveToDisk(Util.CombinePaths(
				Project.CurrentProject.AtlasRoot,
				$"{atlasName}.{AngePath.SHEET_FILE_EXT}"
			));


		#endregion




	}
}