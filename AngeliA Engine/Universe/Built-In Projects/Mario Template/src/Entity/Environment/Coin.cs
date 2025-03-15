using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class Coin : Rigidbody, IBumpable, IAutoTrackWalker {

	// VAR
	public override int CollisionMask => PhysicsMask.MAP;
	public static readonly int TYPE_ID = typeof(Coin).AngeHash();
	public static int CurrentCoinCount { get; private set; } = 0;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int AirDragX => 0;
	public bool IsLoose { get; set; } = false;
	int IBumpable.LastBumpedFrame { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsLoose = false;
		BounceSpeedRate = 800;
	}

	public override void FirstUpdate () {
		if (!IsLoose) {
			if (!PSwitch.Triggering || (this as IAutoTrackWalker).OnTrack) {
				FillAsTrigger(1);
				IgnorePhysics.True(1);
			}
			IgnoreGravity.True(1);
		} else {
			FillAsTrigger(1);
		}
		base.FirstUpdate();
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

	void IBumpable.OnBumped (Rigidbody rig, Damage damage) {
		if (PSwitch.Triggering && damage.Amount > 0 && damage.Type.HasAll(Tag.MagicalDamage)) {
			Active = false;
			FrameworkUtil.InvokeObjectBreak(BrickBlock.TYPE_ID, Rect);
		}
	}

	bool IBumpable.AllowBump (Rigidbody rig, Direction4 from) =>
		IBumpable.IsValidBumpDirection(this, from) &&
		rig == PlayerSystem.Selecting &&
		PSwitch.Triggering;

}
