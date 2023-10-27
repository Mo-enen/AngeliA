using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {



	public abstract class AutoSpriteBraidHair : AutoSpriteHair {

		private int BraidL { get; init; }
		private int BraidR { get; init; }
		protected virtual bool GetFrontL (Character character) => character.Head.FrontSide;
		protected virtual bool GetFrontR (Character character) => character.Head.FrontSide;
		protected virtual int FacingLeftOffsetX => 0;
		protected virtual bool AllowLimbRotate => false;
		protected virtual bool ForceBackOnFlow => true;
		protected virtual int MotionAmount => 618;
		protected virtual int FlowMotionAmount => 618;
		protected virtual int DropMotionAmount => 200;
		protected virtual int PositionAmountX => 0;
		protected virtual int PositionAmountY => 0;

		protected AutoSpriteBraidHair () : base() {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			BraidL = $"{name}.BraidL".AngeHash();
			BraidR = $"{name}.BraidR".AngeHash();
			if (!CellRenderer.HasSprite(BraidL)) BraidL = 0;
			if (!CellRenderer.HasSprite(BraidR)) BraidR = 0;
		}

		protected override Cell[] DrawHair (Character character) {
			var cells = base.DrawHair(character);
			if (BraidL != 0 || BraidR != 0) {
				DrawBraid(character, cells, ForceBackOnFlow);
			}
			return cells;
		}

		private void DrawBraid (Character character, Cell[] cells, bool forceBackOnFlow) {

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

			if (cells != null && cells.Length != 0) {
				// Based on Hair
				if (cells.Length >= 9) {
					var cellTL = cells[0];
					var cellTR = cells[2];
					var cellBL = cells[6];
					var cellBR = cells[8];
					hairBL = cellBL.X - (int)(cellBL.Width * cellBL.PivotX);
					hairBR = cellBR.X - (int)(cellBR.Width * cellBR.PivotX) + cellBR.Width;
					hairTL = cellTL.X - (int)(cellTL.Width * cellTL.PivotX);
					hairTR = cellTR.X - (int)(cellTR.Width * cellTR.PivotX) + cellTR.Width;
					hairB = cellBL.Y - (int)(cellBL.Height * cellBL.PivotY);
					hairT = cellTL.Y - (int)(cellTL.Height * cellTL.PivotY) + cellTL.Height;
				} else {
					var cell = cells[0];
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

			int zLeft = GetFrontL(character) ? 33 : -33;
			int zRight = GetFrontR(character) ? 33 : -33;
			int lerpL = flipX ? 1000 : 0;
			int lerpR = flipX ? 0 : 1000;
			int lerpLY = flipY ? 1000 : 0;
			int lerpRY = flipY ? 0 : 1000;
			int l0 = Util.RemapUnclamped(lerpL, lerpR, hairBL, hairBR, PositionAmountX);
			int l1 = Util.RemapUnclamped(lerpL, lerpR, hairTL, hairTR, PositionAmountX);
			int r0 = Util.RemapUnclamped(lerpR, lerpL, hairBL, hairBR, PositionAmountX);
			int r1 = Util.RemapUnclamped(lerpR, lerpL, hairTL, hairTR, PositionAmountX);
			int l = Util.RemapUnclamped(lerpLY, lerpRY, l0, l1, PositionAmountY);
			int r = Util.RemapUnclamped(lerpLY, lerpRY, r0, r1, PositionAmountY);
			int y = Util.RemapUnclamped(lerpLY, lerpRY, hairB, hairT, PositionAmountY);
			int rot = 0;
			int deltaHeight = 0;
			bool rolling = character.IsRolling;
			if (!character.FacingRight && FacingLeftOffsetX != 0) {
				l += FacingLeftOffsetX;
				r += FacingLeftOffsetX;
			}
			if (MotionAmount != 0) {
				rot = !flipY ? (character.DeltaPositionX * MotionAmount / 1500).Clamp(-90, 90) : 0;
				deltaHeight = (character.DeltaPositionY * MotionAmount / 500).Clamp(-4 * A2G, 4 * A2G);
				int braidFlow = (character.DeltaPositionX * FlowMotionAmount / 1200).Clamp(-30, 30);
				int motionRotY = ((character.DeltaPositionY * DropMotionAmount) / 1000).Clamp(-70, 0);

				var bcells = DrawBraid(
					BraidL, l, y, zLeft, 0,
					(character.FacingRight ? rot : rot * 2 / 3) - motionRotY,
					flipX, flipY, deltaHeight, rolling, AllowLimbRotate
				);
				Flow(bcells, character.FacingRight ? braidFlow : braidFlow / 2, forceBackOnFlow);

				bcells = DrawBraid(
					BraidR, r, y, zRight, 1000,
					(character.FacingRight ? rot * 2 / 3 : rot) + motionRotY,
					flipX, flipY, deltaHeight, rolling, AllowLimbRotate
				);
				Flow(bcells, character.FacingRight ? braidFlow / 2 : braidFlow, forceBackOnFlow);

			} else {
				DrawBraid(BraidL, l, y, zLeft, 0, rot, flipX, flipY, deltaHeight, rolling, AllowLimbRotate);
				DrawBraid(BraidR, r, y, zRight, 1000, rot, flipX, flipY, deltaHeight, rolling, AllowLimbRotate);
			}

			// Func
			static Cell[] DrawBraid (
				int spriteID, int x, int y, int z,
				int px, int rot, bool flipX, bool flipY, int deltaHeight, bool rolling, bool allowLimbRotate
			) {
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
					CellRenderer.Draw(spriteID, x, y, px, py, rot, width, height, z);
					return null;
				} else {
					return CellRenderer.Draw_9Slice(spriteID, x, y, px, py, rot, width, height, z);
				}
			}
			static void Flow (Cell[] cells, int deltaRot, bool forceBackOnFlow) {
				if (cells == null || deltaRot == 0) return;
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


	public abstract class AutoSpriteHair : Hair {

		private int SpriteFFL { get; init; }
		private int SpriteFFR { get; init; }
		private int SpriteFB { get; init; }
		private int SpriteBF { get; init; }

		public AutoSpriteHair () {
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

		protected override Cell[] DrawHair (Character character) => DrawFlowSprite(character, SpriteFFL, SpriteFFR, SpriteFB, SpriteBF);

	}


	public abstract class Hair {


		// Const
		private const int A2G = Const.CEL / Const.ART_CEL;

		// Data
		private static readonly Dictionary<int, Hair> Pool = new();
		private static readonly Dictionary<int, int> DefaultPool = new();


		// MSG
		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(Hair).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Hair hair) continue;
				int id = type.AngeHash();
				Pool.TryAdd(id, hair);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(charType)) {
					DefaultPool.TryAdd(dType.AngeHash(), id);
				}
			}
		}


		// API
		public static void Draw (Character character) => Draw(character, out _);
		public static void Draw (Character character, out Hair hair) {
			hair = null;
			if (
				character.HairID != 0 &&
				Pool.TryGetValue(character.HairID, out hair)
			) {
				hair.DrawHair(character);
			}
		}


		public static bool TryGetDefaultHairID (int characterID, out int hairID) => DefaultPool.TryGetValue(characterID, out hairID);


		protected abstract Cell[] DrawHair (Character character);


		protected static Cell[] DrawFlowSprite (Character character, int spriteFrontFL, int spriteFrontFR, int spriteFrontB, int spriteBackF) {

			// Back Hair
			if (character.Head.FrontSide) {
				var bCells = DrawSprite(spriteBackF, character, -32, out var backHairRect);
				TwistHair(character, bCells, backHairRect);
				MakeHairFlow(character, bCells, true);
			}

			// Front Hair
			var fCells = DrawSprite(
				!character.Head.FrontSide ? spriteFrontB :
				character.Head.Width > 0 ? spriteFrontFR : spriteFrontFL,
				character, 32, out var hairRect
			);
			TwistHair(character, fCells, hairRect);
			MakeHairFlow(character, fCells, false);
			return fCells;

			// Func
			static Cell[] DrawSprite (int spriteID, Character character, int z, out RectInt hairRect) {

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
				hairRect = new RectInt(
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
				return CellRenderer.Draw_9Slice(hairSprite.GlobalID, hairRect, character.HairColor, z);

			}
			static void TwistHair (Character character, Cell[] cells, RectInt hairRect) {
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
			static void MakeHairFlow (Character character, Cell[] cells, bool strech) {

				if (
					cells == null || cells.Length != 9 ||
					!character.Head.FrontSide ||
					(cells[3].Height == 0 && cells[6].Height == 0)
				) return;

				int flowAmountX = 0;
				int flowAmountY = 0;
				int basicRootY = character.BasicRootY;
				switch (character.AnimatedPoseType) {
					case CharacterPoseAnimationType.SwimMove:
						flowAmountX = 50;
						flowAmountY = 10;
						break;
					case CharacterPoseAnimationType.Dash:
						flowAmountX = 100;
						flowAmountY = 20;
						break;
					case CharacterPoseAnimationType.Rush:
						flowAmountX = 140;
						flowAmountY = 20;
						break;
					case CharacterPoseAnimationType.Walk:
						if (character.PoseRootY < basicRootY + A2G / 4) {
							flowAmountX = 30;
						} else if (character.PoseRootY < basicRootY + A2G / 2) {
							flowAmountX = 50;
							flowAmountY = 20;
						} else {
							flowAmountX = 80;
							flowAmountY = 30;
						}
						break;
					case CharacterPoseAnimationType.Run:
						if (character.PoseRootY < basicRootY + A2G / 2) {
							flowAmountX = 60;
						} else if (character.PoseRootY < basicRootY + A2G) {
							flowAmountX = 80;
							flowAmountY = 30;
						} else {
							flowAmountX = 100;
							flowAmountY = 40;
						}
						break;
				}
				if (flowAmountX != 0 || flowAmountY != 0) {
					bool facingRight = character.Head.Width > 0;
					int leftX = facingRight ? -flowAmountX : flowAmountX / 2;
					int rightX = facingRight ? -flowAmountX / 2 : flowAmountX;
					MoveCell(cells[3], leftX / 2, flowAmountY, strech);
					MoveCell(cells[5], rightX / 2, flowAmountY, strech);
					MoveCell(cells[6], leftX, flowAmountY, strech);
					MoveCell(cells[8], rightX, flowAmountY, strech);
				}
				static void MoveCell (Cell cell, int amountX, int amountY, bool strech) {
					if (strech) {
						int deltaX = Const.CEL * amountX / 1000;
						cell.ReturnPivots();
						if (amountX < 0) {
							cell.X += deltaX;
							cell.Width -= deltaX;
						} else {
							cell.Width += deltaX;
						}
						cell.Y += Const.CEL * amountY / 1000;
					} else {
						cell.X += Const.CEL * amountX / 1000;
						cell.Y += Const.CEL * amountY / 1000;
					}
				}
			}
		}


	}
}
