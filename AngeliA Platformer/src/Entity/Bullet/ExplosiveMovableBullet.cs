using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class ExplosiveMovableBullet : MovableBullet {
	public override int Duration => 600;
	protected virtual int Radius => Const.CEL * 2;
	protected virtual int ExplosionDuration => 10;
	protected abstract int ExplosionID { get; }
	public override void OnActivated () {
		base.OnActivated();
		Damage.Override(0);
	}
	protected override void BeforeDespawn (IDamageReceiver receiver) {
		if (Active) return;
		if (ExplosionID != 0 && Stage.SpawnEntity(ExplosionID, X + Width / 2, Y + Height / 2) is Explosion exp) {
			exp.Sender = Sender;
			exp.BreakObjectArtwork = TypeID;
		}
	}
}