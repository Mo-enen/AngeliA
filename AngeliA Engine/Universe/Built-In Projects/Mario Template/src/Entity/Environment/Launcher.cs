using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class LauncherBlack : Launcher {
	private static readonly SpriteCode TOP_SP = "LauncherBlack.Top";
	private static readonly SpriteCode MID_SP = "LauncherBlack.Mid";
	private static readonly SpriteCode BOTTOM_SP = "LauncherBlack.Bottom";
	protected override int TopSprite => TOP_SP;
	protected override int MidSprite => MID_SP;
	protected override int BottomSprite => BOTTOM_SP;
	public override Int2 LaunchVelocity => TargetEntityID == Coin.TYPE_ID ? new(Util.QuickRandom(32, 52), Util.QuickRandom(-12, 12)) : new(42, 0);
	public override int LaunchFrequency => 240;
	public override int FailbackEntityID => BulletBillBlack.TYPE_ID;
}


public class LauncherRed : Launcher {
	private static readonly SpriteCode TOP_SP = "LauncherRed.Top";
	private static readonly SpriteCode MID_SP = "LauncherRed.Mid";
	private static readonly SpriteCode BOTTOM_SP = "LauncherRed.Bottom";
	protected override int TopSprite => TOP_SP;
	protected override int MidSprite => MID_SP;
	protected override int BottomSprite => BOTTOM_SP;
	public override Int2 LaunchVelocity => TargetEntityID == Coin.TYPE_ID ? new(Util.QuickRandom(54, 74), Util.QuickRandom(-12, 12)) : new(64, 0);
	public override int LaunchFrequency => 120;
	public override int FailbackEntityID => BulletBillRed.TYPE_ID;
}


[EntityAttribute.MapEditorGroup("Entity")]
public abstract class Launcher : AngeliA.Platformer.Launcher, ICarrier {

	// VAR
	protected abstract int TopSprite { get; }
	protected abstract int MidSprite { get; }
	protected abstract int BottomSprite { get; }
	public override int MaxLaunchCount => 6;
	public override int ItemCountPreLaunch => TargetEntityID == Coin.TYPE_ID ? 6 : 1;
	public override bool AllowingAutoLaunch => Pose == FittingPose.Up;
	public override Int2 LaunchOffset => new(Const.CEL + Const.HALF, 0);
	public override bool LaunchWhenEntranceBlocked => false;
	public override bool KeepLaunchedEntityInMap => false;
	public override bool LaunchTowardsPlayer => true;
	public override bool UseMomentum => true;
	int ICarrier.CarryLeft { get; set; }
	int ICarrier.CarryRight { get; set; }
	int ICarrier.CarryHorizontalFrame { get; set; }
	private FittingPose Pose = FittingPose.Unknown;
	private Launcher LauncherAtBottom;
	private int CurrentFallingSpeed;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		CurrentFallingSpeed = 0;
		Pose = FittingPose.Unknown;
	}

	public override void FirstUpdate () => Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);

	public override void Update () {
		base.Update();
		// Get Pose
		if (Pose == FittingPose.Unknown) {
			LauncherAtBottom = Physics.GetEntity(TypeID, Rect.EdgeOutsideDown(1), PhysicsMask.ENVIRONMENT, this) as Launcher;
			bool hasU = Physics.GetEntity(TypeID, Rect.EdgeOutsideUp(1), PhysicsMask.ENVIRONMENT, this) != null;
			bool hasUU = Physics.GetEntity(TypeID, Rect.Shift(0, Const.CEL).EdgeOutsideUp(1), PhysicsMask.ENVIRONMENT, this) != null;
			Pose = hasU && hasUU ? FittingPose.Down : !hasU ? FittingPose.Up : FittingPose.Mid;
		}

		// Self Movement
		if (LauncherAtBottom != null) {
			// Link To Launcher D
			X = LauncherAtBottom.X;
			Y = LauncherAtBottom.Y + LauncherAtBottom.Height;
		} else {
			// Fall Down
			var oldXY = XY;
			XY = Physics.Move(PhysicsMask.MAP, oldXY, 0, CurrentFallingSpeed, new Int2(Width, Height), this);
			if (XY != oldXY) {
				ICarrier.CarryTargetsOnTopVertically(this, XY.y - oldXY.y);
			} else {
				CurrentFallingSpeed = 0;
			}
			CurrentFallingSpeed -= Rigidbody.GlobalGravity;
		}

	}

	public override void LateUpdate () {
		Renderer.Draw(
			Pose == FittingPose.Mid ? MidSprite : Pose == FittingPose.Up ? TopSprite : BottomSprite,
			Rect
		);
	}

	protected override void OnEntityLaunched (Entity entity, int x, int y) {
		base.OnEntityLaunched(entity, x, y);
		if (entity is Coin coin) {
			coin.IsLoose = true;
		}
		if (entity is BulletBill bullet) {
			bullet.MovingRight = LaunchToRightSide();
		}
	}

	void ICarrier.PerformCarry (int x, int y) {
		X += x;
		Y += y;
	}

}
