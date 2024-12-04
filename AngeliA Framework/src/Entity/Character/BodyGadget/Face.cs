using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public enum CharacterFaceExpression {
	Normal = 0,
	Blink = 1,
	Sleep = 2,
	Damage = 3,
	PassOut = 4,
	Attack = 5,
	Suffer = 6,
	Happy = 7,
}


public sealed class DefaultFace : Face {
	public static readonly int TYPE_ID = typeof(DefaultFace).AngeHash();
	public DefaultFace () => FillFromSheet(GetType().AngeName());
}


public sealed class ModularFace : Face, IModularBodyGadget { }


public abstract class Face : BodyGadget {


	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Face;
	public override bool SpriteLoaded => (SpriteEyeLeft.IsValid || SpriteEyeRight.IsValid) && (SpriteScleraLeft.IsValid || SpriteScleraRight.IsValid);

	private OrientedSprite SpriteEyeLeft;
	private OrientedSprite SpriteScleraLeft;
	private OrientedSprite SpriteEyelashLeft;
	private OrientedSprite SpriteEyebrowLeft;
	private OrientedSprite SpriteEarLeft;
	private OrientedSprite SpriteEyeRight;
	private OrientedSprite SpriteScleraRight;
	private OrientedSprite SpriteEyelashRight;
	private OrientedSprite SpriteEyebrowRight;
	private OrientedSprite SpriteEarRight;
	private OrientedSprite SpriteMouth;
	private OrientedSprite SpriteTooth;


	// API
	public override bool FillFromSheet (string keyword) {
		base.FillFromSheet(keyword);
		SpriteEyeLeft = new OrientedSprite(keyword, "EyeLeft", "Eye");
		SpriteScleraLeft = new OrientedSprite(keyword, "ScleraLeft", "Sclera");
		SpriteEyelashLeft = new OrientedSprite(keyword, "EyelashLeft", "Eyelash");
		SpriteEyebrowLeft = new OrientedSprite(keyword, "EyebrowLeft", "Eyebrow");
		SpriteEarLeft = new OrientedSprite(keyword, "HumanEarLeft", "HumanEar");

		SpriteEyeRight = new OrientedSprite(keyword, "EyeRight", "Eye");
		SpriteScleraRight = new OrientedSprite(keyword, "ScleraRight", "Sclera");
		SpriteEyelashRight = new OrientedSprite(keyword, "EyelashRight", "Eyelash");
		SpriteEyebrowRight = new OrientedSprite(keyword, "EyebrowRight", "Eyebrow");
		SpriteEarRight = new OrientedSprite(keyword, "HumanEarRight", "HumanEar");

		SpriteMouth = new OrientedSprite(keyword, "Mouth");
		SpriteTooth = new OrientedSprite(keyword, "Tooth");
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.FaceID == 0 || !TryGetGadget(renderer.FaceID, out var face)) return;
		face.DrawGadget(renderer);
	}


	public override void DrawGadget (PoseCharacterRenderer renderer) {

		if (!SpriteLoaded) return;

		var head = renderer.Head;
		if (head.IsFullCovered || !head.FrontSide) return;

		using var _ = new SheetIndexScope(SheetIndex);

		// Get Head Rect
		int bounce = renderer.CurrentRenderingBounce;
		var headRect = head.GetGlobalRect();
		if (bounce.Abs() != 1000) {
			const int A2G = Const.CEL / Const.ART_CEL;
			bool reverse = bounce < 0;
			bounce = bounce.Abs();
			int newWidth = (reverse ?
				headRect.width * 1000 / bounce :
				headRect.width - headRect.width * (1000 - bounce) / 1000
			).Clamp(headRect.width - A2G * 2, headRect.width + A2G * 2);
			headRect.x -= (newWidth - headRect.width) / 2;
			headRect.width = newWidth;
		}

		// Get Face Rect
		bool facingRight = head.Width > 0;
		var faceRect = headRect;
		var border = head.Border;
		if (!facingRight) {
			border.left = head.Border.right;
			border.right = head.Border.left;
		}
		if (!border.IsZero) {
			if (head.Height < 0) {
				(border.down, border.up) = (border.up, border.down);
			}
			faceRect = headRect.Shrink(border);
		}

		// Draw Face
		var expression = GetCurrentExpression(renderer);
		int startCellIndex = Renderer.GetUsedCellCount();

		DrawEye(renderer, expression, faceRect, true);
		DrawEye(renderer, expression, faceRect, false);
		DrawMouth(renderer, expression, faceRect);
		DrawSpriteAsHumanEar(renderer, SpriteEarLeft, SpriteEarRight);

		// Move with Head
		if (Renderer.GetCells(out var cells, out int count)) {

			// Twist
			int twist = renderer.HeadTwist;
			if (twist != 0) {
				int offsetX = faceRect.width * twist / 2000;
				for (int i = startCellIndex; i < count; i++) {
					var cell = cells[i];
					cell.X += offsetX;
					cell.Width -= offsetX.Abs() / 2;
				}
				Util.ClampCells(cells, headRect, startCellIndex, count);
			}

			// Rotate
			int headRot = renderer.Head.Rotation;
			if (headRot != 0) {
				var body = renderer.Body;
				int offsetY = renderer.Head.Height.Abs() * headRot.Abs() / 360;
				for (int i = startCellIndex; i < count; i++) {
					var cell = cells[i];
					cell.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
					cell.Y -= offsetY;
				}
			}
		}
	}


	protected virtual void DrawEye (PoseCharacterRenderer renderer, CharacterFaceExpression expression, IRect faceRect, bool leftEye) {

		var SpriteEye = leftEye ? SpriteEyeLeft : SpriteEyeRight;
		var SpriteSclera = leftEye ? SpriteScleraLeft : SpriteScleraRight;

		if (!SpriteEye.IsValid || !SpriteSclera.IsValid) return;
		var SpriteEyebrow = leftEye ? SpriteEyebrowLeft : SpriteEyebrowRight;
		var SpriteEyelash = leftEye ? SpriteEyelashLeft : SpriteEyelashRight;

		bool facingRight = renderer.Head.Width > 0;
		if (!SpriteEye.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var eye)) return;
		if (!SpriteSclera.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var sclera)) return;
		SpriteEyebrow.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var eyebrow);

		var rect = faceRect.CornerInside(leftEye ? Alignment.TopLeft : Alignment.TopRight, sclera.GlobalWidth, sclera.GlobalHeight);
		bool eyeOpening =
			expression == CharacterFaceExpression.Normal ||
			expression == CharacterFaceExpression.PassOut ||
			expression == CharacterFaceExpression.Damage;

		// Expression Redirect
		if (!SpriteEyelash.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var eyelash) && !eyeOpening) {
			expression = CharacterFaceExpression.Normal;
			eyeOpening = true;
		}

		// Check for Trigger-Lash
		if (eyeOpening && eyelash != null && eyelash.IsTrigger) {
			eyelash = null;
		}

		switch (expression) {
			case CharacterFaceExpression.Normal: {

				// Sclera
				Renderer.Draw(sclera, rect, z: 33);

				// Eye
				var eyeRect =
					eye.GlobalWidth == sclera.GlobalWidth ? rect :
					leftEye ? rect.CornerInside(Alignment.TopRight, eye.GlobalWidth, eye.GlobalHeight) :
					rect.CornerInside(Alignment.TopLeft, eye.GlobalWidth, eye.GlobalHeight);
				Renderer.Draw(eye, eyeRect, z: 34);

				break;
			}
			case CharacterFaceExpression.Blink:
			case CharacterFaceExpression.Sleep: {
				rect.height = 1;
				break;
			}
			case CharacterFaceExpression.Damage: {

				int pointX = facingRight ? rect.x + rect.width / 4 : rect.xMax - rect.width / 4;
				int pointY = rect.CenterY();
				float scale = (float)(Game.GlobalFrame.PingPong(10) / 10f);

				// Sclera
				Renderer.Draw(sclera, rect.ScaleFrom(scale + 1f, pointX, pointY), z: 33);

				// Eye
				var eyeRect =
					eye.GlobalWidth == sclera.GlobalWidth ? rect :
					leftEye ? rect.CornerInside(Alignment.TopRight, eye.GlobalWidth, eye.GlobalHeight) :
					rect.CornerInside(Alignment.TopLeft, eye.GlobalWidth, eye.GlobalHeight);
				Renderer.Draw(eye, eyeRect.ScaleFrom(1f - scale / 2f, pointX, pointY), z: 34);

				break;
			}
			case CharacterFaceExpression.PassOut: {

				int expand = rect.height / 8;

				// Sclera
				var bigRect = rect.Expand(expand);
				Renderer.Draw(sclera, bigRect, z: 33);

				// Eye
				var eyeRect =
					eye.GlobalWidth == sclera.GlobalWidth ? rect :
					leftEye ? rect.CornerInside(Alignment.TopRight, eye.GlobalWidth, eye.GlobalHeight) :
					rect.CornerInside(Alignment.TopLeft, eye.GlobalWidth, eye.GlobalHeight);
				int shiftX = (int)(Util.Sin(Game.GlobalFrame * 4 * Util.Deg2Rad) * bigRect.width / 2);
				int shiftY = (int)(Util.Cos(Game.GlobalFrame * 4 * Util.Deg2Rad) * bigRect.height / 2);
				if (leftEye) {
					shiftX = -shiftX;
					shiftY = -shiftY;
				}
				Renderer.Draw(eye, eyeRect.Shift(shiftX, shiftY), z: 34);
				break;
			}
			case CharacterFaceExpression.Attack:
			case CharacterFaceExpression.Suffer: {
				rect.height = 1;
				if (eyelash == null) break;
				eyebrow = null;
				Renderer.Draw(
					eyelash,
					leftEye ? rect.xMax : rect.x,
					rect.yMax,
					eyelash.PivotX, eyelash.PivotY,
					leftEye ? 45 : -45,
					(leftEye ? eyelash.GlobalWidth : -eyelash.GlobalWidth) * 10 / 7,
					eyelash.GlobalHeight,
					z: 36
				);
				break;
			}
			case CharacterFaceExpression.Happy: {
				if (eyelash == null) break;
				eyebrow = null;
				Renderer.Draw(
					eyelash,
					rect.CenterX(), rect.yMax,
					eyelash.PivotX, eyelash.PivotY,
					-135,
					eyelash.GlobalWidth - eyelash.GlobalHeight,
					eyelash.GlobalHeight,
					z: 36
				);
				Renderer.Draw(
					eyelash,
					rect.CenterX(), rect.yMax,
					eyelash.PivotX, eyelash.PivotY,
					-45,
					eyelash.GlobalWidth,
					eyelash.GlobalHeight,
					z: 36
				);
				eyelash = null;
				break;
			}
		}

		// Eyebrow
		if (eyebrow != null) {
			bool needFlip = !eyebrow.IsTrigger && !leftEye;
			Renderer.Draw(
				eyebrow,
				leftEye ? rect.xMax : rect.x,
				rect.yMax,
				eyebrow.PivotX, eyebrow.PivotY, 0,
				needFlip ? -eyebrow.GlobalWidth : eyebrow.GlobalWidth,
				eyebrow.GlobalHeight,
				z: 36
			);
		}

		// Eyelash
		if (eyelash != null) {
			Renderer.Draw(
				eyelash,
				leftEye ? rect.xMax : rect.x,
				rect.yMax,
				eyelash.PivotX, eyelash.PivotY, 0,
				leftEye ? eyelash.GlobalWidth : -eyelash.GlobalWidth,
				eyelash.GlobalHeight,
				z: 36
			);
		}

	}


	protected virtual void DrawMouth (PoseCharacterRenderer renderer, CharacterFaceExpression expression, IRect faceRect) {

		if (expression != CharacterFaceExpression.PassOut && expression != CharacterFaceExpression.Damage) return;

		bool facingRight = renderer.Head.Width > 0;
		if (!SpriteMouth.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var mouth)) return;
		SpriteTooth.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var tooth);

		var rect = faceRect.CornerInside(Alignment.BottomMid, mouth.GlobalWidth, mouth.GlobalHeight);

		// Animation for Damage
		if (expression == CharacterFaceExpression.Damage) {
			int newWidth = rect.width / 2 + (Game.GlobalFrame.PingPong(6) * rect.width / 24);
			int newHeight = rect.height * 3 / 2 + ((Game.GlobalFrame + 3).PingPong(6) * rect.height / 36);
			rect.x += (rect.width - newWidth) / 2;
			rect.y += rect.height - newHeight;
			rect.width = newWidth;
			rect.height = newHeight;
			// Draw Tooth
			if (tooth != null) {
				var toothRect = rect.Edge(Direction4.Up, tooth.GlobalHeight);
				if (tooth.GlobalBorder.IsZero) {
					Renderer.Draw(tooth, toothRect, z: 34);
				} else {
					Renderer.DrawSlice(tooth, toothRect, Color32.WHITE, z: 34);
				}
			}
		}

		// Draw Mouth
		if (!facingRight && mouth.IsTrigger) {
			rect.FlipHorizontal();
		}
		if (mouth.GlobalBorder.IsZero) {
			Renderer.Draw(mouth, rect, z: 33);
		} else {
			Renderer.DrawSlice(mouth, rect, Color32.WHITE, z: 33);
		}

	}


	public static void DrawSpriteAsHumanEar (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int offsetXL = 0, int offsetXR = 0) {

		// Get Face Rect
		var head = renderer.Head;
		var headRect = head.GetGlobalRect();
		bool facingRight = head.Width > 0;
		var faceRect = headRect;
		var border = head.Border;
		if (!facingRight) {
			border.left = head.Border.right;
			border.right = head.Border.left;
		}
		if (!border.IsZero) {
			if (head.Height < 0) {
				(border.down, border.up) = (border.up, border.down);
			}
			faceRect = headRect.Shrink(border);
		}

		// Draw Ears
		if (!facingRight) (offsetXL, offsetXR) = (-offsetXR, -offsetXL);

		spriteLeft.TryGetSprite(true, head.Width > 0, renderer.CurrentAnimationFrame, out var spriteL);
		var cellL = Renderer.Draw(
			spriteL,
			faceRect.x + offsetXL, faceRect.yMax, 1000, 1000, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			facingRight ? 33 : -33
		);

		spriteRight.TryGetSprite(true, head.Width > 0, renderer.CurrentAnimationFrame, out var spriteR);
		var cellR = Renderer.Draw(
			spriteR,
			faceRect.xMax + offsetXR, faceRect.yMax, 0, 1000, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			facingRight ? -33 : 33
		);

		// Rotate
		int headRot = renderer.Head.Rotation;
		if (headRot != 0) {
			var body = renderer.Body;
			int offsetY = renderer.Head.Height.Abs() * headRot.Abs() / 360;
			cellL.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
			cellL.Y -= offsetY;
			cellR.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
			cellR.Y -= offsetY;

		}
	}


	public static CharacterFaceExpression GetCurrentExpression (PoseCharacterRenderer renderer) {

		if (renderer.ForceFaceExpressionIndex >= 0) {
			return (CharacterFaceExpression)renderer.ForceFaceExpressionIndex.FinalValue;
		}

		// Attack
		if (
			renderer.TargetCharacter.Attackness.IsAttacking &&
			renderer.TargetCharacter.EquippingToolType != ToolType.Magic &&
			renderer.TargetCharacter.EquippingToolType != ToolType.Ranged
		) return CharacterFaceExpression.Attack;

		// Blink
		if (
			(Game.GlobalFrame + renderer.TargetCharacter.TypeID).UMod(360) <= 8 &&
			renderer.TargetCharacter.AnimationType != CharacterAnimationType.Sleep &&
			renderer.TargetCharacter.AnimationType != CharacterAnimationType.PassOut
		) return CharacterFaceExpression.Blink;

		// In Ground
		if (renderer.TargetCharacter.IsInsideGround) {
			return CharacterFaceExpression.Suffer;
		}

		// Other
		return renderer.TargetCharacter.AnimationType switch {
			CharacterAnimationType.Sleep => CharacterFaceExpression.Sleep,
			CharacterAnimationType.PassOut => CharacterFaceExpression.PassOut,
			CharacterAnimationType.Crash => CharacterFaceExpression.Suffer,
			CharacterAnimationType.TakingDamage => CharacterFaceExpression.Damage,
			_ => CharacterFaceExpression.Normal,
		};
	}


}
