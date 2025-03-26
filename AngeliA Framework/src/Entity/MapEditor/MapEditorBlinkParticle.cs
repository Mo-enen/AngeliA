namespace AngeliA;

[EntityAttribute.Capacity(4, 0)]
internal class MapEditorBlinkParticle : Particle {
	public static readonly int TYPE_ID = typeof(MapEditorBlinkParticle).AngeHash();
	public override int Duration => 8;
	public override bool Loop => false;
	public int SpriteID { get; set; } = Const.PIXEL;
	public override void LateUpdate () {
		if (!Active) return;
		Renderer.SetLayerToAdditive();
		var tint = Tint;
		tint.a = (byte)((Duration - LocalFrame) * Tint.a / 2 / Duration).Clamp(0, 255);
		Renderer.DrawSlice(SpriteID, Rect, tint, int.MaxValue);
		Renderer.SetLayerToDefault();
	}
}