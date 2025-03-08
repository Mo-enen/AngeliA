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

	protected void OnBumped (Rigidbody rig, Direction4 from);

	protected bool AllowBump (Rigidbody rig);

	internal void TryPerformBump (Rigidbody rig, bool horizontal) {
		if (Game.GlobalFrame < LastBumpedFrame + Cooldown) return;
		if (!AllowBump(rig)) return;
		if (horizontal) {
			if (rig.VelocityX > 0) {
				// Right
				if (!FromLeft) return;
				LastBumpedFrame = Game.GlobalFrame;
				OnBumped(rig, Direction4.Left);
			} else if (rig.VelocityX < 0) {
				// Left
				if (!FromRight) return;
				LastBumpedFrame = Game.GlobalFrame;
				OnBumped(rig, Direction4.Right);
			}
		} else {
			if (rig.VelocityY > 0) {
				// Up
				if (!FromBelow) return;
				LastBumpedFrame = Game.GlobalFrame;
				OnBumped(rig, Direction4.Down);
			} else if (rig.VelocityY < 0) {
				// Down
				if (!FromAbove) return;
				LastBumpedFrame = Game.GlobalFrame;
				OnBumped(rig, Direction4.Up);
			}
		}
	}

}
