namespace AngeliA;

public readonly struct GUIBodyColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIBodyColorScope () : this(Color32.WHITE) { }
	public GUIBodyColorScope (Color32 color) {
		OldColor = GUI.BodyColor;
		GUI.BodyColor = color;
	}
	public readonly void Dispose () => GUI.BodyColor = OldColor;
}
