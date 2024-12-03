using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class ModularEar : Ear, IModularBodyGadget { }

public abstract class Ear : BodyGadget {


	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Ear;
	public override bool SpriteLoaded => SpriteEar.IsValid;
	protected virtual int FacingLeftOffsetX => 0;
	protected virtual int MotionAmount => 618;
	public OrientedSprite SpriteEar { get; private set; }


	// MSG
	public override bool FillFromSheet (string basicName) {
		base.FillFromSheet(basicName);
		SpriteEar = new OrientedSprite(basicName, "Ear");
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
			SpriteEar,
			FrontOfHeadL(renderer), FrontOfHeadR(renderer),
			renderer.Head.FrontSide == renderer.TargetCharacter.Movement.FacingRight ? 0 : FacingLeftOffsetX,
			MotionAmount, selfMotion: true
		);
	}

	protected virtual bool FrontOfHeadL (PoseCharacterRenderer renderer) => true;
	protected virtual bool FrontOfHeadR (PoseCharacterRenderer renderer) => true;


	public static void DrawSpriteAsEar (
		PoseCharacterRenderer renderer, OrientedSprite oSprite,
		bool frontOfHeadL = true, bool frontOfHeadR = true, int offsetX = 0,
		int motionAmount = 1000, bool selfMotion = true
	) {
		if (!oSprite.IsValid) return;

		var head = renderer.Head;
		int leftEarID = oSprite.GetSpriteID(head.FrontSide, false);
		int rightEarID = oSprite.GetSpriteID(head.FrontSide, true);
		if (leftEarID == 0 && rightEarID == 0) return;

		if (head.Tint.a == 0) return;

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
		if (Renderer.TryGetSprite(leftEarID, out var earSpriteL)) {
			var cell = Renderer.Draw(
				earSpriteL,
				headRect.x + shiftL.x + offsetX,
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
		if (Renderer.TryGetSprite(rightEarID, out var earSpriteR)) {
			var cell = Renderer.Draw(
				earSpriteR,
				headRect.xMax + shiftR.x + offsetX,
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
