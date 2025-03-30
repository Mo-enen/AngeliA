namespace AngeliA;

/// <summary>
/// Scope that makes labels inside display with given font
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
/// 		int fontID = /*Your font ID here*/;
/// 
/// 		using (new FontScope(fontID)) {
/// 
/// 			// Labels inside will be display with the given font
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public readonly struct FontScope : System.IDisposable {
	private readonly int OldFontIndex;
	/// <summary>
	/// Scope that makes labels inside display with given font
	/// </summary>
	public FontScope (int fontID) {
		OldFontIndex = Renderer.CurrentFontIndex;
		Renderer.SetFontID(fontID);
	}
	public readonly void Dispose () {
		Renderer.SetFontIndex(OldFontIndex);
	}
}
