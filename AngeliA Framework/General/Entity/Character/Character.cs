using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum CharacterState {
		GamePlay = 0,
		Sleep,
		PassOut,
	}


	[EntityAttribute.MapEditorGroup("Character")]
	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.Layer(Const.ENTITY_LAYER_CHARACTER)]
	public abstract partial class Character : Rigidbody {




		#region --- VAR ---


		// Const
		public const int FULL_SLEEP_DURATION = 90;
		private static readonly int[] BOUNCE_AMOUNTS = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
		private static readonly int[] BOUNCE_AMOUNTS_BIG = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };

		// Particle
		public static int SleepParticleCode { get; set; } = typeof(SleepParticle).AngeHash();
		public static int SleepDoneParticleCode { get; set; } = 0;
		public static int FootstepParticleCode { get; set; } = typeof(CharacterFootstep).AngeHash();
		public static int SlideParticleCode { get; set; } = typeof(SlideDust).AngeHash();
		public static int DashParticleCode { get; set; } = typeof(CharacterFootstep).AngeHash();
		public static int PassOutParticleCode { get; set; } = typeof(PassOutStarParticle).AngeHash();
		public static int TeleportParticleCode { get; set; } = typeof(AppearSmokeParticle).AngeHash();

		// Api
		public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
		public WeaponType EquippingWeaponType { get; set; } = WeaponType.Hand;
		public WeaponHandHeld EquippingWeaponHeld { get; set; } = WeaponHandHeld.Float;
		public bool IsPassOut => HealthPoint == 0;
		public bool IsFullPassOut => HealthPoint == 0 && Game.GlobalFrame > PassOutFrame + 48;
		public int SleepFrame { get; private set; } = 0;
		public bool Teleporting => Game.GlobalFrame < TeleportEndFrame.Abs();
		public bool RenderWithSheet { get; private set; } = false;
		public int CurrentAnimationFrame { get; set; } = 0;
		public int CurrentRenderingBounce { get; private set; } = 1000;
		protected override bool PhysicsEnable => CharacterState != CharacterState.Sleep;
		protected override int AirDragX => 0;
		protected override int AirDragY => 0;
		protected override int GravityRise => Gravity;
		protected override bool CarryOtherRigidbodyOnTop => false;
		protected override bool AllowBeingCarryByOtherRigidbody => true;
		protected sealed override int CollisionMask => IsGrabFlipping ? 0 : Const.MASK_MAP;
		protected sealed override int PhysicsLayer => Const.LAYER_CHARACTER;
		protected virtual int Bouncy => 150;

		// Short
		private static int EquipmentTypeCount => _EquipmentTypeCount > 0 ? _EquipmentTypeCount : (_EquipmentTypeCount = System.Enum.GetValues(typeof(EquipmentType)).Length);
		private static int _EquipmentTypeCount = 0;

		// Data
		private static readonly HashSet<int> RenderWithSheetPool = new();
		private int PassOutFrame = int.MinValue;
		private int LastRequireBounceFrame = int.MinValue;
		private int TeleportEndFrame = 0;
		private bool TeleportWithPortal = false;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			OnActivated_Pose();
			OnActivated_Movement();
			OnActivated_Health();
			OnActivated_Attack();
			OnActivated_Navigation();
			CharacterState = CharacterState.GamePlay;
			PassOutFrame = int.MinValue;
			VelocityX = 0;
			VelocityY = 0;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay) {
				CellPhysics.FillEntity(PhysicsLayer, this, NavigationEnable);
			}
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			PoseZOffset = 0;
		}


		public override void PhysicsUpdate () {

			if (IsEmptyHealth) SetCharacterState(CharacterState.PassOut);

			if (Teleporting) {
				PhysicsUpdate_AnimationType();
				return;
			}

			// Behaviour
			MovementState = CharacterMovementState.Idle;
			switch (CharacterState) {
				default:
				case CharacterState.GamePlay:
					if (TakingDamage) {
						// Tacking Damage
						VelocityX = VelocityX.MoveTowards(0, KnockbackDeceleration);
					} else {
						// General
						PhysicsUpdate_Attack();
						PhysicsUpdate_Movement_GamePlay();
					}
					break;

				case CharacterState.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					Width = Const.CEL;
					Height = Const.CEL;
					OffsetX = -Const.HALF;
					OffsetY = 0;
					if (!IsFullHealth && SleepFrame >= FULL_SLEEP_DURATION) SetHealth(MaxHP);
					break;

				case CharacterState.PassOut:
					VelocityX = 0;
					break;
			}
			PhysicsUpdate_Movement_After();
			PhysicsUpdate_Navigation();
			PhysicsUpdate_AnimationType();
			base.PhysicsUpdate();
		}


		private void PhysicsUpdate_AnimationType () {
			var poseType = GetCurrentPoseAnimationType(this);
			if (poseType != AnimatedPoseType) {
				CurrentAnimationFrame = 0;
				AnimatedPoseType = poseType;
			}
			// Func
			static CharacterPoseAnimationType GetCurrentPoseAnimationType (Character character) {
				if (Game.GlobalFrame <= character.LockedAnimationTypeFrame) return character.LockedAnimationType;
				if (character.Teleporting) return CharacterPoseAnimationType.Idle;
				if (character.TakingDamage) return CharacterPoseAnimationType.TakingDamage;
				if (character.CharacterState == CharacterState.Sleep) return CharacterPoseAnimationType.Sleep;
				if (character.CharacterState == CharacterState.PassOut) return CharacterPoseAnimationType.PassOut;
				if (character.IsRolling) return CharacterPoseAnimationType.Rolling;
				return character.MovementState switch {
					CharacterMovementState.Walk => CharacterPoseAnimationType.Walk,
					CharacterMovementState.Run => CharacterPoseAnimationType.Run,
					CharacterMovementState.JumpUp => CharacterPoseAnimationType.JumpUp,
					CharacterMovementState.JumpDown => CharacterPoseAnimationType.JumpDown,
					CharacterMovementState.SwimIdle => CharacterPoseAnimationType.SwimIdle,
					CharacterMovementState.SwimMove => CharacterPoseAnimationType.SwimMove,
					CharacterMovementState.SquatIdle => CharacterPoseAnimationType.SquatIdle,
					CharacterMovementState.SquatMove => CharacterPoseAnimationType.SquatMove,
					CharacterMovementState.Dash => CharacterPoseAnimationType.Dash,
					CharacterMovementState.Rush => CharacterPoseAnimationType.Rush,
					CharacterMovementState.Pound => character.SpinOnGroundPound ? CharacterPoseAnimationType.Spin : CharacterPoseAnimationType.Pound,
					CharacterMovementState.Climb => CharacterPoseAnimationType.Climb,
					CharacterMovementState.Fly => CharacterPoseAnimationType.Fly,
					CharacterMovementState.Slide => CharacterPoseAnimationType.Slide,
					CharacterMovementState.GrabTop => CharacterPoseAnimationType.GrabTop,
					CharacterMovementState.GrabSide => CharacterPoseAnimationType.GrabSide,
					CharacterMovementState.GrabFlip => CharacterPoseAnimationType.Rolling,
					_ => CharacterPoseAnimationType.Idle,
				};
			}
		}


		public override void FrameUpdate () {
			if (FrameUpdate_RenderCharacter()) {
				FrameUpdate_Particle();
				FrameUpdate_Inventory();
				base.FrameUpdate();
			}
		}


		private bool FrameUpdate_RenderCharacter () {

			bool blinking = IsInvincible && !TakingDamage && (Game.GlobalFrame - InvincibleEndFrame).UMod(8) < 4;
			if (blinking) return false;

			bool colorFlash = TakingDamage && (Game.GlobalFrame - LastDamageFrame).UMod(8) < 4;
			if (colorFlash) CellRenderer.SetLayerToColor();
			int cellIndexStart = CellRenderer.GetUsedCellCount();

			// Render
			CurrentRenderingBounce = GetCurrentRenderingBounce();
			if (RenderWithSheet) {
				FrameUpdate_SheetRendering();
			} else {
				FrameUpdate_PoseRendering();
			}
			CurrentAnimationFrame = GrowAnimationFrame(CurrentAnimationFrame);

			// Cell Effect
			if (
				(colorFlash || (Teleporting && TeleportWithPortal)) &&
				CellRenderer.GetCells(out var cells, out int count)
			) {
				// Color Flash
				if (colorFlash) {
					for (int i = cellIndexStart; i < count; i++) {
						var cell = cells[i];
						cell.Color = Const.WHITE;
					}
				}
				// Portal
				if (Teleporting && TeleportWithPortal) {
					for (int i = cellIndexStart; i < count; i++) {
						var cell = cells[i];
						cell.ScaleFrom(
							Util.RemapUnclamped(
								0, 30, 1000, 0,
								(Game.GlobalFrame - TeleportEndFrame + 60).PingPong(30)
							), X, Y + Height / 2
						);
					}
				}
			}

			// Final
			CellRenderer.SetLayerToDefault();
			return true;
		}


		private void FrameUpdate_Particle () {

			if (CharacterState == CharacterState.GamePlay) {
				// Run Particle
				if (
					FootstepParticleCode != 0 &&
					IsGrounded &&
					LastStartRunFrame >= 0 &&
					(Game.GlobalFrame - LastStartRunFrame) % 20 == 19 &&
					Stage.TrySpawnEntity(FootstepParticleCode, X, Y, out var entity) &&
					entity is Particle particle
				) {
					if (CellRenderer.TryGetSprite(GroundedID, out var sprite)) {
						particle.Tint = sprite.SummaryTint;
					} else {
						particle.Tint = Const.WHITE;
					}
				}
				// Slide Particle
				if (SlideParticleCode != 0 && IsSliding && Game.GlobalFrame % 24 == 0) {
					var rect = Rect;
					Stage.SpawnEntity(
						SlideParticleCode, FacingRight ? rect.xMax : rect.xMin, rect.yMin + rect.height * 3 / 4
					);
				}
				// Dash Particle
				if (IsGrounded && DashParticleCode != 0 && IsDashing && Game.GlobalFrame % 8 == 0) {
					if (
						Stage.TrySpawnEntity(DashParticleCode, X, Y, out var dashEntity) &&
						dashEntity is Particle dashParticle
					) {
						if (CellRenderer.TryGetSprite(GroundedID, out var sprite)) {
							dashParticle.Tint = sprite.SummaryTint;
						} else {
							dashParticle.Tint = Const.WHITE;
						}
					}
				}
				// Charging Bounce
				if (Game.GlobalFrame % 10 == 0 && IsChargingAttack) {
					Bounce();
				}

			}

			// Sleep
			if (CharacterState == CharacterState.Sleep) {
				// ZZZ Particle
				if (SleepParticleCode != 0 && Game.GlobalFrame % 42 == 0) {
					Stage.TrySpawnEntity(SleepParticleCode, X, Y + Height / 2, out _);
				}
				// Full Sleep Particle
				if (SleepFrame == FULL_SLEEP_DURATION) {
					var rect = Rect;
					if (SleepDoneParticleCode != 0 && Stage.TrySpawnEntity(
						SleepDoneParticleCode,
						rect.x + rect.width / 2,
						rect.y + rect.height / 2,
						out var sleepParticle
					)) {
						sleepParticle.Width = Const.CEL * 2;
						sleepParticle.Height = Const.CEL * 2;
					}
				}
				// ++
				SleepFrame++;
			}
		}


		private void FrameUpdate_Inventory () {

			AttackDuration.Override = null;
			AttackCooldown.Override = null;
			RepeatAttackWhenHolding.Override = null;
			MinimalChargeAttackDuration.Override = null;
			LockFacingOnAttack.Override = null;
			MovementLoseRateOnAttack.Override = null;

			int invCapacity = GetInventoryCapacity();
			if (invCapacity > 0) {

				bool eventAvailable = CharacterState == CharacterState.GamePlay && !FrameTask.HasTask() && !TakingDamage;
				bool attackStart = eventAvailable && Game.GlobalFrame == LastAttackFrame;
				bool squatStart = eventAvailable && Game.GlobalFrame == LastSquatFrame;

				// Inventory
				for (int i = 0; i < invCapacity; i++) {
					GetItemFromInventory(i)?.OnItemUpdate_FromInventory(this);
				}

				// Equipping
				bool equippingWeapon = false;
				for (int i = 0; i < EquipmentTypeCount; i++) {
					var type = (EquipmentType)i;
					var item = GetEquippingItem(type);
					if (item == null) continue;
					item.OnItemUpdate_FromEquipment(this);
					if (attackStart) item.OnAttack(this);
					if (squatStart) item.OnRepair(this);
					if (item is Weapon weapon) {
						equippingWeapon = true;
						AttackDuration.Override = weapon.AttackDuration;
						AttackCooldown.Override = weapon.AttackCooldown;
						RepeatAttackWhenHolding.Override = weapon.RepeatAttackWhenHolding;
						MinimalChargeAttackDuration.Override = weapon.ChargeAttackDuration;
						LockFacingOnAttack.Override = weapon.LockFacingOnAttack;
						MovementLoseRateOnAttack.Override = weapon.MovementLoseRateOnAttack;
						if (attackStart) Bullet.SpawnBullet(weapon.BulletID, this, weapon);
					}
				}

				// Spawn Punch Bullet
				if (attackStart && !equippingWeapon) {
					SpawnPunchBullet();
				}

			}
		}


		protected virtual int GetInventoryCapacity () => 0;
		protected virtual Item GetItemFromInventory (int itemIndex) => null;
		protected virtual Equipment GetEquippingItem (EquipmentType type) => null;


		#endregion




		#region --- API ---


		public virtual void SetCharacterState (CharacterState state) {

			if (CharacterState == state) return;

			PassOutFrame = int.MinValue;
			CharacterState = state;
			ResetNavigation();

			switch (state) {

				case CharacterState.GamePlay:
					if (CharacterState == CharacterState.Sleep) {
						Bounce();
					}
					VelocityX = 0;
					VelocityY = 0;
					break;

				case CharacterState.Sleep:
					SleepFrame = 0;
					VelocityX = 0;
					VelocityY = 0;
					break;

				case CharacterState.PassOut:
					PassOutFrame = Game.GlobalFrame;
					if (PassOutParticleCode != 0 && Stage.SpawnEntity(PassOutParticleCode, X, Y) is Particle particle) {
						particle.UserData = this;
					}
					break;

			}

		}


		public void EnterTeleportState (int duration, bool front, bool withPortal) {
			TeleportEndFrame = (Game.GlobalFrame + duration) * (front ? 1 : -1);
			TeleportWithPortal = withPortal;
		}


		public void SetAsFullAsleep () {
			if (CharacterState == CharacterState.Sleep) {
				SleepFrame = FULL_SLEEP_DURATION;
			}
		}


		public void LockAnimationType (CharacterPoseAnimationType type, int duration = 1) {
			LockedAnimationType = type;
			LockedAnimationTypeFrame = Game.GlobalFrame + duration;
		}


		// Bounce
		public void Bounce () => LastRequireBounceFrame = Game.GlobalFrame;
		public void CancelBounce () => LastRequireBounceFrame = int.MinValue;


		#endregion




		#region --- LGC ---


		private int GrowAnimationFrame (int frame) {
			switch (MovementState) {

				case CharacterMovementState.Climb:
					int climbVelocity = IntendedY != 0 ? IntendedY : IntendedX;
					if (climbVelocity > 0) {
						frame++;
					} else if (climbVelocity < 0) {
						frame--;
					}
					break;

				case CharacterMovementState.GrabTop:
					if (IntendedX > 0) {
						frame++;
					} else if (IntendedX < 0) {
						frame--;
					}
					break;

				case CharacterMovementState.GrabSide:
					if (IntendedY > 0) {
						frame++;
					} else if (IntendedY < 0) {
						frame--;
					}
					break;

				case CharacterMovementState.GrabFlip:
					frame += VelocityY > 0 ? 1 : -1;
					break;

				case CharacterMovementState.Run:
				case CharacterMovementState.Walk:
					frame += IntendedX > 0 == FacingRight ? 1 : -1;
					break;

				case CharacterMovementState.Rush:
					if (VelocityX == 0 || VelocityX > 0 == FacingRight) {
						frame++;
					} else {
						frame = 0;
					}
					break;

				default:
					frame++;
					break;

			}
			return frame;
		}


		private int GetCurrentRenderingBounce () {
			int frame = Game.GlobalFrame;
			int bounce = 1000;
			int duration = BOUNCE_AMOUNTS.Length;
			bool reverse = false;
			bool isPounding = MovementState == CharacterMovementState.Pound;
			bool isSquatting = MovementState == CharacterMovementState.SquatIdle || MovementState == CharacterMovementState.SquatMove;
			if (frame < LastRequireBounceFrame + duration) {
				bounce = BOUNCE_AMOUNTS[frame - LastRequireBounceFrame];
				if (AttackChargeStartFrame.HasValue && Game.GlobalFrame > AttackChargeStartFrame.Value + MinimalChargeAttackDuration) {
					bounce += (1000 - bounce) / 2;
				}
			} else if (isPounding) {
				bounce = 1500;
			} else if (!isPounding && IsGrounded && frame.InRangeExclude(LastPoundingFrame, LastPoundingFrame + duration)) {
				bounce = BOUNCE_AMOUNTS_BIG[frame - LastPoundingFrame];
			} else if (isSquatting && frame.InRangeExclude(LastSquatFrame, LastSquatFrame + duration)) {
				bounce = BOUNCE_AMOUNTS[frame - LastSquatFrame];
			} else if (IsGrounded && frame.InRangeExclude(LastGroundFrame, LastGroundFrame + duration)) {
				bounce = BOUNCE_AMOUNTS[frame - LastGroundFrame];
			} else if (!isSquatting && frame.InRangeExclude(LastSquattingFrame, LastSquattingFrame + duration)) {
				bounce = BOUNCE_AMOUNTS[frame - LastSquattingFrame];
				reverse = true;
			}
			if (bounce != 1000) bounce = Util.RemapUnclamped(0, 1000, (1000 - Bouncy).Clamp(0, 999), 1000, bounce);
			return reverse ? -bounce : bounce;
		}


		#endregion




	}
}
