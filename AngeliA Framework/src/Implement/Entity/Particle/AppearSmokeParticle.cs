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


	// API
	public static void SpawnMagicSmoke (int x, int y, int scale = 1000) => SpawnMagicSmoke(x, y, new(246, 196, 255, 255), Color32.WHITE, scale);
	public static void SpawnMagicSmoke (int x, int y, Color32 tintA, Color32 tintB, int scale = 1000) {
		int z = int.MaxValue - 1;
		if (Stage.SpawnEntity(TYPE_ID, x, y) is AppearSmokeParticle particle0) {
			particle0.Tint = tintA;
			particle0.X += Util.QuickRandom(Game.GlobalFrame * 181).UMod(Const.HALF) - Const.HALF / 2;
			particle0.Y += Util.QuickRandom(Game.GlobalFrame * 832).UMod(Const.HALF) - Const.HALF / 2;
			particle0.Rotation = Util.QuickRandom(Game.GlobalFrame * 163).UMod(360);
			particle0._Scale = scale * (Util.QuickRandom(Game.GlobalFrame * 4116).UMod(800) + 300) / 1000;
			particle0._RenderingZ = z;
		}
		if (Stage.SpawnEntity(TYPE_ID, x, y) is AppearSmokeParticle particle1) {
			particle1.Tint = tintB;
			particle1.X += Util.QuickRandom(Game.GlobalFrame * 125).UMod(Const.HALF) - Const.HALF / 2;
			particle1.Y += Util.QuickRandom(Game.GlobalFrame * 67).UMod(Const.HALF) - Const.HALF / 2;
			particle1.Rotation = Util.QuickRandom(Game.GlobalFrame * 127).UMod(360);
			particle1._Scale = scale * (Util.QuickRandom(Game.GlobalFrame * 9).UMod(800) + 300) / 1000;
			particle1._RenderingZ = Util.QuickRandom(Game.GlobalFrame * 12) % 2 == 0 ? z + 1 : z - 1;
		}
	}


}
