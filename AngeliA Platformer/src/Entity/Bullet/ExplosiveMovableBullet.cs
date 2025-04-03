using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Movable bullet that spawn an explosion entity when despawn
/// </summary>
public abstract class ExplosiveMovableBullet : MovableBullet {
	public override int Duration => 600;
	/// <summary>
	/// Radius in global space for the explosion.
	/// </summary>
	protected virtual int ExplosionRadius => Const.CEL * 2 + Const.HALF;
	/// <summary>
	/// Duration of the explosion in frame
	/// </summary>
	protected virtual int ExplosionDuration => 10;
	/// <summary>
	/// Type ID of the explosion
	/// </summary>
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
			exp.Radius = ExplosionRadius;
		}
	}
}