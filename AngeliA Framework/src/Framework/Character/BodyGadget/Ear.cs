using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class ModularEar : Ear, IModularBodyGadget { }

public abstract class Ear : BodyGadget {


	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Ear;
	public override bool SpriteLoaded => SpriteIdL != 0 || SpriteIdR != 0;
	private int SpriteIdL { get; set; }
	private int SpriteIdR { get; set; }
	private int SpriteIdLBack { get; set; }
	private int SpriteIdRBack { get; set; }
	protected virtual int FacingLeftOffsetX => 0;
	protected virtual int MotionAmount => 618;


	// MSG
	public override bool FillFromSheet (string basicName) {
		base.FillFromSheet(basicName);
		SpriteIdL = $"{basicName}.EarL".AngeHash();
		SpriteIdR = $"{basicName}.EarR".AngeHash();
		SpriteIdLBack = $"{basicName}.EarLB".AngeHash();
		SpriteIdRBack = $"{basicName}.EarRB".AngeHash();
		if (!Renderer.HasSprite(SpriteIdL)) SpriteIdL = 0;
		if (!Renderer.HasSprite(SpriteIdR)) SpriteIdR = 0;
		if (!Renderer.HasSprite(SpriteIdLBack)) SpriteIdLBack = SpriteIdL;
		if (!Renderer.HasSprite(SpriteIdRBack)) SpriteIdRBack = SpriteIdR;
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacter character) {
		if (character.EarID != 0 && TryGetGadget(character.EarID, out var ear)) {
			ear.DrawGadget(character);
		}
	}


	public override void DrawGadget (PoseCharacter character) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawSpriteAsEar(
			character,
			character.Head.FrontSide ? SpriteIdL : SpriteIdLBack,
			character.Head.FrontSide ? SpriteIdR : SpriteIdRBack,
			FrontOfHeadL(character), FrontOfHeadR(character),
			character.Head.FrontSide == character.Movement.FacingRight ? 0 : FacingLeftOffsetX,
			MotionAmount, selfMotion: true
		);
	}

	protected virtual bool FrontOfHeadL (PoseCharacter character) => true;
	protected virtual bool FrontOfHeadR (PoseCharacter character) => true;


	public static void DrawSpriteAsEar (
		PoseCharacter character, int spriteIdLeft, int spriteIdRight,
		bool frontOfHeadL = true, bool frontOfHeadR = true, int offsetX = 0,
		int motionAmount = 1000, bool selfMotion = true
	) {
		if (spriteIdLeft == 0 && spriteIdRight == 0) return;

		int leftEarID = spriteIdLeft;
		int rightEarID = spriteIdRight;
		if (leftEarID == 0 && rightEarID == 0) return;

		var head = character.Head;
		if (head.Tint.a == 0) return;

		bool facingRight = character.Movement.FacingRight;
		var headRect = head.GetGlobalRect();
		bool flipY = head.Height < 0;
		Int2 shiftL = default;
		Int2 shiftR = default;
		Int2 expandSizeL = default;
		Int2 expandSizeR = default;
		int z = head.FrontSide ? 33 : -33;
		const int A2G = Const.CEL / Const.ART_CEL;

		if (character.HealthPoint == 0) headRect.y -= A2G;
		int basicRootY = character.BasicRootY;

		// Motion X
		const int MAX_SHIFT = A2G * 2;
		int motionAmountL = facingRight ? 2 * motionAmount / 1000 : motionAmount / 1000;
		int motionAmountR = facingRight ? motionAmount / 1000 : 2 * motionAmount / 1000;
		shiftL.x = (-character.DeltaPositionX * motionAmountL * A2G / 55).Clamp(-MAX_SHIFT, MAX_SHIFT);
		shiftR.x = (-character.DeltaPositionX * motionAmountR * A2G / 55).Clamp(-MAX_SHIFT, MAX_SHIFT);
		expandSizeL.x = (character.DeltaPositionX.Abs() * motionAmountL * A2G / 50).Clamp(-MAX_SHIFT, MAX_SHIFT);
		expandSizeR.x = (character.DeltaPositionX.Abs() * motionAmountR * A2G / 50).Clamp(-MAX_SHIFT, MAX_SHIFT);

		// Animation
		switch (character.AnimationType) {

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
				if (character.PoseRootY < basicRootY + A2G / 2) {
					expandSizeL.y -= A2G / 4;
					expandSizeR.y -= A2G / 4;
				} else if (character.PoseRootY < basicRootY + A2G) {
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
			rotL = rotR = ((character.DeltaPositionY * motionAmount) / 2000).Clamp(-20, 20);
		}

		// Self Motion
		if (selfMotion) {
			// L
			int animationFrame = (character.TypeID + Game.GlobalFrame).Abs(); // ※ Intended ※
			float ease01 =
				animationFrame.PingPong(319).Clamp(0, 12) / 36f +
				(animationFrame + 172).PingPong(771).Clamp(0, 16) / 48f +
				(animationFrame + 736).PingPong(1735).Clamp(0, 12) / 36f;
			int selfRot = (int)(Ease.InBounce((1f - ease01).Clamp01()) * 200);
			rotL = (rotL + selfRot).Clamp(-20, 20);
			// R
			animationFrame = (character.TypeID * 2 + Game.GlobalFrame).Abs(); // ※ Intended ※
			ease01 =
				animationFrame.PingPong(372).Clamp(0, 12) / 36f +
				(animationFrame + 141).PingPong(763).Clamp(0, 16) / 48f +
				(animationFrame + 782).PingPong(1831).Clamp(0, 12) / 36f;
			selfRot = (int)(Ease.InBounce((1f - ease01).Clamp01()) * 200);
			rotR = (rotR + selfRot).Clamp(-20, 20);
		}

		// Twist
		int twist = character.HeadTwist;
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
			if (character.Head.Rotation != 0) {
				cell.RotateAround(character.Head.Rotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
				cell.Y -= character.Head.Height.Abs() * character.Head.Rotation.Abs() / 360;
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
			if (character.Head.Rotation != 0) {
				cell.RotateAround(character.Head.Rotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
				cell.Y -= character.Head.Height.Abs() * character.Head.Rotation.Abs() / 360;
			}
		}


	}


}
