namespace AngeliA;

public class PoseAttack_Hand : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Attackness.AttackStyleLoop = 2;
		switch (Attackness.AttackStyleIndex % Attackness.AttackStyleLoop) {
			case 0:
				Punch();
				break;
			case 1:
				Smash();
				break;
		}
	}
	private static void Punch () {

		int aFrame = (Game.GlobalFrame - Attackness.LastAttackFrame).UDivide(5);
		if (aFrame >= 4) return;

		Head.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;

		Body.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G / 2;
		Hip.Y -= A2G / 2;
		Body.Y -= A2G / 2;
		Body.Height = Head.Y - Body.Y;

		// Arm
		UpperArmL.PivotX = 0;
		UpperArmR.PivotX = 1000;


		ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.left;
		ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.right;
		ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderL.Height = Util.Min(ShoulderL.Height, Body.Height);
		ShoulderR.Height = Util.Min(ShoulderR.Height, Body.Height);
		ShoulderL.PivotX = 1000;
		ShoulderR.PivotX = 1000;

		UpperArmL.PivotX = 0;
		UpperArmR.PivotX = 1000;

		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmL.PivotX = 1000;
		UpperArmR.PivotX = 0;

		var uArmRest = FacingRight ? UpperArmL : UpperArmR;
		var lArmRest = FacingRight ? LowerArmL : LowerArmR;
		var handRest = FacingRight ? HandL : HandR;
		var uArmAtt = FacingRight ? UpperArmR : UpperArmL;
		var lArmAtt = FacingRight ? LowerArmR : LowerArmL;
		var handAtt = FacingRight ? HandR : HandL;

		uArmAtt.Z = lArmAtt.Z = handAtt.Z = aFrame == 0 ? -34 : 34;
		uArmRest.Z = lArmRest.Z = handRest.Z = 34;
		uArmRest.Height = uArmRest.SizeY;
		lArmRest.Height = lArmRest.SizeY;
		handRest.Height = handRest.SizeY;
		uArmAtt.Height = uArmAtt.SizeY;
		lArmAtt.Height = lArmAtt.SizeY;
		handAtt.Height = handAtt.SizeY;

		uArmRest.LimbRotate(FacingSign * -50, 1300);
		lArmRest.LimbRotate(FacingSign * -100);
		handRest.LimbRotate(-FacingSign);

		if (aFrame == 0) {
			uArmAtt.X -= FacingSign * uArmAtt.Height;
			uArmAtt.LimbRotate(FacingSign * -90, 0);
			lArmAtt.LimbRotate(0, 0);
			handAtt.Z = 34;
		} else if (aFrame == 1) {
			uArmAtt.X += FacingSign * A2G;
			uArmAtt.LimbRotate(FacingSign * -90, 500);
			lArmAtt.LimbRotate(0, 500);
			handAtt.Width += handAtt.Width.Sign() * A2G * 3 / 2;
			handAtt.Height += handAtt.Height.Sign() * A2G * 3 / 2;
		} else if (aFrame == 2) {
			uArmAtt.LimbRotate(FacingSign * -90, 500);
			lArmAtt.LimbRotate(0, 500);
			handAtt.Width += handAtt.Width.Sign() * A2G * 4 / 3;
			handAtt.Height += handAtt.Height.Sign() * A2G * 4 / 3;
		} else {
			uArmAtt.LimbRotate(FacingSign * -35, 800);
			lArmAtt.LimbRotate(-15, 1000);
		}

		handAtt.LimbRotate(-FacingSign);

		// Leg
		if (AnimationType == CharacterAnimationType.Idle) {
			if (aFrame == 0) {
				UpperLegL.X -= A2G;
				UpperLegR.X += A2G;
				LowerLegL.X -= A2G;
				LowerLegR.X += A2G;
				FootL.X -= A2G;
				FootR.X += A2G;
			} else if (FacingRight) {
				LowerLegL.X -= A2G;
				FootL.X -= A2G;
			} else {
				LowerLegR.X += A2G;
				FootR.X += A2G;
			}
		}

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
	private static void Smash () {

		int aFrame = (Game.GlobalFrame - Attackness.LastAttackFrame).UDivide(5);
		if (aFrame >= 4) return;

		UpperArmL.Z = FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = FrontSign * UpperArmR.Z.Abs();
		LowerArmL.Z = FrontSign * LowerArmL.Z.Abs();
		LowerArmR.Z = FrontSign * LowerArmR.Z.Abs();
		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

		Head.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;
		Head.Y += A2G * 2 * (aFrame == 0 ? 0 : (3 - aFrame) * -2);

		Body.Y -= aFrame * A2G / 4;
		Hip.Y -= aFrame * A2G / 4;
		Body.Height = Head.Y - Body.Y;

		// Arm
		ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.left;
		ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.right;
		ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderL.Height = Util.Min(ShoulderL.Height, Body.Height);
		ShoulderR.Height = Util.Min(ShoulderR.Height, Body.Height);
		ShoulderL.PivotX = 1000;
		ShoulderR.PivotX = 1000;

		UpperArmL.PivotX = 0;
		UpperArmR.PivotX = 1000;

		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmL.PivotX = 1000;
		UpperArmR.PivotX = 0;

		if (aFrame == 0) {
			UpperArmL.LimbRotate(FacingRight ? 135 : -175);
			UpperArmR.LimbRotate(FacingRight ? 175 : -135);
		} else if (aFrame == 1) {
			UpperArmL.LimbRotate(FacingRight ? -30 : 40);
			UpperArmR.LimbRotate(FacingRight ? -40 : 30);
		} else if (aFrame == 2) {
			UpperArmL.LimbRotate(FacingRight ? -15 : 20);
			UpperArmR.LimbRotate(FacingRight ? -20 : 15);
		} else if (aFrame == 3) {
			UpperArmL.LimbRotate(FacingRight ? -5 : 9);
			UpperArmR.LimbRotate(FacingRight ? -9 : 5);
		}
		UpperArmL.Height += A2G;
		UpperArmR.Height += A2G;

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandL.Width += HandL.Width.Sign() * A2G;
		HandL.Height += HandL.Height.Sign() * A2G;
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		if (AnimationType == CharacterAnimationType.Idle) {
			if (aFrame == 0) {
				UpperLegL.X -= A2G;
				UpperLegR.X += A2G;
				LowerLegL.X -= A2G;
				LowerLegR.X += A2G;
				FootL.X -= A2G;
				FootR.X += A2G;
			} else if (FacingRight) {
				LowerLegL.X -= A2G;
				FootL.X -= A2G;
			} else {
				LowerLegR.X += A2G;
				FootR.X += A2G;
			}
		}

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
}
