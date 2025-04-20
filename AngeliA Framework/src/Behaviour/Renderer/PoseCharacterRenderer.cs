using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseCharacterRenderer : CharacterRenderer {




	#region --- VAR ---


	// Const
	/// <summary>
	/// How many cm does one artwork pixel represents
	/// </summary>
	public const int CM_PER_PX = 5;
	private const int A2G = Const.CEL / Const.ART_CEL;
	private static readonly int[] DEFAULT_POSE_ANIMATION_IDS = [typeof(PoseAnimation_Idle).AngeHash(), typeof(PoseAnimation_Walk).AngeHash(), typeof(PoseAnimation_Run).AngeHash(), typeof(PoseAnimation_JumpUp).AngeHash(), typeof(PoseAnimation_JumpDown).AngeHash(), typeof(PoseAnimation_SwimIdle).AngeHash(), typeof(PoseAnimation_SwimMove).AngeHash(), typeof(PoseAnimation_SquatIdle).AngeHash(), typeof(PoseAnimation_SquatMove).AngeHash(), typeof(PoseAnimation_Dash).AngeHash(), typeof(PoseAnimation_Rush).AngeHash(), typeof(PoseAnimation_Crash).AngeHash(), typeof(PoseAnimation_Pound).AngeHash(), typeof(PoseAnimation_Brake).AngeHash(), typeof(PoseAnimation_Climb).AngeHash(), typeof(PoseAnimation_Fly).AngeHash(), typeof(PoseAnimation_Slide).AngeHash(), typeof(PoseAnimation_GrabTop).AngeHash(), typeof(PoseAnimation_GrabSide).AngeHash(), typeof(PoseAnimation_Spin).AngeHash(), typeof(PoseAnimation_Animation_TakingDamage).AngeHash(), typeof(PoseAnimation_Sleep).AngeHash(), typeof(PoseAnimation_PassOut).AngeHash(), typeof(PoseAnimation_Rolling).AngeHash(),];
	private static readonly int ANI_TYPE_COUNT = typeof(CharacterAnimationType).EnumLength();
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
	/// <summary>
	/// Mid-Bottom local position of character hip y position
	/// </summary>
	public int BasicRootY { get; set; } = 0;
	/// <summary>
	/// Total offset X for pose rendering
	/// </summary>
	public int PoseRootX { get; set; } = 0;
	/// <summary>
	/// Total offset Y for pose rendering
	/// </summary>
	public int PoseRootY { get; set; } = 0;
	/// <summary>
	/// Make head rotate like shaking head 
	/// </summary>
	public int HeadTwist { get; set; } = 0;
	/// <summary>
	/// Make body rotate left or right
	/// </summary>
	public int BodyTwist { get; set; } = 0;
	/// <summary>
	/// Character body height in cm
	/// </summary>
	public int CharacterHeight { get; set; } = 160; // in CM
	/// <summary>
	/// Basic rendering Z value for last time character get rendered
	/// </summary>
	public int RenderedCellZ { get; private set; } = 0;
	/// <summary>
	/// How many frames does it takes the character to transition from one pose to another
	/// </summary>
	public int BlendDuration { get; set; } = 6;

	// BodyPart
	/// <summary>
	/// All body parts of the pose character
	/// </summary>
	public readonly BodyPart[] BodyParts = new BodyPart[BodyPart.BODY_PART_COUNT];
	/// <summary>
	/// Head of the pose character
	/// </summary>
	public BodyPart Head { get; init; } = null;
	/// <summary>
	/// Body of the pose character
	/// </summary>
	public BodyPart Body { get; init; } = null;
	/// <summary>
	/// Hip of the pose character
	/// </summary>
	public BodyPart Hip { get; init; } = null;
	/// <summary>
	/// Left shoulder of the pose character
	/// </summary>
	public BodyPart ShoulderL { get; init; } = null;
	/// <summary>
	/// Right shoulder of the pose character
	/// </summary>
	public BodyPart ShoulderR { get; init; } = null;
	/// <summary>
	/// Left upper arm of the pose character
	/// </summary>
	public BodyPart UpperArmL { get; init; } = null;
	/// <summary>
	/// Right upper arm of the pose character
	/// </summary>
	public BodyPart UpperArmR { get; init; } = null;
	/// <summary>
	/// Left lower arm of the pose character
	/// </summary>
	public BodyPart LowerArmL { get; init; } = null;
	/// <summary>
	/// Right lower arm of the pose character
	/// </summary>
	public BodyPart LowerArmR { get; init; } = null;
	/// <summary>
	/// Left hand of the pose character
	/// </summary>
	public BodyPart HandL { get; init; } = null;
	/// <summary>
	/// Right hand of the pose character
	/// </summary>
	public BodyPart HandR { get; init; } = null;
	/// <summary>
	/// Left upper leg of the pose character
	/// </summary>
	public BodyPart UpperLegL { get; init; } = null;
	/// <summary>
	/// Right upper leg of the pose character
	/// </summary>
	public BodyPart UpperLegR { get; init; } = null;
	/// <summary>
	/// Left lower leg of the pose character
	/// </summary>
	public BodyPart LowerLegL { get; init; } = null;
	/// <summary>
	/// Right lower leg of the pose character
	/// </summary>
	public BodyPart LowerLegR { get; init; } = null;
	/// <summary>
	/// Left foot of the pose character
	/// </summary>
	public BodyPart FootL { get; init; } = null;
	/// <summary>
	/// Right foot of the pose character
	/// </summary>
	public BodyPart FootR { get; init; } = null;

	// Hand
	/// <summary>
	/// Rotation of the object grabbing by the left hand
	/// </summary>
	public readonly FrameBasedInt HandGrabRotationL = new(0);
	/// <summary>
	/// Rotation of the object grabbing by the right hand
	/// </summary>
	public readonly FrameBasedInt HandGrabRotationR = new(0);
	/// <summary>
	/// Size scaling of the object grabbing by the left hand (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt HandGrabScaleL = new(1000);
	/// <summary>
	/// Size scaling of the object grabbing by the right hand (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt HandGrabScaleR = new(1000);
	/// <summary>
	/// Angle twist of the object grabbing by the left hand (0 means disappear, 1000 means normal)
	/// </summary>
	public readonly FrameBasedInt HandGrabAttackTwistL = new(1000);
	/// <summary>
	/// Angle twist of the object grabbing by the right hand (0 means disappear, 1000 means normal)
	/// </summary>
	public readonly FrameBasedInt HandGrabAttackTwistR = new(1000);

	// Gadget
	/// <summary>
	/// ID of current face gadget instance
	/// </summary>
	public readonly FrameBasedInt FaceID = new(0);
	/// <summary>
	/// ID of current hair gadget instance
	/// </summary>
	public readonly FrameBasedInt HairID = new(0);
	/// <summary>
	/// ID of current animal-ear gadget instance (like cat-girl's ears, not human ears)
	/// </summary>
	public readonly FrameBasedInt EarID = new(0);
	/// <summary>
	/// ID of current tail gadget instance
	/// </summary>
	public readonly FrameBasedInt TailID = new(0);
	/// <summary>
	/// ID of current wing gadget instance
	/// </summary>
	public readonly FrameBasedInt WingID = new(0);
	/// <summary>
	/// ID of current horn gadget instance
	/// </summary>
	public readonly FrameBasedInt HornID = new(0);
	/// <summary>
	/// Which face expression does the face need to render
	/// </summary>
	public readonly FrameBasedInt ForceFaceExpressionIndex = new(-1);

	// Suit
	/// <summary>
	/// Current hat suit id
	/// </summary>
	public readonly FrameBasedInt SuitHead = new(0);
	/// <summary>
	/// Current body suit id (cloth with sleeves)
	/// </summary>
	public readonly FrameBasedInt SuitBody = new(0);
	/// <summary>
	/// Current hip suit id (pants or skirt)
	/// </summary>
	public readonly FrameBasedInt SuitHip = new(0);
	/// <summary>
	/// Current gloves suit id
	/// </summary>
	public readonly FrameBasedInt SuitHand = new(0);
	/// <summary>
	/// Current shoes suit id
	/// </summary>
	public readonly FrameBasedInt SuitFoot = new(0);

	// Data
	private static readonly Dictionary<int, CharacterRenderingConfig> ConfigPoolRendering = [];
	private static readonly int EquipmentTypeCount = System.Enum.GetValues(typeof(EquipmentType)).Length;
	private static int GlobalPoseRenderingZOffset;
	private static int RenderingConfigGlobalVersion = -1;
	private int LocalRenderingConfigVersion = int.MinValue;
	private int PrevAniID = 0;
	private int BlendAniID = 0;
	private int AnimationBlendFrame = 0;
	private readonly FrameBasedInt[] PoseAnimationIDs;
	private readonly FrameBasedInt ManualPoseAnimationID = new(0);


	#endregion




	#region --- MSG ---


	// Init
	[OnGameInitialize(-128)]
	internal static TaskResult InitializePose () {

		if (!Renderer.IsReady) return TaskResult.Continue;

		bool forceResetRendering = false;

#if DEBUG
		if (DEFAULT_POSE_ANIMATION_IDS.Length != ANI_TYPE_COUNT) {
			Debug.LogWarning($"FAILBACK_POSE_ANIMATION_IDS length have to be {ANI_TYPE_COUNT}");
		}
		forceResetRendering = true;
#endif

		ReloadRenderingConfigPoolFromFileAndSheet(forceResetRendering);
		return TaskResult.End;
	}


#if DEBUG
	[OnGameFocused]
	internal static void OnGameFocused () {
		if (Game.GlobalFrame != 0) {
			ReloadRenderingConfigPoolFromFileAndSheet(true);
		}
	}
#endif


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () => ReloadRenderingConfigPoolFromFileAndSheet();


	public PoseCharacterRenderer (Character target) : base(target) {
		// Body Part
		for (int i = 0; i < BodyPart.BODY_PART_COUNT; i++) {
			var bodyPart = BodyParts[i] = new BodyPart(
				parent: (i >= 7 && i < 11) || (i >= 13 && i < 17) ? BodyParts[i - 2] : null,
				useLimbFlip: i == 9 || i == 10 || i == 15 || i == 16,
				rotateWithBody: i != 2 && i != 1 && i < 11,
				defaultPivotX: BodyPart.BODY_DEF_PIVOT[i].x,
				defaultPivotY: BodyPart.BODY_DEF_PIVOT[i].y
			);
			bodyPart.SetData(BodyPart.DEFAULT_BODY_PART_ID[i]);
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
		// Load Default Ani
		for (int i = 0; i < ANI_TYPE_COUNT; i++) {
			PoseAnimationIDs[i].BaseValue = DEFAULT_POSE_ANIMATION_IDS[i];
		}
		SyncRenderingConfigFromPool();
	}


	// Global Update
	[BeforeBeforeUpdate]
	internal static void BeforeBeforeUpdate () => GlobalPoseRenderingZOffset = 0;


	// Renderer Msg
	public override void OnActivated () {
		base.OnActivated();
		PrevAniID = 0;
		BlendAniID = 0;
		AnimationBlendFrame = 0;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		SyncRenderingConfigFromPool();
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (BodyParts == null) return;
		int cellIndexStart = Renderer.GetUsedCellCount();
		ResetPoseToDefault(false);
		AnimateForPose();
		PoseUpdateRotationAndTwist();
		RenderEquipment();
		RenderInventory();
		RenderBodyGadgets();
		RenderCloths();
		TargetCharacter?.OnCharacterRendered();
		DrawBodyPart(cellIndexStart);
	}


	protected virtual void RenderBodyGadgets () {
		using (new RotateCellScope(Body.Rotation, Body.GlobalX, Body.GlobalY)) {
			Wing.DrawGadgetFromPool(this);
			Face.DrawGadgetFromPool(this);
			Hair.DrawGadgetFromPool(this);
			Ear.DrawGadgetFromPool(this);
			Horn.DrawGadgetFromPool(this);
		}
		Tail.DrawGadgetFromPool(this);
	}


	protected virtual void RenderCloths () {
		HeadCloth.DrawClothFromPool(this);
		BodyCloth.DrawClothFromPool(this);
		HandCloth.DrawClothFromPool(this);
		HipCloth.DrawClothFromPool(this);
		FootCloth.DrawClothFromPool(this);
	}


	protected virtual void PerformPoseAnimation () {
		int currentAniID = ManualPoseAnimationID != 0 ? ManualPoseAnimationID : PoseAnimationIDs[(int)TargetCharacter.AnimationType];
		if (BlendDuration > 0) {
			// ID Changed
			if (currentAniID != PrevAniID) {
				if (currentAniID == BlendAniID && BlendAniID != 0 && AnimationBlendFrame < BlendDuration) {
					AnimationBlendFrame = BlendDuration - AnimationBlendFrame;
				} else {
					AnimationBlendFrame = 0;
				}
				BlendAniID = PrevAniID;
				PrevAniID = currentAniID;
			}
			if (BlendAniID != 0 && AnimationBlendFrame < BlendDuration) {
				// Blend
				float blend01 = ((float)AnimationBlendFrame / BlendDuration).Clamp01();
				PoseAnimation.PerformAnimationBlendFromPool(BlendAniID, currentAniID, blend01, this);
				AnimationBlendFrame++;
			} else {
				// Just Perform
				PoseAnimation.PerformAnimationFromPool(currentAniID, this);
			}
		} else {
			// Just Perform
			PoseAnimation.PerformAnimationFromPool(currentAniID, this);
		}
	}


	protected virtual void RenderEquipment () {

		for (int i = 0; i < EquipmentTypeCount; i++) {
			var eqType = (EquipmentType)i;
			using var _ = new RotateCellScope(
				eqType == EquipmentType.HandTool ? Body.Rotation : 0, Body.GlobalX, Body.GlobalY,
				keepOriginalRotation: true
			);
			int id = Inventory.GetEquipment(TargetCharacter.InventoryID, eqType, out int equipmentCount);
			var eq = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			if (eq == null || !eq.ItemConditionCheck(TargetCharacter)) continue;
			eq.BeforePoseAnimationUpdate_FromEquipment(this);
		}
		CalculateBodypartGlobalPosition();

		for (int i = 0; i < EquipmentTypeCount; i++) {
			var eqType = (EquipmentType)i;
			using var _ = new RotateCellScope(
				eqType == EquipmentType.HandTool ? Body.Rotation : 0, 
				Body.GlobalX, Body.GlobalY,
				keepOriginalRotation: false
			);
			int id = Inventory.GetEquipment(TargetCharacter.InventoryID, eqType, out int equipmentCount);
			var eq = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			if (eq == null || !eq.ItemConditionCheck(TargetCharacter)) continue;
			eq.OnPoseAnimationUpdate_FromEquipment(this);
			// Draw Tool
			if (
				eq is HandTool tool &&
				PoseAnimation.TryGetAnimationFromPool(tool.GetHandheldPoseAnimationID(TargetCharacter), out var poseAni) &&
				poseAni is HandheldPoseAnimation handheldAni &&
				TargetCharacter.AnimationType != CharacterAnimationType.Sleep &&
				TargetCharacter.AnimationType != CharacterAnimationType.PassOut &&
				TargetCharacter.AnimationType != CharacterAnimationType.Crash
			) {
				handheldAni.DrawTool(tool, this);
			}
		}
		CalculateBodypartGlobalPosition();

	}


	protected virtual void RenderInventory () {
		int invCapacity = Inventory.GetInventoryCapacity(TargetCharacter.InventoryID);
		TargetCharacter.ResetInventoryUpdate(invCapacity);
		for (int i = 0; i < invCapacity; i++) {
			int id = Inventory.GetItemAt(TargetCharacter.InventoryID, i);
			var item = id != 0 ? ItemSystem.GetItem(id) : null;
			if (item == null || !item.ItemConditionCheck(TargetCharacter) || !item.CheckUpdateAvailable(TargetCharacter.TypeID)) continue;
			item.OnPoseAnimationUpdate_FromInventory(this, TargetCharacter.InventoryID, i);
		}
		CalculateBodypartGlobalPosition();
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
			bodypart.SetPivotToDefault();
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

		// Body
		Body.X = 0;
		Body.Y = Hip.Height;
		Body.Z = POSE_Z_BODY;
		Body.Width = facingSign * Body.SizeX;

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

		ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
		ShoulderR.Y = Body.Y + Body.Height - bodyBorderU;
		ShoulderR.Width = -ShoulderR.SizeX;
		ShoulderR.Height = ShoulderR.SizeY;

		// Arm
		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmL.Z = (Movement.FacingFront ? facingSign * POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
		UpperArmL.Width = UpperArmL.SizeX;
		UpperArmL.Height = UpperArmL.FlexableSizeY;

		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmR.Z = (Movement.FacingFront ? facingSign * -POSE_Z_UPPERARM : -POSE_Z_UPPERARM);
		UpperArmR.Width = UpperArmR.SizeX;
		UpperArmR.Height = UpperArmR.FlexableSizeY;

		ShoulderL.Z = UpperArmL.Z - 1;
		ShoulderR.Z = UpperArmR.Z - 1;

		LowerArmL.X = UpperArmL.X;
		LowerArmL.Y = UpperArmL.Y - UpperArmL.Height;
		LowerArmL.Z = (Movement.FacingFront ? facingSign * POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
		LowerArmL.Width = LowerArmL.SizeX;
		LowerArmL.Height = LowerArmL.FlexableSizeY;

		LowerArmR.X = UpperArmR.X;
		LowerArmR.Y = UpperArmR.Y - UpperArmR.Height;
		LowerArmR.Z = (Movement.FacingFront ? facingSign * -POSE_Z_LOWERARM : -POSE_Z_LOWERARM);
		LowerArmR.Width = LowerArmR.SizeX;
		LowerArmR.Height = LowerArmR.FlexableSizeY;

		HandL.X = LowerArmL.X;
		HandL.Y = LowerArmL.Y - LowerArmL.Height;
		HandL.Z = (Movement.FacingFront && Movement.FacingRight ? POSE_Z_HAND : -POSE_Z_HAND_CASUAL);
		HandL.Width = HandL.SizeX;
		HandL.Height = HandL.SizeY;

		HandR.X = LowerArmR.X;
		HandR.Y = LowerArmR.Y - LowerArmR.Height;
		HandR.Z = (Movement.FacingFront && !Movement.FacingRight ? POSE_Z_HAND : -POSE_Z_HAND_CASUAL);
		HandR.Width = -HandR.SizeX;
		HandR.Height = HandR.SizeY;

		// Leg
		UpperLegL.X = Hip.X - Hip.Width.Abs() / 2 + hipBorderL;
		UpperLegL.Y = Hip.Y;
		UpperLegL.Z = POSE_Z_UPPERLEG;
		UpperLegL.Width = UpperLegL.SizeX;
		UpperLegL.Height = UpperLegL.FlexableSizeY;

		UpperLegR.X = Hip.X + Hip.Width.Abs() / 2 - hipBorderR;
		UpperLegR.Y = Hip.Y;
		UpperLegR.Z = POSE_Z_UPPERLEG;
		UpperLegR.Width = UpperLegR.SizeX;
		UpperLegR.Height = UpperLegR.FlexableSizeY;

		LowerLegL.X = UpperLegL.X;
		LowerLegL.Y = UpperLegL.Y - UpperLegL.Height;
		LowerLegL.Z = POSE_Z_LOWERLEG;
		LowerLegL.Width = LowerLegL.SizeX;
		LowerLegL.Height = LowerLegL.FlexableSizeY;
		if (Movement.FacingRight) LowerLegL.X -= A2G;

		LowerLegR.X = UpperLegR.X;
		LowerLegR.Y = UpperLegR.Y - UpperLegR.Height;
		LowerLegR.Z = POSE_Z_LOWERLEG;
		LowerLegR.Width = LowerLegR.SizeX;
		LowerLegR.Height = LowerLegR.FlexableSizeY;
		if (!Movement.FacingRight) LowerLegR.X += A2G;

		FootL.X = Movement.FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
		FootL.Y = LowerLegL.Y - LowerLegL.Height;
		FootL.Z = POSE_Z_FOOT;
		FootL.Width = facingSign * FootL.SizeX;
		FootL.Height = FootL.FlexableSizeY;

		FootR.X = Movement.FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
		FootR.Y = LowerLegR.Y - LowerLegR.Height;
		FootR.Z = POSE_Z_FOOT;
		FootR.Width = facingSign * FootR.SizeX;
		FootR.Height = FootR.FlexableSizeY;

	}


	private void AnimateForPose () {

		// Movement
		var Movement = TargetCharacter.Movement;

		HandGrabScaleL.BaseValue = Movement.FacingRight ? 1000 : -1000;
		HandGrabScaleR.BaseValue = Movement.FacingRight ? 1000 : -1000;
		HandGrabRotationL.BaseValue = 0;
		HandGrabRotationR.BaseValue = 0;

		PerformPoseAnimation();
		CalculateBodypartGlobalPosition();

		// Get Animation ID from Handtool
		int handheldAniID = 0;
		int performAniID = 0;
		int equippingID = Inventory.GetEquipment(TargetCharacter.InventoryID, EquipmentType.HandTool, out _);
		if (equippingID != 0 && ItemSystem.GetItem(equippingID) is HandTool eqTool) {
			handheldAniID = eqTool.GetHandheldPoseAnimationID(TargetCharacter);
			performAniID = eqTool.GetPerformPoseAnimationID(TargetCharacter);
		}

		// Handheld
		if (handheldAniID != 0) {
			PoseAnimation.PerformAnimationFromPool(handheldAniID, this);
			CalculateBodypartGlobalPosition();
		}

		// Attacking
		if (TargetCharacter.Attackness.IsAttacking && performAniID != 0) {
			if (TargetCharacter.CurrentAttackSpeedRate == 0 && TargetCharacter.IsGrounded && !Movement.IsSquatting) ResetPoseToDefault(true);
			HandGrabScaleL.Override(Movement.FacingRight ? 1000 : -1000);
			HandGrabScaleR.Override(Movement.FacingRight ? 1000 : -1000);
			HandGrabAttackTwistL.Override(1000);
			HandGrabAttackTwistR.Override(1000);
			PoseAnimation.PerformAnimationFromPool(performAniID, this);
			CalculateBodypartGlobalPosition();
		}

	}


	private void PoseUpdateRotationAndTwist () {

		Head.Rotation = Head.Rotation.Clamp(-90, 90);
		HeadTwist = !Head.FrontSide ? 0 : HeadTwist.Clamp(-1000, 1000);

		// Head Rot Offset
		if (Head.Rotation != 0) {
			Head.Y -= Head.Rotation.Abs() / 2;
		}

		// Body Rot Offset
		Body.Rotation = Body.Rotation.Clamp(-90, 90);
		if (Body.Rotation != 0) {
			int rawX = (-Body.Rotation.Sign() * Ease.OutSine(Body.Rotation.Abs() / 90f) * Body.Width.Abs() / 2f).RoundToInt();
			int rawY = (-Ease.OutQuint(Body.Rotation.Abs() / 90f) * Body.Width.Abs() / 4f).RoundToInt();
			Body.X += rawX;
			Body.Y += rawY;
			Body.GlobalX += rawX;
			Body.GlobalY += rawY;
			for (int i = 0; i < BodyPart.BODY_PART_COUNT; i++) {
				var bodypart = BodyParts[i];
				if (!bodypart.RotateWithBody) continue;
				bodypart.X += rawX;
				bodypart.GlobalX += rawX;
				bodypart.Y += rawY;
				bodypart.GlobalY += rawY;
			}
		}

		// Twist
		if (HeadTwist != 0) {
			Head.Width = Head.Width.Sign() * (Head.Width.Abs() - (Head.Width * HeadTwist).Abs() / 2000);
			Head.X += Head.Width.Abs() * HeadTwist / 2000;
			Head.GlobalX = TargetCharacter.X + PoseRootX + Head.X;
		}

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

			using var _ = new RotateCellScope(
				bodyPart.RotateWithBody ? Body.Rotation : 0, 
				Body.GlobalX, 
				Body.GlobalY
			);

			if (bodyPart == Head && Renderer.TryGetSpriteFromGroup(bodyPart.ID, Head.FrontSide ? 0 : 1, out var headSprite, false, true)) {
				Renderer.Draw(
					headSprite,
					bodyPart.GlobalX,
					bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
					bodyPart.Tint, bodyPart.Z
				);
			} else {
				Renderer.Draw(
					bodyPart.ID,
					bodyPart.GlobalX,
					bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation, bodyPart.Width, bodyPart.Height,
					bodyPart.Tint, bodyPart.Z
				);
			}
		}

		// Z Offset / Tint
		RenderedCellZ = PlayerSystem.Selecting == TargetCharacter ? 40 : GlobalPoseRenderingZOffset;
		if (Renderer.GetCells(out var cells, out int count)) {
			bool tinted = Tint != Color32.WHITE;
			bool scaled = Scale != 1000;
			for (int i = cellIndexStart; i < count; i++) {
				var cell = cells[i];
				cell.Z += RenderedCellZ;
				if (tinted) {
					cell.Color *= Tint;
				}
				if (scaled) {
					cell.ScaleFrom(Scale, TargetCharacter.X, TargetCharacter.Y);
				}
			}
		}
		GlobalPoseRenderingZOffset -= 40;

	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Get current body gadget ID the character is using
	/// </summary>
	public int GetGadgetID (BodyGadgetType type) => type switch {
		BodyGadgetType.Face => FaceID,
		BodyGadgetType.Hair => HairID,
		BodyGadgetType.Ear => EarID,
		BodyGadgetType.Horn => HornID,
		BodyGadgetType.Tail => TailID,
		BodyGadgetType.Wing => WingID,
		_ => 0,
	};


	/// <summary>
	/// Get current suit ID the character is using
	/// </summary>
	public int GetSuitID (ClothType type) => type switch {
		ClothType.Head => SuitHead,
		ClothType.Body => SuitBody,
		ClothType.Hand => SuitHand,
		ClothType.Hip => SuitHip,
		ClothType.Foot => SuitFoot,
		_ => 0,
	};


	// Config
	public static void ReloadRenderingConfigPoolFromFileAndSheet (bool forceReset = false) {
		RenderingConfigGlobalVersion++;
		ConfigPoolRendering.Clear();
		string renderRoot = Universe.BuiltIn.SlotCharacterRenderingConfigRoot;
		foreach (var type in typeof(Character).AllChildClass()) {
			int typeID = type.AngeHash();
			string path = Util.CombinePaths(renderRoot, $"{type.Name}.json");
			// Load From File
			var config = !forceReset ? JsonUtil.LoadJsonFromPath<CharacterRenderingConfig>(path) : null;
			// Create Default Config
			if (config == null) {
				config = new CharacterRenderingConfig();
				config.LoadFromSheet(type);
				JsonUtil.SaveJsonToPath(config, path, prettyPrint: true);
			}
			// Add to Pool
			ConfigPoolRendering.Add(typeID, config);
		}
		ConfigPoolRendering.TrimExcess();
	}


	public static bool TryGetConfigFromPool (int id, out CharacterRenderingConfig config) => ConfigPoolRendering.TryGetValue(id, out config);


	public void SaveCharacterToConfig (bool saveToFile = false) {

		if (!ConfigPoolRendering.TryGetValue(TargetCharacter.TypeID, out var config)) return;

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

		// File
		if (saveToFile) {
			string renderRoot = Universe.BuiltIn.SlotCharacterRenderingConfigRoot;
			string path = Util.CombinePaths(renderRoot, $"{TargetCharacter.GetType().AngeName()}.json");
			JsonUtil.SaveJsonToPath(config, path, prettyPrint: true);
		}

	}


	public void SyncRenderingConfigFromPool (bool forceSync = false) {
		if (!forceSync && LocalRenderingConfigVersion == RenderingConfigGlobalVersion) return;
		LocalRenderingConfigVersion = RenderingConfigGlobalVersion;
		if (ConfigPoolRendering.TryGetValue(TargetCharacter.TypeID, out var rConfig)) {
			rConfig.LoadToCharacter(this);
		} else {
			for (int i = 0; i < BodyPart.DEFAULT_BODY_PART_ID.Length; i++) {
				BodyParts[i].SetData(BodyPart.DEFAULT_BODY_PART_ID[i]);
			}
		}
	}


	// Animation ID
	/// <summary>
	/// Override animation for given animation type for specified frames long
	/// </summary>
	public void OverridePoseAnimation (CharacterAnimationType type, int id, int duration = 1) => PoseAnimationIDs[(int)type].Override(id, duration, 4096);

	/// <summary>
	/// Make the renderer draw the character based on the given animation for specified frames long
	/// </summary>
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
