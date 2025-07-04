namespace AngeliA;

/// <summary>
/// Scope to clamp rendering cell into given rect position
/// </summary>
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
/// 		int x = QTest.Int("X", 0, 0, Const.CEL * 128);
/// 		int y = QTest.Int("Y", 0, 0, Const.CEL * 128);
/// 		int w = QTest.Int("W", Const.CEL * 24, 0, Const.CEL * 128);
/// 		int h = QTest.Int("H", Const.CEL * 24, 0, Const.CEL * 128);
/// 
/// 		var cameraRect = Renderer.CameraRect;
/// 
/// 		Renderer.SetLayer(RenderLayer.UI);
/// 
/// 		using (new ClampCellsScope(new IRect(cameraRect.x + x, cameraRect.y + y, w, h))) {
/// 
/// 			// Rendering cell created inside will have be clamped inside
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.Shrink(Const.CEL * 3).Fit(1, 1));
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public readonly struct ClampCellsScope : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly IRect rect;

	/// <summary>
	/// Scope to clamp rendering cell into given rect position
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	public ClampCellsScope (IRect rect) {
		this.rect = rect;
	}

	public readonly void Dispose () {
		Renderer.ClampCells(LayerIndex, rect, UsedCount);
	}
}
