using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

public abstract class Flail : MeleeWeapon {

	// VAR
	private int SpriteIdHead { get; init; }
	private int SpriteIdChain { get; init; }
	protected virtual int ChainLength => Const.CEL * 7 / 9;
	protected virtual int ChainLengthAttackGrow => 500;
	protected virtual int HeadCount => 1;
	protected virtual bool FixGrabRotation => true;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
	public override int HandheldPoseAnimationID => PoseHandheld_SingleHanded.TYPE_ID;
	public override int PerformPoseAnimationID => PoseAttack_WaveSingleHanded_SmashOnly.TYPE_ID;

	// MSG
	public Flail () {
		SpriteIdHead = $"{GetType().AngeName()}.Head".AngeHash();
		if (!Renderer.HasSprite(SpriteIdHead)) SpriteIdHead = 0;
		SpriteIdChain = $"{GetType().AngeName()}.Chain".AngeHash();
		if (!Renderer.HasSpriteGroup(SpriteIdChain)) SpriteIdChain = 0;
	}

	public override Cell OnToolSpriteRendered (PoseCharacterRenderer renderer, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		// Fix Grab Rotation
		if (FixGrabRotation) {
			renderer.HandGrabRotationL += (
				renderer.HandGrabRotationL.Sign() * -Util.Sin(renderer.HandGrabRotationL.Abs() * Util.Deg2Rad) * 30
			).RoundToInt();
			renderer.HandGrabRotationR += (
				renderer.HandGrabRotationR.Sign() * -Util.Sin(renderer.HandGrabRotationR.Abs() * Util.Deg2Rad) * 30
			).RoundToInt();
		}
		// Draw
		var cell = base.OnToolSpriteRendered(renderer, x, y, width, height, grabRotation, grabScale, sprite, z);
		for (int i = 0; i < HeadCount; i++) {
			DrawFlailHead(renderer, cell, i);
		}
		return cell;
	}

	private void DrawFlailHead (PoseCharacterRenderer renderer, Cell handleCell, int headIndex) {

		var attack = renderer.TargetCharacter.Attackness;
		var movement = renderer.TargetCharacter.Movement;
		bool isAttacking = attack.IsAttacking;
		bool climbing = renderer.TargetCharacter.AnimationType == CharacterAnimationType.Climb;
		int deltaX = renderer.TargetCharacter.DeltaPositionX.Clamp(-20, 20);
		int deltaY = renderer.TargetCharacter.DeltaPositionY.Clamp(-30, 30);
		var point = handleCell.LocalToGlobal(handleCell.Width / 2, handleCell.Height);
		int chainLength = isAttacking ? ChainLength * ChainLengthAttackGrow / 1000 : ChainLength;
		Int2 headPos;

		if (isAttacking) {
			// Attack
			int localFrame = Game.GlobalFrame - attack.LastAttackFrame;
			int duration = Duration;
			int swingX = Const.CEL.LerpTo(-Const.CEL, Ease.OutBack((float)localFrame / duration));
			headPos = handleCell.LocalToGlobal(
				handleCell.Width / 2 + (movement.FacingRight ? -swingX : swingX) + headIndex * 96,
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
				(Game.GlobalFrame - (movement.LastEndMoveFrame >= 0 ? movement.LastEndMoveFrame : 0)).Clamp(0, SHAKE_DURATION),
				(Game.GlobalFrame - (attack.LastAttackFrame >= 0 ? attack.LastAttackFrame : 0)).Clamp(0, SHAKE_DURATION)
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
			int scale = renderer.HandGrabScaleR;
			if (climbing && !isAttacking) scale = -scale.Abs();
			int rot = headSprite.IsTrigger ?
				new Float2(point.x - headPos.x, point.y - headPos.y).GetRotation() : 0;
			Renderer.Draw(
				headSprite, headPos.x, headPos.y,
				headSprite.PivotX, headSprite.PivotY, rot,
				headSprite.GlobalWidth * scale / 1000,
				headSprite.GlobalHeight * scale.Abs() / 1000,
				(movement.FacingFront ? 36 : -36) - headIndex
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
						movement.FacingFront ? 35 : -35
					);
				}
			}
		}

	}

}
