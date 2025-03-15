using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class BigSmokeParticle : Particle {

	public static readonly int TYPE_ID = typeof(BigSmokeParticle).AngeHash();
	public override int Duration => 10;
	public override bool Loop => false;
	public override int RenderingZ => int.MaxValue - 1024;

	[OnCharacterPound_Entity]
	[OnCharacterCrash_Entity]
	internal static void OnPoundCrash (Entity target) {

		var rect = target.Rect;
		if (Stage.SpawnEntity(
			TYPE_ID,
			rect.CenterX(),
			rect.y - Const.HALF / 2
		) is not BigSmokeParticle particle) return;

		particle.Scale = target.Width * 2000 / Const.CEL;
		if (Renderer.TryGetSpriteFromGroup(target is Rigidbody rig ? rig.GroundedID : 0, 0, out var sprite)) {
			particle.Tint = sprite.SummaryTint;
		} else {
			particle.Tint = Color32.WHITE;
		}
	}

	[OnCharacterFly_Entity]
	[OnCharacterJump_Entity]
	internal static void OnJumpFly (Entity target) {
		if (target is not Character character) return;
		if (character.InWater) return;
		if (character.Movement.CurrentJumpCount > character.Movement.JumpCount + 1) return;
		// Fly without Rise
		if (character.Movement.CurrentJumpCount > character.Movement.JumpCount && character.Movement.FlyRiseSpeed <= 0) return;
		// Spawn Particle
		if (Stage.SpawnEntity(TYPE_ID, character.X, character.Y - character.DeltaPositionY) is not BigSmokeParticle particle) return;
		bool firstJump = character.Movement.CurrentJumpCount <= 1;
		particle.Scale = firstJump ? 800 : 900;
		if (firstJump && Renderer.TryGetSpriteFromGroup(character.GroundedID, 0, out var sprite)) {
			particle.Tint = sprite.SummaryTint;
		} else {
			particle.Tint = Color32.WHITE;
		}
	}

}