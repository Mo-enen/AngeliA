using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public enum CharacterState {
		GamePlay = 0,
		InVehicle,
		Sleep,
		Passout,
	}


	[EntityAttribute.MapEditorGroup("Character")]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL)]
	public abstract partial class eCharacter : eYayaRigidbody, IDamageReceiver {




		#region --- VAR ---


		// Api
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override bool IgnoreRiseGravityShift => true;
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => IsGrabFliping ? 0 : YayaConst.MASK_MAP;
		public override int CarrierSpeed => 0;
		public bool TakingDamage => Game.GlobalFrame < LastDamageFrame + DamageStunDuration;
		public int SleepAmount {
			get => Util.Remap(0, 90, 0, 1000, SleepFrame);
			set => SleepFrame = Util.Remap(0, 1000, 0, 90, value);
		}
		public int PassoutFrame { get; private set; } = int.MinValue;
		public int SleepFrame { get; protected set; } = 0;
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;

		// Data
		private int PrevSleepAmount = 0;


		#endregion




		#region --- MSG ---


		protected eCharacter () => OnInitialize_Render();


		public override void OnActived () {
			base.OnActived();
			OnActived_Movement();
			OnActived_Action();
			OnActived_Health();
			OnActived_Attack();
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
			if (IsEmptyHealth) SetCharacterState(CharacterState.Passout);

			// Behaviour
			MoveState = MovementState.Idle;
			switch (CharacterState) {
				default:
				case CharacterState.GamePlay:
					if (TakingDamage) {
						// Tacking Damage
						AntiKnockback();
					} else {
						// General
						Update_Action();
						Update_Attack();
						Update_Movement();
						// Stop when Attacking
						if (StopMoveOnAttack && IsAttacking && IsGrounded) {
							VelocityX = 0;
						}
					}
					base.PhysicsUpdate();
					break;
				case CharacterState.InVehicle:



					break;
				case CharacterState.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					Width = Const.CEL;
					Height = Const.CEL;
					OffsetX = -Const.CEL / 2;
					OffsetY = 0;
					SleepFrame++;
					if (!IsFullHealth && SleepAmount >= 1000) SetHealth(MaxHP);
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
					break;
			}

		}


		public override void FrameUpdate () {
			FrameUpdate_Renderer();
			PrevSleepAmount = SleepAmount;
			base.FrameUpdate();
		}


		#endregion




		#region --- API ---


		// Virtual
		public virtual void TakeDamage (int damage) {

			if (
				CharacterState != CharacterState.GamePlay || damage <= 0 ||
				Invincible || HealthPoint <= 0
			) return;

			// Health Down
			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);
			InvincibleStartFrame = Game.GlobalFrame;
			LastDamageFrame = Game.GlobalFrame;

			// Render
			VelocityX = FacingRight ? -KnockBackSpeed : KnockBackSpeed;
			RenderDamage(DamageStunDuration);
			if (!IsEmptyHealth) {
				RenderBlink(InvincibleFrame);
			}
		}


		// Behavior
		public bool IsAttackAllowedByMovement () =>
			(AttackInAir || (IsGrounded || InWater || InSand || IsClimbing)) &&
			(AttackInWater || !InWater) &&
			(AttackWhenMoving || IntendedX == 0) &&
			(AttackWhenClimbing || !IsClimbing) &&
			(AttackWhenFlying || !IsFlying) &&
			(AttackWhenRolling || !IsRolling) &&
			(AttackWhenSquating || !IsSquating) &&
			(AttackWhenDashing || !IsDashing) &&
			(AttackWhenSliding || !IsSliding) &&
			(AttackWhenGrabing || (!IsGrabingTop && !IsGrabingSide));


		public void SetCharacterState (CharacterState state) {
			if (CharacterState == state) return;
			PassoutFrame = int.MinValue;
			switch (state) {
				case CharacterState.GamePlay:
					if (CharacterState == CharacterState.Sleep) {
						RenderBounce();
					}
					CharacterState = CharacterState.GamePlay;
					Update_Action();
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


		#endregion



	}
}
