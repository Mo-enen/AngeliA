using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public enum CharacterAnimationType {
	Idle = 0, Walk, Run,
	JumpUp, JumpDown, SwimIdle, SwimMove,
	SquatIdle, SquatMove, Dash, Rush, Crash, Pound,
	Climb, Fly, Slide, GrabTop, GrabSide, Spin,
	TakingDamage, Sleep, PassOut, Rolling,
}


public abstract class PoseCharacter : Character {




	#region --- VAR ---


	// Const
	private const int CM_PER_PX = 5;
	private const int A2G = Const.CEL / Const.ART_CEL;
	private static readonly int[] DEFAULT_BODY_PART_ID = { "DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(), };
	private static readonly string[] BODY_PART_NAME = { "Head", "Body", "Hip", "Shoulder", "Shoulder", "UpperArm", "UpperArm", "LowerArm", "LowerArm", "Hand", "Hand", "UpperLeg", "UpperLeg", "LowerLeg", "LowerLeg", "Foot", "Foot", };
	private static readonly int[] FAILBACK_POSE_ANIMATION_IDS = { typeof(PoseAnimation_Idle).AngeHash(), typeof(PoseAnimation_Walk).AngeHash(), typeof(PoseAnimation_Run).AngeHash(), typeof(PoseAnimation_JumpUp).AngeHash(), typeof(PoseAnimation_JumpDown).AngeHash(), typeof(PoseAnimation_SwimIdle).AngeHash(), typeof(PoseAnimation_SwimMove).AngeHash(), typeof(PoseAnimation_SquatIdle).AngeHash(), typeof(PoseAnimation_SquatMove).AngeHash(), typeof(PoseAnimation_Dash).AngeHash(), typeof(PoseAnimation_Rush).AngeHash(), typeof(PoseAnimation_Crash).AngeHash(), typeof(PoseAnimation_Pound).AngeHash(), typeof(PoseAnimation_Climb).AngeHash(), typeof(PoseAnimation_Fly).AngeHash(), typeof(PoseAnimation_Slide).AngeHash(), typeof(PoseAnimation_GrabTop).AngeHash(), typeof(PoseAnimation_GrabSide).AngeHash(), typeof(PoseAnimation_Spin).AngeHash(), typeof(PoseAnimation_Animation_TakingDamage).AngeHash(), typeof(PoseAnimation_Sleep).AngeHash(), typeof(PoseAnimation_PassOut).AngeHash(), typeof(PoseAnimation_Rolling).AngeHash(), };
	private static readonly int[] FAILBACK_POSE_HANDHELD_IDS = { typeof(PoseHandheld_Single).AngeHash(), typeof(PoseHandheld_Double).AngeHash(), typeof(PoseHandheld_EachHand).AngeHash(), typeof(PoseHandheld_Pole).AngeHash(), typeof(PoseHandheld_MagicPole).AngeHash(), typeof(PoseHandheld_Bow).AngeHash(), typeof(PoseHandheld_Shooting).AngeHash(), typeof(PoseHandheld_Float).AngeHash(), };
	private static readonly int[] FAILBACK_POSE_ATTACK_IDS = { typeof(PoseAttack_Hand).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Arrow).AngeHash(), typeof(PoseAttack_Polearm).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Scratch).AngeHash(), typeof(PoseAttack_Magic).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), };
	private static readonly int ANI_TYPE_COUNT = typeof(CharacterAnimationType).EnumLength();
	private static readonly int HAND_HELD_COUNT = typeof(WeaponHandheld).EnumLength();
	private static readonly int WEAPON_TYPE_COUNT = typeof(WeaponType).EnumLength();
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
	public readonly BuffInt FaceID = new(0);
	public readonly BuffInt HairID = new(0);
	public readonly BuffInt EarID = new(0);
	public readonly BuffInt TailID = new(0);
	public readonly BuffInt WingID = new(0);
	public readonly BuffInt HornID = new(0);

	// Suit
	public readonly BuffInt SuitHead = new(0);
	public readonly BuffInt SuitBody = new(0);
	public readonly BuffInt SuitHip = new(0);
	public readonly BuffInt SuitHand = new(0);
	public readonly BuffInt SuitFoot = new(0);

	// Data
	private static readonly Dictionary<int, CharacterConfig> ConfigPool = new();
	private readonly BodyPart[] BodyParts = null;
	private readonly BuffInt[] PoseAnimationIDs;
	private readonly BuffInt[] PoseHandheldIDs;
	private readonly BuffInt[] PoseAttackIDs;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)] // > Renderer.Initialize()
	public static void InitializePose () {

#if DEBUG
		if (FAILBACK_POSE_ANIMATION_IDS.Length != ANI_TYPE_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_ANIMATION_IDS length have to be {ANI_TYPE_COUNT}");
		}
		if (FAILBACK_POSE_HANDHELD_IDS.Length != HAND_HELD_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_HANDHELD_IDS length have to be {HAND_HELD_COUNT}");
		}
		if (FAILBACK_POSE_ATTACK_IDS.Length != WEAPON_TYPE_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_ATTACK_IDS length have to be {WEAPON_TYPE_COUNT}");
		}
#endif

		// Config Pool
		ConfigPool.Clear();
		string metaRoot = Universe.BuiltIn.CharacterConfigRoot;
		foreach (var type in typeof(PoseCharacter).AllChildClass()) {

			int typeID = type.AngeHash();
			string name = type.Name;

			// Load From File
			string configPath = Util.CombinePaths(metaRoot, $"{name}.json");
			var config = JsonUtil.LoadJsonFromPath<CharacterConfig>(configPath);

			// Create Default Config
			if (config == null) {
				config = CreateCharacterConfigFromSheet(name);
				JsonUtil.SaveJsonToPath(config, configPath, prettyPrint: true);

			}

			// Final
			if (config != null) {
				ConfigPool.Add(typeID, config);
			}

		}

	}


	public PoseCharacter () {
		// [order -64 from stage]
		// Body Part
		int len = DEFAULT_BODY_PART_ID.Length;
		BodyParts = new BodyPart[len];
		for (int i = 0; i < len; i++) {
			var bodyPart = BodyParts[i] = new BodyPart(
				parent: (i >= 7 && i < 11) || (i >= 13 && i < 17) ? BodyParts[i - 2] : null,
				useLimbFlip: i == 9 || i == 10 || i == 15 || i == 16
			);
			bodyPart.SetData(DEFAULT_BODY_PART_ID[i]);
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
		// Ani
		PoseAnimationIDs = new BuffInt[ANI_TYPE_COUNT].FillWithNewValue();
		PoseHandheldIDs = new BuffInt[HAND_HELD_COUNT].FillWithNewValue();
		PoseAttackIDs = new BuffInt[WEAPON_TYPE_COUNT].FillWithNewValue();
		// Load Default Ani
		for (int i = 0; i < ANI_TYPE_COUNT; i++) {
			PoseAnimation.TryGetPoseAnimationDefaultID(TypeID, (CharacterAnimationType)i, out int id);
			PoseAnimationIDs[i].BaseValue = id != 0 ? id : FAILBACK_POSE_ANIMATION_IDS[i];
		}
		for (int i = 0; i < HAND_HELD_COUNT; i++) {
			PoseAnimation.TryGetHandheldDefaultID(TypeID, (WeaponHandheld)i, out int id);
			PoseHandheldIDs[i].BaseValue = id != 0 ? id : FAILBACK_POSE_HANDHELD_IDS[i];
		}
		for (int i = 0; i < WEAPON_TYPE_COUNT; i++) {
			PoseAnimation.TryGetAttackDefaultID(TypeID, (WeaponType)i, out int id);
			PoseAttackIDs[i].BaseValue = id != 0 ? id : FAILBACK_POSE_ATTACK_IDS[i];
		}
		// Load From Pool
		LoadCharacterFromConfigPool();
	}


	public override void BeforeUpdate () {
		PoseRenderingZOffset = 0;
		base.BeforeUpdate();
	}


	protected override void RenderCharacter () {

		if (!BodyPartsReady) return;
		int cellIndexStart = Renderer.GetUsedCellCount();

		ResetPoseToDefault(false);
		PerformPoseAnimation();
		PoseUpdate_HeadTwist();
		PoseUpdate_Items();

		Wing.DrawGadgetFromPool(this);
		Tail.DrawGadgetFromPool(this);
		Face.DrawGadgetFromPool(this);
		Hair.DrawGadgetFromPool(this);
		Ear.DrawGadgetFromPool(this);
		Horn.DrawGadgetFromPool(this);

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
				bodypart.Tint = Color32.WHITE;
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
		UpperLegL.FlexableSizeY = UpperLegR.FlexableSizeY = UpperLegL.SizeY * PoseRootY / legRootSize;
		LowerLegL.FlexableSizeY = LowerLegR.FlexableSizeY = LowerLegL.SizeY * PoseRootY / legRootSize;
		FootL.FlexableSizeY = FootR.FlexableSizeY = FootL.SizeY * PoseRootY / legRootSize;
		UpperArmL.FlexableSizeY = UpperArmR.FlexableSizeY = UpperArmL.SizeY * targetUnitHeight / defaultCharHeight;
		LowerArmL.FlexableSizeY = LowerArmR.FlexableSizeY = LowerArmL.SizeY * targetUnitHeight / defaultCharHeight;
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
		UpperArmL.Height = UpperArmL.FlexableSizeY;
		UpperArmL.PivotX = 1000;
		UpperArmL.PivotY = 1000;

		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmR.Z = (FacingFront ? facingSign * -POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
		UpperArmR.Width = UpperArmR.SizeX;
		UpperArmR.Height = UpperArmR.FlexableSizeY;
		UpperArmR.PivotX = 0;
		UpperArmR.PivotY = 1000;

		ShoulderL.Z = UpperArmL.Z - 1;
		ShoulderR.Z = UpperArmR.Z - 1;

		LowerArmL.X = UpperArmL.X;
		LowerArmL.Y = UpperArmL.Y - UpperArmL.Height;
		LowerArmL.Z = (FacingFront ? facingSign * POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
		LowerArmL.Width = LowerArmL.SizeX;
		LowerArmL.Height = LowerArmL.FlexableSizeY;
		LowerArmL.PivotX = 1000;
		LowerArmL.PivotY = 1000;

		LowerArmR.X = UpperArmR.X;
		LowerArmR.Y = UpperArmR.Y - UpperArmR.Height;
		LowerArmR.Z = (FacingFront ? facingSign * -POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
		LowerArmR.Width = LowerArmR.SizeX;
		LowerArmR.Height = LowerArmR.FlexableSizeY;
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
		UpperLegL.Height = UpperLegL.FlexableSizeY;
		UpperLegL.PivotX = 0;
		UpperLegL.PivotY = 1000;

		UpperLegR.X = Hip.X + Hip.Width.Abs() / 2 - hipBorderR;
		UpperLegR.Y = Hip.Y;
		UpperLegR.Z = POSE_Z_UPPERLEG;
		UpperLegR.Width = UpperLegR.SizeX;
		UpperLegR.Height = UpperLegR.FlexableSizeY;
		UpperLegR.PivotX = 1000;
		UpperLegR.PivotY = 1000;

		LowerLegL.X = UpperLegL.X;
		LowerLegL.Y = UpperLegL.Y - UpperLegL.Height;
		LowerLegL.Z = POSE_Z_LOWERLEG;
		LowerLegL.Width = LowerLegL.SizeX;
		LowerLegL.Height = LowerLegL.FlexableSizeY;
		LowerLegL.PivotX = 0;
		LowerLegL.PivotY = 1000;
		if (FacingRight) LowerLegL.X -= A2G;

		LowerLegR.X = UpperLegR.X;
		LowerLegR.Y = UpperLegR.Y - UpperLegR.Height;
		LowerLegR.Z = POSE_Z_LOWERLEG;
		LowerLegR.Width = LowerLegR.SizeX;
		LowerLegR.Height = LowerLegR.FlexableSizeY;
		LowerLegR.PivotX = 1000;
		LowerLegR.PivotY = 1000;
		if (!FacingRight) LowerLegR.X += A2G;

		FootL.X = FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
		FootL.Y = LowerLegL.Y - LowerLegL.Height;
		FootL.Z = POSE_Z_FOOT;
		FootL.Width = facingSign * FootL.SizeX;
		FootL.Height = FootL.FlexableSizeY;
		FootL.PivotX = 0;
		FootL.PivotY = 1000;

		FootR.X = FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
		FootR.Y = LowerLegR.Y - LowerLegR.Height;
		FootR.Z = POSE_Z_FOOT;
		FootR.Width = facingSign * FootR.SizeX;
		FootR.Height = FootR.FlexableSizeY;
		FootR.PivotX = 0;
		FootR.PivotY = 1000;

	}


	private void PerformPoseAnimation () {

		// Movement
		HandGrabScaleL = FacingRight ? 1000 : -1000;
		HandGrabScaleR = FacingRight ? 1000 : -1000;

		PoseAnimation.AnimateFromPool(PoseAnimationIDs[(int)AnimationType], this);

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
					PoseAnimation.AnimateFromPool(PoseHandheldIDs[(int)EquippingWeaponHeld], this);
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
			PoseAnimation.AnimateFromPool(PoseAttackIDs[(int)EquippingWeaponType], this);
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


	// Config
	public static CharacterConfig CreateCharacterConfigFromSheet (string characterAngeName) {

		int typeID = characterAngeName.AngeHash();
		int bodyPartLen = DEFAULT_BODY_PART_ID.Length;
		var config = new CharacterConfig();

		// Body Parts
		for (int i = 0; i < bodyPartLen; i++) {
			int id = DEFAULT_BODY_PART_ID[i];
			int newID = $"{characterAngeName}.{BODY_PART_NAME[i]}".AngeHash();
			if (i == 0) {
				// For Head
				if (Renderer.HasSpriteGroup(newID) || Renderer.HasSprite(newID)) {
					id = newID;
				}
			} else {
				// For Other
				if (Renderer.HasSprite(newID)) id = newID;
			}

			switch (i) {
				case 0: config.Head = id; break;
				case 1: config.Body = id; break;
				case 2: config.Hip = id; break;
				case 3 or 4: config.Shoulder = id; break;
				case 5 or 6: config.UpperArm = id; break;
				case 7 or 8: config.LowerArm = id; break;
				case 9 or 10: config.Hand = id; break;
				case 11 or 12: config.UpperLeg = id; break;
				case 13 or 14: config.LowerLeg = id; break;
				case 15 or 16: config.Foot = id; break;
			}
		}

		// Gadget
		config.Face = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Face, out int defaultId0) ? defaultId0 : DefaultFace.TYPE_ID;
		config.Hair = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Hair, out int defaultId1) ? defaultId1 : DefaultHair.TYPE_ID;
		config.Ear = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Ear, out int defaultId2) ? defaultId2 : 0;
		config.Tail = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Tail, out int defaultId3) ? defaultId3 : 0;
		config.Wing = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Wing, out int defaultId4) ? defaultId4 : 0;
		config.Horn = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Horn, out int defaultId5) ? defaultId5 : 0;

		// Suit
		config.SuitHead = Cloth.TryGetDefaultClothID(typeID, ClothType.Head, out int suitId0) ? suitId0 : 0;
		config.SuitBody = Cloth.TryGetDefaultClothID(typeID, ClothType.Body, out int suitId1) ? suitId1 : DefaultBodySuit.TYPE_ID;
		config.SuitHip = Cloth.TryGetDefaultClothID(typeID, ClothType.Hip, out int suitId2) ? suitId2 : DefaultHipSuit.TYPE_ID;
		config.SuitHand = Cloth.TryGetDefaultClothID(typeID, ClothType.Hand, out int suitId3) ? suitId3 : 0;
		config.SuitFoot = Cloth.TryGetDefaultClothID(typeID, ClothType.Foot, out int suitId4) ? suitId4 : DefaultFootSuit.TYPE_ID;

		return config;

	}


	public void LoadCharacterFromConfig (CharacterConfig config) {

		CharacterHeight = config.CharacterHeight;

		// Body Part
		Head.SetData(config.Head);
		Body.SetData(config.Body);
		Hip.SetData(config.Hip);
		ShoulderL.SetData(config.Shoulder);
		ShoulderR.SetData(config.Shoulder);
		UpperArmL.SetData(config.UpperArm);
		UpperArmR.SetData(config.UpperArm);
		LowerArmL.SetData(config.LowerArm);
		LowerArmR.SetData(config.LowerArm);
		HandL.SetData(config.Hand);
		HandR.SetData(config.Hand);
		UpperLegL.SetData(config.UpperLeg);
		UpperLegR.SetData(config.UpperLeg);
		LowerLegL.SetData(config.LowerLeg);
		LowerLegR.SetData(config.LowerLeg);
		FootL.SetData(config.Foot);
		FootR.SetData(config.Foot);

		// Gadget
		FaceID.BaseValue = config.Face;
		HairID.BaseValue = config.Hair;
		EarID.BaseValue = config.Ear;
		TailID.BaseValue = config.Tail;
		WingID.BaseValue = config.Wing;
		HornID.BaseValue = config.Horn;

		// Suit
		SuitHead.BaseValue = config.SuitHead;
		SuitBody.BaseValue = config.SuitBody;
		SuitHip.BaseValue = config.SuitHip;
		SuitHand.BaseValue = config.SuitHand;
		SuitFoot.BaseValue = config.SuitFoot;

	}


	public void LoadCharacterFromConfigPool () {
		if (ConfigPool.TryGetValue(TypeID, out var config)) {
			LoadCharacterFromConfig(config);
		} else {
			// Load Default Bodypart
			for (int i = 0; i < DEFAULT_BODY_PART_ID.Length; i++) {
				BodyParts[i].SetData(DEFAULT_BODY_PART_ID[i]);
			}
		}
	}


	public void SaveCharacterToConfig () {

		if (!ConfigPool.TryGetValue(TypeID, out var config)) return;

		config.CharacterHeight = CharacterHeight;

		// Body Part
		config.Head = Head.ID;
		config.Body = Body.ID;
		config.Hip = Hip.ID;
		config.Shoulder = ShoulderL.ID;
		config.Shoulder = ShoulderR.ID;
		config.UpperArm = UpperArmL.ID;
		config.UpperArm = UpperArmR.ID;
		config.LowerArm = LowerArmL.ID;
		config.LowerArm = LowerArmR.ID;
		config.Hand = HandL.ID;
		config.Hand = HandR.ID;
		config.UpperLeg = UpperLegL.ID;
		config.UpperLeg = UpperLegR.ID;
		config.LowerLeg = LowerLegL.ID;
		config.LowerLeg = LowerLegR.ID;
		config.Foot = FootL.ID;
		config.Foot = FootR.ID;

		// Gadget
		config.Face = FaceID.BaseValue;
		config.Hair = HairID.BaseValue;
		config.Ear = EarID.BaseValue;
		config.Tail = TailID.BaseValue;
		config.Wing = WingID.BaseValue;
		config.Horn = HornID.BaseValue;

		// Suit
		config.SuitHead = SuitHead.BaseValue;
		config.SuitBody = SuitBody.BaseValue;
		config.SuitHip = SuitHip.BaseValue;
		config.SuitHand = SuitHand.BaseValue;
		config.SuitFoot = SuitFoot.BaseValue;

	}


	// Animation ID
	public void OverridePoseAnimation (CharacterAnimationType type, int id, int duration = 1) => PoseAnimationIDs[(int)type].Override(id, duration);


	public void OverridePoseHandheldAnimation (WeaponHandheld handheld, int id, int duration = 1) => PoseHandheldIDs[(int)handheld].Override(id, duration);


	public void OverridePoseAttackAnimation (WeaponType type, int id, int duration = 1) => PoseAttackIDs[(int)type].Override(id, duration);


	#endregion




	#region --- LGC ---


	private void CalculateBodypartGlobalPosition () {
		foreach (var part in BodyParts) {
			part.GlobalX = X + PoseRootX + part.X;
			part.GlobalY = Y + PoseRootY + part.Y;
		}
	}


	#endregion




}
