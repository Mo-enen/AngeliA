using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Explosion : Entity {


	// Api
	protected virtual int CollisionMask => PhysicsMask.DYNAMIC;
	protected virtual int Duration => 10;
	protected virtual int Damage => 1;
	protected virtual int Radius => Const.CEL * 2;
	protected virtual Color32 WaveColor => new(255, 255, 255, 255);
	protected virtual Color32 RingColor => new(255, 0, 0, 255);
	protected virtual Color32 FireColor => new(255, 255, 0, 255);
	public Entity Sender { get; set; } = null;
	public int BreakObjectArtwork { get; set; }
	protected int ExplodedFrame { get; private set; } = -1;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		if (FromWorld) {
			X += Const.HALF;
			Y += Const.HALF;
		}
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
			var hits = Physics.OverlapAll(
				CollisionMask,
				new IRect(X - Radius, Y - Radius, Radius * 2, Radius * 2),
				out int count,
				null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				if (hits[i].Entity is not IDamageReceiver receiver) continue;
				if (receiver is Entity e && !e.Active) continue;
				var hitRect = hits[i].Rect;
				if (!Util.OverlapRectCircle(Radius, X, Y, hitRect.xMin, hitRect.yMin, hitRect.xMax, hitRect.yMax)) continue;
				receiver.TakeDamage(new Damage(Damage, Sender ?? this, this, Tag.ExplosiveDamage));
			}
			OnExplode();
		}
	}


	protected abstract void OnExplode ();


}