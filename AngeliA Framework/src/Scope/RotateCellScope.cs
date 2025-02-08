namespace AngeliA;

public readonly struct RotateCellScope (int rotation, int pointX = int.MinValue, int piontY = int.MinValue, bool keepOriginalRotation = false) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int Rotation = rotation;
	private readonly int PointX = pointX;
	private readonly int PointY = piontY;
	private readonly bool KeepRotation = keepOriginalRotation;
	public RotateCellScope () : this(0, int.MinValue, int.MinValue, false) { }
	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		if (PointX != int.MinValue && PointY != int.MinValue && Rotation == 0) return;
		for (int i = UsedCount; i < count; i++) {
			if (PointX == int.MinValue || PointY == int.MinValue) {
				cells[i].Rotation = Rotation;
			} else {
				var cell = cells[i];
				int oldRot = cell.Rotation1000;
				float oldPivotX = cell.PivotX;
				float oldPivotY = cell.PivotY;
				cell.RotateAround(Rotation, PointX, PointY);
				if (KeepRotation) {
					cell.ReturnPivots(oldPivotX, oldPivotY);
					cell.Rotation1000 = oldRot;
				}
			}
		}
	}
}
