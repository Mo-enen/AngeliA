using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// General representation of an explosion
/// </summary>
[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Explosion : Entity {


	// Api
	/// <summary>
	/// Cells in which physics layers will be effect by the explosion
	/// </summary>
	protected virtual int CollisionMask => PhysicsMask.DYNAMIC;
	
	/// <summary>
	/// How many frames does this explosion exists in stage
	/// </summary>
	protected virtual int Duration => 10;
	
	/// <summary>
	/// How many damage does this explosion deal to the targets
	/// </summary>
	protected virtual int Damage => 1;
	
	/// <summary>
	/// Color tint of the wave sprite
	/// </summary>
	protected virtual Color32 WaveColor => new(255, 255, 255, 255);
	
	/// <summary>
	/// Color tint of the ring sprite
	/// </summary>
	protected virtual Color32 RingColor => new(255, 0, 0, 255);

	/// <summary>
	/// Color tint of the fire sprite
	/// </summary>
	protected virtual Color32 FireColor => new(255, 255, 0, 255);
	
	/// <summary>
	/// True if this explosion break map blocks
	/// </summary>
	protected virtual bool DestroyBlocks => true;

	/// <summary>
	/// Size of the explosion in global space
	/// </summary>
	public int Radius { get; set; } = Const.CEL * 2 + Const.HALF;
	
	/// <summary>
	/// Which entity create this explosion
	/// </summary>
	public Entity Sender { get; set; } = null;
	
	/// <summary>
	/// Artwork sprite ID for object break callback
	/// </summary>
	public int BreakObjectArtwork { get; set; }
	
	protected int ExplodedFrame { get; private set; } = -1;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		X = X.ToUnifyGlobal() + Const.HALF;
		Y = Y.ToUnifyGlobal() + Const.HALF;
		Sender = null;
		BreakObjectArtwork = 0;
		ExplodedFrame = -1;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (ExplodedFrame >= 0 && Game.GlobalFrame >= ExplodedFrame + Duration) {
			Active = false;
			return;
		}
	}


	public override void Update () {
		base.Update();
		if (!Active) return;

		// Explode
		if (ExplodedFrame < 0 && (!FromWorld || Stage.ViewRect.Shrink(Const.CEL * 0).Overlaps(Rect))) {
			ExplodedFrame = Game.GlobalFrame;
			var range = new IRect(X - Radius, Y - Radius, Radius * 2, Radius * 2);
			var hits = Physics.OverlapAll(
				CollisionMask,
				range,
				out int count,
				null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				if (hits[i].Entity is not IDamageReceiver receiver) continue;
				if (receiver is Entity e && !e.Active) continue;
				var hitRect = hits[i].Rect;
				if (!Util.OverlapRectCircle(Radius, X, Y, hitRect.xMin, hitRect.yMin, hitRect.xMax, hitRect.yMax)) continue;
				receiver.TakeDamage(new Damage(Damage, bullet: this, type: Tag.ExplosiveDamage));
			}

			// Destroy Block
			if (DestroyBlocks) {
				for (int x = range.x; x <= range.xMax; x += Const.CEL) {
					for (int y = range.y; y <= range.yMax; y += Const.CEL) {
						if (!Util.OverlapRectCircle(
							Radius, X, Y,
							x + Const.HALF, y + Const.HALF, x + Const.HALF + 1, y + Const.HALF + 1
						)) continue;
						FrameworkUtil.PickBlockAt(
							(x + 1).ToUnit(),
							(y + 1).ToUnit(),
							allowMultiplePick: true,
							dropItemAfterPicked: true
						);
					}
				}
			}

			// Callback
			OnExplode(range);

			// On Break
			if (BreakObjectArtwork != 0) {
				FrameworkUtil.InvokeObjectBreak(BreakObjectArtwork, new IRect(X, Y, Const.CEL, Const.CEL));
			}

			// Remove from World
			if (FromWorld) {
				FrameworkUtil.RemoveFromWorldMemory(this);
			}

		}
	}


	/// <summary>
	/// This function is called the this explosion explode
	/// </summary>
	/// <param name="range">(in global space)</param>
	protected abstract void OnExplode (IRect range);


}