namespace AngeliA;

public readonly struct ReverseCellsScope : System.IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	public ReverseCellsScope () {
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
	}
	public readonly void Dispose () {
		int start = UsedCount;
		if (Renderer.GetCells(LayerIndex, out var cells, out int count) && start < count) {
			System.Array.Reverse(cells, start, count - start);
		}
	}
}
