﻿using System.Collections.Generic;
using System.Collections;
using System.Text.Json.Serialization;

namespace AngeliA;

/// <summary>
/// Complete pose infomation at one moment for a pose-style character
/// </summary>
internal class CharacterPose {

	// Api
	private readonly BodyPartTransform[] BodyParts = new BodyPartTransform[BodyPart.BODY_PART_COUNT];

	public BodyPartTransform Head { get; init; } = null;
	public BodyPartTransform Body { get; init; } = null;
	public BodyPartTransform Hip { get; init; } = null;
	public BodyPartTransform ShoulderL { get; init; } = null;
	public BodyPartTransform ShoulderR { get; init; } = null;
	public BodyPartTransform UpperArmL { get; init; } = null;
	public BodyPartTransform UpperArmR { get; init; } = null;
	public BodyPartTransform LowerArmL { get; init; } = null;
	public BodyPartTransform LowerArmR { get; init; } = null;
	public BodyPartTransform HandL { get; init; } = null;
	public BodyPartTransform HandR { get; init; } = null;
	public BodyPartTransform UpperLegL { get; init; } = null;
	public BodyPartTransform UpperLegR { get; init; } = null;
	public BodyPartTransform LowerLegL { get; init; } = null;
	public BodyPartTransform LowerLegR { get; init; } = null;
	public BodyPartTransform FootL { get; init; } = null;
	public BodyPartTransform FootR { get; init; } = null;

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

	// API
	public CharacterPose () {
		for (int i = 0; i < BodyParts.Length; i++) {
			BodyParts[i] = new BodyPartTransform();
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
	}

	public CharacterPose (PoseCharacterRenderer source) : this() {
		RecordFromCharacter(source);
	}

	/// <summary>
	/// Read pose data from the character
	/// </summary>
	public void RecordFromCharacter (PoseCharacterRenderer rendering) {
		for (int i = 0; i < BodyParts.Length; i++) {
			var record = BodyParts[i];
			var pose = rendering.BodyParts[i];
			record.X = pose.X;
			record.Y = pose.Y;
			record.Z = pose.Z;
			record.Rotation = pose.Rotation;
			record.SizeX = pose.SizeX * pose.Width.Sign3();
			record.SizeY = pose.SizeY * pose.Height.Sign3();
			record.Width = pose.Width.Sign3() * (pose.Width.Abs() - pose.SizeX);
			record.Height = pose.Height.Sign3() * (pose.Height.Abs() - pose.SizeY);
			record.PivotX = pose.PivotX;
			record.PivotY = pose.PivotY;
			record.FrontSide = pose.FrontSide;
		}
		PoseRootX = rendering.PoseRootX;
		PoseRootY = rendering.PoseRootY - rendering.BasicRootY;
		BodyTwist = rendering.BodyTwist;
		HeadTwist = rendering.HeadTwist;
		HandGrabRotationL = rendering.HandGrabRotationL;
		HandGrabRotationR = rendering.HandGrabRotationR;
		HandGrabScaleL = rendering.HandGrabScaleL;
		HandGrabScaleR = rendering.HandGrabScaleR;
		HandGrabAttackTwistL = rendering.HandGrabAttackTwistL;
		HandGrabAttackTwistR = rendering.HandGrabAttackTwistR;
	}

	/// <summary>
	/// Make the character perform this pose
	/// </summary>
	public void ApplyToCharacter (PoseCharacterRenderer rendering) {
		for (int i = 0; i < BodyParts.Length; i++) {
			var record = BodyParts[i];
			var pose = rendering.BodyParts[i];
			pose.Z = record.Z;
			pose.FrontSide = record.FrontSide;
			pose.X = record.X;
			pose.Y = record.Y;
			pose.Rotation = record.Rotation;
			pose.Width = record.Width + record.SizeX.Sign3() * pose.SizeX;
			pose.Height = record.Height + record.SizeY.Sign3() * pose.SizeY;
			pose.PivotX = record.PivotX;
			pose.PivotY = record.PivotY;
		}
		rendering.PoseRootX = PoseRootX;
		rendering.PoseRootY = rendering.BasicRootY + PoseRootY;
		rendering.BodyTwist = BodyTwist;
		rendering.HeadTwist = HeadTwist;
		rendering.HandGrabRotationL.Override(HandGrabRotationL);
		rendering.HandGrabRotationR.Override(HandGrabRotationR);
		rendering.HandGrabScaleL.Override(HandGrabScaleL);
		rendering.HandGrabScaleR.Override(HandGrabScaleR);
		rendering.HandGrabAttackTwistL.Override(HandGrabAttackTwistL);
		rendering.HandGrabAttackTwistR.Override(HandGrabAttackTwistR);
	}

	/// <summary>
	/// Make the character perform this pose with weight
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="blend01">1 means this apply pose. 0 means apply character's current pose.</param>
	public void BlendToCharacter (PoseCharacterRenderer rendering, float blend01) {

		for (int i = 0; i < BodyParts.Length; i++) {
			var record = BodyParts[i];
			var pose = rendering.BodyParts[i];

			pose.Z = Util.LerpUnclamped(pose.Z, record.Z, blend01).RoundToInt();
			pose.Rotation = Util.LerpAngleUnclamped(pose.Rotation, record.Rotation, blend01).RoundToInt();

			int targetW = record.Width + record.SizeX.Sign3() * pose.SizeX;
			pose.Width = FixSign(pose.Width, targetW);
			pose.Width = Util.LerpUnclamped(pose.Width, targetW, blend01).RoundToInt();

			int targetH = record.Height + record.SizeY.Sign3() * pose.SizeY;
			pose.Height = FixSign(pose.Height, targetH);
			pose.Height = Util.LerpUnclamped(pose.Height, targetH, blend01).RoundToInt();

			pose.PivotX = Util.LerpUnclamped(pose.PivotX, record.PivotX, blend01).RoundToInt();
			pose.PivotY = Util.LerpUnclamped(pose.PivotY, record.PivotY, blend01).RoundToInt();
			pose.X = Util.LerpUnclamped(pose.X, record.X, blend01).RoundToInt();
			pose.Y = Util.LerpUnclamped(pose.Y, record.Y, blend01).RoundToInt();

		}

		rendering.HandGrabRotationL.Override(
			(int)Util.LerpAngleUnclamped(rendering.HandGrabRotationL, HandGrabRotationL, blend01)
		);
		rendering.HandGrabRotationR.Override(
			(int)Util.LerpAngleUnclamped(rendering.HandGrabRotationR, HandGrabRotationR, blend01)
		);

		rendering.HandGrabScaleL.Override(
			(int)Util.LerpUnclamped(rendering.HandGrabScaleL, HandGrabScaleL, blend01)
		);
		rendering.HandGrabScaleR.Override(
			(int)Util.LerpUnclamped(rendering.HandGrabScaleR, HandGrabScaleR, blend01)
		);

		rendering.HandGrabAttackTwistL.Override(
			(int)Util.LerpUnclamped(rendering.HandGrabAttackTwistL, HandGrabAttackTwistL, blend01)
		);
		rendering.HandGrabAttackTwistR.Override(
			(int)Util.LerpUnclamped(rendering.HandGrabAttackTwistR, HandGrabAttackTwistR, blend01)
		);

		rendering.PoseRootX = (int)Util.LerpUnclamped(rendering.PoseRootX, PoseRootX, blend01);
		rendering.PoseRootY = (int)Util.LerpUnclamped(rendering.PoseRootY, rendering.BasicRootY + PoseRootY, blend01);
		rendering.BodyTwist = (int)Util.LerpUnclamped(rendering.BodyTwist, BodyTwist, blend01);
		rendering.HeadTwist = (int)Util.LerpUnclamped(rendering.HeadTwist, HeadTwist, blend01);

	}

	// LGC
	private static int FixSign (int basicValue, int targetValue) {
		if (basicValue.Sign() != targetValue.Sign()) {
			basicValue = -basicValue;
		}
		return basicValue;
	}

}
