using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class FireBurstParticle : Particle {

	public static readonly int TYPE_ID = typeof(FireBurstParticle).AngeHash();
	public override int Duration => 6;
	public override bool Loop => false;
	public override int RenderingZ => int.MaxValue - 1;
	public override int Scale => _Scale;
	public int _Scale { get; set; } = 1000;




	public static FireBurstParticle Spawn (int x, int y, Color32 burstTint, int scale = 1000) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is FireBurstParticle particle) {
			particle._Scale = scale;
			particle.Tint = burstTint;
			return particle;
		}
		return null;
	}


}

