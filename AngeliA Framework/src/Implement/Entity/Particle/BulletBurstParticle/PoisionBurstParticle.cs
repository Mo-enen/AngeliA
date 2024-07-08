using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class PoisionBurstParticle : Particle {

	public static readonly int TYPE_ID = typeof(PoisionBurstParticle).AngeHash();
	public override int Duration => 6;
	public override bool Loop => false;
	public override int RenderingZ => int.MaxValue - 1;
	public override int Scale => _Scale;
	public int _Scale { get; set; } = 1000;
	public int RingID { get; set; } = 0;
	public int Radius { get; set; } = Const.CEL * 2;
	public Color32 RingTint { get; set; } = new Color32(81, 166, 58, 128);


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		FrameworkUtil.DrawExplosionRing(
			RingID,
			X, Y, Radius,
			LocalFrame, Duration, RingTint, RenderingZ
		);
	}


	public static PoisionBurstParticle Spawn (int x, int y, int radius, int ringSpriteID, Color32 burstTint, Color32 ringTint, int scale = 1000) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is not PoisionBurstParticle particle) return null;
		particle._Scale = scale;
		particle.Radius = radius;
		particle.Tint = burstTint;
		particle.RingTint = ringTint;
		particle.RingID = ringSpriteID;
		return particle;
	}


}

