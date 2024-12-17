using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class PoisonBuff : Buff {

	public static readonly int TYPE_ID = typeof(PoisonBuff).AngeHash();
	private const int DAMAGE_FREQUENCY = 300;

	public override void BeforeUpdate (Character target) {
		// Take Damage
		int endFrame = target.Buff.GetBuffEndFrame(TypeID);
		if ((endFrame - Game.GlobalFrame) % DAMAGE_FREQUENCY == DAMAGE_FREQUENCY / 2) {
			target.TakeDamage(new Damage(1, null, null, Tag.PoisonDamage) {
				IgnoreInvincible = true,
				IgnoreStun = true,
			});
		}
	}

	public override void LateUpdate (Character target) {
		base.LateUpdate(target);
		// Green Tint
		target.Rendering.Tint.Tint(new Color32(220, 255, 220), 1);
		// Draw Effect
		int endFrame = target.Buff.GetBuffEndFrame(TypeID);
		DrawPoisonEffect(target.Rect.Expand(32, 32, -32, 32), endFrame);
	}

	private void DrawPoisonEffect (IRect range, int endFrame) {
		if (!Renderer.TryGetSprite(TypeID, out var sprite)) return;
		var rect = range;
		int left = rect.x;
		int down = rect.y;
		int width = rect.width;
		int height = rect.height;
		int seed = endFrame * 347345634;
		int frame = endFrame - Game.GlobalFrame;
		var tint = new Color32(200, 225, 255, (byte)(frame * 10).Clamp(0, 255));
		float frame01 = frame / 120f;
		float fixedFrame01 = frame01 * Const.CEL / height;
		const int COUNT = 4;
		for (int i = 0; i < COUNT; i++) {
			float lerp01 = i / (float)COUNT;
			int x = left + (Util.QuickRandomWithSeed(seed + i * 21632, 0, width)).UMod(width);
			int y = down - (((fixedFrame01 + lerp01) * height).RoundToInt()).UMod(height) + height;
			Renderer.Draw(sprite, x, y, 500, 500, 0, 132, 132, tint, int.MaxValue);
		}

	}

}
