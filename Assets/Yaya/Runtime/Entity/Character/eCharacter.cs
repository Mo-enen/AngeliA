using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public enum CharacterState {
		GamePlay = 0,
		Sleep,
		Passout,
	}


	[EntityAttribute.MapEditorGroup("Character")]
	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL)]
	public abstract partial class eCharacter : Rigidbody {




		#region --- VAR ---


		// Const
		private const int FULL_SLEEP_DURATION = 90;

		// Api
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
		public bool IsFullPassout => Game.GlobalFrame > PassoutFrame + 48;
		public bool IsFullSleeped => SleepFrame >= FULL_SLEEP_DURATION;
		public bool IsExactlyFullSleeped => SleepFrame == FULL_SLEEP_DURATION;
		protected override int CollisionMask => IsGrabFliping ? 0 : YayaConst.MASK_MAP;
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override bool IgnoreRiseGravityShift => true;
		protected override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override bool PhysicsEnable => CharacterState != CharacterState.Sleep;

		// Data
		private int SleepFrame = 0;
		private int PassoutFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		protected eCharacter () => OnInitialize_Render();


		public override void OnActived () {
			base.OnActived();
			OnActived_Movement();
			OnActived_Action();
			OnActived_Health();
			OnActived_Attack();
			OnActived_Navigation();
			CharacterState = CharacterState.GamePlay;
			PassoutFrame = int.MinValue;
			VelocityX = 0;
			VelocityY = 0;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay) {
				CellPhysics.FillEntity(PhysicsLayer, this, NavigationEnable);
			}
		}


		public override void PhysicsUpdate () {

			if (IsEmptyHealth) SetCharacterState(CharacterState.Passout);

			// Behaviour
			MoveState = MovementState.Idle;
			switch (CharacterState) {
				default:
				case CharacterState.GamePlay:
					if (TakingDamage) {
						// Tacking Damage
						VelocityX = VelocityX.MoveTowards(0, KnockbackDecceleration);
					} else {
						// General
						PhysicsUpdate_Action();
						PhysicsUpdate_Attack();
						PhysicsUpdate_Movement_GamePlay();
						// Stop when Attacking
						if (StopMoveOnAttack && IsAttacking && IsGrounded) {
							VelocityX = 0;
						}
					}
					break;

				case CharacterState.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					Width = Const.CEL;
					Height = Const.CEL;
					OffsetX = -Const.HALF;
					OffsetY = 0;
					if (!IsFullHealth && IsFullSleeped) SetHealth(MaxHP);
					break;

				case CharacterState.Passout:
					VelocityX = 0;
					break;
			}
			PhysicsUpdate_Movement_After();
			PhysicsUpdate_Navigation();
			base.PhysicsUpdate();
		}


		public override void FrameUpdate () {
			FrameUpdate_Renderer();
			base.FrameUpdate();
			if (CharacterState == CharacterState.Sleep) SleepFrame++;
		}


		#endregion




		#region --- API ---


		public bool IsAttackAllowedByMovement () =>
			!IsRushing &&
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
					VelocityX = 0;
					VelocityY = 0;
					break;

				case CharacterState.Sleep:
					SleepFrame = 0;
					VelocityX = 0;
					VelocityY = 0;
					break;

				case CharacterState.Passout:
					PassoutFrame = Game.GlobalFrame;
					var particle = Game.Current.SpawnEntity<ePassoutStarParticle>(X, Y);
					if (particle != null) {
						particle.Character = this;
					}
					break;

				default:
					throw new System.NotImplementedException();
			}
			CharacterState = state;
		}


		public void FullSleep () {
			SetCharacterState(CharacterState.Sleep);
			SleepFrame = FULL_SLEEP_DURATION;
		}


		#endregion




	}
}
