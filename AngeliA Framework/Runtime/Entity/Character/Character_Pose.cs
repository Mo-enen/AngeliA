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
		public static readonly int[] DEFAULT_BODY_PART_ID = {
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
		private const int POSE_Z_HAND_CASUAL = 26;
		private const int POSE_Z_HAND = 36;
		private const int POSE_Z_UPPERLEG = 2;
		private const int POSE_Z_LOWERLEG = 1;
		private const int POSE_Z_FOOT = 2;

		// Api
		protected static int PoseZOffset { get; set; } = 0;
		public CharacterPoseAnimationType AnimatedPoseType { get; set; } = CharacterPoseAnimationType.Idle;
		public int PoseTwist { get; set; } = 0;
		public int PoseRootX { get; set; } = 0;
		public int PoseRootY { get; set; } = 0;
		public int HeadTwist { get; set; } = 0;
		public int HeadRotation { get; set; } = 0;
		public int BasicRootY { get; private set; } = 0;
		public bool BodyPartsReady => BodyParts != null;
		public bool ShowingTail => AnimatedPoseType != CharacterPoseAnimationType.Fly || !Wing.TryGetWing(WingID, out var wing) || !wing.IsPropeller;
		public Color32 SkinColor { get; set; } = new(239, 194, 160, 255);
		public Color32 HairColor { get; set; } = new(51, 51, 51, 255);
		public int CharacterHeight { get; set; } = 160; // in CM
		public int HandGrabRotationL { get; set; } = 0;
		public int HandGrabRotationR { get; set; } = 0;
		public int HandGrabScaleL { get; set; } = 1000;
		public int HandGrabScaleR { get; set; } = 1000;
		public int HandGrabAttackTwistL { get; set; } = 1000;
		public int HandGrabAttackTwistR { get; set; } = 1000;

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
			var poseType = GetCurrentPoseAnimationType(this);
			if (poseType != AnimatedPoseType) {
				CurrentAnimationFrame = 0;
				AnimatedPoseType = poseType;
			}
			// Func
			static CharacterPoseAnimationType GetCurrentPoseAnimationType (Character character) {
				if (character.EnteringDoor) return CharacterPoseAnimationType.Idle;
				if (character.TakingDamage) return CharacterPoseAnimationType.TakingDamage;
				if (character.CharacterState == CharacterState.Sleep) return CharacterPoseAnimationType.Sleep;
				if (character.CharacterState == CharacterState.PassOut) return CharacterPoseAnimationType.PassOut;
				if (character.IsRolling) return CharacterPoseAnimationType.Rolling;
				return character.MovementState switch {
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
					CharacterMovementState.Pound => character.SpinOnGroundPound ? CharacterPoseAnimationType.Spin : CharacterPoseAnimationType.Pound,
					CharacterMovementState.Climb => CharacterPoseAnimationType.Climb,
					CharacterMovementState.Fly => CharacterPoseAnimationType.Fly,
					CharacterMovementState.Slide => CharacterPoseAnimationType.Slide,
					CharacterMovementState.GrabTop => CharacterPoseAnimationType.GrabTop,
					CharacterMovementState.GrabSide => CharacterPoseAnimationType.GrabSide,
					CharacterMovementState.GrabFlip => CharacterPoseAnimationType.Rolling,
					_ => CharacterPoseAnimationType.Idle,
				};
			}
		}


		private void FrameUpdate_PoseRendering () {

			int cellIndexStart = CellRenderer.GetUsedCellCount();

			AnimationLibrary.Begin(this);

			ResetPoseToDefault();
			PerformPoseAnimation_Movement();
			PerformPoseAnimation_Handheld();
			PerformPoseAnimation_Attack();
			PoseUpdate_Items();
			PoseUpdate_HeadTwist();

			Wing.Draw(this);
			Tail.Draw(this);
			Face.Draw(this);
			Hair.Draw(this);
			Ear.Draw(this);
			Horn.Draw(this);

			Cloth.DrawHeadSuit(this);
			Cloth.DrawBodySuit(this);
			Cloth.DrawHipSuit(this);
			Cloth.DrawHandSuit(this);
			Cloth.DrawFootSuit(this);

			PoseUpdate_HeadRotate();
			DrawBodyPart(cellIndexStart);

		}


		// Pipeline
		private void ResetPoseToDefault () {

			int bounce = CurrentRenderingBounce;
			int facingSign = FacingRight ? 1 : -1;

			PoseRootX = 0;
			PoseTwist = 0;
			HeadTwist = 0;
			HeadRotation = 0;

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
			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;
			int hipBorderL = FacingRight ? Hip.Border.left : Hip.Border.right;
			int hipBorderR = FacingRight ? Hip.Border.right : Hip.Border.left;

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
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Width = ShoulderL.SizeX;
			ShoulderL.Height = ShoulderL.SizeY;
			ShoulderL.PivotX = 1000;
			ShoulderL.PivotY = 1000;

			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Width = -ShoulderR.SizeX;
			ShoulderR.Height = ShoulderR.SizeY;
			ShoulderR.PivotX = 1000;
			ShoulderR.PivotY = 1000;

			// Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.Z = (FacingFront ? facingSign * POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
			UpperArmL.Width = UpperArmL.SizeX;
			UpperArmL.Height = upperArmHeight;
			UpperArmL.PivotX = 1000;
			UpperArmL.PivotY = 1000;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.Z = (FacingFront ? facingSign * -POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
			UpperArmR.Width = UpperArmR.SizeX;
			UpperArmR.Height = upperArmHeight;
			UpperArmR.PivotX = 0;
			UpperArmR.PivotY = 1000;

			ShoulderL.Z = UpperArmL.Z - 1;
			ShoulderR.Z = UpperArmR.Z - 1;

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
			HandL.Z = (FacingFront && FacingRight ? POSE_Z_HAND : -POSE_Z_HAND_CASUAL);
			HandL.Width = HandL.SizeX;
			HandL.Height = HandL.SizeY;
			HandL.PivotX = 1000;
			HandL.PivotY = 1000;

			HandR.X = LowerArmR.X;
			HandR.Y = LowerArmR.Y - LowerArmR.Height;
			HandR.Z = (FacingFront && !FacingRight ? POSE_Z_HAND : -POSE_Z_HAND_CASUAL);
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


		private void PerformPoseAnimation_Movement () {

			HandGrabScaleL = FacingRight ? 1000 : -1000;
			HandGrabScaleR = FacingRight ? 1000 : -1000;

			switch (AnimatedPoseType) {

				case CharacterPoseAnimationType.TakingDamage:
					AnimationLibrary.Damage();
					break;

				case CharacterPoseAnimationType.Sleep:
					AnimationLibrary.Sleep();
					break;

				case CharacterPoseAnimationType.PassOut:
					AnimationLibrary.PassOut();
					break;

				case CharacterPoseAnimationType.Dash:
					AnimationLibrary.Dash();
					break;

				case CharacterPoseAnimationType.Rolling:
					AnimationLibrary.Rolling();
					break;

				case CharacterPoseAnimationType.Idle:
					AnimationLibrary.Idle();
					break;

				case CharacterPoseAnimationType.Walk:
					AnimationLibrary.Walk();
					break;

				case CharacterPoseAnimationType.Run:
					AnimationLibrary.Run();
					break;

				case CharacterPoseAnimationType.JumpUp:
					AnimationLibrary.JumpUp();
					break;

				case CharacterPoseAnimationType.JumpDown:
					AnimationLibrary.JumpDown();
					break;

				case CharacterPoseAnimationType.SwimIdle:
					AnimationLibrary.SwimIdle();
					break;

				case CharacterPoseAnimationType.SwimMove:
					AnimationLibrary.SwimMove();
					break;

				case CharacterPoseAnimationType.SquatIdle:
					AnimationLibrary.SquatIdle();
					break;

				case CharacterPoseAnimationType.SquatMove:
					AnimationLibrary.SquatMove();
					break;

				case CharacterPoseAnimationType.Rush:
					AnimationLibrary.Rush();
					break;

				case CharacterPoseAnimationType.Pound:
					AnimationLibrary.Pound();
					break;

				case CharacterPoseAnimationType.Spin:
					AnimationLibrary.Spin();
					break;

				case CharacterPoseAnimationType.Climb:
					AnimationLibrary.Climb();
					break;

				case CharacterPoseAnimationType.Fly:
					AnimationLibrary.Fly();
					break;

				case CharacterPoseAnimationType.Slide:
					AnimationLibrary.Slide();
					break;

				case CharacterPoseAnimationType.GrabTop:
					AnimationLibrary.GrabTop();
					break;

				case CharacterPoseAnimationType.GrabSide:
					AnimationLibrary.GrabSide();
					break;
			}

			// Validate
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);

			// Make Global Pos Ready
			CalculateBodypartGlobalPosition();

		}


		private void PerformPoseAnimation_Handheld () {

			if (IsAttacking) return;

			if (AnimatedPoseType switch {
				CharacterPoseAnimationType.Idle => true,
				CharacterPoseAnimationType.Walk => true,
				CharacterPoseAnimationType.Run => true,
				CharacterPoseAnimationType.JumpUp => true,
				CharacterPoseAnimationType.JumpDown => true,
				CharacterPoseAnimationType.SwimIdle => true,
				CharacterPoseAnimationType.SwimMove => true,
				CharacterPoseAnimationType.SquatIdle => true,
				CharacterPoseAnimationType.SquatMove => true,
				CharacterPoseAnimationType.Pound => true,
				CharacterPoseAnimationType.Fly => true,
				CharacterPoseAnimationType.Rush => EquippingWeaponHeld == WeaponHandHeld.Pole,
				CharacterPoseAnimationType.Dash => EquippingWeaponHeld == WeaponHandHeld.Pole,
				CharacterPoseAnimationType.Rolling => EquippingWeaponHeld == WeaponHandHeld.Bow || EquippingWeaponHeld == WeaponHandHeld.Firearm,
				_ => false,
			}) {
				// Override Handheld
				switch (EquippingWeaponHeld) {
					case WeaponHandHeld.DoubleHanded:
						AnimationLibrary.HandHeld_Double();
						break;
					case WeaponHandHeld.Bow:
						AnimationLibrary.HandHeld_Bow();
						break;
					case WeaponHandHeld.Firearm:
						AnimationLibrary.HandHeld_Firearm();
						break;
					case WeaponHandHeld.Pole:
						if (EquippingWeaponType == WeaponType.Magic) {
							AnimationLibrary.HandHeld_Magic_Pole();
						} else {
							AnimationLibrary.HandHeld_Pole();
						}
						break;
					case WeaponHandHeld.OneOnEachHand:
						if (IsChargingAttack) AnimationLibrary.HandHeld_Charging_EachHand();
						break;
					case WeaponHandHeld.Float:
						if (IsChargingAttack) AnimationLibrary.HandHeld_Magic_Float_Charging();
						break;
					default:
						if (IsChargingAttack) AnimationLibrary.HandHeld_Charging();
						break;
				}
				CalculateBodypartGlobalPosition();
			} else {
				// Redirect Handheld
				if (
					EquippingWeaponHeld == WeaponHandHeld.DoubleHanded ||
					EquippingWeaponHeld == WeaponHandHeld.Bow ||
					EquippingWeaponHeld == WeaponHandHeld.Firearm ||
					EquippingWeaponHeld == WeaponHandHeld.Pole
				) {
					EquippingWeaponHeld = WeaponHandHeld.SingleHanded;
				}
			}

		}


		private void PerformPoseAnimation_Attack () {

			if (!IsAttacking) return;

			if (MovementLoseRateOnAttack == 0) {
				ResetPoseToDefault();
			}

			HandGrabScaleL = HandGrabScaleR = FacingRight ? 1000 : -1000;
			HandGrabAttackTwistL = HandGrabAttackTwistR = 1000;

			switch (EquippingWeaponType) {
				default:
				case WeaponType.Hand:
					Attack_Hand();
					break;
				case WeaponType.Sword:
				case WeaponType.Axe:
				case WeaponType.Hammer:
				case WeaponType.Flail:
				case WeaponType.Hook:
				case WeaponType.Throwing:
					Attack_Wave();
					break;
				case WeaponType.Polearm:
					Attack_Polearm();
					break;
				case WeaponType.Claw:
					Attack_Scratch();
					break;
				case WeaponType.Ranged:
					Attack_Ranged();
					break;
				case WeaponType.Magic:
					Attack_Magic();
					break;
			}

			CalculateBodypartGlobalPosition();
		}


		private void PoseUpdate_Items () {

			// Equipment
			for (int i = 0; i < EquipmentTypeCount; i++) {
				GetEquippingItem((EquipmentType)i)?.PoseAnimationUpdate_FromEquipment(this);
			}

			// Inventory
			int iCount = GetInventoryCapacity();
			for (int i = 0; i < iCount; i++) {
				GetItemFromInventory(i)?.PoseAnimationUpdate_FromInventory(this);
			}
		}


		private void PoseUpdate_HeadTwist () {

			//HeadTwist = QTest.Int["t", 0, -1000, 1000];
			//HeadRotation = QTest.Int["r", 0, -90, 90];

			if (HeadTwist == 0) return;
			if (!Head.FrontSide) {
				HeadTwist = 0;
				return;
			}
			HeadTwist = HeadTwist.Clamp(-1000, 1000);
			Head.Width = Head.Width.Sign() * (Head.Width.Abs() - (Head.Width * HeadTwist).Abs() / 2000);
			Head.X += Head.Width.Abs() * HeadTwist / 2000;
			Head.GlobalX = X + PoseRootX + Head.X;
		}


		private void PoseUpdate_HeadRotate () {
			if (HeadRotation == 0) return;
			HeadRotation = HeadRotation.Clamp(-90, 90);
			int offsetY = Head.Height.Abs() * HeadRotation.Abs() / 360;
			Head.Rotation += HeadRotation;
			Head.Y -= offsetY;
			Head.GlobalY -= offsetY;
		}


		private void DrawBodyPart (int cellIndexStart) {

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
				if (bodyPart == Body && !bodyPart.Border.IsZero) {
					CellRenderer.Draw_9Slice(
						id,
						X + PoseRootX + bodyPart.X,
						Y + PoseRootY + bodyPart.Y,
						bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
						bodyPart.Tint, bodyPart.Z
					);
				} else {
					CellRenderer.Draw(
						id,
						X + PoseRootX + bodyPart.X,
						Y + PoseRootY + bodyPart.Y,
						bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
						bodyPart.Tint, bodyPart.Z
					);
				}
			}

			// Final
			PoseZOffset -= 40;
			if (CellRenderer.GetCells(out var cells, out int count)) {
				// Z Offset
				for (int i = cellIndexStart; i < count; i++) {
					cells[i].Z += PoseZOffset;
				}
			}

		}


		#endregion




		#region --- LGC ---


		public void CalculateBodypartGlobalPosition () {
			foreach (var part in BodyParts) {
				part.GlobalX = X + PoseRootX + part.X;
				part.GlobalY = Y + PoseRootY + part.Y;
			}
		}


		// Attack Animations
		private void Attack_Hand () {
			AttackStyleLoop = 2;
			switch (AttackStyleIndex % AttackStyleLoop) {
				default:
					AnimationLibrary.Attack_Hand_Punch();
					break;
				case 1:
					AnimationLibrary.Attack_Hand_Smash();
					break;
			}
		}


		private void Attack_Wave () {
			int style;
			switch (EquippingWeaponHeld) {

				// Single Handed
				case WeaponHandHeld.SingleHanded:
					AttackStyleLoop = 4;
					style =
						LastAttackCharged ||
						EquippingWeaponType == WeaponType.Throwing ||
						EquippingWeaponType == WeaponType.Flail ?
						0 : AttackStyleIndex % AttackStyleLoop;
					switch (style) {
						default:
							AnimationLibrary.Attack_WaveSingleHanded_SmashDown();
							break;
						case 1:
							AnimationLibrary.Attack_WaveSingleHanded_SmashUp();
							break;
						case 2:
							AnimationLibrary.Attack_WaveSingleHanded_SlashIn();
							break;
						case 3:
							AnimationLibrary.Attack_WaveSingleHanded_SlashOut();
							break;
					}
					break;

				// Double Handed
				case WeaponHandHeld.DoubleHanded:
					AttackStyleLoop = 4;
					style =
						LastAttackCharged ||
						EquippingWeaponType == WeaponType.Throwing ||
						EquippingWeaponType == WeaponType.Flail ?
						0 : AttackStyleIndex % AttackStyleLoop;
					switch (style) {
						default:
							AnimationLibrary.Attack_WaveDoubleHanded_SmashDown();
							break;
						case 1:
							AnimationLibrary.Attack_WaveDoubleHanded_SmashUp();
							break;
						case 2:
							AnimationLibrary.Attack_WaveDoubleHanded_SlashIn();
							break;
						case 3:
							AnimationLibrary.Attack_WaveDoubleHanded_SlashOut();
							break;
					}
					break;

				// Each Hand
				case WeaponHandHeld.OneOnEachHand:
					AttackStyleLoop = 4;
					style = LastAttackCharged ? 0 : AttackStyleIndex % AttackStyleLoop;
					switch (style) {
						default:
							AnimationLibrary.Attack_WaveEachHand_SmashDown();
							break;
						case 1:
							AnimationLibrary.Attack_WaveEachHand_SmashUp();
							break;
						case 2:
							AnimationLibrary.Attack_WaveEachHand_SlashIn();
							break;
						case 3:
							AnimationLibrary.Attack_WaveEachHand_SlashOut();
							break;
					}
					break;

				// Pole
				case WeaponHandHeld.Pole:
					AttackStyleLoop = 4;
					style = LastAttackCharged || EquippingWeaponType == WeaponType.Flail ? 0 : AttackStyleIndex % AttackStyleLoop;
					switch (style) {
						default:
							AnimationLibrary.Attack_WavePolearm_SmashDown();
							break;
						case 1:
							AnimationLibrary.Attack_WavePolearm_SmashUp();
							break;
						case 2:
							AnimationLibrary.Attack_WavePolearm_SlashIn();
							break;
						case 3:
							AnimationLibrary.Attack_WavePolearm_SlashOut();
							break;
					}
					break;
			}
		}


		private void Attack_Polearm () {
			AttackStyleLoop = 8;
			int style = LastAttackCharged ? 0 : AttackStyleIndex % AttackStyleLoop;
			if (LastAttackCharged) style = 4;
			switch (style) {
				default:
					AnimationLibrary.Attack_PokePolearm();
					break;
				case 4:
					AnimationLibrary.Attack_WavePolearm_SmashDown();
					break;
				case 5:
					AnimationLibrary.Attack_WavePolearm_SmashUp();
					break;
				case 6:
					AnimationLibrary.Attack_WavePolearm_SlashIn();
					break;
				case 7:
					AnimationLibrary.Attack_WavePolearm_SlashOut();
					break;
			}
		}


		private void Attack_Scratch () {
			AttackStyleLoop = 3;
			int style = LastAttackCharged ? 2 : AttackStyleIndex % AttackStyleLoop;
			switch (style) {
				default:
					AnimationLibrary.Attack_Scratch_In();
					break;
				case 1:
					AnimationLibrary.Attack_Scratch_Out();
					break;
				case 2:
					AnimationLibrary.Attack_Scratch_Up();
					break;
			}
		}


		private void Attack_Ranged () {
			AttackStyleLoop = 1;
			if (EquippingWeaponHeld == WeaponHandHeld.Bow) {
				AnimationLibrary.Attack_Bow();
			} else {
				AnimationLibrary.Attack_Firearm();
			}
		}


		private void Attack_Magic () {
			AttackStyleLoop = 1;
			switch (EquippingWeaponHeld) {
				default:
				case WeaponHandHeld.Float:
					AnimationLibrary.Attack_Magic_Float();
					break;
				case WeaponHandHeld.SingleHanded:
					AnimationLibrary.Attack_Magic_SingleHanded();
					break;
				case WeaponHandHeld.Pole:
					AnimationLibrary.Attack_Magic_Pole();
					break;
			}
		}


		#endregion




	}
}
