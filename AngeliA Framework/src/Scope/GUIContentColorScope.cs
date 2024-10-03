namespace AngeliA;

public readonly struct GUIContentColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIContentColorScope (Color32 color) {
		OldColor = GUI.ContentColor;
		GUI.ContentColor = color;
	}
	public readonly void Dispose () => GUI.ContentColor = OldColor;
}
