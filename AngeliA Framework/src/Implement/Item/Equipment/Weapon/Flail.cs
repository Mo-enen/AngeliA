using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 


// Flail
public abstract class Flail<B> : Flail where B : MeleeBullet {
	public Flail () => BulletID = typeof(B).AngeHash();
}
[RequireSprite("{0}.Head", "{0}.Chain")]
public abstract class Flail : MeleeWeapon {

	public sealed override WeaponType WeaponType => WeaponType.Flail;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	private int SpriteIdHead { get; init; }
	private int SpriteIdChain { get; init; }
	protected virtual int ChainLength => Const.CEL * 7 / 9;
	protected virtual int ChainLengthAttackGrow => 500;
	protected virtual int HeadCount => 1;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;

	public Flail () {
		SpriteIdHead = $"{GetType().AngeName()}.Head".AngeHash();
		if (!Renderer.HasSprite(SpriteIdHead)) SpriteIdHead = 0;
		SpriteIdChain = $"{GetType().AngeName()}.Chain".AngeHash();
		if (!Renderer.HasSpriteGroup(SpriteIdChain)) SpriteIdChain = 0;
	}

	protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		// Fix Grab Rotation
		if (character.EquippingWeaponHeld != WeaponHandheld.Pole) {
			character.HandGrabRotationL += (
				character.HandGrabRotationL.Sign() * -Util.Sin(character.HandGrabRotationL.Abs() * Util.Deg2Rad) * 30
			).RoundToInt();
			character.HandGrabRotationR += (
				character.HandGrabRotationR.Sign() * -Util.Sin(character.HandGrabRotationR.Abs() * Util.Deg2Rad) * 30
			).RoundToInt();
		}
		// Draw
		var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
		for (int i = 0; i < HeadCount; i++) {
			DrawFlailHead(character, cell, i);
		}
		return cell;
	}

	private void DrawFlailHead (PoseCharacter character, Cell handleCell, int headIndex) {

		bool isAttacking = character.IsAttacking;
		bool climbing = character.AnimationType == CharacterAnimationType.Climb;
		int deltaX = character.DeltaPositionX.Clamp(-20, 20);
		int deltaY = character.DeltaPositionY.Clamp(-30, 30);
		var point = handleCell.LocalToGlobal(handleCell.Width / 2, handleCell.Height);
		int chainLength = isAttacking ? ChainLength * ChainLengthAttackGrow / 1000 : ChainLength;
		Int2 headPos;

		if (isAttacking) {
			// Attack
			int localFrame = Game.GlobalFrame - character.LastAttackFrame;
			int duration = AttackDuration;
			int swingX = Const.CEL.LerpTo(-Const.CEL, Ease.OutBack((float)localFrame / duration));
			headPos = handleCell.LocalToGlobal(
				handleCell.Width / 2 + (character.FacingRight ? -swingX : swingX) + headIndex * 96,
				handleCell.Height + chainLength - headIndex * 16
			);
		} else {
			// Hover
			headPos = new Int2(
				point.x - deltaX,
				point.y - chainLength - deltaY
			);
			// Shake
			const int SHAKE_DURATION = 60;
			int shakeFrame = Util.Min(
				(Game.GlobalFrame - (character.LastEndMoveFrame >= 0 ? character.LastEndMoveFrame : 0)).Clamp(0, SHAKE_DURATION),
				(Game.GlobalFrame - (character.LastAttackFrame >= 0 ? character.LastAttackFrame : 0)).Clamp(0, SHAKE_DURATION)
			);
			if (!climbing && shakeFrame >= 0 && shakeFrame < SHAKE_DURATION) {
				headPos.x += (
					Util.Cos(shakeFrame / (float)SHAKE_DURATION * 720f * Util.Deg2Rad) *
					(SHAKE_DURATION - shakeFrame) * 0.75f
				).RoundToInt();
			}
			if (headIndex > 0) {
				headPos.x += (headIndex % 2 == 0 ? 1 : -1) * ((headIndex + 1) / 2) * 84;
				headPos.y += (headIndex + 1) / 2 * 32;
			}
		}

		// Draw Head
		if (SpriteIdHead != 0 && Renderer.TryGetSprite(SpriteIdHead, out var headSprite)) {
			int scale = character.HandGrabScaleR;
			if (climbing && !isAttacking) scale = -scale.Abs();
			int rot = headSprite.IsTrigger ?
				new Float2(point.x - headPos.x, point.y - headPos.y).GetRotation() : 0;
			Renderer.Draw(
				headSprite, headPos.x, headPos.y,
				headSprite.PivotX, headSprite.PivotY, rot,
				headSprite.GlobalWidth * scale / 1000,
				headSprite.GlobalHeight * scale.Abs() / 1000,
				(character.FacingFront ? 36 : -36) - headIndex
			);
		}

		// Draw Chain
		if (SpriteIdChain != 0 && Renderer.HasSpriteGroup(SpriteIdChain, out int chainCount)) {
			int rot = new Float2(point.x - headPos.x, point.y - headPos.y).GetRotation();
			for (int i = 0; i < chainCount; i++) {
				if (Renderer.TryGetSpriteFromGroup(SpriteIdChain, i, out var chainSprite, false, true)) {
					Renderer.Draw(
						chainSprite,
						Util.RemapUnclamped(-1, chainCount, point.x, headPos.x, i),
						Util.RemapUnclamped(-1, chainCount, point.y, headPos.y, i),
						500, 500, rot,
						chainSprite.GlobalWidth / 2, chainSprite.GlobalHeight / 2,
						character.FacingFront ? 35 : -35
					);
				}
			}
		}

	}

}


// Implement
[ItemCombination(typeof(iSpikeBall), typeof(iChain), typeof(iTreeBranch), 1)]
public class iFlailWood : Flail {}
[ItemCombination(typeof(iFlailWood), typeof(iIngotIron), 1)]
public class iFlailIron : Flail { }
[ItemCombination(typeof(iIngotGold), typeof(iFlailIron), 1)]
public class iFlailGold : Flail { }
[ItemCombination(typeof(iFlailWood), typeof(iFlailWood), typeof(iFlailWood), 1)]
public class iFlailTriple : Flail {
	protected override int HeadCount => 3;
}
[ItemCombination(typeof(iEyeBall), typeof(iFlailIron), 1)]
public class iFlailEye : Flail { }
[ItemCombination(typeof(iSkull), typeof(iFlailWood), 1)]
public class iFlailSkull : Flail { }
[ItemCombination(typeof(iIronHook), typeof(iRope), typeof(iTreeBranch), 1)]
public class iFishingPole : Flail { }
[ItemCombination(typeof(iChain), typeof(iMaceSpiked), typeof(iTreeBranch), 1)]
public class iFlailMace : Flail { }
[ItemCombination(typeof(iIronHook), typeof(iChain), 1)]
public class iFlailHook : Flail { }
[ItemCombination(typeof(iChain), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
public class iNunchaku : Flail {
	public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	protected override int ChainLength => Const.CEL * 2 / 9;
	protected override int ChainLengthAttackGrow => 2000;
}
[ItemCombination(typeof(iPickWood), typeof(iChain), typeof(iTreeBranch), 1)]
public class iFlailPick : Flail {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	public override int AttackDuration => 24;
}
[ItemCombination(typeof(iChain), typeof(iChain), typeof(iMaceSpiked), typeof(iMaceSpiked), 1)]
public class iChainMace : Flail {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	protected override int ChainLength => Const.CEL * 2 / 9;
	protected override int ChainLengthAttackGrow => 2000;
	public override int AttackDuration => 24;
}
[ItemCombination(typeof(iSpikeBall), typeof(iChain), 1)]
public class iChainSpikeBall : Flail {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	protected override int ChainLengthAttackGrow => 2000;
	public override int AttackDuration => 24;
}
[ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iChain), 1)]
public class iChainBarbed : Flail {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	protected override int ChainLengthAttackGrow => 2000;
	public override int AttackDuration => 24;
}
[ItemCombination(typeof(iFist), typeof(iChain), 1)]
public class iChainFist : Flail {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	protected override int ChainLengthAttackGrow => 2000;
	public override int AttackDuration => 24;
}
