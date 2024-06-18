using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AngeliA;

[System.Serializable]
public sealed class ModularAnimation : PoseAnimation, IJsonSerializationCallback {




	#region --- SUB ---


	public enum CharacterOverrideType { None, Pose, Handheld, Attack, }


	public enum Binding {

		Rot_Head, Rot_UpperArmL, Rot_UpperArmR, Rot_LowerArmL, Rot_LowerArmR, Rot_UpperLegL, Rot_UpperLegR, Rot_LowerLegL, Rot_LowerLegR,

		Twist_Head, Twist_Body,

		Grab_RotL, Grab_RotR, Grab_ScaleL, Grab_ScaleR, Grab_TwistL, Grab_TwistR,

		Width_Head, Width_Body, Width_ShoulderL, Width_ShoulderR, Width_UpperArmL, Width_UpperArmR, Width_LowerArmL, Width_LowerArmR, Width_HandL, Width_HandR, Width_UpperLegL, Width_UpperLegR, Width_LowerLegL, Width_LowerLegR, Width_FootL, Width_FootR,

		Height_Head, Height_Body, Height_ShoulderL, Height_ShoulderR, Height_UpperArmL, Height_UpperArmR, Height_LowerArmL, Height_LowerArmR, Height_HandL, Height_HandR, Height_UpperLegL, Height_UpperLegR, Height_LowerLegL, Height_LowerLegR, Height_FootL, Height_FootR,

	}


	[System.Serializable]
	public class KeyFrame {
		public int Frame;
		public int Value;
		public EaseType Ease;
		public KeyFrame () { }
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

		public Binding Binding;
		public KeyFrame[] KeyFrames;

		public void Sort () {
			if (KeyFrames == null) return;
			System.Array.Sort(KeyFrames, KeyFrameComparer.Instance);
		}


		public int Evaluate (int frame) => Evaluate(frame, 0, out _);
		internal int Evaluate (int frame, int fromKeyFrame, out int keyFrame) {
			keyFrame = 0;
			if (KeyFrames.Length == 0) return 0;
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
			return Util.RemapUnclamped(left.Frame, right.Frame, left.Value, right.Value, frame);
		}

	}


	private class RawLayer {
		public Binding Binding;
		public int[] RawData;
	}


	#endregion




	#region --- VAR ---


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
	public KeyLayer[] KeyLayers;

	// Data
	[JsonIgnore] RawLayer[] RawLayers;
	[JsonIgnore] int Duration = 1;


	#endregion




	#region --- MSG ---


	public override void Animate (PoseCharacter character) {
		int frame = Override switch {
			CharacterOverrideType.Attack => Game.GlobalFrame - character.LastAttackFrame,
			CharacterOverrideType.Pose => character.CurrentAnimationFrame,
			CharacterOverrideType.Handheld => character.IsChargingAttack ? GetFrameForCharging(character) : character.CurrentAnimationFrame,
			_ => character.CurrentAnimationFrame,
		};
		if (UseRawData) {
			AnimateFromRawData(character, frame);
		} else {
			AnimateFromKeyFrame(character, frame);
		}
	}


	private void AnimateFromKeyFrame (PoseCharacter character, int frame) {
		if (KeyLayers == null || KeyLayers.Length == 0) return;
		base.Animate(character);
		for (int i = 0; i < KeyLayers.Length; i++) {
			var layer = KeyLayers[i];
			int value = layer.Evaluate(frame.UMod(Duration));
			PerformFrame(character, layer.Binding, value);
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
			PerformFrame(character, layer.Binding, layer.RawData[_frame]);
		};
	}


	public void OnBeforeSaveToDisk () {
		if (KeyLayers == null) return;
		foreach (var layer in KeyLayers) {
			layer.Sort();
		}
	}


	public void OnAfterLoadedFromDisk () {
		if (KeyLayers == null) return;
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
			rawLayer.Binding = sourceLayer.Binding;
			if (sourceKeyFrames == null || sourceKeyFrames.Length == 0) {
				rawLayer.RawData = new int[0];
				continue;
			}
			int rawFrameCount = sourceKeyFrames[^1].Frame - 1;
			rawFrameCount = rawFrameCount.Clamp(1, 600);
			rawLayer.RawData = new int[rawFrameCount];
			int cacheKeyFrame = 0;
			for (int f = 0; f < rawFrameCount; f++) {
				// Source >> Raw
				rawLayer.RawData[f] = sourceLayer.Evaluate(f, cacheKeyFrame, out int newKeyFrame);
				cacheKeyFrame = (newKeyFrame - 1).GreaterOrEquelThanZero();
			}
		}
	}


	private void PerformFrame (PoseCharacter character, Binding binding, int data) {
		switch (binding) {

			// Rot
			case Binding.Rot_Head:
				character.Head.Rotation = data;
				break;
			case Binding.Rot_UpperArmL:
				character.UpperArmL.Rotation = data;
				break;
			case Binding.Rot_UpperArmR:
				character.UpperArmR.Rotation = data;
				break;
			case Binding.Rot_LowerArmL:
				character.LowerArmL.Rotation = data;
				break;
			case Binding.Rot_LowerArmR:
				character.LowerArmR.Rotation = data;
				break;
			case Binding.Rot_UpperLegL:
				character.UpperLegL.Rotation = data;
				break;
			case Binding.Rot_UpperLegR:
				character.UpperLegR.Rotation = data;
				break;
			case Binding.Rot_LowerLegL:
				character.LowerLegL.Rotation = data;
				break;
			case Binding.Rot_LowerLegR:
				character.LowerLegR.Rotation = data;
				break;

			// Twist
			case Binding.Twist_Head:
				character.HeadTwist = data;
				break;
			case Binding.Twist_Body:
				character.BodyTwist = data;
				break;

			// Grab
			case Binding.Grab_RotL:
				character.HandGrabRotationL = data;
				break;
			case Binding.Grab_RotR:
				character.HandGrabRotationR = data;
				break;
			case Binding.Grab_ScaleL:
				character.HandGrabScaleL = data;
				break;
			case Binding.Grab_ScaleR:
				character.HandGrabScaleR = data;
				break;
			case Binding.Grab_TwistL:
				character.HandGrabAttackTwistL = data;
				break;
			case Binding.Grab_TwistR:
				character.HandGrabAttackTwistR = data;
				break;

			// Width
			case Binding.Width_Head:
				character.Head.Width = character.Head.SizeX + data;
				break;
			case Binding.Width_Body:
				character.Body.Width = character.Body.SizeX + data;
				break;
			case Binding.Width_ShoulderL:
				character.ShoulderL.Width = character.ShoulderL.SizeX + data;
				break;
			case Binding.Width_ShoulderR:
				character.ShoulderR.Width = character.ShoulderR.SizeX + data;
				break;
			case Binding.Width_UpperArmL:
				character.UpperArmL.Width = character.UpperArmL.SizeX + data;
				break;
			case Binding.Width_UpperArmR:
				character.UpperArmR.Width = character.UpperArmR.SizeX + data;
				break;
			case Binding.Width_LowerArmL:
				character.LowerArmL.Width = character.LowerArmL.SizeX + data;
				break;
			case Binding.Width_LowerArmR:
				character.LowerArmR.Width = character.LowerArmR.SizeX + data;
				break;
			case Binding.Width_HandL:
				character.HandL.Width = character.HandL.SizeX + data;
				break;
			case Binding.Width_HandR:
				character.HandR.Width = character.HandR.SizeX + data;
				break;
			case Binding.Width_UpperLegL:
				character.UpperLegL.Width = character.UpperLegL.SizeX + data;
				break;
			case Binding.Width_UpperLegR:
				character.UpperLegR.Width = character.UpperLegR.SizeX + data;
				break;
			case Binding.Width_LowerLegL:
				character.LowerLegL.Width = character.LowerLegL.SizeX + data;
				break;
			case Binding.Width_LowerLegR:
				character.LowerLegR.Width = character.LowerLegR.SizeX + data;
				break;
			case Binding.Width_FootL:
				character.FootL.Width = character.FootL.SizeX + data;
				break;
			case Binding.Width_FootR:
				character.FootR.Width = character.FootR.SizeX + data;
				break;

			// Height
			case Binding.Height_Head:
				character.Head.Height = character.Head.FlexableSizeY + data;
				break;
			case Binding.Height_Body:
				character.Body.Height = character.Body.FlexableSizeY + data;
				break;
			case Binding.Height_ShoulderL:
				character.ShoulderL.Height = character.ShoulderL.FlexableSizeY + data;
				break;
			case Binding.Height_ShoulderR:
				character.ShoulderR.Height = character.ShoulderR.FlexableSizeY + data;
				break;
			case Binding.Height_UpperArmL:
				character.UpperArmL.Height = character.UpperArmL.FlexableSizeY + data;
				break;
			case Binding.Height_UpperArmR:
				character.UpperArmR.Height = character.UpperArmR.FlexableSizeY + data;
				break;
			case Binding.Height_LowerArmL:
				character.LowerArmL.Height = character.LowerArmL.FlexableSizeY + data;
				break;
			case Binding.Height_LowerArmR:
				character.LowerArmR.Height = character.LowerArmR.FlexableSizeY + data;
				break;
			case Binding.Height_HandL:
				character.HandL.Height = character.HandL.FlexableSizeY + data;
				break;
			case Binding.Height_HandR:
				character.HandR.Height = character.HandR.FlexableSizeY + data;
				break;
			case Binding.Height_UpperLegL:
				character.UpperLegL.Height = character.UpperLegL.FlexableSizeY + data;
				break;
			case Binding.Height_UpperLegR:
				character.UpperLegR.Height = character.UpperLegR.FlexableSizeY + data;
				break;
			case Binding.Height_LowerLegL:
				character.LowerLegL.Height = character.LowerLegL.FlexableSizeY + data;
				break;
			case Binding.Height_LowerLegR:
				character.LowerLegR.Height = character.LowerLegR.FlexableSizeY + data;
				break;
			case Binding.Height_FootL:
				character.FootL.Height = character.FootL.FlexableSizeY + data;
				break;
			case Binding.Height_FootR:
				character.FootR.Height = character.FootR.FlexableSizeY + data;
				break;
		}
	}


	private int GetFrameForCharging (PoseCharacter character) {
		float lerp01 = ((float)(Game.GlobalFrame - character.AttackChargeStartFrame.Value) / Util.Max(character.MinimalChargeAttackDuration * 2, 1)).Clamp01();
		return (lerp01 * Duration).RoundToInt();
	}


	#endregion




}
