namespace AngeliA;

/// <summary>
/// Scope that make rendering cells rotate
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		var cameraRect = Renderer.CameraRect;
/// 
/// 		int rot = QTest.Int("Rot", 0, 0, 360);
/// 		int pointX = QTest.Int("Pivot X", 4096, 0, 4096 * 2);
/// 		int pointY = QTest.Int("Pivot Y", 2048, 0, 4096);
/// 		QTest.Mark(new Int2(cameraRect.x + pointX, cameraRect.y + pointY));
/// 
/// 		using (var scroll = new RotateCellScope(rot, cameraRect.x + pointX, cameraRect.y + pointY)) {
/// 
/// 			Renderer.Draw(
/// 				BuiltInSprite.ICON_ENTITY, 
/// 				cameraRect.CenterX(), cameraRect.CenterY(), 
/// 				500, 500, 0, 512, 512
/// 			);
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct RotateCellScope : System.IDisposable {

	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int Rotation;
	private readonly int PointX;
	private readonly int PointY;
	private readonly bool KeepRotation;

	/// <summary>
	/// Scope that make rendering cells rotate
	/// </summary>
	/// <param name="rotation"></param>
	/// <param name="pointX">Orientation point X in global space</param>
	/// <param name="pointY">Orientation point Y in global space</param>
	/// <param name="keepOriginalRotation">True if only change the position of cells (not rotation)</param>
	public RotateCellScope (int rotation, int pointX = int.MinValue, int pointY = int.MinValue, bool keepOriginalRotation = false) {
		Rotation = rotation;
		PointX = pointX;
		PointY = pointY;
		KeepRotation = keepOriginalRotation;
	}

	/// <summary>
	/// Scope that make rendering cells rotate
	/// </summary>
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
