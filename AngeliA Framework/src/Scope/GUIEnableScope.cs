namespace AngeliA;

public readonly struct GUIEnableScope : System.IDisposable {
	private readonly bool OldEnable;
	public GUIEnableScope (bool enable) {
		OldEnable = GUI.Enable;
		GUI.Enable = enable;
	}
	public readonly void Dispose () => GUI.Enable = OldEnable;
}
