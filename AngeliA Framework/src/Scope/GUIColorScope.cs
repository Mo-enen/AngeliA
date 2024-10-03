namespace AngeliA;

public readonly struct GUIColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIColorScope (Color32 color) {
		OldColor = GUI.Color;
		GUI.Color = color;
	}
	public readonly void Dispose () => GUI.Color = OldColor;
}
