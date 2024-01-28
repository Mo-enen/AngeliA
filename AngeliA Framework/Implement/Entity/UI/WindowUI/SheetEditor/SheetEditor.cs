using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AngeliaFramework {
	//[RequireSpriteFromField]
	//[RequireLanguageFromField]
	public partial class SheetEditor : WindowUI {




		#region --- SUB ---


		private class RequirementData {
			public string SpriteName;
			public string AtlasName;
		}


		private class SpriteListComparer : IComparer<AngeSprite> {
			public static readonly SpriteListComparer Instance = new();
			public int Compare (AngeSprite a, AngeSprite b) => a.RealName.CompareTo(b.RealName);
		}


		#endregion




		#region --- VAR ---


		// Const 
		private const int PANEL_WIDTH = 300;
		public static readonly int TYPE_ID = typeof(SheetEditor).AngeHash();
		private static readonly Dictionary<int, RequirementData> EMPTY_POOL = new();

		// Api
		public new static SheetEditor Instance => WindowUI.Instance as SheetEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private static readonly Dictionary<string, Dictionary<int, RequirementData>> RequirementPool = new();
		private Dictionary<int, RequirementData> CurrentAtlasRequirementPool = EMPTY_POOL;
		private readonly EditableSheet Sheet = new();
		private readonly List<AngeSprite> Sprites = new();
		private int CurrentAtlasIndex = -2;
		private bool IsDirty = false;


		#endregion




		#region --- MSG ---


		[OnGameInitialize]
		public static void OnGameInitialize () {
			// Init Requirement
			RequirementPool.Clear();
			foreach (var (name, atlasName) in AngeUtil.ForAllSpriteNameRequirements()) {
				if (!RequirementPool.TryGetValue(atlasName, out var pool)) {
					RequirementPool.Add(atlasName, pool = new());
				}
				int id = name.AngeHash();
				if (pool.ContainsKey(id)) continue;
				pool.Add(id, new RequirementData() {
					SpriteName = name,
					AtlasName = atlasName,
				});
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			Sheet.LoadFromDisk(ProjectSystem.CurrentProject.SheetPath);
			IsDirty = false;
			Game.StopGame();
			SelectAtlas(-1);
		}


		public override void OnInactivated () {
			base.OnInactivated();
			if (IsDirty) {
				IsDirty = false;
				Sheet.SaveToDisk(ProjectSystem.CurrentProject.SheetPath);
				CellRenderer.ReloadSheet();
			}
			Sheet.Clear();
		}


		// Update
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceOffset(Unify(PANEL_WIDTH), 0);
			ControlHintUI.ForceHideGamepad();
			Skybox.ForceSkyboxTint(new Byte4(32, 33, 37, 255), new Byte4(32, 33, 37, 255));
		}


		public override void UpdateUI () {
			base.UpdateUI();

			// Empty
			if (!Sheet.NotEmpty) {
				Update_EmptySheetPanel(MainWindowRect);
				return;
			}

			// Panel Rect
			int leftPanelWidth = Unify(PANEL_WIDTH);
			var panelRect = MainWindowRect.EdgeInside(Direction4.Left, leftPanelWidth);
			if (CurrentAtlasIndex < 0) {
				Update_AtlasSelector(panelRect);
			} else {
				Update_SpriteSelector(panelRect);
			}

			// Content
			Update_ContentPanel(MainWindowRect.EdgeInside(Direction4.Right, MainWindowRect.width - leftPanelWidth));

		}


		private void Update_AtlasSelector (IRect panelRect) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: 0);

			// Atlas List




		}


		private void Update_SpriteSelector (IRect panelRect) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: 0);




		}


		private void Update_ContentPanel (IRect panelRect) {




		}


		private void Update_EmptySheetPanel (IRect panelRect) {




		}


		#endregion




		#region --- LGC ---


		private void SelectAtlas (int index) {

			if (CurrentAtlasIndex == index) return;

			CurrentAtlasIndex = index;
			CurrentAtlasRequirementPool = EMPTY_POOL;
			Sprites.Clear();

			if (index >= 0 && index < Sheet.Atlas.Count) {
				var atlas = Sheet.Atlas[index];

				// Require
				if (RequirementPool.TryGetValue(atlas.Name, out var currentPool)) {
					CurrentAtlasRequirementPool = currentPool;
				}

				// Sprite List
				foreach (var sprite in Sheet.Sprites) {
					if (sprite.AtlasIndex != index) continue;
					if (
						sprite.Group != null &&
						sprite.Group.Sprites.Count > 0 &&
						sprite != sprite.Group.Sprites[0]
					) continue;
					Sprites.Add(sprite);
				}

				// Sort
				Sprites.Sort(SpriteListComparer.Instance);
			}
		}


		#endregion




	}
}