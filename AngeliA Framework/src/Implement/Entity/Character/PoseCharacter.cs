using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;


public enum CharacterAnimationType {
	Idle = 0, Walk, Run,
	JumpUp, JumpDown, SwimIdle, SwimMove,
	SquatIdle, SquatMove, Dash, Rush, Crash, Pound,
	Climb, Fly, Slide, GrabTop, GrabSide, Spin,
	TakingDamage, Sleep, PassOut, Rolling,
}


[RequireSprite("{0}.Head 0", "{0}.Head 1", "{0}.Body", "{0}.Foot", "{0}.Hand", "{0}.Hip", "{0}.LowerArm", "{0}.LowerLeg", "{0}.Shoulder", "{0}.UpperArm", "{0}.UpperLeg")]
public abstract class PoseCharacter : Character {




	#region --- VAR ---


	// Const
	private const int CM_PER_PX = 5;
	private const int A2G = Const.CEL / Const.ART_CEL;
	private static readonly int[] DEFAULT_BODY_PART_ID = { "DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(), };
	private static readonly string[] BODY_PART_NAME = { "Head", "Body", "Hip", "Shoulder", "Shoulder", "UpperArm", "UpperArm", "LowerArm", "LowerArm", "Hand", "Hand", "UpperLeg", "UpperLeg", "LowerLeg", "LowerLeg", "Foot", "Foot", };
	private static readonly int[] DEFAULT_POSE_ANIMATION_IDS = { typeof(PoseAnimation_Idle).AngeHash(), typeof(PoseAnimation_Walk).AngeHash(), typeof(PoseAnimation_Run).AngeHash(), typeof(PoseAnimation_JumpUp).AngeHash(), typeof(PoseAnimation_JumpDown).AngeHash(), typeof(PoseAnimation_SwimIdle).AngeHash(), typeof(PoseAnimation_SwimMove).AngeHash(), typeof(PoseAnimation_SquatIdle).AngeHash(), typeof(PoseAnimation_SquatMove).AngeHash(), typeof(PoseAnimation_Dash).AngeHash(), typeof(PoseAnimation_Rush).AngeHash(), typeof(PoseAnimation_Crash).AngeHash(), typeof(PoseAnimation_Pound).AngeHash(), typeof(PoseAnimation_Climb).AngeHash(), typeof(PoseAnimation_Fly).AngeHash(), typeof(PoseAnimation_Slide).AngeHash(), typeof(PoseAnimation_GrabTop).AngeHash(), typeof(PoseAnimation_GrabSide).AngeHash(), typeof(PoseAnimation_Spin).AngeHash(), typeof(PoseAnimation_Animation_TakingDamage).AngeHash(), typeof(PoseAnimation_Sleep).AngeHash(), typeof(PoseAnimation_PassOut).AngeHash(), typeof(PoseAnimation_Rolling).AngeHash(), };
	private static readonly int[] DEFAULT_POSE_HANDHELD_IDS = { typeof(PoseHandheld_Single).AngeHash(), typeof(PoseHandheld_Double).AngeHash(), typeof(PoseHandheld_EachHand).AngeHash(), typeof(PoseHandheld_Pole).AngeHash(), typeof(PoseHandheld_MagicPole).AngeHash(), typeof(PoseHandheld_Bow).AngeHash(), typeof(PoseHandheld_Shooting).AngeHash(), typeof(PoseHandheld_Float).AngeHash(), };
	private static readonly int[] DEFAULT_POSE_ATTACK_IDS = { typeof(PoseAttack_Hand).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Arrow).AngeHash(), typeof(PoseAttack_Polearm).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Scratch).AngeHash(), typeof(PoseAttack_Magic).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), };
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
	protected static int PoseRenderingZOffset { get; set; } = 0;
	public int BasicRootY { get; private set; } = 0;
	public int PoseRootX { get; set; } = 0;
	public int PoseRootY { get; set; } = 0;
	public int HeadRotation { get; set; } = 0;
	public int HeadTwist { get; set; } = 0;
	public int BodyTwist { get; set; } = 0;
	public int HandGrabRotationL { get; set; } = 0;
	public int HandGrabRotationR { get; set; } = 0;
	public int HandGrabScaleL { get; set; } = 1000;
	public int HandGrabScaleR { get; set; } = 1000;
	public int HandGrabAttackTwistL { get; set; } = 1000;
	public int HandGrabAttackTwistR { get; set; } = 1000;
	public int HideBraidFrame { get; set; } = -1;
	public Color32 SkinColor { get; set; } = new(239, 194, 160, 255);
	public Color32 HairColor { get; set; } = new(51, 51, 51, 255);
	public int CharacterHeight { get; set; } = 160; // in CM
	public bool BodyPartsReady => BodyParts != null;
	public override bool SpinOnGroundPound => Wing.IsPropellerWing(WingID);
	public override bool FlyGlideAvailable => WingID != 0 && !Wing.IsPropellerWing(WingID);
	public override bool FlyAvailable => WingID != 0;
	public override int GrowingHeight => base.GrowingHeight * CharacterHeight / 160;

	// BodyPart
	public BodyPart Head { get; init; } = null;
	public BodyPart Body { get; init; } = null;
	public BodyPart Hip { get; init; } = null;
	public BodyPart ShoulderL { get; init; } = null;
	public BodyPart ShoulderR { get; init; } = null;
	public BodyPart UpperArmL { get; init; } = null;
	public BodyPart UpperArmR { get; init; } = null;
	public BodyPart LowerArmL { get; init; } = null;
	public BodyPart LowerArmR { get; init; } = null;
	public BodyPart HandL { get; init; } = null;
	public BodyPart HandR { get; init; } = null;
	public BodyPart UpperLegL { get; init; } = null;
	public BodyPart UpperLegR { get; init; } = null;
	public BodyPart LowerLegL { get; init; } = null;
	public BodyPart LowerLegR { get; init; } = null;
	public BodyPart FootL { get; init; } = null;
	public BodyPart FootR { get; init; } = null;

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
	private readonly BodyPart[] BodyParts = null;
	private readonly int[] PoseAnimationIDs = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
	private readonly int[] PoseHandheldIDs = { 0, 0, 0, 0, 0, 0, 0, 0, };
	private readonly int[] PoseAttackIDs = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
	private int IgnoreTailFrame = -1;
	private int IgnoreEarFrame = -1;
	private int IgnoreHairFrame = -1;
	private int IgnoreFaceFrame = -1;
	private int IgnoreHornFrame = -1;
	private int IgnoreWingFrame = -1;
	private static int CachedConfigVersion = 0;
	private int LoadedConfigVersion = 0;


	#endregion




	#region --- MSG ---


	[OnUniverseOpen(32)]
	public static void OnUniverseOpen_Pose () {
		if (Game.GlobalFrame != 0) InitializePose();
	}


	[OnGameInitialize]
	public static void InitializePose () {

		// Require Reload
		CachedConfigVersion++;

		// Config Pool
		BodyPartConfigPool.Clear();
		foreach (var type in typeof(PoseCharacter).AllChildClass()) {

			int typeID = type.AngeHash();
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
					if (Renderer.HasSpriteGroup(newID) || Renderer.HasSprite(newID)) {
						id = newID;
					}
				} else {
					// For Other
					if (Renderer.HasSprite(newID)) id = newID;
				}
				config.BodyParts[i] = id;
			}

			// Gadget
			config.FaceID = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Face, out int defaultID) ? defaultID : 0;
			config.HairID = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Hair, out defaultID) ? defaultID : 0;
			config.EarID = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Ear, out defaultID) ? defaultID : 0;
			config.TailID = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Tail, out defaultID) ? defaultID : 0;
			config.WingID = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Wing, out defaultID) ? defaultID : 0;
			config.HornID = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Horn, out defaultID) ? defaultID : 0;

			// Suit
			config.SuitHead = Cloth.TryGetDefaultClothID(typeID, ClothType.Head, out int suitID) ? suitID : 0;
			config.SuitBody = Cloth.TryGetDefaultClothID(typeID, ClothType.Body, out suitID) ? suitID : 0;
			config.SuitHip = Cloth.TryGetDefaultClothID(typeID, ClothType.Hip, out suitID) ? suitID : 0;
			config.SuitHand = Cloth.TryGetDefaultClothID(typeID, ClothType.Hand, out suitID) ? suitID : 0;
			config.SuitFoot = Cloth.TryGetDefaultClothID(typeID, ClothType.Foot, out suitID) ? suitID : 0;

			// Final
			BodyPartConfigPool.Add(typeID, config);

		}

	}


	public PoseCharacter () {
		int len = DEFAULT_BODY_PART_ID.Length;
		BodyParts = new BodyPart[len];
		for (int i = 0; i < len; i++) {
			var bodyPart = BodyParts[i] = new BodyPart();
			bodyPart.SetData(DEFAULT_BODY_PART_ID[i],
				i == 9 || i == 10 || i == 15 || i == 16,
				(i >= 7 && i < 11) || (i >= 13 && i < 17) ? BodyParts[i - 2] : null
			);
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


	public override void OnActivated () {
		base.OnActivated();
		if (CachedConfigVersion != LoadedConfigVersion) {
			LoadedConfigVersion = CachedConfigVersion;
			LoadCharacterFromConfigPool();
		}
	}


	public override void BeforePhysicsUpdate () {
		PoseRenderingZOffset = 0;
		base.BeforePhysicsUpdate();
	}


	protected override void RenderCharacter () {

		if (!BodyPartsReady) return;
		int cellIndexStart = Renderer.GetUsedCellCount();

		ResetPoseToDefault(false);
		PerformPoseAnimation();
		PoseUpdate_HeadTwist();
		PoseUpdate_Items();

		if (Game.GlobalFrame > IgnoreWingFrame) Wing.DrawGadgetFromPool(this);
		if (Game.GlobalFrame > IgnoreTailFrame) Tail.DrawGadgetFromPool(this);
		if (Game.GlobalFrame > IgnoreFaceFrame) Face.DrawGadgetFromPool(this);
		if (Game.GlobalFrame > IgnoreHairFrame) Hair.DrawGadgetFromPool(this);
		if (Game.GlobalFrame > IgnoreEarFrame) Ear.DrawGadgetFromPool(this);
		if (Game.GlobalFrame > IgnoreHornFrame) Horn.DrawGadgetFromPool(this);

		HeadCloth.DrawClothFromPool(this);
		BodyCloth.DrawClothFromPool(this);
		HipCloth.DrawClothFromPool(this);
		HandCloth.DrawClothFromPool(this);
		FootCloth.DrawClothFromPool(this);

		PoseUpdate_HeadRotate();
		DrawBodyPart(cellIndexStart);

	}


	// Pipeline
	private void ResetPoseToDefault (bool motionOnly) {

		int bounce = CurrentRenderingBounce;
		int facingSign = FacingRight ? 1 : -1;

		PoseRootX = 0;
		BodyTwist = 0;
		HeadTwist = 0;
		HeadRotation = 0;

		foreach (var bodypart in BodyParts) {
			bodypart.Rotation = 0;
			bodypart.FrontSide = FacingFront;
			if (!motionOnly) {
				bodypart.Tint = SkinColor;
				bodypart.Covered = BodyPart.CoverMode.None;
			}
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


	private void PerformPoseAnimation () {

		// Movement
		HandGrabScaleL = FacingRight ? 1000 : -1000;
		HandGrabScaleR = FacingRight ? 1000 : -1000;

		PoseAnimation.AnimateFromPool(GetPoseAnimationID(AnimationType), this);

		Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
		Body.Height = Body.Height.GreaterOrEquel(1);
		CalculateBodypartGlobalPosition();

		// Handheld
		if (!IsAttacking) {
			// Normal
			switch (AnimationType) {
				case CharacterAnimationType.Idle:
				case CharacterAnimationType.Walk:
				case CharacterAnimationType.Run:
				case CharacterAnimationType.JumpUp:
				case CharacterAnimationType.JumpDown:
				case CharacterAnimationType.SwimIdle:
				case CharacterAnimationType.SwimMove:
				case CharacterAnimationType.SquatIdle:
				case CharacterAnimationType.SquatMove:
				case CharacterAnimationType.Pound:
				case CharacterAnimationType.Fly:
				case CharacterAnimationType.Rush when EquippingWeaponHeld == WeaponHandheld.Pole:
				case CharacterAnimationType.Dash when EquippingWeaponHeld == WeaponHandheld.Pole:
				case CharacterAnimationType.Rolling when EquippingWeaponHeld == WeaponHandheld.Bow || EquippingWeaponHeld == WeaponHandheld.Shooting:
					// Override Handheld
					PoseAnimation.AnimateFromPool(GetPoseHandheldID(EquippingWeaponHeld), this);
					CalculateBodypartGlobalPosition();
					break;
				default:
					// Redirect Handheld
					if (
						EquippingWeaponHeld == WeaponHandheld.DoubleHanded ||
						EquippingWeaponHeld == WeaponHandheld.Bow ||
						EquippingWeaponHeld == WeaponHandheld.Shooting ||
						EquippingWeaponHeld == WeaponHandheld.Pole
					) {
						EquippingWeaponHeld = WeaponHandheld.SingleHanded;
					}
					break;
			}
		} else {
			// Attacking
			if (CurrentSpeedLoseOnAttack == 0) ResetPoseToDefault(true);
			HandGrabScaleL = HandGrabScaleR = FacingRight ? 1000 : -1000;
			HandGrabAttackTwistL = HandGrabAttackTwistR = 1000;
			PoseAnimation.AnimateFromPool(GetPoseAttackID(EquippingWeaponType), this);
			CalculateBodypartGlobalPosition();
		}

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
		Head.Rotation = HeadRotation.Clamp(-90, 90);
		int offsetY = Head.Height.Abs() * Head.Rotation.Abs() / 360;
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
			if (bodyPart.ID == 0 || bodyPart.IsFullCovered) continue;
			if (bodyPart == Head && Renderer.TryGetSpriteFromGroup(bodyPart.ID, Head.FrontSide ? 0 : 1, out var headSprite, false, true)) {
				Renderer.Draw(
					headSprite,
					X + PoseRootX + bodyPart.X,
					Y + PoseRootY + bodyPart.Y,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
					bodyPart.Tint, bodyPart.Z
				);
			} else {
				Renderer.Draw(
					bodyPart.ID,
					X + PoseRootX + bodyPart.X,
					Y + PoseRootY + bodyPart.Y,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
					bodyPart.Tint, bodyPart.Z
				);
			}

		}

		// Z Offset
		PoseRenderingZOffset -= 40;
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = cellIndexStart; i < count; i++) {
				cells[i].Z += PoseRenderingZOffset;
			}
		}

	}


	#endregion




	#region --- API ---


	public void IgnoreBodyGadget (BodyGadgetType type, int duration = 1) {
		switch (type) {
			case BodyGadgetType.Face:
				IgnoreFaceFrame = Game.GlobalFrame + duration;
				break;
			case BodyGadgetType.Hair:
				IgnoreHairFrame = Game.GlobalFrame + duration;
				break;
			case BodyGadgetType.Ear:
				IgnoreEarFrame = Game.GlobalFrame + duration;
				break;
			case BodyGadgetType.Horn:
				IgnoreHornFrame = Game.GlobalFrame + duration;
				break;
			case BodyGadgetType.Tail:
				IgnoreTailFrame = Game.GlobalFrame + duration;
				break;
			case BodyGadgetType.Wing:
				IgnoreWingFrame = Game.GlobalFrame + duration;
				break;
		}
	}


	// Animation ID
	public int GetPoseAnimationID (CharacterAnimationType type) {
		// From Equipment
		for (int i = 0; i < EquipmentTypeCount; i++) {
			if (GetEquippingItem((EquipmentType)i) is not Equipment equip) continue;
			int equipResult = equip.GetOverrideMovementAnimationID(type, this);
			if (equipResult != 0) return equipResult;
		}
		// From Character Config
		int charResult = PoseAnimationIDs[(int)type];
		if (charResult != 0) return charResult;
		// From Default
		return DEFAULT_POSE_ANIMATION_IDS[(int)type];
	}


	public void SetPoseAnimationID (CharacterAnimationType type, int id) => PoseAnimationIDs[(int)type] = id;


	public int GetPoseHandheldID (WeaponHandheld handheld) {
		// From Equipment
		if (GetEquippingItem(EquipmentType.Weapon) is Weapon weapon) {
			int equipResult = weapon.GetOverrideHandheldAnimationID(this);
			if (equipResult != 0) return equipResult;
		}
		// From Character Config
		int charResult = PoseHandheldIDs[(int)handheld];
		if (charResult != 0) return charResult;
		// From Default
		return DEFAULT_POSE_HANDHELD_IDS[(int)handheld];
	}


	public void SetPoseHandheldID (WeaponHandheld handheld, int id) => PoseHandheldIDs[(int)handheld] = id;


	public int GetPoseAttackID (WeaponType type) {
		// From Equipment
		if (GetEquippingItem(EquipmentType.Weapon) is Weapon weapon) {
			int equipResult = weapon.GetOverrideAttackAnimationID(this);
			if (equipResult != 0) return equipResult;
		}
		// From Character Config
		int charResult = PoseAttackIDs[(int)type];
		if (charResult != 0) return charResult;
		// From Default
		return DEFAULT_POSE_ATTACK_IDS[(int)type];
	}


	public void SetPoseAttackID (WeaponType type, int id) => PoseAttackIDs[(int)type] = id;


	#endregion




	#region --- LGC ---


	private void CalculateBodypartGlobalPosition () {
		foreach (var part in BodyParts) {
			part.GlobalX = X + PoseRootX + part.X;
			part.GlobalY = Y + PoseRootY + part.Y;
		}
	}


	private void LoadCharacterFromConfigPool () {
		int len = DEFAULT_BODY_PART_ID.Length;
		if (BodyPartConfigPool.TryGetValue(TypeID, out var config) && config != null) {

			// Body Part
			for (int i = 0; i < len; i++) {
				var bodyPartID = config.BodyParts[i];
				BodyParts[i].SetData(
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
			// Load Default Bodypart
			for (int i = 0; i < len; i++) {
				BodyParts[i].SetData(
					DEFAULT_BODY_PART_ID[i],
					i == 9 || i == 10 || i == 15 || i == 16,
					(i >= 7 && i < 11) || (i >= 13 && i < 17) ? BodyParts[i - 2] : null
				);
			}
		}

	}


	#endregion




}
