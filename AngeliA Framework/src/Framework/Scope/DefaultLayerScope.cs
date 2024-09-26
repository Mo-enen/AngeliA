namespace AngeliA;

public readonly struct DefaultLayerScope : System.IDisposable {
	private readonly int OldLayer;
	public DefaultLayerScope () {
		OldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(RenderLayer.DEFAULT);
	}
	public readonly void Dispose () => Renderer.SetLayer(OldLayer);
}
