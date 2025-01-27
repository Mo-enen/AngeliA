using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class OnFireBuff : Buff {

	private const int DAMAGE_FREQUENCY = 40;
	public static readonly int TYPE_ID = typeof(OnFireBuff).AngeHash();
	private static readonly SpriteCode FireSprite = "Fire";

	[OnDealDamage_Damage_IDamageReceiver]
	internal static void OnDealDamage (Damage damage, IDamageReceiver receiver) {
		if (!damage.Type.HasAll(Tag.FireDamage)) return;
		if (receiver is IWithCharacterBuff wBuff && !wBuff.CurrentBuff.HasBuff(TYPE_ID)) {
			wBuff.CurrentBuff.GiveBuff(TYPE_ID, 200);
		}
	}

	public override void BeforeUpdate (Character target) {
		// Putout Check
		if (
			Game.GlobalFrame == target.Movement.LastPoundingFrame + 1 ||
			Game.GlobalFrame == target.Movement.LastSquatFrame + 1 ||
			Game.GlobalFrame == target.Movement.LastDashFrame + 1 ||
			Game.GlobalFrame == target.Movement.LastRushFrame + 1
		) {
			target.Buff.ClearBuff(TypeID);
			return;
		}
		// Take Damage
		int endFrame = target.Buff.GetBuffEndFrame(TypeID);
		if ((endFrame - Game.GlobalFrame) % DAMAGE_FREQUENCY == DAMAGE_FREQUENCY / 2) {
			(target as IDamageReceiver).TakeDamage(new Damage(1, type: Tag.FireDamage) {
				IgnoreInvincible = true,
			});
		}
	}

	public override void LateUpdate (Character target) {
		base.LateUpdate(target);
		// Draw Effect
		FrameworkUtil.DrawOnFireEffect(
			FireSprite,
			target.Rect.Expand(32, 32, -32, 0).EdgeInsideDown(Const.CEL * 2),
			seed: target.TypeID
		);
	}

}
