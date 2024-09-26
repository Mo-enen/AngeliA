namespace AngeliA;

public readonly struct ClampCellsScope : System.IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	private readonly IRect Rect;
	public ClampCellsScope (IRect rect) {
		Rect = rect;
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
	}
	public readonly void Dispose () {
		Renderer.ClampCells(LayerIndex, Rect, UsedCount);
	}
}
