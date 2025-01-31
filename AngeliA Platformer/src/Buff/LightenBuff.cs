using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Platformer;

public class LightenBuff : Buff {

	public static readonly int TYPE_ID = typeof(LightenBuff).AngeHash();

	private static readonly SpriteCode LightenSp = "Lighten";
	public override int DefaultDuration => 120;

	[OnDealDamage_Damage_IDamageReceiver]
	internal static void OnDealDamage (Damage damage, IDamageReceiver receiver) {
		if (!damage.Type.HasAll(Tag.LightenDamage)) return;
		if (receiver is IWithCharacterBuff wBuff && !wBuff.CurrentBuff.HasBuff(TYPE_ID)) {
			wBuff.CurrentBuff.GiveBuff(TYPE_ID);
		}
	}

	public override void BeforeUpdate (Character target) {
		base.BeforeUpdate(target);
		if (Util.QuickRandom(0, 10) != 0) return;
		target.Movement.RunSpeed.Multiply(-1000, 1);
		target.Movement.WalkSpeed.Multiply(-1000, 1);
		target.Movement.SwimSpeed.Multiply(-1000, 1);
		target.Movement.SquatMoveSpeed.Multiply(-1000, 1);
		target.Movement.GrabMoveSpeedX.Multiply(-1000, 1);
		target.Movement.FlyMoveSpeed.Multiply(-1000, 1);
		target.Movement.RushSpeed.Multiply(-1000, 1);
		target.Movement.DashSpeed.Multiply(-1000, 1);
	}

	public override void LateUpdate (Character target) {
		base.LateUpdate(target);
		FrameworkUtil.DrawLightenEffect(LightenSp, target.Rect.EdgeInsideDown(Const.CEL * 2));
	}

}
