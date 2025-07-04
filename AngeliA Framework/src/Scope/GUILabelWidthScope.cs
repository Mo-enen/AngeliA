namespace AngeliA;

/// <summary>
/// Scope that set internal label width of GUI elements inside
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
/// 		using (new GUILabelWidthScope(128)) {
/// 
/// 			// GUI elements inside will have internal label with 128 in width
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUILabelWidthScope : System.IDisposable {

	private readonly int OldWidth;

	/// <summary>
	/// Scope that set internal label width of GUI elements inside
	/// </summary>
	public GUILabelWidthScope (int width) {
		OldWidth = GUI.LabelWidth;
		GUI.LabelWidth = width;
	}

	public readonly void Dispose () => GUI.LabelWidth = OldWidth;

}
