using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public sealed class DefaultHair : Hair {
	public static readonly int TYPE_ID = typeof(DefaultHair).AngeHash();
	public DefaultHair () => FillFromSheet(GetType().AngeName());
}


public sealed class ModularHair : Hair, IModularBodyGadget { }


public abstract class Hair : BodyGadget {


	// Const
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Hair;
	protected virtual int FlowAmountX => 500;
	protected virtual int FlowAmountY => 500;
	public override bool SpriteLoaded => SpriteHairForward.IsValid || SpriteHairBackward.IsValid;
	public OrientedSprite SpriteHairForward { get; private set; }
	public OrientedSprite SpriteHairBackward { get; private set; }


	// API
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHairForward = new OrientedSprite(name, "Hair", "HairF");
		SpriteHairBackward = new OrientedSprite(name, "HairB");
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.HairID != 0 && TryGetGadget(renderer.HairID, out var hair)) {
			hair.DrawGadget(renderer);
		}
	}


	public override void DrawGadget (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawSpriteAsHair(renderer, SpriteHairForward, SpriteHairBackward, FlowAmountX, FlowAmountY);
	}


	public static Cell[] DrawSpriteAsHair (PoseCharacterRenderer renderer, OrientedSprite spriteForward, OrientedSprite spriteBackward, int flowAmountX, int flowAmountY) {

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
		return fCells;

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


}
