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
		public bool FlipFacing = false;
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
		public bool FlipFacing;
		public int[] RawData;
	}


	#endregion




	#region --- VAR ---



	// Const
	public const int MAX_RAW_LENGTH = 600;
	private static readonly bool[,] VALID_MAP = {// [ Target-17, Type-9 ]
						// Rot,   Twist, GrabR, GrabS, GrabT, W,     H,     X,     Y,
		/* Head */		{  true , true , false, false, false, true , true , true , true , },
		/* Body */		{  false, true , false, false, false, true , true , true , true , },
		/* Hip */		{  false, false, false, false, false, true , true , true , true , },
		/* ShoulderL */	{  false, false, false, false, false, true , true , true , true , },
		/* ShoulderR */	{  false, false, false, false, false, true , true , true , true , },
		/* UpperArmL */	{  true , false, false, false, false, true , true , true , true , },
		/* UpperArmR */	{  true , false, false, false, false, true , true , true , true , },
		/* LowerArmL */	{  true , false, false, false, false, true , true , true , true , },
		/* LowerArmR */	{  true , false, false, false, false, true , true , true , true , },
		/* HandL */		{  false, false, true , true , true , true , true , true , true , },
		/* HandR */		{  false, false, true , true , true , true , true , true , true , },
		/* UpperLegL */	{  true , false, false, false, false, true , true , true , true , },
		/* UpperLegR */	{  true , false, false, false, false, true , true , true , true , },
		/* LowerLegL */	{  true , false, false, false, false, true , true , true , true , },
		/* LowerLegR */	{  true , false, false, false, false, true , true , true , true , },
		/* FootL */		{  false, false, false, false, false, true , true , true , true , },
		/* FootR */		{  false, false, false, false, false, true , true , true , true , },
	};

	// Api
	[JsonIgnore] public bool UseRawData => !Game.IsToolApplication;
	[JsonIgnore] public int Duration { get; private set; } = 1;

	// Ser
	[JsonIgnore] public int ID;
	[JsonIgnore] public string Name = string.Empty;
	[JsonIgnore] public CharacterOverrideType Override = CharacterOverrideType.None;
	public KeyLayer[] KeyLayers = System.Array.Empty<KeyLayer>();

	// Data
	[JsonIgnore] RawLayer[] RawLayers = System.Array.Empty<RawLayer>();
	[JsonIgnore] readonly Int2[] BodyPartPos = new Int2[PoseCharacter.BODY_PART_COUNT];


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
			AnimateFromRawData(character, frame);
		} else {
			AnimateFromKeyFrame(character, frame);
		}
		// Func
		static int GetFrameForCharging (PoseCharacter character, int duration) {
			float lerp01 = ((float)(Game.GlobalFrame - character.AttackChargeStartFrame.Value) / Util.Max(character.MinimalChargeAttackDuration * 2, 1)).Clamp01();
			return (lerp01 * duration).RoundToInt();
		}
	}


	private void AnimateFromKeyFrame (PoseCharacter character, int frame) {
		if (KeyLayers == null || KeyLayers.Length == 0) return;
		ResetLimbCache(character);
		for (int i = 0; i < KeyLayers.Length; i++) {
			var layer = KeyLayers[i];
			var value = layer.Evaluate(frame.UMod(Duration));
			PerformFrame(character, layer.BindingType, layer.BindingTarget, layer.FlipFacing, value);
		}
		ApplyForLimb(character);
	}


	private void AnimateFromRawData (PoseCharacter character, int frame) {
		if (RawLayers == null) Key_to_Raw();
		if (RawLayers == null || RawLayers.Length == 0) return;
		ResetLimbCache(character);
		for (int i = 0; i < RawLayers.Length; i++) {
			var layer = RawLayers[i];
			if (layer.RawData.Length == 0) continue;
			int _frame = frame.UMod(layer.RawData.Length);
			PerformFrame(character, layer.BindingType, layer.BindingTarget, layer.FlipFacing, layer.RawData[_frame]);
		};
		ApplyForLimb(character);
	}


	private void ResetLimbCache (PoseCharacter character) {
		for (int i = 0; i < BodyPartPos.Length; i++) {
			var part = character.BodyParts[i];
			BodyPartPos[i] = new Int2(part.X, part.Y);
		}
	}


	private void ApplyForLimb (PoseCharacter character) {
		// Make into Offset
		for (int i = 0; i < BodyPartPos.Length; i++) {
			var part = character.BodyParts[i];
			var old = BodyPartPos[i];
			BodyPartPos[i] = new(part.X - old.x, part.Y - old.y);
		}
		// Limb Animate
		character.ResetAllLimbsPosition();
		// Apply Offset
		for (int i = 0; i < BodyPartPos.Length; i++) {
			var part = character.BodyParts[i];
			var offset = BodyPartPos[i];
			part.X += offset.x;
			part.Y += offset.y;
			part.GlobalX = character.X + character.PoseRootX + part.X;
			part.GlobalY = character.Y + character.PoseRootY + part.Y;
		}
	}


	public void OnBeforeSaveToDisk () {
		if (KeyLayers == null) return;
		foreach (var layer in KeyLayers) {
			layer.Sort();
		}
	}


	public void OnAfterLoadedFromDisk () {
		KeyLayers ??= new KeyLayer[0];
		foreach (var layer in KeyLayers) {
			layer.Sort();
			layer.KeyFrames ??= new();
		}
		CalculateDuration();
		if (UseRawData) {
			Key_to_Raw();
		}
	}


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


	#endregion




	#region --- LGC ---


	private void Key_to_Raw () {
		if (KeyLayers == null) {
			RawLayers = new RawLayer[0];
			return;
		}
		// Fill Raw Layers
		RawLayers = new RawLayer[KeyLayers.Length].FillWithNewValue();
		for (int layerIndex = 0; layerIndex < RawLayers.Length; layerIndex++) {
			var sourceLayer = KeyLayers[layerIndex];
			var sourceKeyFrames = sourceLayer.KeyFrames;
			var rawLayer = RawLayers[layerIndex];
			rawLayer.BindingType = sourceLayer.BindingType;
			rawLayer.BindingTarget = sourceLayer.BindingTarget;
			rawLayer.FlipFacing = sourceLayer.FlipFacing;
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


	private void PerformFrame (PoseCharacter character, BindingType bindingType, BindingTarget bindingTarget, bool flipFacing, int value) {

		// Gate
		int targetIndex = (int)bindingTarget;
		int typeIndex = (int)bindingType;
		if (
			targetIndex < 0 || targetIndex >= VALID_MAP.GetLength(0) ||
			typeIndex < 0 || typeIndex >= VALID_MAP.GetLength(1) ||
			!VALID_MAP[(int)bindingTarget, (int)bindingType]
		) return;

		(var bodypart, bool facingRight) = bindingTarget switch {
			BindingTarget.Head => (character.Head, character.Head.FacingRight),
			BindingTarget.Body => (character.Body, character.Body.FacingRight),
			BindingTarget.Hip => (character.Hip, character.Hip.FacingRight),
			BindingTarget.ShoulderL => (character.ShoulderL, character.FacingRight),
			BindingTarget.ShoulderR => (character.ShoulderR, character.FacingRight),
			BindingTarget.UpperArmL => (character.UpperArmL, character.FacingRight),
			BindingTarget.UpperArmR => (character.UpperArmR, character.FacingRight),
			BindingTarget.LowerArmL => (character.LowerArmL, character.FacingRight),
			BindingTarget.LowerArmR => (character.LowerArmR, character.FacingRight),
			BindingTarget.HandL => (character.HandL, character.FacingRight),
			BindingTarget.HandR => (character.HandR, character.FacingRight),
			BindingTarget.UpperLegL => (character.UpperLegL, character.FacingRight),
			BindingTarget.UpperLegR => (character.UpperLegR, character.FacingRight),
			BindingTarget.LowerLegL => (character.LowerLegL, character.FacingRight),
			BindingTarget.LowerLegR => (character.LowerLegR, character.FacingRight),
			BindingTarget.FootL => (character.FootL, character.FacingRight),
			BindingTarget.FootR => (character.FootR, character.FacingRight),
			_ => (character.Head, character.Head.FacingRight),
		};

		int flippedValue = !flipFacing || facingRight ? value : -value;

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
				bodypart.Width += flippedValue;
				break;
			case BindingType.Height:
				bodypart.Height += value;
				break;
			case BindingType.X:
				bodypart.X += flippedValue;
				break;
			case BindingType.Y:
				bodypart.Y += value;
				break;
		}

	}


	#endregion




}
