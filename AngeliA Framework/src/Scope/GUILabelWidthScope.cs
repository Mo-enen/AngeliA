namespace AngeliA;

public readonly struct GUILabelWidthScope : System.IDisposable {
	private readonly int OldWidth;
	public GUILabelWidthScope (int width) {
		OldWidth = GUI.LabelWidth;
		GUI.LabelWidth = width;
	}
	public readonly void Dispose () => GUI.LabelWidth = OldWidth;
}
