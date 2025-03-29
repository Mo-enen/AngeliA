namespace AngeliA;

/// <summary>
/// Scope that clamp rendering cells inside given range by changing the size scale of the content
/// </summary>
/// <param name="rect">Rect position in global space</param>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		var range = new IRect();
/// 
/// 		using (new DynamicClampCellScope(range)) {
/// 
/// 			// Rendering cell created inside will be clamped by rescale
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public readonly struct DynamicClampCellScope (IRect rect) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		for (int i = UsedCount; i < count; i++) {
			var cell = cells[i];
			var bounds = cell.GetGlobalBounds();
			minX = Util.Min(minX, bounds.xMin);
			minY = Util.Min(minY, bounds.yMin);
			maxX = Util.Max(maxX, bounds.xMax);
			maxY = Util.Max(maxY, bounds.yMax);
		}
		if (minX == int.MaxValue || minX == maxX || minY == maxY) return;
		var _rect = rect.Fit(maxX - minX, maxY - minY);
		float sizeScl = (float)_rect.width / (maxX - minX);
		int left = _rect.xMin;
		int right = _rect.xMax;
		int down = _rect.yMin;
		int up = _rect.yMax;
		for (int i = UsedCount; i < count; i++) {
			var cell = cells[i];
			cell.X = Util.RemapUnclamped(minX, maxX, left, right, cell.X);
			cell.Y = Util.RemapUnclamped(minY, maxY, down, up, cell.Y);
			cell.Width = (cell.Width * sizeScl).RoundToInt();
			cell.Height = (cell.Height * sizeScl).RoundToInt();
		}
	}
}
