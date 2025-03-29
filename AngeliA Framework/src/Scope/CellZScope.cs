namespace AngeliA;

/// <summary>
/// Scope to change rendering cell z value
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
/// 		int newZ = 0;
/// 
/// 		using (new CellZScope(newZ)) {
/// 
/// 			// Rendering cell created inside will have their z value override
/// 			
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public readonly struct CellZScope (int z) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int Z = z;
	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		for (int i = UsedCount; i < count; i++) {
			cells[i].Z = Z;
		}
	}
}
