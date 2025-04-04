using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class QuickSmokeSmallParticle : Particle {

	public static readonly int TYPE_ID = typeof(QuickSmokeSmallParticle).AngeHash();
	public override int Duration => 10;
	public override bool Loop => false;
	private int RotationOffset = 0;

	public override void LateUpdate () {
		if (!Active) return;
		int smokeDuration = Duration - 4;
		int smokeFrame = (Game.GlobalFrame - SpawnFrame - 4).GreaterOrEquelThanZero();
		int _smokeFrame = smokeDuration * smokeDuration - (smokeDuration - smokeFrame) * (smokeDuration - smokeFrame);
		var tint = Tint;
		tint.a = (byte)(Util.Remap(0, smokeDuration, 512, 0, smokeFrame) * Tint.a / 255);
		var cell = Renderer.Draw(
			TypeID, X, Y,
			500, 500,
			(smokeFrame + Game.GlobalFrame) * 12 + RotationOffset,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			tint, int.MaxValue - 1
		);
		cell.X += (Width > 0 ? _smokeFrame : -_smokeFrame) * 6;
		cell.Y += cell.Height / 2;
		cell.Width = Util.Remap(0, smokeDuration, cell.Width, cell.Width * 2, smokeFrame) * Scale / 1000;
		cell.Height = Util.Remap(0, smokeDuration, cell.Height, cell.Height * 2, smokeFrame) * Scale / 1000;
	}

	public static QuickSmokeSmallParticle Spawn (int x, int y, bool facingRight) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is QuickSmokeSmallParticle particle) {
			particle.Width = facingRight ? Const.CEL : -Const.CEL;
			particle.RotationOffset = Util.QuickRandom(0, 360);
			return particle;
		} else {
			return null;
		}
	}

}

public class QuickSmokeBigParticle : Particle {

	public static readonly int TYPE_ID = typeof(QuickSmokeBigParticle).AngeHash();

	public override int Duration => _Duration;
	public override bool Loop => false;

	private int _Duration = 10;
	private int _SpeedX = 6;
	private int _Scale = 1000;

	public override void OnActivated () {
		base.OnActivated();
		_Duration = 10;
		_SpeedX = 6;
		_Scale = 1000;
	}

	public override void LateUpdate () {
		if (!Active) return;
		int smokeDuration = Duration - 4;
		int smokeFrame = (Game.GlobalFrame - SpawnFrame - 4).GreaterOrEquelThanZero();
		int _smokeFrame = smokeDuration * smokeDuration - (smokeDuration - smokeFrame) * (smokeDuration - smokeFrame);
		var tint = Tint;
		tint.a = (byte)Util.Remap(0, smokeDuration, 512, 0, smokeFrame);
		var cell = Renderer.Draw(
			TypeID, X, Y,
			500, 500, (smokeFrame + Game.GlobalFrame) * 12,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			tint, int.MaxValue - 1
		);
		cell.X += (Width > 0 ? _smokeFrame : -_smokeFrame) * _SpeedX;
		cell.Y += cell.Height / 2;
		cell.Width = Util.Remap(0, smokeDuration, cell.Width, cell.Width * 2, smokeFrame) * _Scale / 1000;
		cell.Height = Util.Remap(0, smokeDuration, cell.Height, cell.Height * 2, smokeFrame) * _Scale / 1000;
	}

	public void SetParam (int duration = 10, int speedX = 6, int scale = 1000) {
		_Duration = duration;
		_SpeedX = speedX;
		_Scale = scale;
	}

	public static QuickSmokeBigParticle Spawn (int x, int y, bool facingRight) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is QuickSmokeBigParticle particle) {
			particle.Width = facingRight ? Const.CEL : -Const.CEL;
			return particle;
		} else {
			return null;
		}
	}

}
