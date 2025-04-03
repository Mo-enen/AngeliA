using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity that bounce the touching targets
/// </summary>
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.RepositionWhenInactive]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Spring : Rigidbody, IBlockEntity, IAutoTrackWalker {


	// Const
	private static readonly int[] BOUNCE_ANI = [0, 1, 2, 3, 3, 3, 3, 2, 2, 1, 1, 0,];

	// Api
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	/// <summary>
	/// True if the spring bounce horizontaly
	/// </summary>
	protected abstract bool Horizontal { get; }
	/// <summary>
	/// Initial speed this spring gives when bounce
	/// </summary>
	protected abstract int Power { get; }
	/// <summary>
	/// Current rotation of the artwork sprite
	/// </summary>
	public int ArtworkRotation { get; set; } = 0;
	public override bool AllowBeingPush => !Horizontal;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

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
				PhysicsMask.DYNAMIC, Rect.EdgeOutsideLeft(1), this, OperationMode.ColliderAndTrigger
			)) {
				PerformBounce(Direction4.Left);
			}
			if (Physics.Overlap(
				PhysicsMask.DYNAMIC, Rect.EdgeOutsideRight(1), this, OperationMode.ColliderAndTrigger
			)) {
				PerformBounce(Direction4.Right);
			}
		} else {
			// Vertical
			if (Physics.Overlap(
				PhysicsMask.DYNAMIC, Rect.EdgeOutsideUp(1), this, OperationMode.ColliderAndTrigger
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
		if (FrameworkUtil.PerformSpringBounce(this, side, Power, 0) || forceBounce) {
			LastBounceFrame = Game.GlobalFrame;
		}
	}


}
