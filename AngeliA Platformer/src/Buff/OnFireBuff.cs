using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class OnFireBuff : Buff {

	private const int DAMAGE_FREQUENCY = 40;
	public static readonly int TYPE_ID = typeof(OnFireBuff).AngeHash();
	private static readonly SpriteCode FireSprite = "Fire";

	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Bullet.OnBulletDealDamage += OnBulletDealDamage;
		Bullet.OnBulletHitEnvironment += OnBulletHitEnvironment;
		static void OnBulletDealDamage (Bullet bullet, IDamageReceiver receiver, Tag damageType) {
			if (!damageType.HasAll(Tag.FireDamage)) return;
			IFire.SpreadFire(Fire.TYPE_ID, bullet.Rect.Expand(Const.CEL));
			if (receiver is IWithCharacterBuff wBuff) {
				wBuff.CurrentBuff.GiveBuff(OnFireBuff.TYPE_ID, 200);
			}
		}
		static void OnBulletHitEnvironment (Bullet bullet, Tag damageType) {
			if (!damageType.HasAll(Tag.FireDamage)) return;
			IFire.SpreadFire(Fire.TYPE_ID, bullet.Rect.Expand(Const.CEL));
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
			target.TakeDamage(new Damage(1, null, null, Tag.FireDamage) {
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
