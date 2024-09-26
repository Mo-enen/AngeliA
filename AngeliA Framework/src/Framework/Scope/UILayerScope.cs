namespace AngeliA;

public readonly struct UILayerScope : System.IDisposable {
	private readonly int OldLayer;
	private readonly bool IgnoreSorting;
	public UILayerScope () : this(false) { }
	public UILayerScope (bool ignoreSorting) {
		OldLayer = Renderer.CurrentLayerIndex;
		IgnoreSorting = ignoreSorting;
		Renderer.SetLayer(RenderLayer.UI);
	}
	public readonly void Dispose () {
		if (!IgnoreSorting) {
			Renderer.ReverseUnsortedCells(RenderLayer.UI);
		}
		Renderer.SetLayer(OldLayer);
	}
}
