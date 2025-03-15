using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

public class PowExpodeParticle : Particle {


	// VAR
	public static readonly int TYPE_ID = typeof(PowExpodeParticle).AngeHash();
	public override int Duration => 24;
	public override bool Loop => false;

	// MSG
	public override void LateUpdate () {
		base.LateUpdate();

		// Pow Block
		int powSize = Util.QuickRandom(256, 512);
		int powRot = Util.QuickRandom(-20, 20);
		Renderer.Draw(PowBlock.TYPE_ID, X, Y, 500, 500, powRot, powSize, powSize);

		// Wave
		int waveSize = LocalFrame * Const.CEL * 50 / Duration;
		Renderer.Draw(
			BuiltInSprite.RING_32,
			X, Y, 500, 500, 0, waveSize, waveSize,
			Color32.WHITE.WithNewA((Duration - LocalFrame) * 128 / 24),
			z: int.MaxValue
		);
		waveSize = waveSize * 3 / 2 + Util.QuickRandom(-512, 512);
		Renderer.Draw(
			BuiltInSprite.RING_32,
			X, Y, 500, 500, 0, waveSize, waveSize,
			Color32.WHITE.WithNewA((Duration - LocalFrame) * 64 / 24),
			z: int.MaxValue
		);
	}

	public static void Spawn (int x, int y) {
		Stage.SpawnEntity(TYPE_ID, x, y);
	}


}
