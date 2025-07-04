namespace AngeliA;

/// <summary>
/// Scope that make rendering cells inside use given render layer
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
/// 		using (new LayerScope(RenderLayer.ADD)) {
/// 
/// 			// Rendering cells inside will be draw into additive layer
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct LayerScope : System.IDisposable {
	private readonly int OldLayer;
	/// <summary>
	/// Scope that make rendering cells inside use given render layer
	/// </summary>
	/// <param name="layer">Use RenderLayer.XXX to get this index</param>
	public LayerScope (int layer) {
		OldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(layer);
	}
	public readonly void Dispose () {
		if (Renderer.CurrentLayerIndex == RenderLayer.UI) {
			Renderer.ReverseUnsortedCells(RenderLayer.UI);
		}
		Renderer.SetLayer(OldLayer);
	}
}
