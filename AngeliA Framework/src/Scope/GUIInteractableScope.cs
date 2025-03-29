namespace AngeliA;

/// <summary>
/// Scope that set interactable of GUI elements inside
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
/// 		using (new GUIInteractableScope(false)) {
/// 
/// 			// GUI elements inside will be not interactable
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIInteractableScope : System.IDisposable {
	private readonly bool OldInteractable;
	public GUIInteractableScope (bool interactable) {
		OldInteractable = GUI.Interactable;
		GUI.Interactable = interactable;
	}
	public readonly void Dispose () => GUI.Interactable = OldInteractable;
}
