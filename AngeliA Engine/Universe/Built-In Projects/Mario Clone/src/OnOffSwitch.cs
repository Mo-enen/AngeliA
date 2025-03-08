using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioClone;


public class OnSwitch : OnOffSwitch {
	protected override bool ReverseOnOff => false;
}


public class OffSwitch : OnOffSwitch {
	protected override bool ReverseOnOff => true;
}


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class OnOffSwitch : Entity, IBumpable {

	// VAR
	private const int COOL_DOWN = 12;
	private static readonly SpriteCode OnSprite = "OnSwitch";
	private static readonly SpriteCode OffSprite = "OffSwitch";
	public static bool CurrentOn { get; private set; } = true;
	private static int LastSwitchFrame = int.MinValue;
	protected abstract bool ReverseOnOff { get; }
	int IBumpable.LastBumpedFrame { get; set; } = int.MinValue;

	// MSG
	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void LateUpdate () {
		base.LateUpdate();

		// Body
		var cell = Renderer.Draw(CurrentOn == ReverseOnOff ? OffSprite : OnSprite, Rect);

		// Bump Animation
		var bump = this as IBumpable;
		const float DUR = 12f;
		if (Game.GlobalFrame < bump.LastBumpedFrame + DUR) {
			cell.Y += (int)(Ease.OutBack(
				(Game.GlobalFrame - bump.LastBumpedFrame) / DUR
			) * 64);
		}

	}

	public static void TrySwitch () {
		if (Game.GlobalFrame < LastSwitchFrame + COOL_DOWN) return;
		LastSwitchFrame = Game.GlobalFrame;
		CurrentOn = !CurrentOn;
	}

	void IBumpable.OnBumped (Rigidbody rig, Direction4 from) => TrySwitch();

	bool IBumpable.AllowBump (Rigidbody rig) => rig == PlayerSystem.Selecting;

}
