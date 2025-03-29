namespace AngeliA;

/// <summary>
/// Scope that change the skin of the GUI elements inside
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
/// 		using (new GUISkinScope(/*Your GUI Skin*/)) {
/// 
/// 			// GUI elements inside will use the given GUI skin
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUISkinScope : System.IDisposable {
	private readonly GUISkin OldSkin;
	public GUISkinScope (GUISkin skin) {
		OldSkin = GUI.Skin;
		GUI.Skin = skin;
	}
	public readonly void Dispose () => GUI.Skin = OldSkin;
}
