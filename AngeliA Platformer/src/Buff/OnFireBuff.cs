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
			IFire.SpreadFire(TYPE_ID, bullet.Rect.Expand(Const.CEL));
			if (receiver is IWithCharacterBuff wBuff) {
				wBuff.CurrentBuff.GiveBuff(TYPE_ID, 200);
			}
		}
		static void OnBulletHitEnvironment (Bullet bullet, Tag damageType) {
			if (!damageType.HasAll(Tag.FireDamage)) return;
			IFire.SpreadFire(TYPE_ID, bullet.Rect.Expand(Const.CEL));
		}
	}

	public override void BeforeUpdate (Character target) {
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
		int endFrame = target.Buff.GetBuffEndFrame(TypeID);
		DrawFireEffect(target.Rect.Expand(32, 32, -32, 32), endFrame);
	}

	private void DrawFireEffect (IRect range, int endFrame) {
		if (!Renderer.TryGetSprite(FireSprite, out var sprite, ignoreAnimation: false)) return;
		var rect = range;
		int left = rect.x;
		int down = rect.y;
		int width = rect.width;
		int height = rect.height;
		int seed = endFrame * 347345634;
		int frame = endFrame - Game.GlobalFrame;
		var tint = new Color32(200, 225, 255, (byte)(frame * 10).Clamp(0, 255));
		float frame01 = frame / 40f;
		float fixedFrame01 = frame01 * Const.CEL / height;
		const int COUNT = 2;
		for (int i = 0; i < COUNT; i++) {
			float lerp01 = i / (float)COUNT;
			int x = left + (Util.QuickRandomWithSeed(seed + i * 21632, 0, width)).UMod(width);
			int y = down - (((fixedFrame01 + lerp01) * height).RoundToInt()).UMod(height) + height;
			Renderer.Draw(sprite, x, y, 500, 500, 0, 96, 96, tint, int.MaxValue);
		}
	}

}
