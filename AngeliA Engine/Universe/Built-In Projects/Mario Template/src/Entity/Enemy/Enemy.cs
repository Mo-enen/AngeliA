using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.MapEditorGroup(nameof(Enemy))]
public abstract class Enemy : Rigidbody, IDamageReceiver {


	// VAR
	private const int PASS_COUNT_DELAY = 30;
	private static readonly int FCT_FONT = "SuperMarioBros".AngeHash();
	private static readonly IntToChars StepCountToChar = new();
	public bool IsPassout => PassoutFrame != int.MinValue;
	protected abstract bool AllowPlayerStepOn { get; }
	protected abstract bool AttackOnTouchPlayer { get; }
	protected virtual int PlayerStepOnCooldown => 6;
	protected virtual bool DelayPassoutOnStep => true;
	public override int PhysicalLayer => PhysicsLayer.CHARACTER;
	public override int SelfCollisionMask => PhysicsMask.MAP;
	public override int AirDragX => IsGrounded ? 1 : 0;
	public override bool CarryOtherOnTop => false;
	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	protected int LastPlayerStepOnFrame { get; private set; } = int.MinValue;
	private int PassoutFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		PassoutFrame = int.MinValue;
		LastPlayerStepOnFrame = int.MinValue;
	}

	public override void Update () {
		base.Update();

		// Passout Check
		if (PassoutFrame != int.MinValue) {
			if (Game.GlobalFrame > PassoutFrame + PASS_COUNT_DELAY) {
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
			var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, Rect.ExpandHorizontal(2), out int count, this);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];

				if (
					hit.Entity is not PlayableCharacter ch ||
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

	protected virtual void OnPlayerStepOn (PlayableCharacter player) {
		player.VelocityY = 64;
		if (DelayPassoutOnStep) {
			PassoutFrame = Game.GlobalFrame;
		} else {
			MakePassout();
		}
		int score = 100 + player.CurrentStepCombo * 100;
		MarioUtil.PlayMarioAudio(Sound.StepOnEnemy, XY);
		MarioUtil.GiveScore(score);
		FloatingCombatText.Spawn(
			CenterX, Y + Height,
			StepCountToChar.GetChars(score),
			fontID: FCT_FONT,
			style: GUI.Skin.SmallCenterLabel
		);
		player.CurrentStepCombo++;
	}

	protected virtual void AttackPlayer (IDamageReceiver player) => player.TakeDamage(new Damage(1));

	public void MakePassout (int spriteID = int.MinValue) {
		PassoutFrame = Game.GlobalFrame - PASS_COUNT_DELAY - 1;
		FrameworkUtil.InvokeObjectFreeFall(
			spriteID != int.MinValue ? spriteID : TypeID,
			X + Width / 2, Y + Height / 2,
			speedX: Util.QuickRandomSign() * 32,
			speedY: 82,
			rotationSpeed: Util.QuickRandomSign() * 8
		);
		Active = false;
	}

	public virtual void OnDamaged (Damage damage) {
		if (damage.Amount <= 0) return;
		MakePassout();
	}


}
