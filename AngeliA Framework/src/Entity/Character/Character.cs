using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AngeliA;


public enum CharacterState { GamePlay = 0, Sleep, PassOut, }


public enum CharacterInventoryType { None = 0, Unique, Map, }


[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.MapEditorGroup("Character")]
[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL * 2)]
[EntityAttribute.Layer(EntityLayer.CHARACTER)]
public abstract class Character : Rigidbody, IDamageReceiver, IActionTarget {




	#region --- VAR ---


	// Const
	public const int FULL_SLEEP_DURATION = 90;
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
	public bool Teleporting => Game.GlobalFrame < _TeleportEndFrame.Abs();
	public bool TeleportToFrontSide => _TeleportEndFrame > 0;
	public int CurrentAttackSpeedRate => Movement.MovementState switch {
		CharacterMovementState.Walk => Attackness.WalkingSpeedRateOnAttack,
		CharacterMovementState.Run => Attackness.RunningSpeedRateOnAttack,
		CharacterMovementState.JumpDown => Attackness.AirSpeedRateOnAttack,
		CharacterMovementState.JumpUp => Attackness.AirSpeedRateOnAttack,
		_ => Attackness.DefaultSpeedRateOnAttack,
	};
	public sealed override int PhysicalLayer => PhysicsLayer.CHARACTER;
	public sealed override int CollisionMask => Movement.IsGrabFlipping ? 0 : PhysicsMask.MAP;
	public override int AirDragX => 0;
	public override int AirDragY => 0;
	public override int FallingGravity => 5;
	public override int RisingGravity => 5;
	public override bool CarryOtherRigidbodyOnTop => false;
	public override bool AllowBeingCarryByOtherRigidbody => true;
	public virtual CharacterInventoryType InventoryType => CharacterInventoryType.None;

	public int Bouncy { get; set; } = 150;
	public bool HelmetInteractable { get; set; } = true;
	public bool BodySuitInteractable { get; set; } = true;
	public bool GlovesInteractable { get; set; } = true;
	public bool ShoesInteractable { get; set; } = true;
	public bool JewelryInteractable { get; set; } = true;
	public bool WeaponInteractable { get; set; } = true;
	public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
	public CharacterAnimationType AnimationType { get; set; } = CharacterAnimationType.Idle;
	public WeaponType EquippingWeaponType { get; set; } = WeaponType.Hand;
	public WeaponHandheld EquippingWeaponHeld { get; set; } = WeaponHandheld.Float;
	public int SleepStartFrame { get; set; } = int.MinValue;
	public int PassOutFrame { get; private set; } = int.MinValue;
	public int LastRequireBounceFrame { get; set; } = int.MinValue;
	public int Team { get; set; } = Const.TEAM_NEUTRAL;
	public Tag IgnoreDamageType { get; set; } = Tag.None;
	public int AttackTargetTeam { get; set; } = Const.TEAM_ALL;
	public int DespawnAfterPassoutDelay { get; set; } = 60;
	public int InventoryID { get; private set; }

	// Behaviour
	public CharacterMovement Movement;
	public CharacterAttackness Attackness;
	public CharacterHealth Health;
	public CharacterNavigation Navigation;
	public CharacterRenderer Rendering;
	private CharacterMovement MovementOverride;
	private CharacterAttackness AttacknessOverride;
	private CharacterHealth HealthOverride;
	private CharacterNavigation NavigationOverride;
	private CharacterRenderer RendererOverride;
	public readonly CharacterMovement NativeMovement;
	public readonly CharacterAttackness NativeAttackness;
	public readonly CharacterHealth NativeHealth;
	public readonly CharacterNavigation NativeNavigation;
	public readonly CharacterRenderer NativeRenderer;
	public readonly CharacterBuff Buff;
	private int OverridingMovementFrame = int.MinValue;
	private int OverridingAttacknessFrame = int.MinValue;
	private int OverridingHealthFrame = int.MinValue;
	private int OverridingNavigationFrame = int.MinValue;
	private int OverridingRendererFrame = int.MinValue;

	// Data
	private readonly string TypeName;
	private int _TeleportEndFrame = 0;
	private int _TeleportDuration = 0;
	private CharacterAnimationType LockedAnimationType = CharacterAnimationType.Idle;
	private int LockedAnimationTypeFrame = int.MinValue;
	private int ForceStayFrame = -1;
	private int PrevZ;


	#endregion




	#region --- MSG ---


	[AfterEntityReposition]
	internal static void AfterEntityReposition (Entity entity, Int3? from, Int3 to) {

		if (
			!from.HasValue ||
			entity is not Character character ||
			character.InventoryType != CharacterInventoryType.Map
		) return;

		// Repos Inventory
		string fromInvName = Inventory.GetPositionBasedInventoryName(character.TypeName, from.Value);
		string toInvName = Inventory.GetPositionBasedInventoryName(character.TypeName, to);
		Inventory.RenameEquipInventory(fromInvName, toInvName);

	}


	public Character () {

		TypeName = GetType().AngeName();

		// Behaviour
		Movement = NativeMovement = CreateNativeMovement();
		Attackness = NativeAttackness = CreateNativeAttackness();
		Health = NativeHealth = CreateNativeHealth();
		Navigation = NativeNavigation = CreateNativeNavigation();
		Rendering = NativeRenderer = CreateNativeRenderer();
		Buff = new CharacterBuff(this);

		// Init Inventory
		if (InventoryType == CharacterInventoryType.Unique) {
			const int COUNT = INVENTORY_COLUMN * INVENTORY_ROW;
			if (Inventory.HasInventory(InventoryID)) {
				int invCount = Inventory.GetInventoryCapacity(InventoryID);
				if (invCount != COUNT) {
					Inventory.ResizeInventory(InventoryID, COUNT);
				}
			} else {
				// Create New
				Inventory.AddNewEquipmentInventoryData(TypeName, COUNT);
			}
		}
	}


	public override void OnActivated () {
		base.OnActivated();

		// Inv
		switch (InventoryType) {
			case CharacterInventoryType.Unique:
				InventoryID = TypeID;
				break;
			case CharacterInventoryType.Map:
				if (MapUnitPos.HasValue) {
					const int COUNT = INVENTORY_COLUMN * INVENTORY_ROW;
					string name = Inventory.GetPositionBasedInventoryName(TypeName, MapUnitPos.Value);
					InventoryID = name.AngeHash();
					if (Inventory.HasInventory(InventoryID)) {
						int invCount = Inventory.GetInventoryCapacity(InventoryID);
						if (invCount != COUNT) {
							Inventory.ResizeInventory(InventoryID, COUNT);
						}
					} else {
						// Create New
						Inventory.AddNewEquipmentInventoryData(name, COUNT);
					}
				} else {
					InventoryID = TypeID;
				}
				break;
		}

		// Behavour
		Movement = NativeMovement;
		Attackness = NativeAttackness;
		Health = NativeHealth;
		Navigation = NativeNavigation;
		Rendering = NativeRenderer;
		NativeMovement.OnActivated();
		NativeHealth.OnActivated();
		NativeAttackness.OnActivated();
		NativeNavigation.OnActivated();
		NativeRenderer.OnActivated();

		// Misc
		CharacterState = CharacterState.GamePlay;
		PassOutFrame = int.MinValue;
		VelocityX = 0;
		VelocityY = 0;
		MovementOverride = null;
		AttacknessOverride = null;
		HealthOverride = null;
		NavigationOverride = null;
		RendererOverride = null;
		OverridingMovementFrame = int.MinValue;
		OverridingAttacknessFrame = int.MinValue;
		OverridingHealthFrame = int.MinValue;
		OverridingNavigationFrame = int.MinValue;
		OverridingRendererFrame = int.MinValue;
		Team = Const.TEAM_NEUTRAL;
		IgnoreDamageType = Tag.None;
		AttackTargetTeam = Const.TEAM_ALL;
		DespawnAfterPassoutDelay = 60;
		ForceStayFrame = -1;
		PrevZ = Stage.ViewZ;
		Bouncy = 150;
		bool allowInv = InventoryType != CharacterInventoryType.None;
		HelmetInteractable = allowInv;
		BodySuitInteractable = allowInv;
		GlovesInteractable = allowInv;
		ShoesInteractable = allowInv;
		JewelryInteractable = allowInv;
		WeaponInteractable = allowInv;
	}


	public override void OnInactivated () {
		base.OnInactivated();
		MovementOverride = null;
		AttacknessOverride = null;
		NavigationOverride = null;
	}


	// Physics Update
	public override void FirstUpdate () {

		// Force Stay on Stage Check
		if (Game.GlobalFrame > ForceStayFrame && (Stage.ViewZ != PrevZ || !Stage.SpawnRect.Overlaps(Rect))) {
			// Leave Stage
			Active = false;
			return;
		}
		PrevZ = Stage.ViewZ;

		// Update Behaviour
		Movement = Game.GlobalFrame <= OverridingMovementFrame && MovementOverride != null ? MovementOverride : NativeMovement;
		Attackness = Game.GlobalFrame <= OverridingAttacknessFrame && AttacknessOverride != null ? AttacknessOverride : NativeAttackness;
		Health = Game.GlobalFrame <= OverridingHealthFrame && HealthOverride != null ? HealthOverride : NativeHealth;
		Navigation = Game.GlobalFrame <= OverridingNavigationFrame && NavigationOverride != null ? NavigationOverride : NativeNavigation;
		Rendering = Game.GlobalFrame <= OverridingRendererFrame && RendererOverride != null ? RendererOverride : NativeRenderer;

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
		if (!Active) return;
		// Despawn for Passout
		if (
			DespawnAfterPassoutDelay >= 0 &&
			CharacterState == CharacterState.PassOut &&
			(Game.GlobalFrame - PassOutFrame) >= DespawnAfterPassoutDelay
		) {
			Active = false;
			return;
		}
		// Update
		BeforeUpdate_Inventory();
		Buff.ApplyOnUpdate();
		Rendering.BeforeUpdate();
	}


	private void BeforeUpdate_Inventory () {

		int invCapacity = Inventory.GetInventoryCapacity(InventoryID);
		if (invCapacity > 0) {

			// Inventory
			ResetInventoryUpdate(invCapacity);
			for (int i = 0; i < invCapacity; i++) {
				int id = Inventory.GetItemAt(InventoryID, i);
				var item = id != 0 ? ItemSystem.GetItem(id) : null;
				if (item == null || !item.CheckUpdateAvailable(TypeID)) continue;
				item.BeforeItemUpdate_FromInventory(this);
			}

			// Equipping
			for (int i = 0; i < Const.EquipmentTypeCount; i++) {
				var type = (EquipmentType)i;
				int id = Inventory.GetEquipment(InventoryID, type, out int equipmentCount);
				var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
				if (item == null) continue;
				item.BeforeItemUpdate_FromEquipment(this);
				if (item is Weapon weapon) {
					Attackness.AttackDuration = weapon.AttackDuration;
					Attackness.AttackCooldown = weapon.AttackCooldown;
					Attackness.MinimalChargeAttackDuration = weapon.ChargeAttackDuration;
					Attackness.RepeatAttackWhenHolding = weapon.RepeatAttackWhenHolding;
					Attackness.LockFacingOnAttack = weapon.LockFacingOnAttack;
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

		if (!Active) return;

		if (Health.IsEmptyHealth) {
			SetCharacterState(CharacterState.PassOut);
		}

		if (Teleporting) {
			Update_AnimationType();
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
					Update_RepairEquipment();
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
		Update_AnimationType();
		base.Update();
	}


	private void Update_AnimationType () {
		var poseType = GetCurrentPoseAnimationType(this);
		if (poseType != AnimationType) {
			Rendering.CurrentAnimationFrame = 0;
			AnimationType = poseType;
		}
		// Func
		static CharacterAnimationType GetCurrentPoseAnimationType (Character character) {
			if (Game.GlobalFrame <= character.LockedAnimationTypeFrame) return character.LockedAnimationType;
			if (character.Teleporting) return character._TeleportDuration < 0 ? CharacterAnimationType.Rolling : CharacterAnimationType.Idle;
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
				CharacterMovementState.Pound => character.Rendering.SpinOnGroundPound ? CharacterAnimationType.Spin : CharacterAnimationType.Pound,
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


	private void Update_RepairEquipment () {
		if (Health.TakingDamage || Game.GlobalFrame != Movement.LastSquatFrame + 1) return;
		for (int i = 0; i < Const.EquipmentTypeCount; i++) {
			int id = Inventory.GetEquipment(InventoryID, (EquipmentType)i, out int equipmentCount);
			var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			if (item == null) continue;
			if (item.TryRepair(this)) break;
		}
	}


	// Frame Update
	public override void LateUpdate () {
		if (!Active) {
			base.LateUpdate();
			return;
		}
		LateUpdate_RenderCharacter();
		LateUpdate_Event();
		LateUpdate_Inventory();
		base.LateUpdate();
		LateUpdate_BouceHighlight();
	}


	private void LateUpdate_RenderCharacter () {

		bool blinking = Health.IsInvincible && !Health.TakingDamage && (Game.GlobalFrame - Health.InvincibleEndFrame).UMod(8) < 4;
		if (blinking) return;

		int oldLayerIndex = Renderer.CurrentLayerIndex;
		bool colorFlash = Health.TakingDamage && Health.HP > 0 && (Game.GlobalFrame - Health.LastDamageFrame).UMod(8) < 4;
		if (colorFlash) Renderer.SetLayerToColor();
		int cellIndexStart = Renderer.GetUsedCellCount();

		// Render
		Rendering.UpdateForBounce();
		Rendering.LateUpdate();
		Rendering.GrowAnimationFrame();

		// Cell Effect
		bool teleportingWithPortal = Teleporting && _TeleportDuration < 0;
		if (
			(colorFlash || teleportingWithPortal) &&
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
			if (teleportingWithPortal) {
				int pointX = X;
				int pointY = Y + Const.CEL;
				int duration = _TeleportDuration.Abs();
				int localFrame = Game.GlobalFrame - _TeleportEndFrame.Abs() + duration;
				for (int i = cellIndexStart; i < count; i++) {
					float lerp01 = localFrame.PingPong(duration / 2) / (duration / 2f);
					int offsetX = (int)((1f - lerp01) * Const.CEL * Util.Sin(lerp01 * 720f * Util.Deg2Rad));
					int offsetY = (int)((1f - lerp01) * Const.CEL * Util.Cos(lerp01 * 720f * Util.Deg2Rad));
					var cell = cells[i];
					cell.X += offsetX;
					cell.Y += offsetY;
					cell.RotateAround(localFrame * 720 / duration, pointX + offsetX, pointY + offsetY);
					cell.ScaleFrom(
						Util.RemapUnclamped(0, duration / 2, 1000, 0, localFrame.PingPong(duration / 2)),
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
					_TeleportDuration < 0 && frame == _TeleportEndFrame.Abs() - _TeleportDuration.Abs() / 2 + 1
				) {
					OnTeleport?.Invoke(targetCharacter);
				}
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

		int invCapacity = Inventory.GetInventoryCapacity(InventoryID);
		if (invCapacity > 0) {

			bool eventAvailable = CharacterState == CharacterState.GamePlay && !TaskSystem.HasTask() && !Health.TakingDamage;
			int attackLocalFrame = eventAvailable && Attackness.IsAttacking ? Game.GlobalFrame - Attackness.LastAttackFrame : -1;

			// Inventory
			ResetInventoryUpdate(invCapacity);
			for (int i = 0; i < invCapacity; i++) {
				int id = Inventory.GetItemAt(InventoryID, i);
				var item = id != 0 ? ItemSystem.GetItem(id) : null;
				if (item == null || !item.CheckUpdateAvailable(TypeID)) continue;
				item.OnItemUpdate_FromInventory(this);
			}

			// Equipping
			for (int i = 0; i < Const.EquipmentTypeCount; i++) {
				int id = Inventory.GetEquipment(InventoryID, (EquipmentType)i, out int equipmentCount);
				var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
				if (item == null) continue;
				item.OnItemUpdate_FromEquipment(this);
				if (item is Weapon weapon) {
					if (attackLocalFrame == weapon.BulletDelayFrame) {
						var bullet = weapon.SpawnBullet(this);
						item.OnCharacterAttack(this, bullet);
						Buff.ApplyOnAttack(bullet);
					}
				}
			}

		}

		// Equipping
		int equippingID = Inventory.GetEquipment(InventoryID, EquipmentType.Weapon, out _);
		if (equippingID != 0 && ItemSystem.GetItem(equippingID) is Weapon eqWeapon) {
			EquippingWeaponType = eqWeapon.WeaponType;
			EquippingWeaponHeld = eqWeapon.Handheld;
		} else {
			EquippingWeaponType = WeaponType.Hand;
			EquippingWeaponHeld = WeaponHandheld.Float;
		}

	}


	private void LateUpdate_BouceHighlight () {
		if ((this as IActionTarget).IsHighlighted) {
			// Bounce
			if (Game.GlobalFrame % 20 == 0) Bounce();
			// Hint
			ControlHintUI.DrawGlobalHint(
				X - Const.HALF, Y + Const.CEL * 2,
				Gamekey.Action, BuiltInText.HINT_SWITCH_PLAYER, true
			);
		}
	}


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
		for (int i = 0; i < Const.EquipmentTypeCount && damage > 0; i++) {
			int id = Inventory.GetEquipment(InventoryID, (EquipmentType)i, out int equipmentCount);
			var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			item?.OnTakeDamage_FromEquipment(this, sender, ref damage);
		}

		// Inventory
		int invCapacity = Inventory.GetInventoryCapacity(InventoryID);
		ResetInventoryUpdate(invCapacity);
		for (int i = 0; i < invCapacity && damage > 0; i++) {
			int id = Inventory.GetItemAt(InventoryID, i);
			var item = id != 0 ? ItemSystem.GetItem(id) : null;
			if (item == null || !item.CheckUpdateAvailable(TypeID)) continue;
			item.OnTakeDamage_FromInventory(this, sender, ref damage);
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
	public void ResetInventoryUpdate (int invCapacity) {
		for (int i = 0; i < invCapacity; i++) {
			int id = Inventory.GetItemAt(InventoryID, i);
			var item = id != 0 ? ItemSystem.GetItem(id) : null;
			if (item == null) continue;
			item.LastUpdateFrame = -1;
		}
	}


	public bool EquipmentAvailable (EquipmentType equipmentType) => equipmentType switch {
		EquipmentType.Weapon => WeaponInteractable,
		EquipmentType.BodyArmor => BodySuitInteractable,
		EquipmentType.Helmet => HelmetInteractable,
		EquipmentType.Shoes => ShoesInteractable,
		EquipmentType.Gloves => GlovesInteractable,
		EquipmentType.Jewelry => JewelryInteractable,
		_ => false,
	};


	// Behaviour
	protected virtual CharacterMovement CreateNativeMovement () => new(this);
	protected virtual CharacterAttackness CreateNativeAttackness () => new(this);
	protected virtual CharacterHealth CreateNativeHealth () => new();
	protected virtual CharacterNavigation CreateNativeNavigation () => new(this);
	protected virtual CharacterRenderer CreateNativeRenderer () => new SheetCharacterRenderer(this);


	public void OverrideMovement (CharacterMovement movementOverride, int duration = 1) {
		if (movementOverride == null) return;
		if (movementOverride != MovementOverride) {
			movementOverride.OnActivated();
		}
		OverridingMovementFrame = Game.GlobalFrame + duration;
		MovementOverride = movementOverride;
	}
	public void OverrideAttackness (CharacterAttackness attacknessOverride, int duration = 1) {
		if (attacknessOverride == null) return;
		if (attacknessOverride != AttacknessOverride) {
			attacknessOverride.OnActivated();
		}
		OverridingAttacknessFrame = Game.GlobalFrame + duration;
		AttacknessOverride = attacknessOverride;
	}
	public void OverrideHealth (CharacterHealth healthOverride, int duration = 1) {
		if (healthOverride == null) return;
		if (healthOverride != HealthOverride) {
			healthOverride.OnActivated();
		}
		OverridingHealthFrame = Game.GlobalFrame + duration;
		HealthOverride = healthOverride;
	}
	public void OverrideNavigation (CharacterNavigation navigationOverride, int duration = 1) {
		if (navigationOverride == null) return;
		if (navigationOverride != NavigationOverride) {
			navigationOverride.OnActivated();
		}
		OverridingNavigationFrame = Game.GlobalFrame + duration;
		NavigationOverride = navigationOverride;
	}
	public void OverrideRenderer (CharacterRenderer rendererOverride, int duration = 1) {
		if (rendererOverride == null) return;
		if (rendererOverride != RendererOverride) {
			rendererOverride.OnActivated();
		}
		OverridingRendererFrame = Game.GlobalFrame + duration;
		RendererOverride = rendererOverride;
	}


	// Misc
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


	public virtual bool IsAttackAllowedByEquipment () {
		int id = Inventory.GetEquipment(InventoryID, EquipmentType.Weapon, out int equipmentCount);
		var weapon = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Weapon : null;
		return weapon != null && weapon.AllowingAttack(this);
	}


	public virtual bool Invoke () {
		PlayerSystem.SetCharacterAsPlayer(this);
		return PlayerSystem.Selecting == this;
	}


	public virtual bool AllowInvoke () => false;


	public void ForceStayOnStage (int duration = 1) => ForceStayFrame = Game.GlobalFrame + duration;


	#endregion




}
