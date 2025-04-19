using System.Collections.Generic;
using System.Collections;

namespace AngeliA;

/// <summary>
/// Complete pose infomation at one moment for a pose-style character
/// </summary>
public class CharacterPose {

	// Api
	private readonly BodyPart[] BodyParts = new BodyPart[BodyPart.BODY_PART_COUNT];

	public BodyPart Head { get; init; } = null;
	public BodyPart Body { get; init; } = null;
	public BodyPart Hip { get; init; } = null;
	public BodyPart ShoulderL { get; init; } = null;
	public BodyPart ShoulderR { get; init; } = null;
	public BodyPart UpperArmL { get; init; } = null;
	public BodyPart UpperArmR { get; init; } = null;
	public BodyPart LowerArmL { get; init; } = null;
	public BodyPart LowerArmR { get; init; } = null;
	public BodyPart HandL { get; init; } = null;
	public BodyPart HandR { get; init; } = null;
	public BodyPart UpperLegL { get; init; } = null;
	public BodyPart UpperLegR { get; init; } = null;
	public BodyPart LowerLegL { get; init; } = null;
	public BodyPart LowerLegR { get; init; } = null;
	public BodyPart FootL { get; init; } = null;
	public BodyPart FootR { get; init; } = null;

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
			BodyParts[i] = new BodyPart(
				parent: (i >= 7 && i < 11) || (i >= 13 && i < 17) ? BodyParts[i - 2] : null,
				useLimbFlip: i == 9 || i == 10 || i == 15 || i == 16,
				rotateWithBody: i != 2 && i != 1 && i < 11,
				defaultPivotX: BodyPart.BODY_DEF_PIVOT[i].x,
				defaultPivotY: BodyPart.BODY_DEF_PIVOT[i].y
			);
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

	/// <summary>
	/// Make the character perform this pose
	/// </summary>
	public void ApplyToCharacter (PoseCharacterRenderer rendering) {
		for (int i = 0; i < BodyParts.Length; i++) {
			var record = BodyParts[i];
			var pose = rendering.BodyParts[i];
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
		rendering.PoseRootX = PoseRootX;
		rendering.PoseRootY = PoseRootY;
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
			record.Width = pose.Width;
			record.Height = pose.Height;
			record.PivotX = pose.PivotX;
			record.PivotY = pose.PivotY;
			record.FrontSide = pose.FrontSide;
			record.Covered = pose.Covered;
			record.Tint = pose.Tint;
		}
		PoseRootX = rendering.PoseRootX;
		PoseRootY = rendering.PoseRootY;
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
	/// Make the character perform this pose with weight
	/// </summary>
	public void BlendToCharacter (PoseCharacterRenderer rendering, float blend01) {

		blend01 = 1f - blend01;

		for (int i = 0; i < BodyParts.Length; i++) {
			var record = BodyParts[i];
			var pose = rendering.BodyParts[i];

			pose.Width = FixSign(pose.Width, record.Width);
			pose.Height = FixSign(pose.Height, record.Height);

			pose.Z = Util.LerpUnclamped(pose.Z, record.Z, blend01).RoundToInt();
			pose.Rotation = Util.LerpAngleUnclamped(pose.Rotation, record.Rotation, blend01).RoundToInt();
			pose.Width = Util.LerpUnclamped(pose.Width, record.Width, blend01).RoundToInt();
			pose.Height = Util.LerpUnclamped(pose.Height, record.Height, blend01).RoundToInt();
			pose.Tint = Color32.Lerp(pose.Tint, record.Tint, blend01);

			pose.PivotX = Util.LerpUnclamped(pose.PivotX, record.PivotX, blend01).RoundToInt();
			pose.PivotY = Util.LerpUnclamped(pose.PivotY, record.PivotY, blend01).RoundToInt();
			pose.X = Util.LerpUnclamped(pose.X, record.X, blend01).RoundToInt();
			pose.Y = Util.LerpUnclamped(pose.Y, record.Y, blend01).RoundToInt();

		}

		rendering.HandGrabScaleL.Override(FixSign(rendering.HandGrabScaleL, HandGrabScaleL));
		rendering.HandGrabScaleR.Override(FixSign(rendering.HandGrabScaleR, HandGrabScaleR));

		rendering.PoseRootX = (int)Util.LerpUnclamped(rendering.PoseRootX, PoseRootX, blend01);
		rendering.PoseRootY = (int)Util.LerpUnclamped(rendering.PoseRootY, PoseRootY, blend01);
		rendering.BodyTwist = (int)Util.LerpUnclamped(rendering.BodyTwist, BodyTwist, blend01);
		rendering.HeadTwist = (int)Util.LerpUnclamped(rendering.HeadTwist, HeadTwist, blend01);
		
		// Func
		static int FixSign (int basicValue, int targetValue) {
			if (basicValue.Sign() != targetValue.Sign()) {
				basicValue = -basicValue;
			}
			return basicValue;
		}
	}

}
