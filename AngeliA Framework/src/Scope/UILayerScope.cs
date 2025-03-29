namespace AngeliA;

/// <summary>
/// Scope that make rendering cells inside into UI layer
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
/// 		using (new UILayerScope()) {
/// 
/// 			// Rendering cells inside will draw into UI layer
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct UILayerScope : System.IDisposable {
	private readonly int OldLayer;
	private readonly bool IgnoreSorting;
	public UILayerScope () : this(false) { }
	public UILayerScope (bool ignoreSorting) {
		OldLayer = Renderer.CurrentLayerIndex;
		IgnoreSorting = ignoreSorting;
		Renderer.SetLayer(RenderLayer.UI);
	}
	public readonly void Dispose () {
		if (!IgnoreSorting) {
			Renderer.ReverseUnsortedCells(RenderLayer.UI);
		}
		Renderer.SetLayer(OldLayer);
	}
}
