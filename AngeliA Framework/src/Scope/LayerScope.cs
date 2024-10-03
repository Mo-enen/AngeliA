namespace AngeliA;

public readonly struct LayerScope : System.IDisposable {
	private readonly int OldLayer;
	public LayerScope (int layer) {
		OldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(layer);
	}
	public readonly void Dispose () {
		if (Renderer.CurrentLayerIndex == RenderLayer.UI) {
			Renderer.ReverseUnsortedCells(RenderLayer.UI);
		}
		Renderer.SetLayer(OldLayer);
	}
}
