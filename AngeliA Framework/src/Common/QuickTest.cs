using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

#if DEBUG
public static class QTest {



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
		public DataType type;
		public string group;
		public int groupOrder;
		public int UpdateFrame;
	}

	private class KeyComparer : IComparer<KeyData> {
		public static readonly KeyComparer Instance = new();
		public int Compare (KeyData a, KeyData b) {
			int result = a.groupOrder.CompareTo(b.groupOrder);
			return result != 0 ? result : a.Order.CompareTo(b.Order);
		}
	}


	#endregion




	#region --- VAR ---


	// Api
	public static bool Testing => ShowingWindow && !IgnoringWindow;
	public static bool ShowNotUpdatedData { get; set; } = true;

	// Data
	private static readonly Dictionary<string, BoolData> BoolPool = [];
	private static readonly Dictionary<string, IntData> IntPool = [];
	private static readonly Dictionary<string, FloatData> FloatPool = [];
	private static readonly Dictionary<string, StringData> StringPool = [];
	private static readonly Dictionary<string, PixelData> PixelsPool = [];
	private static readonly Dictionary<string, object> ObjectPool = [];
	private static readonly List<KeyData> Keys = [];
	private static readonly Dictionary<string, bool> GroupFolding = [];
	private static readonly List<(int startFrame, Int3 pos, int duration, int size, Color32 color)> TrailingMark = [];
	private static Int2 PanelPositionOffset = new(1024, 1024);
	private static bool MouseDragMoving;
	private static bool ShowingWindow = false;
	private static bool IgnoringWindow = false;
	private static Color32[,] CurrentPixels;
	private static string CurrentGroup = "";
	private static int CurrentGroupOrder;
	private static int CurrentOrder;
	private static int PrevKeyCount = 0;
	private static int PanelMaxExpand = 0;


	#endregion




	#region --- MSG ---


	[OnGameUpdate(-256)]
	internal static void OnGameUpdate () {

		if (!ShowingWindow || IgnoringWindow) return;

		CurrentGroup = "";
		CurrentGroupOrder = 0;
		CurrentOrder = 0;
		if (Keys.Count != PrevKeyCount) {
			PrevKeyCount = Keys.Count;
			Keys.Sort(KeyComparer.Instance);
		}

		using var _ = new UILayerScope();
		var cameraRect = Renderer.CameraRect;
		int basicPanelWidth = cameraRect.width / 3;
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
		GUI.Label(rect, "Quick Test", GUI.Skin.SmallCenterGreyLabel);
		if (!mouseHoldingL) MouseDragMoving = false;
		if (rect.Expand(0, PanelMaxExpand, 0, 0).MouseInside() || MouseDragMoving) {
			Cursor.SetCursorAsMove();
			if (Input.MouseLeftButtonDown) {
				MouseDragMoving = true;
			}
			if (MouseDragMoving && mouseHoldingL) {
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
			string group = kData.group;
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
			switch (kData.type) {
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
						3126784 + index,
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
							3126784 + index,
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
						data.value = GUI.InputField(3126784 + index, valueRect.ShrinkRight(valueLabelWidth), data.value);
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
		if (panelRect.MouseInside() || MouseDragMoving) {
			Input.IgnoreMouseInput(0);
			Cursor.SetCursorAsNormal(-1);
			Cursor.SetCursor(Cursor.CurrentCursorIndex, int.MaxValue - 1);
		}

	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (Testing) {
			Game.ForceGizmosOnTopOfUI(1);
			LightingSystem.IgnoreLighting(1);
			Input.IgnoreMouseToActionJump();
		}
		// Draw Trailing Mark
		if (Renderer.TryGetSprite(BuiltInSprite.CIRCLE_32, out var circleSP)) {
			var cameraRect = Renderer.CameraRect;
			using var _ = new UILayerScope(ignoreSorting: true);
			for (int i = 0; i < TrailingMark.Count; i++) {
				var (frame, pos, dur, size, color) = TrailingMark[i];
				int localFrame = Game.GlobalFrame - frame;
				if (localFrame >= dur) {
					TrailingMark.RemoveAt(i);
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


	#endregion




	#region --- API ---


	// Data
	public static bool Bool (string key, bool defaultValue = false, string displayLabel = null) {
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
			type = DataType.Bool,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
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


	public static int Int (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (IntPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value.Clamp(result.min, result.max);
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			type = DataType.Int,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
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


	public static float Float (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0f, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (FloatPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value.Clamp(result.min, result.max);
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			type = DataType.Float,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
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


	public static string String (string key, string defaultValue = "", string displayLabel = null) {
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
			type = DataType.String,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
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


	public static void ClearAll () {
		BoolPool.Clear();
		IntPool.Clear();
		FloatPool.Clear();
		StringPool.Clear();
		PixelsPool.Clear();
		ObjectPool.Clear();
		Keys.Clear();
	}


	// Set Data
	public static void SetBool (string key, bool value) {
		if (BoolPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	public static void SetInt (string key, int value) {
		if (IntPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	public static void SetFloat (string key, float value) {
		if (FloatPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	public static void SetString (string key, string value) {
		if (StringPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}


	// Pixels
	public static void StartDrawColumn (string key, int size, bool clearPrevPixels = true) => StartDrawPixels(key, size, (int)(size * 0.618f), clearPrevPixels);
	public static void StartDrawPixels (string key, int width, int height, bool clearPrevPixels = true) {
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
			type = DataType.Pixels,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
			UpdateFrame = Game.PauselessFrame,
		};
		PixelsPool.Add(key, new PixelData() {
			pixels = CurrentPixels,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
	}


	public static void DrawColumn (int x, float value01, Color32 color, Color32 bgColor) {
		int height = CurrentPixels.GetLength(1);
		int valueHeight = (height * value01).RoundToInt().Clamp(0, height);
		for (int i = 0; i < height; i++) {
			CurrentPixels[x, i] = i < valueHeight ? color : bgColor;
		}
	}
	public static void DrawPixel (int x, int y, Color32 pixel) => CurrentPixels[x, y] = pixel;


	// Trailing
	public static void AddTrailingMark (Int2 pos, int duration = 60, int size = 42) => TrailingMark.Add((Game.GlobalFrame, new Int3(pos.x, pos.y, Stage.ViewZ), duration, size, Color32.RED));
	public static void AddTrailingMark (int x, int y, int duration = 60, int size = 42) => TrailingMark.Add((Game.GlobalFrame, new Int3(x, y, Stage.ViewZ), duration, size, Color32.RED));
	public static void AddTrailingMark (Int2 pos, Color32 color, int duration = 60, int size = 42) => TrailingMark.Add((Game.GlobalFrame, new Int3(pos.x, pos.y, Stage.ViewZ), duration, size, color));
	public static void AddTrailingMark (int x, int y, Color32 color, int duration = 60, int size = 42) => TrailingMark.Add((Game.GlobalFrame, new Int3(x, y, Stage.ViewZ), duration, size, color));


	// Obj
	public static T SetObject<T> (string key, T obj) => (T)(ObjectPool[key] = obj);


	public static bool TryGetObject<T> (string key, out T result) {
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


	public static void Group (string group, bool folding = false) {
		CurrentGroup = group;
		CurrentGroupOrder++;
		if (!GroupFolding.ContainsKey(group)) {
			GroupFolding[group] = folding;
		}
	}


	#endregion




}
#endif
