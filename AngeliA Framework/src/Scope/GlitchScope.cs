namespace AngeliA;

public readonly struct GlitchScope : System.IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	private readonly int SpeedAmount;
	private readonly int ShiftAmount;
	private readonly int ScaleAmount;
	public GlitchScope (int speedAmount = 1000, int shiftAmount = 1000, int scaleAmount = 1000) {
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
		SpeedAmount = speedAmount;
		ShiftAmount = shiftAmount;
		ScaleAmount = scaleAmount;
	}
	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			for (int i = UsedCount; i < count; i++) {
				FrameworkUtil.DrawGlitchEffect(cells[i], Game.GlobalFrame, SpeedAmount, ShiftAmount, ScaleAmount);
			}
		}
	}
}
