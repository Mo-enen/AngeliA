namespace AngeliA;

/// <summary>
/// Scope that change the body color of GUI element inside 
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
/// 		using (new GUIBodyColorScope(Color32.GREEN)) {
/// 
/// 			// GUI elements inside will have their body color changed
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIBodyColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIBodyColorScope () : this(Color32.WHITE) { }
	public GUIBodyColorScope (Color32 color) {
		OldColor = GUI.BodyColor;
		GUI.BodyColor = color;
	}
	public readonly void Dispose () => GUI.BodyColor = OldColor;
}
