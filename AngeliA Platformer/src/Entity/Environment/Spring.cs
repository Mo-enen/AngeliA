using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.RepositionWhenInactive]
public abstract class Spring : Rigidbody, IBlockEntity {


	// Const
	private static readonly int[] BOUNCE_ANI = [0, 1, 2, 3, 3, 3, 3, 2, 2, 1, 1, 0,];

	// Api
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	protected abstract bool Horizontal { get; }
	protected abstract int Power { get; }
	public int ArtworkRotation { get; set; } = 0;
	public override bool AllowBeingPush => !Horizontal;

	// Data
	private int LastBounceFrame = int.MinValue;
	private int CurrentArtworkFrame = 0;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		LastBounceFrame = int.MinValue;
		ArtworkRotation = 0;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Check for Bounce
		if (Horizontal) {
			// Horizontal
			if (Physics.Overlap(
				PhysicsMask.DYNAMIC, Rect.EdgeOutside(Direction4.Left, 1), this, OperationMode.ColliderAndTrigger
			)) {
				PerformBounce(Direction4.Left);
			}
			if (Physics.Overlap(
				PhysicsMask.DYNAMIC, Rect.EdgeOutside(Direction4.Right, 1), this, OperationMode.ColliderAndTrigger
			)) {
				PerformBounce(Direction4.Right);
			}
		} else {
			// Vertical
			if (Physics.Overlap(
				PhysicsMask.DYNAMIC, Rect.EdgeOutside(Direction4.Up, 1), this, OperationMode.ColliderAndTrigger
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
				X + Width / 2,
				Y + Height / 2,
				500, 500, ArtworkRotation,
				Const.CEL, Const.CEL, Color32.WHITE
			);
		}
		ArtworkRotation = ArtworkRotation.LerpTo(0, 0.3f);
	}


	// LGC
	private void PerformBounce (Direction4 side, bool forceBounce = false) {
		if (FrameworkUtil.PerformSpringBounce(Rect, side, Power, this) || forceBounce) {
			LastBounceFrame = Game.GlobalFrame;
		}
	}


}
