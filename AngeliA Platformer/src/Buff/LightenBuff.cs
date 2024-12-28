using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Platformer;

public class LightenBuff : Buff {

	public static readonly int TYPE_ID = typeof(LightenBuff).AngeHash();

	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Bullet.OnBulletDealDamage += OnBulletDealDamage;
		static void OnBulletDealDamage (Bullet bullet, IDamageReceiver receiver, Tag damageType) {
			if (!damageType.HasAll(Tag.LightenDamage)) return;
			if (receiver is IWithCharacterBuff wBuff) {
				wBuff.CurrentBuff.GiveBuff(TYPE_ID, 60);
			}
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
		if (!Renderer.TryGetSpriteForGizmos(TypeID, out var sprite)) return;
		for (int i = 0; i < 2; i++) {
			var rect = target.Rect;
			int size = Util.QuickRandom(169, 196);
			int x = rect.CenterX();
			int y = rect.CenterY();
			int rangeX = rect.width / 2;
			int rangeY = rect.height / 2;
			Renderer.Draw(
				sprite,
				Util.QuickRandom(x - rangeX, x + rangeX),
				Util.QuickRandom(y - rangeY, y + rangeY),
				500, 500,
				Util.QuickRandom(-150, 150),
				Util.QuickRandomSign() * size, size,
				z: int.MaxValue
			);
		}
	}

}
