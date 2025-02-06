namespace AngeliA;

public abstract class BraidHair : Hair {

	private static readonly Cell[] SINGLE_CELL = [new()];
	protected virtual bool GetFrontL (PoseCharacterRenderer renderer) => renderer.Head.FrontSide;
	protected virtual bool GetFrontR (PoseCharacterRenderer renderer) => renderer.Head.FrontSide;
	protected virtual int FacingLeftOffsetX => 0;
	protected virtual bool UseLimbRotate => false;
	protected virtual bool ForceBackOnFlow => true;
	protected virtual int MotionAmount => 618;
	protected virtual int FlowMotionAmount => 618;
	protected virtual int DropMotionAmount => 200;
	protected virtual int PositionAmountX => 0;
	protected virtual int PositionAmountY => 0;
	public OrientedSprite SpriteBraidLeft { get; init; }
	public OrientedSprite SpriteBraidRight { get; init; }

	protected BraidHair () : base() {
		string name = (GetType().DeclaringType ?? GetType()).AngeName();
		SpriteBraidLeft = new OrientedSprite(name, "BraidLeft", "Braid");
		SpriteBraidRight = new OrientedSprite(name, "BraidRight", "Braid");
	}

	public override void DrawGadget (PoseCharacterRenderer renderer) {
		using var _ = new SheetIndexScope(SheetIndex);
		var cells = DrawSpriteAsHair(renderer, SpriteHairForward, SpriteHairBackward, FlowAmountX, FlowAmountY);
		if (SpriteBraidLeft.IsValid || SpriteBraidRight.IsValid) {
			DrawBraid(renderer, cells, ForceBackOnFlow);
		}
	}

	private void DrawBraid (PoseCharacterRenderer renderer, Cell[] cells, bool forceBackOnFlow) => DrawBraid(
		renderer, cells, forceBackOnFlow, SpriteBraidLeft, SpriteBraidRight,
		GetFrontL(renderer) ? 33 : -33, GetFrontR(renderer) ? 33 : -33,
		PositionAmountX, PositionAmountY, FacingLeftOffsetX, MotionAmount,
		FlowMotionAmount, DropMotionAmount, UseLimbRotate, 0, 0
	);

	public static void DrawBraid (
		PoseCharacterRenderer renderer, bool forceBackOnFlow, OrientedSprite spriteLeft, OrientedSprite spriteRight,
		int zLeft, int zRight, int positionAmountX = 0, int positionAmountY = 0,
		int facingLeftOffsetX = 0, int motionAmount = 618, int flowMotionAmount = 618, int dropMotionAmount = 200, bool useLimbRotate = false,
		int offsetX = 0, int offsetY = 0
	) => DrawBraid(renderer, null, forceBackOnFlow, spriteLeft, spriteRight, zLeft, zRight, positionAmountX, positionAmountY, facingLeftOffsetX, motionAmount, flowMotionAmount, dropMotionAmount, useLimbRotate, offsetX, offsetY);

	private static void DrawBraid (
		PoseCharacterRenderer renderer, Cell[] hairCells, bool forceBackOnFlow, OrientedSprite spriteLeft, OrientedSprite spriteRight,
		int zLeft, int zRight, int positionAmountX, int positionAmountY,
		int facingLeftOffsetX, int motionAmount, int flowMotionAmount, int dropMotionAmount, bool useLimbRotate,
		int offsetX, int offsetY
	) {

		const int A2G = Const.CEL / Const.ART_CEL;

		var head = renderer.Head;
		bool flipX = !head.FrontSide;
		bool flipY = head.Height < 0;
		int hairBL;
		int hairBR;
		int hairTL;
		int hairTR;
		int hairB;
		int hairT;

		if (hairCells != null && hairCells.Length != 0) {
			// Based on Hair
			if (hairCells.Length >= 9) {
				var cellTL = hairCells[0];
				var cellTR = hairCells[2];
				var cellBL = hairCells[6];
				var cellBR = hairCells[8];
				hairBL = cellBL.X - (int)(cellBL.Width * cellBL.PivotX);
				hairBR = cellBR.X - (int)(cellBR.Width * cellBR.PivotX) + cellBR.Width;
				hairTL = cellTL.X - (int)(cellTL.Width * cellTL.PivotX);
				hairTR = cellTR.X - (int)(cellTR.Width * cellTR.PivotX) + cellTR.Width;
				hairB = cellBL.Y - (int)(cellBL.Height * cellBL.PivotY);
				hairT = cellTL.Y - (int)(cellTL.Height * cellTL.PivotY) + cellTL.Height;
			} else {
				var cell = hairCells[0];
				hairBL = hairTL = cell.X - (int)(cell.Width * cell.PivotX);
				hairBR = hairTR = hairBL + cell.Width;
				hairB = cell.Y - (int)(cell.Height * cell.PivotY);
				hairT = hairB + cell.Height;
			}
			if (hairBL > hairBR) (hairBL, hairBR) = (hairBR, hairBL);
			if (hairTL > hairTR) (hairTL, hairTR) = (hairTR, hairTL);
			if (hairB > hairT) (hairB, hairT) = (hairT, hairB);
		} else {
			// Based on Head
			var headRect = head.GetGlobalRect();
			hairBL = hairTL = headRect.x;
			hairBR = hairTR = headRect.xMax;
			hairB = headRect.y;
			hairT = headRect.yMax;
		}

		var movement = renderer.TargetCharacter.Movement;
		int lerpL = flipX ? 1000 : 0;
		int lerpR = flipX ? 0 : 1000;
		int lerpLY = flipY ? 1000 : 0;
		int lerpRY = flipY ? 0 : 1000;
		int l0 = Util.RemapUnclamped(lerpL, lerpR, hairBL, hairBR, positionAmountX);
		int l1 = Util.RemapUnclamped(lerpL, lerpR, hairTL, hairTR, positionAmountX);
		int r0 = Util.RemapUnclamped(lerpR, lerpL, hairBL, hairBR, positionAmountX);
		int r1 = Util.RemapUnclamped(lerpR, lerpL, hairTL, hairTR, positionAmountX);
		int l = Util.RemapUnclamped(lerpLY, lerpRY, l0, l1, positionAmountY);
		int r = Util.RemapUnclamped(lerpLY, lerpRY, r0, r1, positionAmountY);
		int y = Util.RemapUnclamped(lerpLY, lerpRY, hairB, hairT, positionAmountY);
		int rot = 0;
		int deltaHeight = 0;
		bool rolling = movement.IsRolling;
		if (!movement.FacingRight && facingLeftOffsetX != 0) {
			l += facingLeftOffsetX;
			r += facingLeftOffsetX;
		}

		spriteLeft.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var braidL);
		spriteRight.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var braidR);

		if (motionAmount != 0) {
			rot = !flipY ? (renderer.TargetCharacter.DeltaPositionX * motionAmount / 1500).Clamp(-90, 90) : 0;
			deltaHeight = (renderer.TargetCharacter.DeltaPositionY * motionAmount / 500).Clamp(-4 * A2G, 4 * A2G);
			int braidFlow = (renderer.TargetCharacter.DeltaPositionX * flowMotionAmount / 1200).Clamp(-30, 30);
			int motionRotY = ((renderer.TargetCharacter.DeltaPositionY * dropMotionAmount) / 1000).Clamp(-70, 0);

			var bCells = DrawBraidLogic(
				braidL, l + offsetX, y + offsetY, zLeft, 0,
				(movement.FacingRight ? rot : rot * 2 / 3) - motionRotY,
				flipX, flipY, deltaHeight, rolling, useLimbRotate
			);
			TwistRotateHair(renderer, bCells, false);
			Flow(bCells, movement.FacingRight ? braidFlow : braidFlow / 2, forceBackOnFlow);

			bCells = DrawBraidLogic(
				braidR, r + offsetX, y + offsetY, zRight, 1000,
				(movement.FacingRight ? rot * 2 / 3 : rot) + motionRotY,
				flipX, flipY, deltaHeight, rolling, useLimbRotate
			);
			TwistRotateHair(renderer, bCells, true);
			Flow(bCells, movement.FacingRight ? braidFlow / 2 : braidFlow, forceBackOnFlow);

		} else {
			var bCells = DrawBraidLogic(braidL, l + offsetX, y + offsetY, zLeft, 0, rot, flipX, flipY, deltaHeight, rolling, useLimbRotate);
			TwistRotateHair(renderer, bCells, false);
			bCells = DrawBraidLogic(braidR, r + offsetX, y + offsetY, zRight, 1000, rot, flipX, flipY, deltaHeight, rolling, useLimbRotate);
			TwistRotateHair(renderer, bCells, true);
		}

		// Func
		static Cell[] DrawBraidLogic (AngeSprite sprite, int x, int y, int z, int px, int rot, bool flipX, bool flipY, int deltaHeight, bool rolling, bool allowLimbRotate) {
			if (sprite == null) return null;
			int width = flipX ? -sprite.GlobalWidth : sprite.GlobalWidth;
			int height = flipY ? -sprite.GlobalHeight : sprite.GlobalHeight;
			height = (height + deltaHeight).Clamp(height / 3, height * 3);
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
		static void TwistRotateHair (PoseCharacterRenderer renderer, Cell[] cells, bool isRight) {
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
			int headRot = renderer.Head.Rotation;
			if (headRot != 0) {
				var body = renderer.Body;
				int offsetY = renderer.Head.Height.Abs() * headRot.Abs() / 360;
				foreach (var cell in cells) {
					cell.Rotation -= headRot / 2;
					cell.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
					cell.Y -= offsetY;
				}
			}
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

}
