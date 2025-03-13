using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public abstract class Enemy : Rigidbody, IBumpable, IDamageReceiver {

	public bool IsPassout => PassoutFrame >= 0;
	protected virtual bool AllowPlayerStepOn => true;
	protected virtual bool AttackOnTouchPlayer => true;
	protected virtual int PlayerStepOnCooldown => 6;
	public override int PhysicalLayer => PhysicsLayer.CHARACTER;
	public override int CollisionMask => PhysicsMask.MAP;
	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	bool IBumpable.TransferBumpToOther => false;
	bool IBumpable.TransferBumpFromOther => true;
	bool IBumpable.FromBelow => false;
	int IBumpable.LastBumpedFrame { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }
	private int LastPlayerStepOnFrame = int.MinValue;
	private int PassoutFrame = -1;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		PassoutFrame = -1;
		LastPlayerStepOnFrame = int.MinValue;
	}

	public override void Update () {
		base.Update();

		// Passout Check
		if (PassoutFrame >= 0) {
			if (Game.GlobalFrame > PassoutFrame + 30) {
				Active = false;
			}
			VelocityX = 0;
			VelocityY = 0;
			IgnorePhysics.True(1);
			FillAsTrigger(1);
			return;
		}

		// Player Interaction Check
		if (AllowPlayerStepOn || AttackOnTouchPlayer) {
			var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, Rect, out int count, this);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];

				if (
					hit.Entity is not Character ch ||
					ch.CharacterState != CharacterState.GamePlay ||
					ch.Team != Const.TEAM_PLAYER
				) continue;

				if (AllowPlayerStepOn && (ch.VelocityY < 0 || ch.Y >= Y + Height / 2)) {
					// Step On
					if (Game.GlobalFrame > LastPlayerStepOnFrame + PlayerStepOnCooldown) {

						LastPlayerStepOnFrame = Game.GlobalFrame;
						OnPlayerStepOn(ch);
					}
				} else if (AttackOnTouchPlayer) {
					// Attack
					AttackPlayer(ch as IDamageReceiver);
				}
			}
		}

	}

	protected virtual void OnPlayerStepOn (Character player) {
		player.VelocityY = 64;
		PassoutFrame = Game.GlobalFrame;
		MarioUtil.PlayMarioAudio(Sound.StepOnEnemy, XY);
	}

	protected virtual void AttackPlayer (IDamageReceiver player) => player.TakeDamage(new Damage(1));

	bool IBumpable.AllowBump (Rigidbody rig) => false;

	void IBumpable.OnBumped (Rigidbody rig) { }

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (damage.Amount <= 0) return;
		PassoutFrame = Game.GlobalFrame;
	}

}
