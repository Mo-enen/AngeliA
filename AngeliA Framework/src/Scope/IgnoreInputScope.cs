namespace AngeliA;


/// <summary>
/// Scope that make GUI elements ignore keyboard or mouse input from user
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
/// 		using (new IgnoreInputScope()) {
/// 
/// 			// GUI elements inside will ignore keyboard or mouse input from user
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct IgnoreInputScope : System.IDisposable {

	private readonly bool OldIgnoreKey;
	private readonly bool OldIgnoreMouse;

	/// <summary>
	/// Scope that make GUI elements ignore keyboard or mouse input from user
	/// </summary>
	public IgnoreInputScope () : this(true, true) { }

	/// <summary>
	/// Scope that make GUI elements ignore keyboard or mouse input from user
	/// </summary>
	public IgnoreInputScope (bool ignoreKey, bool ignoreMouse) {
		OldIgnoreKey = Input.IgnoringKeyInput;
		OldIgnoreMouse = Input.IgnoringMouseInput;
		if (ignoreKey) {
			Input.IgnoreKeyInput();
		} else {
			Input.CancelIgnoreKeyInput();
		}
		if (ignoreMouse) {
			Input.IgnoreMouseInput();
		} else {
			Input.CancelIgnoreMouseInput();
		}
	}

	public readonly void Dispose () {
		if (OldIgnoreKey) {
			Input.IgnoreKeyInput();
		} else {
			Input.CancelIgnoreKeyInput();
		}
		if (OldIgnoreMouse) {
			Input.IgnoreMouseInput();
		} else {
			Input.CancelIgnoreMouseInput();
		}
	}

}
