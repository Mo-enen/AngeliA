using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class FireBurstParticle : Particle {

	public static readonly int TYPE_ID = typeof(FireBurstParticle).AngeHash();
	public override int Duration => 6;
	public override bool Loop => false;
	public override int RenderingZ => _RenderingZ;
	public override int Scale => _Scale;
	public int _RenderingZ { get; set; } = int.MaxValue - 1;
	public int RenderingLayer { get; set; } = RenderLayer.DEFAULT;
	public int _Scale { get; set; } = 1000;
	public int RingID { get; set; } = 0;
	public int Radius { get; set; } = Const.CEL * 2;
	public Color32 RingTint { get; set; } = Color32.WHITE_64;


	public override void OnActivated () {
		base.OnActivated();
		_RenderingZ = int.MaxValue - 1;
		_Scale = 1000;
		RenderingLayer = RenderLayer.DEFAULT;
	}


	public override void LateUpdate () {
		using var _ = new LayerScope(RenderingLayer);
		base.LateUpdate();
		if (!Active) return;
		FrameworkUtil.DrawExplosionRing(
			RingID,
			X, Y, Radius,
			LocalFrame, Duration, RingTint, _RenderingZ
		);
	}


	public static FireBurstParticle Spawn (int x, int y, int radius, int ringSpriteID, Color32 burstTint, Color32 ringTint, int scale = 1000, int layer = RenderLayer.DEFAULT) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is FireBurstParticle particle) {
			particle.RenderingLayer = layer;
			particle._Scale = scale;
			particle.Radius = radius;
			particle.Tint = burstTint;
			particle.RingTint = ringTint;
			particle.RingID = ringSpriteID;
			return particle;
		}
		return null;
	}


}

