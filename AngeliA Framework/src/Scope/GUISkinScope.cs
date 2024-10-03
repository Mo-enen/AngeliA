namespace AngeliA;

public readonly struct GUISkinScope : System.IDisposable {
	private readonly GUISkin OldSkin;
	public GUISkinScope (GUISkin skin) {
		OldSkin = GUI.Skin;
		GUI.Skin = skin;
	}
	public readonly void Dispose () => GUI.Skin = OldSkin;
}
