namespace AngeliA;


/// <summary>
/// Draw a continuous tailing cell effect for the objects rendering inside
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	static int CurrentX = 0;
/// 	static int CurrentY = 0;
/// 	static int CurrentRot = 0;
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		int speedX = QTest.Int("Speed X", 50, -64, 64);
/// 		int speedY = QTest.Int("Speed Y", 10, -64, 64);
/// 		int rotateSpeed = QTest.Int("Rotate Speed", 8, -64, 64);
/// 		int count = QTest.Int("Count", 8, 1, 24);
/// 		int frameStep = QTest.Int("Frame Step", 16, 1, 32);
/// 		int scaleStart = QTest.Int("Scale Start", 1000, 0, 2000);
/// 		int scaleEnd = QTest.Int("Scale End", 100, 0, 2000);
/// 
/// 		using (new AfterimageScope(
/// 			speedX, speedY, Color32.WHITE, Color32.CLEAR, 
/// 			rotateSpeed, count, frameStep, scaleStart, scaleEnd
/// 		)) {
/// 
/// 			var cameraRect = Renderer.CameraRect;
/// 
/// 			// Render object here
/// 			Renderer.Draw(
/// 				BuiltInSprite.ICON_ENTITY,
/// 				cameraRect.x + CurrentX,
/// 				cameraRect.y + CurrentY,
/// 				500, 500, CurrentRot,
/// 				Const.CEL * 2, Const.CEL * 2
/// 			);
/// 
/// 			// Move the object
/// 			CurrentX += speedX;
/// 			CurrentY += speedY;
/// 			CurrentRot += rotateSpeed;
/// 			CurrentX = CurrentX.UMod(cameraRect.width);
/// 			CurrentY = CurrentY.UMod(cameraRect.height);
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public readonly struct AfterimageScope : System.IDisposable {

	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int SpeedX;
	private readonly int SpeedY;
	private readonly Color32 TintStart;
	private readonly Color32 TintEnd;
	private readonly int RotateSpeed;
	private readonly int Count;
	private readonly int FrameStep;
	private readonly int ScaleStart;
	private readonly int ScaleEnd;
	private readonly int Layer;

	/// <summary>
	/// Draw a continuous tailing cell effect for the objects rendering inside
	/// </summary>
	/// <param name="speedX">How fast is the object moves horizontaly</param>
	/// <param name="speedY">How fast is the object moves verticaly</param>
	/// <param name="tintStart">Color tint for start of the effect</param>
	/// <param name="tintEnd">Color tint for end of the effect</param>
	/// <param name="rotateSpeed">How fast does the object rotate</param>
	/// <param name="count">How many effect should be drawn</param>
	/// <param name="frameStep">Frame distance between each effects</param>
	/// <param name="scaleStart">Size scale when the effect start (0 means 0%, 1000 means 100%)</param>
	/// <param name="scaleEnd">Size scale when the effect end (0 means 0%, 1000 means 100%)</param>
	/// <param name="renderLayer">Which layer does this effect renders into. Use RenderLayer.XXX to get the value.</param>
	public AfterimageScope (int speedX, int speedY, Color32 tintStart, Color32 tintEnd, int rotateSpeed = 0, int count = 3, int frameStep = 2, int scaleStart = 1000, int scaleEnd = 1000, int renderLayer = RenderLayer.DEFAULT) {
		SpeedX = speedX;
		SpeedY = speedY;
		TintStart = tintStart;
		TintEnd = tintEnd;
		RotateSpeed = rotateSpeed;
		Count = count;
		FrameStep = frameStep;
		ScaleStart = scaleStart;
		ScaleEnd = scaleEnd;
		Layer = renderLayer;
	}

	/// <inheritdoc cref="AfterimageScope(int, int, Color32, Color32, int, int, int, int, int, int)"/>
	public AfterimageScope (int speedX, int speedY, int rotateSpeed = 0, int count = 3, int frameStep = 2, int scaleStart = 1000, int scaleEnd = 1000, int renderLayer = RenderLayer.DEFAULT) : this(speedX, speedY, Color32.WHITE, Color32.WHITE, rotateSpeed, count, frameStep, scaleStart, scaleEnd, renderLayer) { }

	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			int oldLayer = Renderer.CurrentLayerIndex;
			Renderer.SetLayer(Layer);
			try {
				for (int i = UsedCount; i < count; i++) {
					DrawAfterimageEffect(
						cells[i],
						SpeedX, SpeedY,
						TintStart, TintEnd,
						RotateSpeed, Count, FrameStep,
						ScaleStart, ScaleEnd
					);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			Renderer.SetLayer(oldLayer);
		}
	}

	private static void DrawAfterimageEffect (Cell source, int speedX, int speedY, Color32 tintStart, Color32 tintEnd, int rotateSpeed = 0, int count = 3, int frameStep = 2, int scaleStart = 1000, int scaleEnd = 1000) {
		for (int i = 1; i <= count; i++) {
			int index = i * frameStep;
			float lerp01 = (i - 1f) / (count - 1);
			var cell = Renderer.Draw(Const.PIXEL, default);
			cell.CopyFrom(source);
			cell.X -= index * speedX;
			cell.Y -= index * speedY;
			cell.Z -= index;
			cell.Rotation -= index * rotateSpeed;
			cell.Color *= Color32.Lerp(tintStart, tintEnd, lerp01);
			cell.ScaleFrom(
				Util.LerpUnclamped(scaleStart, scaleEnd, lerp01).RoundToInt(),
				cell.X, cell.Y
			);
		}
	}

}
