using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

#if DEBUG
public class QTest {



	#region --- SUB ---


	private enum DataType { Bool, Int, Float, String, Pixels, }


	private class BoolData {
		public bool value;
		public string displayLabel;
		public KeyData Key;
	}


	private class IntData {
		public int value;
		public int min;
		public int max;
		public int step;
		public string displayLabel;
		public KeyData Key;
	}


	private class FloatData {
		public float value;
		public float min;
		public float max;
		public float step;
		public string displayLabel;
		public KeyData Key;
	}


	private class StringData {
		public string value;
		public string displayLabel;
		public KeyData Key;
	}


	private class PixelData {
		public Color32[,] pixels;
		public KeyData Key;
	}


	private class KeyData {
		public string key;
		public int Order;
		public DataType Type;
		public string Group;
		public int GroupOrder;
		public int UpdateFrame;
	}


	private class KeyComparer : IComparer<KeyData> {
		public static readonly KeyComparer Instance = new();
		public int Compare (KeyData a, KeyData b) {
			int result = a.GroupOrder.CompareTo(b.GroupOrder);
			return result != 0 ? result : a.Order.CompareTo(b.Order);
		}
	}


	#endregion




	#region --- VAR ---


	// Api
	public const int MAX_WINDOW_COUNT = 16;
	public static bool Testing => ShowingWindow && !IgnoringWindow;
	public static bool ShowNotUpdatedData { get; set; } = true;

	// Data
	private static readonly QTest[] Windows = new QTest[MAX_WINDOW_COUNT].FillWithNewValue();
	private static readonly List<(int startFrame, Int3 pos, int duration, int size, Color32 color)> Marks = [];
	private static bool ShowingWindow = false;
	private static bool IgnoringWindow = false;
	private static int CurrentWindowIndex = 0;
	private static int MouseDragMovingIndex = -1;
	private readonly Dictionary<string, BoolData> BoolPool = [];
	private readonly Dictionary<string, IntData> IntPool = [];
	private readonly Dictionary<string, FloatData> FloatPool = [];
	private readonly Dictionary<string, StringData> StringPool = [];
	private readonly Dictionary<string, PixelData> PixelsPool = [];
	private readonly Dictionary<string, object> ObjectPool = [];
	private readonly List<KeyData> Keys = [];
	private readonly Dictionary<string, bool> GroupFolding = [];
	private Int2 PanelPositionOffset = new(int.MinValue, int.MinValue);
	private Color32[,] CurrentPixels;
	private string CurrentGroup = "";
	private int CurrentGroupOrder;
	private int CurrentOrder;
	private int PrevKeyCount = 0;
	private int PanelMaxExpand = 0;


	#endregion




	#region --- MSG ---


	[OnGameUpdateLater]
	internal static void UpdateMarks () {
		if (Testing) {
			Game.ForceGizmosOnTopOfUI(1);
			LightingSystem.IgnoreLighting(1);
			Input.IgnoreMouseToActionJump();
		}
		// Draw Trailing Mark
		if (Renderer.TryGetSprite(BuiltInSprite.CIRCLE_32, out var circleSP)) {
			var cameraRect = Renderer.CameraRect;
			using var _ = new UILayerScope(ignoreSorting: true);
			for (int i = 0; i < Marks.Count; i++) {
				var (frame, pos, dur, size, color) = Marks[i];
				int localFrame = Game.GlobalFrame - frame;
				if (localFrame >= dur) {
					Marks.RemoveAt(i);
					i--;
					continue;
				}
				if (pos.z != Stage.ViewZ) continue;
				Renderer.Draw(
					circleSP,
					pos.x.Clamp(cameraRect.xMin, cameraRect.xMax),
					pos.y.Clamp(cameraRect.yMin, cameraRect.yMax),
					500, 500, 0, size, size,
					color.WithNewA((dur - localFrame) * 360 / dur), z: int.MaxValue
				);
			}
		}
	}


	[OnGameUpdate(-256)]
	internal static void UpdateAllWindows () {
		if (!Testing) return;
		for (int i = 0; i < Windows.Length; i++) {
			Windows[i].UpdateWindow(i);
		}
	}


	[OnGameUpdateLater(int.MaxValue)]
	internal static void ResetWindowIndex () => CurrentWindowIndex = 0;


	private void UpdateWindow (int windowIndex) {

		if (Keys.Count == 0) return;

		int validWindowCount = 0;
		for (int i = 0; i < Windows.Length; i++) {
			if (Windows[i].Keys.Count > 0) validWindowCount++;
		}

		CurrentGroup = "";
		CurrentGroupOrder = 0;
		CurrentOrder = 0;
		if (Keys.Count != PrevKeyCount) {
			PrevKeyCount = Keys.Count;
			Keys.Sort(KeyComparer.Instance);
		}

		using var _ = new UILayerScope();
		var cameraRect = Renderer.CameraRect;
		int basicPanelWidth = cameraRect.width / (validWindowCount + 1);
		if (PanelPositionOffset.x == int.MinValue) {
			PanelPositionOffset.x = 1024 + windowIndex * basicPanelWidth;
			PanelPositionOffset.y = 1024 + cameraRect.height;
		}
		var panelRect = new IRect(
			cameraRect.x + PanelPositionOffset.x,
			cameraRect.y + PanelPositionOffset.y,
			basicPanelWidth, 1
		);
		int padding = GUI.FieldPadding;

		Input.CancelIgnoreMouseInput();
		bool mouseHoldingL = Game.IsMouseLeftHolding;
		var rect = panelRect.EdgeInsideUp(GUI.FieldHeight).Shrink(padding, padding, 0, 0);

		// BG
		int border = GUI.Unify(1);
		var bgCell = Renderer.DrawPixel(panelRect, Color32.BLACK);

		// Drag to Move
		var titleCell = Renderer.DrawPixel(rect, Color32.GREY_20);
		GUI.Label(rect, $"Quick Test [{windowIndex}]", GUI.Skin.SmallCenterGreyLabel);
		if (!mouseHoldingL) MouseDragMovingIndex = -1;

		if (
			(rect.Expand(0, PanelMaxExpand, 0, 0).MouseInside() && MouseDragMovingIndex == -1) ||
			MouseDragMovingIndex == windowIndex
		) {
			Cursor.SetCursorAsMove();
			if (Input.MouseLeftButtonDown) {
				MouseDragMovingIndex = windowIndex;
			}
			if (MouseDragMovingIndex == windowIndex && mouseHoldingL) {
				PanelPositionOffset += Input.MouseGlobalPositionDelta;
			}
		}

		// Ignore Btn
		var ignoreRect = rect.EdgeInsideRight(rect.height).Shift(PanelMaxExpand, 0);
		if (GUI.Button(ignoreRect, BuiltInSprite.ICON_DELETE, GUI.Skin.SmallIconButton)) {
			IgnoringWindow = true;
			Input.UseAllMouseKey();
		}
		rect.SlideDown(padding);

		// Content
		int index = 0;
		int indent = GUI.FieldHeight;
		rect = rect.ShrinkLeft(indent);
		int rectLeft = rect.x;
		rect = rect.ShrinkLeft(indent);
		foreach (var kData in Keys) {

			string key = kData.key;
			string group = kData.Group;
			bool groupNotEmpty = !string.IsNullOrEmpty(group);
			bool folding = GroupFolding.TryGetValue(group, out bool _folding) && _folding;
			rect.x = groupNotEmpty ? rectLeft + indent : rectLeft;

			// Update Check
			if (!ShowNotUpdatedData && kData.UpdateFrame < Game.PauselessFrame - 1) continue;

			// Group Fold
			if (group != CurrentGroup) {
				CurrentGroup = group;
				if (groupNotEmpty) {
					using var __ = new GUIContentColorScope(Color32.GREY_128);
					// Tri Mark
					Renderer.Draw(
						folding ? BuiltInSprite.ICON_TRIANGLE_RIGHT : BuiltInSprite.ICON_TRIANGLE_DOWN,
						rect.EdgeOutsideLeft(rect.height - padding).Shift(-indent - padding / 2, 0).Fit(1, 1),
						Color32.GREY_96
					);
					// Fold Button
					if (GUI.Button(rect.Expand(indent, 0, 0, 0), group, GUI.Skin.SmallLabelButton)) {
						GroupFolding[group] = !folding;
					}
					rect.SlideDown();
				}
			}

			// Fold Check
			if (folding) continue;

			// Value
			var valueRect = rect.ShrinkLeft(rect.width / 3);
			switch (kData.Type) {
				// Bool
				case DataType.Bool: {
					var data = BoolPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					data.value = GUI.Toggle(valueRect, data.value);
					// Display Label
					if (data.displayLabel != null) {
						GUI.BackgroundLabel(
							valueRect.ShrinkLeft(valueRect.height + padding + padding),
							data.displayLabel,
							Color32.BLACK,
							out var bounds,
							padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// Int
				case DataType.Int: {
					var data = IntPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int valueLabelWidth = valueRect.height * 2;
					data.value = GUI.HandleSlider(
						3126784 + index + windowIndex * 624123,
						valueRect.ShrinkRight(valueLabelWidth),
						data.value, data.min, data.max,
						step: data.step
					);
					if (data.displayLabel == null) {
						GUI.IntLabel(valueRect.EdgeInsideRight(valueLabelWidth), data.value, GUI.Skin.SmallCenterLabel);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeInsideRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, out var bounds, padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// Float
				case DataType.Float: {
					var data = FloatPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int valueLabelWidth = valueRect.height * 2;
					if (data.displayLabel == null) {
						data.value = GUI.HandleSlider(
							3126784 + index + windowIndex * 624123,
							valueRect.ShrinkRight(valueLabelWidth),
							(data.value * 10000f).RoundToInt(), (data.min * 10000f).RoundToInt(), (data.max * 10000f).RoundToInt(),
							step: (data.step * 10000f).RoundToInt()
						) / 10000f;
						GUI.Label(
							valueRect.EdgeInsideRight(valueLabelWidth),
							data.value.ToString("0.00"),
							GUI.Skin.SmallCenterLabel
						);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeInsideRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, out var bounds, padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// String
				case DataType.String: {
					var data = StringPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int valueLabelWidth = valueRect.height * 2;
					if (data.displayLabel == null) {
						data.value = GUI.InputField(
							3126784 + index + windowIndex * 624123,
							valueRect.ShrinkRight(valueLabelWidth),
							data.value
						);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeInsideRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, out var bounds, padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// Pixels
				case DataType.Pixels: {
					var data = PixelsPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int width = data.pixels.GetLength(0);
					int height = data.pixels.GetLength(1);
					if (width <= 0 || height <= 0) break;
					IRect canvasRect;
					if ((float)valueRect.width / valueRect.height < (float)width / height) {
						// Wide
						canvasRect = valueRect.Fit(width, height, 500, 1000);
					} else {
						// Tall
						canvasRect = valueRect.Envelope(width, height);
						canvasRect.y = valueRect.yMax - canvasRect.height;
					}
					rect = canvasRect;
					float pxWidth = (float)canvasRect.width / width;
					float pxHeight = (float)canvasRect.height / height;
					float pixelFixW = 512f / width;
					float pixelFixH = 512f / height;
					var pRect = new IRect(
						0, 0,
						((pxWidth + pixelFixW) * 5f / 4f).CeilToInt(),
						((pxHeight + pixelFixH) * 5f / 4f).CeilToInt()
					);
					for (int j = 0; j < height; j++) {
						pRect.y = canvasRect.y + (j * pxHeight).RoundToInt();
						for (int i = 0; i < width; i++) {
							pRect.x = canvasRect.x + (i * pxWidth).RoundToInt();
							Game.DrawGizmosRect(pRect, data.pixels[i, j]);
						}
					}
					break;
				}
			}

			// Next
			rect.SlideDown(padding);
			if (rect.height != GUI.FieldHeight) {
				rect.yMin = rect.yMax - GUI.FieldHeight;
			}

			index++;
		}

		// Final
		PanelMaxExpand = Util.Max(PanelMaxExpand, panelRect.width - basicPanelWidth);
		panelRect.yMin = rect.yMax - padding;
		panelRect.width = Util.Max(panelRect.width, basicPanelWidth + PanelMaxExpand);
		bgCell.SetRect(panelRect);
		titleCell.SetRect(panelRect.EdgeInsideUp(titleCell.Height));
		CurrentGroup = "";

		// Clamp
		PanelPositionOffset.Clamp(
			0,
			Util.Min(panelRect.height, cameraRect.height),
			cameraRect.width - panelRect.width,
			cameraRect.height
		);

		// Block Event
		if (panelRect.MouseInside() || MouseDragMovingIndex == windowIndex) {
			Input.IgnoreMouseInput(0);
			Cursor.SetCursorAsNormal(-1);
			Cursor.SetCursor(Cursor.CurrentCursorIndex, int.MaxValue - 1);
		}

	}


	#endregion




	#region --- API ---


	public static void SetCurrentWindow (int index) => CurrentWindowIndex = index.Clamp(0, MAX_WINDOW_COUNT - 1);


	public static void LoadAllDataFromFile (string path) {
		ClearAll();
		if (!Util.FileExists(path)) return;




	}


	public static void SaveAllDataToFile (string path) {



	}


	// Data
	public static bool Bool (string key, bool defaultValue = false, string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].BoolLogic(key, defaultValue, displayLabel);
	private bool BoolLogic (string key, bool defaultValue = false, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (BoolPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value;
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Bool,
			Group = CurrentGroup,
			GroupOrder = CurrentGroupOrder,
			UpdateFrame = Game.PauselessFrame,
		};
		BoolPool.Add(key, new BoolData() {
			value = defaultValue,
			displayLabel = displayLabel,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue;
	}

	public static int Int (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].IntLogic(key, defaultValue, min, max, step, displayLabel);
	private int IntLogic (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (IntPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			result.min = min;
			result.max = max;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value.Clamp(result.min, result.max);
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Int,
			Group = CurrentGroup,
			GroupOrder = CurrentGroupOrder,
			UpdateFrame = Game.PauselessFrame,
		};
		IntPool.Add(key, new IntData() {
			value = defaultValue.Clamp(min, max),
			displayLabel = displayLabel,
			min = min,
			max = max,
			step = step,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}

	public static float Float (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0, string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].FloatLogic(key, defaultValue, min, max, step, displayLabel);
	private float FloatLogic (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0f, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (FloatPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			result.min = min;
			result.max = max;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value.Clamp(result.min, result.max);
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Float,
			Group = CurrentGroup,
			GroupOrder = CurrentGroupOrder,
			UpdateFrame = Game.PauselessFrame,
		};
		FloatPool.Add(key, new FloatData() {
			value = defaultValue.Clamp(min, max),
			displayLabel = displayLabel,
			min = min,
			max = max,
			step = step,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}

	public static string String (string key, string defaultValue = "", string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].StringLogic(key, defaultValue, displayLabel);
	private string StringLogic (string key, string defaultValue = "", string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (StringPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value;
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.String,
			Group = CurrentGroup,
			GroupOrder = CurrentGroupOrder,
			UpdateFrame = Game.PauselessFrame,
		};
		StringPool.Add(key, new StringData() {
			value = defaultValue,
			displayLabel = displayLabel,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue;
	}

	public static void ClearAll (int windowIndex = -1) {
		if (windowIndex < 0) {
			foreach (var win in Windows) {
				win.ClearAllLogic();
			}
		} else {
			Windows[CurrentWindowIndex].ClearAllLogic();
		}
	}
	private void ClearAllLogic () {
		BoolPool.Clear();
		IntPool.Clear();
		FloatPool.Clear();
		StringPool.Clear();
		PixelsPool.Clear();
		ObjectPool.Clear();
		Keys.Clear();
	}


	// Set Data
	public static void SetBool (string key, bool value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetBoolLogic(key, value);
	private void SetBoolLogic (string key, bool value) {
		if (BoolPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	public static void SetInt (string key, int value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetIntLogic(key, value);
	private void SetIntLogic (string key, int value) {
		if (IntPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	public static void SetFloat (string key, float value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetFloatLogic(key, value);
	private void SetFloatLogic (string key, float value) {
		if (FloatPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	public static void SetString (string key, string value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetStringLogic(key, value);
	private void SetStringLogic (string key, string value) {
		if (StringPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}


	// Get Data
	public static bool GetBool (string key, bool defaultValue = false, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetBoolLogic(key, defaultValue);
	private bool GetBoolLogic (string key, bool defaultValue = false) => BoolPool.TryGetValue(key, out var data) ? data.value : defaultValue;

	public static int GetInt (string key, int defaultValue = 0, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetIntLogic(key, defaultValue);
	private int GetIntLogic (string key, int defaultValue = 0) => IntPool.TryGetValue(key, out var data) ? data.value : defaultValue;

	public static float GetFloat (string key, float defaultValue = 0, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetFloatLogic(key, defaultValue);
	private float GetFloatLogic (string key, float defaultValue = 0f) => FloatPool.TryGetValue(key, out var data) ? data.value : defaultValue;

	public static string GetString (string key, string defaultValue = "", int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetStringLogic(key, defaultValue);
	private string GetStringLogic (string key, string defaultValue = "") => StringPool.TryGetValue(key, out var data) ? data.value : defaultValue;


	// Pixels
	public static void StartDrawColumn (string key, int size, bool clearPrevPixels = true, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].StartDrawColumnLogic(key, size, clearPrevPixels);
	private void StartDrawColumnLogic (string key, int size, bool clearPrevPixels = true) => StartDrawPixelsLogic(key, size, (int)(size * 0.618f), clearPrevPixels);


	public static void StartDrawPixels (string key, int width, int height, bool clearPrevPixels = true, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].StartDrawPixelsLogic(key, width, height, clearPrevPixels);
	private void StartDrawPixelsLogic (string key, int width, int height, bool clearPrevPixels = true) {
		ShowingWindow = true;
		CurrentOrder++;
		if (PixelsPool.TryGetValue(key, out var result)) {
			if (clearPrevPixels) {
				System.Array.Clear(result.pixels);
			}
			CurrentPixels = result.pixels;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			return;
		}
		CurrentPixels = new Color32[width, height];
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Pixels,
			Group = CurrentGroup,
			GroupOrder = CurrentGroupOrder,
			UpdateFrame = Game.PauselessFrame,
		};
		PixelsPool.Add(key, new PixelData() {
			pixels = CurrentPixels,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
	}


	public static void DrawColumn (int x, float value01, Color32 color, Color32 bgColor, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].DrawColumnLogic(x, value01, color, bgColor);
	private void DrawColumnLogic (int x, float value01, Color32 color, Color32 bgColor) {
		int height = CurrentPixels.GetLength(1);
		int valueHeight = (height * value01).RoundToInt().Clamp(0, height);
		for (int i = 0; i < height; i++) {
			CurrentPixels[x, i] = i < valueHeight ? color : bgColor;
		}
	}

	public static void DrawPixel (int x, int y, Color32 pixel, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].CurrentPixels[x, y] = pixel;


	// Mark
	public static void Mark (Int2 pos, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(pos.x, pos.y, Stage.ViewZ), duration, size, Color32.RED));
	public static void Mark (int x, int y, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(x, y, Stage.ViewZ), duration, size, Color32.RED));
	public static void Mark (Int2 pos, Color32 color, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(pos.x, pos.y, Stage.ViewZ), duration, size, color));
	public static void Mark (int x, int y, Color32 color, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(x, y, Stage.ViewZ), duration, size, color));


	// Obj
	public static T SetObject<T> (string key, T obj, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetObjectLogic(key, obj);
	private T SetObjectLogic<T> (string key, T obj) => (T)(ObjectPool[key] = obj);


	public static bool TryGetObject<T> (string key, out T result, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].TryGetObjectLogic(key, out result);
	private bool TryGetObjectLogic<T> (string key, out T result) {
		if (ObjectPool.TryGetValue(key, out var obj) && obj is T tObj) {
			result = tObj;
			return true;
		} else {
			result = default;
			return false;
		}
	}


	// Misc
	public static void HideTest () {
		ShowingWindow = false;
		IgnoringWindow = true;
	}


	public static void ShowTest () {
		ShowingWindow = true;
		IgnoringWindow = false;
	}


	public static void Group (string group, bool folding = false, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GroupLogic(group, folding);
	private void GroupLogic (string group, bool folding = false) {
		CurrentGroup = group;
		CurrentGroupOrder++;
		if (!GroupFolding.ContainsKey(group)) {
			GroupFolding[group] = folding;
		}
	}


	#endregion




}
#endif
