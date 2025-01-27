using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class FreezeBuff : Buff {

	public static readonly int TYPE_ID = typeof(FreezeBuff).AngeHash();

	[OnDealDamage_Damage_IDamageReceiver]
	internal static void OnDealDamage (Damage damage, IDamageReceiver receiver) {
		if (!damage.Type.HasAll(Tag.IceDamage)) return;
		if (receiver is IWithCharacterBuff wBuff && !wBuff.CurrentBuff.HasBuff(TYPE_ID)) {
			wBuff.CurrentBuff.GiveBuff(TYPE_ID, 300);
		}
	}

	public override void BeforeUpdate (Character target) {
		base.BeforeUpdate(target);
		var mov = target.Movement;
		mov.WalkSpeed.Multiply(600);
		mov.RunSpeed.Multiply(600);
		mov.SwimSpeed.Multiply(600);
		mov.DashSpeed.Multiply(600);
		mov.RushSpeed.Multiply(600);
		mov.ClimbSpeedX.Multiply(600);
		mov.ClimbSpeedY.Multiply(600);
		mov.FlyFallSpeed.Multiply(1500);
		mov.FlyRiseSpeed.Multiply(600);
	}

	public override void LateUpdate (Character target) {
		base.LateUpdate(target);
		target.Rendering.Tint.Override(new Color32(142, 200, 220, 255), 1);
	}

}
