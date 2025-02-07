namespace AngeliA;

public class PoseAnimation_Rolling : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Rolling).AngeHash();

	private static readonly int[,] ROLLING = { { 1450, +100, -000, 0900, 0500, -020, -025, -015, -040, 70, 80, }, { 1200, +450, -000, 0800, 0250, +025, +030, -025, -030, 75, 85, }, { 0850, +800, -000, -800, -100, -160, -150, -145, -125, 80, 90, }, { 0300, +450, -000, -750, -200, -170, -160, -155, -115, 80, 90, }, { 0650, -100, +000, -750, -200, -170, -160, -155, -115, 80, 90, }, { 0850, -450, +000, -800, -100, -160, -150, -145, -125, 80, 90, }, { 0950, -800, +000, 0800, 0250, -065, -065, -025, -030, 75, 85, }, { 1200, -450, +000, 0900, 0750, -040, -045, -015, -040, 70, 80, }, };
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int rawFrame24 = CurrentAnimationFrame.UMod(24);
		int arrFrame = rawFrame24 / 3; // 01234567
		int upsideSign = arrFrame < 2 || arrFrame > 5 ? 1 : -1;

		Rendering.PoseRootY = Rendering.PoseRootY * ROLLING[arrFrame, 0] / 1500;

		Head.FrontSide = Body.FrontSide = arrFrame < 2 || arrFrame > 5;

		Body.Width = Hip.Width = Body.Width * 2 / 3;
		Head.Height = Head.SizeY * ROLLING[arrFrame, 3] / 1000;
		Body.Height = Body.SizeY * ROLLING[arrFrame, 4] / 1500;

		Head.X = FacingSign * A2G * ROLLING[arrFrame, 1] / 250;
		Body.X = FacingSign * A2G * ROLLING[arrFrame, 2] / 300;
		Head.Y = upsideSign > 0 ? 0 : Head.SizeY;
		Hip.Y = Head.Y - Body.Height;
		Body.Y = Hip.Y + Hip.Height;

		// Arm
		ShoulderL.X = Body.X - Body.Width / 2 + Body.Border.left;
		ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderL.Height /= 2;

		ShoulderR.X = Body.X + Body.Width / 2 - Body.Border.right;
		ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderR.Height /= 2;

		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmL.Z = upsideSign * UpperArmL.Z.Abs();
		UpperArmL.LimbRotate(FacingSign * ROLLING[arrFrame, 5]);
		UpperArmL.Height /= 2;

		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmR.Z = upsideSign * UpperArmR.Z.Abs();
		UpperArmR.LimbRotate(FacingSign * ROLLING[arrFrame, 6]);
		UpperArmR.Height /= 2;

		LowerArmL.LimbRotate(0);
		LowerArmL.Z = upsideSign * LowerArmL.Z.Abs();
		LowerArmL.Height /= 2;

		LowerArmR.LimbRotate(0);
		LowerArmR.Z = upsideSign * LowerArmR.Z.Abs();
		LowerArmR.Height /= 2;

		HandL.LimbRotate(FacingSign * upsideSign * 1);
		HandL.Z = upsideSign * POSE_Z_HAND;

		HandR.LimbRotate(FacingSign * upsideSign * 1);
		HandR.Z = upsideSign * POSE_Z_HAND;

		// Leg
		UpperLegL.X = Body.X - Body.Width.Abs() / 2 + Body.Border.left - FacingSign * A2G * 2;
		UpperLegL.Y = Body.Y;
		UpperLegL.LimbRotate(FacingSign * ROLLING[arrFrame, 7], 0);

		UpperLegR.X = Body.X + Body.Width.Abs() / 2 - Body.Border.right - FacingSign * A2G * 2;
		UpperLegR.Y = Body.Y;
		UpperLegR.LimbRotate(FacingSign * ROLLING[arrFrame, 8], 0);

		LowerLegL.LimbRotate(FacingSign * ROLLING[arrFrame, 9], 0);
		LowerLegR.LimbRotate(FacingSign * ROLLING[arrFrame, 10], 0);

		FootL.LimbRotate(FacingSign * upsideSign * -1);
		FootR.LimbRotate(FacingSign * upsideSign * -1);

		if (upsideSign < 0) {
			UpperLegL.PivotX = 1000 - UpperLegL.PivotX;
			UpperLegR.PivotX = 1000 - UpperLegR.PivotX;
		}

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation + Body.Height.Sign() * FacingSign * 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + Body.Height.Sign() * FacingSign * 90;
	}
}
