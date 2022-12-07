using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Standard;


namespace Yaya {


	public enum CharacterState {
		GamePlay = 0,
		Sleep = 1,
		Passout = 2,
	}


	[System.Flags]
	public enum CharacterIdentity {
		None = 0,
		Player = 1 << 0,
		Enemy = 1 << 1,
		NPC = 1 << 2,

		All = Player | Enemy | NPC,
	}


	public interface IPermissionCharacter {
		CharacterIdentity Identity { get; }
	}


	[EntityAttribute.MapEditorGroup("Character")]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL)]
	public abstract partial class eCharacter : eYayaRigidbody, ISerializationCallbackReceiver, IDamageReceiver, IPermissionCharacter {




		#region --- VAR ---


		// Api
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override bool IgnoreRiseGravityShift => true;
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public override int CarrierSpeed => 0;
		public virtual CharacterIdentity Identity => CharacterIdentity.None;
		public bool TakingDamage => Game.GlobalFrame < LastDamageFrame + DamageStunDuration;
		public bool InAir => !IsGrounded && !InWater && !InSand && !IsClimbing;
		public int SleepAmount {
			get => Util.Remap(0, 90, 0, 1000, SleepFrame);
			set => SleepFrame = Util.Remap(0, 1000, 0, 90, value);
		}
		public int PassoutFrame { get; private set; } = int.MinValue;
		public int SleepFrame { get; protected set; } = 0;
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			string typeName = GetType().Name;
			if (typeName[0] == 'e') typeName = typeName[1..];
			AngeUtil.LoadMeta(this, "", typeName);
			OnInitialize_Render();
		}


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
					} else if (StopMoveOnAttack && IsAttacking) {
						// Stop when Attacking
						if (IsGrounded) VelocityX = 0;
					} else {
						// Move as Normal
						Update_Action();
						Update_Attack();
						Update_Movement();
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
					if (!IsFullHealth && SleepAmount >= 1000) SetHealth(MaxHP);
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
					break;
			}

		}


		public override void FrameUpdate () {
			FrameUpdate_Render();
			base.FrameUpdate();
		}


		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);
		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);


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
			(AttackWhenClimbing || !IsClimbing) &&
			(AttackWhenFlying || !IsFlying) &&
			(AttackWhenRolling || !IsRolling) &&
			(AttackWhenSquating || !IsSquating) &&
			(AttackWhenDashing || !IsDashing);


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


		// Misc
		protected override bool InsideGroundCheck () => CellPhysics.Overlap(YayaConst.MASK_LEVEL, new(X, Y + Height / 4, 1, 1), this);


		#endregion



	}
}
