namespace AngeliA;

/// <summary>
/// Scope that set enable of GUI elements inside
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
/// 		using (new GUIEnableScope(false)) {
/// 
/// 			// GUI elements inside will be disabled
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIEnableScope : System.IDisposable {
	private readonly bool OldEnable;
	public GUIEnableScope (bool enable) {
		OldEnable = GUI.Enable;
		GUI.Enable = enable;
	}
	public readonly void Dispose () => GUI.Enable = OldEnable;
}
