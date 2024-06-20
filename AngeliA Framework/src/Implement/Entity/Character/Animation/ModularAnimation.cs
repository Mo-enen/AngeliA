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
	public struct FrameValue {
		[JsonPropertyName("v")] public int Value;
		[JsonPropertyName("f")] public bool FlipFacing;
		public FrameValue (int value, bool flipFacing) {
			Value = value;
			FlipFacing = flipFacing;
		}
	}


	[System.Serializable]
	public struct KeyFrame {
		[JsonPropertyName("f")] public int Frame;
		[JsonPropertyName("v")] public FrameValue Value;
		[JsonPropertyName("e")] public EaseType Ease;
		public KeyFrame (int frame, FrameValue value, EaseType ease = EaseType.Const) {
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
		public KeyFrame[] KeyFrames;

		public void Sort () {
			if (KeyFrames == null) return;
			System.Array.Sort(KeyFrames, KeyFrameComparer.Instance);
		}

		public FrameValue Evaluate (int frame) => Evaluate(frame, 0, out _);
		internal FrameValue Evaluate (int frame, int fromKeyFrame, out int keyFrame) {
			keyFrame = 0;
			if (KeyFrames.Length == 0) return default;
			if (KeyFrames.Length == 1) return KeyFrames[0].Value;
			int index = -1;
			for (int i = fromKeyFrame; i < KeyFrames.Length; i++) {
				var k = KeyFrames[i];
				if (k.Frame > frame) break;
				index = i;
			}
			if (index == -1) return KeyFrames[0].Value;
			keyFrame = KeyFrames.Length - 1;
			if (index == KeyFrames.Length - 1) return KeyFrames[^1].Value;
			keyFrame = index;
			var left = KeyFrames[index];
			var right = KeyFrames[index + 1];
			float lerp01 = Util.InverseLerpUnclamped(left.Frame, right.Frame, frame);
			lerp01 = Ease.Invoke(left.Ease, lerp01);
			int resultValue = Util.LerpUnclamped(
				left.Value.Value,
				right.Value.Value,
				lerp01
			).RoundToInt();
			return new FrameValue(resultValue, left.Value.FlipFacing);
		}

	}


	private class RawLayer {
		public BindingType BindingType;
		public BindingTarget BindingTarget;
		public FrameValue[] RawData;
	}


	#endregion




	#region --- VAR ---


	// Const
	public const int MAX_LENGTH = 600;

	// Api
	[JsonIgnore] public bool UseRawData => !Game.IsToolApplication;

	// Ser
	[JsonIgnore] public int ID;
	[JsonIgnore] public string Name = string.Empty;
	public string CharacterName = string.Empty;
	public CharacterOverrideType Override = CharacterOverrideType.None;
	public CharacterAnimationType PoseType = CharacterAnimationType.Idle;
	public WeaponHandheld Handheld = WeaponHandheld.SingleHanded;
	public WeaponType AttackType = WeaponType.Hand;
	public KeyLayer[] KeyLayers = System.Array.Empty<KeyLayer>();

	// Data
	[JsonIgnore] RawLayer[] RawLayers = System.Array.Empty<RawLayer>();
	[JsonIgnore] int Duration = 1;


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
		base.Animate(character);
		for (int i = 0; i < KeyLayers.Length; i++) {
			var layer = KeyLayers[i];
			var value = layer.Evaluate(frame.UMod(Duration));
			PerformFrame(character, layer.BindingType, layer.BindingTarget, value);
		}
	}


	private void AnimateFromRawData (PoseCharacter character, int frame) {
		if (RawLayers == null) Key_to_Raw();
		if (RawLayers == null || RawLayers.Length == 0) return;
		base.Animate(character);
		for (int i = 0; i < RawLayers.Length; i++) {
			var layer = RawLayers[i];
			if (layer.RawData.Length == 0) continue;
			int _frame = frame.UMod(layer.RawData.Length);
			PerformFrame(character, layer.BindingType, layer.BindingTarget, layer.RawData[_frame]);
		};
	}


	public void OnBeforeSaveToDisk () {
		if (KeyLayers == null) return;
		foreach (var layer in KeyLayers) {
			layer.Sort();
		}
	}


	public void OnAfterLoadedFromDisk () {
		KeyLayers ??= new KeyLayer[0];
		Duration = 1;
		foreach (var layer in KeyLayers) {
			layer.Sort();
			if (layer.KeyFrames.Length > 0) {
				Duration = Util.Max(Duration, layer.KeyFrames[^1].Frame - 1);
			}
		}
		if (UseRawData) {
			Key_to_Raw();
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
			if (sourceKeyFrames == null || sourceKeyFrames.Length == 0) {
				rawLayer.RawData = new FrameValue[0];
				continue;
			}
			int rawFrameCount = sourceKeyFrames[^1].Frame - 1;
			rawFrameCount = rawFrameCount.Clamp(1, MAX_LENGTH);
			rawLayer.RawData = new FrameValue[rawFrameCount];
			int cacheKeyFrame = 0;
			for (int f = 0; f < rawFrameCount; f++) {
				// Source >> Raw
				rawLayer.RawData[f] = sourceLayer.Evaluate(f, cacheKeyFrame, out int newKeyFrame);
				cacheKeyFrame = (newKeyFrame - 1).GreaterOrEquelThanZero();
			}
		}
	}


	private void PerformFrame (PoseCharacter character, BindingType bindingType, BindingTarget bindingTarget, FrameValue value) {

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

		int flippedValue = !value.FlipFacing || facingRight ? value.Value : -value.Value;

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
				bodypart.Height += value.Value;
				break;
			case BindingType.X:
				bodypart.X += flippedValue;
				break;
			case BindingType.Y:
				bodypart.Y += value.Value;
				break;
		}

	}


	#endregion




}
