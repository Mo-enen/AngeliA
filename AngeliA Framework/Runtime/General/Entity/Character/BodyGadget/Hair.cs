using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {



	public abstract class BraidHair : Hair {

		private static readonly Cell[] SINGLE_CELL = new Cell[] { new Cell() };
		private int BraidL { get; init; }
		private int BraidR { get; init; }
		protected virtual bool GetFrontL (PoseCharacter character) => character.Head.FrontSide;
		protected virtual bool GetFrontR (PoseCharacter character) => character.Head.FrontSide;
		protected virtual int FacingLeftOffsetX => 0;
		protected virtual bool UseLimbRotate => false;
		protected virtual bool ForceBackOnFlow => true;
		protected virtual int MotionAmount => 618;
		protected virtual int FlowMotionAmount => 618;
		protected virtual int DropMotionAmount => 200;
		protected virtual int PositionAmountX => 0;
		protected virtual int PositionAmountY => 0;

		protected BraidHair () : base() {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			BraidL = $"{name}.BraidL".AngeHash();
			BraidR = $"{name}.BraidR".AngeHash();
			if (!CellRenderer.HasSprite(BraidL)) BraidL = 0;
			if (!CellRenderer.HasSprite(BraidR)) BraidR = 0;
		}

		public override void DrawGadget (PoseCharacter character) {
			var cells = DrawSpriteAsHair(character, SpriteFFL, SpriteFFR, SpriteFB, SpriteBF, FlowAmountX, FlowAmountY);
			if (Game.GlobalFrame > character.HideBraidFrame && (BraidL != 0 || BraidR != 0)) {
				DrawBraid(character, cells, ForceBackOnFlow);
			}
		}

		private void DrawBraid (PoseCharacter character, Cell[] cells, bool forceBackOnFlow) => DrawBraid(
			character, cells, forceBackOnFlow, BraidL, BraidR,
			GetFrontL(character) ? 33 : -33, GetFrontR(character) ? 33 : -33,
			PositionAmountX, PositionAmountY, FacingLeftOffsetX, MotionAmount,
			FlowMotionAmount, DropMotionAmount, UseLimbRotate, 0, 0
		);

		public static void DrawBraid (
			PoseCharacter character, bool forceBackOnFlow, int braidL, int braidR,
			int zLeft, int zRight, int positionAmountX = 0, int positionAmountY = 0,
			int facingLeftOffsetX = 0, int motionAmount = 618, int flowMotionAmount = 618, int dropMotionAmount = 200, bool useLimbRotate = false,
			int offsetX = 0, int offsetY = 0
		) => DrawBraid(character, null, forceBackOnFlow, braidL, braidR, zLeft, zRight, positionAmountX, positionAmountY, facingLeftOffsetX, motionAmount, flowMotionAmount, dropMotionAmount, useLimbRotate, offsetX, offsetY);

		private static void DrawBraid (
			PoseCharacter character, Cell[] hairCells, bool forceBackOnFlow, int braidL, int braidR,
			int zLeft, int zRight, int positionAmountX, int positionAmountY,
			int facingLeftOffsetX, int motionAmount, int flowMotionAmount, int dropMotionAmount, bool useLimbRotate,
			int offsetX, int offsetY
		) {

			const int A2G = Const.CEL / Const.ART_CEL;

			var head = character.Head;
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
			bool rolling = character.IsRolling;
			if (!character.FacingRight && facingLeftOffsetX != 0) {
				l += facingLeftOffsetX;
				r += facingLeftOffsetX;
			}
			if (motionAmount != 0) {
				rot = !flipY ? (character.DeltaPositionX * motionAmount / 1500).Clamp(-90, 90) : 0;
				deltaHeight = (character.DeltaPositionY * motionAmount / 500).Clamp(-4 * A2G, 4 * A2G);
				int braidFlow = (character.DeltaPositionX * flowMotionAmount / 1200).Clamp(-30, 30);
				int motionRotY = ((character.DeltaPositionY * dropMotionAmount) / 1000).Clamp(-70, 0);

				var bCells = DrawBraid(
					braidL, l + offsetX, y + offsetY, zLeft, 0,
					(character.FacingRight ? rot : rot * 2 / 3) - motionRotY,
					flipX, flipY, deltaHeight, rolling, useLimbRotate
				);
				TwistRotateHair(character, bCells, false);
				Flow(bCells, character.FacingRight ? braidFlow : braidFlow / 2, forceBackOnFlow);

				bCells = DrawBraid(
					braidR, r + offsetX, y + offsetY, zRight, 1000,
					(character.FacingRight ? rot * 2 / 3 : rot) + motionRotY,
					flipX, flipY, deltaHeight, rolling, useLimbRotate
				);
				TwistRotateHair(character, bCells, true);
				Flow(bCells, character.FacingRight ? braidFlow / 2 : braidFlow, forceBackOnFlow);

			} else {
				var bCells = DrawBraid(braidL, l + offsetX, y + offsetY, zLeft, 0, rot, flipX, flipY, deltaHeight, rolling, useLimbRotate);
				TwistRotateHair(character, bCells, false);
				bCells = DrawBraid(braidR, r + offsetX, y + offsetY, zRight, 1000, rot, flipX, flipY, deltaHeight, rolling, useLimbRotate);
				TwistRotateHair(character, bCells, true);
			}

			// Func
			static Cell[] DrawBraid (int spriteID, int x, int y, int z, int px, int rot, bool flipX, bool flipY, int deltaHeight, bool rolling, bool allowLimbRotate) {
				if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return null;
				int width = flipX ? -sprite.GlobalWidth : sprite.GlobalWidth;
				int height = flipY ? -sprite.GlobalHeight : sprite.GlobalHeight;
				height = (height + deltaHeight).Clamp(height / 3, height * 3);
				if (rolling) height /= 2;
				int py = 1000;
				if (allowLimbRotate && !flipX && !flipY) {
					AngeUtil.LimbRotate(
						ref x, ref y, ref px, ref py, ref rot, ref width, ref height, rot, false, 0
					);
				}
				if (!allowLimbRotate) px = 1000 - px;
				if (sprite.GlobalBorder.IsZero) {
					SINGLE_CELL[0] = CellRenderer.Draw(spriteID, x, y, px, py, rot, width, height, z);
					return SINGLE_CELL;
				} else {
					return CellRenderer.Draw_9Slice(spriteID, x, y, px, py, rot, width, height, z);
				}
			}
			static void TwistRotateHair (PoseCharacter character, Cell[] cells, bool isRight) {
				if (cells == null) return;
				// Twist
				int twist = character.HeadTwist;
				if (twist != 0) {
					int headCenterX = character.Head.GlobalX;
					foreach (var cell in cells) {
						cell.X = cell.X.LerpTo(headCenterX, twist.Abs());
						cell.Z *= isRight == twist > 0 ? -1 : 1;
					}
				}
				// Rotate
				int headRot = character.HeadRotation;
				if (headRot != 0) {
					var body = character.Body;
					int offsetY = character.Head.Height.Abs() * headRot.Abs() / 360;
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


	public abstract class Hair : BodyGadget {


		// Const
		protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Hair;
		protected virtual int FlowAmountX => 500;
		protected virtual int FlowAmountY => 500;
		protected int SpriteFFL { get; init; }
		protected int SpriteFFR { get; init; }
		protected int SpriteFB { get; init; }
		protected int SpriteBF { get; init; }


		// API
		public Hair () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteFFL = $"{name}.HairFFL".AngeHash();
			SpriteFFR = $"{name}.HairFFR".AngeHash();
			SpriteFB = $"{name}.HairFB".AngeHash();
			SpriteBF = $"{name}.HairBF".AngeHash();
			if (!CellRenderer.HasSprite(SpriteFFL)) SpriteFFL = 0;
			if (!CellRenderer.HasSprite(SpriteFFR)) SpriteFFR = 0;
			if (!CellRenderer.HasSprite(SpriteFB)) SpriteFB = 0;
			if (!CellRenderer.HasSprite(SpriteBF)) SpriteBF = 0;
		}


		public static void DrawGadgetFromPool (PoseCharacter character) {
			if (character.HairID != 0 && TryGetGadget(character.HairID, out var hair)) {
				hair.DrawGadget(character);
			}
		}


		public override void DrawGadget (PoseCharacter character) => DrawSpriteAsHair(character, SpriteFFL, SpriteFFR, SpriteFB, SpriteBF, FlowAmountX, FlowAmountY);


		public static Cell[] DrawSpriteAsHair (PoseCharacter character, int spriteFrontFL, int spriteFrontFR, int spriteFrontB, int spriteBackF, int flowAmountX, int flowAmountY) {

			// Back Hair
			if (character.Head.FrontSide) {
				var bCells = DrawSprite(spriteBackF, character, -32, out var backHairRect);
				FlowHair(character, bCells, true, flowAmountX, flowAmountY);
				TwistHair(character, bCells, backHairRect);
				RotateHair(character, bCells);
			}

			// Front Hair
			var fCells = DrawSprite(
				!character.Head.FrontSide ? spriteFrontB :
				character.Head.Width > 0 ? spriteFrontFR : spriteFrontFL,
				character, 32, out var hairRect
			);
			FlowHair(character, fCells, false, flowAmountX, flowAmountY);
			TwistHair(character, fCells, hairRect);
			RotateHair(character, fCells);
			return fCells;

			// Func
			static Cell[] DrawSprite (int spriteID, PoseCharacter character, int z, out IRect hairRect) {

				hairRect = default;
				if (spriteID == 0) return null;

				var head = character.Head;

				if (!CellRenderer.TryGetSprite(spriteID, out var hairSprite)) return null;

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
					if (hairRect.y < character.Y) {
						hairRect.height -= character.Y - hairRect.y;
						hairRect.y += character.Y - hairRect.y;
					}
				}

				// Flip X
				if (flipX) hairRect.FlipHorizontal();

				// Draw Hair
				return CellRenderer.Draw_9Slice(
					hairSprite.GlobalID,
					hairRect.CenterX(), hairRect.y + hairRect.height,
					500, 1000, 0,
					hairRect.width, hairRect.height,
					character.HairColor, z
				);

			}
			static void FlowHair (PoseCharacter character, Cell[] cells, bool allCells, int amountX, int amountY) {

				if (
					cells == null || cells.Length != 9 ||
					!character.Head.FrontSide ||
					(cells[3].Height == 0 && cells[6].Height == 0)
				) return;

				if (amountX != 0 || amountY != 0) {

					// X
					int maxX = 30 * amountX / 1000;
					int offsetX = (-character.DeltaPositionX * amountX / 1000).Clamp(-maxX, maxX);
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
					int offsetAmountY = 1000 + (character.DeltaPositionY * amountY / 10000).Clamp(-maxY, maxY) * 50;
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
			static void TwistHair (PoseCharacter character, Cell[] cells, IRect hairRect) {
				int twist = character.HeadTwist;
				if (twist == 0 || cells == null || cells.Length != 9 || !character.Head.FrontSide) return;
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
			static void RotateHair (PoseCharacter character, Cell[] cells) {
				int headRot = character.HeadRotation;
				if (headRot == 0 || cells == null) return;
				var body = character.Body;
				int offsetY = character.Head.Height.Abs() * headRot.Abs() / 360;
				if (cells.Length == 9) {
					// 9 Slice
					foreach (var cell in cells) cell.ReturnPivots(0.5f, 1f);

					RotateCell(cells[0], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					RotateCell(cells[1], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					RotateCell(cells[2], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);

					cells[3].Rotation -= headRot / 3;
					cells[4].Rotation -= headRot / 3;
					cells[5].Rotation -= headRot / 3;
					RotateCell(cells[3], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					RotateCell(cells[4], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					RotateCell(cells[5], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);

					cells[6].Rotation -= headRot / 2;
					cells[7].Rotation -= headRot / 2;
					cells[8].Rotation -= headRot / 2;
					RotateCell(cells[6], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					RotateCell(cells[7], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					RotateCell(cells[8], headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
				} else {
					// All Cells
					foreach (var cell in cells) {
						RotateCell(cell, headRot, body.GlobalX, body.GlobalY + body.Height, offsetY);
					}
				}
				// Func
				static void RotateCell (Cell _cell, int rotation, int pointX, int pointY, int offsetY) {
					_cell.RotateAround(rotation, pointX, pointY);
					_cell.Y -= offsetY;
				}
			}
		}


	}
}
