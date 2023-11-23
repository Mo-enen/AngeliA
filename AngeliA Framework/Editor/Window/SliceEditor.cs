using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;



namespace AngeliaFramework.Editor {
	public class SliceEditor : EditorWindow {




		#region --- VAR ---


		private enum SortMode { Original, Name, XY, YX, }


		private const double FLASH_DURATION = 2d;
		private static GUIStyle MasterStyle => _MasterStyle ??= new GUIStyle() {
			padding = new RectOffset(24, 24, 24, 24),
		};
		private static GUIStyle _MasterStyle = null;
		private static Texture2D AseIcon = null;
		private Vector2 MasterScrollPos = default;
		private AseData CurrentAse = null;
		private string SliceLines = "";
		private string AsePath = "";
		private readonly List<string> AllAses = new();
		private double CopyFlashTime = double.MinValue;
		private double PasteFlashTime = double.MinValue;
		private bool ShowCopyFlash = false;
		private bool ShowPasteFlash = false;
		private string SliceCreator_Name = "Slice {x} {y}";
		private Vector2Int SliceCreator_Start = new(1, 1);
		private Vector2Int SliceCreator_Size = new(16, 16);
		private Vector2Int SliceCreator_Count = new(1, 1);
		private Vector2Int SliceCreator_Padding = new(1, 1);
		private bool SliceCreator_Open = false;
		private bool AddEmptyLineBetween = false;
		private SortMode Sort = SortMode.Original;


		#endregion




		#region --- MSG ---


		[MenuItem("AngeliA/Slice Editor", false, 27)]
		public static void OpenEditor () {
			var window = GetWindow<SliceEditor>(true, "Slice Editor", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(1024, 1024);
			window.AllAses.Clear();
			foreach (var filePath in AngeEditorUtil.ForAllAsepriteFiles()) {
				window.AllAses.Add(filePath);
			}
			window.CopyFlashTime = double.MinValue;
			window.PasteFlashTime = double.MinValue;
		}


		private void OnGUI () {
			using var _ = new GUILayout.VerticalScope(MasterStyle);
			if (CurrentAse != null) {
				GUI_Editor();
			} else {
				GUI_Selector();
			}
			MGUI.CancelFocusOnClick(this);
		}


		private void Update () {
			if (EditorApplication.timeSinceStartup < CopyFlashTime + FLASH_DURATION) {
				ShowCopyFlash = EditorApplication.timeSinceStartup % 0.2d > 0.1d;
				Repaint();
			} else if (ShowCopyFlash) {
				ShowCopyFlash = false;
				Repaint();
			}
			if (EditorApplication.timeSinceStartup < PasteFlashTime + FLASH_DURATION) {
				ShowPasteFlash = EditorApplication.timeSinceStartup % 0.2d > 0.1d;
				Repaint();
			} else if (ShowPasteFlash) {
				ShowPasteFlash = false;
				Repaint();
			}
		}


		private void GUI_Selector () {
			if (AllAses.Count == 0) {
				EditorGUILayout.HelpBox("No ase file founded.", MessageType.Warning, true);
				return;
			}
			if (AseIcon == null && AllAses.Count > 0) {
				foreach (var path in AllAses) {
					var fPath = EditorUtil.FixedRelativePath(path);
					if (!string.IsNullOrEmpty(fPath)) {
						AseIcon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<DefaultAsset>(fPath));
						break;
					}
				}
			}
			const int HEIGHT = 22;
			MasterScrollPos = EditorGUILayout.BeginScrollView(MasterScrollPos);
			string prevParent = "";
			foreach (var path in AllAses) {
				string parent = Util.GetNameWithoutExtension(Util.GetParentPath(path));
				if (parent != prevParent) {
					GUI.Label(MGUI.Rect(0, HEIGHT), parent, MGUI.MiniGreyLabel);
					prevParent = parent;
				}
				var rect = MGUI.Rect(0, HEIGHT).Shrink(6, 6, 0, 0);
				if (GUI.Button(rect, "", EditorStyles.toolbarButton)) {
					var ase = AseData.CreateFromBytes(Util.FileToByte(path));
					if (ase != null) {
						CurrentAse = ase;
						AsePath = path;
						SliceLines = LoadSliceLines(ase);
					}
				}
				GUI.Label(rect.Shrink(24, 0, 0, 0), Util.GetNameWithoutExtension(path));
				rect.x += 3;
				rect.y += 3;
				rect.height -= 6;
				rect.width = rect.height;
				GUI.DrawTexture(rect, AseIcon != null ? AseIcon : Texture2D.whiteTexture);
				EditorGUIUtility.AddCursorRect(MGUI.LastRect(), MouseCursor.Link);
				MGUI.Space(1);
			}
			EditorGUILayout.EndScrollView();
		}


		private void GUI_Editor () {

			// Text Area
			bool oldE = GUI.enabled;
			GUI.enabled = false;
			SliceLines = EditorGUI.TextArea(MGUI.Rect(0, 128), SliceLines);
			GUI.enabled = oldE;
			MGUI.Space(6);

			// Copy Paste Buttons
			using (new GUILayout.HorizontalScope()) {

				bool cFlashing = EditorApplication.timeSinceStartup < CopyFlashTime + FLASH_DURATION;
				bool pFlashing = EditorApplication.timeSinceStartup < PasteFlashTime + FLASH_DURATION;

				// Copy
				var oldB = GUI.backgroundColor;
				GUI.backgroundColor = ShowCopyFlash ? new Color(1f, 1f, 1f, 0.8f) : oldB;
				if (GUI.Button(MGUI.Rect(0, 36), cFlashing ? "Copied" : "Copy")) {
					EditorGUIUtility.systemCopyBuffer = SliceLines;
					CopyFlashTime = EditorApplication.timeSinceStartup;
				}
				EditorGUIUtility.AddCursorRect(MGUI.LastRect(), MouseCursor.Link);

				// Paste
				GUI.backgroundColor = ShowPasteFlash ? new Color(1f, 1f, 1f, 0.8f) : oldB;
				MGUI.Space(4);
				if (GUI.Button(MGUI.Rect(0, 36), pFlashing ? "Pasted" : "Paste")) {
					SliceLines = EditorGUIUtility.systemCopyBuffer;
					PasteFlashTime = EditorApplication.timeSinceStartup;
				}
				EditorGUIUtility.AddCursorRect(MGUI.LastRect(), MouseCursor.Link);

				GUI.backgroundColor = oldB;
			}

			MGUI.Space(6);

			// Options
			bool newAddEmptyLineBetween = EditorGUI.Toggle(MGUI.Rect(0, 18), "Add Empty Line Between", AddEmptyLineBetween);
			if (newAddEmptyLineBetween != AddEmptyLineBetween) {
				AddEmptyLineBetween = newAddEmptyLineBetween;
				SliceLines = LoadSliceLines(CurrentAse);
				Repaint();
			}

			GUILayout.Space(2);
			var newSort = (SortMode)EditorGUI.EnumPopup(MGUI.Rect(0, 18), "Sort Mode", Sort);
			if (newSort != Sort) {
				Sort = newSort;
				SliceLines = LoadSliceLines(CurrentAse);
				Repaint();
			}

			// Slice Creator
			if (MGUI.Fold("Slice Creator", ref SliceCreator_Open)) {
				using (new EditorGUI.IndentLevelScope()) {
					SliceCreator_Name = EditorGUILayout.DelayedTextField("Name", SliceCreator_Name);
					SliceCreator_Start = EditorGUILayout.Vector2IntField("Start", SliceCreator_Start);
					SliceCreator_Count = EditorGUILayout.Vector2IntField("Count", SliceCreator_Count);
					SliceCreator_Size = EditorGUILayout.Vector2IntField("Size", SliceCreator_Size);
					SliceCreator_Padding = EditorGUILayout.Vector2IntField("Padding", SliceCreator_Padding);
					MGUI.Space(6);
					using (new GUILayout.HorizontalScope()) {
						MGUI.Rect(0, 24);
						if (GUI.Button(MGUI.Rect(69, 24), "Create")) {
							CreateSlices();
							SliceLines = LoadSliceLines(CurrentAse);
							Repaint();
						}
					}
				}
			}

			MGUI.Rect(0, 0);

			// Workflow
			using (new GUILayout.HorizontalScope()) {

				if (GUI.Button(MGUI.Rect(69, 24), "Back")) {
					CurrentAse = null;
					SliceLines = "";
					AsePath = "";
					CopyFlashTime = double.MinValue;
					PasteFlashTime = double.MinValue;
				}

				MGUI.Rect(0, 24);

				if (GUI.Button(MGUI.Rect(69, 24), "Clear")) {
					SliceLines = "";
				}

				MGUI.Space(2);
				if (GUI.Button(MGUI.Rect(69, 24), "Apply")) {
					if (EditorUtil.Dialog("", "Apply changes into file?", "Apply", "Cancel")) {
						ApplySliceLines(CurrentAse, SliceLines, AsePath);
						SliceLines = LoadSliceLines(CurrentAse);
						return;
					}
				}
			}
		}


		#endregion




		#region --- LGC ---


		private string LoadSliceLines (AseData ase) {
			var slices = new List<(string name, AseData.SliceChunk.SliceData data, bool hasPivot)>();
			ase.ForAllChunks<AseData.SliceChunk>(0, (slice, chunkIndex) => {
				if (slice.SliceNum > 0) {
					if (!slice.CheckFlag(AseData.SliceChunk.SliceFlag.NinePatches)) {
						var data = slice.Slices[0];
						data.CenterX = 0;
						data.CenterY = 0;
						data.CenterWidth = data.Width;
						data.CenterHeight = data.Height;
						slice.Slices[0] = data;
					}
					slices.Add((slice.Name, slice.Slices[0], slice.CheckFlag(AseData.SliceChunk.SliceFlag.HasPivot)));
				}
			});

			// Sort
			switch (Sort) {
				case SortMode.XY:
					slices.Sort((a, b) => {
						int result = a.data.X.CompareTo(b.data.X);
						if (result == 0) result = a.data.Y.CompareTo(b.data.Y);
						if (result == 0) result = a.name.CompareTo(b.name);
						return result;
					});
					break;
				case SortMode.YX:
					slices.Sort((a, b) => {
						int result = a.data.Y.CompareTo(b.data.Y);
						if (result == 0) result = a.data.X.CompareTo(b.data.X);
						if (result == 0) result = a.name.CompareTo(b.name);
						return result;
					});

					break;
				case SortMode.Name:
					slices.Sort((a, b) => {
						return a.name.CompareTo(b.name);
					});
					break;
			}

			// Add Line 
			string prevName = "";
			if (AddEmptyLineBetween) {
				for (int i = 0; i < slices.Count; i++) {
					var (_name, _, _) = slices[i];
					_name = AngeEditorUtil.GetBlockRealName(_name);
					int index = _name.IndexOf('.');
					if (index >= 0) _name = _name[..index];
					_name = _name.TrimEnd_NumbersEmpty_();
					if (!string.IsNullOrEmpty(prevName) && _name != prevName) {
						slices.Insert(i, ("", null, false));
					}
					prevName = _name;
				}
			}
			// Append Line
			var builder = new StringBuilder();
			foreach (var (_name, _slice, _hasPivot) in slices) {
				if (!string.IsNullOrEmpty(_name)) {
					builder.AppendLine(GetSliceLine(
						_name,
						_slice.X, _slice.Y, (int)_slice.Width, (int)_slice.Height,
						_slice.BorderL, _slice.BorderR, _slice.BorderD, _slice.BorderU,
						_hasPivot ? new Vector2Int(_slice.PivotX, _slice.PivotY) : null
					));
				} else {
					builder.AppendLine();
				}
			}
			return builder.ToString();
		}


		private static void ApplySliceLines (AseData ase, string lines, string path) {

			// Apply
			var reader = new StringReader(lines);
			ase.RemoveAllChunks<AseData.SliceChunk>();
			while (reader.Peek() >= 0) {
				// Append
				string line = "";
				while (
					reader.Peek() >= 0 &&
					(string.IsNullOrEmpty(line = reader.ReadLine()) || string.IsNullOrWhiteSpace(line))
				) { }
				if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line)) {
					var slice = new AseData.SliceChunk();
					if (GetInfoFromSliceLine(line, out string name, out var rect, out var border, out var pivot)) {
						FillSliceInfoTo(slice, name, rect, border, pivot);
						ase.AddChunk(0, slice);
						ase.AddChunk(0, new AseData.UserDataChunk() {
							Flag = 2,
							R = 0,
							G = 0,
							B = 0,
							A = 128,
							Text = "",
						});
					}
				}
			}
			Util.ByteToFile(ase.ToBytes(), path);
		}


		private void CreateSlices () {
			var ase = CurrentAse;
			if (CurrentAse == null || CurrentAse.FrameDatas.Count == 0) return;
			ase.RemoveAllChunks<AseData.SliceChunk>();
			var gap = SliceCreator_Padding.Clamped(0, 0);
			var offset = SliceCreator_Start.Clamped(0, 0);
			var size = SliceCreator_Size.Clamped(0, 0);
			var count = SliceCreator_Count.Clamped(0, 0);
			Color32 color32 = Color.black;
			string nameFormat = SliceCreator_Name.Replace("{x}", "{0}").Replace("{y}", "{1}").Replace("{i}", "{2}");
			int index = 0;
			for (int j = 0; j < count.y; j++) {
				for (int i = 0; i < count.x; i++) {
					ase.AddChunk(0, new AseData.SliceChunk() {
						Flag = 0,
						Name = string.Format(nameFormat, i, j, index),
						SliceNum = 1,
						Slices = new AseData.SliceChunk.SliceData[] {
							new AseData.SliceChunk.SliceData() {
								FrameIndex = 0,
								PivotX = 0,
								PivotY = 1,
								X = offset.x + i * (size.x + gap.x),
								Y = offset.y + j * (size.y + gap.y),
								Width = (uint)size.x,
								Height = (uint)size.y,
								CenterX = 0,
								CenterY = 0,
								CenterWidth = 0,
								CenterHeight = 0,
							}
						},
					});
					ase.AddChunk(0, new AseData.UserDataChunk() {
						Flag = 2,
						R = color32.r,
						G = color32.g,
						B = color32.b,
						A = color32.a,
						Text = "",
					});
					index++;
				}
			}



		}


		private static string GetSliceLine (string name, int x, int y, int w, int h, int borderL, int borderR, int borderD, int borderU, Vector2Int? pivot) {
			if (pivot.HasValue) {
				return $"{name}, {x}, {y}, {w}, {h}, {borderL}, {borderR}, {borderD}, {borderU}, {pivot.Value.x}, {pivot.Value.y}";
			} else {
				return $"{name}, {x}, {y}, {w}, {h}, {borderL}, {borderR}, {borderD}, {borderU}";
			}
		}


		private static bool GetInfoFromSliceLine (string line, out string name, out RectInt rect, out Vector4Int border, out Vector2Int? pivot) {
			name = "";
			rect = default;
			border = default;
			pivot = default;
			if (!string.IsNullOrEmpty(line) && line.Contains('^')) return false;
			var values = line.Split(',');
			int x = 0, y = 0, w = 0, h = 0, l = 0, r = 0, d = 0, u = 0;
			name = "";

			if (values.Length > 0) name = values[0];
			if (values.Length > 1) Util.TryCompute(values[1], out x);
			if (values.Length > 2) Util.TryCompute(values[2], out y);
			if (values.Length > 3) Util.TryCompute(values[3], out w);
			if (values.Length > 4) Util.TryCompute(values[4], out h);
			if (values.Length > 5) Util.TryCompute(values[5], out l);
			if (values.Length > 6) Util.TryCompute(values[6], out r);
			if (values.Length > 7) Util.TryCompute(values[7], out d);
			if (values.Length > 8) Util.TryCompute(values[8], out u);
			if (values.Length > 10) {
				Util.TryCompute(values[9], out int px);
				Util.TryCompute(values[10], out int py);
				pivot = new(px, py);
			} else {
				pivot = null;
			}
			rect = new(x, y, w, h);
			border = new() { left = l, right = r, down = d, up = u, };
			return !string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name);
		}


		private static void FillSliceInfoTo (AseData.SliceChunk slice, string name, RectInt rect, Vector4Int border, Vector2Int? pivot) {


			border.left = Mathf.Max(border.left, 0);
			border.right = Mathf.Max(border.right, 0);
			border.down = Mathf.Max(border.down, 0);
			border.up = Mathf.Max(border.up, 0);

			slice.Name = name;
			slice.SliceNum = 1;
			slice.Flag =
				border.IsZero && !pivot.HasValue ? 0u :
				!border.IsZero && !pivot.HasValue ? 1u :
				border.IsZero && pivot.HasValue ? 2u :
				3u;
			slice.Slices = new AseData.SliceChunk.SliceData[] { new (){
				X = rect.x,
				Y = rect.y,
				Width = (uint)rect.width,
				Height = (uint)rect.height,
				FrameIndex = 0,
				PivotX = pivot.HasValue ? pivot.Value.x : 0,
				PivotY = pivot.HasValue ? pivot.Value.y : 0,
				CenterX = border.left,
				CenterY = border.up,
				CenterWidth = (uint)(rect.width - border.left - border.right),
				CenterHeight= (uint)(rect.height - border.down - border.up),
			}};
		}


		#endregion




	}
}
