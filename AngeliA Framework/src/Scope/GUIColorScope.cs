namespace AngeliA;

/// <summary>
/// Scope that change the color of GUI element inside 
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
/// 		using (new GUIColorScope(Color32.GREEN)) {
/// 
/// 			// GUI elements inside will have their color changed
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	/// <summary>
	/// Scope that change the color of GUI element inside 
	/// </summary>
	public GUIColorScope (Color32 color) {
		OldColor = GUI.Color;
		GUI.Color = color;
	}
	public readonly void Dispose () => GUI.Color = OldColor;
}
