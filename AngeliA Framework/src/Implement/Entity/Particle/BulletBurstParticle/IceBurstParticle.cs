using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class IceBurstParticle : Particle {

	public static readonly int TYPE_ID = typeof(IceBurstParticle).AngeHash();
	public override int Duration => 20;
	public override bool Loop => false;
	public override int RenderingZ => int.MaxValue - 1;
	public override int Scale => _Scale;
	public int _Scale { get; set; } = 1000;
	public int FallSpeedStart { get; set; } = 0;
	public int FallSpeed { get; set; } = Const.CEL / 12;
	public int ShiftSpeedStart { get; set; } = 0;
	public int ShiftSpeed { get; set; } = 0;

	public override void Update () {
		base.Update();
		float lerp01 = (float)LocalFrame / Duration;
		X += Util.Lerp(ShiftSpeedStart, ShiftSpeed, lerp01).RoundToInt();
		Y -= Util.Lerp(FallSpeedStart, FallSpeed, lerp01 * 2f).RoundToInt();
		Tint = Tint.WithNewA(Util.Lerp(255, 0, lerp01).RoundToInt());
	}

	public static void SpawnBurst (int x, int y, Color32 tint, int scale = 1000, int count = 3) {
		for (int i = 0; i < count; i++) {
			if (Stage.SpawnEntity(TYPE_ID, x, y) is not IceBurstParticle particle) continue;
			particle.Tint = tint;
			particle.X += Util.QuickRandom(Game.GlobalFrame + i * 181).UMod(Const.HALF) - Const.HALF / 2;
			particle.Y += Util.QuickRandom(Game.GlobalFrame + i * 832).UMod(Const.HALF) - Const.HALF / 2;
			particle.Rotation = Util.QuickRandom(Game.GlobalFrame + i * 163).UMod(360);
			particle._Scale = scale * (Util.QuickRandom(Game.GlobalFrame + i * 4116).UMod(800) + 300) / 1000;
			particle.FallSpeedStart = Util.QuickRandom(Game.GlobalFrame + i * 8345).UMod(96) - 48;
			particle.FallSpeed = Util.QuickRandom(Game.GlobalFrame + i * 23923454).UMod(42);
			particle.ShiftSpeedStart = Util.QuickRandom(Game.GlobalFrame + i * 89412).UMod(48) - 24;
			particle.ShiftSpeed = 0;
		}
	}

}
