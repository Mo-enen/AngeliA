using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

internal partial class GameEditor {


	// VAR
	private readonly object ScreenThumbnailTexture;
	private readonly float[] HueAmount = new float[24];
	private readonly float[] SaturationAmount = new float[24];
	private readonly float[] ValueAmount = new float[24];
	private readonly float[] DullHueAmount = new float[3];
	private float MaxHueAmount;
	private float MaxSaturationAmount;
	private float MaxValueAmount;
	private int RequireColorAnalyzeFrame = -2;
	private FRect ThumbnailUV;
	private Int2 ScreenTextureSize;


	// MSG
	private void DrawColorAnalyzerPanel (ref IRect panelRect) {

		if (Game.GlobalFrame == RequireColorAnalyzeFrame || GenericDialogUI.ShowingDialog) {
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
			rect.SlideDown(padding + GUI.FieldHeight);
			rect.yMin = rect.yMax - toolbarSize;
		}

		// Refresh Button
		if (GUI.Button(rect.Shrink(padding * 2, padding * 2, 0, 0), BuiltInText.UI_REFRESH) || RequireColorAnalyzeFrame < 0) {
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
		System.Array.Clear(DullHueAmount);
		System.Array.Clear(SaturationAmount);
		System.Array.Clear(ValueAmount);
		MaxHueAmount = 0f;
		MaxSaturationAmount = 0f;
		MaxValueAmount = 0f;
		int hueCount = HueAmount.Length;
		int dullHueCount = DullHueAmount.Length;
		int satCount = SaturationAmount.Length;
		int valCount = ValueAmount.Length;
		var screenPixels = Game.GetPixelsFromTexture(screenTexture);
		int len = screenPixels.Length;
		for (int i = 0; i < len; i++) {
			var pix = screenPixels[i];
			Util.RgbToHsv(pix, out float h, out float s, out float v);
			// Hue
			if (v >= 0.2f && s >= 0.2f) {
				int hueIndex = (h * hueCount).RoundToInt().Clamp(0, hueCount - 1);
				HueAmount[hueIndex] += 0.618f;
			} else {
				int hueIndex = (v * dullHueCount).RoundToInt().Clamp(0, dullHueCount - 1);
				DullHueAmount[hueIndex] += 0.618f;
			}
			// Saturation
			int sIndex = (s * satCount).RoundToInt().Clamp(0, satCount - 1);
			SaturationAmount[sIndex] += 1f;
			// Value
			int vIndex = (v * valCount).RoundToInt().Clamp(0, valCount - 1);
			ValueAmount[vIndex] += 1f;
		}

		// Cache
		for (int i = 0; i < hueCount; i++) {
			MaxHueAmount = Util.Max(MaxHueAmount, HueAmount[i]);
		}
		for (int i = 0; i < dullHueCount; i++) {
			MaxHueAmount = Util.Max(MaxHueAmount, DullHueAmount[i]);
		}
		for (int i = 0; i < satCount; i++) {
			MaxSaturationAmount = Util.Max(MaxSaturationAmount, SaturationAmount[i]);
		}
		for (int i = 0; i < valCount; i++) {
			MaxValueAmount = Util.Max(MaxValueAmount, ValueAmount[i]);
		}

	}

	private void DrawColorAnalyzeChart (IRect rect) {

		rect = rect.Shrink(GUI.FieldPadding, 0, GUI.FieldPadding, GUI.FieldPadding);

		DrawChart(rect.PartHorizontal(0, 3).ShrinkRight(GUI.FieldPadding), HueAmount, MaxHueAmount, 0, DullHueAmount);

		DrawChart(rect.PartHorizontal(1, 3).ShrinkRight(GUI.FieldPadding), SaturationAmount, MaxSaturationAmount, 1, DullHueAmount);

		DrawChart(rect.PartHorizontal(2, 3).ShrinkRight(GUI.FieldPadding), ValueAmount, MaxValueAmount, 2, DullHueAmount);

		// Final
		static void DrawChart (IRect rect, float[] amounts, float max, int typeIndex, float[] dullHue) {

			// BG
			Renderer.DrawPixel(rect, Color32.GREY_20);

			// Label
			GUI.Label(
				rect.EdgeOutsideDown(GUI.FieldHeight),
				typeIndex == 0 ? "H" : typeIndex == 1 ? "S" : "V",
				GUI.Skin.SmallCenterGreyLabel
			);

			// Chart
			max = Util.Max(max, 0.00000001f);
			int left = rect.x;
			int right = rect.xMax;
			int down = rect.y;
			int up = rect.yMax;
			int count = amounts.Length;
			int dullCount = dullHue.Length;
			for (int i = 0; i < count; i++) {
				if (typeIndex == 0) {
					// H
					int x = (int)Util.LerpUnclamped(left, right, i / (float)(count + dullCount));
					int nextX = (int)Util.LerpUnclamped(left, right, (i + 1) / (float)(count + dullCount));
					int top = (int)Util.LerpUnclamped(down, up, amounts[i] / max);
					Game.DrawGizmosRect(
						IRect.MinMaxRect(x, down, nextX, top),
						Util.HsvToRgb(i / (float)count, 1f, 1f)
					);
				} else if (typeIndex == 1) {
					// S
					int x = (int)Util.LerpUnclamped(left, right, i / (float)count);
					int nextX = (int)Util.LerpUnclamped(left, right, (i + 1) / (float)count);
					int top = (int)Util.LerpUnclamped(down, up, amounts[i] / max);
					Game.DrawGizmosRect(
						IRect.MinMaxRect(x, down, nextX, top),
						Util.HsvToRgb(0.618f, i / (float)count, 1f)
					);
				} else {
					// V
					int x = (int)Util.LerpUnclamped(left, right, i / (float)count);
					int nextX = (int)Util.LerpUnclamped(left, right, (i + 1) / (float)count);
					int top = (int)Util.LerpUnclamped(down, up, amounts[i] / max);
					Game.DrawGizmosRect(
						IRect.MinMaxRect(x, down, nextX, top),
						Util.HsvToRgb(0f, 1f, Util.LerpUnclamped(0.3f, 1f, i / (float)count))
					);
				}
			}

			// Dull Hue
			if (typeIndex == 0) {
				left = (int)Util.LerpUnclamped(left, right, count / (float)(count + dullCount));
				for (int i = 0; i < dullCount; i++) {
					int x = (int)Util.LerpUnclamped(left, right, i / (float)dullCount);
					int nextX = (int)Util.LerpUnclamped(left, right, (i + 1) / (float)dullCount);
					int top = (int)Util.LerpUnclamped(down, up, dullHue[i] / max);
					Game.DrawGizmosRect(
						IRect.MinMaxRect(x, down, nextX, top),
						Util.HsvToRgb(0f, 0f, Util.LerpUnclamped(0.3f, 1f, i / (float)dullCount))
					);
				}
			}
		}
	}

}
