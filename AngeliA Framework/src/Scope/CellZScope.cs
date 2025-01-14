namespace AngeliA;

public readonly struct CellZScope (int z) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int Z = z;
	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		for (int i = UsedCount; i < count; i++) {
			cells[i].Z = Z;
		}
	}
}
