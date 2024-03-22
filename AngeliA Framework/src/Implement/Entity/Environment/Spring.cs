using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


public class SpringWoodHorizontal : Spring, ICombustible {
	protected override bool Horizontal => true;
	protected override int Power => 64;
	int ICombustible.BurnStartFrame { get; set; }
}


public class SpringWoodVertical : Spring, ICombustible {
	protected override bool Horizontal => false;
	protected override int Power => 64;
	int ICombustible.BurnStartFrame { get; set; }
}


public class SpringMetalHorizontal : Spring {
	protected override bool Horizontal => true;
	protected override int Power => 128;
}


public class SpringMetalVertical : Spring {
	protected override bool Horizontal => false;
	protected override int Power => 128;
}



[EntityAttribute.UpdateOutOfRange]
public abstract class Spring : EnvironmentRigidbody {


	// Const
	private static readonly int[] BOUNCE_ANI = new int[] { 0, 1, 2, 3, 3, 3, 3, 2, 2, 1, 1, 0, };

	// Api
	protected override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	protected abstract bool Horizontal { get; }
	protected abstract int Power { get; }

	// Short
	private IRect FullRect => new(X, Y, Const.CEL, Const.CEL);

	// Data
	private int LastBounceFrame = int.MinValue;
	private int CurrentArtworkFrame = 0;
	private Direction4 BounceSide = default;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Width = Horizontal ? Const.CEL - 64 : Const.CEL;
		Height = !Horizontal ? Const.CEL - 32 : Const.CEL;
		if (Horizontal) OffsetX = (Const.CEL - Width) / 2;
		LastBounceFrame = int.MinValue;
		BounceSide = default;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Check for Bounce
		if (Horizontal) {
			// Horizontal
			if (Physics.Overlap(
				PhysicsMask.ENTITY, new(X - 1, Y, Const.HALF, Const.CEL), this
			)) {
				PerformBounce(Direction4.Left);
			} else if (Physics.Overlap(
				PhysicsMask.ENTITY, new(X + Const.HALF, Y, Const.HALF + 1, Const.CEL), this
			)) {
				PerformBounce(Direction4.Right);
			}
		} else {
			// Vertical
			if (Physics.Overlap(
				PhysicsMask.ENTITY, new(X, Y + Const.HALF, Const.CEL, Const.HALF + 1), this
			)) {
				PerformBounce(Direction4.Up);
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (Game.GlobalFrame < LastBounceFrame + BOUNCE_ANI.Length) {
			CurrentArtworkFrame++;
		} else {
			CurrentArtworkFrame = 0;
		}
		int frame = CurrentArtworkFrame.UMod(BOUNCE_ANI.Length);
		if (Renderer.TryGetSpriteFromGroup(TypeID, BOUNCE_ANI[frame], out var sprite, false, true)) {
			Renderer.Draw(
				sprite,
				X + Const.HALF, Y,
				500, 0, 0,
				Const.CEL, Const.CEL, Color32
.WHITE
			);
		}
	}


	// LGC
	private void PerformBounce (Direction4 side) {
		LastBounceFrame = Game.GlobalFrame;
		BounceSide = side;
		const int GAP = 16;
		const int THRESHOLD = 96;
		var globalRect = FullRect.Expand(
			Horizontal ? GAP : 0, Horizontal ? GAP : 0,
			Horizontal ? 0 : GAP, Horizontal ? 0 : GAP
		);
		Entity ignore = this;
		for (int safe = 0; safe < 2048; safe++) {
			var hits = Physics.OverlapAll(PhysicsMask.ENTITY, globalRect, out int count, ignore);
			if (count == 0) break;
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				globalRect = hit.Entity.Rect.EdgeOutside(BounceSide, THRESHOLD);
				ignore = hit.Entity;
				PerformBounce(rig);
				break;
			}
		}
	}


	private void PerformBounce (Rigidbody target) {
		if (target == null) return;
		if (Horizontal) {
			// Horizontal
			if (BounceSide == Direction4.Left) {
				if (target.VelocityX > -Power) target.VelocityX = -Power;
			} else {
				if (target.VelocityX < Power) target.VelocityX = Power;
			}
		} else {
			// Vertical
			if (target.VelocityY < Power) target.VelocityY = Power;
			target.MakeGrounded(6);
		}
	}


}
