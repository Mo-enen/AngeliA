namespace AngeliA;

public class PoseAnimation_SquatIdle : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int arrFrame = CurrentAnimationFrame.UMod(64) / 16;

		float ease = Ease.InOutCirc(arrFrame / 4f);
		if (arrFrame >= 2) ease = 1f - ease;
		int oneEase = (int)(ease * A2G);
		int halfEase = (int)(ease * A2G / 2);
		int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
		int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

		int above = Rendering.PoseRootY /= 2;

		Body.Width += FacingSign * halfEase;
		Body.Height = Body.SizeY / 2 - oneEase;

		Head.Y = Body.Y + Body.Height;
		Head.Height -= A2G;

		ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
		ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
		UpperArmL.X = ShoulderL.X;
		UpperArmR.X = ShoulderR.X;
		LowerArmL.X = UpperArmL.X;
		LowerArmR.X = UpperArmR.X;

		// Arm
		ShoulderL.Y = Body.Y + Body.Height;
		ShoulderR.Y = Body.Y + Body.Height;
		ShoulderL.Height /= 2;
		ShoulderR.Height /= 2;

		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmL.LimbRotate(25);

		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmR.Z = UpperArmR.Z.Abs();
		UpperArmR.LimbRotate(-25);

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
		LowerLegL.Height -= A2G;
		LowerLegL.X = UpperLegL.X + (FacingRight ? -A2G : 0);
		LowerLegL.Y = Util.Max(UpperLegL.Y - UpperLegL.Height, Body.Y - above + LowerLegL.Height);

		LowerLegR.Height -= A2G;
		LowerLegR.X = UpperLegR.X + (FacingRight ? 0 : A2G);
		LowerLegR.Y = Util.Max(UpperLegR.Y - UpperLegR.Height, Body.Y - above + LowerLegR.Height);

		FootL.X = FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
		FootR.X = FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
		FootL.Y = -above + FootL.Height;
		FootR.Y = -above + FootR.Height;

		// Arm
		UpperArmL.X -= halfEase;
		UpperArmL.Width += halfEase;
		UpperArmR.X += halfEase;
		UpperArmR.Width += halfEase;
		LowerArmL.X -= halfEase;
		LowerArmL.Width += halfEase;
		LowerArmR.X += halfEase;
		LowerArmR.Width += halfEase;
		HandL.X -= halfEase;
		HandR.X += halfEase;

	}
}
