using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class Coin : Rigidbody, IBumpable, IAutoTrackWalker {

	// VAR
	private static readonly AudioCode COIN_AC = "Coin";
	private static readonly AudioCode BUMP_AC = "Bump";
	public override int SelfCollisionMask => PhysicsMask.MAP;
	public static readonly int TYPE_ID = typeof(Coin).AngeHash();
	public static int CurrentCoinCount { get; private set; } = 0;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int AirDragX => 0;
	public bool IsLoose => LooseFrame >= 0;
	int IBumpable.LastBumpedFrame { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }
	private int LooseFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		LooseFrame = int.MinValue;
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
		// Loose Despawn Check
		if (IsLoose && Game.GlobalFrame > LooseFrame + 240) {
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		// Draw
		if (PSwitch.Triggering && !IsLoose) {
			// As Block
			var cell = Renderer.Draw(BrickBlock.TYPE_ID, Rect);
			IBumpable.AnimateForBump(this, cell);
		} else {
			// As Coin
			if (IsLoose && Game.GlobalFrame > LooseFrame + 120) {
				// Blink Coin
				if (Game.GlobalFrame % 8 < 4) {
					Draw();
				}
			} else {
				// Normal Coin
				Draw();
			}
		}
	}

	void IBumpable.OnBumped (Entity entity, Damage damage) {
		if (!PSwitch.Triggering) return;
		if (damage.Amount > 0 && damage.Type.HasAll(Tag.MagicalDamage)) {
			// Break
			Active = false;
			FrameworkUtil.InvokeObjectBreak(BrickBlock.TYPE_ID, Rect);
		} else {
			// Bump
			Game.PlaySoundAtPosition(BUMP_AC, XY);
		}
	}

	bool IBumpable.AllowBump (Entity entity, Direction4 from) =>
		IBumpable.IsValidBumpDirection(this, from) &&
		entity == PlayerSystem.Selecting &&
		PSwitch.Triggering;

	// API
	public void MakeLoose () => LooseFrame = Game.GlobalFrame;

	public static void Collect (int count) {
		CurrentCoinCount++;
		Game.PlaySound(COIN_AC, 0.5f);
	}

	public static void ResetCoinCount () => CurrentCoinCount = 0;

}
