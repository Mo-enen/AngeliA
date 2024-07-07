using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class CrystalBurstParticle : Particle {

	public static readonly int TYPE_ID = typeof(CrystalBurstParticle).AngeHash();
	public override int Duration => 20;
	public override bool Loop => false;
	public override int RenderingZ => int.MaxValue - 1;
	public override int Scale => _Scale;
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
		_Scale = 1000;
	}

	
	// API
	public static void SpawnBurst (int x, int y, int scale = 1000) => SpawnBurst(x, y, Color32.WHITE, scale);
	public static void SpawnBurst (int x, int y, Color32 tint, int scale = 1000) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is CrystalBurstParticle particle) {
			particle.Tint = tint;
			particle.X += Util.QuickRandom(Game.GlobalFrame * 181).UMod(Const.HALF) - Const.HALF / 2;
			particle.Y += Util.QuickRandom(Game.GlobalFrame * 832).UMod(Const.HALF) - Const.HALF / 2;
			particle.Rotation = Util.QuickRandom(Game.GlobalFrame * 163).UMod(360);
			particle._Scale = scale * (Util.QuickRandom(Game.GlobalFrame * 4116).UMod(800) + 300) / 1000;
			

		}
	}


}
