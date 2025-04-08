using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Class that makes rendering cells pixel perfect
/// </summary>
public static class PixelPerfect {

	// VAR
	/// <summary>
	/// True if require pixel perfect for current frame
	/// </summary>
	public static readonly FrameBasedBool Enable = new(false);
	/// <summary>
	/// True if require pixel perfect for UI rendering layer
	/// </summary>
	public static bool IncludeUiLayer { get; set; } = false;
	/// <summary>
	/// Global unit / pixel. Default 16.
	/// </summary>
	public static readonly FrameBasedInt PixelScale = new(Const.ART_SCALE);

	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable.BaseValue = Universe.BuiltInInfo.UsePixelPerfectRendering;
	}

	[OnGameUpdateLater(4096)]
	internal static void OnGameUpdateLater () {
		if (!Enable) return;
		for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
			if (layer == RenderLayer.UI && !IncludeUiLayer) continue;
			if (!Renderer.GetCells(layer, out var cells, out int count)) continue;
			int pixScale = layer == RenderLayer.BEHIND ?
				PixelScale * 1000 / Universe.BuiltInInfo.WorldBehindParallax.GreaterOrEquel(1) :
				PixelScale;
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				cell.ReturnPivots();
				int l = cell.X / pixScale * pixScale;
				int r = (cell.X + cell.Width) / pixScale * pixScale;
				int d = cell.Y / pixScale * pixScale;
				int u = (cell.Y + cell.Height) / pixScale * pixScale;
				cell.SetRect(new IRect(l, d, r - l, u - d));
			}
		}
	}

}
