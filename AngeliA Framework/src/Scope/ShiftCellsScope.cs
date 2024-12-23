namespace AngeliA;
public readonly struct ShiftCellsScope (Int2 shift) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly Int2 Shift = shift;
	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		for (int i = UsedCount; i < count; i++) {
			var cell = cells[i];
			cell.X += Shift.x;
			cell.Y += Shift.y;
		}
	}
}