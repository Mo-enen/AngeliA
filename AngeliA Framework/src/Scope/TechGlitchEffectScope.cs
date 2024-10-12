namespace AngeliA;

public readonly struct TechGlitchEffectScope (int range, Color32 tint = default) : System.IDisposable {

	private readonly int LayerIndex = Renderer.CurrentLayerIndex;
	private readonly int UsedCount = Renderer.GetUsedCellCount();
	private readonly int Range = range;
	private readonly Color32 Tint = tint != default ? tint : new Color32(156, 125, 84);

	public TechGlitchEffectScope () : this(5) { }

	public readonly void Dispose () {
		if (!Renderer.GetCells(LayerIndex, out var cells, out int count)) return;
		try {
			float framePingPong01 = Ease.InOutQuad(Game.GlobalFrame.PingPong(120) / 120f) / 4f;
			var tint = Tint.WithNewA((byte)(255 - framePingPong01 * 255).Clamp(0, 255));
			for (int i = UsedCount; i < count; i++) {
				var cell = cells[i];
				cell.X += Util.QuickRandom(-Range, Range + 1);
				cell.Y += Util.QuickRandom(-Range, Range + 1);
				cell.Color *= tint;
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
	}

}
