namespace AngeliA;


public readonly struct EnvironmentShadowScope (int offsetX = -64, int offsetY = 0, byte alpha = 64, int z = -65520) : System.IDisposable {
	private readonly int StartIndex = Renderer.GetUsedCellCount();
	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	public readonly int OffsetX = offsetX;
	public readonly int OffsetY = offsetY;
	public readonly byte Alpha = alpha;
	public readonly int Z = z;
	public EnvironmentShadowScope () : this(-64, 0, 64, -65520) { }
	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			for (int i = StartIndex; i < count; i++) {
				FrameworkUtil.DrawEnvironmentShadow(cells[i], OffsetX, OffsetY, Alpha, Z);
			}
		}
	}
}
