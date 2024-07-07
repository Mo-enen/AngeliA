using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class AppearSmokeParticle : Particle {

	public static readonly int TYPE_ID = typeof(AppearSmokeParticle).AngeHash();
	public override int Duration => 24;
	public override bool Loop => false;
	public override int RenderingZ => _RenderingZ;
	public override int Scale => _Scale;
	public int _RenderingZ { get; set; } = int.MaxValue - 1;
	public int RenderingLayer { get; set; } = RenderLayer.DEFAULT;
	public int _Scale { get; set; } = 1000;


	[OnGameInitializeLater(64)]
	public static void OnGameInitialize () {
		Character.OnTeleport += OnTeleport;
		static void OnTeleport (Character character) {
			if (Stage.TrySpawnEntity(TYPE_ID, character.X, character.Y + character.Height / 2, out var entity) && entity is AppearSmokeParticle particle) {
				particle._Scale = 2000;
				particle.SpawnFrame += 3;
			}
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		_RenderingZ = int.MaxValue - 1;
		_Scale = 1000;
		RenderingLayer = RenderLayer.DEFAULT;
	}


	public override void LateUpdate () {
		using var _ = new LayerScope(RenderingLayer);
		base.LateUpdate();
	}


}
