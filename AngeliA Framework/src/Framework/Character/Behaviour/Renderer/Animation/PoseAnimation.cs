using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[System.Serializable]
public abstract class PoseAnimation {




	#region --- VAR ---


	// Const
	protected const int POSE_Z_HAND = 36;
	protected const int A2G = Const.CEL / Const.ART_CEL;

	// Api
	protected virtual bool ValidHeadPosition => true;

	// Data
	private static readonly Dictionary<int, PoseAnimation> Pool = [];
	
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

		Pool.Clear();

		// Code >> Pool
		foreach (var type in typeof(PoseAnimation).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not PoseAnimation ani) continue;
			int aniID = type.AngeHash();
			Pool.TryAdd(aniID, ani);
		}

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


	public virtual void Animate (PoseCharacterRenderer renderer) {
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




	#region --- API ---


	protected static void ResetShoulderAndUpperArmPos (bool resetLeft = true, bool resetRight = true) {

		int bodyHipSizeY = Body.SizeY + Hip.SizeY;
		int targetUnitHeight = Rendering.CharacterHeight * A2G / PoseCharacterRenderer.CM_PER_PX - Head.SizeY;
		int legRootSize = UpperLegL.SizeY + LowerLegL.SizeY + FootL.SizeY;
		int defaultCharHeight = bodyHipSizeY + legRootSize;

		int bodyBorderU = Body.Border.up * targetUnitHeight / defaultCharHeight * Body.Height.Abs() / Body.SizeY;
		int bodyBorderL = (FacingRight ? Body.Border.left : Body.Border.right) * Body.Width.Abs() / Body.SizeX;
		int bodyBorderR = (FacingRight ? Body.Border.right : Body.Border.left) * Body.Width.Abs() / Body.SizeX;

		if (resetLeft) {

			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - bodyBorderU;
			ShoulderL.Width = ShoulderL.SizeX;
			ShoulderL.Height = ShoulderL.SizeY;
			ShoulderL.PivotX = 1000;
			ShoulderL.PivotY = 1000;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.Width = UpperArmL.SizeX;
			UpperArmL.Height = UpperArmL.FlexableSizeY;
			UpperArmL.PivotX = 1000;
			UpperArmL.PivotY = 1000;

		}

		if (resetRight) {
			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - bodyBorderU;
			ShoulderR.Width = -ShoulderR.SizeX;
			ShoulderR.Height = ShoulderR.SizeY;
			ShoulderR.PivotX = 1000;
			ShoulderR.PivotY = 1000;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.Width = UpperArmR.SizeX;
			UpperArmR.Height = UpperArmR.FlexableSizeY;
			UpperArmR.PivotX = 0;
			UpperArmR.PivotY = 1000;
		}
	}


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
		int deltaX = (int)(2f * ease01 * A2G);
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