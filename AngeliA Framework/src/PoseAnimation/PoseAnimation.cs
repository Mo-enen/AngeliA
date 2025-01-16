using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[System.Serializable]
public abstract class PoseAnimation {




	#region --- SUB ---


	private class BlendRecord {
		private readonly BodyPart[] BodyParts = new BodyPart[PoseCharacterRenderer.BODY_PART_COUNT];
		public int PoseRootX;
		public int PoseRootY;
		public int BodyTwist;
		public int HeadTwist;
		public int HandGrabRotationL;
		public int HandGrabRotationR;
		public int HandGrabScaleL;
		public int HandGrabScaleR;
		public int HandGrabAttackTwistL;
		public int HandGrabAttackTwistR;
		public BlendRecord () {
			for (int i = 0; i < BodyParts.Length; i++) {
				BodyParts[i] = new BodyPart(null, false);
			}
		}
		public void ApplyToPose () {
			for (int i = 0; i < BodyParts.Length; i++) {
				var record = BodyParts[i];
				var pose = Rendering.BodyParts[i];
				pose.X = record.X;
				pose.Y = record.Y;
				pose.Z = record.Z;
				pose.Rotation = record.Rotation;
				pose.Width = record.Width;
				pose.Height = record.Height;
				pose.PivotX = record.PivotX;
				pose.PivotY = record.PivotY;
				pose.FrontSide = record.FrontSide;
				pose.Covered = record.Covered;
				pose.Tint = record.Tint;
			}
			Rendering.PoseRootX = PoseRootX;
			Rendering.PoseRootY = PoseRootY;
			Rendering.BodyTwist = BodyTwist;
			Rendering.HeadTwist = HeadTwist;
			Rendering.HandGrabRotationL = HandGrabRotationL;
			Rendering.HandGrabRotationR = HandGrabRotationR;
			Rendering.HandGrabScaleL = HandGrabScaleL;
			Rendering.HandGrabScaleR = HandGrabScaleR;
			Rendering.HandGrabAttackTwistL = HandGrabAttackTwistL;
			Rendering.HandGrabAttackTwistR = HandGrabAttackTwistR;
		}
		public void RecordFromPose () {
			for (int i = 0; i < BodyParts.Length; i++) {
				var record = BodyParts[i];
				var pose = Rendering.BodyParts[i];
				record.X = pose.X;
				record.Y = pose.Y;
				record.Z = pose.Z;
				record.Rotation = pose.Rotation;
				record.Width = pose.Width;
				record.Height = pose.Height;
				record.PivotX = pose.PivotX;
				record.PivotY = pose.PivotY;
				record.FrontSide = pose.FrontSide;
				record.Covered = pose.Covered;
				record.Tint = pose.Tint;
			}
			PoseRootX = Rendering.PoseRootX;
			PoseRootY = Rendering.PoseRootY;
			BodyTwist = Rendering.BodyTwist;
			HeadTwist = Rendering.HeadTwist;
			HandGrabRotationL = Rendering.HandGrabRotationL;
			HandGrabRotationR = Rendering.HandGrabRotationR;
			HandGrabScaleL = Rendering.HandGrabScaleL;
			HandGrabScaleR = Rendering.HandGrabScaleR;
			HandGrabAttackTwistL = Rendering.HandGrabAttackTwistL;
			HandGrabAttackTwistR = Rendering.HandGrabAttackTwistR;
		}
		public void BlendToPose (float blend01) {
			blend01 = 1f - blend01;
			for (int i = 0; i < BodyParts.Length; i++) {
				var record = BodyParts[i];
				var pose = Rendering.BodyParts[i];

				pose.Width = FixSign(pose.Width, record.Width);
				pose.Height = FixSign(pose.Height, record.Height);

				pose.X = (int)Util.LerpUnclamped(pose.X, record.X, blend01);
				pose.Y = (int)Util.LerpUnclamped(pose.Y, record.Y, blend01);
				pose.Z = (int)Util.LerpUnclamped(pose.Z, record.Z, blend01);
				pose.Rotation = (int)Util.LerpUnclamped(pose.Rotation, record.Rotation, blend01);
				pose.Width = (int)Util.LerpUnclamped(pose.Width, record.Width, blend01);
				pose.Height = (int)Util.LerpUnclamped(pose.Height, record.Height, blend01);
				pose.PivotX = (int)Util.LerpUnclamped(pose.PivotX, record.PivotX, blend01);
				pose.PivotY = (int)Util.LerpUnclamped(pose.PivotY, record.PivotY, blend01);
				pose.Tint = Color32.Lerp(pose.Tint, record.Tint, blend01);
			}

			Rendering.HandGrabScaleL = FixSign(Rendering.HandGrabScaleL, HandGrabScaleL);
			Rendering.HandGrabScaleR = FixSign(Rendering.HandGrabScaleR, HandGrabScaleR);

			Rendering.PoseRootX = (int)Util.LerpUnclamped(Rendering.PoseRootX, PoseRootX, blend01);
			Rendering.PoseRootY = (int)Util.LerpUnclamped(Rendering.PoseRootY, PoseRootY, blend01);
			Rendering.BodyTwist = (int)Util.LerpUnclamped(Rendering.BodyTwist, BodyTwist, blend01);
			Rendering.HeadTwist = (int)Util.LerpUnclamped(Rendering.HeadTwist, HeadTwist, blend01);
			Rendering.HandGrabRotationL = (int)Util.LerpAngle(Rendering.HandGrabRotationL, HandGrabRotationL, blend01);
			Rendering.HandGrabRotationR = (int)Util.LerpAngle(Rendering.HandGrabRotationR, HandGrabRotationR, blend01);
			Rendering.HandGrabScaleL = (int)Util.LerpUnclamped(Rendering.HandGrabScaleL, HandGrabScaleL, blend01);
			Rendering.HandGrabScaleR = (int)Util.LerpUnclamped(Rendering.HandGrabScaleR, HandGrabScaleR, blend01);
			Rendering.HandGrabAttackTwistL = (int)Util.LerpUnclamped(Rendering.HandGrabAttackTwistL, HandGrabAttackTwistL, blend01);
			Rendering.HandGrabAttackTwistR = (int)Util.LerpUnclamped(Rendering.HandGrabAttackTwistR, HandGrabAttackTwistR, blend01);


			// Func
			static int FixSign (int basicValue, int targetValue) {
				if (basicValue.Sign() != targetValue.Sign()) {
					basicValue = -basicValue;
				}
				return basicValue;
			}
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	protected const int POSE_Z_HAND = 36;
	protected const int A2G = Const.CEL / Const.ART_CEL;

	// Api
	protected virtual bool ValidHeadPosition => true;
	protected virtual bool DontBlendToNext => false;
	protected virtual bool DontBlendToPrev => false;

	// Data
	private static readonly Dictionary<int, PoseAnimation> Pool = [];
	private static readonly BlendRecord BlendRecordDef = new();
	private static readonly BlendRecord BlendRecordAni = new();

	// Cache
	protected static Character Target = null;
	protected static PoseCharacterRenderer Rendering = null;
	protected static CharacterMovement Movement;
	protected static CharacterAttackness Attackness;
	protected static BodyPart Head = null;
	protected static BodyPart Body = null;
	protected static BodyPart Hip = null;
	protected static BodyPart ShoulderL = null;
	protected static BodyPart ShoulderR = null;
	protected static BodyPart UpperArmL = null;
	protected static BodyPart UpperArmR = null;
	protected static BodyPart LowerArmL = null;
	protected static BodyPart LowerArmR = null;
	protected static BodyPart HandL = null;
	protected static BodyPart HandR = null;
	protected static BodyPart UpperLegL = null;
	protected static BodyPart UpperLegR = null;
	protected static BodyPart LowerLegL = null;
	protected static BodyPart LowerLegR = null;
	protected static BodyPart FootL = null;
	protected static BodyPart FootR = null;
	protected static bool FacingRight = true;
	protected static bool FacingFront = true;
	protected static bool IsChargingAttack = false;
	protected static int FacingSign = 0;
	protected static int FrontSign = 0;
	protected static int CurrentAnimationFrame = 0;
	protected static CharacterAnimationType AnimationType;
	protected static float AttackLerp;
	protected static float AttackEase;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-129)]
	public static void OnGameInitialize () {

		// Code >> Pool
		Pool.Clear();
		foreach (var type in typeof(PoseAnimation).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not PoseAnimation ani) continue;
			int aniID = type.AngeHash();
			Pool.TryAdd(aniID, ani);
		}
		Pool.TrimExcess();

	}


	public static void PerformAnimationFromPool (int id, PoseCharacterRenderer renderer) {
		if (Pool.TryGetValue(id, out var result)) {
			PerformAnimation(result, renderer);
		} else {
			// Valid Head Position
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}
	}


	public static void PerformAnimation (PoseAnimation animation, PoseCharacterRenderer renderer) {
		animation.Animate(renderer);
		if (animation.ValidHeadPosition) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}
	}


	public static void PerformAnimationBlendFromPool (int idA, int idB, float blend01, PoseCharacterRenderer renderer) {
		// Get Animation
		Pool.TryGetValue(idA, out var resultA);
		Pool.TryGetValue(idB, out var resultB);
		// Check for Blend Ignore
		if (resultA.DontBlendToNext || resultB.DontBlendToPrev) {
			resultA = null;
		}
		// Perform
		if (resultA != null && resultB != null) {
			PerformAnimationBlend(resultA, resultB, blend01, renderer);
		} else if (resultA != null || resultB != null) {
			PerformAnimation(resultA ?? resultB, renderer);
		} else {
			// Valid Head Position
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}
	}


	public static void PerformAnimationBlend (PoseAnimation animationA, PoseAnimation animationB, float blend01, PoseCharacterRenderer renderer) {

		SetAsAnimating(renderer);

		// Record Def
		BlendRecordDef.RecordFromPose();

		// Perform A
		animationA.Animate(renderer);
		if (animationA.ValidHeadPosition) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}

		// Record A
		BlendRecordAni.RecordFromPose();

		// Back to Def
		BlendRecordDef.ApplyToPose();

		// Perform B
		animationB.Animate(renderer);
		if (animationB.ValidHeadPosition) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}

		// Blend with Recorded A
		BlendRecordAni.BlendToPose(blend01);

	}


	public virtual void Animate (PoseCharacterRenderer renderer) => SetAsAnimating(renderer);


	#endregion




	#region --- API ---


	protected static void SetAsAnimating (PoseCharacterRenderer renderer) {
		Target = renderer.TargetCharacter;
		Rendering = renderer;
		Attackness = renderer.TargetCharacter.Attackness;
		Movement = renderer.TargetCharacter.Movement;
		CurrentAnimationFrame = renderer.CurrentAnimationFrame;
		Head = renderer.Head;
		Body = renderer.Body;
		Hip = renderer.Hip;
		ShoulderL = renderer.ShoulderL;
		ShoulderR = renderer.ShoulderR;
		UpperArmL = renderer.UpperArmL;
		UpperArmR = renderer.UpperArmR;
		LowerArmL = renderer.LowerArmL;
		LowerArmR = renderer.LowerArmR;
		HandL = renderer.HandL;
		HandR = renderer.HandR;
		UpperLegL = renderer.UpperLegL;
		UpperLegR = renderer.UpperLegR;
		LowerLegL = renderer.LowerLegL;
		LowerLegR = renderer.LowerLegR;
		FootL = renderer.FootL;
		FootR = renderer.FootR;
		FacingRight = Movement.FacingRight;
		FacingFront = Movement.FacingFront;
		AnimationType = renderer.TargetCharacter.AnimationType;
		FacingSign = FacingRight ? 1 : -1;
		FrontSign = FacingFront ? 1 : -1;
		IsChargingAttack = !Attackness.IsAttacking && Attackness.IsChargingAttack && Attackness.AttackChargeStartFrame.HasValue;
		AttackLerp = IsChargingAttack ?
			((float)(Game.GlobalFrame - Attackness.AttackChargeStartFrame.Value) / Util.Max(Attackness.MinimalChargeAttackDuration, 1)).Clamp01() :
			(float)(Game.GlobalFrame - Attackness.LastAttackFrame) / Attackness.AttackDuration;
		AttackEase = IsChargingAttack ? 1f - Ease.OutBack(AttackLerp) : Ease.OutBack(AttackLerp);
		if (IsChargingAttack) {
			AttackLerp = 1f - AttackLerp;
		}
	}


	protected static void ResetShoulderAndUpperArmPos (bool resetLeft = true, bool resetRight = true) => FrameworkUtil.ResetShoulderAndUpperArmPos(Rendering, resetLeft, resetRight);


	protected static void AttackHeadDown (float ease01, int headOffsetXAmount = 1000, int headOffsetYAmount = 1000, int headRotateAmount = 1000) {

		// Head Rotate
		int headRotate = FacingSign * (int)Util.LerpUnclamped(-18, 18, ease01) * headRotateAmount / 1000;

		// Head
		int headOffsetX = FacingSign * (int)((0.75f - ease01) * 10 * A2G) - A2G / 3;
		int headOffsetY = (int)((0.75f - ease01) * 10 * A2G) - 5 * A2G;
		headOffsetX = headOffsetX * headOffsetXAmount / 1000;
		headOffsetY = headOffsetY * headOffsetYAmount / 1000;
		if (
			AnimationType == CharacterAnimationType.SquatIdle ||
			AnimationType == CharacterAnimationType.SquatMove
		) {
			headOffsetX /= 2;
			headOffsetY /= 2;
			headRotate /= 2;
		} else if (
			AnimationType == CharacterAnimationType.JumpDown ||
			AnimationType == CharacterAnimationType.JumpUp
		) {
			headOffsetX /= 4;
			headOffsetY /= 4;
			headRotate /= 4;
		}

		Head.X -= headOffsetX.Clamp(-A2G * 2, A2G * 2);
		Head.Y = (Head.Y + headOffsetY).GreaterOrEquel(Body.Y + 1);
		Head.Rotation = headRotate;
		Body.Height = Head.Y - Body.Y;

	}


	protected static void AttackLegShake (float ease01) {
		if (AnimationType != CharacterAnimationType.Idle) return;
		int deltaX = (int)(1f * ease01 * A2G);
		// Move Leg
		if (FacingRight) {
			UpperLegL.X -= deltaX / 2;
			LowerLegL.X -= deltaX;
			FootL.X -= deltaX;
			UpperLegR.X += deltaX / 8;
			LowerLegR.X += deltaX / 4;
			FootR.X += deltaX / 4;
		} else {
			UpperLegL.X -= deltaX / 8;
			LowerLegL.X -= deltaX / 4;
			FootL.X -= deltaX / 4;
			UpperLegR.X += deltaX / 2;
			LowerLegR.X += deltaX;
			FootR.X += deltaX;
		}
	}


	#endregion




}