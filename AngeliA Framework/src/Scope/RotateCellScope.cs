namespace AngeliA;
public readonly struct RotateCellScope (int rotation, int pointX = int.MinValue, int piontY = int.MinValue) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int Rotation = rotation;
	private readonly int PointX = pointX;
	private readonly int PointY = piontY;
	public RotateCellScope () : this(0, int.MinValue, int.MinValue) { }
	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		for (int i = UsedCount; i < count; i++) {
			if (PointX == int.MinValue || PointY == int.MinValue) {
				cells[i].Rotation = Rotation;
			} else {
				cells[i].RotateAround(Rotation, PointX, PointY);
			}
		}
	}
}
