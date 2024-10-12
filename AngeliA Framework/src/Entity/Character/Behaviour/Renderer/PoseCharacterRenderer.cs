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


public class PoseCharacterRenderer : CharacterRenderer {




	#region --- VAR ---


	// Const
	public const int BODY_PART_COUNT = 17;
	public const int CM_PER_PX = 5;
	private const int A2G = Const.CEL / Const.ART_CEL;
	private static readonly int[] DEFAULT_BODY_PART_ID = ["DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(),];
	private static readonly string[] BODY_PART_NAME = ["Head", "Body", "Hip", "Shoulder", "Shoulder", "UpperArm", "UpperArm", "LowerArm", "LowerArm", "Hand", "Hand", "UpperLeg", "UpperLeg", "LowerLeg", "LowerLeg", "Foot", "Foot",];
	private static readonly int[] DEFAULT_POSE_ANIMATION_IDS = [typeof(PoseAnimation_Idle).AngeHash(), typeof(PoseAnimation_Walk).AngeHash(), typeof(PoseAnimation_Run).AngeHash(), typeof(PoseAnimation_JumpUp).AngeHash(), typeof(PoseAnimation_JumpDown).AngeHash(), typeof(PoseAnimation_SwimIdle).AngeHash(), typeof(PoseAnimation_SwimMove).AngeHash(), typeof(PoseAnimation_SquatIdle).AngeHash(), typeof(PoseAnimation_SquatMove).AngeHash(), typeof(PoseAnimation_Dash).AngeHash(), typeof(PoseAnimation_Rush).AngeHash(), typeof(PoseAnimation_Crash).AngeHash(), typeof(PoseAnimation_Pound).AngeHash(), typeof(PoseAnimation_Climb).AngeHash(), typeof(PoseAnimation_Fly).AngeHash(), typeof(PoseAnimation_Slide).AngeHash(), typeof(PoseAnimation_GrabTop).AngeHash(), typeof(PoseAnimation_GrabSide).AngeHash(), typeof(PoseAnimation_Spin).AngeHash(), typeof(PoseAnimation_Animation_TakingDamage).AngeHash(), typeof(PoseAnimation_Sleep).AngeHash(), typeof(PoseAnimation_PassOut).AngeHash(), typeof(PoseAnimation_Rolling).AngeHash(),];
	private static readonly int[] DEFAULT_POSE_HANDHELD_IDS = [typeof(PoseHandheld_Single).AngeHash(), typeof(PoseHandheld_Double).AngeHash(), typeof(PoseHandheld_EachHand).AngeHash(), typeof(PoseHandheld_Pole).AngeHash(), typeof(PoseHandheld_MagicPole).AngeHash(), typeof(PoseHandheld_Bow).AngeHash(), typeof(PoseHandheld_Shooting).AngeHash(), typeof(PoseHandheld_Float).AngeHash(),];
	private static readonly int[] DEFAULT_POSE_ATTACK_IDS = [typeof(PoseAttack_Hand).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Ranged).AngeHash(), typeof(PoseAttack_Polearm).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Scratch).AngeHash(), typeof(PoseAttack_Magic).AngeHash(), typeof(PoseAttack_Wave).AngeHash(), typeof(PoseAttack_Build).AngeHash(), typeof(PoseAttack_Build).AngeHash(),];
	private static readonly int ANI_TYPE_COUNT = typeof(CharacterAnimationType).EnumLength();
	private static readonly int HAND_HELD_COUNT = typeof(ToolHandheld).EnumLength();
	private static readonly int TOOL_TYPE_COUNT = typeof(ToolType).EnumLength();
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
	public int BasicRootY { get; private set; } = 0;
	public int PoseRootX { get; set; } = 0;
	public int PoseRootY { get; set; } = 0;
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
	public int RenderedCellZ { get; private set; } = 0;

	// BodyPart
	public readonly BodyPart[] BodyParts = new BodyPart[BODY_PART_COUNT];
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
	public readonly FrameBasedInt FaceID = new(0);
	public readonly FrameBasedInt HairID = new(0);
	public readonly FrameBasedInt EarID = new(0);
	public readonly FrameBasedInt TailID = new(0);
	public readonly FrameBasedInt WingID = new(0);
	public readonly FrameBasedInt HornID = new(0);

	// Suit
	public readonly FrameBasedInt SuitHead = new(0);
	public readonly FrameBasedInt SuitBody = new(0);
	public readonly FrameBasedInt SuitHip = new(0);
	public readonly FrameBasedInt SuitHand = new(0);
	public readonly FrameBasedInt SuitFoot = new(0);

	// Data
	private static readonly Dictionary<int, CharacterRenderingConfig> ConfigPool_Rendering = [];
	private static readonly int EquipmentTypeCount = System.Enum.GetValues(typeof(EquipmentType)).Length;
	private static int GlobalPoseRenderingZOffset;
	private static int RenderingConfigGlobalVersion = -1;
	private int LocalRenderingConfigVersion = int.MinValue;
	private readonly FrameBasedInt[] PoseAnimationIDs;
	private readonly FrameBasedInt[] PoseHandheldIDs;
	private readonly FrameBasedInt[] PoseAttackIDs;
	private readonly FrameBasedInt ManualPoseAnimationID = new(0);


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static TaskResult InitializePose () {

		if (!Renderer.IsReady) return TaskResult.Continue;

#if DEBUG
		if (DEFAULT_POSE_ANIMATION_IDS.Length != ANI_TYPE_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_ANIMATION_IDS length have to be {ANI_TYPE_COUNT}");
		}
		if (DEFAULT_POSE_HANDHELD_IDS.Length != HAND_HELD_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_HANDHELD_IDS length have to be {HAND_HELD_COUNT}");
		}
		if (DEFAULT_POSE_ATTACK_IDS.Length != TOOL_TYPE_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_ATTACK_IDS length have to be {TOOL_TYPE_COUNT}");
		}
#endif

		ReloadRenderingConfigPoolFromFileAndSheet();
		return TaskResult.End;
	}


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () => ReloadRenderingConfigPoolFromFileAndSheet();


	[BeforeBeforeUpdate]
	internal static void BeforeBeforeUpdate () => GlobalPoseRenderingZOffset = 0;


	public PoseCharacterRenderer (Character target) : base(target) {
		// Body Part
		for (int i = 0; i < BODY_PART_COUNT; i++) {
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
		PoseAnimationIDs = new FrameBasedInt[ANI_TYPE_COUNT].FillWithNewValue();
		PoseHandheldIDs = new FrameBasedInt[HAND_HELD_COUNT].FillWithNewValue();
		PoseAttackIDs = new FrameBasedInt[TOOL_TYPE_COUNT].FillWithNewValue();
		// Load Default Ani
		for (int i = 0; i < ANI_TYPE_COUNT; i++) {
			PoseAnimationIDs[i].BaseValue = DEFAULT_POSE_ANIMATION_IDS[i];
		}
		for (int i = 0; i < HAND_HELD_COUNT; i++) {
			PoseHandheldIDs[i].BaseValue = DEFAULT_POSE_HANDHELD_IDS[i];
		}
		for (int i = 0; i < TOOL_TYPE_COUNT; i++) {
			PoseAttackIDs[i].BaseValue = DEFAULT_POSE_ATTACK_IDS[i];
		}
		SyncRenderingConfigFromPool();
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		SyncRenderingConfigFromPool();
		// Give Default Wing
		if (WingID.BaseValue == 0 && TargetCharacter.Movement.FlyAvailable) {
			WingID.Override(
				TargetCharacter.Movement.GlideOnFlying.BaseValue ? DefaultWing.TYPE_ID : DefaultPropellerWing.TYPE_ID,
				duration: 1
			);
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (BodyParts == null) return;
		int cellIndexStart = Renderer.GetUsedCellCount();
		ResetPoseToDefault(false);
		AnimateForPose();
		PoseUpdate_HeadTwist();
		PoseUpdate_HeadRotate();
		RenderEquipmentAndInventory();
		RenderBodyGadgets();
		RenderCloths();
		DrawBodyPart(cellIndexStart);
	}


	protected virtual void RenderBodyGadgets () {
		Wing.DrawGadgetFromPool(this);
		Tail.DrawGadgetFromPool(this);
		Face.DrawGadgetFromPool(this);
		Hair.DrawGadgetFromPool(this);
		Ear.DrawGadgetFromPool(this);
		Horn.DrawGadgetFromPool(this);
	}


	protected virtual void RenderCloths () {
		HeadCloth.DrawClothFromPool(this);
		BodyCloth.DrawClothFromPool(this);
		HipCloth.DrawClothFromPool(this);
		HandCloth.DrawClothFromPool(this);
		FootCloth.DrawClothFromPool(this);
	}


	protected virtual void PerformPoseAnimation () {
		if (ManualPoseAnimationID != 0) {
			PoseAnimation.PerformAnimationFromPool(ManualPoseAnimationID, this);
		} else {
			PoseAnimation.PerformAnimationFromPool(PoseAnimationIDs[(int)TargetCharacter.AnimationType], this);
		}
	}


	protected virtual void RenderEquipmentAndInventory () {
		// Equipment
		for (int i = 0; i < EquipmentTypeCount; i++) {
			int id = Inventory.GetEquipment(TargetCharacter.InventoryID, (EquipmentType)i, out int equipmentCount);
			var eq = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			eq?.PoseAnimationUpdate_FromEquipment(TargetCharacter);
		}
		// Inventory
		int invCapacity = Inventory.GetInventoryCapacity(TargetCharacter.InventoryID);
		TargetCharacter.ResetInventoryUpdate(invCapacity);
		for (int i = 0; i < invCapacity; i++) {
			int id = Inventory.GetItemAt(TargetCharacter.InventoryID, i, out int stackCount);
			var item = id != 0 ? ItemSystem.GetItem(id) : null;
			if (item == null || !item.CheckUpdateAvailable(TargetCharacter.TypeID)) continue;
			item.PoseAnimationUpdate_FromInventory(TargetCharacter, stackCount);
		}
	}


	// Pipeline
	private void ResetPoseToDefault (bool motionOnly) {

		var Movement = TargetCharacter.Movement;
		int bounce = CurrentRenderingBounce;
		int facingSign = Movement.FacingRight ? 1 : -1;

		PoseRootX = 0;
		BodyTwist = 0;
		HeadTwist = 0;

		foreach (var bodypart in BodyParts) {
			bodypart.Rotation = 0;
			bodypart.FrontSide = Movement.FacingFront;
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
		int bodyBorderU = Body.Border.up * targetUnitHeight / defaultCharHeight * Body.Height.Abs() / Body.SizeY;
		int bodyBorderL = (Movement.FacingRight ? Body.Border.left : Body.Border.right) * Body.Width.Abs() / Body.SizeX;
		int bodyBorderR = (Movement.FacingRight ? Body.Border.right : Body.Border.left) * Body.Width.Abs() / Body.SizeX;
		int hipBorderL = (Movement.FacingRight ? Hip.Border.left : Hip.Border.right) * Hip.Width.Abs() / Hip.SizeX;
		int hipBorderR = (Movement.FacingRight ? Hip.Border.right : Hip.Border.left) * Hip.Width.Abs() / Hip.SizeX;

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
		ShoulderL.Y = Body.Y + Body.Height - bodyBorderU;
		ShoulderL.Width = ShoulderL.SizeX;
		ShoulderL.Height = ShoulderL.SizeY;
		ShoulderL.PivotX = 1000;
		ShoulderL.PivotY = 1000;

		ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
		ShoulderR.Y = Body.Y + Body.Height - bodyBorderU;
		ShoulderR.Width = -ShoulderR.SizeX;
		ShoulderR.Height = ShoulderR.SizeY;
		ShoulderR.PivotX = 1000;
		ShoulderR.PivotY = 1000;

		// Arm
		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmL.Z = (Movement.FacingFront ? facingSign * POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
		UpperArmL.Width = UpperArmL.SizeX;
		UpperArmL.Height = UpperArmL.FlexableSizeY;
		UpperArmL.PivotX = 1000;
		UpperArmL.PivotY = 1000;

		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmR.Z = (Movement.FacingFront ? facingSign * -POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
		UpperArmR.Width = UpperArmR.SizeX;
		UpperArmR.Height = UpperArmR.FlexableSizeY;
		UpperArmR.PivotX = 0;
		UpperArmR.PivotY = 1000;

		ShoulderL.Z = UpperArmL.Z - 1;
		ShoulderR.Z = UpperArmR.Z - 1;

		LowerArmL.X = UpperArmL.X;
		LowerArmL.Y = UpperArmL.Y - UpperArmL.Height;
		LowerArmL.Z = (Movement.FacingFront ? facingSign * POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
		LowerArmL.Width = LowerArmL.SizeX;
		LowerArmL.Height = LowerArmL.FlexableSizeY;
		LowerArmL.PivotX = 1000;
		LowerArmL.PivotY = 1000;

		LowerArmR.X = UpperArmR.X;
		LowerArmR.Y = UpperArmR.Y - UpperArmR.Height;
		LowerArmR.Z = (Movement.FacingFront ? facingSign * -POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
		LowerArmR.Width = LowerArmR.SizeX;
		LowerArmR.Height = LowerArmR.FlexableSizeY;
		LowerArmR.PivotX = 0;
		LowerArmR.PivotY = 1000;

		HandL.X = LowerArmL.X;
		HandL.Y = LowerArmL.Y - LowerArmL.Height;
		HandL.Z = (Movement.FacingFront && Movement.FacingRight ? POSE_Z_HAND : -POSE_Z_HAND_CASUAL);
		HandL.Width = HandL.SizeX;
		HandL.Height = HandL.SizeY;
		HandL.PivotX = 1000;
		HandL.PivotY = 1000;

		HandR.X = LowerArmR.X;
		HandR.Y = LowerArmR.Y - LowerArmR.Height;
		HandR.Z = (Movement.FacingFront && !Movement.FacingRight ? POSE_Z_HAND : -POSE_Z_HAND_CASUAL);
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
		if (Movement.FacingRight) LowerLegL.X -= A2G;

		LowerLegR.X = UpperLegR.X;
		LowerLegR.Y = UpperLegR.Y - UpperLegR.Height;
		LowerLegR.Z = POSE_Z_LOWERLEG;
		LowerLegR.Width = LowerLegR.SizeX;
		LowerLegR.Height = LowerLegR.FlexableSizeY;
		LowerLegR.PivotX = 1000;
		LowerLegR.PivotY = 1000;
		if (!Movement.FacingRight) LowerLegR.X += A2G;

		FootL.X = Movement.FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
		FootL.Y = LowerLegL.Y - LowerLegL.Height;
		FootL.Z = POSE_Z_FOOT;
		FootL.Width = facingSign * FootL.SizeX;
		FootL.Height = FootL.FlexableSizeY;
		FootL.PivotX = 0;
		FootL.PivotY = 1000;

		FootR.X = Movement.FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
		FootR.Y = LowerLegR.Y - LowerLegR.Height;
		FootR.Z = POSE_Z_FOOT;
		FootR.Width = facingSign * FootR.SizeX;
		FootR.Height = FootR.FlexableSizeY;
		FootR.PivotX = 0;
		FootR.PivotY = 1000;

	}


	private void AnimateForPose () {

		// Movement
		var Movement = TargetCharacter.Movement;
		var EquippingToolType = TargetCharacter.EquippingToolType;
		var EquippingToolHeld = TargetCharacter.EquippingToolHeld;

		HandGrabScaleL = Movement.FacingRight ? 1000 : -1000;
		HandGrabScaleR = Movement.FacingRight ? 1000 : -1000;

		PerformPoseAnimation();
		CalculateBodypartGlobalPosition();

		// Handheld
		switch (TargetCharacter.AnimationType) {
			case var _ when EquippingToolType == ToolType.Block:
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
			case CharacterAnimationType.Rush when EquippingToolHeld == ToolHandheld.Pole:
			case CharacterAnimationType.Dash when EquippingToolHeld == ToolHandheld.Pole:
			case CharacterAnimationType.Rolling when EquippingToolHeld == ToolHandheld.Bow || EquippingToolHeld == ToolHandheld.Shooting:
				// Handheld
				PoseAnimation.PerformAnimationFromPool(PoseHandheldIDs[(int)EquippingToolHeld], this);
				CalculateBodypartGlobalPosition();
				break;
			default:
				// Redirect Handheld for Attack Animation
				if (
					EquippingToolHeld == ToolHandheld.DoubleHanded ||
					EquippingToolHeld == ToolHandheld.Bow ||
					EquippingToolHeld == ToolHandheld.Shooting ||
					EquippingToolHeld == ToolHandheld.Pole
				) {
					TargetCharacter.EquippingToolHeld = ToolHandheld.SingleHanded;
				}
				break;
		}

		// Attacking
		if (TargetCharacter.Attackness.IsAttacking) {
			if (TargetCharacter.CurrentAttackSpeedRate == 0 && TargetCharacter.IsGrounded && !Movement.IsSquatting) ResetPoseToDefault(true);
			HandGrabScaleL = HandGrabScaleR = Movement.FacingRight ? 1000 : -1000;
			HandGrabAttackTwistL = HandGrabAttackTwistR = 1000;
			PoseAnimation.PerformAnimationFromPool(PoseAttackIDs[(int)EquippingToolType], this);
			CalculateBodypartGlobalPosition();
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
		Head.GlobalX = TargetCharacter.X + PoseRootX + Head.X;
	}


	private void PoseUpdate_HeadRotate () {
		if (Head.Rotation == 0) return;
		Head.Rotation = Head.Rotation.Clamp(-90, 90);
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
					TargetCharacter.X + PoseRootX + bodyPart.X,
					TargetCharacter.Y + PoseRootY + bodyPart.Y,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
					bodyPart.Tint, bodyPart.Z
				);
			} else {
				Renderer.Draw(
					bodyPart.ID,
					TargetCharacter.X + PoseRootX + bodyPart.X,
					TargetCharacter.Y + PoseRootY + bodyPart.Y,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
					bodyPart.Tint, bodyPart.Z
				);
			}
		}

		// Z Offset
		RenderedCellZ = PlayerSystem.Selecting == TargetCharacter ? 40 : GlobalPoseRenderingZOffset;
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = cellIndexStart; i < count; i++) {
				cells[i].Z += RenderedCellZ;
			}
		}
		GlobalPoseRenderingZOffset -= 40;

	}


	#endregion




	#region --- API ---


	// Config
	public static CharacterRenderingConfig CreateCharacterRenderingConfigFromSheet (System.Type characterType) {

		int typeID = characterType.AngeHash();
		int bodyPartLen = DEFAULT_BODY_PART_ID.Length;
		var config = new CharacterRenderingConfig();

		// Body Parts
		for (int i = 0; i < bodyPartLen; i++) {
			if (!BodyPart.TryGetSpriteIdFromSheet(characterType, BODY_PART_NAME[i], i == 0, out int id)) {
				id = DEFAULT_BODY_PART_ID[i];
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


	public static void ReloadRenderingConfigPoolFromFileAndSheet () {
		RenderingConfigGlobalVersion++;
		ConfigPool_Rendering.Clear();
		string renderRoot = Universe.BuiltIn.SlotCharacterRenderingConfigRoot;
		foreach (var type in typeof(Character).AllChildClass()) {
			int typeID = type.AngeHash();
			// Load From File
			string path = Util.CombinePaths(renderRoot, $"{type.Name}.json");
			var config = JsonUtil.LoadJsonFromPath<CharacterRenderingConfig>(path);
			// Create Default Config
			if (config == null) {
				config = CreateCharacterRenderingConfigFromSheet(type);
				JsonUtil.SaveJsonToPath(config, path, prettyPrint: true);
			}
			// Add to Pool
			ConfigPool_Rendering.Add(typeID, config);
		}
	}


	public void SaveCharacterToConfig () {

		if (!ConfigPool_Rendering.TryGetValue(TargetCharacter.TypeID, out var config)) return;

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


	public void SyncRenderingConfigFromPool () {
		if (LocalRenderingConfigVersion == RenderingConfigGlobalVersion) return;
		LocalRenderingConfigVersion = RenderingConfigGlobalVersion;
		if (ConfigPool_Rendering.TryGetValue(TargetCharacter.TypeID, out var rConfig)) {
			rConfig.LoadToCharacter(this);
		} else {
			for (int i = 0; i < DEFAULT_BODY_PART_ID.Length; i++) {
				BodyParts[i].SetData(DEFAULT_BODY_PART_ID[i]);
			}
		}
	}


	// Animation
	public void ResetAllLimbsPosition () {

		var Movement = TargetCharacter.Movement;

		int targetUnitHeight = CharacterHeight * A2G / CM_PER_PX - Head.SizeY;
		int defaultCharHeight = Body.SizeY + Hip.SizeY + UpperLegL.SizeY + LowerLegL.SizeY + FootL.SizeY;
		int bodyBorderU = Body.Border.up * targetUnitHeight / defaultCharHeight;
		int bodyBorderL = (Movement.FacingRight ? Body.Border.left : Body.Border.right) * Body.Width.Abs() / Body.SizeX;
		int bodyBorderR = (Movement.FacingRight ? Body.Border.right : Body.Border.left) * Body.Width.Abs() / Body.SizeX;
		int hipBorderL = (Movement.FacingRight ? Hip.Border.left : Hip.Border.right) * Hip.Width.Abs() / Hip.SizeX;
		int hipBorderR = (Movement.FacingRight ? Hip.Border.right : Hip.Border.left) * Hip.Width.Abs() / Hip.SizeX;

		ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
		ShoulderL.Y = Body.Y + Body.Height - bodyBorderU;

		ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
		ShoulderR.Y = Body.Y + Body.Height - bodyBorderU;

		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;

		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;

		LowerArmL.X = UpperArmL.X;
		LowerArmL.Y = UpperArmL.Y - UpperArmL.Height;

		LowerArmR.X = UpperArmR.X;
		LowerArmR.Y = UpperArmR.Y - UpperArmR.Height;

		HandL.X = LowerArmL.X;
		HandL.Y = LowerArmL.Y - LowerArmL.Height;

		HandR.X = LowerArmR.X;
		HandR.Y = LowerArmR.Y - LowerArmR.Height;

		UpperLegL.X = Hip.X - Hip.Width.Abs() / 2 + hipBorderL;
		UpperLegL.Y = Hip.Y;

		UpperLegR.X = Hip.X + Hip.Width.Abs() / 2 - hipBorderR;
		UpperLegR.Y = Hip.Y;

		LowerLegL.X = UpperLegL.X;
		LowerLegL.Y = UpperLegL.Y - UpperLegL.Height;

		LowerLegR.X = UpperLegR.X;
		LowerLegR.Y = UpperLegR.Y - UpperLegR.Height;

		FootL.X = Movement.FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
		FootL.Y = LowerLegL.Y - LowerLegL.Height;

		FootR.X = Movement.FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
		FootR.Y = LowerLegR.Y - LowerLegR.Height;

	}


	// Animation ID
	public FrameBasedInt GetPoseAnimationID (CharacterAnimationType type) => PoseAnimationIDs[(int)type];


	public void OverridePoseAnimation (CharacterAnimationType type, int id, int duration = 1) => PoseAnimationIDs[(int)type].Override(id, duration);


	public void OverridePoseHandheldAnimation (ToolHandheld handheld, int id, int duration = 1) => PoseHandheldIDs[(int)handheld].Override(id, duration);


	public void OverridePoseAttackAnimation (ToolType type, int id, int duration = 1) => PoseAttackIDs[(int)type].Override(id, duration);


	public void ManualPoseAnimate (int id, int duration = 1) => ManualPoseAnimationID.Override(id, duration);


	#endregion




	#region --- LGC ---


	private void CalculateBodypartGlobalPosition () {
		foreach (var part in BodyParts) {
			part.GlobalX = TargetCharacter.X + PoseRootX + part.X;
			part.GlobalY = TargetCharacter.Y + PoseRootY + part.Y;
		}
	}


	#endregion




}
