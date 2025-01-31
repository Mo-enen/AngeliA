using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class HeavyBuff : Buff {

	public static readonly int TYPE_ID = typeof(HeavyBuff).AngeHash();
	public override int DefaultDuration => 120;

	public override void BeforeUpdate (Character character) {
		var mov = character.Movement;
		mov.RunAvailable.Override(false, 1);
		mov.FlyAvailable.Override(false);
		mov.RushAvailable.Override(false);
		mov.DashAvailable.Override(false);
		mov.ClimbAvailable.Override(false);
		mov.GrabSideAvailable.Override(false);
		mov.GrabTopAvailable.Override(false);
		mov.JumpSpeed.Multiply(600);
		character.FallingGravityScale.Multiply(2000);
		character.RisingGravityScale.Multiply(4000);
	}

}
