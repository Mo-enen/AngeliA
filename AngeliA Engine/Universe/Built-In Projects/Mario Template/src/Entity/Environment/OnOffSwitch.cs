using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class OnSwitch : OnOffSwitch {
	protected override bool ReverseOnOff => false;
}


public class OffSwitch : OnOffSwitch {
	protected override bool ReverseOnOff => true;
}


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class OnOffSwitch : Entity, IBumpable, IAutoTrackWalker {

	// VAR
	private const int COOL_DOWN = 12;
	private static readonly SpriteCode OnSprite = "OnSwitch";
	private static readonly SpriteCode OffSprite = "OffSwitch";
	public static bool CurrentOn { get; private set; } = true;
	private static int LastSwitchFrame = int.MinValue;
	protected abstract bool ReverseOnOff { get; }
	int IBumpable.LastBumpedFrame { get; set; } = int.MinValue;
	bool IBumpable.TransferWithAttack => true;
	Direction4 IBumpable.LastBumpFrom { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	// MSG
	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorModeChange (OnMapEditorModeChange_ModeAttribute.Mode mode) {
		CurrentOn = true;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, (this as IAutoTrackWalker).OnTrack);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		var cell = Renderer.Draw(CurrentOn == ReverseOnOff ? OffSprite : OnSprite, Rect);
		IBumpable.AnimateForBump(this, cell);
	}

	public static void TrySwitch () {
		if (Game.GlobalFrame < LastSwitchFrame + COOL_DOWN) return;
		LastSwitchFrame = Game.GlobalFrame;
		CurrentOn = !CurrentOn;
	}

	void IBumpable.OnBumped (Rigidbody rig) => TrySwitch();

	bool IBumpable.AllowBump (Rigidbody rig) => rig == PlayerSystem.Selecting;

}
