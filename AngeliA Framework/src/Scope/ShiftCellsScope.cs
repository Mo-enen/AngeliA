namespace AngeliA;



public readonly struct ShiftCellsScope : System.IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	private readonly Int2 Shift;
	public ShiftCellsScope (Int2 shift) {
		Shift = shift;
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
	}
	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			for (int i = UsedCount; i < count; i++) {
				var cell = cells[i];
				cell.X += Shift.x;
				cell.Y += Shift.y;
			}
		}
	}
}