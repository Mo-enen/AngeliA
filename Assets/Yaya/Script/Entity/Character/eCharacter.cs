using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public enum CharacterState {
		GamePlay = 0,
		Sleep = 1,
		Passout = 2,
	}


	[EntityAttribute.MapEditorGroup("Character")]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL)]
	public abstract class eCharacter : eYayaRigidbody, IDamageReceiver {




		#region --- VAR ---


		// Api
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override bool IgnoreRiseGravityShift => true;
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public override bool InAir => base.InAir && !Movement.IsClimbing;
		public override int CarrierSpeed => 0;
		public bool TakingDamage => Game.GlobalFrame < Health.LastDamageFrame + Health.DamageStunDuration;
		public int SleepAmount {
			get => Util.Remap(0, 90, 0, 1000, SleepFrame);
			set => SleepFrame = Util.Remap(0, 1000, 0, 90, value);
		}
		public int PassoutFrame { get; private set; } = int.MinValue;
		public int SleepFrame { get; private set; } = 0;
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
		public MovementState MovementState { get; protected set; } = MovementState.Idle;
		public Movement Movement { get; private set; } = null;
		public Health Health { get; private set; } = null;
		public Action Action { get; private set; } = null;
		public Attackness Attackness { get; private set; } = null;
		public CharacterRenderer Renderer { get; private set; } = null;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {

			base.OnInitialize();
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			var game = Game.Current;
			Movement = game.LoadOrCreateMeta<Movement>(typeName, "Movement");
			Renderer = game.LoadOrCreateMeta<CharacterRenderer>(typeName, "Renderer");
			Action = game.LoadOrCreateMeta<Action>(typeName, "Action");
			Health = game.LoadOrCreateMeta<Health>(typeName, "Health");
			Attackness = game.LoadOrCreateMeta<Attackness>(typeName, "Attackness");

			Movement.OnInitialize(this);
			Renderer.OnInitialize(this);
			Action.OnInitialize(this);
			Health.OnInitialize(this);
			Attackness.OnInitialize(this);

		}


		public override void OnActived () {
			base.OnActived();
			Movement.OnActived();
			Renderer.OnActived();
			Action.OnActived();
			Health.OnActived();
			Attackness.OnActived();
			CharacterState = CharacterState.GamePlay;
			PassoutFrame = int.MinValue;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {

			// Passout Check
			if (Health.EmptyHealth) SetCharacterState(CharacterState.Passout);

			// Behaviour
			MovementState = MovementState.Idle;
			switch (CharacterState) {
				default:
				case CharacterState.GamePlay:
					if (TakingDamage) {
						// Tacking Damage
						Movement.AntiKnockback();
					} else if (Attackness.StopMoveOnAttack && Attackness.IsAttacking) {
						// Stop when Attacking
						if (IsGrounded) VelocityX = 0;
					} else {
						// Move as Normal
						Action.Update();
						Attackness.Update();
						Movement.Update();
						MovementState = Movement.State;
					}
					base.PhysicsUpdate();
					break;
				case CharacterState.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					Width = Const.CEL;
					Height = Const.CEL;
					OffsetX = -Const.CEL / 2;
					OffsetY = 0;
					SleepFrame++;
					if (!Health.FullHealth && SleepFrame >= 1000) Health.Heal(Health.MaxHP);
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
					break;
			}

		}


		public override void FrameUpdate () {
			Renderer.FrameUpdate();
			base.FrameUpdate();
		}


		#endregion




		#region --- API ---


		// Virtual
		public virtual void TakeDamage (int damage) {
			if (CharacterState != CharacterState.GamePlay) return;
			if (!Health.Damage(damage)) return;
			VelocityX = Movement.FacingRight ? -Health.KnockBackSpeed : Health.KnockBackSpeed;
			Renderer.Damage(Health.DamageStunDuration);
			if (!Health.EmptyHealth) {
				Renderer.Blink(Health.InvincibleFrame);
			}
		}


		// Behavior
		public bool IsAttackAllowedByMovement () =>
			(Attackness.AttackInAir || !InAir) &&
			(Attackness.AttackInWater || !InWater) &&
			(Attackness.AttackWhenClimbing || !Movement.IsClimbing) &&
			(Attackness.AttackWhenFlying || !Movement.IsFlying) &&
			(Attackness.AttackWhenRolling || !Movement.IsRolling) &&
			(Attackness.AttackWhenSquating || !Movement.IsSquating) &&
			(Attackness.AttackWhenDashing || !Movement.IsDashing);


		public void SetCharacterState (CharacterState state) {
			if (CharacterState == state) return;
			PassoutFrame = int.MinValue;
			switch (state) {
				case CharacterState.GamePlay:
					if (CharacterState == CharacterState.Sleep) {
						Renderer.Bounce();
					}
					CharacterState = CharacterState.GamePlay;
					Action.Update();
					break;
				case CharacterState.Sleep:
					CharacterState = CharacterState.Sleep;
					SleepFrame = 0;
					break;
				case CharacterState.Passout:
					CharacterState = CharacterState.Passout;
					PassoutFrame = Game.GlobalFrame;
					break;
				default:
					throw new System.NotImplementedException();
			}
		}


		// Misc
		protected override bool InsideGroundCheck () => CellPhysics.Overlap(YayaConst.MASK_LEVEL, new(X, Y + Height / 4, 1, 1), this);


		#endregion



	}
}
