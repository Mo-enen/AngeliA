using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public partial class LanguageEditor : GlobalEditorUI {




		#region --- SUB ---


		private class LanguageData {
			public string Key;
			public bool Required;
			public List<string> Value;
		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(LanguageEditor).AngeHash();
		private static readonly LanguageCode ADD_KEY = "UI.LanguageEditor.AddKey";
		private static readonly LanguageCode ADD_LANGUAGE = "UI.LanguageEditor.AddLanguage";

		// Api
		public new static LanguageEditor Instance => GlobalEditorUI.Instance as LanguageEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private readonly List<string> Languages = new();
		private readonly List<LanguageData> Content = new();
		private bool IsDirty = false;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			LoadFromDisk();
		}


		public override void OnInactivated () {
			base.OnInactivated();
			if (IsDirty) SaveToDisk();
			Content.Clear();
			Languages.Clear();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
		}


		public override void UpdateUI () {
			base.UpdateUI();
			int padding = Unify(32);
			var cameraRect = CellRenderer.CameraRect.Shrink(padding, padding, 0, 0);
			int column = Languages.Count + 1;
			int fieldWidth = Util.Clamp(cameraRect.width / column, 0, Unify(300));
			cameraRect.x += (cameraRect.width - fieldWidth * column) / 2;
			cameraRect.width = fieldWidth * column;
			Update_Bar(cameraRect.EdgeInside(Direction4.Up, Unify(42)));
			Update_Content(cameraRect.EdgeInside(Direction4.Down, CellRenderer.CameraRect.height - Unify(42)));
		}


		private void Update_Bar (IRect panelRect) {

			var rect = panelRect;

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, 0);

			// + Key
			rect.width = Unify(96);
			if (CellRendererGUI.Button(rect, Language.Get(ADD_KEY, "+ Key"), 1)) {
				Content.Insert(0, new LanguageData() {
					Key = string.Empty,
					Value = new List<string>(new string[Languages.Count].FillWithValue(string.Empty)),
				});
				IsDirty = true;
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x += rect.width;

			// + Language
			rect.width = Unify(128);
			if (CellRendererGUI.Button(rect, Language.Get(ADD_LANGUAGE, "+ Language"), 1)) {
				OpenAddLanguagePopup();
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x += rect.width;



		}


		private void Update_Content (IRect panelRect) {



		}


		#endregion




		#region --- LGC ---


		private void LoadFromDisk () {
			IsDirty = false;
			Content.Clear();
			string targetRoot = Game.IsEdittime ? AngePath.BuiltInLanguageRoot : AngePath.UserLanguageRoot;
			int count = Game.IsEdittime ? Language.BuiltInLanguageCount : Language.UserLanguageCount;
			// Load Language
			for (int languageIndex = 0; languageIndex < count; languageIndex++) {
				Languages.Add(Game.IsEdittime ? Language.GetBuiltInLanguageAt(languageIndex) : Language.GetUserLanguageAt(languageIndex));
			}
			Languages.Sort();
			// Load Contents
			var pool = new Dictionary<string, int>();
			for (int languageIndex = 0; languageIndex < count; languageIndex++) {
				foreach (var (key, value) in Language.LoadAllPairsFromDisk(targetRoot, Languages[languageIndex])) {
					LanguageData data;
					if (pool.TryGetValue(key, out int index)) {
						data = Content[index];
					} else {
						data = new LanguageData() {
							Key = key,
							Value = new List<string>(new string[count].FillWithValue(string.Empty)),
							Required = false,
						};
						pool.Add(key, Content.Count);
						Content.Add(data);
					}
					data.Value[languageIndex] = value;
				}
			}
			// Fill Missing Requirements
			foreach (var requiredKey in AngeUtil.ForAllLanguageKeyRequirements()) {
				if (pool.TryGetValue(requiredKey, out int index)) {
					Content[index].Required = true;
					continue;
				}
				pool.Add(requiredKey, Content.Count);
				Content.Add(new LanguageData() {
					Key = requiredKey,
					Value = new List<string>(new string[count].FillWithValue(string.Empty)),
					Required = true,
				});
				IsDirty = true;
			}
		}


		private void SaveToDisk () {
			IsDirty = false;
			string targetRoot = Game.IsEdittime ? AngePath.BuiltInLanguageRoot : AngePath.UserLanguageRoot;
			var list = new List<KeyValuePair<string, string>>();
			for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++) {
				string lan = Languages[languageIndex];
				list.Clear();
				foreach (var data in Content) {
					if (string.IsNullOrWhiteSpace(data.Key)) continue;
					list.Add(new(data.Key, data.Value[languageIndex]));
				}
				Language.SaveAllPairsToDisk(targetRoot, lan, list);
			}
		}


		private void OpenAddLanguagePopup () {
			GenericPopupUI.BeginPopup();
			foreach (string lan in Util.ForAllSystemLanguages()) {
				string language = lan;
				int index = Languages.IndexOf(language);
				GenericPopupUI.AddItem(Util.GetLanguageDisplayName(language), () => {
					if (index >= 0) {
						// Remove Language
						Languages.RemoveAt(index);
						foreach (var data in Content) {
							data.Value.RemoveAt(index);
						}
						IsDirty = true;
					} else {
						// Add Language
						Languages.Add(language);
						Languages.Sort();
						int newIndex = Languages.IndexOf(language);
						foreach (var data in Content) {
							data.Value.Insert(newIndex, string.Empty);
						}
						IsDirty = true;
					}
				}, true, index >= 0);
			}
		}


		#endregion




	}
}