using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public class BarrelIron : EnvironmentRigidbody, IDamageReceiver {


	// Const
	private const int ROLL_SPEED = 12;

	// Api
	public int Team => Const.TEAM_ENVIRONMENT;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override bool AllowBeingPush => false;

	// Data
	private Character Driver = null;
	private bool Rolling = false;
	private int RollingSpeed = 0;
	private int RollingRotation = 0;
	private bool IsDriving = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Rolling = false;
		RollingSpeed = 0;
		RollingRotation = 0;
		const int SIZE_DELTA = 16;
		Width -= SIZE_DELTA;
		Height -= SIZE_DELTA;
		X += SIZE_DELTA / 2;
		Driver = null;
	}


	public override void Update () {
		base.Update();
		IsDriving = DrivingCheck();
		if (IsDriving) OnDriving();
		if (!IsDriving && Rolling && RollingSpeed != 0) {
			int prevX = X;
			PerformMove(RollingSpeed, 0);
			if (X == prevX) RollingSpeed = 0;
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Rolling) {
			// Normal
			if (Renderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
				Renderer.Draw(sprite, Rect);
			}
		} else {
			// Rolling
			if (Renderer.TryGetSpriteFromGroup(TypeID, 1, out var sprite, false, true)) {
				RollingRotation += RollingSpeed;
				Renderer.Draw(
					sprite, X + Width / 2, Y + Height / 2, 500, 500, RollingRotation, Width, Height
				);
			}
		}
	}


	public override void Push (int speedX) {
		base.Push(speedX);
		if (RollingSpeed == 0) return;
		if (IsDriving) return;
		if (speedX.Sign() != RollingSpeed.Sign()) {
			RollingSpeed = 0;
			return;
		}
		RollingRotation += speedX;
		RollingSpeed = speedX.Sign3() * ROLL_SPEED;
	}


	void IDamageReceiver.TakeDamage (Damage damage) {
		if (damage.Amount <= 0) return;
		if (IsDriving) return;
		Rolling = true;
		RollingRotation = 0;
		if (damage.Bullet is Bullet bullet) {
			int bulletX = bullet.X + bullet.Width / 2;
			if (bullet is MovableBullet mBullet) {
				bulletX -= mBullet.Velocity.x;
			}
			RollingSpeed = (Rect.CenterX() - bulletX).Sign3() * ROLL_SPEED;
		} else if (damage.Sender != null) {
			RollingSpeed = (Rect.CenterX() - damage.Sender.Rect.CenterX()).Sign3() * ROLL_SPEED;
		}
	}


	private void OnDriving () {

		if (Driver is Player player && player == Player.Selecting) {
			// Control by Selecting Player
			if (player.Movement.IntendedX != 0) {
				RollingSpeed =
					(player.Movement.FacingRight ? -1 : 1) *
					(player.Movement.IsSquatting ? player.Movement.SquatSpeed * 3 / 2 : player.Movement.WalkSpeed / 2);
			} else {
				RollingSpeed = 0;
			}
		} else if (Driver is Character driver) {
			// Control by Character
			RollingSpeed = -driver.VelocityX;
		}

		// Move
		if (RollingSpeed != 0) {
			int prevX = X;
			PerformMove(RollingSpeed, 0);
			if (X == prevX) RollingSpeed = 0;
		}

		// Carry Driver
		if (Driver is Character cDriver) {
			// Character
			cDriver.X = X + Width / 2;
			cDriver.Y = Y + Height;
			cDriver.Movement.ClearRunningAccumulate();
			cDriver.CancelBounce();
			if (!cDriver.TakingDamage && !cDriver.Teleporting) {
				cDriver.LockAnimationType(
					cDriver.Movement.IntendedX == 0 ?
						cDriver.Movement.IntendedY < 0 ? CharacterAnimationType.SquatIdle : CharacterAnimationType.Idle :
						cDriver.Movement.IntendedY < 0 ? CharacterAnimationType.SquatMove : CharacterAnimationType.Walk
				);
			}
		} else if (Driver != null) {
			// Entity
			Driver.X = X;
			Driver.Y = Y + Height;
		}
	}


	// LGC
	private bool DrivingCheck () {

		if (!Rolling) {
			Driver = null;
			return false;
		}
		var characterDriver = Driver as Character;

		// Check for New Driver Join
		if (characterDriver == null) {
			int shrinkX = DeltaPositionX.Abs() + 16;
			var hits = Physics.OverlapAll(
				PhysicsMask.ENTITY,
				Rect.Shrink(shrinkX, shrinkX, 0, 0).EdgeOutside(Direction4.Up, 1),
				out int count, this
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is Character characterHit && characterHit.Y >= Rect.yMax) {
					characterDriver = characterHit;
				}
			}
		}

		// Movement State Check
		if (characterDriver != null) {
			if (
				characterDriver.CharacterState != CharacterState.GamePlay ||
				characterDriver.VelocityY > Util.Max(DeltaPositionY, 0)
			) {
				characterDriver = null;
			} else if (
				characterDriver.Movement.MovementState != CharacterMovementState.Idle &&
				characterDriver.Movement.MovementState != CharacterMovementState.Walk &&
				characterDriver.Movement.MovementState != CharacterMovementState.Run &&
				characterDriver.Movement.MovementState != CharacterMovementState.SquatIdle &&
				characterDriver.Movement.MovementState != CharacterMovementState.SquatMove &&
				characterDriver.Movement.MovementState != CharacterMovementState.JumpDown &&
				characterDriver.Movement.MovementState != CharacterMovementState.JumpUp
			) {
				characterDriver = null;
			}
		}
		Driver = characterDriver;
		return Driver != null;
	}


}
