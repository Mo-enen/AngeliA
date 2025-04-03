using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class BurnerAltLeft : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => Frequency / 2;
	protected override Direction4 Direction => Direction4.Left;
}
public class BurnerAltRight : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => Frequency / 2;
	protected override Direction4 Direction => Direction4.Right;
}
public class BurnerAltDown : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => Frequency / 2;
	protected override Direction4 Direction => Direction4.Down;
}
public class BurnerAltUp : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => Frequency / 2;
	protected override Direction4 Direction => Direction4.Up;
}


public class BurnerLeft : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => 0;
	protected override Direction4 Direction => Direction4.Left;
}
public class BurnerRight : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => 0;
	protected override Direction4 Direction => Direction4.Right;
}
public class BurnerDown : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => 0;
	protected override Direction4 Direction => Direction4.Down;
}
public class BurnerUp : Burner {
	protected override int Frequency => 240;
	protected override int Duration => 100;
	protected override int TimingOffset => 0;
	protected override Direction4 Direction => Direction4.Up;
}


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Burner : Entity, IAutoTrackWalker {


	// VAR
	private static readonly AudioCode FIRE_AC = "BurnerFire";
	private static readonly SpriteCode BODY_SP = "Burner";
	private static readonly SpriteCode FIRE_SP = "Burner.Fire";
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	protected abstract int Frequency { get; }
	protected abstract int Duration { get; }
	protected abstract int TimingOffset { get; }
	protected abstract Direction4 Direction { get; }
	private int FireLocalFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		FireLocalFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, (this as IAutoTrackWalker).OnTrack);
	}

	public override void Update () {
		base.Update();
		// Fire
		FireLocalFrame = (Game.SettleFrame + TimingOffset) % Frequency;
		if (FireLocalFrame < Duration) {
			// Fire Damage
			var rect = Direction switch {
				Direction4.Left => Rect.CornerOutside(Alignment.MidLeft, Const.CEL * 2, 64),
				Direction4.Right => Rect.CornerOutside(Alignment.MidRight, Const.CEL * 2, 64),
				Direction4.Down => Rect.CornerOutside(Alignment.BottomMid, 64, Const.CEL * 2),
				Direction4.Up => Rect.CornerOutside(Alignment.TopMid, 64, Const.CEL * 2),
				_ => default,
			};
			IDamageReceiver.DamageAllOverlap(
				rect, new Damage(1, Const.TEAM_PLAYER, type: Tag.FireDamage)
			);
			// Audio
			if (FireLocalFrame == 0) {
				FrameworkUtil.PlaySoundAtPosition(FIRE_AC, XY);
			}
		} else {
			FireLocalFrame = int.MinValue;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// Body
		Renderer.Draw(
			BODY_SP, X + Width / 2, Y + Height / 2, 500, 500, Direction.GetRotation(), Const.CEL, Const.CEL
		);
		// Fire
		if (FireLocalFrame >= 0) {
			Renderer.DrawAnimation(
				FIRE_SP,
				Direction.IsVertical() ? X + Width / 2 : Direction == Direction4.Left ? X : X + Width,
				Direction.IsHorizontal() ? Y + Height / 2 : Direction == Direction4.Down ? Y : Y + Height,
				500, 0, Direction.GetRotation(),
				Const.CEL, Const.CEL * 2,
				FireLocalFrame
			);
		}
	}


}