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
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL)]
	public abstract partial class eCharacter : Rigidbody {




		#region --- VAR ---


		// Const
		private const int FULL_SLEEP_DURATION = 90;
		private const int MAX_SUMMON_COUNT = 64;

		// Api
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
		public bool IsFullPassout => Game.GlobalFrame > PassoutFrame + 48;
		public bool IsFullSleeped => SleepFrame >= FULL_SLEEP_DURATION;
		public bool IsExactlyFullSleeped => SleepFrame == FULL_SLEEP_DURATION;
		public override int CollisionMask => IsGrabFliping ? 0 : YayaConst.MASK_MAP;
		public override int CarrierSpeed => 0;
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override bool IgnoreRiseGravityShift => true;
		protected override int PhysicsLayer => YayaConst.LAYER_CHARACTER;

		// Data
		private readonly ListLoop<eSummon> Summons = new(MAX_SUMMON_COUNT);
		private int SleepFrame = 0;
		private int PassoutFrame = int.MinValue;
		private int PrevZ = int.MinValue;


		#endregion




		#region --- MSG ---


		protected eCharacter () {
			OnInitialize_Render();
		}


		public override void OnActived () {
			base.OnActived();
			Summons.Clear();
			OnActived_Movement();
			OnActived_Action();
			OnActived_Health();
			OnActived_Attack();
			CharacterState = CharacterState.GamePlay;
			PassoutFrame = int.MinValue;
			PrevZ = Game.Current.ViewZ;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {

			if (IsEmptyHealth) SetCharacterState(CharacterState.Passout);
			Update_Summon();

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

				case CharacterState.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					Width = Const.CEL;
					Height = Const.CEL;
					OffsetX = -Const.CEL / 2;
					OffsetY = 0;
					if (!IsFullHealth && IsFullSleeped) SetHealth(MaxHP);
					break;

				case CharacterState.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
					break;
			}

			PrevZ = Game.Current.ViewZ;
		}


		private void Update_Summon () {

			// Remove Null
			for (int i = 0; i < Summons.Count; i++) {
				var sum = Summons[i];
				if (sum == null || sum.Active == false) {
					Summons.RemoveAt(i);
					i--;
				}
			}

			// Gether when Z Changed
			if (Game.Current.ViewZ != PrevZ) {
				foreach (var summon in Summons) {
					summon.X = X;
					summon.Y = Y;
				}
			}

		}


		public override void FrameUpdate () {
			FrameUpdate_Renderer();
			base.FrameUpdate();
			if (CharacterState == CharacterState.Sleep) SleepFrame++;
		}


		#endregion




		#region --- API ---


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


		public void FullSleep () {
			SetCharacterState(CharacterState.Sleep);
			SleepFrame = FULL_SLEEP_DURATION;
		}


		public void CreateSummon<T> (int x, int y) where T : eSummon => CreateSummon(typeof(T).AngeHash(), x, y);
		public void CreateSummon (int typeID, int x, int y) {
			if (Game.Current.SpawnEntity(typeID, x, y) is eSummon summon) {
				// Spawned
				summon.Owner = this;
				summon.OnSummoned(true);
				Summons.Add(summon);
			} else {
				// Swape Old
				int count = Summons.Count;
				for (int i = 0; i < count; i++) {
					var sum = Summons[i];
					if (sum.TypeID == typeID) {
						sum.OnInactived();
						sum.X = x;
						sum.Y = y;
						sum.OnActived();
						sum.Owner = this;
						sum.OnSummoned(true);
						Summons.RemoveAt(i);
						Summons.Add(sum);
						break;
					}
				}
			}
		}


		public void MakeSummon (eSummon target) {
			if (target == null || Summons.Contains(target)) return;
			target.Owner = this;
			Summons.Add(target);
			target.OnSummoned(false);
		}


		#endregion



		#region --- LGC ---



		#endregion




	}
}
