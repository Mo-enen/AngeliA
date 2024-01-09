using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AngeliaFramework {
	public partial class LanguageEditor : GlobalEditorUI {




		#region --- SUB ---


		private class LanguageData {
			public string Language;
			public List<KeyValuePair<string, string>> Contents;
		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(LanguageEditor).AngeHash();
		private static readonly StringBuilder CacheTextBuilder = new();

		// Api
		public new static LanguageEditor Instance => GlobalEditorUI.Instance as LanguageEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private readonly List<LanguageData> Pool = new();
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
			Pool.Clear();
		}


		public override void UpdateUI () {
			base.UpdateUI();



		}


		#endregion




		#region --- LGC ---


		private void LoadFromDisk () {
			IsDirty = false;
			Pool.Clear();
			foreach (var path in Util.EnumerateFiles(AngePath.LanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				string lan = Util.GetNameWithoutExtension(path);
				var contents = new List<KeyValuePair<string, string>>();
				foreach (string line in Util.ForAllLines(path, System.Text.Encoding.UTF8)) {
					if (string.IsNullOrWhiteSpace(line)) continue;
					int colon = line.IndexOf(':');
					if (colon <= 0) continue;
					var key = line[..colon];
					var value = colon + 1 < line.Length ? line[(colon + 1)..] : "";
					if (string.IsNullOrWhiteSpace(key)) continue;
					contents.Add(new(key, value.Replace("\\n", "\n")));
				}
				Pool.Add(new LanguageData() {
					Language = lan,
					Contents = contents,
				});
			}
		}


		private void SaveToDisk () {
			IsDirty = false;
			var builder = CacheTextBuilder;
			foreach (var data in Pool) {
				builder.Clear();
				string path = Util.CombinePaths(AngePath.LanguageRoot, $"{data.Language}.{AngePath.LANGUAGE_FILE_EXT}");
				foreach (var (key, content) in data.Contents) {
					builder.Append(key);
					builder.Append(':');
					builder.Append(content);
					builder.Append('\n');
				}
				if (builder.Length > 0) {
					Util.TextToFile(builder.ToString(), path);
					builder.Clear();
				}
			}
		}


		#endregion




	}
}