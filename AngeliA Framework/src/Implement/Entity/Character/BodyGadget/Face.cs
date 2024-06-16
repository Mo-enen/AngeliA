using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public enum CharacterExpression {
	Normal = 0,
	Blink = 1,
	Sleep = 2,
	Damage = 3,
	PassOut = 4,
	Attack = 5,
	Suffer = 6,
}


public sealed class DefaultFace : Face {
	public static readonly int TYPE_ID = typeof(DefaultFace).AngeHash();
	public DefaultFace () => FillFromPool(GetType().AngeName());
}


public class Face : BodyGadget {


	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Face;
	private int Sprite_Eye;
	private int Sprite_Sclera;
	private int Sprite_Eyelash;
	private int Sprite_Eyebrow;
	private int Sprite_Mouth;
	private int Sprite_Tooth;


	// API
	protected override bool FillFromPool (string keyword) {

		Sprite_Eye = GetSpriteID(keyword, "Eye");
		Sprite_Sclera = GetSpriteID(keyword, "Sclera");
		Sprite_Eyelash = GetSpriteID(keyword, "Eyelash");
		Sprite_Eyebrow = GetSpriteID(keyword, "Eyebrow");
		Sprite_Mouth = GetSpriteID(keyword, "Mouth");
		Sprite_Tooth = GetSpriteID(keyword, "Tooth");

		return Sprite_Eye != 0;

		// Func
		static int GetSpriteID (string keyword, string typeName) {
			int id = $"{keyword}.Face.{typeName}".AngeHash();
			return Renderer.HasSpriteGroup(id) || Renderer.HasSprite(id) ? id : 0;
		}
	}


	public static void DrawGadgetFromPool (PoseCharacter character) {
		if (character.FaceID == 0 || !TryGetGadget(character.FaceID, out var face)) return;
		face.DrawGadget(character);
	}


	public override void DrawGadget (PoseCharacter character) {

		var head = character.Head;
		if (head.IsFullCovered || !head.FrontSide) return;

		// Get Head Rect
		int bounce = character.CurrentRenderingBounce;
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
		var expression = GetCurrentExpression(character);
		int startCellIndex = Renderer.GetUsedCellCount();

		DrawEye(character, expression, faceRect, true);
		DrawEye(character, expression, faceRect, false);
		DrawMouth(character, expression, faceRect);

		// Move with Head
		if (Renderer.GetCells(out var cells, out int count)) {

			// Twist
			int twist = character.HeadTwist;
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
			int headRot = character.HeadRotation;
			if (headRot != 0) {
				var body = character.Body;
				int offsetY = character.Head.Height.Abs() * headRot.Abs() / 360;
				for (int i = startCellIndex; i < count; i++) {
					var cell = cells[i];
					cell.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
					cell.Y -= offsetY;
				}
			}
		}
	}


	protected virtual void DrawEye (PoseCharacter character, CharacterExpression expression, IRect faceRect, bool leftEye) {

		if (
			!Renderer.TryGetSprite(Sprite_Eye, out var eye) ||
			!Renderer.TryGetSprite(Sprite_Sclera, out var sclera)
		) return;

		Renderer.TryGetSprite(Sprite_Eyebrow, out var eyebrow);
		bool facingRight = character.Head.Width > 0;
		var rect = faceRect.CornerInside(leftEye ? Alignment.TopLeft : Alignment.TopRight, sclera.GlobalWidth, sclera.GlobalHeight);
		bool eyeOpening =
			expression == CharacterExpression.Normal ||
			expression == CharacterExpression.PassOut ||
			expression == CharacterExpression.Damage;

		// Expression Redirect
		if (!Renderer.TryGetSprite(Sprite_Eyelash, out var eyelash) && !eyeOpening) {
			expression = CharacterExpression.Normal;
			eyeOpening = true;
		}

		// Check for Trigger-Lash
		if (eyeOpening && eyelash != null && eyelash.IsTrigger) {
			eyelash = null;
		}

		switch (expression) {
			case CharacterExpression.Normal: {

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
			case CharacterExpression.Blink:
			case CharacterExpression.Sleep: {
				rect.height = 1;
				break;
			}
			case CharacterExpression.Damage: {

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
			case CharacterExpression.PassOut: {

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
			case CharacterExpression.Attack:
			case CharacterExpression.Suffer: {
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


	protected virtual void DrawMouth (PoseCharacter character, CharacterExpression expression, IRect faceRect) {

		if (expression != CharacterExpression.PassOut && expression != CharacterExpression.Damage) return;
		if (!Renderer.TryGetSprite(Sprite_Mouth, out var mouth)) return;
		Renderer.TryGetSprite(Sprite_Tooth, out var tooth);

		bool facingRight = character.Head.Width > 0;
		var rect = faceRect.CornerInside(Alignment.BottomMid, mouth.GlobalWidth, mouth.GlobalHeight);

		// Animation for Damage
		if (expression == CharacterExpression.Damage) {
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


	public static void DrawSpriteAsHumanEar (PoseCharacter character, int spriteL, int spriteR, int offsetXL = -32, int offsetXR = 0) {

		// Get Face Rect
		var head = character.Head;
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
		var cellL = Renderer.Draw(
			spriteL,
			faceRect.x + offsetXL, faceRect.yMax, 1000, 1000, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			facingRight ? 33 : -33
		);
		var cellR = Renderer.Draw(
			spriteR,
			faceRect.xMax + offsetXR, faceRect.yMax, 0, 1000, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			facingRight ? -33 : 33
		);

		// Rotate
		int headRot = character.HeadRotation;
		if (headRot != 0) {
			var body = character.Body;
			int offsetY = character.Head.Height.Abs() * headRot.Abs() / 360;

			cellL.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
			cellL.Y -= offsetY;
			cellR.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
			cellR.Y -= offsetY;

		}
	}


	public static CharacterExpression GetCurrentExpression (PoseCharacter character) {

		// Attack
		if (
			character.IsAttacking &&
			character.EquippingWeaponType != WeaponType.Magic &&
			character.EquippingWeaponType != WeaponType.Ranged
		) return CharacterExpression.Attack;

		// Blink
		if (
			(Game.GlobalFrame + character.TypeID).UMod(360) <= 8 &&
			character.AnimationType != CharacterAnimationType.Sleep &&
			character.AnimationType != CharacterAnimationType.PassOut
		) return CharacterExpression.Blink;

		// Other
		return character.AnimationType switch {
			CharacterAnimationType.Sleep => CharacterExpression.Sleep,
			CharacterAnimationType.PassOut => CharacterExpression.PassOut,
			CharacterAnimationType.Crash => CharacterExpression.Suffer,
			CharacterAnimationType.TakingDamage => CharacterExpression.Damage,
			_ => CharacterExpression.Normal,
		};
	}


}
