using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Interface that makes the entity can be bump by other (like question mark block in Mario)
/// </summary>
public interface IBumpable {


	// VAR
	/// <summary>
	/// True if the entity can be bump from below
	/// </summary>
	public bool FromBelow => true;
	/// <summary>
	/// True if the entity can be bump from above
	/// </summary>
	public bool FromAbove => false;
	/// <summary>
	/// True if the entity can be bump from left
	/// </summary>
	public bool FromLeft => false;
	/// <summary>
	/// True if the entity can be bump from right
	/// </summary>
	public bool FromRight => false;
	/// <summary>
	/// True if the entity bump other entities when being bumped
	/// </summary>
	public bool TransferBumpToOther => true;
	/// <summary>
	/// True if the entity take transfered bumps from other
	/// </summary>
	public bool TransferBumpFromOther => false;
	/// <summary>
	/// True if the entity perform attack to the entity when transfer bumps (like in Mario bump on question block can kill the goombas on top)
	/// </summary>
	public bool TransferWithAttack => true;
	/// <summary>
	/// Extra speed that gives to the rigidbody when they got transfered bump from this entity
	/// </summary>
	public int BumpTransferPower => 60;
	/// <summary>
	/// How many frames does it have to wait to be bump again
	/// </summary>
	public int BumpCooldown => 16;
	/// <summary>
	/// Frame when the entity get it's last bump
	/// </summary>
	public int LastBumpedFrame { get; set; }
	/// <summary>
	/// Direction for the last bump of this entity
	/// </summary>
	public Direction4 LastBumpFrom { get; set; }


	// MSG
	/// <summary>
	/// This function is called when this entity is bumped
	/// </summary>
	/// <param name="rig">Rigidbody that bumps this entity</param>
	/// <param name="damage">The damage this entity got from this bump</param>
	protected void OnBumped (Entity rig, Damage damage);

	/// <summary>
	/// True if the entity can be bump by the given target and direction currently
	/// </summary>
	protected bool AllowBump (Entity rig, Direction4 from) => IsValidBumpDirection(this, from);

	/// <summary>
	/// Get the instance of the damage that this entity deal to other when it transfer bump
	/// </summary>
	protected Damage GetBumpTransferDamage () => new(1);


	// API
	internal void TryPerformBump (Entity sender, Direction4 directionTo, bool forceBump = false, Damage damageToBumpedObject = default) {

		if (!forceBump && Game.GlobalFrame < LastBumpedFrame + BumpCooldown) return;
		if (this is not Entity entity) return;

		switch (directionTo) {
			case Direction4.Left:
				if (!forceBump && !AllowBump(sender, Direction4.Right)) return;
				if (sender.Rect.x < entity.Rect.CenterX()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Right;
				PerformTransferBump(Direction4.Left, entity);
				OnBumped(sender, damageToBumpedObject);
				break;

			case Direction4.Right:
				if (!forceBump && !AllowBump(sender, Direction4.Left)) return;
				if (sender.Rect.xMax > entity.Rect.CenterX()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Left;
				PerformTransferBump(Direction4.Right, entity);
				OnBumped(sender, damageToBumpedObject);
				break;

			case Direction4.Down:
				if (!forceBump && !AllowBump(sender, Direction4.Up)) return;
				if (sender.Rect.y < entity.Rect.CenterY()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Up;
				PerformTransferBump(Direction4.Down, entity);
				OnBumped(sender, damageToBumpedObject);
				break;

			case Direction4.Up:
				if (!forceBump && !AllowBump(sender, Direction4.Down)) return;
				if (sender.Rect.yMax > entity.Rect.CenterY()) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Down;
				PerformTransferBump(Direction4.Up, entity);
				OnBumped(sender, damageToBumpedObject);
				break;
		}
	}

	/// <summary>
	/// Update the animation for bump, call this function every frame
	/// </summary>
	/// <param name="bumpable">Target entity</param>
	/// <param name="cell">Rendering cell</param>
	/// <param name="duration">How length does the animation takes when it get bump</param>
	/// <param name="distance">How far does it move when it get bump</param>
	/// <param name="size">How big does it scale when it get bump</param>
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

	/// <summary>
	/// Trie if the given direction can be bump
	/// </summary>
	/// <param name="bump">Target entity</param>
	/// <param name="from"></param>
	public static bool IsValidBumpDirection (IBumpable bump, Direction4 from) => from switch {
		Direction4.Up => bump.FromAbove,
		Direction4.Down => bump.FromBelow,
		Direction4.Left => bump.FromLeft,
		Direction4.Right => bump.FromRight,
		_ => true,
	};

	/// <summary>
	/// Perform bump for all overlaped IBumpable entities
	/// </summary>
	/// <param name="sender">Entity that send the bump</param>
	/// <param name="directionTo">Bump the IBumpables to this direction</param>
	/// <param name="forceBump">True if ignore the AllowBump function check this time</param>
	/// <param name="damageToBumpedObject">Damage data that apply to the entities being bump</param>
	/// <param name="collisionMask">Which layer does this bump applies</param>
	public static void BumpAllOverlap (Entity sender, Direction4 directionTo, bool forceBump = false, Damage damageToBumpedObject = default, int collisionMask = PhysicsMask.MAP) {
		var hits = Physics.OverlapAll(
			collisionMask,
			sender.Rect.EdgeOutside(directionTo, 1), out int count, sender
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IBumpable hitBump) continue;
			hitBump.TryPerformBump(sender, directionTo, forceBump, damageToBumpedObject);
		}
	}

	// LGC
	private static void PerformTransferBump (Direction4 direction, Entity self) {
		if (self is not IBumpable selfBump) return;
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, self.Rect.EdgeOutside(direction, 1), out int count, self);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];

			if (hit.Entity is IBumpable bump) {
				if (!bump.TransferBumpFromOther) continue;
				bump.LastBumpFrom = direction.Opposite();
				bump.OnBumped(self, selfBump.GetBumpTransferDamage());
			}

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
			}

			// Attack
			if (selfBump.TransferWithAttack && hit.Entity is IDamageReceiver receiver) {
				receiver.TakeDamage(selfBump.GetBumpTransferDamage());
			}

		}
	}

}
