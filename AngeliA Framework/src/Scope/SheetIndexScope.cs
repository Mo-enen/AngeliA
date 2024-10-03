namespace AngeliA;

public readonly struct SheetIndexScope : System.IDisposable {
	private readonly int OldSheet;
	public SheetIndexScope (int sheet) {
		OldSheet = Renderer.CurrentSheetIndex;
		Renderer.CurrentSheetIndex = sheet;
	}
	public readonly void Dispose () => Renderer.CurrentSheetIndex = OldSheet;
}
