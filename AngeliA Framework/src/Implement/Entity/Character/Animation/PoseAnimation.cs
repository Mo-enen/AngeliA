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

		// File >> Pool
		foreach (var path in Util.EnumerateFiles(Universe.BuiltIn.CharacterAnimationRoot, true, "*.json")) {
			if (JsonUtil.LoadJsonFromPath<ModularAnimation>(path) is not ModularAnimation ani) continue;
			ani.Name = Util.GetNameWithoutExtension(path);
			ani.ID = ani.Name.AngeHash();
			Pool.TryAdd(ani.ID, ani);
			// Default Check
			int tIndex;
			switch (ani.Override) {
				case ModularAnimation.CharacterOverrideType.Pose:
					tIndex = (int)ani.PoseType;
					if (tIndex < 0 || tIndex >= PoseDefaultPool.Length) break;
					PoseDefaultPool[tIndex].TryAdd(ani.CharacterName.AngeHash(), ani.ID);
					break;
				case ModularAnimation.CharacterOverrideType.Handheld:
					tIndex = (int)ani.Handheld;
					if (tIndex < 0 || tIndex >= HandheldDefaultPool.Length) break;
					HandheldDefaultPool[tIndex].TryAdd(ani.CharacterName.AngeHash(), ani.ID);
					break;
				case ModularAnimation.CharacterOverrideType.Attack:
					tIndex = (int)ani.AttackType;
					if (tIndex < 0 || tIndex >= AttackDefaultPool.Length) break;
					AttackDefaultPool[tIndex].TryAdd(ani.CharacterName.AngeHash(), ani.ID);
					break;
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


	protected static void ResetShoulderAndUpperArm (bool resetLeft = true, bool resetRight = true) {
		var character = Target;
		if (resetLeft) {
			int bodyBorderL = character.FacingRight ? character.Body.Border.left : character.Body.Border.right;
			character.ShoulderL.X = character.Body.X - character.Body.Width.Abs() / 2 + bodyBorderL;
			character.ShoulderL.Y = character.Body.Y + character.Body.Height - character.Body.Border.up;
			character.ShoulderL.Height = Util.Min(character.ShoulderL.Height, character.Body.Height);
			character.ShoulderL.PivotX = 1000;
			character.UpperArmL.X = character.ShoulderL.X;
			character.UpperArmL.Y = character.ShoulderL.Y - character.ShoulderL.Height + character.ShoulderL.Border.down;
			character.UpperArmL.PivotX = 1000;
			character.UpperArmL.Height = character.UpperArmL.SizeY;
		}
		if (resetRight) {
			int bodyBorderR = character.FacingRight ? character.Body.Border.right : character.Body.Border.left;
			character.ShoulderR.X = character.Body.X + character.Body.Width.Abs() / 2 - bodyBorderR;
			character.ShoulderR.Y = character.Body.Y + character.Body.Height - character.Body.Border.up;
			character.ShoulderR.Height = Util.Min(character.ShoulderR.Height, character.Body.Height);
			character.ShoulderR.PivotX = 1000;
			character.UpperArmR.X = character.ShoulderR.X;
			character.UpperArmR.Y = character.ShoulderR.Y - character.ShoulderR.Height + character.ShoulderR.Border.down;
			character.UpperArmR.PivotX = 0;
			character.UpperArmR.Height = character.UpperArmR.SizeY;
		}
	}


	protected static void RollRandomFactor (int count = 4) {
		if (count > 0) RandomFactor0 = Util.RandomInt(0, 1001);
		if (count > 1) RandomFactor1 = Util.RandomInt(0, 1001);
		if (count > 2) RandomFactor2 = Util.RandomInt(0, 1001);
		if (count > 3) RandomFactor3 = Util.RandomInt(0, 1001);
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
		Target.HeadRotation = headRotate;

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