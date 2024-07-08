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
	public int FallSpeed { get; set; } = Const.CEL / 12;
	public int FallSpeedStart { get; set; } = 0;
	public int ShiftSpeed { get; set; } = 0;
	public int ShiftSpeedStart { get; set; } = 0;


	public override void OnActivated () {
		base.OnActivated();
		FallSpeed = Const.CEL / 12;
		FallSpeedStart = 0;
	}


	public override void Update () {
		base.Update();
		X += Util.Lerp(ShiftSpeedStart, ShiftSpeed, (LocalFrame * 2f) / Duration).RoundToInt();
		Y -= Util.Lerp(FallSpeedStart, FallSpeed, (LocalFrame * 2f) / Duration).RoundToInt();
		Tint = Tint.WithNewA(Util.Lerp(255, 0, (float)LocalFrame / Duration).RoundToInt());
	}


	// API
	public static void SpawnBurst (int x, int y, Color32 tint, int scale = 1000, int count = 1, int fall = 20, int shift = 0) {
		for (int i = 0; i < count; i++) {
			if (Stage.SpawnEntity(TYPE_ID, x, y) is not CrystalBurstParticle particle) continue;
			particle.Tint = tint;
			particle.Rotation = Util.QuickRandom(Game.GlobalFrame + i * 163).UMod(360);
			particle.FallSpeedStart = 0;
			particle.FallSpeed = fall;
			particle.ShiftSpeedStart = shift;
			particle.ShiftSpeed = 0;
			particle._Scale = scale;
		}
	}


}
