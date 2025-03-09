using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDespawnOutOfRange]
public class AppearSmokeParticle : Particle {

	public static readonly int TYPE_ID = typeof(AppearSmokeParticle).AngeHash();
	public override int Duration => 24;
	public override bool Loop => false;
	public override int RenderingZ => _RenderingZ;
	public int _RenderingZ { get; set; } = int.MaxValue - 1;


	[OnCharacterTeleport_Entity]
	internal static void OnTeleport (Entity target) {
		var rect = target.Rect;
		if (
			Stage.TrySpawnEntity(TYPE_ID, rect.CenterX(), rect.CenterY(), out var entity) &&
			entity is AppearSmokeParticle particle
		) {
			particle.Scale = 2000;
			particle.SpawnFrame += 3;
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		_RenderingZ = int.MaxValue - 1;
		Scale = 1000;
	}


	public override void LateUpdate () {
		using var _ = new LayerScope(RenderingLayer);
		base.LateUpdate();
	}


	// API
	public static void SpawnSmoke (int centerX, int centerY, Color32 tintA, Color32 tintB, int scale = 1000, int count = 2) {
		int z = int.MaxValue - 1;
		for (int i = 0; i < count; i++) {
			if (Stage.SpawnEntity(TYPE_ID, centerX, centerY) is not AppearSmokeParticle particle) break;
			particle.Tint = i % 2 == 0 ? tintA : tintB;
			particle.X += Util.QuickRandom(-Const.HALF / 2, Const.HALF / 2);
			particle.Y += Util.QuickRandom(-Const.HALF / 2, Const.HALF / 2);
			particle.Rotation = Util.QuickRandom(0, 360);
			particle.Scale = scale * Util.QuickRandom(700, 1100) / 1000;
			particle._RenderingZ = z;
		}
	}


}
