using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AngeliA;


public enum CharacterState {
	GamePlay = 0,
	Sleep,
	PassOut,
}



[EntityAttribute.MapEditorGroup("Character")]
[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL * 2)]
[EntityAttribute.Layer(EntityLayer.CHARACTER)]
public abstract partial class Character : Rigidbody {




	#region --- VAR ---


	// Const
	private static readonly int[] BOUNCE_AMOUNTS = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
	private static readonly int[] BOUNCE_AMOUNTS_BIG = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };
	public const int INVENTORY_COLUMN = 6;
	public const int INVENTORY_ROW = 3;

	// Api
	public delegate void CharacterEventHandler (Character character);
	public static event CharacterEventHandler OnSleeping;
	public static event CharacterEventHandler OnFootStepped;
	public static event CharacterEventHandler OnJump;
	public static event CharacterEventHandler OnFly;
	public static event CharacterEventHandler OnSlideStepped;
	public static event CharacterEventHandler OnPassOut;
	public static event CharacterEventHandler OnTeleport;
	public static event CharacterEventHandler OnCrash;
	public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
	public CharacterAnimationType AnimationType { get; set; } = CharacterAnimationType.Idle;
	public WeaponType EquippingWeaponType { get; set; } = WeaponType.Hand;
	public WeaponHandheld EquippingWeaponHeld { get; set; } = WeaponHandheld.Float;
	public bool IsPassOut => HealthPoint == 0;
	public bool IsFullPassOut => HealthPoint == 0 && Game.GlobalFrame > PassOutFrame + 48;
	public bool Teleporting => Game.GlobalFrame < TeleportEndFrame;
	public bool TeleportWithPortal => _TeleportDuration < 0;
	public bool TeleportToFrontSide => _TeleportEndFrame > 0;
	public int TeleportEndFrame => _TeleportEndFrame.Abs();
	public int TeleportDuration => _TeleportDuration.Abs();
	public int CurrentAnimationFrame { get; set; } = 0;
	public int CurrentRenderingBounce { get; private set; } = 1000;
	public int SleepStartFrame { get; private set; } = int.MinValue;
	public int PassOutFrame { get; private set; } = int.MinValue;
	public bool InventoryCurrentAvailable => IsCharacterWithInventory && Game.GlobalFrame > IgnoreInventoryFrame;
	public bool EquipingPickWeapon { get; private set; } = false;
	protected override bool PhysicsEnable => CharacterState != CharacterState.Sleep;
	protected override int AirDragX => 0;
	protected override int AirDragY => 0;
	protected override int Gravity => 5;
	protected override bool CarryOtherRigidbodyOnTop => false;
	protected override bool AllowBeingCarryByOtherRigidbody => true;
	protected sealed override int CollisionMask => IsGrabFlipping ? 0 : PhysicsMask.MAP;
	protected sealed override int PhysicalLayer => PhysicsLayer.CHARACTER;
	protected virtual int Bouncy => 150;
	protected virtual bool IsCharacterWithInventory => false;

	// Data
	protected static int EquipmentTypeCount = System.Enum.GetValues(typeof(EquipmentType)).Length;
	protected int LastRequireBounceFrame = int.MinValue;
	private int _TeleportEndFrame = 0;
	private int _TeleportDuration = 0;
	private CharacterAnimationType LockedAnimationType = CharacterAnimationType.Idle;
	private int LockedAnimationTypeFrame = int.MinValue;
	private int IgnoreInventoryFrame = int.MinValue;


	#endregion




	#region --- MSG ---


	public Character () => InitInventory();


	private void InitInventory () {
		// Init Inventory
		if (IsCharacterWithInventory) {
			const int COUNT = INVENTORY_COLUMN * INVENTORY_ROW;
			if (Inventory.HasInventory(TypeID)) {
				int invCount = Inventory.GetInventoryCapacity(TypeID);
				if (invCount != COUNT) {
					Inventory.ResizeInventory(TypeID, COUNT);
				}
			} else {
				// Create New
				Inventory.AddNewCharacterInventoryData(GetType().AngeName(), COUNT);
			}
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		OnActivated_Movement();
		OnActivated_Health();
		OnActivated_Attack();
		OnActivated_Navigation();
		CharacterState = CharacterState.GamePlay;
		PassOutFrame = int.MinValue;
		VelocityX = 0;
		VelocityY = 0;
	}


	// Physics Update
	public override void FirstUpdate () {
		if (CharacterState == CharacterState.GamePlay) {
			Physics.FillEntity(PhysicalLayer, this, NavigationEnable);
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		SyncMovementConfigFromPool();
		BeforeUpdate_BuffValue();
	}


	private void BeforeUpdate_BuffValue () {

		bool weaponFilled = false;
		int invCapacity = GetInventoryCapacity();
		if (invCapacity > 0) {

			// Inventory
			for (int i = 0; i < invCapacity; i++) {
				GetItemFromInventory(i)?.BeforeItemUpdate_FromInventory(this);
			}

			// Equipping
			for (int i = 0; i < EquipmentTypeCount; i++) {
				var type = (EquipmentType)i;
				var item = GetEquippingItem(type);
				if (item == null) continue;
				item.BeforeItemUpdate_FromEquipment(this);
				if (item is Weapon weapon) {
					weaponFilled = true;
					AttackDuration = weapon.AttackDuration;
					AttackCooldown = weapon.AttackCooldown;
					MinimalChargeAttackDuration = weapon.ChargeAttackDuration;
					RepeatAttackWhenHolding = weapon.RepeatAttackWhenHolding;
					LockFacingOnAttack = weapon.LockFacingOnAttack;
					EquipingPickWeapon = weapon is PickWeapon;
					HoldAttackPunish.Override(weapon.HoldAttackPunish);
					DefaultSpeedLoseOnAttack.Override(weapon.DefaultSpeedLoseOnAttack);
					WalkingSpeedLoseOnAttack.Override(weapon.WalkingSpeedLoseOnAttack);
					RunningSpeedLoseOnAttack.Override(weapon.RunningSpeedLoseOnAttack);
					AttackInAir.Override(weapon.AttackInAir);
					AttackInWater.Override(weapon.AttackInWater);
					AttackWhenWalking.Override(weapon.AttackWhenWalking);
					AttackWhenRunning.Override(weapon.AttackWhenRunning);
					AttackWhenClimbing.Override(weapon.AttackWhenClimbing);
					AttackWhenFlying.Override(weapon.AttackWhenFlying);
					AttackWhenRolling.Override(weapon.AttackWhenRolling);
					AttackWhenSquatting.Override(weapon.AttackWhenSquatting);
					AttackWhenDashing.Override(weapon.AttackWhenDashing);
					AttackWhenSliding.Override(weapon.AttackWhenSliding);
					AttackWhenGrabbing.Override(weapon.AttackWhenGrabbing);
					AttackWhenRush.Override(weapon.AttackWhenRushing);
					AttackWhenPounding.Override(weapon.AttackWhenPounding);
				}
			}
		}
		if (!weaponFilled) {
			// Default
			AttackDuration = 12;
			AttackCooldown = 2;
			MinimalChargeAttackDuration = int.MaxValue;
			RepeatAttackWhenHolding = false;
			LockFacingOnAttack = false;
		}
	}


	public override void Update () {

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
				break;

			case CharacterState.PassOut:
				VelocityX = 0;
				break;
		}
		PhysicsUpdate_Movement_After();
		PhysicsUpdate_Navigation();
		PhysicsUpdate_AnimationType();
		base.Update();
	}


	private void PhysicsUpdate_AnimationType () {
		var poseType = GetCurrentPoseAnimationType(this);
		if (poseType != AnimationType) {
			CurrentAnimationFrame = 0;
			AnimationType = poseType;
		}
		// Func
		static CharacterAnimationType GetCurrentPoseAnimationType (Character character) {
			if (Game.GlobalFrame <= character.LockedAnimationTypeFrame) return character.LockedAnimationType;
			if (character.Teleporting) return character.TeleportWithPortal ? CharacterAnimationType.Rolling : CharacterAnimationType.Idle;
			if (character.TakingDamage) return CharacterAnimationType.TakingDamage;
			if (character.CharacterState == CharacterState.Sleep) return CharacterAnimationType.Sleep;
			if (character.CharacterState == CharacterState.PassOut) return CharacterAnimationType.PassOut;
			if (character.IsRolling) return CharacterAnimationType.Rolling;
			return character.MovementState switch {
				CharacterMovementState.Walk => CharacterAnimationType.Walk,
				CharacterMovementState.Run => CharacterAnimationType.Run,
				CharacterMovementState.JumpUp => CharacterAnimationType.JumpUp,
				CharacterMovementState.JumpDown => CharacterAnimationType.JumpDown,
				CharacterMovementState.SwimIdle => CharacterAnimationType.SwimIdle,
				CharacterMovementState.SwimMove => CharacterAnimationType.SwimMove,
				CharacterMovementState.SquatIdle => CharacterAnimationType.SquatIdle,
				CharacterMovementState.SquatMove => CharacterAnimationType.SquatMove,
				CharacterMovementState.Dash => CharacterAnimationType.Dash,
				CharacterMovementState.Rush => CharacterAnimationType.Rush,
				CharacterMovementState.Crash => CharacterAnimationType.Crash,
				CharacterMovementState.Pound => character.SpinOnGroundPound ? CharacterAnimationType.Spin : CharacterAnimationType.Pound,
				CharacterMovementState.Climb => CharacterAnimationType.Climb,
				CharacterMovementState.Fly => CharacterAnimationType.Fly,
				CharacterMovementState.Slide => CharacterAnimationType.Slide,
				CharacterMovementState.GrabTop => CharacterAnimationType.GrabTop,
				CharacterMovementState.GrabSide => CharacterAnimationType.GrabSide,
				CharacterMovementState.GrabFlip => CharacterAnimationType.Rolling,
				_ => CharacterAnimationType.Idle,
			};
		}
	}


	// Frame Update
	public override void LateUpdate () {
		FrameUpdate_RenderCharacter();
		FrameUpdate_Event();
		FrameUpdate_Inventory();
		base.LateUpdate();
	}


	private void FrameUpdate_RenderCharacter () {

		bool blinking = IsInvincible && !TakingDamage && (Game.GlobalFrame - InvincibleEndFrame).UMod(8) < 4;
		if (blinking) return;

		int oldLayerIndex = Renderer.CurrentLayerIndex;
		bool colorFlash = TakingDamage && HealthPoint > 0 && (Game.GlobalFrame - LastDamageFrame).UMod(8) < 4;
		if (colorFlash) Renderer.SetLayerToColor();
		int cellIndexStart = Renderer.GetUsedCellCount();

		// Render
		CurrentRenderingBounce = GetCurrentRenderingBounce();
		RenderCharacter();
		CurrentAnimationFrame = GrowAnimationFrame(CurrentAnimationFrame);

		// Cell Effect
		if (
			(colorFlash || (Teleporting && TeleportWithPortal)) &&
			Renderer.GetCells(out var cells, out int count)
		) {
			// Color Flash
			if (colorFlash) {
				for (int i = cellIndexStart; i < count; i++) {
					var cell = cells[i];
					cell.Color = Color32.WHITE;
				}
			}
			// Portal
			if (Teleporting && TeleportWithPortal) {
				int pointX = X;
				int pointY = Y + Const.CEL;
				int localFrame = Game.GlobalFrame - TeleportEndFrame + TeleportDuration;
				for (int i = cellIndexStart; i < count; i++) {
					float lerp01 = localFrame.PingPong(TeleportDuration / 2) / (TeleportDuration / 2f);
					int offsetX = (int)((1f - lerp01) * Const.CEL * Util.Sin(lerp01 * 720f * Util.Deg2Rad));
					int offsetY = (int)((1f - lerp01) * Const.CEL * Util.Cos(lerp01 * 720f * Util.Deg2Rad));
					var cell = cells[i];
					cell.X += offsetX;
					cell.Y += offsetY;
					cell.RotateAround(localFrame * 720 / TeleportDuration, pointX + offsetX, pointY + offsetY);
					cell.ScaleFrom(
						Util.RemapUnclamped(0, TeleportDuration / 2, 1000, 0, localFrame.PingPong(TeleportDuration / 2)),
						pointX + offsetX, pointY + offsetY
					);
				}
			}
		}

		// Final
		Renderer.SetLayer(oldLayerIndex);
	}


	private void FrameUpdate_Event () {

		if (CharacterState == CharacterState.GamePlay) {

			// Teleport Event
			if (
				TeleportWithPortal && Game.GlobalFrame == TeleportEndFrame - TeleportDuration / 2 + 1
			) OnTeleport?.Invoke(this);

			// Step
			if (IsGrounded) {
				if (
					(LastStartRunFrame >= 0 && (Game.GlobalFrame - LastStartRunFrame) % 20 == 19) || // Run
					(IsDashing && (Game.GlobalFrame - LastDashFrame) % 8 == 0) || // Dash
					(IsRushing && (Game.GlobalFrame - LastRushFrame) % 3 == 0) // Rush
				) {
					OnFootStepped?.Invoke(this);
				}
			}

			// Jump Fly Crash Slide Bounce
			if (Game.GlobalFrame % 10 == 0 && IsChargingAttack) Bounce();
			if (IsSliding && Game.GlobalFrame % 24 == 0) OnSlideStepped?.Invoke(this);
			if (Game.GlobalFrame == LastJumpFrame) OnJump?.Invoke(this);
			if (Game.GlobalFrame == LastFlyFrame) OnFly?.Invoke(this);
			if (Game.GlobalFrame == LastCrashFrame) OnCrash?.Invoke(this);
		}

		// Sleep
		if (CharacterState == CharacterState.Sleep) {
			// ZZZ Particle
			if (Game.GlobalFrame % 42 == 0) {
				OnSleeping?.Invoke(this);
			}
		}
	}


	private void FrameUpdate_Inventory () {

		int invCapacity = GetInventoryCapacity();
		if (invCapacity > 0) {

			bool eventAvailable = CharacterState == CharacterState.GamePlay && !Task.HasTask() && !TakingDamage;
			int attackLocalFrame = eventAvailable && IsAttacking ? Game.GlobalFrame - LastAttackFrame : -1;

			// Inventory
			for (int i = 0; i < invCapacity; i++) {
				GetItemFromInventory(i)?.OnItemUpdate_FromInventory(this);
			}

			// Equipping
			for (int i = 0; i < EquipmentTypeCount; i++) {
				var item = GetEquippingItem((EquipmentType)i);
				if (item == null) continue;
				item.OnItemUpdate_FromEquipment(this);
				if (attackLocalFrame == 0) item.OnCharacterAttack(this);
				if (item is Weapon weapon) {
					if (attackLocalFrame == weapon.BulletDelayFrame) weapon.SpawnBullet(this);
				}
			}

		}
	}


	// Virtual
	protected abstract void RenderCharacter ();


	#endregion




	#region --- API ---


	public virtual void SetCharacterState (CharacterState state) {

		if (CharacterState == state) return;

		PassOutFrame = int.MinValue;
		SleepStartFrame = int.MinValue;
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
				VelocityX = 0;
				VelocityY = 0;
				SleepStartFrame = Game.GlobalFrame;
				break;

			case CharacterState.PassOut:
				PassOutFrame = Game.GlobalFrame;
				OnPassOut?.Invoke(this);
				break;

		}

	}


	public void EnterTeleportState (int duration, bool front, bool withPortal) {
		_TeleportEndFrame = (Game.GlobalFrame + duration) * (front ? 1 : -1);
		_TeleportDuration = withPortal ? -duration : duration;
	}


	public void LockAnimationType (CharacterAnimationType type, int duration = 1) {
		LockedAnimationType = type;
		LockedAnimationTypeFrame = Game.GlobalFrame + duration;
	}


	// Bounce
	public void Bounce () => LastRequireBounceFrame = Game.GlobalFrame;
	public void CancelBounce () => LastRequireBounceFrame = int.MinValue;


	// Inventory
	public int GetInventoryCapacity () => InventoryCurrentAvailable ? Inventory.GetInventoryCapacity(TypeID) : 0;


	public int GetItemIDFromInventory (int itemIndex) => GetItemIDFromInventory(itemIndex, out _);
	public int GetItemIDFromInventory (int itemIndex, out int count) {
		count = 0;
		if (!InventoryCurrentAvailable) return 0;
		return Inventory.GetItemAt(TypeID, itemIndex, out count);
	}


	public Item GetItemFromInventory (int itemIndex) => GetItemFromInventory(itemIndex, out _);
	public Item GetItemFromInventory (int itemIndex, out int count) {
		count = 0;
		if (!InventoryCurrentAvailable) return null;
		return ItemSystem.GetItem(Inventory.GetItemAt(TypeID, itemIndex, out count));
	}


	public Equipment GetEquippingItem (EquipmentType type) => GetEquippingItem(type, out _);
	public Equipment GetEquippingItem (EquipmentType type, out int equipmentCount) {
		equipmentCount = 0;
		if (!InventoryCurrentAvailable) return null;
		int id = Inventory.GetEquipment(TypeID, type, out equipmentCount);
		if (id == 0 || equipmentCount <= 0) return null;
		return ItemSystem.GetItem(id) as Equipment;
	}


	public void IgnoreInventory (int duration = 1) => IgnoreInventoryFrame = Game.GlobalFrame + duration;


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
			bounce = InWater ? BOUNCE_AMOUNTS_BIG[frame - LastRequireBounceFrame] : BOUNCE_AMOUNTS[frame - LastRequireBounceFrame];
			if (AttackChargeStartFrame.HasValue && Game.GlobalFrame > AttackChargeStartFrame.Value + MinimalChargeAttackDuration) {
				bounce += (1000 - bounce) / 2;
			}
		} else if (isPounding) {
			bounce = 1500;
		} else if (IsGrounded && frame.InRangeExclude(LastPoundingFrame, LastPoundingFrame + duration)) {
			// Gound Pound End
			bounce = BOUNCE_AMOUNTS_BIG[frame - LastPoundingFrame];
		} else if (isSquatting && frame.InRangeExclude(LastSquatFrame, LastSquatFrame + duration)) {
			// Squat Start
			bounce = BOUNCE_AMOUNTS[frame - LastSquatFrame];
		} else if (IsGrounded && frame.InRangeExclude(LastGroundFrame, LastGroundFrame + duration)) {
			// Gounded Start
			bounce = BOUNCE_AMOUNTS[frame - LastGroundFrame];
		} else if (!isSquatting && frame.InRangeExclude(LastSquattingFrame, LastSquattingFrame + duration)) {
			// Squat End
			bounce = BOUNCE_AMOUNTS[frame - LastSquattingFrame];
			reverse = true;
		} else if (IsCrashing && frame.InRangeExclude(LastCrashFrame, LastCrashFrame + duration)) {
			// Crash Start
			bounce = BOUNCE_AMOUNTS_BIG[frame - LastCrashFrame];
		}
		if (bounce != 1000) {
			bounce = Util.RemapUnclamped(0, 1000, (1000 - Bouncy).Clamp(0, 999), 1000, bounce);
		}
		return reverse ? -bounce : bounce;
	}


	#endregion




}
