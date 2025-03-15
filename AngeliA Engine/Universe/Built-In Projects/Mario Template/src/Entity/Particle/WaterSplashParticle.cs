using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(16)]
public class WaterSplashParticle : Particle {

	private static readonly int TYPE_ID = typeof(WaterSplashParticle).AngeHash();
	public override int Duration => 20;
	public override bool Loop => false;

	[OnFallIntoWater_Rigidbody_Entity]
	[OnCameOutOfWater_Rigidbody_Entity]
	internal static void SpawnParticleForRigidbody (Rigidbody rig, Entity water) {
		if (water != null && water is not Liquid) return;
		var rect = rig.Rect;
		if (Stage.SpawnEntity(
			TYPE_ID,
			rect.CenterX(), rect.CenterY() + (rig.InWater ? 0 : -rig.VelocityY)
		) is not WaterSplashParticle particle) return;
		particle.Tint = Color32.WHITE.WithNewA(rig.VelocityY.Abs() * 10);
		particle.Scale = (rig.VelocityY.Abs() * 1000 / 32).Clamp(100, 1500);
	}

}