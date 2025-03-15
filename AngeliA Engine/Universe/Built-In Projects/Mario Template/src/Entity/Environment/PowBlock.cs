using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class PowBlock : Rigidbody, IDamageReceiver, IBumpable {

	// VAR
	public static readonly int TYPE_ID = typeof(PowBlock).AngeHash();
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	int IBumpable.LastBumpedFrame { get; set; }
	bool IBumpable.TransferBumpFromOther => true;
	Direction4 IBumpable.LastBumpFrom { get; set; }

	// MSG
	public override void LateUpdate () {
		base.LateUpdate();
		Draw();
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (!Active || damage.Amount <= 0) return;
		Explode();
	}

	public void Explode () {
		Active = false;
		PowExpodeParticle.Spawn(X + Width / 2, Y + Height / 2);
		// Attack All Enemy on Stage
		if (Stage.TryGetEntities(EntityLayer.GAME, out var entities, out int count)) {
			for (int i = 0; i < count; i++) {
				var entity = entities[i];
				if (entity is not Enemy enemy) continue;
				if (entity is not IDamageReceiver receiver) continue;
				if (!enemy.IsGrounded) continue;
				receiver.TakeDamage(new Damage(1, Const.TEAM_ENEMY, type: Tag.MagicalDamage));
			}
		}
		// Drop All Coins on Stage
		if (!PSwitch.Triggering && Stage.TryGetEntities(EntityLayer.ENVIRONMENT, out entities, out count)) {
			for (int i = 0; i < count; i++) {
				var entity = entities[i];
				if (entity is not Coin coin) continue;
				coin.IsLoose = true;
				coin.VelocityX = Util.QuickRandom(-12, 12);
				coin.MomentumY.value = Util.QuickRandom(12, 24);
			}
		}
	}

	bool IBumpable.AllowBump (Rigidbody rig, Direction4 from) => IBumpable.IsValidBumpDirection(this, from) && rig == PlayerSystem.Selecting;

	void IBumpable.OnBumped (Rigidbody rig, Damage damage) => Explode();

}
