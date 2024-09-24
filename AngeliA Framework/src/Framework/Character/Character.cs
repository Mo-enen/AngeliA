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
public abstract class Character : Rigidbody, IDamageReceiver {




	#region --- VAR ---


	// Const
	public const int FULL_SLEEP_DURATION = 90;
	private static readonly int[] BOUNCE_AMOUNTS = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
	private static readonly int[] BOUNCE_AMOUNTS_BIG = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };
	public const int INVENTORY_COLUMN = 6;
	public const int INVENTORY_ROW = 3;

	// Api
	public delegate void CharacterEventHandler (Character character);
	public delegate void StepEventHandler (int x, int y, int groundedID);
	public static event CharacterEventHandler OnSleeping;
	public static event StepEventHandler OnFootStepped;
	public static event CharacterEventHandler OnJump;
	public static event CharacterEventHandler OnPound;
	public static event CharacterEventHandler OnFly;
	public static event CharacterEventHandler OnSlideStepped;
	public static event CharacterEventHandler OnPassOut;
	public static event CharacterEventHandler OnTeleport;
	public static event CharacterEventHandler OnCrash;
	public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
	public CharacterAnimationType AnimationType { get; set; } = CharacterAnimationType.Idle;
	public WeaponType EquippingWeaponType { get; set; } = WeaponType.Hand;
	public WeaponHandheld EquippingWeaponHeld { get; set; } = WeaponHandheld.Float;
	public bool Teleporting => Game.GlobalFrame < TeleportEndFrame;
	public bool TeleportWithPortal => _TeleportDuration < 0;
	public bool TeleportToFrontSide => _TeleportEndFrame > 0;
	public int TeleportEndFrame => _TeleportEndFrame.Abs();
	public int TeleportDuration => _TeleportDuration.Abs();
	public int CurrentAnimationFrame { get; set; } = 0;
	public int CurrentRenderingBounce { get; private set; } = 1000;
	public int SleepStartFrame { get; protected set; } = int.MinValue;
	public int PassOutFrame { get; private set; } = int.MinValue;
	public bool InventoryCurrentAvailable => AllowInventory && Game.GlobalFrame > IgnoreInventoryFrame;
	public bool EquipingPickWeapon { get; private set; } = false;
	protected int CurrentAttackSpeedRate => Movement.MovementState switch {
		CharacterMovementState.Walk => Attackness.WalkingSpeedRateOnAttack,
		CharacterMovementState.Run => Attackness.RunningSpeedRateOnAttack,
		CharacterMovementState.JumpDown => Attackness.AirSpeedRateOnAttack,
		CharacterMovementState.JumpUp => Attackness.AirSpeedRateOnAttack,
		_ => Attackness.DefaultSpeedRateOnAttack,
	};
	int IDamageReceiver.Team => Const.TEAM_NEUTRAL;
	Tag IDamageReceiver.IgnoreDamageType => Tag.None;
	public override int AirDragX => 0;
	public override int AirDragY => 0;
	public override int Gravity => 5;
	public override bool CarryOtherRigidbodyOnTop => false;
	public override bool AllowBeingCarryByOtherRigidbody => true;
	public sealed override int CollisionMask => Movement.IsGrabFlipping ? 0 : PhysicsMask.MAP;
	public sealed override int PhysicalLayer => PhysicsLayer.CHARACTER;
	public virtual int Bouncy => 150;
	public virtual bool AllowInventory => false;
	public virtual int AttackTargetTeam => Const.TEAM_ALL;

	// Behaviour
	public CharacterMovement Movement;
	public CharacterAttackness Attackness;
	public CharacterHealth Health;
	public CharacterNavigation Navigation;
	private CharacterMovement MovementOverride;
	private CharacterAttackness AttacknessOverride;
	private CharacterHealth HealthOverride;
	private CharacterNavigation NavigationOverride;
	public readonly CharacterMovement NativeMovement;
	public readonly CharacterAttackness NativeAttackness;
	public readonly CharacterHealth NativeHealth;
	public readonly CharacterNavigation NativeNavigation;
	public readonly CharacterBuff Buff;
	private int OverridingMovementFrame = int.MinValue;
	private int OverridingAttacknessFrame = int.MinValue;
	private int OverridingHealthFrame = int.MinValue;
	private int OverridingNavigationFrame = int.MinValue;

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


	public Character () {
		// Behaviour
		Movement = NativeMovement = CreateNativeMovement();
		Attackness = NativeAttackness = CreateNativeAttackness();
		Health = NativeHealth = CreateNativeHealth();
		Navigation = NativeNavigation = CreateNativeNavigation();
		Buff = new(this);
		// Init Inventory
		if (AllowInventory) {
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
		Movement = NativeMovement;
		Attackness = NativeAttackness;
		Health = NativeHealth;
		Navigation = NativeNavigation;
		NativeMovement.OnActivated();
		NativeHealth.OnActivated();
		NativeAttackness.OnActivated();
		NativeNavigation.OnActivated();
		CharacterState = CharacterState.GamePlay;
		PassOutFrame = int.MinValue;
		VelocityX = 0;
		VelocityY = 0;
		MovementOverride = null;
		OverridingMovementFrame = int.MinValue;
		IgnoreInventoryFrame = int.MinValue;
	}


	public override void OnInactivated () {
		base.OnInactivated();
		MovementOverride = null;
		AttacknessOverride = null;
		NavigationOverride = null;
	}


	// Physics Update
	public override void FirstUpdate () {

		// Update Behaviour
		Movement = Game.GlobalFrame <= OverridingMovementFrame && MovementOverride != null ? MovementOverride : NativeMovement;
		Attackness = Game.GlobalFrame <= OverridingAttacknessFrame && AttacknessOverride != null ? AttacknessOverride : NativeAttackness;
		Health = Game.GlobalFrame <= OverridingHealthFrame && HealthOverride != null ? HealthOverride : NativeHealth;
		Navigation = Game.GlobalFrame <= OverridingNavigationFrame && NavigationOverride != null ? NavigationOverride : NativeNavigation;

		// Fill Physics
		if (CharacterState == CharacterState.GamePlay && !IgnoringPhysics) {
			Physics.FillEntity(PhysicalLayer, this, Navigation.NavigationEnable);
		}

		// Nav State
		if (Navigation.NavigationEnable) {
			Movement.PushAvailable.Override(false, priority: 4096);
		}

	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Movement.SyncConfigFromPool();
		BeforeUpdate_Inventory();
		Buff.Apply();
	}


	private void BeforeUpdate_Inventory () {

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
					Attackness.AttackDuration = weapon.AttackDuration;
					Attackness.AttackCooldown = weapon.AttackCooldown;
					Attackness.MinimalChargeAttackDuration = weapon.ChargeAttackDuration;
					Attackness.RepeatAttackWhenHolding = weapon.RepeatAttackWhenHolding;
					Attackness.LockFacingOnAttack = weapon.LockFacingOnAttack;
					EquipingPickWeapon = weapon is PickWeapon;
					Attackness.HoldAttackPunishFrame.Min(weapon.HoldAttackPunish);
					if (weapon.DefaultSpeedRateOnAttack.HasValue) {
						Attackness.DefaultSpeedRateOnAttack.Max(weapon.DefaultSpeedRateOnAttack.Value);
					}
					if (weapon.WalkingSpeedRateOnAttack.HasValue) {
						Attackness.WalkingSpeedRateOnAttack.Max(weapon.WalkingSpeedRateOnAttack.Value);
					}
					if (weapon.RunningSpeedRateOnAttack.HasValue) {
						Attackness.RunningSpeedRateOnAttack.Max(weapon.RunningSpeedRateOnAttack.Value);
					}
					Attackness.AttackInAir.Or(weapon.AttackInAir);
					Attackness.AttackInWater.Or(weapon.AttackInWater);
					Attackness.AttackWhenWalking.Or(weapon.AttackWhenWalking);
					Attackness.AttackWhenRunning.Or(weapon.AttackWhenRunning);
					Attackness.AttackWhenClimbing.Or(weapon.AttackWhenClimbing);
					Attackness.AttackWhenFlying.Or(weapon.AttackWhenFlying);
					Attackness.AttackWhenRolling.Or(weapon.AttackWhenRolling);
					Attackness.AttackWhenSquatting.Or(weapon.AttackWhenSquatting);
					Attackness.AttackWhenDashing.Or(weapon.AttackWhenDashing);
					Attackness.AttackWhenSliding.Or(weapon.AttackWhenSliding);
					Attackness.AttackWhenGrabbing.Or(weapon.AttackWhenGrabbing);
					Attackness.AttackWhenRush.Or(weapon.AttackWhenRushing);
					Attackness.AttackWhenPounding.Or(weapon.AttackWhenPounding);
				}
			}
		}

	}


	public override void Update () {

		if (Health.IsEmptyHealth) SetCharacterState(CharacterState.PassOut);

		if (Teleporting) {
			PhysicsUpdate_AnimationType();
			return;
		}

		// Behaviour
		Movement.MovementState = CharacterMovementState.Idle;
		switch (CharacterState) {
			default:
			case CharacterState.GamePlay:
				if (Health.TakingDamage) {
					// Tacking Damage
					VelocityX = VelocityX.MoveTowards(0, Health.KnockbackDeceleration);
				} else {
					// General
					if (Attackness.IsAttacking) {
						// Lock Facing when Attack
						if (Attackness.LockFacingOnAttack) {
							Movement.LockFacingRight(Attackness.AttackStartFacingRight, 1);
						}
						// Change Speed Rate for Attack
						int currentSpeedRate = CurrentAttackSpeedRate;
						if (currentSpeedRate != 1000) {
							Movement.SetSpeedRate(currentSpeedRate, 0);
						}
					}
					// Update
					Attackness.PhysicsUpdate_Attack();
					Movement.PhysicsUpdateGamePlay();
				}
				break;

			case CharacterState.Sleep:
				VelocityX = 0;
				VelocityY = 0;
				Width = Const.CEL;
				Height = Const.CEL;
				OffsetX = -Const.HALF;
				OffsetY = 0;
				IgnorePhysics(1);
				break;

			case CharacterState.PassOut:
				VelocityX = 0;
				break;
		}
		Movement.PhysicsUpdateLater();
		Navigation.PhysicsUpdate();
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
			if (character.Health.TakingDamage) return CharacterAnimationType.TakingDamage;
			if (character.CharacterState == CharacterState.Sleep) return CharacterAnimationType.Sleep;
			if (character.CharacterState == CharacterState.PassOut) return CharacterAnimationType.PassOut;
			if (character.Movement.IsRolling) return CharacterAnimationType.Rolling;
			return character.Movement.MovementState switch {
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
				CharacterMovementState.Pound => character.Movement.SpinOnGroundPound ? CharacterAnimationType.Spin : CharacterAnimationType.Pound,
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
		LateUpdate_RenderCharacter();
		LateUpdate_Event();
		LateUpdate_Inventory();
		base.LateUpdate();
	}


	private void LateUpdate_RenderCharacter () {

		if (!Active) return;

		bool blinking = Health.IsInvincible && !Health.TakingDamage && (Game.GlobalFrame - Health.InvincibleEndFrame).UMod(8) < 4;
		if (blinking) return;

		int oldLayerIndex = Renderer.CurrentLayerIndex;
		bool colorFlash = Health.TakingDamage && Health.HP > 0 && (Game.GlobalFrame - Health.LastDamageFrame).UMod(8) < 4;
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


	private void LateUpdate_Event () {

		int frame = Game.GlobalFrame;

		if (CharacterState == CharacterState.GamePlay) {

			// Jump Fly Crash Slide Bounce
			if (frame % 10 == 0 && Attackness.IsChargingAttack) Bounce();

			// Events
			if (Movement.Target is Character targetCharacter) {
				// Teleport
				if (
					TeleportWithPortal && frame == TeleportEndFrame - TeleportDuration / 2 + 1
				) OnTeleport?.Invoke(targetCharacter);
				// Step
				if (IsGrounded) {
					if (
						(Movement.LastStartRunFrame >= 0 && (frame - Movement.LastStartRunFrame) % 20 == 19) || // Run
						(Movement.IsDashing && (frame - Movement.LastDashFrame) % 8 == 0) || // Dash
						(Movement.IsRushing && (frame - Movement.LastRushFrame) % 3 == 0) // Rush
					) {
						OnFootStepped?.Invoke(targetCharacter.X, targetCharacter.Y, targetCharacter.GroundedID);
					}
				}
				if (Movement.IsSliding && frame % 24 == 0) OnSlideStepped?.Invoke(targetCharacter);
				if (frame == Movement.LastJumpFrame) OnJump?.Invoke(targetCharacter);
				if (IsGrounded && !Movement.IsPounding && frame == Movement.LastPoundingFrame + 1) OnPound?.Invoke(targetCharacter);
				if (frame == Movement.LastFlyFrame) OnFly?.Invoke(targetCharacter);
				if (frame == Movement.LastCrashFrame) OnCrash?.Invoke(targetCharacter);
			}
		}

		// Sleep
		if (CharacterState == CharacterState.Sleep) {
			// ZZZ Particle
			if (frame % 42 == 0) {
				OnSleeping?.Invoke(this);
			}
		}
	}


	private void LateUpdate_Inventory () {

		int invCapacity = GetInventoryCapacity();
		if (invCapacity > 0) {

			bool eventAvailable = CharacterState == CharacterState.GamePlay && !TaskSystem.HasTask() && !Health.TakingDamage;
			int attackLocalFrame = eventAvailable && Attackness.IsAttacking ? Game.GlobalFrame - Attackness.LastAttackFrame : -1;

			// Inventory
			for (int i = 0; i < invCapacity; i++) {
				GetItemFromInventory(i)?.OnItemUpdate_FromInventory(this);
			}

			// Equipping
			for (int i = 0; i < EquipmentTypeCount; i++) {
				var item = GetEquippingItem((EquipmentType)i);
				if (item == null) continue;
				item.OnItemUpdate_FromEquipment(this);
				if (item is Weapon weapon) {
					if (attackLocalFrame == weapon.BulletDelayFrame) {
						var bullet = weapon.SpawnBullet(this);
						item.OnCharacterAttack(this, bullet);
					}
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
		CharacterState = state;
		Navigation.ResetNavigation();

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


	public void EnterTeleportState (int duration, bool front) {
		_TeleportEndFrame = (Game.GlobalFrame + duration.Abs()) * (front ? 1 : -1);
		_TeleportDuration = duration;
	}


	public void LockAnimationType (CharacterAnimationType type, int duration = 1) {
		LockedAnimationType = type;
		LockedAnimationTypeFrame = Game.GlobalFrame + duration;
	}


	public void GetBonusFromFullSleep () => Health.Heal(Health.MaxHP);


	// Damage
	public virtual void TakeDamage (Damage damage) {
		if (!Active || damage.Amount <= 0 || Health.HP <= 0) return;
		if (CharacterState != CharacterState.GamePlay || Health.IsInvincible) return;
		if (Health.InvincibleOnRush && Movement.IsRushing) return;
		if (Health.InvincibleOnDash && Movement.IsDashing) return;
		OnTakeDamage(damage.Amount, damage.Sender);
	}


	protected virtual void OnTakeDamage (int damage, Entity sender) {

		// Equipment
		for (int i = 0; i < EquipmentTypeCount && damage > 0; i++) {
			GetEquippingItem((EquipmentType)i)?.OnTakeDamage_FromEquipment(this, sender, ref damage);
		}

		// Inventory
		int iCount = GetInventoryCapacity();
		for (int i = 0; i < iCount && damage > 0; i++) {
			GetItemFromInventory(i)?.OnTakeDamage_FromInventory(this, sender, ref damage);
		}

		// Deal Damage
		damage = damage.GreaterOrEquelThanZero();
		Health.HP = (Health.HP - damage).Clamp(0, Health.MaxHP);

		VelocityX = Movement.FacingRight ? -Health.KnockBackSpeed : Health.KnockBackSpeed;

		Health.InvincibleEndFrame = Game.GlobalFrame + Health.InvincibleDuration;
		Health.LastDamageFrame = Game.GlobalFrame;

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


	// Behaviour
	protected virtual CharacterMovement CreateNativeMovement () => new(this);
	protected virtual CharacterAttackness CreateNativeAttackness () => new(this);
	protected virtual CharacterHealth CreateNativeHealth () => new();
	protected virtual CharacterNavigation CreateNativeNavigation () => new(this);


	public void OverrideMovement (CharacterMovement movementOverride, int duration = 1) {
		if (movementOverride != MovementOverride) {
			movementOverride.OnActivated();
		}
		OverridingMovementFrame = Game.GlobalFrame + duration;
		MovementOverride = movementOverride;
	}
	public void OverrideAttackness (CharacterAttackness attacknessOverride, int duration = 1) {
		if (attacknessOverride != AttacknessOverride) {
			attacknessOverride.OnActivated();
		}
		OverridingAttacknessFrame = Game.GlobalFrame + duration;
		AttacknessOverride = attacknessOverride;
	}
	public void OverrideHealth (CharacterHealth healthOverride, int duration = 1) {
		if (healthOverride != HealthOverride) {
			healthOverride.OnActivated();
		}
		OverridingHealthFrame = Game.GlobalFrame + duration;
		HealthOverride = healthOverride;
	}
	public void OverrideNavigation (CharacterNavigation navigationOverride, int duration = 1) {
		if (navigationOverride != NavigationOverride) {
			navigationOverride.OnActivated();
		}
		OverridingNavigationFrame = Game.GlobalFrame + duration;
		NavigationOverride = navigationOverride;
	}


	public virtual bool IsAttackAllowedByMovement () =>
		!Movement.IsCrashing &&
		(Attackness.AttackInAir || IsGrounded || InWater || Movement.IsClimbing) &&
		(Attackness.AttackInWater || !InWater) &&
		(Attackness.AttackWhenWalking || !IsGrounded || !Movement.IsWalking) &&
		(Attackness.AttackWhenRunning || !IsGrounded || !Movement.IsRunning) &&
		(Attackness.AttackWhenClimbing || !Movement.IsClimbing) &&
		(Attackness.AttackWhenFlying || !Movement.IsFlying) &&
		(Attackness.AttackWhenRolling || !Movement.IsRolling) &&
		(Attackness.AttackWhenSquatting || !Movement.IsSquatting) &&
		(Attackness.AttackWhenDashing || !Movement.IsDashing) &&
		(Attackness.AttackWhenSliding || !Movement.IsSliding) &&
		(Attackness.AttackWhenGrabbing || (!Movement.IsGrabbingTop && !Movement.IsGrabbingSide)) &&
		(Attackness.AttackWhenPounding || !Movement.IsPounding) &&
		(Attackness.AttackWhenRush || !Movement.IsRushing);


	public virtual bool IsAttackAllowedByEquipment () => (GetEquippingItem(EquipmentType.Weapon) is Weapon weapon && weapon.AllowingAttack(this));


	#endregion




	#region --- LGC ---


	private int GrowAnimationFrame (int frame) {
		switch (Movement.MovementState) {

			case CharacterMovementState.Climb:
				int climbVelocity = Movement.IntendedY != 0 ? Movement.IntendedY : Movement.IntendedX;
				if (climbVelocity > 0) {
					frame++;
				} else if (climbVelocity < 0) {
					frame--;
				}
				break;

			case CharacterMovementState.GrabTop:
				if (Movement.IntendedX > 0) {
					frame++;
				} else if (Movement.IntendedX < 0) {
					frame--;
				}
				break;

			case CharacterMovementState.GrabSide:
				if (Movement.IntendedY > 0) {
					frame++;
				} else if (Movement.IntendedY < 0) {
					frame--;
				}
				break;

			case CharacterMovementState.GrabFlip:
				frame += VelocityY > 0 ? 1 : -1;
				break;

			case CharacterMovementState.Run:
			case CharacterMovementState.Walk:
				frame += Movement.IntendedX > 0 == Movement.FacingRight ? 1 : -1;
				break;

			case CharacterMovementState.Rush:
				if (VelocityX == 0 || VelocityX > 0 == Movement.FacingRight) {
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
		bool isPounding = Movement.MovementState == CharacterMovementState.Pound;
		bool isSquatting = Movement.MovementState == CharacterMovementState.SquatIdle || Movement.MovementState == CharacterMovementState.SquatMove;
		if (frame < LastRequireBounceFrame + duration) {
			bounce = InWater ? BOUNCE_AMOUNTS_BIG[frame - LastRequireBounceFrame] : BOUNCE_AMOUNTS[frame - LastRequireBounceFrame];
			if (Attackness.AttackChargeStartFrame.HasValue && Game.GlobalFrame > Attackness.AttackChargeStartFrame.Value + Attackness.MinimalChargeAttackDuration) {
				bounce += (1000 - bounce) / 2;
			}
		} else if (isPounding) {
			bounce = 1500;
		} else if (IsGrounded && frame.InRangeExclude(Movement.LastPoundingFrame, Movement.LastPoundingFrame + duration)) {
			// Gound Pound End
			bounce = BOUNCE_AMOUNTS_BIG[frame - Movement.LastPoundingFrame];
		} else if (isSquatting && frame.InRangeExclude(Movement.LastSquatFrame, Movement.LastSquatFrame + duration)) {
			// Squat Start
			bounce = BOUNCE_AMOUNTS[frame - Movement.LastSquatFrame];
		} else if (IsGrounded && frame.InRangeExclude(Movement.LastGroundFrame, Movement.LastGroundFrame + duration)) {
			// Gounded Start
			bounce = BOUNCE_AMOUNTS[frame - Movement.LastGroundFrame];
		} else if (!isSquatting && frame.InRangeExclude(Movement.LastSquattingFrame, Movement.LastSquattingFrame + duration)) {
			// Squat End
			bounce = BOUNCE_AMOUNTS[frame - Movement.LastSquattingFrame];
			reverse = true;
		} else if (Movement.IsCrashing && frame.InRangeExclude(Movement.LastCrashFrame, Movement.LastCrashFrame + duration)) {
			// Crash Start
			bounce = BOUNCE_AMOUNTS_BIG[frame - Movement.LastCrashFrame];
		}
		if (bounce != 1000) {
			bounce = Util.RemapUnclamped(0, 1000, (1000 - Bouncy).Clamp(0, 999), 1000, bounce);
		}
		return reverse ? -bounce : bounce;
	}


	#endregion




}
