using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Procedure animation to animate a pose-style character. ⚠Use global single instance from system pool⚠
/// </summary>
public abstract class PoseAnimation {




	#region --- VAR ---


	// Const
	protected const int POSE_Z_HAND = 36;
	/// <summary>
	/// Scale values from artwork pixel space to global space
	/// </summary>
	protected const int A2G = Const.CEL / Const.ART_CEL;

	// Api
	/// <summary>
	/// True if head position need to be recalculate after perform this animation
	/// </summary>
	protected virtual bool ValidHeadPosition => true;
	/// <summary>
	/// True if this animation should immediately transition to the next
	/// </summary>
	protected virtual bool DontBlendToNext => false;
	/// <summary>
	/// True if the prev animation should immediately transition to this one
	/// </summary>
	protected virtual bool DontBlendToPrev => false;

	// Data
	private static readonly Dictionary<int, PoseAnimation> Pool = [];
	private static readonly CharacterPose BlendRecordDef = new();
	private static readonly CharacterPose BlendRecordAni = new();

	// Cache
	/// <summary>
	/// Character that currently being animated
	/// </summary>
	protected static Character Target = null;
	/// <summary>
	/// Rendering behavior of the target character
	/// </summary>
	protected static PoseCharacterRenderer Rendering = null;
	/// <summary>
	/// Movement behavior of the target character
	/// </summary>
	protected static CharacterMovement Movement;
	/// <summary>
	/// Attackness behavior of the target character
	/// </summary>
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
	/// <summary>
	/// True if the target character is facing right
	/// </summary>
	protected static bool FacingRight = true;
	/// <summary>
	/// True if the target character is facing front
	/// </summary>
	protected static bool FacingFront = true;
	/// <summary>
	/// True if the target character is currently attacking
	/// </summary>
	protected static bool IsChargingAttack = false;
	/// <summary>
	/// Return 1 if the target character is facing right
	/// </summary>
	protected static int FacingSign = 0;
	/// <summary>
	/// Return 1 if the target character is facing front
	/// </summary>
	protected static int FrontSign = 0;
	/// <summary>
	/// Local animation frame of the target character
	/// </summary>
	protected static int CurrentAnimationFrame = 0;
	/// <summary>
	/// Which type of animation does the current character require to show
	/// </summary>
	protected static CharacterAnimationType AnimationType;
	/// <summary>
	/// Liner progress of attack. (0 means start, 1 means end)
	/// </summary>
	protected static float AttackLerp;
	/// <summary>
	/// Eased progress of attack. (0 means start, 1 means end)
	/// </summary>
	protected static float AttackEase;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-129)]
	internal static void OnGameInitialize () {

		// Code >> Pool
		Pool.Clear();
		foreach (var type in typeof(PoseAnimation).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not PoseAnimation ani) continue;
			int aniID = type.AngeHash();
			Pool.TryAdd(aniID, ani);
		}
		Pool.TrimExcess();

	}


	/// <summary>
	/// Animate the target character from animation inside system pool
	/// </summary>
	public static void PerformAnimationFromPool (int id, PoseCharacterRenderer renderer) {
		if (Pool.TryGetValue(id, out var result)) {
			PerformAnimation(result, renderer);
		}
	}


	/// <summary>
	/// Animate target character from given animation
	/// </summary>
	public static void PerformAnimation (PoseAnimation animation, PoseCharacterRenderer renderer) {
		animation.Animate(renderer);
		if (animation.ValidHeadPosition) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}
	}


	/// <summary>
	/// Animate the target character from two animations inside system pool
	/// </summary>
	/// <param name="idA"></param>
	/// <param name="idB"></param>
	/// <param name="blend01">0 means only perform animationA, 1 means only perform animationB. 0.5 means perform two animations equally.</param>
	/// <param name="renderer"></param>
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
		}
	}


	/// <summary>
	/// Animate the target character from two given animations
	/// </summary>
	/// <param name="animationA"></param>
	/// <param name="animationB"></param>
	/// <param name="blend01">0 means only perform animationA, 1 means only perform animationB. 0.5 means perform two animations equally.</param>
	/// <param name="renderer"></param>
	public static void PerformAnimationBlend (PoseAnimation animationA, PoseAnimation animationB, float blend01, PoseCharacterRenderer renderer) {

		SetAsAnimating(renderer);

		// Record Def
		BlendRecordDef.RecordFromCharacter(Rendering);

		// Perform A
		animationA.Animate(renderer);
		if (animationA.ValidHeadPosition) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}

		// Record A
		BlendRecordAni.RecordFromCharacter(Rendering);

		// Back to Def
		BlendRecordDef.ApplyToCharacter(Rendering);

		// Perform B
		animationB.Animate(renderer);
		if (animationB.ValidHeadPosition) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}

		// Blend with Recorded A
		BlendRecordAni.BlendToCharacter(Rendering, 1f - blend01);

	}


	/// <summary>
	/// Get global single instance from system pool
	/// </summary>
	public static bool TryGetAnimationFromPool (int id, out PoseAnimation result) => Pool.TryGetValue(id, out result);


	/// <summary>
	/// Perform the animation logic to the given character
	/// </summary>
	public virtual void Animate (PoseCharacterRenderer renderer) => SetAsAnimating(renderer);


	#endregion




	#region --- API ---


	/// <summary>
	/// Reset position of soulder and upper arm for cached character
	/// </summary>
	protected static void ResetShoulderAndUpperArmPos (bool resetLeft = true, bool resetRight = true) => FrameworkUtil.ResetShoulderAndUpperArmPos(Rendering, resetLeft, resetRight);


	/// <summary>
	/// Make head moves down for attack animation for cached character
	/// </summary>
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
		Body.Rotation = FacingSign * (int)((ease01 - 0.3f) * 15);

	}


	/// <summary>
	/// Make legs shake for attack animation for cached character
	/// </summary>
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




	#region --- LGC ---


	private static void SetAsAnimating (PoseCharacterRenderer renderer) {
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


	#endregion




}