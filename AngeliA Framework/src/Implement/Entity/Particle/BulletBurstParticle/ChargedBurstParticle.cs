using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class ChargedBurstParticle : Particle {

	public static readonly int TYPE_ID = typeof(ChargedBurstParticle).AngeHash();
	public override int Duration => 5;
	public override bool Loop => false;
	public override int RenderingZ => int.MaxValue - 1;
	public override int Scale => _Scale;
	public int _Scale { get; set; } = 1000;
	public int BasicScale { get; set; } = 1000;

	public override void LateUpdate () {
		using var _ = new LayerScope(RenderLayer.ADD);
		Tint = Tint.WithNewA(Util.QuickRandom(Game.GlobalFrame * 2187436, 128, 255));
		Rotation = Util.QuickRandom(Game.GlobalFrame * 163).UMod(360);
		_Scale = Util.QuickRandom(Game.GlobalFrame * 4116, BasicScale, BasicScale + 500);
		base.LateUpdate();
	}

	public static void SpawnBurst (int x, int y, Color32 tint, int basicScale = 1000) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is not ChargedBurstParticle particle) return;
		particle.Tint = tint;
		particle.BasicScale = basicScale;
	}

}
