namespace AngeliA;

public readonly struct ClampCellsScope (IRect rect) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	public readonly void Dispose () {
		Renderer.ClampCells(LayerIndex, rect, UsedCount);
	}
}
