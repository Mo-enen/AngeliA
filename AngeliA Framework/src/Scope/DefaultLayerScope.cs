namespace AngeliA;

/// <summary>
/// Scope that makes renderer draw into default layer
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
/// 		using (new DefaultLayerScope()) {
/// 
/// 			// Rendering cell created inside will draw into default layer
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public readonly struct DefaultLayerScope : System.IDisposable {
	private readonly int OldLayer;
	/// <summary>
	/// Scope that makes renderer draw into default layer
	/// </summary>
	public DefaultLayerScope () {
		OldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(RenderLayer.DEFAULT);
	}
	public readonly void Dispose () => Renderer.SetLayer(OldLayer);
}
