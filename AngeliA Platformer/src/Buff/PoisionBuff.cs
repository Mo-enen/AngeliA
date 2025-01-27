using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class PoisonBuff : Buff {


	private static readonly SpriteCode PoisonSprite = "Poison";

	public static readonly int TYPE_ID = typeof(PoisonBuff).AngeHash();
	private const int DAMAGE_FREQUENCY = 300;


	[OnDealDamage_Damage_IDamageReceiver]
	internal static void OnDealDamage (Damage damage, IDamageReceiver receiver) {
		if (!damage.Type.HasAll(Tag.PoisonDamage)) return;
		if (receiver is IWithCharacterBuff wBuff && !wBuff.CurrentBuff.HasBuff(TYPE_ID)) {
			wBuff.CurrentBuff.GiveBuff(TYPE_ID, 900);
		}
	}


	public override void BeforeUpdate (Character target) {
		// Take Damage
		int endFrame = target.Buff.GetBuffEndFrame(TypeID);
		if ((endFrame - Game.GlobalFrame) % DAMAGE_FREQUENCY == DAMAGE_FREQUENCY / 2) {
			(target as IDamageReceiver).TakeDamage(new Damage(1, type: Tag.PoisonDamage) {
				IgnoreInvincible = true,
				IgnoreStun = true,
			});
		}
	}


	public override void LateUpdate (Character target) {
		base.LateUpdate(target);
		// Green Tint
		target.Rendering.Tint.Tint(new Color32(170, 255, 170), 1);
		// Draw Effect
		FrameworkUtil.DrawPoisonEffect(PoisonSprite, target.Rect.Expand(32, 32, -32, 0).EdgeInsideDown(Const.CEL * 2), seed: target.TypeID);
	}


}
