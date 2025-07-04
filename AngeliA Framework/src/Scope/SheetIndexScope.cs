namespace AngeliA;

/// <summary>
/// Scope that change sheet index of the rendering cells inside
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
/// 		using (new SheetIndexScope(/*Your Sheet Index*/)) {
/// 
/// 			// Rendering cells inside will render with given sheet
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct SheetIndexScope : System.IDisposable {
	private readonly int OldSheet;

	/// <summary>
	/// Scope that change sheet index of the rendering cells inside
	/// </summary>
	public SheetIndexScope () : this(-1) { }

	/// <summary>
	/// Scope that change sheet index of the rendering cells inside
	/// </summary>
	public SheetIndexScope (int sheet) {
		OldSheet = Renderer.CurrentSheetIndex;
		Renderer.CurrentSheetIndex = sheet;
	}

	public readonly void Dispose () => Renderer.CurrentSheetIndex = OldSheet;
}
