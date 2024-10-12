namespace AngeliA;

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
