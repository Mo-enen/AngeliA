using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Character")]
	[EntityAttribute.EntityCapacity(1)]
	public abstract class eCharacter : eYayaRigidbody, IDamageReceiver {




		#region --- VAR ---


		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public override bool InAir => base.InAir && !Movement.IsClimbing;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public override bool IgnoreRiseGravityShift => true;
		public int PassoutFrame { get; private set; } = int.MinValue;
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
		public MovementState MovementState { get; private set; } = MovementState.Idle;
		public int ArtworkOffsetZ { get => Renderer.ArtworkOffsetZ; set => Renderer.ArtworkOffsetZ = value; }
		public int ArtworkScale { get => Renderer.ArtworkScale; set => Renderer.ArtworkScale = value; }
		public override int CarrierSpeed => 0;

		// Beh
		public IActionEntity CurrentActionTarget => Action.CurrentTarget;
		public bool KeepTriggerAttackWhenHold => Attackness.KeepTriggerWhenHold;
		public bool CancelAttackOnJump => Attackness.CancelAttackOnJump;
		public bool IsAttacking => Attackness.IsAttacking;
		public virtual bool FacingFront => Movement.FacingFront;
		public virtual bool FacingRight => Movement.FacingRight;
		public bool UseFreeStyleSwim => Movement.UseFreeStyleSwim;
		public int IntendedX => Movement.IntendedX;
		public int IntendedY => Movement.IntendedY;
		public int LastMoveDirectionX => Movement.LastMoveDirection.x;
		public int LastMoveDirectionY => Movement.LastMoveDirection.y;
		public int AttackCombo => Attackness.Combo;
		public int LastGroundFrame => Movement.LastGroundFrame;
		public int LastSquatFrame => Movement.LastSquatFrame;
		public int LastSquatingFrame => Movement.LastSquatingFrame;
		public int LastPoundingFrame => Movement.LastPoundingFrame;
		public int HealthPoint => Health.HealthPoint;
		public int BodyArtworkID => Renderer.BodyArtworkID;

		// Data
		private Movement Movement = null;
		private Health Health = null;
		private Action Action = null;
		private Attackness Attackness = null;
		private CharacterRenderer Renderer = null;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {

			base.OnInitialize();
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			var game = Game.Current;
			Movement = game.LoadMeta<Movement>(typeName, "Movement") ?? new();
			Renderer = game.LoadMeta<CharacterRenderer>(typeName, "Renderer") ?? new();
			Action = game.LoadMeta<Action>(typeName, "Action") ?? new();
			Health = game.LoadMeta<Health>(typeName, "Health") ?? new();
			Attackness = game.LoadMeta<Attackness>(typeName, "Attackness") ?? new();
		}


		public override void OnActived () {
			base.OnActived();
			Movement.OnActived(this);
			Renderer.OnActived(this);
			Action.OnActived(this);
			Health.OnActived(this);
			Attackness.OnActived(this);
			CharacterState = CharacterState.GamePlay;
			PassoutFrame = int.MinValue;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {

			int frame = Game.GlobalFrame;

			// Passout Check
			if (CharacterState != CharacterState.Passout && Health.EmptyHealth) {
				SetCharacterState(CharacterState.Passout);
			}

			// Behaviour
			switch (CharacterState) {
				default:
				case CharacterState.GamePlay:
					if (frame < Health.LastDamageFrame + Health.DamageStunDuration) {
						// Tacking Damage
						InvokeAntiKnockback();
					} else if (Attackness.StopMoveOnAttack && frame < Attackness.LastAttackFrame + Attackness.Duration) {
						// Stop when Attacking
						if (IsGrounded) VelocityX = 0;
					} else {
						// Move as Normal
						Action.Update();
						Attackness.Update();
						Movement.Update();
					}
					base.PhysicsUpdate();
					break;
				case CharacterState.Animate:



					break;
				case CharacterState.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					if (!Health.FullHealth) InvokeHeal(Health.MaxHP);
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
					break;
			}

			MovementState =
				Movement.IsFlying ? MovementState.Fly :
				Movement.IsClimbing ? MovementState.Climb :
				Movement.IsPounding ? MovementState.Pound :
				Movement.IsRolling ? MovementState.Roll :
				Movement.IsDashing ? (!IsGrounded && InWater ? MovementState.SwimDash : MovementState.Dash) :
				Movement.IsSquating ? (Movement.IsMoving ? MovementState.SquatMove : MovementState.SquatIdle) :
				InWater && !IsGrounded ? (Movement.IsMoving ? MovementState.SwimMove : MovementState.SwimIdle) :
				InAir ? (FinalVelocityY > 0 ? MovementState.JumpUp : MovementState.JumpDown) :
				Movement.IsRunning ? MovementState.Run :
				Movement.IsMoving ? MovementState.Walk :
				MovementState.Idle;

		}


		public override void FrameUpdate () {
			Renderer.Update();
			base.FrameUpdate();
		}


		#endregion




		#region --- API ---


		// Virtual
		public virtual bool InvokeAction () => Action.Invoke();
		public virtual bool CancelAction () => Action.CancelInvoke();

		public virtual bool InvokeAttack () => Attackness.Attack();
		public virtual void CancelAttack () => Attackness.CancelAttack();
		public virtual bool CheckAttackReady (bool holding) => Attackness.CheckReady(holding);

		public virtual void InvokeMove (Direction3 x, Direction3 y) => Movement.Move(x, y);
		public virtual void InvokeMove (int x, int y) => Movement.Move(x, y);
		public virtual void InvokeJump () => Movement.Jump();
		public virtual void InvokeHoldJump (bool holding) => Movement.HoldJump(holding);
		public virtual void InvokeDash () => Movement.Dash();
		public virtual void InvokePound () => Movement.Pound();
		public virtual void InvokeAntiKnockback () => Movement.AntiKnockback();

		public virtual void InvokeDamage (int damage) {
			if (CharacterState != CharacterState.GamePlay) return;
			if (!Health.Damage(damage)) return;
			VelocityX = Movement.FacingRight ? -Health.KnockBackSpeed : Health.KnockBackSpeed;
			Renderer.Damage(Health.DamageStunDuration);
			if (!Health.EmptyHealth) {
				Renderer.Blink(Health.InvincibleFrame);
			}
		}
		public virtual bool InvokeHeal (int heal) => Health.Heal(heal);
		public virtual void InvokeSetHealth (int health) => Health.SetHealth(health);

		public virtual void InvokeBounce () => Renderer.Bounce();


		// Behavior
		public bool IsAttackAllowedByMovement () => (Attackness.AttackInAir || !InAir) &&
			(Attackness.AttackInWater || !InWater) &&
			(Attackness.AttackWhenClimbing || !Movement.IsClimbing) &&
			(Attackness.AttackWhenFlying || !Movement.IsFlying) &&
			(Attackness.AttackWhenRolling || !Movement.IsRolling) &&
			(Attackness.AttackWhenSquating || !Movement.IsSquating) &&
			(Attackness.AttackWhenDashing || !Movement.IsDashing);


		public void SetCharacterState (CharacterState state) {
			switch (state) {
				case CharacterState.GamePlay:
					if (CharacterState == CharacterState.Sleep) {
						X += Const.CELL_SIZE / 2;
					}
					CharacterState = CharacterState.GamePlay;
					Renderer.Bounce();
					Action.Update();
					break;
				case CharacterState.Animate:
					CharacterState = CharacterState.Animate;
					break;
				case CharacterState.Sleep:
					CharacterState = CharacterState.Sleep;
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
