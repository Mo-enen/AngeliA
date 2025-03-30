namespace AngeliA;

/// <summary>
/// Scope that shift position of the rendering cells inside
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
/// 		using (new ShiftCellsScope(/*Your Offset Position Here*/)) {
/// 
/// 			// Rendering cells inside will offset their position
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct ShiftCellsScope : System.IDisposable {

	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly Int2 Shift;

	/// <summary>
	/// Scope that shift position of the rendering cells inside
	/// </summary>
	public ShiftCellsScope () : this(default) { }
	/// <summary>
	/// Scope that shift position of the rendering cells inside
	/// </summary>
	public ShiftCellsScope (Int2 shift) {
		Shift = shift;
	}

	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		for (int i = UsedCount; i < count; i++) {
			var cell = cells[i];
			cell.X += Shift.x;
			cell.Y += Shift.y;
		}
	}
}