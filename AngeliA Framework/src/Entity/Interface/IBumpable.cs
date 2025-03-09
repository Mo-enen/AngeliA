using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IBumpable {

	public bool FromBelow => true;
	public bool FromAbove => false;
	public bool FromLeft => false;
	public bool FromRight => false;
	public int Cooldown => 16;
	public int LastBumpedFrame { get; set; }
	public Direction4 LastBumpFrom { get; set; }

	protected void OnBumped (Rigidbody rig);

	protected bool AllowBump (Rigidbody rig);

	internal void TryPerformBump (Rigidbody rig, bool horizontal) {
		if (Game.GlobalFrame < LastBumpedFrame + Cooldown) return;
		if (!AllowBump(rig)) return;
		if (horizontal) {
			if (rig.VelocityX > 0) {
				// Right
				if (!FromLeft) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Left;
				OnBumped(rig);
			} else if (rig.VelocityX < 0) {
				// Left
				if (!FromRight) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Right;
				OnBumped(rig);
			}
		} else {
			if (rig.VelocityY > 0) {
				// Up
				if (!FromBelow) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Down;
				OnBumped(rig);
			} else if (rig.VelocityY < 0) {
				// Down
				if (!FromAbove) return;
				LastBumpedFrame = Game.GlobalFrame;
				LastBumpFrom = Direction4.Up;
				OnBumped(rig);
			}
		}
	}

	public static void AnimateForBump (IBumpable bumpable, Cell cell, int duration = 12) {
		if (Game.GlobalFrame >= bumpable.LastBumpedFrame + duration) return;
		float ease01 = Ease.OutBack((Game.GlobalFrame - bumpable.LastBumpedFrame) / (float)duration);
		switch (bumpable.LastBumpFrom) {
			case Direction4.Left:
				cell.ReturnPivots(0f, 0.5f);
				cell.X += (int)(ease01 * 32);
				break;
			case Direction4.Right:
				cell.ReturnPivots(1f, 0.5f);
				cell.X -= (int)(ease01 * 32);
				break;
			case Direction4.Down:
				cell.ReturnPivots(0.5f, 0f);
				cell.Y += (int)(ease01 * 32);
				break;
			case Direction4.Up:
				cell.ReturnPivots(0.5f, 1f);
				cell.Y -= (int)(ease01 * 32);
				break;
		}
		cell.Width += (int)(ease01 * 32);
		cell.Height += (int)(ease01 * 32);
		cell.Z++;
	}

}
