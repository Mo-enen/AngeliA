using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class Hair : BodyGadget {




	#region --- VAR ---


	// Const
	private static readonly Cell[] SINGLE_CELL = [new()];

	// Api
	public sealed override BodyGadgetType GadgetType => BodyGadgetType.Hair;
	protected virtual int FlowAmountX => 500;
	protected virtual int FlowAmountY => 500;
	public override bool SpriteLoaded => SpriteHairForward.IsValid || SpriteHairBackward.IsValid;
	public OrientedSprite SpriteHairForward { get; private set; }
	public OrientedSprite SpriteHairBackward { get; private set; }
	public OrientedSprite SpriteBraidLeft { get; private set; }
	public OrientedSprite SpriteBraidRight { get; private set; }
	protected virtual int FacingLeftOffsetX => 0;
	protected virtual bool UseLimbRotate => false;
	protected virtual bool ForceBackOnFlow => true;
	protected virtual int MotionAmount => 618;
	protected virtual int FlowMotionAmount => 618;
	protected virtual int DropMotionAmount => 200;


	#endregion




	#region --- MSG ---


	public override void DrawGadget (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		// Hair
		DrawSpriteAsHair(renderer, SpriteHairForward, SpriteHairBackward, FlowAmountX, FlowAmountY);
		// Braid
		if (SpriteBraidLeft.IsValid || SpriteBraidRight.IsValid) {
			DrawBraid(
				renderer, ForceBackOnFlow, SpriteBraidLeft, SpriteBraidRight,
				FacingLeftOffsetX, MotionAmount,
				FlowMotionAmount, DropMotionAmount, UseLimbRotate, 0, 0
			);
		}
	}


	public override void DrawGadgetGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteHairBackward.TryGetSpriteForGizmos(out var spriteB)) {
			rect = rect.Fit(spriteB);
			Renderer.Draw(spriteB, rect, tint, z);
		}
		if (SpriteHairForward.TryGetSpriteForGizmos(out var spriteF)) {
			Renderer.Draw(spriteF, rect, tint, z);
		}
	}


	#endregion




	#region --- API ---


	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHairForward = new OrientedSprite(name, "Hair", "HairF");
		SpriteHairBackward = new OrientedSprite(name, "HairB");
		SpriteBraidLeft = new OrientedSprite(name, "BraidLeft", "Braid");
		SpriteBraidRight = new OrientedSprite(name, "BraidRight", "Braid");
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.HairID != 0 && TryGetGadget(renderer.HairID, out var hair)) {
			hair.DrawGadget(renderer);
		}
	}


	public static void DrawSpriteAsHair (PoseCharacterRenderer renderer, OrientedSprite spriteForward, OrientedSprite spriteBackward, int flowAmountX, int flowAmountY) {

		var head = renderer.Head;

		// Hair Appear in Back
		if (head.FrontSide) {
			spriteBackward.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var sprite);
			var bCells = DrawHairSprite(sprite, renderer, -32, out var backHairRect);
			FlowHair(renderer, bCells, true, flowAmountX, flowAmountY);
			TwistHair(renderer, bCells, backHairRect);
			RotateHair(renderer, bCells);
		}

		// Hair Appear in Front
		spriteForward.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var fSprite);
		var fCells = DrawHairSprite(
			fSprite,
			renderer, 32, out var hairRect
		);
		FlowHair(renderer, fCells, false, flowAmountX, flowAmountY);
		TwistHair(renderer, fCells, hairRect);
		RotateHair(renderer, fCells);

		// Fix Seam
		if (fCells != null && fCells.Length > 1 && head.Rotation != 0 || renderer.HeadTwist != 0) {
			fCells[1].Width += fCells[1].Width > 0 ? 4 : -4;
			fCells[1].Height += fCells[1].Height > 0 ? 4 : -4;
		}

		// Func
		static Cell[] DrawHairSprite (AngeSprite hairSprite, PoseCharacterRenderer renderer, int z, out IRect hairRect) {

			hairRect = default;
			if (hairSprite == null) return null;

			var head = renderer.Head;
			var headRect = head.GetGlobalRect();

			// Expand Rect
			bool flipX = !head.FrontSide && head.Height < 0;
			bool flipY = head.Height < 0;
			int expandLR = hairSprite.PivotX * hairSprite.GlobalWidth / 1000;
			int expandU = (1000 - hairSprite.PivotY) * hairSprite.GlobalHeight / 1000;
			hairRect = new IRect(
				headRect.xMin - expandLR,
				headRect.yMax + expandU - hairSprite.GlobalHeight,
				headRect.width.Abs() + expandLR * 2,
				hairSprite.GlobalHeight
			);

			// Fix Position Y
			if (flipY) {
				// Upside-down
				if (hairRect.height > headRect.height) {
					hairRect.y = headRect.yMax + expandU;
					hairRect.height = headRect.height + expandU + expandU;
				} else {
					hairRect.y = headRect.y - expandU + hairRect.height;
				}
				hairRect.height *= -1;
			} else {
				// Normal
				if (hairRect.y < renderer.TargetCharacter.Y) {
					hairRect.height -= renderer.TargetCharacter.Y - hairRect.y;
					hairRect.y += renderer.TargetCharacter.Y - hairRect.y;
				}
			}

			// Flip X
			if (flipX) hairRect.FlipHorizontal();

			// Draw Hair
			return Renderer.DrawSlice(
				hairSprite,
				hairRect.CenterX(), hairRect.y + hairRect.height,
				500, 1000, 0,
				hairRect.width, hairRect.height,
				z
			);

		}
		static void FlowHair (PoseCharacterRenderer renderer, Cell[] cells, bool allCells, int amountX, int amountY) {

			if (
				cells == null || cells.Length != 9 ||
				!renderer.Head.FrontSide ||
				(cells[3].Height == 0 && cells[6].Height == 0)
			) return;

			if (amountX != 0 || amountY != 0) {

				// X
				int maxX = 30 * amountX / 1000;
				int offsetX = (-renderer.TargetCharacter.DeltaPositionX * amountX / 1000).Clamp(-maxX, maxX);
				cells[3].X += offsetX / 2;
				cells[5].X += offsetX / 2;
				cells[6].X += offsetX;
				cells[8].X += offsetX;
				if (allCells) {
					cells[4].X += offsetX / 2;
					cells[7].X += offsetX;
				}
				// Y
				int maxY = 20 * amountY / 1000;
				int offsetAmountY = 1000 + (renderer.TargetCharacter.DeltaPositionY * amountY / 10000).Clamp(-maxY, maxY) * 50;
				offsetAmountY = offsetAmountY.Clamp(800, 1200);
				cells[0].Height = cells[0].Height * offsetAmountY / 1000;
				cells[2].Height = cells[2].Height * offsetAmountY / 1000;
				cells[3].Height = cells[3].Height * offsetAmountY / 1000;
				cells[5].Height = cells[5].Height * offsetAmountY / 1000;
				cells[6].Height = cells[6].Height * offsetAmountY / 1000;
				cells[8].Height = cells[8].Height * offsetAmountY / 1000;
				if (allCells) {
					cells[1].Height = cells[1].Height * offsetAmountY / 1000;
					cells[4].Height = cells[4].Height * offsetAmountY / 1000;
					cells[7].Height = cells[7].Height * offsetAmountY / 1000;
				}

			}
		}
		static void TwistHair (PoseCharacterRenderer renderer, Cell[] cells, IRect hairRect) {
			int twist = renderer.HeadTwist;
			if (twist == 0 || cells == null || cells.Length != 9 || !renderer.Head.FrontSide) return;
			foreach (var cell in cells) cell.ReturnPivots();
			int offsetX = (hairRect.width * twist).Abs() / 4000;
			int offsetX2 = offsetX + offsetX;
			var cell0 = cells[0];
			var cell1 = cells[1];
			var cell2 = cells[2];
			var cell3 = cells[3];
			var cell4 = cells[4];
			var cell5 = cells[5];
			var cell6 = cells[6];
			var cell7 = cells[7];
			var cell8 = cells[8];
			if (twist > 0) {
				// Twist R
				cell0.X -= offsetX;
				cell3.X -= offsetX;
				cell6.X -= offsetX;
				cell0.Width = SmartAdd(cell0.Width, offsetX2);
				cell3.Width = SmartAdd(cell3.Width, offsetX2);
				cell6.Width = SmartAdd(cell6.Width, offsetX2);

				cell1.X += offsetX;
				cell4.X += offsetX;
				cell7.X += offsetX;
				cell1.Width = SmartAdd(cell1.Width, -offsetX);
				cell4.Width = SmartAdd(cell4.Width, -offsetX);
				cell7.Width = SmartAdd(cell7.Width, -offsetX);

				cell2.Width = SmartAdd(cell2.Width, -offsetX);
				cell5.Width = SmartAdd(cell5.Width, -offsetX);
				cell8.Width = SmartAdd(cell8.Width, -offsetX);
			} else {
				// Twist L
				cell0.X += offsetX;
				cell3.X += offsetX;
				cell6.X += offsetX;
				cell0.Width = SmartAdd(cell0.Width, -offsetX);
				cell3.Width = SmartAdd(cell3.Width, -offsetX);
				cell6.Width = SmartAdd(cell6.Width, -offsetX);

				cell1.Width = SmartAdd(cell1.Width, -offsetX);
				cell4.Width = SmartAdd(cell4.Width, -offsetX);
				cell7.Width = SmartAdd(cell7.Width, -offsetX);

				cell2.X -= offsetX;
				cell5.X -= offsetX;
				cell8.X -= offsetX;
				cell2.Width = SmartAdd(cell2.Width, offsetX2);
				cell5.Width = SmartAdd(cell5.Width, offsetX2);
				cell8.Width = SmartAdd(cell8.Width, offsetX2);
			}
			static int SmartAdd (int width, int offset) =>
				width > 0 == offset > 0 || offset.Abs() < width.Abs() ? width + offset : 0;
		}
		static void RotateHair (PoseCharacterRenderer renderer, Cell[] cells) {
			var head = renderer.Head;
			int headRot = head.Rotation;
			int groundRot = -renderer.Body.Rotation - headRot;
			if (cells == null) return;
			if (cells.Length == 9) {
				// 9 Slice
				foreach (var cell in cells) cell.ReturnPivots(0.5f, 1f);

				cells[0].RotateAround(headRot, head.GlobalX, head.GlobalY);
				cells[1].RotateAround(headRot, head.GlobalX, head.GlobalY);
				cells[2].RotateAround(headRot, head.GlobalX, head.GlobalY);

				cells[3].Rotation = cells[3].Rotation.LerpTo(groundRot, 0.33f);
				cells[4].Rotation = cells[4].Rotation.LerpTo(groundRot, 0.33f);
				cells[5].Rotation = cells[5].Rotation.LerpTo(groundRot, 0.33f);
				cells[3].RotateAround(headRot, head.GlobalX, head.GlobalY);
				cells[4].RotateAround(headRot, head.GlobalX, head.GlobalY);
				cells[5].RotateAround(headRot, head.GlobalX, head.GlobalY);

				cells[6].Rotation = cells[6].Rotation.LerpTo(groundRot, 0.67f);
				cells[7].Rotation = cells[7].Rotation.LerpTo(groundRot, 0.67f);
				cells[8].Rotation = cells[8].Rotation.LerpTo(groundRot, 0.67f);
				cells[6].RotateAround(headRot, head.GlobalX, head.GlobalY);
				cells[7].RotateAround(headRot, head.GlobalX, head.GlobalY);
				cells[8].RotateAround(headRot, head.GlobalX, head.GlobalY);
			} else {
				// All Cells
				foreach (var cell in cells) {
					cell.RotateAround(headRot, head.GlobalX, head.GlobalY);
				}
			}

		}
	}


	// Braid
	public static void DrawBraid (
		PoseCharacterRenderer renderer, bool forceBackOnFlow, OrientedSprite spriteLeft, OrientedSprite spriteRight,
		int facingLeftOffsetX, int motionAmount, int flowMotionAmount, int dropMotionAmount, bool useLimbRotate,
		int offsetX, int offsetY
	) {

		const int A2G = Const.CEL / Const.ART_CEL;

		var head = renderer.Head;
		bool flipX = !head.FrontSide;
		bool flipY = head.Height < 0;
		Int2 hairBL;
		Int2 hairBR;
		Int2 hairTL;
		Int2 hairTR;

		// Based on Head
		bool positiveLerp = head.Width > 0;
		hairBL = head.GlobalLerp(positiveLerp ? 0f : 1f, 0f);
		hairBR = head.GlobalLerp(positiveLerp ? 1f : 0f, 0f);
		hairTL = head.GlobalLerp(positiveLerp ? 0f : 1f, 1f);
		hairTR = head.GlobalLerp(positiveLerp ? 1f : 0f, 1f);

		spriteLeft.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var braidL);
		spriteRight.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var braidR);

		if (braidL == null && braidR == null) return;

		int posAmountXL = braidL.PivotX;
		int posAmountXR = 1000 - braidR.PivotX;
		int posAmountYL = braidL.PivotY;
		int posAmountYR = braidR.PivotY;

		var targetCharacter = renderer.TargetCharacter;
		var movement = targetCharacter.Movement;
		int lerpL = flipX ? 1000 : 0;
		int lerpR = flipX ? 0 : 1000;
		int l0 = Util.RemapUnclamped(lerpL, lerpR, hairBL.x, hairBR.x, posAmountXL);
		int l1 = Util.RemapUnclamped(lerpL, lerpR, hairTL.x, hairTR.x, posAmountXL);
		int r0 = Util.RemapUnclamped(lerpR, lerpL, hairBL.x, hairBR.x, posAmountXR);
		int r1 = Util.RemapUnclamped(lerpR, lerpL, hairTL.x, hairTR.x, posAmountXR);
		int l = Util.RemapUnclamped(0, 1000, l0, l1, posAmountYL);
		int r = Util.RemapUnclamped(0, 1000, r0, r1, posAmountYR);
		int yl = Util.RemapUnclamped(0, 1000, hairBL.y, hairTL.y, posAmountYL);
		int yr = Util.RemapUnclamped(0, 1000, hairBR.y, hairTR.y, posAmountYR);
		int rot = 0;
		int deltaHeight = 0;
		bool rolling = movement.IsRolling;
		if (!movement.FacingRight && facingLeftOffsetX != 0) {
			l += facingLeftOffsetX;
			r += facingLeftOffsetX;
		}

		int shakePosY = (renderer.PoseRootY - renderer.BasicRootY) / 2;
		yl += shakePosY;
		yr += shakePosY;

		int zLeft = braidL != null && braidL.LocalZ > 0 == head.FrontSide ? 33 : -33;
		int zRight = braidR != null && braidR.LocalZ > 0 == head.FrontSide ? 33 : -33;

		if (motionAmount != 0) {
			rot = !flipY ? (targetCharacter.DeltaPositionX * motionAmount / 1500).Clamp(-90, 90) : 0;
			deltaHeight = (targetCharacter.DeltaPositionY * motionAmount / 500).Clamp(-4 * A2G, 4 * A2G);
			int braidFlow = (targetCharacter.DeltaPositionX * flowMotionAmount / 1200).Clamp(-30, 30);
			int motionRotY = (targetCharacter.DeltaPositionY * dropMotionAmount / 1000).Clamp(-70, 0);

			var bCells = DrawBraidLogic(
				braidL, l + offsetX, yl + offsetY, zLeft, 0,
				(movement.FacingRight ? rot : rot * 2 / 3) - motionRotY,
				flipX, flipY, deltaHeight, rolling, useLimbRotate
			);
			TwistHair(renderer, bCells, false);
			Flow(bCells, movement.FacingRight ? braidFlow : braidFlow / 2, forceBackOnFlow);

			bCells = DrawBraidLogic(
				braidR, r + offsetX, yr + offsetY, zRight, 1000,
				(movement.FacingRight ? rot * 2 / 3 : rot) + motionRotY,
				flipX, flipY, deltaHeight, rolling, useLimbRotate
			);
			TwistHair(renderer, bCells, true);
			Flow(bCells, movement.FacingRight ? braidFlow / 2 : braidFlow, forceBackOnFlow);

		} else {
			var bCells = DrawBraidLogic(braidL, l + offsetX, yl + offsetY, zLeft, 0, rot, flipX, flipY, deltaHeight, rolling, useLimbRotate);
			TwistHair(renderer, bCells, false);
			bCells = DrawBraidLogic(braidR, r + offsetX, yr + offsetY, zRight, 1000, rot, flipX, flipY, deltaHeight, rolling, useLimbRotate);
			TwistHair(renderer, bCells, true);
		}

		// Func
		static Cell[] DrawBraidLogic (AngeSprite sprite, int x, int y, int z, int px, int rot, bool flipX, bool flipY, int deltaHeight, bool rolling, bool allowLimbRotate) {
			if (sprite == null) return null;
			int width = flipX ? -sprite.GlobalWidth : sprite.GlobalWidth;
			int height = flipY ? -sprite.GlobalHeight : sprite.GlobalHeight;
			if (!flipY) {
				height = (height + deltaHeight).Clamp(height / 3, height * 3);
			}
			if (rolling) height /= 2;
			int py = 1000;
			if (allowLimbRotate && !flipX && !flipY) {
				FrameworkUtil.LimbRotate(
					ref x, ref y, ref px, ref py, ref rot, ref width, ref height, rot, false, 0
				);
			}
			if (!allowLimbRotate) px = 1000 - px;
			if (sprite.GlobalBorder.IsZero) {
				SINGLE_CELL[0] = Renderer.Draw(sprite, x, y, px, py, rot, width, height, z);
				return SINGLE_CELL;
			} else {
				return Renderer.DrawSlice(sprite, x, y, px, py, rot, width, height, z);
			}
		}
		static void TwistHair (PoseCharacterRenderer renderer, Cell[] cells, bool isRight) {
			if (cells == null) return;
			// Twist
			int twist = renderer.HeadTwist;
			if (twist != 0) {
				int headCenterX = renderer.Head.GlobalX;
				foreach (var cell in cells) {
					cell.X = cell.X.LerpTo(headCenterX, twist.Abs());
					cell.Z *= isRight == twist > 0 ? -1 : 1;
				}
			}
			// Rotate
			//var body = renderer.Body;
			//int headRot = renderer.Head.Rotation;
			//int offsetY = renderer.Head.Height.Abs() * headRot.Abs() / 360;
			//foreach (var cell in cells) {
			//	cell.Rotation -= headRot;
			//	cell.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
			//	cell.Y -= offsetY;
			//}


		}
		static void Flow (Cell[] cells, int deltaRot, bool forceBackOnFlow) {
			if (cells == null || cells.Length != 9 || deltaRot == 0) return;
			// M
			for (int i = 3; i < 6; i++) {
				var cell = cells[i];
				cell.Rotation = (cell.Rotation + deltaRot / 2).Clamp(-85, 85);
				if (forceBackOnFlow) cell.Z = -33;
			}
			// D
			for (int i = 6; i < 9; i++) {
				var cell = cells[i];
				cell.Rotation = (cell.Rotation + deltaRot).Clamp(-85, 85);
				if (forceBackOnFlow) cell.Z = -33;
			}
		}
	}


	#endregion




}
