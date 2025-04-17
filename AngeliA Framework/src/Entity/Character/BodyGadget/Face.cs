using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


/// <summary>
/// Face expression type for pose animation characters
/// </summary>
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


/// <summary>
/// Face gadget for pose characters
/// </summary>
public abstract class Face : BodyGadget {




	#region --- VAR ---


	// Api
	public sealed override BodyGadgetType GadgetType => BodyGadgetType.Face;
	public override bool SpriteLoaded => (SpriteEyeLeft.IsValid || SpriteEyeRight.IsValid) && (SpriteScleraLeft.IsValid || SpriteScleraRight.IsValid);

	// Data
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


	#endregion




	#region --- MSG ---


	public override void DrawGadget (PoseCharacterRenderer renderer) {

		if (!SpriteLoaded) return;

		var head = renderer.Head;
		if (head.IsFullCovered) return;

		using var _ = new SheetIndexScope(SheetIndex);
		if (!head.FrontSide) {
			DrawSpriteAsHumanEar(renderer, SpriteEarLeft, SpriteEarRight);
			return;
		}

		var faceRect = GetFaceRect(renderer, out var headRect);

		// Draw Face
		var expression = GetCurrentExpression(renderer);
		int startCellIndex = Renderer.GetUsedCellCount();
		bool facingRight = renderer.Head.Width > 0;
		int aniFrame = renderer.CurrentAnimationFrame;

		DrawEye(expression, faceRect, true, facingRight, aniFrame);
		DrawEye(expression, faceRect, false, facingRight, aniFrame);
		DrawMouth(expression, faceRect, facingRight, aniFrame);
		
		// Twist
		if (Renderer.GetCells(out var cells, out int count)) {
			int twist = renderer.HeadTwist;
			if (twist != 0) {
				int offsetX = faceRect.width * twist / 2000;
				for (int i = startCellIndex; i < count; i++) {
					var cell = cells[i];
					cell.X += offsetX;
					cell.Width -= offsetX.Abs() / 2;
				}
				FrameworkUtil.ClampCells(cells, headRect, startCellIndex, count);
			}
		}

		DrawSpriteAsHumanEar(renderer, SpriteEarLeft, SpriteEarRight);

		// Move with Head
		if (Renderer.GetCells(out cells, out count)) {
			// Rotate
			int headRot = renderer.Head.Rotation;
			if (headRot != 0) {
				var body = renderer.Body;
				int offsetY = renderer.Head.Height.Abs() * headRot.Abs() / 360;
				for (int i = startCellIndex; i < count; i++) {
					cells[i].RotateAround(headRot, head.GlobalX, head.GlobalY);
				}
			}
		}
	}


	public override void DrawGadgetGizmos (IRect rect, Color32 tint, int z) {
		DrawEye(CharacterFaceExpression.Normal, rect, true, true, 0);
		DrawEye(CharacterFaceExpression.Normal, rect, false, true, 0);
	}


	/// <summary>
	/// Draw one eye for pose-style character
	/// </summary>
	/// <param name="expression">Current face expression</param>
	/// <param name="faceRect">Rect position for the character's face</param>
	/// <param name="leftEye">True if this eye is the left eye</param>
	/// <param name="facingRight">True if the character is facing right</param>
	/// <param name="animationFrame"></param>
	protected virtual void DrawEye (CharacterFaceExpression expression, IRect faceRect, bool leftEye, bool facingRight, int animationFrame) {

		var SpriteEye = leftEye ? SpriteEyeLeft : SpriteEyeRight;
		var SpriteSclera = leftEye ? SpriteScleraLeft : SpriteScleraRight;

		if (!SpriteEye.IsValid || !SpriteSclera.IsValid) return;
		var SpriteEyebrow = leftEye ? SpriteEyebrowLeft : SpriteEyebrowRight;
		var SpriteEyelash = leftEye ? SpriteEyelashLeft : SpriteEyelashRight;

		if (!SpriteEye.TryGetSprite(true, facingRight, animationFrame, out var eye)) return;
		if (!SpriteSclera.TryGetSprite(true, facingRight, animationFrame, out var sclera)) return;
		SpriteEyebrow.TryGetSprite(true, facingRight, animationFrame, out var eyebrow);

		var rect = faceRect.CornerInside(leftEye ? Alignment.TopLeft : Alignment.TopRight, sclera.GlobalWidth, sclera.GlobalHeight);
		bool eyeOpening =
			expression == CharacterFaceExpression.Normal ||
			expression == CharacterFaceExpression.PassOut ||
			expression == CharacterFaceExpression.Damage;

		// Expression Redirect
		if (!SpriteEyelash.TryGetSprite(true, facingRight, animationFrame, out var eyelash) && !eyeOpening) {
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


	/// <summary>
	/// Draw mouth for pose-style character
	/// </summary>
	/// <param name="expression">Current face expression</param>
	/// <param name="faceRect">Rect position for the character's face</param>
	/// <param name="facingRight">True if the character is facing right</param>
	/// <param name="animationFrame"></param>
	protected virtual void DrawMouth (CharacterFaceExpression expression, IRect faceRect, bool facingRight, int animationFrame) {

		if (expression != CharacterFaceExpression.PassOut && expression != CharacterFaceExpression.Damage) return;

		if (!SpriteMouth.TryGetSprite(true, facingRight, animationFrame, out var mouth)) return;
		SpriteTooth.TryGetSprite(true, facingRight, animationFrame, out var tooth);

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
				var toothRect = rect.EdgeInside(Direction4.Up, tooth.GlobalHeight);
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


	#endregion




	#region --- API ---


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


	/// <summary>
	/// Get current rect position for the given character's face
	/// </summary>
	public IRect GetFaceRect (PoseCharacterRenderer renderer, out IRect headRect) {

		var head = renderer.Head;

		using var _ = new SheetIndexScope(SheetIndex);

		// Get Head Rect
		int bounce = renderer.CurrentRenderingBounce;
		headRect = head.GetGlobalRect();
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

		return faceRect;
	}


	/// <summary>
	/// Get rect position of sclera part of the eye
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="leftEye">True if the eye is left eye</param>
	public IRect GetScleraRect (PoseCharacterRenderer renderer, bool leftEye) {
		var SpriteSclera = leftEye ? SpriteScleraLeft : SpriteScleraRight;
		bool facingRight = renderer.Head.Width > 0;
		if (!SpriteSclera.TryGetSprite(true, facingRight, renderer.CurrentAnimationFrame, out var sclera)) return default;
		var faceRect = GetFaceRect(renderer, out _);
		return faceRect.CornerInside(
			leftEye ? Alignment.TopLeft : Alignment.TopRight, sclera.GlobalWidth, sclera.GlobalHeight
		);
	}


	/// <summary>
	/// Draw face gadget for given character
	/// </summary>
	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.FaceID == 0 || !TryGetGadget(renderer.FaceID, out var face)) return;
		face.DrawGadget(renderer);
	}


	/// <summary>
	/// Draw two human style ears on both sides of the face
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Left ear sprite</param>
	/// <param name="spriteRight">Right ear sprite</param>
	/// <param name="offsetXL">Horizontal offset for left ear in global space</param>
	/// <param name="offsetXR">Horizontal offset for right ear in global space</param>
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

		spriteLeft.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var spriteL);
		Renderer.Draw(
			spriteL,
			faceRect.x + offsetXL, faceRect.yMax, 1000, 1000, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			head.FrontSide ? facingRight ? 33 : -33 : -33
		);

		spriteRight.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var spriteR);
		Renderer.Draw(
			spriteR,
			faceRect.xMax + offsetXR, faceRect.yMax, 0, 1000, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			head.FrontSide ? facingRight ? -33 : 33 : -33
		);

	}


	public static CharacterFaceExpression GetCurrentExpression (PoseCharacterRenderer renderer) {

		if (renderer.ForceFaceExpressionIndex >= 0) {
			return (CharacterFaceExpression)renderer.ForceFaceExpressionIndex.FinalValue;
		}

		// Attack
		var att = renderer.TargetCharacter.Attackness;
		if (Game.GlobalFrame < att.LastAttackFrame + 18 && att.AttackStyleIndex % 2 == 0) {
			return CharacterFaceExpression.Attack;
		}

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


	#endregion




}
