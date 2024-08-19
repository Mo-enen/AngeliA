using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class BarrelVehicle : Vehicle<BarrelMovement>, IDamageReceiver, IActionTarget {

	// Api
	public int Team => Const.TEAM_ENVIRONMENT;
	public override bool AllowBeingPush => false;
	public override Int2? DriverLocalPosition => Rolling ? new Int2(Width / 2, Height) : null;
	public override Int2? DriverLeaveLocalPosition => null;
	public override int AirDragX => Rolling ? 0 : 5;
	public virtual int RollSpeed => 18;

	// Data
	private bool Rolling = false;
	private int CurrentRollingSpeed = 0;
	private int RollingRotation = 0;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Rolling = false;
		CurrentRollingSpeed = 0;
		RollingRotation = 0;
	}

	public override void Update () {
		base.Update();
		if (!Rolling) {
			// Idle
			if (Driver != null) {
				StopDrive();
			}
			CurrentRollingSpeed = 0;
			VelocityX = 0;
		} else {
			// Roll
			if (Driver == null) {
				// Self Rolling
				VelocityX = 0;
				if (CurrentRollingSpeed != 0) {
					int prevX = X;
					CurrentRollingSpeed = CurrentRollingSpeed.Clamp(-RollSpeed, RollSpeed);
					PerformMove(CurrentRollingSpeed, 0);
					if (X == prevX) {
						CurrentRollingSpeed = 0;
					}
				}
			} else {
				// Driving
				CurrentRollingSpeed = CurrentRollingSpeed.LerpTo(VelocityX, 500);
			}
		}
	}

	protected override void LateUpdateVehicle () {
		base.LateUpdateVehicle();
		if (!Rolling) {
			// Normal
			if (Renderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
				Renderer.Draw(sprite, Rect);
			}
		} else {
			// Rolling
			if (Renderer.TryGetSpriteFromGroup(TypeID, 1, out var sprite, false, true)) {
				RollingRotation += CurrentRollingSpeed;
				Renderer.Draw(
					sprite, X + OffsetX + Width / 2, Y + OffsetY + Height / 2, 500, 500, RollingRotation, Width, Height
				);
			}
		}
	}

	public override void Push (int speedX) {
		base.Push(speedX);
		if (CurrentRollingSpeed == 0 || Driver != null) return;
		if (speedX.Sign() != CurrentRollingSpeed.Sign()) {
			CurrentRollingSpeed = 0;
			return;
		}
		RollingRotation += speedX;
		CurrentRollingSpeed = speedX.Sign3() * RollSpeed;
	}

	void IDamageReceiver.TakeDamage (Damage damage) {
		if (damage.Amount <= 0 || Driver != null) return;
		Rolling = true;
		RollingRotation = 0;
		if (damage.Bullet is Bullet bullet) {
			int bulletX = bullet.X + bullet.Width / 2;
			if (bullet is MovableBullet mBullet) {
				bulletX -= mBullet.Velocity.x;
			}
			CurrentRollingSpeed = (Rect.CenterX() - bulletX).Sign3() * RollSpeed;
		} else if (damage.Sender != null) {
			CurrentRollingSpeed = (Rect.CenterX() - damage.Sender.Rect.CenterX()).Sign3() * RollSpeed;
		}
	}

	bool IActionTarget.Invoke () => false;

	bool IActionTarget.AllowInvoke () => false;

	protected override bool CheckForStartDrive (out Character driver) {

		driver = null;
		if (!Rolling) return false;

		// Check for New Driver Join
		int shrinkX = DeltaPositionX.Abs() + 16;
		var hits = Physics.OverlapAll(
			PhysicsMask.CHARACTER,
			Rect.Shrink(shrinkX, shrinkX, 0, 0).EdgeOutside(Direction4.Up, 1),
			out int count, this
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is Character characterHit && characterHit.Y >= Rect.yMax) {
				driver = characterHit;
				break;
			}
		}

		return driver != null;
	}

	protected override bool CheckForStopDrive () {
		// Driver Movement State Check
		if (
			Driver.CharacterState != CharacterState.GamePlay ||
			Driver.VelocityY > Util.Max(DeltaPositionY, 0)
		) {
			CurrentRollingSpeed = VelocityX.Sign3() * RollSpeed;
			return true;
		}
		return false;
	}

}
