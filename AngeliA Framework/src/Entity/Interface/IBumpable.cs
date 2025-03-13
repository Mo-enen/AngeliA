using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IBumpable {


	// VAR
	public bool FromBelow => true;
	public bool FromAbove => false;
	public bool FromLeft => false;
	public bool FromRight => false;
	public bool TransferBumpToOther => true;
	public bool TransferBumpFromOther => false;
	public bool TransferWithAttack => true;
	public int BumpTransferPower => 60;
	public int BumpCooldown => 16;
	public int LastBumpedFrame { get; set; }
	public Direction4 LastBumpFrom { get; set; }


	// MSG
	protected void OnBumped (Rigidbody rig);

	protected bool AllowBump (Rigidbody rig);

	protected Damage GetBumpDamage () => new(1);


	// API
	internal void TryPerformBump (Rigidbody rig, bool horizontal) {
		if (Game.GlobalFrame < LastBumpedFrame + BumpCooldown) return;
		if (this is not Entity entity) return;
		if (!AllowBump(rig)) return;
		if (horizontal) {
			if (rig.VelocityX > 0) {
				// Right
				if (!FromLeft) return;
				if (rig.Rect.xMax > entity.Rect.CenterX()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Left;
				PerformTransferBump(Direction4.Right, entity);
				OnBumped(rig);
			} else if (rig.VelocityX < 0) {
				// Left
				if (!FromRight) return;
				if (rig.Rect.x < entity.Rect.CenterX()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Right;
				PerformTransferBump(Direction4.Left, entity);
				OnBumped(rig);
			}
		} else {
			if (rig.VelocityY > 0) {
				// Up
				if (!FromBelow) return;
				if (rig.Rect.yMax > entity.Rect.CenterY()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Down;
				PerformTransferBump(Direction4.Up, entity);
				OnBumped(rig);
			} else if (rig.VelocityY < 0) {
				// Down
				if (!FromAbove) return;
				if (rig.Rect.y < entity.Rect.CenterY()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Up;
				PerformTransferBump(Direction4.Down, entity);
				OnBumped(rig);
			}
		}
	}

	public static void AnimateForBump (IBumpable bumpable, Cell cell, int duration = 12, int distance = 32, int size = 32) {
		if (Game.GlobalFrame >= bumpable.LastBumpedFrame + duration) return;
		float ease01 = Ease.OutBack((Game.GlobalFrame - bumpable.LastBumpedFrame) / (float)duration);
		switch (bumpable.LastBumpFrom) {
			case Direction4.Left:
				cell.ReturnPivots(0f, 0.5f);
				cell.X += (int)(ease01 * distance);
				break;
			case Direction4.Right:
				cell.ReturnPivots(1f, 0.5f);
				cell.X -= (int)(ease01 * distance);
				break;
			case Direction4.Down:
				cell.ReturnPivots(0.5f, 0f);
				cell.Y += (int)(ease01 * distance);
				break;
			case Direction4.Up:
				cell.ReturnPivots(0.5f, 1f);
				cell.Y -= (int)(ease01 * distance);
				break;
		}
		cell.Width += (int)(ease01 * size);
		cell.Height += (int)(ease01 * size);
		cell.Z++;
	}


	// LGC
	private static void PerformTransferBump (Direction4 direction, Entity self) {
		if (self is not IBumpable selfBump) return;
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, self.Rect.EdgeOutside(direction, 1), out int count, self);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];

			if (hit.Entity is IBumpable bump && !bump.TransferBumpFromOther) continue;

			// Perform Bump Transfer
			if (hit.Entity is Rigidbody rig) {
				switch (direction) {
					case Direction4.Up:
						rig.VelocityY = selfBump.BumpTransferPower.GreaterOrEquel(rig.VelocityY);
						break;
					case Direction4.Down:
						rig.VelocityY = (-selfBump.BumpTransferPower).LessOrEquel(rig.VelocityY);
						break;
					case Direction4.Left:
						rig.VelocityX = (-selfBump.BumpTransferPower).LessOrEquel(rig.VelocityX);
						break;
					case Direction4.Right:
						rig.VelocityX = selfBump.BumpTransferPower.GreaterOrEquel(rig.VelocityX);
						break;
				}
			} else {
				switch (direction) {
					case Direction4.Up:
						hit.Entity.Y += selfBump.BumpTransferPower;
						break;
					case Direction4.Down:
						hit.Entity.Y -= selfBump.BumpTransferPower;
						break;
					case Direction4.Left:
						hit.Entity.X -= selfBump.BumpTransferPower;
						break;
					case Direction4.Right:
						hit.Entity.X += selfBump.BumpTransferPower;
						break;
				}
			}

			// Attack
			if (selfBump.TransferWithAttack && hit.Entity is IDamageReceiver receiver) {
				receiver.TakeDamage(selfBump.GetBumpDamage());
			}

		}
	}

}
