namespace AngeliA;

public readonly struct FontScope : System.IDisposable {
	private readonly int OldFontIndex;
	public FontScope (int fontID) {
		OldFontIndex = Renderer.CurrentFontIndex;
		Renderer.SetFontID(fontID);
	}
	public readonly void Dispose () {
		Renderer.SetFontIndex(OldFontIndex);
	}
}
