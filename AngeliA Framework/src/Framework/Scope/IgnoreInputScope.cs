namespace AngeliA;

public readonly struct IgnoreInputScope : System.IDisposable {
	private readonly bool OldIgnoreKey;
	private readonly bool OldIgnoreMouse;
	public IgnoreInputScope (bool ignoreKey = true, bool ignoreMouse = true) {
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
