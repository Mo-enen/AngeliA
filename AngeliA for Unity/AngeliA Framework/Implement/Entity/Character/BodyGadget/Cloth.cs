using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	public enum ClothType { Head, Body, Hand, Hip, Foot, }

	public enum FrontMode { Front, Back, AlwaysFront, AlwaysBack, }





	[RequireSprite("{1}.HeadSuit")]
	[RequireLanguage("{1}.Head")]
	public abstract class HeadCloth : Cloth {

		protected sealed override ClothType ClothType => ClothType.Head;
		protected virtual FrontMode Front => FrontMode.Front;
		protected virtual bool PixelShiftForLeft => true;
		private int SpriteID { get; init; } = 0;


		// MSG
		public HeadCloth () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteID = $"{name}.HeadSuit".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID) && !CellRenderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		}

		public static void DrawClothFromPool (PoseCharacter character) {
			if (character.Suit_Head != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.Suit_Head, out var cloth)) {
				cloth.Draw(character);
			}
		}

		public override void Draw (PoseCharacter character) => DrawClothForHead(character, SpriteID, Front, PixelShiftForLeft);

		public static void DrawClothForHead (PoseCharacter character, int spriteGroupID, FrontMode frontMode, bool pixelShiftForLeft) {

			var head = character.Head;
			if (spriteGroupID == 0 || head.IsFullCovered) return;

			// Width Amount
			int widthAmount = 1000;
			if (character.HeadTwist != 0) widthAmount -= character.HeadTwist.Abs() / 2;
			if (head.Height < 0) widthAmount = -widthAmount;

			// Draw
			Cell[] cells = null;
			if (CellRenderer.HasSpriteGroup(spriteGroupID)) {
				if (head.FrontSide) {
					// Front
					bool front = frontMode != FrontMode.AlwaysBack && frontMode != FrontMode.Back;
					if (CellRenderer.TryGetSpriteFromGroup(spriteGroupID, 0, out var sprite, false, true)) {
						bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
						cells = AttachClothOn(
							head, sprite, 500, 1000,
							(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
							usePixelShift ? (front ? -16 : 16) : 0, 0
						);
					}
				} else {
					// Back
					if (CellRenderer.TryGetSpriteFromGroup(spriteGroupID, 1, out var sprite, false, true)) {
						bool front = frontMode != FrontMode.AlwaysBack && frontMode != FrontMode.Front;
						bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
						cells = AttachClothOn(
							head, sprite, 500, 1000,
							(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
							usePixelShift ? (front ? -16 : 16) : 0, 0
						);
					}
				}
			} else if (CellRenderer.TryGetSprite(spriteGroupID, out var sprite)) {
				// Single Sprite
				bool front = frontMode != FrontMode.AlwaysBack && (
					frontMode == FrontMode.AlwaysFront ||
					frontMode == FrontMode.Front == head.FrontSide
				);
				bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
				cells = AttachClothOn(
					head, sprite, 500, 1000,
					(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
					usePixelShift ? (front ? -16 : 16) : 0, 0
				);
			}
			// Head Rotate
			if (cells != null && character.HeadRotation != 0) {
				int offsetY = character.Head.Height.Abs() * character.HeadRotation.Abs() / 360;
				foreach (var cell in cells) {
					cell.RotateAround(character.HeadRotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
					cell.Y -= offsetY;
				}
			}
		}

	}



	[RequireSprite("{1}.BodySuit", "{1}.BodySuitL", "{1}.BodySuitR", "{1}.ShoulderSuit", "{1}.UpperArmSuit", "{1}.LowerArmSuit")]
	[RequireLanguage("{1}.Body")]
	public abstract class BodyCloth : Cloth {

		protected sealed override ClothType ClothType => ClothType.Body;
		private int SpriteIdFrontL { get; init; }
		private int SpriteIdFrontR { get; init; }
		private int SpriteIdShoulder { get; init; }
		private int SpriteIdUpperArm { get; init; }
		private int SpriteIdLowerArm { get; init; }
		protected virtual int TwistShiftTopAmount => 300;
		protected virtual int LocalZ => 7;

		public BodyCloth () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteIdFrontL = SpriteIdFrontR = $"{name}.BodySuit".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdFrontL) && !CellRenderer.HasSpriteGroup(SpriteIdFrontL)) SpriteIdFrontL = 0;
			if (SpriteIdFrontL == 0) {
				SpriteIdFrontL = $"{name}.BodySuitL".AngeHash();
				SpriteIdFrontR = $"{name}.BodySuitR".AngeHash();
				if (!CellRenderer.HasSprite(SpriteIdFrontL) && !CellRenderer.HasSpriteGroup(SpriteIdFrontL)) SpriteIdFrontL = 0;
				if (!CellRenderer.HasSprite(SpriteIdFrontR) && !CellRenderer.HasSpriteGroup(SpriteIdFrontR)) SpriteIdFrontR = SpriteIdFrontL;
			}
			SpriteIdShoulder = $"{name}.ShoulderSuit".AngeHash();
			SpriteIdUpperArm = $"{name}.UpperArmSuit".AngeHash();
			SpriteIdLowerArm = $"{name}.LowerArmSuit".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdShoulder) && !CellRenderer.HasSpriteGroup(SpriteIdShoulder)) SpriteIdShoulder = 0;
			if (!CellRenderer.HasSprite(SpriteIdUpperArm) && !CellRenderer.HasSpriteGroup(SpriteIdUpperArm)) SpriteIdUpperArm = 0;
			if (!CellRenderer.HasSprite(SpriteIdLowerArm) && !CellRenderer.HasSpriteGroup(SpriteIdLowerArm)) SpriteIdLowerArm = 0;
		}

		public static void DrawClothFromPool (PoseCharacter character) {
			if (character.Suit_Body != 0 && Pool.TryGetValue(character.Suit_Body, out var cloth)) {
				cloth.Draw(character);
			}
		}

		public override void Draw (PoseCharacter character) {
			DrawClothForBody(character, SpriteIdFrontL, SpriteIdFrontR, LocalZ, TwistShiftTopAmount);
			DrawCape(character, TypeID);
			DrawClothForShoulder(character, SpriteIdShoulder);
			DrawClothForUpperArm(character, SpriteIdUpperArm);
			DrawClothForLowerArm(character, SpriteIdLowerArm);
		}

		public static void DrawClothForBody (PoseCharacter character, int spriteIdFrontL, int spriteIdFrontR, int localZ, int twistShiftTopAmount) {

			if (spriteIdFrontL == 0 && spriteIdFrontR == 0) return;

			var body = character.Body;
			bool facingRight = body.Width > 0;
			bool separatedSprite = spriteIdFrontL != spriteIdFrontR;
			int spriteGroupId = facingRight ? spriteIdFrontR : spriteIdFrontL;
			if (spriteGroupId == 0 || body.IsFullCovered) return;

			var hip = character.Hip;
			int poseTwist = character.BodyTwist;
			int groupIndex = body.FrontSide ? 0 : 1;
			if (!CellRenderer.TryGetSpriteFromGroup(spriteGroupId, groupIndex, out var suitSprite, false, true)) return;

			var rect = new IRect(
				body.GlobalX - body.Width / 2,
				hip.GlobalY,
				body.Width,
				body.Height + hip.Height
			);

			// Border
			if (!suitSprite.GlobalBorder.IsZero) {
				if (rect.width > 0) {
					rect = rect.Expand(
						suitSprite.GlobalBorder.left,
						suitSprite.GlobalBorder.right,
						suitSprite.GlobalBorder.down,
						suitSprite.GlobalBorder.up
					);
				} else {
					rect = rect.Expand(
						-suitSprite.GlobalBorder.left,
						-suitSprite.GlobalBorder.right,
						suitSprite.GlobalBorder.down,
						suitSprite.GlobalBorder.up
					);
				}
			}

			// Flip
			bool flipX = separatedSprite && !facingRight;
			if (flipX) rect.FlipHorizontal();

			// Draw
			var cell = CellRenderer.Draw(suitSprite, rect, body.Z + localZ);

			// Twist
			if (poseTwist != 0 && body.FrontSide && body.Height > 0) {
				if (flipX) poseTwist = -poseTwist;
				int shiftTop = body.Height * twistShiftTopAmount / 1000;
				int shiftX = poseTwist * cell.Width / 2500;
				var cellL = CellRenderer.Draw(Const.PIXEL, default);
				cellL.CopyFrom(cell);
				var cellR = CellRenderer.Draw(Const.PIXEL, default);
				cellR.CopyFrom(cell);
				cellL.Shift.up = cellR.Shift.up = shiftTop;
				cellL.Width += body.Width.Sign() * shiftX;
				cellL.Shift.right = cellL.Width.Abs() / 2;
				cellR.Width -= body.Width.Sign() * shiftX;
				cellR.X = cellL.X + cellL.Width / 2 - cellR.Width / 2;
				cellR.Shift.left = cellR.Width.Abs() / 2;
				cell.Shift.down = cell.Height - shiftTop;
			}

			// Hide Limb
			body.Covered = suitSprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
				 BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		}

		public static void DrawClothForShoulder (PoseCharacter character, int spriteID) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					CoverClothOn(character.ShoulderL, spriteL.GlobalID);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					CoverClothOn(character.ShoulderR, spriteR.GlobalID);
				}
			} else {
				CoverClothOn(character.ShoulderL, spriteID);
				CoverClothOn(character.ShoulderR, spriteID);
			}
		}

		public static void DrawClothForUpperArm (PoseCharacter character, int spriteID, int localZ = 1) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					CoverClothOn(character.UpperArmL, spriteL.GlobalID, localZ);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					CoverClothOn(character.UpperArmR, spriteR.GlobalID, localZ);
				}
			} else {
				CoverClothOn(character.UpperArmL, spriteID, localZ);
				CoverClothOn(character.UpperArmR, spriteID, localZ);
			}
		}

		public static void DrawClothForLowerArm (PoseCharacter character, int spriteID, int localZ = 1) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					CoverClothOn(character.LowerArmL, spriteL.GlobalID, localZ);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					CoverClothOn(character.LowerArmR, spriteR.GlobalID, localZ);
				}
			} else {
				CoverClothOn(character.LowerArmL, spriteID, localZ);
				CoverClothOn(character.LowerArmR, spriteID, localZ);
			}
		}

		public static void DrawCape (PoseCharacter character, int capeID, int motionAmount = 1000) {
			if (capeID == 0) return;
			if (!CellRenderer.TryGetSpriteGroup(capeID, out var group) || group.Length < 4) return;
			var sprite = group[character.Body.FrontSide ? 2 : 3];
			var animatedPoseType = character.AnimationType;
			if (
				animatedPoseType == CharacterAnimationType.SquatIdle ||
				animatedPoseType == CharacterAnimationType.SquatMove ||
				animatedPoseType == CharacterAnimationType.Dash ||
				animatedPoseType == CharacterAnimationType.Rolling ||
				animatedPoseType == CharacterAnimationType.Spin ||
				animatedPoseType == CharacterAnimationType.Fly ||
				animatedPoseType == CharacterAnimationType.Sleep ||
				animatedPoseType == CharacterAnimationType.Crash ||
				animatedPoseType == CharacterAnimationType.PassOut
			) return;
			DrawCape(character, sprite, motionAmount);
			// Func
			static void DrawCape (PoseCharacter character, AngeSprite sprite, int motionAmount = 1000) {

				var body = character.Body;

				// Draw
				int height = sprite.GlobalHeight + body.Height.Abs() - body.SizeY;
				var cells = CellRenderer.Draw_9Slice(
					sprite,
					body.GlobalX, body.GlobalY + body.Height,
					500, 1000, 0,
					sprite.GlobalWidth,
					body.Height.Sign() * height,
					Const.WHITE, body.FrontSide ? -31 : 31
				);

				// Flow Motion
				if (motionAmount != 0) {
					// X
					int maxX = 30 * motionAmount / 1000;
					int offsetX = (-character.DeltaPositionX * motionAmount / 1000).Clamp(-maxX, maxX);
					cells[3].X += offsetX / 2;
					cells[4].X += offsetX / 2;
					cells[5].X += offsetX / 2;
					cells[6].X += offsetX;
					cells[7].X += offsetX;
					cells[8].X += offsetX;
					// Y
					int maxY = 20 * motionAmount / 1000;
					int offsetAmountY = 1000 + (character.DeltaPositionY * motionAmount / 10000).Clamp(-maxY, maxY) * 1000 / 20;
					offsetAmountY = offsetAmountY.Clamp(800, 1200);
					cells[0].Height = cells[0].Height * offsetAmountY / 1000;
					cells[1].Height = cells[1].Height * offsetAmountY / 1000;
					cells[2].Height = cells[2].Height * offsetAmountY / 1000;
					cells[3].Height = cells[3].Height * offsetAmountY / 1000;
					cells[4].Height = cells[4].Height * offsetAmountY / 1000;
					cells[5].Height = cells[5].Height * offsetAmountY / 1000;
					cells[6].Height = cells[6].Height * offsetAmountY / 1000;
					cells[7].Height = cells[7].Height * offsetAmountY / 1000;
					cells[8].Height = cells[8].Height * offsetAmountY / 1000;
				}
			}
		}

	}



	[RequireSprite("{1}.HipSuit", "{1}.SkirtSuit", "{1}.UpperLegSuit", "{1}.LowerLegSuit")]
	[RequireLanguage("{1}.Hip")]
	public abstract class HipCloth : Cloth {

		protected sealed override ClothType ClothType => ClothType.Hip;
		protected virtual bool CoverLegs => true;
		private int SpriteIdHip { get; init; }
		private int SpriteIdSkirt { get; init; }
		private int SpriteIdUpperLeg { get; init; }
		private int SpriteIdLowerLeg { get; init; }

		public HipCloth () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteIdHip = $"{name}.HipSuit".AngeHash();
			SpriteIdSkirt = $"{name}.SkirtSuit".AngeHash();
			SpriteIdUpperLeg = $"{name}.UpperLegSuit".AngeHash();
			SpriteIdLowerLeg = $"{name}.LowerLegSuit".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdHip) && !CellRenderer.HasSpriteGroup(SpriteIdHip)) SpriteIdHip = 0;
			if (!CellRenderer.HasSprite(SpriteIdSkirt) && !CellRenderer.HasSpriteGroup(SpriteIdSkirt)) SpriteIdSkirt = 0;
			if (!CellRenderer.HasSprite(SpriteIdUpperLeg) && !CellRenderer.HasSpriteGroup(SpriteIdUpperLeg)) SpriteIdUpperLeg = 0;
			if (!CellRenderer.HasSprite(SpriteIdLowerLeg) && !CellRenderer.HasSpriteGroup(SpriteIdLowerLeg)) SpriteIdLowerLeg = 0;
		}

		public static void DrawClothFromPool (PoseCharacter character) {
			if (character.Suit_Hip != 0 && Pool.TryGetValue(character.Suit_Hip, out var cloth)) {
				cloth.Draw(character);
			}
		}

		public override void Draw (PoseCharacter character) {
			DrawClothForHip(character, SpriteIdHip, CoverLegs ? 4 : 1);
			DrawClothForSkirt(character, SpriteIdSkirt, CoverLegs ? 6 : 1);
			DrawClothForUpperLeg(character, SpriteIdUpperLeg);
			DrawClothForLowerLeg(character, SpriteIdLowerLeg);
		}

		public static void DrawClothForHip (PoseCharacter character, int spriteID, int localZ = 1) {

			var hip = character.Hip;
			if (spriteID == 0 || hip.IsFullCovered) return;
			if (
				!CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var sprite, false, true) &&
				!CellRenderer.TryGetSprite(spriteID, out sprite)
			) return;

			var rect = hip.GetGlobalRect();
			if (!sprite.GlobalBorder.IsZero) {
				if (hip.Width > 0) {
					rect = rect.Expand(
						sprite.GlobalBorder.left,
						sprite.GlobalBorder.right,
						sprite.GlobalBorder.down,
						sprite.GlobalBorder.up
					);
				} else {
					rect = rect.Expand(
						sprite.GlobalBorder.right,
						sprite.GlobalBorder.left,
						sprite.GlobalBorder.down,
						sprite.GlobalBorder.up
					);
				}
			}

			// Limb
			hip.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
				 BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

			// Draw
			CellRenderer.Draw(sprite, rect, hip.Z + localZ);

		}

		public static void DrawClothForSkirt (PoseCharacter character, int spriteID, int localZ = 6) {

			var hip = character.Hip;
			if (spriteID == 0 || hip.IsFullCovered) return;
			if (
				!CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var sprite, false, true) &&
				!CellRenderer.TryGetSprite(spriteID, out sprite)
			) return;

			var body = character.Body;
			var upperLegL = character.UpperLegL;
			var upperLegR = character.UpperLegR;
			var animatedPoseType = character.AnimationType;
			const int A2G = 16;

			// Skirt
			int bodyWidthAbs = body.Width.Abs();
			var legTopL = upperLegL.GlobalLerp(0.5f, 1f);
			var legTopR = upperLegR.GlobalLerp(0.5f, 1f);
			int left = legTopL.x - upperLegL.SizeX / 2;
			int right = legTopR.x + upperLegR.SizeX / 2;
			int centerX = (left + right) / 2;
			int centerY = (legTopL.y + legTopR.y) / 2;
			bool stretch =
				animatedPoseType != CharacterAnimationType.GrabSide &&
				animatedPoseType != CharacterAnimationType.Dash &&
				animatedPoseType != CharacterAnimationType.Idle;
			int width = Util.Max(
				(right - left).Abs(), bodyWidthAbs - body.Border.left - body.Border.right
			);
			width += sprite.GlobalBorder.horizontal;
			if (stretch) width += Stretch(upperLegL.Rotation, upperLegR.Rotation);
			width += animatedPoseType switch {
				CharacterAnimationType.JumpUp or CharacterAnimationType.JumpDown => 2 * A2G,
				CharacterAnimationType.Run => A2G / 2,
				_ => 0,
			};
			int shiftY = animatedPoseType switch {
				CharacterAnimationType.Dash => A2G,
				_ => 0,
			};
			int offsetY = sprite.GlobalHeight * (1000 - sprite.PivotY) / 1000 + shiftY;
			CellRenderer.Draw(
				sprite,
				centerX,
				body.Height > 0 ? Util.Max(centerY + offsetY, character.Y + sprite.GlobalHeight) : centerY - offsetY,
				500, 1000, 0,
				width,
				body.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight,
				hip.Z + localZ
			);

			// Limb
			hip.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

			// Func
			static int Stretch (int rotL, int rotR) {
				int result = 0;
				if (rotL > 0) result += rotL / 2;
				if (rotR < 0) result += rotR / -2;
				return result;
			}
		}

		public static void DrawClothForUpperLeg (PoseCharacter character, int spriteID, int localZ = 1) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					CoverClothOn(character.UpperLegL, spriteL.GlobalID, localZ);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					CoverClothOn(character.UpperLegR, spriteR.GlobalID, localZ);
				}
			} else {
				CoverClothOn(character.UpperLegL, spriteID, localZ);
				CoverClothOn(character.UpperLegR, spriteID, localZ);
			}
		}

		public static void DrawClothForLowerLeg (PoseCharacter character, int spriteID, int localZ = 1) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					CoverClothOn(character.LowerLegL, spriteL.GlobalID, localZ);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					CoverClothOn(character.LowerLegR, spriteR.GlobalID, localZ);
				}
			} else {
				CoverClothOn(character.LowerLegL, spriteID, localZ);
				CoverClothOn(character.LowerLegR, spriteID, localZ);
			}
		}

		public static void DrawDoubleClothTailsOnHip (PoseCharacter character, int spriteIdLeft, int spriteIdRight, bool drawOnAllPose = false) {

			var animatedPoseType = character.AnimationType;
			var hip = character.Hip;
			var body = character.Body;
			if (
				!drawOnAllPose && (
					animatedPoseType == CharacterAnimationType.Rolling ||
					animatedPoseType == CharacterAnimationType.Sleep ||
					animatedPoseType == CharacterAnimationType.PassOut ||
					animatedPoseType == CharacterAnimationType.Fly
				)
			) return;

			var hipRect = hip.GetGlobalRect();
			int z = body.FrontSide ? -39 : 39;
			bool facingRight = body.Width > 0;
			int rotL = facingRight ? 30 : 18;
			int rotR = facingRight ? -18 : -30;
			int scaleX = 1000;
			int scaleY = 1000;

			if (body.Height < 0) {
				rotL = 180 - rotL;
				rotR = -180 + rotR;
				z = -z;
			}

			if (animatedPoseType == CharacterAnimationType.Dash) scaleY = 500;

			DrawClothTail(character, spriteIdLeft, hipRect.x + 16, hipRect.y, z, rotL, scaleX, scaleY);
			DrawClothTail(character, spriteIdRight, hipRect.xMax - 16, hipRect.y, z, rotR, scaleX, scaleY);

		}

		public static void DrawClothTail (PoseCharacter character, int spriteID, int globalX, int globalY, int z, int rotation, int scaleX = 1000, int scaleY = 1000, int motionAmount = 1000) {

			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;

			int rot = 0;

			// Motion
			if (motionAmount != 0) {
				// Idle Rot
				int animationFrame = (character.TypeID + Game.GlobalFrame).Abs(); // ※ Intended ※
				rot += rotation.Sign() * (animationFrame.PingPong(180) / 10 - 9);
				// Delta Y >> Rot
				int deltaY = character.DeltaPositionY;
				rot -= rotation.Sign() * (deltaY * 2 / 3).Clamp(-20, 20);
			}

			// Draw
			CellRenderer.Draw(
				sprite,
				globalX, globalY,
				sprite.PivotX, sprite.PivotY, rotation + rot,
				sprite.GlobalWidth * scaleX / 1000,
				sprite.GlobalHeight * scaleY / 1000,
				z
			);

		}

	}


	[RequireSprite("{1}.HandSuit")]
	[RequireLanguage("{1}.Hand")]
	public abstract class HandCloth : Cloth {

		protected sealed override ClothType ClothType => ClothType.Hand;
		private int SpriteID { get; init; }

		public HandCloth () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteID = $"{name}.HandSuit".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID) && !CellRenderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		}

		public static void DrawClothFromPool (PoseCharacter character) {
			if (character.Suit_Hand != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.Suit_Hand, out var cloth)) {
				cloth.Draw(character);
			}
		}

		public override void Draw (PoseCharacter character) => DrawClothForHand(character, SpriteID);

		public static void DrawClothForHand (PoseCharacter character, int spriteID, int localZ = 1) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					CoverClothOn(character.HandL, spriteL.GlobalID, localZ);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					CoverClothOn(character.HandR, spriteR.GlobalID, localZ);
				}
			} else {
				CoverClothOn(character.HandL, spriteID, localZ);
				CoverClothOn(character.HandR, spriteID, localZ);
			}
		}

	}



	[RequireSprite("{1}.FootSuit")]
	[RequireLanguage("{1}.Foot")]
	public abstract class FootCloth : Cloth {

		protected sealed override ClothType ClothType => ClothType.Foot;
		private int SpriteID { get; init; }

		public FootCloth () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteID = $"{name}.FootSuit".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID) && !CellRenderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		}

		public static void DrawClothFromPool (PoseCharacter character) {
			if (character.Suit_Foot != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.Suit_Foot, out var cloth)) {
				cloth.Draw(character);
			}
		}

		public override void Draw (PoseCharacter character) => DrawClothForFoot(character, SpriteID);

		public static void DrawClothForFoot (PoseCharacter character, int spriteID, int localZ = 1) {
			if (spriteID == 0) return;
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
					DrawClothForFootLogic(character.FootL, spriteL.GlobalID, localZ);
				}
				if (CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
					DrawClothForFootLogic(character.FootR, spriteR.GlobalID, localZ);
				}
			} else {
				DrawClothForFootLogic(character.FootL, spriteID, localZ);
				DrawClothForFootLogic(character.FootR, spriteID, localZ);
			}
			// Func
			static void DrawClothForFootLogic (BodyPart foot, int spriteID, int localZ) {
				if (spriteID == 0 || foot.IsFullCovered) return;
				if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
				var location = foot.GlobalLerp(0f, 0f);
				int width = Util.Max(foot.Width, sprite.GlobalWidth);
				if (sprite.GlobalBorder.IsZero) {
					CellRenderer.Draw(
						sprite, location.x, location.y,
						0, 0, foot.Rotation,
						foot.Width.Sign() * width, sprite.GlobalHeight,
						foot.Z + localZ
					);
				} else {
					CellRenderer.Draw_9Slice(
						sprite, location.x, location.y,
						0, 0, foot.Rotation,
						foot.Width.Sign() * width, sprite.GlobalHeight,
						foot.Z + localZ
					);
				}

				foot.Covered = sprite.Tag != SpriteTag.SHOW_LIMB_TAG ?
					BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

			}
		}

	}


	[RequireSprite("{2}")]
	public abstract class Cloth {


		// Const
		private static readonly Cell[] SINGLE_CELL = { CellRenderer.EMPTY_CELL };

		// Api
		public int TypeID { get; init; }
		protected abstract ClothType ClothType { get; }

		// Data
		protected static readonly Dictionary<int, Cloth> Pool = new();
		protected static readonly Dictionary<int, int[]> DefaultPool = new();
		private static readonly int ClothTypeCount = typeof(ClothType).EnumLength();


		// MSG
		[OnGameInitialize(-127)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(PoseCharacter);
			foreach (var type in typeof(Cloth).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Cloth cloth) continue;
				int suitID = type.AngeHash();
				Pool.TryAdd(suitID, cloth);
			}
			// Default Suit
			foreach (var (suitID, cloth) in Pool) {
				var dType = cloth.GetType().DeclaringType;
				if (dType == null || !dType.IsSubclassOf(charType)) continue;
				int charID = dType.AngeHash();
				if (DefaultPool.TryGetValue(charID, out var suitArray)) {
					suitArray[(int)cloth.ClothType] = suitID;
				} else {
					var arr = new int[ClothTypeCount];
					arr[(int)cloth.ClothType] = suitID;
					DefaultPool.TryAdd(charID, arr);
				}
			}
		}

		public Cloth () => TypeID = GetType().AngeHash();

		public abstract void Draw (PoseCharacter character);


		// Pool
		public static bool HasCloth (int clothID) => Pool.ContainsKey(clothID);
		public static bool TryGetCloth (int clothID, out Cloth cloth) => Pool.TryGetValue(clothID, out cloth);


		public static bool TryGetDefaultClothID (int characterID, ClothType suitType, out int suitID) {
			if (DefaultPool.TryGetValue(characterID, out var suitArray)) {
				suitID = suitArray[(int)suitType];
				return true;
			}
			suitID = 0;
			return false;
		}


		// Draw
		public static Cell[] AttachClothOn (
			BodyPart bodyPart, AngeSprite sprite, int locationX, int locationY, int localZ,
			int widthAmount = 1000, int heightAmount = 1000,
			int localRotation = 0, int shiftPixelX = 0, int shiftPixelY = 0, bool defaultHideLimb = true
		) {
			var location = bodyPart.GlobalLerp(locationX / 1000f, locationY / 1000f);
			location.x += shiftPixelX;
			location.y += shiftPixelY;
			Cell[] result;
			if (sprite.GlobalBorder.IsZero) {
				var cell = CellRenderer.Draw(
					sprite,
					location.x,
					location.y,
					sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
					(bodyPart.Width > 0 ? sprite.GlobalWidth : -sprite.GlobalWidth) * widthAmount / 1000,
					(bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight) * heightAmount / 1000,
					bodyPart.Z + localZ
				);
				result = SINGLE_CELL;
				result[0] = cell;
			} else {
				result = CellRenderer.Draw_9Slice(
					sprite, location.x, location.y,
					sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
					(bodyPart.Width > 0 ? sprite.GlobalWidth : -sprite.GlobalWidth) * widthAmount / 1000,
					(bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight) * heightAmount / 1000,
					bodyPart.Z + localZ
				);
			}
			if (defaultHideLimb) {
				bodyPart.Covered = sprite.Tag != SpriteTag.SHOW_LIMB_TAG ?
					BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
			} else {
				bodyPart.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
					BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
			}
			return result;
		}


		public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID) => CoverClothOn(bodyPart, spriteID, 1, Const.WHITE, true);
		public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID, int localZ) => CoverClothOn(bodyPart, spriteID, localZ, Const.WHITE, true);
		public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID, int localZ, Byte4 tint, bool defaultHideLimb = true) {
			if (spriteID == 0 || bodyPart.IsFullCovered || !CellRenderer.TryGetSprite(spriteID, out var sprite)) return null;
			Cell[] result;
			if (sprite.GlobalBorder.IsZero) {
				SINGLE_CELL[0] = CellRenderer.Draw(
					sprite, bodyPart.GlobalX, bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
					bodyPart.Width, bodyPart.Height, tint, bodyPart.Z + localZ
				);
				result = SINGLE_CELL;
			} else {
				result = CellRenderer.Draw_9Slice(
					sprite, bodyPart.GlobalX, bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
					bodyPart.Width, bodyPart.Height, tint, bodyPart.Z + localZ
				);
			}
			if (defaultHideLimb) {
				bodyPart.Covered = sprite.Tag != SpriteTag.SHOW_LIMB_TAG ?
					BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
			} else {
				bodyPart.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
					BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
			}
			return result;
		}


		public string GetDisplayName () {
			string typeName = (GetType().DeclaringType ?? GetType()).AngeName();
			return $"{Language.Get($"{typeName}.{ClothType}".AngeHash(), Util.GetDisplayName(typeName))}";
		}


	}
}
