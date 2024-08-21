using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

#if DEBUG
public static class QTest {

	// SUB
	private enum DataType { Bool, Int, Float, Pixels, }

	private struct BoolData {
		public bool value;
		public string displayLabel;
		public bool enable;
		public int KeyIndex;
	}

	private struct IntData {
		public int value;
		public int min;
		public int max;
		public int step;
		public string displayLabel;
		public bool enable;
		public int KeyIndex;
	}

	private struct FloatData {
		public float value;
		public float min;
		public float max;
		public float step;
		public string displayLabel;
		public bool enable;
		public int KeyIndex;
	}

	private struct PixelData {
		public Color32[,] pixels;
		public int KeyIndex;
	}

	private struct KeyData {
		public string key;
		public int Order;
		public DataType type;
		public string group;
		public int groupOrder;
	}

	private class KeyComparer : IComparer<KeyData> {
		public static readonly KeyComparer Instance = new();
		public int Compare (KeyData a, KeyData b) {
			int result = a.groupOrder.CompareTo(b.groupOrder);
			return result != 0 ? result : a.Order.CompareTo(b.Order);
		}
	}

	// VAR
	private static readonly Dictionary<string, BoolData> BoolPool = new();
	private static readonly Dictionary<string, IntData> IntPool = new();
	private static readonly Dictionary<string, FloatData> FloatPool = new();
	private static readonly Dictionary<string, PixelData> PixelsPool = new();
	private static readonly Dictionary<string, object> ObjectPool = new();
	private static readonly List<KeyData> Keys = new();
	private static readonly Dictionary<string, bool> GroupFolding = new();
	private static Int2 PanelPositionOffset = new Int2(1024, 1024);
	private static bool MouseDragMoving;
	private static bool ShowingWindow = false;
	private static bool IgnoringWindow = false;
	private static Color32[,] CurrentPixels;
	private static string CurrentGroup = "";
	private static int CurrentGroupOrder;
	private static int CurrentOrder;
	private static int PrevKeyCount = 0;

	// MSG
	[OnGameUpdate(-256)]
	internal static void OnGameUpdate () {

		if (!ShowingWindow || IgnoringWindow || !Game.IsToolApplication) return;
		CurrentGroup = "";
		CurrentGroupOrder = 0;
		CurrentOrder = 0;
		if (Keys.Count != PrevKeyCount) {
			PrevKeyCount = Keys.Count;
			Keys.Sort(KeyComparer.Instance);
		}

		using var _ = new UILayerScope();
		var cameraRect = Renderer.CameraRect;
		var panelRect = new IRect(
			cameraRect.x + PanelPositionOffset.x,
			cameraRect.y + PanelPositionOffset.y,
			cameraRect.width / 3,
			1
		);
		int padding = GUI.FieldPadding;

		Input.CancelIgnoreMouseInput();
		bool mouseHoldingL = Game.IsMouseLeftHolding;
		var rect = panelRect.EdgeUp(GUI.FieldHeight).Shrink(padding, padding, 0, 0);

		// BG
		int border = GUI.Unify(1);
		var bgCell = Renderer.DrawPixel(panelRect, Color32.BLACK);

		// Drag to Move
		GUI.Label(rect, "Quick Test", GUI.Skin.SmallGreyLabel);
		if (!mouseHoldingL) MouseDragMoving = false;
		if (rect.MouseInside() || MouseDragMoving) {
			Cursor.SetCursorAsMove();
			if (Input.MouseLeftButtonDown) {
				MouseDragMoving = true;
			}
			if (MouseDragMoving && mouseHoldingL) {
				PanelPositionOffset += Input.MouseGlobalPositionDelta;
			}
		}

		// Ignore
		var ignoreRect = rect.EdgeRight(rect.height);
		if (GUI.Button(ignoreRect, BuiltInSprite.ICON_DELETE, GUI.Skin.SmallIconButton)) {
			IgnoringWindow = true;
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
			bool folding = GroupFolding.TryGetValue(group, out bool _folding) ? _folding : false;

			// Group Fold
			if (group != CurrentGroup) {
				CurrentGroup = group;
				if (groupNotEmpty) {
					using var __ = new GUIContentColorScope(Color32.GREY_128);
					if (GUI.Button(rect.Expand(indent, 0, 0, 0), group, GUI.Skin.SmallLabelButton)) {
						GroupFolding[group] = !folding;
					}
					rect.SlideDown();
				}
			}

			// Fold Check
			if (folding) continue;

			rect.x = groupNotEmpty ? rectLeft + indent : rectLeft;

			// Value
			var valueRect = rect.ShrinkLeft(rect.width / 3);
			switch (kData.type) {
				// Bool
				case DataType.Bool: {
					var data = BoolPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					using var _e = new GUIEnableScope(data.enable);
					bool newValue = GUI.Toggle(valueRect, data.value);
					if (newValue != data.value) {
						data.value = newValue;
						BoolPool[key] = data;
					}
					// Display Label
					if (data.displayLabel != null) {
						GUI.BackgroundLabel(
							valueRect.ShrinkLeft(valueRect.height + padding + padding),
							data.displayLabel,
							Color32.BLACK, padding,
							style: GUI.Skin.SmallLabel
						);
					}
					break;
				}
				// Int
				case DataType.Int: {
					var data = IntPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					using var _e = new GUIEnableScope(data.enable);
					int valueLabelWidth = valueRect.height * 2;
					int newValue = GUI.HandleSlider(
						3126784 + index,
						valueRect.ShrinkRight(valueLabelWidth),
						data.value, data.min, data.max,
						step: data.step
					);
					if (newValue != data.value) {
						data.value = newValue;
						IntPool[key] = data;
					}
					if (data.displayLabel == null) {
						GUI.IntLabel(valueRect.EdgeRight(valueLabelWidth), newValue, GUI.Skin.SmallCenterLabel);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, padding,
							style: GUI.Skin.SmallLabel
						);
					}
					break;
				}
				// Float
				case DataType.Float: {
					var data = FloatPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					using var _e = new GUIEnableScope(data.enable);
					int valueLabelWidth = valueRect.height * 2;
					if (data.displayLabel == null) {
						float newValue = GUI.HandleSlider(
							3126784 + index,
							valueRect.ShrinkRight(valueLabelWidth),
							(data.value * 100f).RoundToInt(), (data.min * 100f).RoundToInt(), (data.max * 100f).RoundToInt(),
							step: (data.step * 100f).RoundToInt()
						) / 100f;
						if (!newValue.Almost(data.value)) {
							data.value = newValue;
							FloatPool[key] = data;
						}
						GUI.Label(
							valueRect.EdgeRight(valueLabelWidth),
							newValue.ToString("0.00"),
							GUI.Skin.SmallCenterLabel
						);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, padding,
							style: GUI.Skin.SmallLabel
						);
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
					float pixelFixW = 512 / (float)width;
					float pixelFixH = 512 / (float)height;
					var pRect = new IRect(
						0, 0,
						(pxWidth + pixelFixW).CeilToInt(),
						(pxHeight + pixelFixH).CeilToInt()
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
		panelRect.yMin = rect.yMax - padding;
		bgCell.SetRect(panelRect);
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

	// API
	public static bool Bool (string key, bool defaultValue = false, string displayLabel = null, bool enable = true) {
		ShowingWindow = true;
		CurrentOrder++;
		if (BoolPool.TryGetValue(key, out var result)) {
			if (result.displayLabel != displayLabel || result.enable != enable) {
				result.displayLabel = displayLabel;
				result.enable = enable;
				BoolPool[key] = result;
			}
			var kData = Keys[result.KeyIndex];
			kData.Order = CurrentOrder;
			Keys[result.KeyIndex] = kData;
			return result.value;
		}
		BoolPool.Add(key, new BoolData() {
			value = defaultValue,
			displayLabel = displayLabel,
			enable = enable,
			KeyIndex = Keys.Count,
		});
		Keys.Add(new KeyData() {
			key = key,
			Order = CurrentOrder,
			type = DataType.Bool,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
		});
		IgnoringWindow = false;
		return defaultValue;
	}

	public static int Int (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, string displayLabel = null, bool enable = true) {
		ShowingWindow = true;
		CurrentOrder++;
		if (IntPool.TryGetValue(key, out var result)) {
			if (result.displayLabel != displayLabel || result.enable != enable) {
				result.displayLabel = displayLabel;
				result.enable = enable;
				IntPool[key] = result;
			}
			var kData = Keys[result.KeyIndex];
			kData.Order = CurrentOrder;
			Keys[result.KeyIndex] = kData;
			return result.value.Clamp(result.min, result.max);
		}
		IntPool.Add(key, new IntData() {
			value = defaultValue.Clamp(min, max),
			enable = enable,
			displayLabel = displayLabel,
			min = min,
			max = max,
			step = step,
			KeyIndex = Keys.Count,
		});
		Keys.Add(new KeyData() {
			key = key,
			Order = CurrentOrder,
			type = DataType.Int,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
		});
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}

	public static float Float (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0f, string displayLabel = null, bool enable = true) {
		ShowingWindow = true;
		CurrentOrder++;
		if (FloatPool.TryGetValue(key, out var result)) {
			if (result.displayLabel != displayLabel || result.enable != enable) {
				result.displayLabel = displayLabel;
				result.enable = enable;
				FloatPool[key] = result;
			}
			var kData = Keys[result.KeyIndex];
			kData.Order = CurrentOrder;
			Keys[result.KeyIndex] = kData;
			return result.value.Clamp(result.min, result.max);
		}
		FloatPool.Add(key, new FloatData() {
			value = defaultValue.Clamp(min, max),
			enable = enable,
			displayLabel = displayLabel,
			min = min,
			max = max,
			step = step,
			KeyIndex = Keys.Count,
		});
		Keys.Add(new KeyData() {
			key = key,
			Order = CurrentOrder,
			type = DataType.Float,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
		});
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}

	public static void BeginPixels (string key, int width, int height, bool clearPrevPixels = true) {
		ShowingWindow = true;
		CurrentOrder++;
		if (PixelsPool.TryGetValue(key, out var result)) {
			if (clearPrevPixels) {
				System.Array.Clear(result.pixels);
			}
			CurrentPixels = result.pixels;
			var kData = Keys[result.KeyIndex];
			kData.Order = CurrentOrder;
			Keys[result.KeyIndex] = kData;
			return;
		}
		CurrentPixels = new Color32[width, height];
		PixelsPool.Add(key, new PixelData() {
			pixels = CurrentPixels,
			KeyIndex = Keys.Count,
		});
		Keys.Add(new KeyData() {
			key = key,
			Order = CurrentOrder,
			type = DataType.Pixels,
			group = CurrentGroup,
			groupOrder = CurrentGroupOrder,
		});
		IgnoringWindow = false;
	}

	public static void DrawPixel (int x, int y, Color32 pixel) => CurrentPixels[x, y] = pixel;

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

}
#endif
