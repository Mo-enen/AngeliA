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
	protected virtual bool DestroyBlocks => true;
	public Entity Sender { get; set; } = null;
	public int BreakObjectArtwork { get; set; }
	protected int ExplodedFrame { get; private set; } = -1;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		if (FromWorld) {
			X++;
			Y++;
		}
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
				receiver.TakeDamage(new Damage(Damage, Sender ?? this, this, Tag.ExplosiveDamage));
			}
			// Destroy Block
			if (DestroyBlocks) {
				bool procedural = Universe.BuiltInInfo.UseProceduralMap;
				for (int x = range.x; x < range.xMax; x += Const.CEL) {
					for (int y = range.y; y < range.yMax; y += Const.CEL) {
						if (!Util.OverlapRectCircle(
							Radius, X, Y,
							x + Const.HALF, y + Const.HALF, x + Const.HALF + 1, y + Const.HALF + 1
						)) continue;
						FrameworkUtil.PickBlockAt(
							(x + 1).ToUnit(),
							(y + 1).ToUnit(),
							allowMultiplePick: true,
							dropItemAfterPicked: procedural
						);
					}
				}
			}
			// Callback
			OnExplode(range);
		}
	}


	protected abstract void OnExplode (IRect range);


}