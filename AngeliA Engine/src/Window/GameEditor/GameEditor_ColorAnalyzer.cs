using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

internal partial class GameEditor {

	// VAR
	private readonly object ScreenThumbnailTexture;
	private readonly object ColorAnalyzeChartTexture;
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
			var chartSize = Game.GetTextureSize(ColorAnalyzeChartTexture);
			int chartHeight = chartSize.y * (rect.width - padding * 2) / chartSize.x;
			rect.yMin = rect.yMax - chartHeight;
			Game.DrawGizmosTexture(rect.Shrink(padding, padding, -padding, padding), ColorAnalyzeChartTexture);
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



		// Get Result Chart




	}

}
