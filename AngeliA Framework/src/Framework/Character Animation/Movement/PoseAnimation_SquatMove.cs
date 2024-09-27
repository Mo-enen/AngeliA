namespace AngeliA;

public class PoseAnimation_SquatMove : PoseAnimation {
	private static readonly float[] EASE = [0f, 0.03125f, 0.125f, 0.28125f, 0.5f, 0.71875f, 0.875f, 0.96875f, 1f, 0.96875f, 0.875f, 0.71875f, 0.5f, 0.28125f, 0.125f, 0.03125f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f,];
	public override void Animate (PoseCharacter character) {
		base.Animate(character);

		const int FRAME_LENGTH = 16;

		int loop = Util.Max(600 / Target.Movement.SquatSpeed.FinalValue.Clamp(1, 256) / FRAME_LENGTH * FRAME_LENGTH, 1);
		int frameRate = (loop / FRAME_LENGTH).GreaterOrEquel(1);
		int arrFrame = (CurrentAnimationFrame.UMod(loop) / frameRate) % FRAME_LENGTH;
		arrFrame = (arrFrame + 4).UMod(FRAME_LENGTH);

		float ease = EASE[arrFrame];
		int easeA2G = (int)(ease * A2G);
		int easeA2G2 = (int)(ease * 2 * A2G);
		int above = Target.PoseRootY = Target.PoseRootY / 2 + easeA2G;

		Body.Height = Body.SizeY / 2 + easeA2G;
		Head.Y = Body.Y + Body.Height;
		Head.Height -= A2G;

		// Arm
		ShoulderL.Y = Body.Y + Body.Height;
		ShoulderR.Y = Body.Y + Body.Height;
		ShoulderL.Height /= 2;
		ShoulderR.Height /= 2;

		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmL.LimbRotate(arrFrame >= 6 && arrFrame <= 12 ? 45 : 25);

		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmR.Z = UpperArmR.Z.Abs();
		UpperArmR.LimbRotate(arrFrame >= 6 && arrFrame <= 12 ? -45 : -25);

		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmL.LimbRotate(-90);
		LowerArmL.Height = LowerArmL.SizeY * 3 / 4;

		LowerArmR.Z = LowerArmR.Z.Abs();
		LowerArmR.LimbRotate(90);
		LowerArmR.Height = LowerArmR.SizeY * 3 / 4;

		HandL.Z = (FacingFront ? 1 : -1) * HandL.Z.Abs();
		HandL.LimbRotate(0);

		HandR.Z = (FacingFront ? 1 : -1) * HandR.Z.Abs();
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.X -= FacingRight ? easeA2G2 : easeA2G;
		UpperLegR.X += FacingRight ? easeA2G : easeA2G2;

		LowerLegL.Height -= A2G;
		LowerLegL.X = UpperLegL.X + (FacingRight ? -A2G : 0);
		LowerLegL.Y = Util.Max(UpperLegL.Y - UpperLegL.Height, Body.Y - above + LowerLegL.Height);

		LowerLegR.Height -= A2G;
		LowerLegR.X = UpperLegR.X + (FacingRight ? 0 : A2G);
		LowerLegR.Y = Util.Max(UpperLegR.Y - UpperLegR.Height, Body.Y - above + LowerLegR.Height);

		FootL.X = FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
		FootL.Y = FootL.Height - above;

		FootR.X = FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
		FootR.Y = FootR.Height - above;

		// Final
		Target.HandGrabRotationL =
			Target.EquippingWeaponHeld == WeaponHandheld.OneOnEachHand ? FacingRight ? 80 : -100 :
			FacingSign * 100;
		Target.HandGrabRotationR =
			Target.EquippingWeaponHeld == WeaponHandheld.OneOnEachHand ? FacingRight ? 100 : -80 :
			FacingSign * 100;

	}
}
