using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class SlowDownBuff : Buff {

	public static readonly int TYPE_ID = typeof(SlowDownBuff).AngeHash();

	public override void BeforeUpdate (Character character) {
		var mov = character.Movement;
		mov.WalkSpeed.Multiply(800);
		mov.RunSpeed.Multiply(800);
		mov.JumpSpeed.Multiply(800);
		mov.SwimSpeed.Multiply(600);
		mov.DashSpeed.Multiply(600);
		mov.RushSpeed.Multiply(800);
		mov.ClimbSpeedX.Multiply(800);
		mov.ClimbSpeedY.Multiply(800);
		mov.SquatMoveSpeed.Multiply(800);
		mov.SlideDropSpeed.Multiply(2000);
		mov.FlyFallSpeed.Multiply(2000);
		mov.FlyRiseSpeed.Multiply(600);
	}

}
