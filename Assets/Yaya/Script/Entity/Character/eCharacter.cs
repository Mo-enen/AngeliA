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
		public override int PushLevel => YayaConst.PUSH_CHARACTER;
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override bool IgnoreRiseGravityShift => true;
		public int PassoutFrame { get; private set; } = int.MinValue;
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
		public MovementState MovementState { get; private set; } = MovementState.Idle;
		public int ArtworkOffsetZ { get => Renderer.ArtworkOffsetZ; set => Renderer.ArtworkOffsetZ = value; }
		public int ArtworkScale { get => Renderer.ArtworkScale; set => Renderer.ArtworkScale = value; }
		public override int CarrierSpeed => 0;

		// Data
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
						Movement.AntiKnockback();
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
					if (!Health.FullHealth) Health.Heal(Health.MaxHP);
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
