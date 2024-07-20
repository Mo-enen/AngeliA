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
	private static readonly Dictionary<int, PoseAnimation> Pool = new();
	private static readonly Dictionary<int, int>[] PoseDefaultPool = new Dictionary<int, int>[typeof(CharacterAnimationType).EnumLength()].FillWithNewValue();
	private static readonly Dictionary<int, int>[] HandheldDefaultPool = new Dictionary<int, int>[typeof(WeaponHandheld).EnumLength()].FillWithNewValue();
	private static readonly Dictionary<int, int>[] AttackDefaultPool = new Dictionary<int, int>[typeof(WeaponType).EnumLength()].FillWithNewValue();

	// Cache
	protected static PoseCharacter Target = null;
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
	protected static int FacingSign = 0;
	protected static int FrontSign = 0;
	protected static int CurrentAnimationFrame = 0;
	protected static int RandomFactor0 = 0;
	protected static int RandomFactor1 = 0;
	protected static int RandomFactor2 = 0;
	protected static int RandomFactor3 = 0;
	protected static CharacterAnimationType AnimationType;


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
			// Attribute >> Default
			int tIndex;
			foreach (var att in type.GetCustomAttributes(false)) {
				switch (att) {
					case DefaultCharacterAnimationAttribute aniAtt:
						tIndex = (int)aniAtt.Type;
						if (tIndex < 0 || tIndex >= PoseDefaultPool.Length) break;
						PoseDefaultPool[tIndex].TryAdd(aniAtt.CharacterID, aniID);
						break;
					case DefaultCharacterHandheldAnimationAttribute heldAtt:
						tIndex = (int)heldAtt.Held;
						if (tIndex < 0 || tIndex >= HandheldDefaultPool.Length) break;
						HandheldDefaultPool[tIndex].TryAdd(heldAtt.CharacterID, aniID);
						break;
					case DefaultCharacterAttackAnimationAttribute attAtt:
						tIndex = (int)attAtt.Type;
						if (tIndex < 0 || tIndex >= AttackDefaultPool.Length) break;
						AttackDefaultPool[tIndex].TryAdd(attAtt.CharacterID, aniID);
						break;
				}
			}
		}

	}


	public static bool TryGetPoseAnimationDefaultID (int characterID, CharacterAnimationType type, out int animationID) => PoseDefaultPool[(int)type].TryGetValue(characterID, out animationID);
	public static bool TryGetHandheldDefaultID (int characterID, WeaponHandheld handheld, out int animationID) => HandheldDefaultPool[(int)handheld].TryGetValue(characterID, out animationID);
	public static bool TryGetAttackDefaultID (int characterID, WeaponType type, out int animationID) => AttackDefaultPool[(int)type].TryGetValue(characterID, out animationID);


	public static void AnimateFromPool (int id, PoseCharacter character) {
		bool validHeadPos = true;
		if (Pool.TryGetValue(id, out var result)) {
			result.Animate(character);
			validHeadPos = result.ValidHeadPosition;
		}
		if (validHeadPos) {
			Head.Y = Head.Y.GreaterOrEquel(Body.Y + 1);
			Body.Height = Body.Height.GreaterOrEquel(1);
		}
	}


	public virtual void Animate (PoseCharacter character) {
		if (character == Target && character.CurrentAnimationFrame == CurrentAnimationFrame) return;
		Target = character;
		CurrentAnimationFrame = character.CurrentAnimationFrame;
		Head = character.Head;
		Body = character.Body;
		Hip = character.Hip;
		ShoulderL = character.ShoulderL;
		ShoulderR = character.ShoulderR;
		UpperArmL = character.UpperArmL;
		UpperArmR = character.UpperArmR;
		LowerArmL = character.LowerArmL;
		LowerArmR = character.LowerArmR;
		HandL = character.HandL;
		HandR = character.HandR;
		UpperLegL = character.UpperLegL;
		UpperLegR = character.UpperLegR;
		LowerLegL = character.LowerLegL;
		LowerLegR = character.LowerLegR;
		FootL = character.FootL;
		FootR = character.FootR;
		FacingRight = character.FacingRight;
		FacingFront = character.FacingFront;
		AnimationType = character.AnimationType;
		FacingSign = FacingRight ? 1 : -1;
		FrontSign = FacingFront ? 1 : -1;
	}


	#endregion




	#region --- API ---


	protected static void ResetShoulderAndUpperArmPos (bool resetLeft = true, bool resetRight = true) {
		
		int bodyHipSizeY = Body.SizeY + Hip.SizeY;
		int targetUnitHeight = Target.CharacterHeight * A2G / PoseCharacter.CM_PER_PX - Head.SizeY;
		int legRootSize = UpperLegL.SizeY + LowerLegL.SizeY + FootL.SizeY;
		int defaultCharHeight = bodyHipSizeY + legRootSize;

		int bodyBorderU = Body.Border.up * targetUnitHeight / defaultCharHeight;
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


	protected static void RollRandomFactor (int count = 4) {
		if (count > 0) RandomFactor0 = Util.QuickRandom(0, 1001);
		if (count > 1) RandomFactor1 = Util.QuickRandom(0, 1001);
		if (count > 2) RandomFactor2 = Util.QuickRandom(0, 1001);
		if (count > 3) RandomFactor3 = Util.QuickRandom(0, 1001);
	}


	protected static void AttackHeadDown (float ease01, int headOffsetXAmount = 1000, int headOffsetYAmount = 1000, int bodyOffsetYAmount = 1000, int headRotateAmount = 1000) {

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

		// Body
		int bodyOffsetY = (int)(ease01 * A2G) + A2G * 2;
		bodyOffsetY = bodyOffsetY * bodyOffsetYAmount / 1000;
		Body.Y -= bodyOffsetY;
		Hip.Y -= bodyOffsetY;
		Body.Height = Head.Y - Body.Y;

		// Leg Position Y
		if (bodyOffsetY != 0) {
			UpperLegL.Y -= bodyOffsetY;
			UpperLegR.Y -= bodyOffsetY;
			UpperLegL.Height -= bodyOffsetY;
			UpperLegR.Height -= bodyOffsetY;
		}
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