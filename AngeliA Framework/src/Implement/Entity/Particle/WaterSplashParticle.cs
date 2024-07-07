using System.Collections;
using System.Collections.Generic;

namespace AngeliA;
[EntityAttribute.Capacity(16)]
public class WaterSplashParticle : Particle {

	private static readonly int TYPE_ID = typeof(WaterSplashParticle).AngeHash();
	public override int Duration => 20;
	public override bool Loop => false;

	[OnGameInitializeLater(64)]
	public static void OnGameInitializeLater () {
		Rigidbody.OnFallIntoWater += SpawnParticleForRigidbody;
		Rigidbody.OnJumpOutOfWater += SpawnParticleForRigidbody;
		static void SpawnParticleForRigidbody (Rigidbody rig) => Stage.SpawnEntity(
			TYPE_ID,
			rig.X + rig.OffsetX + rig.Width / 2,
			rig.Y + rig.OffsetY + rig.Height / 2 + (rig.InWater ? 0 : -rig.VelocityY)
		);
	}

}