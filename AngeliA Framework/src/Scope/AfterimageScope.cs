namespace AngeliA;

public readonly struct AfterimageScope (int speedX, int speedY, Color32 tintStart, Color32 tintEnd, int rotateSpeed = 0, int count = 3, int frameStep = 2, int scaleStart = 1000, int scaleEnd = 1000, int renderLayer = RenderLayer.DEFAULT) : System.IDisposable {
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int SpeedX = speedX;
	private readonly int SpeedY = speedY;
	private readonly Color32 TintStart = tintStart;
	private readonly Color32 TintEnd = tintEnd;
	private readonly int RotateSpeed = rotateSpeed;
	private readonly int Count = count;
	private readonly int FrameStep = frameStep;
	private readonly int ScaleStart = scaleStart;
	private readonly int ScaleEnd = scaleEnd;
	private readonly int Layer = renderLayer;
	public AfterimageScope (int speedX, int speedY, int rotateSpeed = 0, int count = 3, int frameStep = 2, int scaleStart = 1000, int scaleEnd = 1000, int renderLayer = RenderLayer.DEFAULT) : this(speedX, speedY, Color32.WHITE, Color32.WHITE, rotateSpeed, count, frameStep, scaleStart, scaleEnd, renderLayer) { }
	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			int oldLayer = Renderer.CurrentLayerIndex;
			Renderer.SetLayer(Layer);
			try {
				for (int i = UsedCount; i < count; i++) {
					FrameworkUtil.DrawAfterimageEffect(
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
}
