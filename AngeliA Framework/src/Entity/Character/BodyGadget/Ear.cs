using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class ModularEar : Ear, IModularBodyGadget { }

public abstract class Ear : BodyGadget {


	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Ear;
	public override bool SpriteLoaded => SpriteEarLeft.IsValid || SpriteEarRight.IsValid;
	protected virtual int MotionAmount => 618;
	public OrientedSprite SpriteEarLeft { get; private set; }
	public OrientedSprite SpriteEarRight { get; private set; }


	// MSG
	public override bool FillFromSheet (string basicName) {
		base.FillFromSheet(basicName);
		SpriteEarLeft = new OrientedSprite(basicName, "EarLeft", "Ear");
		SpriteEarRight = new OrientedSprite(basicName, "EarRight", "Ear");
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.EarID != 0 && TryGetGadget(renderer.EarID, out var ear)) {
			ear.DrawGadget(renderer);
		}
	}


	public override void DrawGadget (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawSpriteAsEar(
			renderer,
			SpriteEarLeft, SpriteEarRight,
			FrontOfHeadL(renderer), FrontOfHeadR(renderer),
			MotionAmount, selfMotion: true
		);
	}

	protected virtual bool FrontOfHeadL (PoseCharacterRenderer renderer) => true;
	protected virtual bool FrontOfHeadR (PoseCharacterRenderer renderer) => true;


	public static void DrawSpriteAsEar (
		PoseCharacterRenderer renderer,
		OrientedSprite spriteLeft, OrientedSprite spriteRight,
		bool frontOfHeadL = true, bool frontOfHeadR = true,
		int motionAmount = 1000, bool selfMotion = true
	) {
		var head = renderer.Head;
		if (head.Tint.a == 0) return;

		spriteLeft.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var earSpriteL);
		spriteRight.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var earSpriteR);
		if (earSpriteL == null && earSpriteR == null) return;

		bool facingRight = renderer.TargetCharacter.Movement.FacingRight;
		var headRect = head.GetGlobalRect();
		bool flipY = head.Height < 0;
		Int2 shiftL = default;
		Int2 shiftR = default;
		Int2 expandSizeL = default;
		Int2 expandSizeR = default;
		int z = head.FrontSide ? 33 : -33;
		const int A2G = Const.CEL / Const.ART_CEL;

		if (renderer.TargetCharacter.Health.HP == 0) headRect.y -= A2G;
		int basicRootY = renderer.BasicRootY;

		// Motion X
		const int MAX_SHIFT = A2G * 2;
		int motionAmountL = facingRight ? 2 * motionAmount / 1000 : motionAmount / 1000;
		int motionAmountR = facingRight ? motionAmount / 1000 : 2 * motionAmount / 1000;
		shiftL.x = (-renderer.TargetCharacter.DeltaPositionX * motionAmountL * A2G / 55).Clamp(-MAX_SHIFT, MAX_SHIFT);
		shiftR.x = (-renderer.TargetCharacter.DeltaPositionX * motionAmountR * A2G / 55).Clamp(-MAX_SHIFT, MAX_SHIFT);
		expandSizeL.x = (renderer.TargetCharacter.DeltaPositionX.Abs() * motionAmountL * A2G / 50).Clamp(-MAX_SHIFT, MAX_SHIFT);
		expandSizeR.x = (renderer.TargetCharacter.DeltaPositionX.Abs() * motionAmountR * A2G / 50).Clamp(-MAX_SHIFT, MAX_SHIFT);

		// Animation
		switch (renderer.TargetCharacter.AnimationType) {

			case CharacterAnimationType.Pound:
			case CharacterAnimationType.JumpDown:
			case CharacterAnimationType.Spin:
				expandSizeL.y += A2G;
				expandSizeR.y += A2G;

				break;
			case CharacterAnimationType.JumpUp:
			case CharacterAnimationType.Sleep:
			case CharacterAnimationType.PassOut:
				expandSizeL.y -= A2G;
				expandSizeR.y -= A2G;
				break;

			case CharacterAnimationType.Run:
				if (renderer.PoseRootY < basicRootY + A2G / 2) {
					expandSizeL.y -= A2G / 4;
					expandSizeR.y -= A2G / 4;
				} else if (renderer.PoseRootY < basicRootY + A2G) {
					expandSizeL.y -= A2G / 2;
					expandSizeR.y -= A2G / 2;
				} else {
					expandSizeL.y -= A2G;
					expandSizeR.y -= A2G;
				}
				break;
		}

		// Rot
		int rotL = 0;
		int rotR = 0;
		if (motionAmount != 0) {
			rotL = rotR = ((renderer.TargetCharacter.DeltaPositionY * motionAmount) / 2000).Clamp(-20, 20);
		}

		// Self Motion
		if (selfMotion) {
			// L
			int animationFrame = (renderer.TargetCharacter.TypeID + Game.GlobalFrame).Abs(); // ※ Intended ※
			float ease01 =
				animationFrame.PingPong(319).Clamp(0, 12) / 36f +
				(animationFrame + 172).PingPong(771).Clamp(0, 16) / 48f +
				(animationFrame + 736).PingPong(1735).Clamp(0, 12) / 36f;
			int selfRot = (int)(Ease.InBounce((1f - ease01).Clamp01()) * 200);
			rotL = (rotL + selfRot).Clamp(-20, 20);
			// R
			animationFrame = (renderer.TargetCharacter.TypeID * 2 + Game.GlobalFrame).Abs(); // ※ Intended ※
			ease01 =
				animationFrame.PingPong(372).Clamp(0, 12) / 36f +
				(animationFrame + 141).PingPong(763).Clamp(0, 16) / 48f +
				(animationFrame + 782).PingPong(1831).Clamp(0, 12) / 36f;
			selfRot = (int)(Ease.InBounce((1f - ease01).Clamp01()) * 200);
			rotR = (rotR + selfRot).Clamp(-20, 20);
		}

		// Twist
		int twist = renderer.HeadTwist;
		if (twist != 0) {
			int offset = A2G * twist.Abs() / 500;
			expandSizeL.x -= offset;
			expandSizeR.x -= offset;
		}

		// Draw
		if (earSpriteL != null) {
			var cell = Renderer.Draw(
				earSpriteL,
				headRect.x + shiftL.x,
				(flipY ? headRect.y : headRect.yMax) + shiftL.y,
				earSpriteL.PivotX, earSpriteL.PivotY, -rotL,
				earSpriteL.GlobalWidth + expandSizeL.x,
				(earSpriteL.GlobalHeight + expandSizeL.y) * (flipY ? -1 : 1),
				frontOfHeadL ? z : -z
			);
			if (renderer.Head.Rotation != 0) {
				cell.RotateAround(renderer.Head.Rotation, renderer.Body.GlobalX, renderer.Body.GlobalY + renderer.Body.Height);
				cell.Y -= renderer.Head.Height.Abs() * renderer.Head.Rotation.Abs() / 360;
			}
		}
		if (earSpriteR != null) {
			var cell = Renderer.Draw(
				earSpriteR,
				headRect.xMax + shiftR.x,
				(flipY ? headRect.y : headRect.yMax) + shiftR.y,
				earSpriteR.PivotX, earSpriteR.PivotY, rotR,
				earSpriteR.GlobalWidth + expandSizeR.x,
				(earSpriteR.GlobalHeight + expandSizeR.y) * (flipY ? -1 : 1),
				frontOfHeadR ? z : -z
			);
			if (renderer.Head.Rotation != 0) {
				cell.RotateAround(renderer.Head.Rotation, renderer.Body.GlobalX, renderer.Body.GlobalY + renderer.Body.Height);
				cell.Y -= renderer.Head.Height.Abs() * renderer.Head.Rotation.Abs() / 360;
			}
		}


	}


}
