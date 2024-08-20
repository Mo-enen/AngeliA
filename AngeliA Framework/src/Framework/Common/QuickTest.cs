using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

#if DEBUG
public static class QTest {

	// SUB
	private enum DataType { Bool, Int, Float, Pixels, }

	// VAR
	private static readonly Dictionary<string, (bool value, string displayLabel, bool enable)> BoolPool = new();
	private static readonly Dictionary<string, (int value, int min, int max, int step, string displayLabel, bool enable)> IntPool = new();
	private static readonly Dictionary<string, (float value, float min, float max, float step, string displayLabel, bool enable)> FloatPool = new();
	private static readonly Dictionary<string, (Color32[,] pixels, string displayLabel)> PixelsPool = new();
	private static readonly Dictionary<string, object> ObjectPool = new();
	private static readonly List<(string key, DataType type)> Keys = new();
	private static Int2 PanelPositionOffset = new Int2(1024, 1024);
	private static bool MouseDragMoving;
	private static bool ShowingWindow = false;
	private static bool IgnoringWindow = false;
	private static Color32[,] CurrentPixels;

	// MSG
	[OnGameUpdate(-256)]
	internal static void OnGameUpdate () {

		if (!ShowingWindow || IgnoringWindow || !Game.IsToolApplication) return;

		using var _ = new UILayerScope();
		var cameraRect = Renderer.CameraRect;
		var panelRect = new IRect(
			cameraRect.x + PanelPositionOffset.x,
			cameraRect.y + PanelPositionOffset.y,
			cameraRect.width / 3,
			1
		);

		Input.CancelIgnoreMouseInput();
		bool mouseHoldingL = Game.IsMouseLeftHolding;
		var rect = panelRect.EdgeUp(GUI.FieldHeight).Shrink(GUI.FieldPadding, GUI.FieldPadding, 0, 0);

		// BG
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
		rect.SlideDown(GUI.FieldPadding);

		// Content
		int index = 0;
		foreach (var (key, type) in Keys) {

			// Separate
			if (string.IsNullOrWhiteSpace(key)) {
				Renderer.Draw(
					BuiltInSprite.SOFT_LINE_H,
					rect.CornerInside(Alignment.TopMid, rect.width, GUI.Unify(1)),
					Color32.GREY_42
				);
				rect.y -= GUI.FieldPadding;
				continue;
			}

			// Value
			var valueRect = rect.ShrinkLeft(rect.width / 3);
			switch (type) {
				// Bool
				case DataType.Bool: {
					var (value, displayLabel, enable) = BoolPool[key];
					// Label
					GUI.SmallLabel(rect, displayLabel ?? key);
					// Value
					using var _e = new GUIEnableScope(enable);
					bool newValue = GUI.Toggle(valueRect, value);
					if (newValue != value) {
						BoolPool[key] = (newValue, displayLabel, enable);
					}
					break;
				}
				// Int
				case DataType.Int: {
					var (value, min, max, step, displayLabel, enable) = IntPool[key];
					// Label
					GUI.SmallLabel(rect, displayLabel ?? key);
					// Value
					using var _e = new GUIEnableScope(enable);
					int valueLabelWidth = valueRect.height * 2;
					int newValue = GUI.HandleSlider(
						3126784 + index,
						valueRect.ShrinkRight(valueLabelWidth),
						value, min, max,
						step: step
					);
					if (newValue != value) {
						IntPool[key] = (newValue, min, max, step, displayLabel, enable);
					}
					GUI.IntLabel(valueRect.EdgeRight(valueLabelWidth), newValue, GUI.Skin.SmallCenterLabel);
					break;
				}
				// Float
				case DataType.Float: {
					var (value, min, max, step, displayLabel, enable) = FloatPool[key];
					// Label
					GUI.SmallLabel(rect, displayLabel ?? key);
					// Value
					using var _e = new GUIEnableScope(enable);
					int valueLabelWidth = valueRect.height * 2;
					float newValue = GUI.HandleSlider(
						3126784 + index,
						valueRect.ShrinkRight(valueLabelWidth),
						(value * 100f).RoundToInt(), (min * 100f).RoundToInt(), (max * 100f).RoundToInt(),
						step: (step * 100f).RoundToInt()
					) / 100f;
					if (!newValue.Almost(value)) {
						FloatPool[key] = (newValue, min, max, step, displayLabel, enable);
					}
					GUI.Label(
						valueRect.EdgeRight(valueLabelWidth),
						newValue.ToString("0.00"),
						GUI.Skin.SmallCenterLabel
					);
					break;
				}
				// Pixels
				case DataType.Pixels: {
					var (pixels, displayLabel) = PixelsPool[key];
					// Label
					GUI.SmallLabel(rect, displayLabel ?? key);
					// Value
					int width = pixels.GetLength(0);
					int height = pixels.GetLength(1);
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
							Game.DrawGizmosRect(pRect, pixels[i, j]);
						}
					}
					break;
				}
			}

			// Next
			rect.SlideDown(GUI.FieldPadding);
			if (rect.height != GUI.FieldHeight) {
				rect.yMin = rect.yMax - GUI.FieldHeight;
			}

			index++;
		}

		// Final
		panelRect.yMin = rect.yMax - GUI.FieldPadding;
		bgCell.SetRect(panelRect);
		int border = GUI.Unify(1);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16,
			panelRect.x, panelRect.y, 0, 0, 0, panelRect.width, panelRect.height,
			border, border, border, border, Const.SliceIgnoreCenter, Color32.GREY_128, z: 0
		);

		// Clamp
		PanelPositionOffset.Clamp(0, panelRect.height, cameraRect.width - panelRect.width, cameraRect.height);

		// Block Event
		if (panelRect.MouseInside() || MouseDragMoving) {
			Input.IgnoreMouseInput(0);
			Cursor.SetCursorAsNormal(-1);
			Cursor.SetCursor(Cursor.CurrentCursorIndex, int.MaxValue - 1);
		}

	}

	// API
	public static bool Bool (string key, bool defaultValue = false, bool separate = false, string displayLabel = null, bool enable = true) {
		ShowingWindow = true;
		if (BoolPool.TryGetValue(key, out var result)) {
			if (result.displayLabel != displayLabel || result.enable != enable) {
				result.displayLabel = displayLabel;
				result.enable = enable;
				BoolPool[key] = result;
			}
			return result.value;
		}
		BoolPool.Add(key, (defaultValue, displayLabel, enable));
		if (separate) {
			Keys.Add(("", DataType.Bool));
		}
		Keys.Add((key, DataType.Bool));
		IgnoringWindow = false;
		return defaultValue;
	}

	public static int Int (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, bool separate = false, string displayLabel = null, bool enable = true) {
		ShowingWindow = true;
		if (IntPool.TryGetValue(key, out var result)) {
			if (result.displayLabel != displayLabel || result.enable != enable) {
				result.displayLabel = displayLabel;
				result.enable = enable;
				IntPool[key] = result;
			}
			return result.value.Clamp(result.min, result.max);
		}
		IntPool.Add(key, (defaultValue.Clamp(min, max), min, max, step, displayLabel, enable));
		if (separate) {
			Keys.Add(("", DataType.Int));
		}
		Keys.Add((key, DataType.Int));
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}

	public static float Float (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0f, bool separate = false, string displayLabel = null, bool enable = true) {
		ShowingWindow = true;
		if (FloatPool.TryGetValue(key, out var result)) {
			if (result.displayLabel != displayLabel || result.enable != enable) {
				result.displayLabel = displayLabel;
				result.enable = enable;
				FloatPool[key] = result;
			}
			return result.value.Clamp(result.min, result.max);
		}
		FloatPool.Add(key, (defaultValue.Clamp(min, max), min, max, step, displayLabel, enable));
		if (separate) {
			Keys.Add(("", DataType.Float));
		}
		Keys.Add((key, DataType.Float));
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}

	public static void BeginPixels (string key, int width, int height, bool separate = false, bool clearPrevPixels = true, string displayLabel = null) {
		ShowingWindow = true;
		if (PixelsPool.TryGetValue(key, out var result)) {
			if (clearPrevPixels) {
				System.Array.Clear(result.pixels);
			}
			if (result.displayLabel != displayLabel) {
				result.displayLabel = displayLabel;
				PixelsPool[key] = result;
			}
			CurrentPixels = result.pixels;
			return;
		}
		CurrentPixels = new Color32[width, height];
		PixelsPool.Add(key, (CurrentPixels, displayLabel));
		if (separate) {
			Keys.Add(("", DataType.Pixels));
		}
		Keys.Add((key, DataType.Pixels));
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

}
#endif
