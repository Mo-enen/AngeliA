using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {


	public enum CharacterPoseAnimationType {
		Idle = 0, Walk, Run,
		JumpUp, JumpDown, SwimIdle, SwimMove,
		SquatIdle, SquatMove, Dash, Rush, Pound,
		Climb, Fly, Slide, GrabTop, GrabSide, Spin,
		TakingDamage, Sleep, PassOut, Rolling,
	}


	public abstract partial class Character {




		#region --- VAR ---


		// Const
		private const int CM_PER_PX = 5;
		protected const int A2G = Const.CEL / Const.ART_CEL;
		protected static readonly int[] DEFAULT_BODY_PART_ID = {
			"DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(),
			"DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(),
			"DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(),
			"DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(),
			"DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(),
			"DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(),
			"DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(),
			"DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(),
		};
		private static readonly string[] BODY_PART_NAME = {
			"Head", "Body", "Hip",
			"Shoulder", "Shoulder",
			"UpperArm", "UpperArm",
			"LowerArm", "LowerArm",
			"Hand", "Hand",
			"UpperLeg", "UpperLeg",
			"LowerLeg", "LowerLeg",
			"Foot", "Foot",
		};
		private const int POSE_Z_HEAD = 10;
		private const int POSE_Z_BODY = 0;
		private const int POSE_Z_UPPERARM = 8;
		private const int POSE_Z_LOWERARM = 16;
		private const int POSE_Z_HAND = 24;
		private const int POSE_Z_UPPERLEG = 2;
		private const int POSE_Z_LOWERLEG = 1;
		private const int POSE_Z_FOOT = 2;

		// Api
		protected static int PoseZOffset { get; set; } = 0;
		public CharacterPoseAnimationType AnimatedPoseType { get; set; } = CharacterPoseAnimationType.Idle;
		public int PoseTwist { get; private set; } = 0;
		public int PoseRootX { get; private set; } = 0;
		public int PoseRootY { get; private set; } = 0;
		public int BasicRootY { get; private set; } = 0;
		public bool BodyPartsReady => BodyParts != null;
		public virtual Color32 SkinColor => new(239, 194, 160, 255);
		public virtual Color32 HairColor => new(51, 51, 51, 255);
		public virtual bool SpinOnGroundPound => false;
		public virtual int CharacterHeight => 160; // in CM

		// BodyPart
		public BodyPart Head { get; private set; } = null;
		public BodyPart Body { get; private set; } = null;
		public BodyPart Hip { get; private set; } = null;
		public BodyPart ShoulderL { get; private set; } = null;
		public BodyPart ShoulderR { get; private set; } = null;
		public BodyPart UpperArmL { get; private set; } = null;
		public BodyPart UpperArmR { get; private set; } = null;
		public BodyPart LowerArmL { get; private set; } = null;
		public BodyPart LowerArmR { get; private set; } = null;
		public BodyPart HandL { get; private set; } = null;
		public BodyPart HandR { get; private set; } = null;
		public BodyPart UpperLegL { get; private set; } = null;
		public BodyPart UpperLegR { get; private set; } = null;
		public BodyPart LowerLegL { get; private set; } = null;
		public BodyPart LowerLegR { get; private set; } = null;
		public BodyPart FootL { get; private set; } = null;
		public BodyPart FootR { get; private set; } = null;

		// Gadget
		public int FaceID { get; set; } = 0;
		public int HairID { get; set; } = 0;
		public int EarID { get; set; } = 0;
		public int TailID { get; set; } = 0;
		public int WingID { get; set; } = 0;
		public int HornID { get; set; } = 0;
		public int BoobID { get; set; } = 0;

		// Suit
		public int Suit_Head { get; set; } = 0;
		public int Suit_Body { get; set; } = 0;
		public int Suit_Hip { get; set; } = 0;
		public int Suit_Hand { get; set; } = 0;
		public int Suit_Foot { get; set; } = 0;

		// Data
		private static readonly Dictionary<int, BodyPartConfig> BodyPartConfigPool = new();
		private BodyPart[] BodyParts = null;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-64)]
		public static void BeforeGameInitializeLater () {

			// Sheet Pool
			RenderWithSheetPool.Clear();
			foreach (var type in typeof(Character).AllChildClass()) {
				if (type.GetCustomAttribute<RenderWithSheetAttribute>(true) != null) {
					RenderWithSheetPool.TryAdd(type.AngeHash());
				}
			}

			// Config Pool
			BodyPartConfigPool.Clear();
			foreach (var type in typeof(Character).AllChildClass()) {

				int typeID = type.AngeHash();
				if (RenderWithSheetPool.Contains(typeID)) continue;

				string basicName = type.Name;
				if (basicName[0] == 'e') basicName = basicName[1..];

				int len = DEFAULT_BODY_PART_ID.Length;
				var config = new BodyPartConfig() {
					BodyParts = new int[len],
				};

				// Body Parts
				for (int i = 0; i < len; i++) {
					int id = DEFAULT_BODY_PART_ID[i];
					int newID = $"{basicName}.{BODY_PART_NAME[i]}".AngeHash();
					if (i == 0) {
						// For Head
						if (CellRenderer.HasSpriteGroup(newID) || CellRenderer.HasSprite(newID)) {
							id = newID;
						}
					} else {
						// For Other
						if (CellRenderer.HasSprite(newID)) id = newID;
					}
					config.BodyParts[i] = id;
				}

				// Gadget
				config.FaceID = Face.TryGetDefaultFaceID(typeID, out int defaultID) ? defaultID : 0;
				config.HairID = Hair.TryGetDefaultHairID(typeID, out defaultID) ? defaultID : 0;
				config.EarID = Ear.TryGetDefaultEarID(typeID, out defaultID) ? defaultID : 0;
				config.TailID = Tail.TryGetDefaultTailID(typeID, out defaultID) ? defaultID : 0;
				config.WingID = Wing.TryGetDefaultWingID(typeID, out defaultID) ? defaultID : 0;
				config.HornID = Horn.TryGetDefaultHornID(typeID, out defaultID) ? defaultID : 0;
				config.BoobID = Boob.TryGetDefaultBoobID(typeID, out defaultID) ? defaultID : 0;

				// Suit
				config.SuitHead = Cloth.TryGetDefaultSuitID(typeID, ClothType.Head, out int suitID) ? suitID : 0;
				config.SuitBody = Cloth.TryGetDefaultSuitID(typeID, ClothType.Body, out suitID) ? suitID : 0;
				config.SuitHip = Cloth.TryGetDefaultSuitID(typeID, ClothType.Hip, out suitID) ? suitID : 0;
				config.SuitHand = Cloth.TryGetDefaultSuitID(typeID, ClothType.Hand, out suitID) ? suitID : 0;
				config.SuitFoot = Cloth.TryGetDefaultSuitID(typeID, ClothType.Foot, out suitID) ? suitID : 0;

				// Final
				BodyPartConfigPool.Add(typeID, config);

			}

		}


		private void OnActivated_Pose () {

			RenderWithSheet = RenderWithSheetPool.Contains(TypeID);
			if (RenderWithSheetPool.Contains(TypeID)) return;

			int len = DEFAULT_BODY_PART_ID.Length;
			if (BodyParts == null || BodyParts.Length != len) {
				BodyParts = new BodyPart[len];
			}

			if (BodyPartConfigPool.TryGetValue(TypeID, out var config) && config != null) {

				// Body Part
				for (int i = 0; i < len; i++) {
					var bodyPartID = config.BodyParts[i];
					BodyParts[i] = new BodyPart(
						bodyPartID,
						i == 9 || i == 10 || i == 15 || i == 16,
						(i >= 7 && i < 11) || (i >= 13 && i < 17) ? BodyParts[i - 2] : null
					);
				}

				// Gadget
				FaceID = config.FaceID;
				HairID = config.HairID;
				EarID = config.EarID;
				TailID = config.TailID;
				WingID = config.WingID;
				HornID = config.HornID;
				BoobID = config.BoobID;

				// Suit
				Suit_Head = config.SuitHead;
				Suit_Body = config.SuitBody;
				Suit_Hip = config.SuitHip;
				Suit_Hand = config.SuitHand;
				Suit_Foot = config.SuitFoot;

			} else {
				for (int i = 0; i < len; i++) {
					if (BodyParts[i] == null) BodyParts[i] = new();
				}
			}

			Head = BodyParts[0];
			Body = BodyParts[1];
			Hip = BodyParts[2];
			ShoulderL = BodyParts[3];
			ShoulderR = BodyParts[4];
			UpperArmL = BodyParts[5];
			UpperArmR = BodyParts[6];
			LowerArmL = BodyParts[7];
			LowerArmR = BodyParts[8];
			HandL = BodyParts[9];
			HandR = BodyParts[10];
			UpperLegL = BodyParts[11];
			UpperLegR = BodyParts[12];
			LowerLegL = BodyParts[13];
			LowerLegR = BodyParts[14];
			FootL = BodyParts[15];
			FootR = BodyParts[16];
		}


		private void RenderUpdate_AnimationType () {
			var poseType = GetCurrentPoseAnimationType();
			if (poseType != AnimatedPoseType) {
				CurrentAnimationFrame = 0;
				AnimatedPoseType = poseType;
			}
		}


		private void RenderUpdate_Pose () {

			int cellIndexStart = CellRenderer.GetUsedCellCount();

			PoseUpdate_ResetToDefault();
			PoseUpdate_CalculateBodyPart();
			CalculateBodypartGlobalPosition();
			OnPoseCalculated();
			CalculateBodypartGlobalPosition();

			Wing.Draw(this, out var wing);
			if (ShowingTail(wing)) Tail.Draw(this);
			Face.Draw(this);
			Hair.Draw(this);
			Ear.Draw(this);
			Horn.Draw(this);
			if (!Cloth.HasCloth(Suit_Body)) Boob.Draw(this);

			Cloth.DrawHeadSuit(this);
			Cloth.DrawBodySuit(this);
			if (Boob.TryGetBoob(BoobID, out var boob) && boob.SuitAvailable) {
				Cloth.DrawBoobSuit(this, boob.Size);
			}
			Cloth.DrawHipSuit(this);
			Cloth.DrawHandSuit(this);
			Cloth.DrawFootSuit(this);

			PoseUpdate_DrawBodyPart(cellIndexStart);
		}


		// Pipeline
		private void PoseUpdate_ResetToDefault () {

			int bounce = CurrentRenderingBounce;
			int facingSign = FacingRight ? 1 : -1;

			PoseRootX = 0;
			PoseTwist = 0;
			foreach (var unit in BodyParts) {
				unit.Rotation = 0;
				unit.FrontSide = FacingFront;
				unit.Tint = SkinColor;
			}

			// Hip
			Hip.X = 0;
			Hip.Y = 0;
			Hip.Z = POSE_Z_BODY;
			Hip.Width = facingSign * Hip.SizeX;
			Hip.PivotX = 500;
			Hip.PivotY = 0;

			// Body
			Body.X = 0;
			Body.Y = Hip.Height;
			Body.Z = POSE_Z_BODY;
			Body.Width = facingSign * Body.SizeX;
			Body.PivotX = 500;
			Body.PivotY = 0;

			// Character Height
			int bodyHipSizeY = Body.SizeY + Hip.SizeY;
			int targetUnitHeight = CharacterHeight * A2G / CM_PER_PX - Head.SizeY;
			int legRootSize = UpperLegL.SizeY + LowerLegL.SizeY + FootL.SizeY;
			int defaultCharHeight = bodyHipSizeY + legRootSize;
			Body.Height = Body.SizeY * targetUnitHeight / defaultCharHeight;
			Hip.Height = Hip.SizeY * targetUnitHeight / defaultCharHeight;
			PoseRootY = BasicRootY = legRootSize * targetUnitHeight / defaultCharHeight;
			int upperLegHeight = UpperLegL.SizeY * PoseRootY / legRootSize;
			int lowerLegHeight = LowerLegL.SizeY * PoseRootY / legRootSize;
			int footHeight = FootL.SizeY * PoseRootY / legRootSize;
			int upperArmHeight = UpperArmL.SizeY * targetUnitHeight / defaultCharHeight;
			int lowerArmHeight = LowerArmL.SizeY * targetUnitHeight / defaultCharHeight;
			int bodyBorderL = FacingRight ? Body.Border.Left : Body.Border.Right;
			int bodyBorderR = FacingRight ? Body.Border.Right : Body.Border.Left;
			int hipBorderL = FacingRight ? Hip.Border.Left : Hip.Border.Right;
			int hipBorderR = FacingRight ? Hip.Border.Right : Hip.Border.Left;

			// Head
			Head.X = 0;
			Head.Y = Body.Y + Body.Height;
			Head.Z = POSE_Z_HEAD;
			Head.Width = facingSign * Head.SizeX;
			Head.Height = Head.SizeY;
			Head.PivotX = 500;
			Head.PivotY = 0;

			// Bounce
			if (bounce.Abs() != 1000) {
				bool reverse = bounce < 0;
				bounce = bounce.Abs();
				if (reverse) {
					Body.Width = Body.Width * bounce / 1000;
					Body.Height += Body.Height * (1000 - bounce) / 1000;
					Head.Width = Head.Width * bounce / 1000;
					Head.Height += Head.Height * (1000 - bounce) / 1000;
				} else {
					Body.Width += Body.Width * (1000 - bounce) / 1000;
					Body.Height = Body.Height * bounce / 1000;
					Head.Width += Head.Width * (1000 - bounce) / 1000;
					Head.Height = Head.Height * bounce / 1000;
				}
				Head.Y = Body.Y + Body.Height;
			}

			// Shoulder
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderL.Z = POSE_Z_BODY;
			ShoulderL.Width = ShoulderL.SizeX;
			ShoulderL.Height = ShoulderL.SizeY;
			ShoulderL.PivotX = 1000;
			ShoulderL.PivotY = 1000;

			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderR.Z = POSE_Z_BODY;
			ShoulderR.Width = -ShoulderR.SizeX;
			ShoulderR.Height = ShoulderR.SizeY;
			ShoulderR.PivotX = 1000;
			ShoulderR.PivotY = 1000;

			// Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmL.Z = (FacingFront ? facingSign * POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
			UpperArmL.Width = UpperArmL.SizeX;
			UpperArmL.Height = upperArmHeight;
			UpperArmL.PivotX = 1000;
			UpperArmL.PivotY = 1000;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;
			UpperArmR.Z = (FacingFront ? facingSign * -POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
			UpperArmR.Width = UpperArmR.SizeX;
			UpperArmR.Height = upperArmHeight;
			UpperArmR.PivotX = 0;
			UpperArmR.PivotY = 1000;

			LowerArmL.X = UpperArmL.X;
			LowerArmL.Y = UpperArmL.Y - UpperArmL.Height;
			LowerArmL.Z = (FacingFront ? facingSign * POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
			LowerArmL.Width = LowerArmL.SizeX;
			LowerArmL.Height = lowerArmHeight;
			LowerArmL.PivotX = 1000;
			LowerArmL.PivotY = 1000;

			LowerArmR.X = UpperArmR.X;
			LowerArmR.Y = UpperArmR.Y - UpperArmR.Height;
			LowerArmR.Z = (FacingFront ? facingSign * -POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
			LowerArmR.Width = LowerArmR.SizeX;
			LowerArmR.Height = lowerArmHeight;
			LowerArmR.PivotX = 0;
			LowerArmR.PivotY = 1000;

			HandL.X = LowerArmL.X;
			HandL.Y = LowerArmL.Y - LowerArmL.Height;
			HandL.Z = (FacingFront ? facingSign * POSE_Z_HAND : -POSE_Z_HAND);
			HandL.Width = HandL.SizeX;
			HandL.Height = HandL.SizeY;
			HandL.PivotX = 1000;
			HandL.PivotY = 1000;

			HandR.X = LowerArmR.X;
			HandR.Y = LowerArmR.Y - LowerArmR.Height;
			HandR.Z = (FacingFront ? facingSign * -POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Width = -HandR.SizeX;
			HandR.Height = HandR.SizeY;
			HandR.PivotX = 1000;
			HandR.PivotY = 1000;

			// Leg
			UpperLegL.X = Hip.X - Hip.Width.Abs() / 2 + hipBorderL;
			UpperLegL.Y = Hip.Y;
			UpperLegL.Z = POSE_Z_UPPERLEG;
			UpperLegL.Width = UpperLegL.SizeX;
			UpperLegL.Height = upperLegHeight;
			UpperLegL.PivotX = 0;
			UpperLegL.PivotY = 1000;

			UpperLegR.X = Hip.X + Hip.Width.Abs() / 2 - hipBorderR;
			UpperLegR.Y = Hip.Y;
			UpperLegR.Z = POSE_Z_UPPERLEG;
			UpperLegR.Width = UpperLegR.SizeX;
			UpperLegR.Height = upperLegHeight;
			UpperLegR.PivotX = 1000;
			UpperLegR.PivotY = 1000;

			LowerLegL.X = UpperLegL.X;
			LowerLegL.Y = UpperLegL.Y - UpperLegL.Height;
			LowerLegL.Z = POSE_Z_LOWERLEG;
			LowerLegL.Width = LowerLegL.SizeX;
			LowerLegL.Height = lowerLegHeight;
			LowerLegL.PivotX = 0;
			LowerLegL.PivotY = 1000;
			if (FacingRight) LowerLegL.X -= A2G;

			LowerLegR.X = UpperLegR.X;
			LowerLegR.Y = UpperLegR.Y - UpperLegR.Height;
			LowerLegR.Z = POSE_Z_LOWERLEG;
			LowerLegR.Width = LowerLegR.SizeX;
			LowerLegR.Height = lowerLegHeight;
			LowerLegR.PivotX = 1000;
			LowerLegR.PivotY = 1000;
			if (!FacingRight) LowerLegR.X += A2G;

			FootL.X = FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
			FootL.Y = LowerLegL.Y - LowerLegL.Height;
			FootL.Z = POSE_Z_FOOT;
			FootL.Width = facingSign * FootL.SizeX;
			FootL.Height = footHeight;
			FootL.PivotX = 0;
			FootL.PivotY = 1000;

			FootR.X = FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
			FootR.Y = LowerLegR.Y - LowerLegR.Height;
			FootR.Z = POSE_Z_FOOT;
			FootR.Width = facingSign * FootR.SizeX;
			FootR.Height = footHeight;
			FootR.PivotX = 0;
			FootR.PivotY = 1000;

		}


		private void PoseUpdate_CalculateBodyPart () {
			switch (AnimatedPoseType) {
				case CharacterPoseAnimationType.TakingDamage:
					PoseAnimation_Damage();
					break;
				case CharacterPoseAnimationType.Sleep:
					PoseAnimation_Sleep();
					break;
				case CharacterPoseAnimationType.PassOut:
					PoseAnimation_PassOut();
					break;
				case CharacterPoseAnimationType.Dash:
					PoseAnimation_Dash();
					break;
				case CharacterPoseAnimationType.Rolling:
					PoseAnimation_Rolling();
					break;
				case CharacterPoseAnimationType.Idle:
					PoseAnimation_Idle();
					break;
				case CharacterPoseAnimationType.Walk:
					PoseAnimation_Walk();
					break;
				case CharacterPoseAnimationType.Run:
					PoseAnimation_Run();
					break;
				case CharacterPoseAnimationType.JumpUp:
					PoseAnimation_JumpUp();
					break;
				case CharacterPoseAnimationType.JumpDown:
					PoseAnimation_JumpDown();
					break;
				case CharacterPoseAnimationType.SwimIdle:
					PoseAnimation_SwimIdle();
					break;
				case CharacterPoseAnimationType.SwimMove:
					PoseAnimation_SwimMove();
					break;
				case CharacterPoseAnimationType.SquatIdle:
					PoseAnimation_SquatIdle();
					break;
				case CharacterPoseAnimationType.SquatMove:
					PoseAnimation_SquatMove();
					break;
				case CharacterPoseAnimationType.Rush:
					PoseAnimation_Rush();
					break;
				case CharacterPoseAnimationType.Pound:
					PoseAnimation_Pound();
					break;
				case CharacterPoseAnimationType.Spin:
					PoseAnimation_Spin();
					break;
				case CharacterPoseAnimationType.Climb:
					PoseAnimation_Climb();
					break;
				case CharacterPoseAnimationType.Fly:
					PoseAnimation_Fly();
					break;
				case CharacterPoseAnimationType.Slide:
					PoseAnimation_Slide();
					break;
				case CharacterPoseAnimationType.GrabTop:
					PoseAnimation_GrabTop();
					break;
				case CharacterPoseAnimationType.GrabSide:
					PoseAnimation_GrabSide();
					break;
			}
		}


		protected virtual void OnPoseCalculated () {
			if (IsAttacking) {
				if (StopMoveOnAttack) {
					PoseUpdate_ResetToDefault();
				}
				PoseAnimationOverride_Attack_SmashDown();
			}
		}


		private void PoseUpdate_DrawBodyPart (int cellIndexStart) {

			// Fix Approximately Rotation
			LowerArmL.FixApproximatelyRotation();
			LowerArmR.FixApproximatelyRotation();
			HandL.FixApproximatelyRotation();
			HandR.FixApproximatelyRotation();
			LowerLegL.FixApproximatelyRotation();
			LowerLegR.FixApproximatelyRotation();
			FootL.FixApproximatelyRotation();
			FootR.FixApproximatelyRotation();

			// Draw
			foreach (var bodyPart in BodyParts) {
				if (bodyPart.ID == 0 || bodyPart.Tint.a == 0) continue;
				int id = bodyPart.ID;
				if (bodyPart == Head && CellRenderer.TryGetSpriteFromGroup(id, Head.FrontSide ? 0 : 1, out var headSprite, false, true)) {
					id = headSprite.GlobalID;
				}
				if (bodyPart.Border.IsZero) {
					// General
					CellRenderer.Draw(
						id,
						X + PoseRootX + bodyPart.X,
						Y + PoseRootY + bodyPart.Y,
						bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
						bodyPart.Tint, bodyPart.Z
					);
				} else {
					// 9 Slice
					CellRenderer.Draw_9Slice(
						id,
						X + PoseRootX + bodyPart.X,
						Y + PoseRootY + bodyPart.Y,
						bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
						bodyPart.Tint, bodyPart.Z
					);
				}
			}

			// Z Offset
			PoseZOffset -= 40;
			if (CellRenderer.GetCells(out var layerCells, out int count)) {
				int endIndex = Mathf.Min(CellRenderer.GetUsedCellCount(), count);
				for (int i = cellIndexStart; i < endIndex; i++) {
					layerCells[i].Z += PoseZOffset;
				}
			}

		}


		#endregion




		#region --- API ---


		// Cloth
		public void DrawClothForBody (int spriteGroupId) => DrawClothForBody(spriteGroupId, Const.WHITE);
		public void DrawClothForBody (int spriteGroupId, Color32 tint) {
			if (spriteGroupId == 0) return;
			int groupIndex = !Body.FrontSide ? 3 :
				PoseTwist.Abs() < 333 ? 0 :
				Body.Width > 0 == PoseTwist < 0 ? 1 :
				2;
			if (!CellRenderer.TryGetSpriteFromGroup(spriteGroupId, groupIndex, out var suitSprite, false, true)) return;
			var rect = new RectInt(
				Body.GlobalX - Body.Width / 2,
				Hip.GlobalY,
				Body.Width,
				Body.Height + Hip.Height
			);
			if (!suitSprite.GlobalBorder.IsZero) {
				int midW = (suitSprite.GlobalWidth - suitSprite.GlobalBorder.Horizontal).GreaterOrEquel(1);
				int midH = (suitSprite.GlobalHeight - suitSprite.GlobalBorder.Vertical).GreaterOrEquel(1);
				rect = rect.Expand(
					suitSprite.GlobalBorder.Left * rect.width / midW,
					suitSprite.GlobalBorder.Right * rect.width / midW,
					suitSprite.GlobalBorder.Down * rect.height / midH,
					suitSprite.GlobalBorder.Up * rect.height / midH
				);
			}
			CellRenderer.Draw(suitSprite.GlobalID, rect, tint, Body.Z + 7);
			if (CellRenderer.TryGetMeta(suitSprite.GlobalID, out var meta) && meta.Tag == Const.HIDE_LIMB_TAG) {
				Body.Tint = Const.CLEAR;
			}
		}


		public void DrawClothForBoob (int spriteID, RectInt boobRect) => DrawClothForBoob(spriteID, boobRect, Const.WHITE);
		public void DrawClothForBoob (int spriteID, RectInt boobRect, Color32 tint) {
			if (spriteID == 0 || boobRect.width == 0 || !Body.FrontSide) return;
			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(spriteID, boobRect, tint, Body.Z + 8);
			} else {
				CellRenderer.Draw_9Slice(spriteID, boobRect, tint, Body.Z + 8);
			}
		}


		public void DrawClothForHip (int spriteID, bool motionStrech) => DrawClothForHip(spriteID, motionStrech, Const.WHITE);
		public void DrawClothForHip (int spriteID, bool motionStrech, Color32 tint) {
			if (spriteID == 0) return;
			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			int bodyWidthAbs = Body.Width.Abs();
			int centerX;
			int centerY;
			int width;
			int shiftY = 0;
			var legTopL = UpperLegL.GlobalLerp(0.5f, 1f);
			var legTopR = UpperLegR.GlobalLerp(0.5f, 1f);
			if (motionStrech) {
				// Skirt
				int left = legTopL.x - UpperLegL.SizeX / 2;
				int right = legTopR.x + UpperLegR.SizeX / 2;
				centerX = (left + right) / 2;
				centerY = (legTopL.y + legTopR.y) / 2;
				bool stretch =
					AnimatedPoseType != CharacterPoseAnimationType.GrabSide &&
					AnimatedPoseType != CharacterPoseAnimationType.Dash &&
					AnimatedPoseType != CharacterPoseAnimationType.Idle;
				width = Mathf.Max(
					(right - left).Abs(), bodyWidthAbs - Body.Border.Left - Body.Border.Right
				);
				width += sprite.GlobalBorder.Horizontal;
				if (stretch) width += Stretch(UpperLegL.Rotation, UpperLegR.Rotation);
				width += AnimatedPoseType switch {
					CharacterPoseAnimationType.JumpUp or CharacterPoseAnimationType.JumpDown => 2 * A2G,
					CharacterPoseAnimationType.Run => A2G / 2,
					_ => 0,
				};
				shiftY = AnimatedPoseType switch {
					CharacterPoseAnimationType.Dash => A2G,
					_ => 0,
				};
			} else {
				// Hip
				width = bodyWidthAbs - Body.Border.Left - Body.Border.Right;
				width += sprite.GlobalBorder.Horizontal;
				if (
					AnimatedPoseType == CharacterPoseAnimationType.Fly ||
					AnimatedPoseType == CharacterPoseAnimationType.Dash
				) {
					int left = UpperLegL.GlobalLerp(0.5f, 0.5f).x - UpperLegL.SizeX / 2;
					int right = UpperLegR.GlobalLerp(0.5f, 0.5f).x + UpperLegR.SizeX / 2;
					centerX = (left + right) / 2;
					centerY = (UpperLegL.GlobalY + UpperLegR.GlobalY) / 2;
				} else {
					centerX = Body.GlobalX - bodyWidthAbs / 2 + (FacingRight ? Body.Border.Left : Body.Border.Right) + width / 2;
					centerY = Body.GlobalY;
				}
			}
			int offsetY = sprite.GlobalHeight * (1000 - sprite.PivotY) / 1000 + shiftY;
			CellRenderer.Draw(
				spriteID,
				centerX,
				Body.Height > 0 ? Mathf.Max(centerY + offsetY, Y + sprite.GlobalHeight) : centerY - offsetY,
				500, 1000, 0,
				Body.Width.Sign() * width,
				Body.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight,
				tint, Body.Z + 6
			);
			// Func
			static int Stretch (int rotL, int rotR) {
				int result = 0;
				if (rotL > 0) result += rotL / 2;
				if (rotR < 0) result += rotR / -2;
				return result;
			}
		}


		public void DrawClothForFoot (BodyPart foot, int spriteID) => DrawClothForFoot(foot, spriteID, Const.WHITE);
		public void DrawClothForFoot (BodyPart foot, int spriteID, Color32 tint) {
			if (spriteID == 0) return;
			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			var location = foot.GlobalLerp(0f, 0f);
			int width = Mathf.Max(foot.Width.Abs(), sprite.GlobalWidth);
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(
					spriteID, location.x, location.y,
					0, 0, foot.Rotation,
					foot.Width.Sign() * width, sprite.GlobalHeight,
					tint, foot.Z + 1
				);
			} else {
				CellRenderer.Draw_9Slice(
					spriteID, location.x, location.y,
					0, 0, foot.Rotation,
					foot.Width.Sign() * width, sprite.GlobalHeight,
					tint, foot.Z + 1
				);
			}
			if (!CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) || meta.Tag != Const.SHOW_LIMB_TAG) {
				foot.Tint = Const.CLEAR;
			}
		}


		public void DrawArmorForShoulder (int spriteID, int rotationAmount = 700) => DrawArmorForShoulder(spriteID, Const.WHITE, rotationAmount);
		public void DrawArmorForShoulder (int spriteID, Color32 tint, int rotationAmount = 700) {
			if (
				AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				AnimatedPoseType == CharacterPoseAnimationType.PassOut
			) return;
			AttachClothOn(
				UpperArmL, Direction3.Up, spriteID, 36, tint, true,
				(-UpperArmL.Rotation * rotationAmount / 1000).Clamp(-UpperArmL.Rotation - 30, -UpperArmL.Rotation + 30)
			);
			AttachClothOn(
				UpperArmR, Direction3.Up, spriteID, 36, tint, false,
				(-UpperArmR.Rotation * rotationAmount / 1000).Clamp(-UpperArmR.Rotation - 30, -UpperArmR.Rotation + 30)
			);
		}


		public void DrawArmorForLimb (int armSpriteID, int legSpriteID) => DrawArmorForLimb(armSpriteID, legSpriteID, Const.WHITE);
		public void DrawArmorForLimb (int armSpriteID, int legSpriteID, Color32 tint) {
			AttachClothOn(LowerArmL, Direction3.Up, armSpriteID, LowerArmL.Z + 16, tint, true);
			AttachClothOn(LowerArmR, Direction3.Up, armSpriteID, LowerArmR.Z + 16, tint, false);
			AttachClothOn(LowerLegL, Direction3.Up, legSpriteID, LowerLegL.Z + 16, tint, !FacingRight);
			AttachClothOn(LowerLegR, Direction3.Up, legSpriteID, LowerLegR.Z + 16, tint, !FacingRight);
		}


		public void AttachClothOn (
			BodyPart bodyPart, Direction3 verticalLocation, int groupID, int z, 
			Color32 tint, bool flipX = false, int localRotation = 0, int offsetX = 0, int offsetY = 0
		) {
			if (groupID == 0) return;
			if (!CellRenderer.TryGetSpriteFromGroup(groupID, bodyPart.FrontSide ? 0 : 1, out var sprite, false, true)) return;
			var location = verticalLocation switch {
				Direction3.Up => bodyPart.GlobalLerp(0.5f, 1f),
				Direction3.None => bodyPart.GlobalLerp(0.5f, 0.5f),
				Direction3.Down => bodyPart.GlobalLerp(0.5f, 0f),
				_ => bodyPart.GlobalLerp(0.5f, 1f),
			};
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(
					sprite.GlobalID,
					location.x + bodyPart.Width.Abs() * offsetX / 1000,
					location.y + bodyPart.Height.Abs() * offsetY / 1000,
					sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
					flipX ? -sprite.GlobalWidth : sprite.GlobalWidth,
					bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight,
					tint, z
				);
			} else {
				CellRenderer.Draw_9Slice(
					sprite.GlobalID,
					location.x + bodyPart.Width.Abs() * offsetX / 1000,
					location.y + bodyPart.Height.Abs() * offsetY / 1000,
					sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
					flipX ? -sprite.GlobalWidth : sprite.GlobalWidth,
					bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight,
					tint, z
				);
			}
			if (CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) && meta.Tag == Const.HIDE_LIMB_TAG) {
				bodyPart.Tint = Const.CLEAR;
			}
		}


		public void CoverClothOn (BodyPart bodyPart, int spriteID, int z, Color32 tint, bool defaultHideLimb = true) {
			if (spriteID == 0) return;
			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(
					spriteID, bodyPart.GlobalX, bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
					bodyPart.Width, bodyPart.Height, tint, z
				);
			} else {
				CellRenderer.Draw_9Slice(
					spriteID, bodyPart.GlobalX, bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
					bodyPart.Width, bodyPart.Height, tint, z
				);
			}
			if (defaultHideLimb) {
				if (!CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) || meta.Tag != Const.SHOW_LIMB_TAG) {
					bodyPart.Tint = Const.CLEAR;
				}
			} else {
				if (CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) && meta.Tag == Const.HIDE_LIMB_TAG) {
					bodyPart.Tint = Const.CLEAR;
				}
			}
		}


		#endregion




		#region --- LGC ---


		private CharacterPoseAnimationType GetCurrentPoseAnimationType () {
			if (EnteringDoor) return CharacterPoseAnimationType.Idle;
			if (TakingDamage) return CharacterPoseAnimationType.TakingDamage;
			if (CharacterState == CharacterState.Sleep) return CharacterPoseAnimationType.Sleep;
			if (CharacterState == CharacterState.PassOut) return CharacterPoseAnimationType.PassOut;
			if (IsRolling) return CharacterPoseAnimationType.Rolling;
			return MovementState switch {
				CharacterMovementState.Walk => CharacterPoseAnimationType.Walk,
				CharacterMovementState.Run => CharacterPoseAnimationType.Run,
				CharacterMovementState.JumpUp => CharacterPoseAnimationType.JumpUp,
				CharacterMovementState.JumpDown => CharacterPoseAnimationType.JumpDown,
				CharacterMovementState.SwimIdle => CharacterPoseAnimationType.SwimIdle,
				CharacterMovementState.SwimMove => CharacterPoseAnimationType.SwimMove,
				CharacterMovementState.SquatIdle => CharacterPoseAnimationType.SquatIdle,
				CharacterMovementState.SquatMove => CharacterPoseAnimationType.SquatMove,
				CharacterMovementState.Dash => CharacterPoseAnimationType.Dash,
				CharacterMovementState.Rush => CharacterPoseAnimationType.Rush,
				CharacterMovementState.Pound => SpinOnGroundPound ? CharacterPoseAnimationType.Spin : CharacterPoseAnimationType.Pound,
				CharacterMovementState.Climb => CharacterPoseAnimationType.Climb,
				CharacterMovementState.Fly => CharacterPoseAnimationType.Fly,
				CharacterMovementState.Slide => CharacterPoseAnimationType.Slide,
				CharacterMovementState.GrabTop => CharacterPoseAnimationType.GrabTop,
				CharacterMovementState.GrabSide => CharacterPoseAnimationType.GrabSide,
				CharacterMovementState.GrabFlip => CharacterPoseAnimationType.Rolling,
				_ => CharacterPoseAnimationType.Idle,
			};
		}


		private void CalculateBodypartGlobalPosition () {
			foreach (var pose in BodyParts) {
				pose.GlobalX = X + PoseRootX + pose.X;
				pose.GlobalY = Y + PoseRootY + pose.Y;
			}
		}


		private bool ShowingTail (Wing wing) => AnimatedPoseType != CharacterPoseAnimationType.Fly || wing == null || !wing.IsPropeller;


		#endregion




	}
}
