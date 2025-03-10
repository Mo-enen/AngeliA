using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

public class Coin : Entity, IBumpable {

	// VAR
	public static readonly int TYPE_ID = typeof(Coin).AngeHash();
	public static int CurrentCoinCount { get; private set; } = 0;
	int IBumpable.LastBumpedFrame { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }

	// MSG
	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, !PSwitch.Triggering);
	}

	public override void Update () {
		base.Update();
		// Touch to Collect
		var player = PlayerSystem.Selecting;
		if (player != null && player.Rect.Overlaps(Rect)) {
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

	void IBumpable.OnBumped (Rigidbody rig) { }

	bool IBumpable.AllowBump (Rigidbody rig) => PSwitch.Triggering;

}
