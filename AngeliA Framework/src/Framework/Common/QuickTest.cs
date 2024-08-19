using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

#if DEBUG


public static class QTest {

	// SUB
	private enum DataType { Bool, Int, Float, }

	// VAR
	private static readonly Dictionary<string, bool> BoolPool = new();
	private static readonly Dictionary<string, (int value, int min, int max)> IntPool = new();
	private static readonly Dictionary<string, (float value, float min, float max)> FloatPool = new();
	private static readonly List<(string key, DataType type)> Keys = new();
	private static bool ShowingWindow = false;
	private static IRect PanelUnitRect = new IRect(100, 100, 200, 200);

	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Keys.Clear();
		BoolPool.Clear();
		IntPool.Clear();
		FloatPool.Clear();
	}


	[OnGameUpdateLater(4096)]
	internal static void OnGameUpdateLater () {

		if (!ShowingWindow) return;

		using var _ = new UILayerScope();
		var cameraRect = Renderer.CameraRect;
		var panelRect = new IRect(
			cameraRect.x + GUI.Unify(PanelUnitRect.x),
			cameraRect.y + GUI.Unify(PanelUnitRect.y),
			GUI.Unify(PanelUnitRect.width),
			GUI.Unify(PanelUnitRect.height)
		);

		// BG
		int border = GUI.Unify(2);
		Renderer.DrawPixel(panelRect, Color32.BLACK);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16,
			panelRect.x, panelRect.y, 0, 0, 0, panelRect.width, panelRect.height,
			border, border, border, border, Const.SliceIgnoreCenter, Color32.WHITE, z: 0
		);

		// Content
		foreach (var (key, type) in Keys) {
			switch (type) {
				// Bool
				case DataType.Bool: {




					break;
				}
				// Int
				case DataType.Int: {




					break;
				}
				// Float
				case DataType.Float: {




					break;
				}
			}
		}

		// Block Event
		if (panelRect.MouseInside() && Input.AnyMouseButtonDown) {
			Input.UseAllMouseKey();
		}

	}

	// API
	public static bool Bool (string key, bool defaultValue = false) {
		ShowingWindow = true;
		if (BoolPool.TryGetValue(key, out var result)) {
			return result;
		}
		BoolPool.Add(key, defaultValue);
		Keys.Add((key, DataType.Bool));
		return defaultValue;
	}

	public static int Int (string key, int defaultValue = 0, int min = 0, int max = 100) {
		ShowingWindow = true;
		if (IntPool.TryGetValue(key, out var result)) {
			return result.value.Clamp(result.min, result.max);
		}
		IntPool.Add(key, (defaultValue.Clamp(min, max), min, max));
		Keys.Add((key, DataType.Int));
		return defaultValue.Clamp(min, max);
	}

	public static float Float (string key, float defaultValue = 0, float min = 0, float max = 1f) {
		ShowingWindow = true;
		if (FloatPool.TryGetValue(key, out var result)) {
			return result.value.Clamp(result.min, result.max);
		}
		FloatPool.Add(key, (defaultValue.Clamp(min, max), min, max));
		Keys.Add((key, DataType.Float));
		return defaultValue.Clamp(min, max);
	}

}


#endif
