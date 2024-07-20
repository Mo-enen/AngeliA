using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AngeliA;

[System.Serializable]
public sealed class ModularAnimation : PoseAnimation, IJsonSerializationCallback {




	#region --- SUB ---


	public enum CharacterOverrideType { None, Pose, Handheld, Attack, }


	public enum BindingType {
		Rotation,
		Twist,
		GrabRot, GrabScale, GrabTwist,
		Width, Height, X, Y,
	}


	public enum BindingTarget {
		Head, Body, Hip,
		ShoulderL, ShoulderR, UpperArmL, UpperArmR, LowerArmL, LowerArmR, HandL, HandR,
		UpperLegL, UpperLegR, LowerLegL, LowerLegR, FootL, FootR,
	}


	[System.Serializable]
	public struct KeyFrame {
		[JsonPropertyName("f")] public int Frame;
		[JsonPropertyName("v")] public int Value;
		[JsonPropertyName("e")] public EaseType Ease;
		public KeyFrame (int frame, int value, EaseType ease = EaseType.Const) {
			Frame = frame;
			Value = value;
			Ease = ease;
		}
	}


	[System.Serializable]
	public class KeyLayer {

		private class KeyFrameComparer : IComparer<KeyFrame> {
			public static readonly KeyFrameComparer Instance = new();
			public int Compare (KeyFrame x, KeyFrame y) => x.Frame.CompareTo(y.Frame);
		}

		public BindingType BindingType;
		public BindingTarget BindingTarget;
		public bool FlipAngleFromCharacterFacing = true;
		public List<KeyFrame> KeyFrames = new();

		public void Sort () => KeyFrames.Sort(KeyFrameComparer.Instance);

		public int Evaluate (int frame) => Evaluate(frame, 0, out _);
		internal int Evaluate (int frame, int fromKeyFrame, out int keyFrame) {
			keyFrame = 0;
			if (KeyFrames.Count == 0) return default;
			if (KeyFrames.Count == 1) return KeyFrames[0].Value;
			int index = -1;
			for (int i = fromKeyFrame; i < KeyFrames.Count; i++) {
				var k = KeyFrames[i];
				if (k.Frame > frame) break;
				index = i;
			}
			if (index == -1) return KeyFrames[0].Value;
			keyFrame = KeyFrames.Count - 1;
			if (index == KeyFrames.Count - 1) return KeyFrames[^1].Value;
			keyFrame = index;
			var left = KeyFrames[index];
			var right = KeyFrames[index + 1];
			float lerp01 = Util.InverseLerpUnclamped(left.Frame, right.Frame, frame);
			lerp01 = Ease.Invoke(left.Ease, lerp01);
			return Util.LerpUnclamped(left.Value, right.Value, lerp01).RoundToInt();
		}

	}


	private class RawLayer {
		public BindingType BindingType;
		public BindingTarget BindingTarget;
		public bool FlipAngleFromCharacterFacing = true;
		public int[] RawData;
	}


	private class KeyLayerComparer : IComparer<KeyLayer> {
		public static readonly KeyLayerComparer Instance = new();
		public int Compare (KeyLayer x, KeyLayer y) {
			int result = ((int)x.BindingTarget).CompareTo((int)y.BindingTarget);
			return result != 0 ? result : ((int)x.BindingType).CompareTo((int)y.BindingType);
		}
	}


	#endregion




	#region --- VAR ---



	// Const
	public const int MAX_RAW_LENGTH = 600;
	private const int MXSZ = Const.CEL;
	private static readonly (int min, int max)[,] RANGE_MAP = {// [ Target-17, Type-9 ]
						//  Rot,           Twist,         GrabR,         GrabS,         GrabT,         W,             H,             X,             Y,
		/* Head */		{   (-90 ,   90),  (-1000,1000),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* Body */		{   (0   , 0   ),  (-1000,1000),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* Hip  */		{   (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* ShoulderL */	{   (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* ShoulderR */	{   (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* UpperArmL */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* UpperArmR */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* LowerArmL */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* LowerArmR */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* HandL */		{   (0   , 0   ),  (0   , 0   ),  (-360,  360),  (0000, 3000),  (-1000,1000),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* HandR */		{   (0   , 0   ),  (0   , 0   ),  (-360,  360),  (0000, 3000),  (-1000,1000),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* UpperLegL */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* UpperLegR */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* LowerLegL */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* LowerLegR */	{   (-360,  360),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* FootL */		{   (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
		/* FootR */		{   (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (0   , 0   ),  (-MXSZ,MXSZ),  (0000, MXSZ),  (-MXSZ,MXSZ),  (-MXSZ,MXSZ), },
	};
	private static readonly int[] STEP_MAP = {
		// Rot, Twist, GrabR, GrabS, GrabT, W,  H,  X,  Y,
		   5,   10,    5,     10,    10,    16, 16, 16, 16,
	};

	// Api
	[JsonIgnore] public bool UseRawData => !Game.IsToolApplication;
	[JsonIgnore] public int Duration { get; private set; } = 1;

	// Ser
	[JsonIgnore] public int ID;
	[JsonIgnore] public string Name = string.Empty;
	[JsonIgnore] public CharacterOverrideType Override = CharacterOverrideType.None;
	public List<KeyLayer> KeyLayers = new();
	public bool FlipLimbsFromCharacterFacing = false;

	// Data
	[JsonIgnore] RawLayer[] RawLayers = System.Array.Empty<RawLayer>();


	#endregion




	#region --- MSG ---


	public override void Animate (PoseCharacter character) {
		int frame = Override switch {
			CharacterOverrideType.Attack => Game.GlobalFrame - character.LastAttackFrame,
			CharacterOverrideType.Pose => character.CurrentAnimationFrame,
			CharacterOverrideType.Handheld => character.IsChargingAttack ? GetFrameForCharging(character, Duration) : character.CurrentAnimationFrame,
			_ => character.CurrentAnimationFrame,
		};
		if (UseRawData) {
			AnimateFromRawData(character, frame, false);
		} else {
			AnimateFromKeyFrame(character, frame, false);
		}
		// Apply Limb 
		ApplyLimbRotate(character.UpperArmL);
		ApplyLimbRotate(character.UpperArmR);
		ApplyLimbRotate(character.LowerArmL);
		ApplyLimbRotate(character.LowerArmR);
		ApplyLimbRotate(character.HandL);
		ApplyLimbRotate(character.HandR);
		ApplyLimbRotate(character.UpperLegL);
		ApplyLimbRotate(character.UpperLegR);
		ApplyLimbRotate(character.LowerLegL);
		ApplyLimbRotate(character.LowerLegR);
		ApplyLimbRotate(character.FootL);
		ApplyLimbRotate(character.FootR);
		// Pos Only
		if (UseRawData) {
			AnimateFromRawData(character, frame, true);
		} else {
			AnimateFromKeyFrame(character, frame, true);
		}
		// Func
		static int GetFrameForCharging (PoseCharacter character, int duration) {
			float lerp01 = ((float)(Game.GlobalFrame - character.AttackChargeStartFrame.Value) / Util.Max(character.MinimalChargeAttackDuration * 2, 1)).Clamp01();
			return (lerp01 * duration).RoundToInt();
		}
	}


	private void AnimateFromKeyFrame (PoseCharacter character, int frame, bool forPos) {
		if (KeyLayers == null || KeyLayers.Count == 0) return;
		if (forPos) {
			// Pos Only
			for (int i = 0; i < KeyLayers.Count; i++) {
				var layer = KeyLayers[i];
				if (layer.BindingType != BindingType.X && layer.BindingType != BindingType.Y) continue;
				var value = layer.Evaluate(frame.UMod(Duration));
				PerformFrame(character, layer.BindingType, layer.BindingTarget, value, layer.FlipAngleFromCharacterFacing);
			}
		} else {
			// No Pos
			for (int i = 0; i < KeyLayers.Count; i++) {
				var layer = KeyLayers[i];
				if (layer.BindingType == BindingType.X || layer.BindingType == BindingType.Y) continue;
				var value = layer.Evaluate(frame.UMod(Duration));
				PerformFrame(character, layer.BindingType, layer.BindingTarget, value, layer.FlipAngleFromCharacterFacing);
			}
		}
	}


	private void AnimateFromRawData (PoseCharacter character, int frame, bool forPos) {
		if (RawLayers == null) Key_to_Raw();
		if (RawLayers == null || RawLayers.Length == 0) return;
		if (forPos) {
			// Pos Only
			for (int i = 0; i < RawLayers.Length; i++) {
				var layer = RawLayers[i];
				if (layer.BindingType != BindingType.X && layer.BindingType != BindingType.Y) continue;
				if (layer.RawData.Length == 0) continue;
				int _frame = frame.UMod(layer.RawData.Length);
				PerformFrame(character, layer.BindingType, layer.BindingTarget, layer.RawData[_frame], layer.FlipAngleFromCharacterFacing);
			};
		} else {
			// No Pos
			for (int i = 0; i < RawLayers.Length; i++) {
				var layer = RawLayers[i];
				if (layer.BindingType == BindingType.X || layer.BindingType == BindingType.Y) continue;
				if (layer.RawData.Length == 0) continue;
				int _frame = frame.UMod(layer.RawData.Length);
				PerformFrame(character, layer.BindingType, layer.BindingTarget, layer.RawData[_frame], layer.FlipAngleFromCharacterFacing);
			};
		}
	}


	public void OnBeforeSaveToDisk () {
		if (KeyLayers == null) return;
		foreach (var layer in KeyLayers) {
			layer.Sort();
		}
	}


	public void OnAfterLoadedFromDisk () {
		KeyLayers ??= new();
		foreach (var layer in KeyLayers) {
			layer.Sort();
			layer.KeyFrames ??= new();
		}
		CalculateDuration();
		if (UseRawData) {
			Key_to_Raw();
		}
	}


	public void SortKeyFramesForAllLayers () {
		foreach (var layer in KeyLayers) layer.Sort();
	}


	public void SortAllLayers () => KeyLayers.Sort(KeyLayerComparer.Instance);


	#endregion




	#region --- API ---


	public void CalculateDuration () {
		Duration = 1;
		foreach (var layer in KeyLayers) {
			if (layer.KeyFrames.Count > 0) {
				Duration = Util.Max(Duration, layer.KeyFrames[^1].Frame);
			}
		}
	}


	public bool HasPair (BindingType type, BindingTarget target) {
		foreach (var layer in KeyLayers) {
			if (layer.BindingType == type && layer.BindingTarget == target) return true;
		}
		return false;
	}


	public void ClearAllEmptyLayers () {
		for (int i = 0; i < KeyLayers.Count; i++) {
			var layer = KeyLayers[i];
			if (layer.BindingTarget < 0 && layer.BindingType < 0 && layer.KeyFrames.Count == 0) {
				KeyLayers.RemoveAt(i);
				i--;
			}
		}
	}


	public static bool IsValidPair (BindingType type, BindingTarget target) => target >= 0 && type >= 0 && RANGE_MAP[(int)target, (int)type] != (0, 0);


	public static (int min, int max) GetValidRange (BindingType type, BindingTarget target) => target >= 0 && type >= 0 ? RANGE_MAP[(int)target, (int)type] : default;


	public static int GetAdjustStep (BindingType type) => type >= 0 ? STEP_MAP[(int)type] : default;


	#endregion




	#region --- LGC ---


	private void Key_to_Raw () {
		if (KeyLayers == null) {
			RawLayers = new RawLayer[0];
			return;
		}
		// Fill Raw Layers
		RawLayers = new RawLayer[KeyLayers.Count].FillWithNewValue();
		for (int layerIndex = 0; layerIndex < RawLayers.Length; layerIndex++) {
			var sourceLayer = KeyLayers[layerIndex];
			var sourceKeyFrames = sourceLayer.KeyFrames;
			var rawLayer = RawLayers[layerIndex];
			rawLayer.BindingType = sourceLayer.BindingType;
			rawLayer.BindingTarget = sourceLayer.BindingTarget;
			rawLayer.FlipAngleFromCharacterFacing = sourceLayer.FlipAngleFromCharacterFacing;
			if (sourceKeyFrames == null || sourceKeyFrames.Count == 0) {
				rawLayer.RawData = new int[0];
				continue;
			}
			int rawFrameCount = sourceKeyFrames[^1].Frame - 1;
			rawFrameCount = rawFrameCount.Clamp(1, MAX_RAW_LENGTH);
			rawLayer.RawData = new int[rawFrameCount];
			int cacheKeyFrame = 0;
			for (int f = 0; f < rawFrameCount; f++) {
				// Source >> Raw
				rawLayer.RawData[f] = sourceLayer.Evaluate(f, cacheKeyFrame, out int newKeyFrame);
				cacheKeyFrame = (newKeyFrame - 1).GreaterOrEquelThanZero();
			}
		}
	}


	private void PerformFrame (PoseCharacter character, BindingType bindingType, BindingTarget bindingTarget, int value, bool requireFlip) {

		// Gate
		int targetIndex = (int)bindingTarget;
		int typeIndex = (int)bindingType;
		if (
			targetIndex < 0 || targetIndex >= RANGE_MAP.GetLength(0) ||
			typeIndex < 0 || typeIndex >= RANGE_MAP.GetLength(1) ||
			RANGE_MAP[(int)bindingTarget, (int)bindingType] == (0, 0)
		) return;

		bool flipLimb = FlipLimbsFromCharacterFacing && !character.FacingRight;

		(var bodypart, bool facingRight) = bindingTarget switch {
			BindingTarget.Head => (character.Head, character.Head.FacingRight),
			BindingTarget.Body => (character.Body, character.Body.FacingRight),
			BindingTarget.Hip => (character.Hip, character.Hip.FacingRight),
			BindingTarget.ShoulderL => (flipLimb ? character.ShoulderR : character.ShoulderL, character.FacingRight),
			BindingTarget.ShoulderR => (flipLimb ? character.ShoulderL : character.ShoulderR, character.FacingRight),
			BindingTarget.UpperArmL => (flipLimb ? character.UpperArmR : character.UpperArmL, character.FacingRight),
			BindingTarget.UpperArmR => (flipLimb ? character.UpperArmL : character.UpperArmR, character.FacingRight),
			BindingTarget.LowerArmL => (flipLimb ? character.LowerArmR : character.LowerArmL, character.FacingRight),
			BindingTarget.LowerArmR => (flipLimb ? character.LowerArmL : character.LowerArmR, character.FacingRight),
			BindingTarget.HandL => (flipLimb ? character.HandR : character.HandL, character.FacingRight),
			BindingTarget.HandR => (flipLimb ? character.HandL : character.HandR, character.FacingRight),
			BindingTarget.UpperLegL => (flipLimb ? character.UpperLegR : character.UpperLegL, character.FacingRight),
			BindingTarget.UpperLegR => (flipLimb ? character.UpperLegL : character.UpperLegR, character.FacingRight),
			BindingTarget.LowerLegL => (flipLimb ? character.LowerLegR : character.LowerLegL, character.FacingRight),
			BindingTarget.LowerLegR => (flipLimb ? character.LowerLegL : character.LowerLegR, character.FacingRight),
			BindingTarget.FootL => (flipLimb ? character.FootR : character.FootL, character.FacingRight),
			BindingTarget.FootR => (flipLimb ? character.FootL : character.FootR, character.FacingRight),
			_ => (character.Head, character.Head.FacingRight),
		};

		int flippedValue = !requireFlip || facingRight ? value : -value;

		switch (bindingType) {
			default:
			case BindingType.Rotation:
				bodypart.Rotation = flippedValue;
				break;
			case BindingType.Twist:
				if (bodypart == character.Head) {
					character.HeadTwist = flippedValue;
				} else if (bodypart == character.Body) {
					character.BodyTwist = flippedValue;
				}
				break;
			case BindingType.GrabRot:
				if (bodypart == character.HandL) {
					character.HandGrabRotationL = flippedValue;
				} else if (bodypart == character.HandR) {
					character.HandGrabRotationR = flippedValue;
				}
				break;
			case BindingType.GrabScale:
				if (bodypart == character.HandL) {
					character.HandGrabScaleL = flippedValue;
				} else if (bodypart == character.HandR) {
					character.HandGrabScaleR = flippedValue;
				}
				break;
			case BindingType.GrabTwist:
				if (bodypart == character.HandL) {
					character.HandGrabAttackTwistL = flippedValue;
				} else if (bodypart == character.HandR) {
					character.HandGrabAttackTwistR = flippedValue;
				}
				break;
			case BindingType.Width:
				if (bodypart == character.Head || bodypart == character.Body || bodypart == character.Hip) {
					bodypart.Width += flippedValue;
				} else {
					bodypart.Width += bodypart.Width.Sign() * value;
				}
				break;
			case BindingType.Height:
				bodypart.Height += value;
				break;
			case BindingType.X:
				if (bodypart == character.Hip) {
					character.PoseRootX += flippedValue;
				} else {
					bodypart.X += flippedValue;
				}
				break;
			case BindingType.Y:
				if (bodypart == character.Hip) {
					character.PoseRootY += value;
				} else {
					bodypart.Y += value;
				}
				break;
		}

	}


	private void ApplyLimbRotate (BodyPart bodypart) {
		var parent = bodypart.LimbParent;
		if (parent != null) {
			Util.LimbRotate(
				ref bodypart.X, ref bodypart.Y, ref bodypart.PivotX, ref bodypart.PivotY,
				ref bodypart.Rotation, ref bodypart.Width, ref bodypart.Height,
				parent.X, parent.Y, parent.Rotation, parent.Width, parent.Height,
				bodypart.Rotation, bodypart.UseLimbFlip, 1000
			);
		} else {
			Util.LimbRotate(
				ref bodypart.X, ref bodypart.Y, ref bodypart.PivotX, ref bodypart.PivotY,
				ref bodypart.Rotation, ref bodypart.Width, ref bodypart.Height,
				bodypart.Rotation, bodypart.UseLimbFlip, 1000
			);
		}
	}


	#endregion




}
