namespace AngeliA;

/// <summary>
/// Scope that change the content color of GUI element inside 
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
/// 		using (new GUIContentColorScope(Color32.GREEN)) {
/// 
/// 			// GUI elements inside will have their content color changed
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIContentColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIContentColorScope (Color32 color) {
		OldColor = GUI.ContentColor;
		GUI.ContentColor = color;
	}
	public readonly void Dispose () => GUI.ContentColor = OldColor;
}
