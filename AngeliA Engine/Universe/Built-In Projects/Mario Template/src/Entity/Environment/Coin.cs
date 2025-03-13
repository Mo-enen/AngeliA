using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class Coin : Entity, IBumpable, IAutoTrackWalker {

	// VAR
	public static readonly int TYPE_ID = typeof(Coin).AngeHash();
	public static int CurrentCoinCount { get; private set; } = 0;
	int IBumpable.LastBumpedFrame { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }

	// MSG
	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(
			PhysicsLayer.ENVIRONMENT, this,
			!PSwitch.Triggering || (this as IAutoTrackWalker).OnTrack
		);
	}

	public override void Update () {
		base.Update();
		// Touch to Collect
		var player = PlayerSystem.Selecting;
		if (!PSwitch.Triggering && player != null && player.Rect.Overlaps(Rect)) {
			Collect(1);
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		if (PSwitch.Triggering) {
			// As Block
			var cell = Renderer.Draw(BrickBlock.TYPE_ID, Rect);
			IBumpable.AnimateForBump(this, cell);
		} else {
			// As Coin
			Draw();
		}
	}

	public static void Collect (int count) {
		CurrentCoinCount++;
	}

	public static void ResetCoinCount () => CurrentCoinCount = 0;

	void IBumpable.OnBumped (Rigidbody rig) { }

	bool IBumpable.AllowBump (Rigidbody rig) => PSwitch.Triggering;

}
