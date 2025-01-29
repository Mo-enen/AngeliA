using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class BarrelVehicle : Vehicle<BarrelMovement>, IDamageReceiver {

	// Api
	public override bool AllowBeingPush => true;
	public override Int2? DriverLocalPosition => Rolling ? new Int2(Width / 2, BarrelSize) : null;
	public override Int2? DriverLeaveLocalPosition => null;
	public override int AirDragX => Rolling ? 0 : 5;
	public virtual int RollSpeed => 18;
	public virtual int BarrelSize => Const.CEL;
	Tag IDamageReceiver.IgnoreDamageType => Tag.None;

	// Data
	private bool Rolling = false;
	private int CurrentRollingSpeed = 0;
	private int RollingRotation = 0;

	// MSG
	public override void OnActivated () {
		Width = BarrelSize;
		Height = BarrelSize;
		Movement.SwimWidthAmount.BaseValue = 1000;
		Movement.MovementWidth.BaseValue = BarrelSize;
		Movement.MovementHeight.BaseValue = BarrelSize;
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
			Width = BarrelSize;
			OffsetX = -BarrelSize / 2;
			OffsetY = 0;
			Width = BarrelSize;
			Height = BarrelSize;
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
				OffsetX = -BarrelSize / 2;
				OffsetY = 0;
				Width = BarrelSize;
				Height = BarrelSize;
			} else {
				// Driving
				CurrentRollingSpeed = CurrentRollingSpeed.LerpTo(VelocityX, 500);
				Movement.MovementWidth.Override(Driver.Width, 1);
				Movement.MovementHeight.Override(BarrelSize + Driver.Height, 1);
			}
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
				RollingRotation += CurrentRollingSpeed;
				Renderer.Draw(
					sprite,
					X,
					Y + BarrelSize / 2,
					500, 500, RollingRotation,
					BarrelSize, BarrelSize
				);
			}
		}
	}

	public override void Push (int speedX) {
		base.Push(Rolling ? speedX : speedX / 2);
		if (Driver != null || !Rolling) return;
		if (CurrentRollingSpeed != 0 && speedX.Sign() != CurrentRollingSpeed.Sign()) {
			CurrentRollingSpeed = 0;
			return;
		}
		RollingRotation += speedX;
		CurrentRollingSpeed = speedX.Sign3() * RollSpeed;
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (damage.Amount <= 0 || Driver != null) return;
		Rolling = true;
		RollingRotation = 0;
		if (damage.Bullet is Bullet bullet) {
			int bulletX;
			if (bullet is MeleeBullet) {
				bulletX = bullet.Sender != null ? bullet.Sender.Rect.CenterX() : bullet.Rect.CenterX();
			} else {
				bulletX = bullet.Rect.CenterX();
				if (bullet is MovableBullet mBullet) {
					bulletX -= mBullet.Velocity.x;
				}
			}
			CurrentRollingSpeed = (Rect.CenterX() - bulletX).Sign3() * RollSpeed;
		} else if (damage.Bullet != null) {
			CurrentRollingSpeed = (Rect.CenterX() - damage.Bullet.Rect.CenterX()).Sign3() * RollSpeed;
		}
	}

	protected override bool CheckForStartDrive (out Character driver) {

		driver = null;
		if (!Rolling) return false;

		// Check for New Driver Join
		int shrinkX = DeltaPositionX.Abs() + 16;
		var hits = Physics.OverlapAll(
			PhysicsMask.CHARACTER,
			Rect.Shrink(shrinkX, shrinkX, 0, 0).EdgeOutsideUp(1),
			out int count, this
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is Character characterHit && characterHit.Y >= Rect.yMax && !characterHit.InWater) {
				driver = characterHit;
				break;
			}
		}

		return driver != null;
	}

	protected override bool CheckForStopDrive () {
		// Driver Movement State Check
		bool playerStop = Driver == PlayerSystem.Selecting && Input.GameKeyDown(Gamekey.Jump);
		if (
			playerStop ||
			Driver.CharacterState != CharacterState.GamePlay ||
			Driver.VelocityY > Util.Max(DeltaPositionY, 0) ||
			Driver.InWater
		) {
			CurrentRollingSpeed = VelocityX.Sign3() * RollSpeed;
			Width = BarrelSize;
			Height = BarrelSize;
			Driver.VelocityY = Driver.NativeMovement.JumpSpeed + DeltaPositionY;
			IgnorePhysics.True();
			return true;
		}
		return false;
	}

}
