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
	protected int ArtworkRotation { get; set; } = 0;
	protected int ArtworkID { get; set; }
	public override bool AllowBeingPush => !Horizontal;

	// Data
	private int LastBounceFrame = int.MinValue;
	private int CurrentArtworkFrame = 0;
	private Direction4 BounceSide = default;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		LastBounceFrame = int.MinValue;
		BounceSide = default;
		ArtworkID = TypeID;
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
		if (Renderer.TryGetSpriteFromGroup(ArtworkID, BOUNCE_ANI[frame], out var sprite, false, true)) {
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
		bool bounced = forceBounce;
		var globalRect = Rect.EdgeOutside(side, 16);
		Entity ignore = this;
		for (int safe = 0; safe < 2048; safe++) {
			var hits = Physics.OverlapAll(PhysicsMask.ENTITY, globalRect, out int count, ignore, OperationMode.ColliderAndTrigger);
			if (count == 0) break;
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				bounced = true;
				var hitRect = hit.Entity.Rect;
				if (Horizontal) {
					globalRect.y = hitRect.y;
					if (side == Direction4.Left) {
						globalRect.x = Util.Min(globalRect.x, hitRect.x - globalRect.width);
					} else {
						globalRect.x = Util.Max(globalRect.x, hitRect.xMax);
					}
				} else {
					globalRect.x = hitRect.x;
					globalRect.y = Util.Max(globalRect.y, hitRect.yMax);
				}
				ignore = hit.Entity;
				PerformBounce(rig);
				break;
			}
		}
		if (bounced) {
			LastBounceFrame = Game.GlobalFrame;
			BounceSide = side;
		}
	}


	private void PerformBounce (Rigidbody target) {
		if (target == null) return;
		if (Horizontal) {
			// Horizontal
			if (BounceSide == Direction4.Left) {
				if (target.VelocityX > -Power) {
					target.VelocityX = -Power;
					if (target is IWithCharacterMovement wMov) {
						wMov.CurrentMovement.FacingRight = false;
					}
				}
			} else {
				if (target.VelocityX < Power) {
					target.VelocityX = Power;
					if (target is IWithCharacterMovement wMov) {
						wMov.CurrentMovement.FacingRight = true;
					}
				}
			}
		} else {
			// Vertical
			if (target.VelocityY < Power) target.VelocityY = Power;
			target.MakeGrounded(6);
		}
	}


}
