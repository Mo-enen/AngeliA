using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

internal partial class GameEditor {


	// VAR
	const int HUE_ITER_COUNT = 2;
	private readonly object ScreenThumbnailTexture;
	private readonly float[] HueAmount = new float[360];
	private readonly float[] HueAmountAlt = new float[360];
	private float MaxHueAmount;
	private int RequireColorAnalyzeFrame = -2;
	private FRect ThumbnailUV;
	private Int2 ScreenTextureSize;


	// MSG
	private void DrawColorAnalyzerPanel (ref IRect panelRect) {

		if (Game.GlobalFrame == RequireColorAnalyzeFrame) {
			Game.CancelGizmosOnTopOfUI();
			return;
		}

		Game.ForceGizmosOnTopOfUI(1);

		// Min Width
		int minWidth = Unify(396);
		if (panelRect.width < minWidth) {
			panelRect.xMin -= minWidth - panelRect.width;
		}

		// Content
		int padding = GUI.FieldPadding;
		int toolbarSize = Unify(28);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);

		// Thumbnail
		if (RequireColorAnalyzeFrame >= 0) {
			int thumbnailHeight = ((ScreenTextureSize.y * ThumbnailUV.height) * (rect.width - padding * 2) / (ScreenTextureSize.x * ThumbnailUV.width)).RoundToInt();
			rect.yMin = rect.yMax - thumbnailHeight;
			Game.DrawGizmosTexture(rect.Shrink(padding, padding, -padding, padding), ThumbnailUV, ScreenThumbnailTexture, flipY: true);
			rect.SlideDown(padding);
			rect.yMin = rect.yMax - toolbarSize;
		} else {
			rect.y -= Unify(64);
		}

		// Result Chart
		if (RequireColorAnalyzeFrame >= 0) {
			int chartHeight = Unify(128);
			rect.yMin = rect.yMax - chartHeight;
			DrawColorAnalyzeChart(rect.Shrink(padding));
			rect.SlideDown(padding);
			rect.yMin = rect.yMax - toolbarSize;
		}

		// Analyze Button
		if (GUI.Button(rect.Shrink(padding * 2, padding * 2, 0, 0), BuiltInText.UI_APPLY)) {
			RequireColorAnalyzeFrame = Game.GlobalFrame + 1;
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;

	}

	private void PerformColorAnalyze () {

		var cameraRect = Renderer.CameraRect;
		var screenTexture = Game.GetScreenRenderingTexture();
		ScreenTextureSize = Game.GetTextureSize(screenTexture);
		ScreenTextureSize.x = ScreenTextureSize.x.GreaterOrEquel(1);
		ScreenTextureSize.y = ScreenTextureSize.y.GreaterOrEquel(1);
		var sourceRange = new IRect(
			0, 0, ScreenTextureSize.x, ScreenTextureSize.y
		).Shrink(
			(RiggedGameLeftBarWidth * ScreenTextureSize.x / (float)cameraRect.width).CeilToInt(),
			(ToolbarRect.width * ScreenTextureSize.x / (float)cameraRect.width).CeilToInt(),
			0, 0
		);

		// Get Thumbnail
		Game.FillResizedTexture(screenTexture, ScreenThumbnailTexture);
		ThumbnailUV = new FRect(
			sourceRange.x / (float)ScreenTextureSize.x,
			sourceRange.y / (float)ScreenTextureSize.y,
			sourceRange.width / (float)ScreenTextureSize.x,
			sourceRange.height / (float)ScreenTextureSize.y
		);

		// Calculate Result
		System.Array.Clear(HueAmount);
		System.Array.Clear(HueAmountAlt);
		MaxHueAmount = 0f;
		var screenPixels = Game.GetPixelsFromTexture(screenTexture);
		int len = screenPixels.Length;
		for (int i = 0; i < len; i++) {
			var pix = screenPixels[i];
			Util.RgbToHsv(pix, out float h, out float s, out float v);
			if (s < 0.1f || v < 0.1f) continue;
			int hueIndex = (h * 360).RoundToInt().Clamp(0, 359);
			float hueAmount = s * 0.3f + v;
			HueAmount[hueIndex] += hueAmount;
		}

		// Iterate Hue
		for (int iter = 0; iter < HUE_ITER_COUNT; iter++) {
			var (source, target) = iter % 2 == 0 ? (HueAmount, HueAmountAlt) : (HueAmountAlt, HueAmount);
			for (int i = 0; i < 360; i++) {
				if (source[i].AlmostZero()) continue;
				target[i] = GetIterateHue(source, i);
			}
			static float GetIterateHue (float[] hues, int index) {
				const int RAD = 2;
				float result = 0f;
				int left = index - RAD;
				int right = index + RAD;
				for (int i = left; i <= right; i++) {
					result += hues[i.UMod(360)];
				}
				return result / (RAD * 2 + 1);
			}
		}

		// Cache
		var targetCache = HUE_ITER_COUNT % 2 == 0 ? HueAmount : HueAmountAlt;
		for (int i = 0; i < 360; i++) {
			float amount = targetCache[i];
			amount = (float)System.Math.Log(amount + 1f);
			targetCache[i] = amount;
			MaxHueAmount = Util.Max(MaxHueAmount, amount);
		}

	}

	private void DrawColorAnalyzeChart (IRect rect) {
		var hues = HUE_ITER_COUNT % 2 == 0 ? HueAmount : HueAmountAlt;
		int left = rect.x;
		int right = rect.xMax;
		int down = rect.y;
		int up = rect.yMax;
		float maxHue = Util.Max(MaxHueAmount, 0.00000001f);
		for (int i = 0; i < 360; i++) {
			int x = (int)Util.LerpUnclamped(left, right, i / 360f);
			int nextX = (int)Util.LerpUnclamped(left, right, (i + 1) / 360f);
			int top = (int)Util.LerpUnclamped(down, up, hues[i] / maxHue);
			Game.DrawGizmosLine(x, down, x, top, nextX - x, Util.HsvToRgb(i / 360f, 1f, 1f));
		}
	}

}
