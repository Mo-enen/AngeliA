using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 
public abstract class ExplosiveMovableBullet : MovableBullet {
	protected override int Duration => 600;
	protected override int Damage => 0;
	protected override int SpawnWidth => Const.CEL;
	protected override int SpawnHeight => Const.CEL;
	protected virtual int Radius => Const.CEL * 2;
	protected virtual int ExplosionDuration => 10;
	protected virtual int ExplosionID => Explosion.TYPE_ID;
	protected override void BeforeDespawn (IDamageReceiver receiver) {
		if (Active) return;
		if (Stage.SpawnEntity(ExplosionID, X + Width / 2, Y + Height / 2) is Explosion exp) {
			exp.Sender = Sender;
			exp.BreakObjectArtwork = TypeID;
		}
	}
}