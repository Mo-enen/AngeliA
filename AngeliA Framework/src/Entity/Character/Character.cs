using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AngeliA;


public enum CharacterState { GamePlay = 0, Sleep, PassOut, }


public enum CharacterInventoryType { None = 0, Unique, Map, }


public enum CharacterAnimationType {
	Idle = 0, Walk, Run,
	JumpUp, JumpDown, SwimIdle, SwimMove,
	SquatIdle, SquatMove, Dash, Rush, Crash, Pound, Brake,
	Climb, Fly, Slide, GrabTop, GrabSide, Spin,
	TakingDamage, Sleep, PassOut, Rolling,
}


[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.MapEditorGroup("Character")]
[EntityAttribute.Layer(EntityLayer.CHARACTER)]
public abstract class Character : Rigidbody, IDamageReceiver, ICarrier, IWithCharacterMovement, IWithCharacterAttackness, IWithCharacterHealth, IWithCharacterBuff, IWithCharacterRenderer {




	#region --- VAR ---


	// Const
	public const int INVENTORY_COLUMN = 6;
	public const int INVENTORY_ROW = 3;

	// Api
	public bool Teleporting => Game.GlobalFrame < _TeleportEndFrame.Abs();
	public int TeleportEndFrame => _TeleportEndFrame.Abs();
	public bool TeleportingWithPortal => Teleporting && _TeleportDuration < 0;
	public bool TeleportToFrontSide => _TeleportEndFrame > 0;
	public int CurrentAttackSpeedRate => Movement.MovementState switch {
		CharacterMovementState.Walk => Attackness.WalkingSpeedRateOnAttack,
		CharacterMovementState.Run => Attackness.RunningSpeedRateOnAttack,
		CharacterMovementState.JumpDown => Attackness.AirSpeedRateOnAttack,
		CharacterMovementState.JumpUp => Attackness.AirSpeedRateOnAttack,
		_ => Attackness.DefaultSpeedRateOnAttack,
	};
	public sealed override int PhysicalLayer => PhysicsLayer.CHARACTER;
	public sealed override int SelfCollisionMask => Movement.IsGrabFlipping ? 0 : PhysicsMask.MAP;
	public override int AirDragX => 0;
	public override int AirDragY => 0;
	public override bool CarryOtherOnTop => false;
	public override bool AllowBeingPush => true;
	public override bool FacingRight => Movement.FacingRight;
	public override bool EjectWhenInsideGround => false;
	public virtual CharacterInventoryType InventoryType => CharacterInventoryType.None;
	public virtual int FinalCharacterHeight => Movement.MovementHeight;
	public virtual int DefaultCharacterHeight => 160;
	public virtual int Team => Const.TEAM_NEUTRAL;
	public virtual int AttackTargetTeam => Const.TEAM_ALL;
	public virtual Tag IgnoreDamageType => Tag.None;
	bool ICarrier.AllowBeingCarry => true;
	bool IDamageReceiver.IsInvincible => Health.IsInvincible;
	bool IDamageReceiver.TakeDamageFromLevel => Game.GlobalFrame > IgnoreDamageFromLevelFrame;
	CharacterMovement IWithCharacterMovement.CurrentMovement => Movement;
	CharacterAttackness IWithCharacterAttackness.CurrentAttackness => Attackness;
	CharacterHealth IWithCharacterHealth.CurrentHealth => Health;
	CharacterBuff IWithCharacterBuff.CurrentBuff => Buff;
	CharacterRenderer IWithCharacterRenderer.CurrentRenderer => Rendering;
	public int Bouncy { get; set; } = 150;
	public bool HelmetInteractable { get; set; } = true;
	public bool BodySuitInteractable { get; set; } = true;
	public bool GlovesInteractable { get; set; } = true;
	public bool ShoesInteractable { get; set; } = true;
	public bool JewelryInteractable { get; set; } = true;
	public bool HandToolInteractable { get; set; } = true;
	public CharacterState CharacterState { get; private set; } = CharacterState.GamePlay;
	public CharacterAnimationType AnimationType { get; set; } = CharacterAnimationType.Idle;
	public int SleepStartFrame { get; set; } = int.MinValue;
	public int PassOutFrame { get; private set; } = int.MinValue;
	public int LastRequireBounceFrame { get; set; } = int.MinValue;
	public int DespawnAfterPassoutDelay { get; set; } = 60;
	public int InventoryID { get; private set; }
	public int RenderingCellIndex { get; private set; }

	public readonly FrameBasedInt FullSleepDuration = new(90);
	public readonly FrameBasedInt TeleportDuration = new(30);

	// Behaviour
	public CharacterMovement Movement;
	public CharacterAttackness Attackness;
	public CharacterHealth Health;
	public CharacterRenderer Rendering;
	private CharacterMovement MovementOverride;
	private CharacterAttackness AttacknessOverride;
	private CharacterHealth HealthOverride;
	private CharacterRenderer RendererOverride;
	public readonly CharacterMovement NativeMovement;
	public readonly CharacterAttackness NativeAttackness;
	public readonly CharacterHealth NativeHealth;
	public readonly CharacterRenderer NativeRenderer;
	public readonly CharacterBuff Buff;
	private int OverridingMovementFrame = int.MinValue;
	private int OverridingAttacknessFrame = int.MinValue;
	private int OverridingHealthFrame = int.MinValue;
	private int OverridingRendererFrame = int.MinValue;

	// Data
	private readonly string TypeName;
	private readonly int DisplayNameID;
	private readonly int DescriptionID;
	private int _TeleportEndFrame = 0;
	private int _TeleportDuration = 0;
	private CharacterAnimationType LockedAnimationType = CharacterAnimationType.Idle;
	private int LockedAnimationTypeFrame = int.MinValue;
	private int ForceTriggerFrame = -1;
	private int IgnoreDamageFromLevelFrame = -1;


	#endregion




	#region --- MSG ---


	public Character () {

		TypeName = GetType().AngeName();
		DisplayNameID = $"{TypeName}.Name".AngeHash();
		DescriptionID = $"{TypeName}.Des".AngeHash();

		// Behaviour
		Movement = NativeMovement = CreateNativeMovement();
		Attackness = NativeAttackness = CreateNativeAttackness();
		Health = NativeHealth = CreateNativeHealth();
		Rendering = NativeRenderer = CreateNativeRenderer();
		Buff = new CharacterBuff(this);

		// Init Inventory
		if (InventoryType == CharacterInventoryType.Unique) {
			InventoryID = Inventory.InitializeInventoryData(
				TypeName, INVENTORY_COLUMN * INVENTORY_ROW, hasEquipment: true
			);
		}

	}


	public override void OnActivated () {
		base.OnActivated();

		if (FromWorld) {
			X = X.ToUnifyGlobal() + Const.HALF;
		}

		// Inv ID
		if (InventoryType == CharacterInventoryType.Map) {
			if (MapUnitPos.HasValue) {
				InventoryID = Inventory.InitializeInventoryData(
					TypeID, TypeName, INVENTORY_COLUMN * INVENTORY_ROW, MapUnitPos.Value, hasEquipment: true
				);
			} else {
				InventoryID = TypeID;
			}
		}

		// Behavour
		Movement = NativeMovement;
		Attackness = NativeAttackness;
		Health = NativeHealth;
		Rendering = NativeRenderer;
		NativeMovement.OnActivated();
		NativeHealth.OnActivated();
		NativeAttackness.OnActivated();
		NativeRenderer.OnActivated();

		// Load Buff from Map
		IWithCharacterBuff.GiveBuffFromMap(this);

		// Misc
		CharacterState = CharacterState.GamePlay;
		PassOutFrame = int.MinValue;
		VelocityX = 0;
		VelocityY = 0;
		MovementOverride = null;
		AttacknessOverride = null;
		HealthOverride = null;
		RendererOverride = null;
		OverridingMovementFrame = int.MinValue;
		OverridingAttacknessFrame = int.MinValue;
		OverridingHealthFrame = int.MinValue;
		OverridingRendererFrame = int.MinValue;
		DespawnAfterPassoutDelay = 60;
		Bouncy = 150;
		bool allowInv = InventoryType != CharacterInventoryType.None;
		HelmetInteractable = allowInv;
		BodySuitInteractable = allowInv;
		GlovesInteractable = allowInv;
		ShoesInteractable = allowInv;
		JewelryInteractable = allowInv;
		HandToolInteractable = allowInv;
	}


	public override void OnInactivated () {
		base.OnInactivated();
		MovementOverride = null;
		AttacknessOverride = null;
	}


	public override void AfterReposition (Int3 fromUnitPos, Int3 toUnitPos) {

		if (InventoryType != CharacterInventoryType.Map) return;

		// Repos Inventory
		Inventory.RepositionInventory(InventoryID, toUnitPos);

	}


	// Physics Update
	public override void FirstUpdate () {

		// Update Behaviour
		Movement = Game.GlobalFrame <= OverridingMovementFrame && MovementOverride != null ? MovementOverride : NativeMovement;
		Attackness = Game.GlobalFrame <= OverridingAttacknessFrame && AttacknessOverride != null ? AttacknessOverride : NativeAttackness;
		Health = Game.GlobalFrame <= OverridingHealthFrame && HealthOverride != null ? HealthOverride : NativeHealth;
		Rendering = Game.GlobalFrame <= OverridingRendererFrame && RendererOverride != null ? RendererOverride : NativeRenderer;

		// Fill Physics
		bool trigger = Game.GlobalFrame <= ForceTriggerFrame;
		if (CharacterState == CharacterState.GamePlay && !IgnorePhysics) {
			Physics.FillEntity(PhysicalLayer, this, trigger);
		}
		if (trigger) {
			Movement.PushAvailable.Override(false, priority: 4096);
		}

		RefreshPrevPosition();
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
		IsGrounded = GroundedCheck();
		BeforeUpdate_Inventory();
		Buff.ApplyOnBeforeUpdate();
		Rendering.BeforeUpdate();
	}


	private void BeforeUpdate_Inventory () {

		int invCapacity = Inventory.GetInventoryCapacity(InventoryID);
		if (invCapacity <= 0) return;

		// Inventory
		for (int i = 0; i < invCapacity; i++) {
			int id = Inventory.GetItemAt(InventoryID, i);
			var item = id != 0 ? ItemSystem.GetItem(id) : null;
			if (
				item == null ||
				!item.ItemConditionCheck(this) ||
				!item.CheckUpdateAvailable(TypeID)
			) continue;
			item.BeforeItemUpdate_FromInventory(this, InventoryID, i);
		}

		// Equipping
		ResetInventoryUpdate(invCapacity);
		for (int i = 0; i < Const.EquipmentTypeCount; i++) {
			var type = (EquipmentType)i;
			int id = Inventory.GetEquipment(InventoryID, type, out int equipmentCount);
			var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			if (item == null || !item.ItemConditionCheck(this)) continue;
			item.BeforeItemUpdate_FromEquipment(this);
			if (item is HandTool tool) {
				Attackness.AttackDuration = tool.Duration;
				Attackness.AttackCooldown = tool.Cooldown;
				Attackness.MinimalChargeAttackDuration = tool.ChargeDuration;
				Attackness.RepeatAttackWhenHolding = tool.RepeatWhenHolding;
				Attackness.LockFacingOnAttack = tool.LockFacingOnUse;
				Attackness.HoldAttackPunishFrame.Min(tool.HoldPunish, 1);
				if (tool.CancelUseWhenRelease && Attackness.IsAttacking && !Attackness.HoldingAttack) {
					Attackness.CancelAttack();
				}
				if (tool.DefaultMovementSpeedRateOnUse.HasValue) {
					Attackness.DefaultSpeedRateOnAttack.Max(tool.DefaultMovementSpeedRateOnUse.Value, 1);
				}
				if (tool.WalkingMovementSpeedRateOnUse.HasValue) {
					Attackness.WalkingSpeedRateOnAttack.Max(tool.WalkingMovementSpeedRateOnUse.Value, 1);
				}
				if (tool.RunningMovementSpeedRateOnUse.HasValue) {
					Attackness.RunningSpeedRateOnAttack.Max(tool.RunningMovementSpeedRateOnUse.Value, 1);
				}
				Attackness.AttackInAir.Override(tool.AvailableInAir, 1);
				Attackness.AttackInWater.Override(tool.AvailableInWater, 1);
				Attackness.AttackWhenWalking.Override(tool.AvailableWhenWalking, 1);
				Attackness.AttackWhenRunning.Override(tool.AvailableWhenRunning, 1);
				Attackness.AttackWhenClimbing.Override(tool.AvailableWhenClimbing, 1);
				Attackness.AttackWhenFlying.Override(tool.AvailableWhenFlying, 1);
				Attackness.AttackWhenRolling.Override(tool.AvailableWhenRolling, 1);
				Attackness.AttackWhenSquatting.Override(tool.AvailableWhenSquatting, 1);
				Attackness.AttackWhenDashing.Override(tool.AvailableWhenDashing, 1);
				Attackness.AttackWhenSliding.Override(tool.AvailableWhenSliding, 1);
				Attackness.AttackWhenGrabbing.Override(tool.AvailableWhenGrabbing, 1);
				Attackness.AttackWhenRush.Override(tool.AvailableWhenRushing, 1);
				Attackness.AttackWhenPounding.Override(tool.AvailableWhenPounding, 1);
			}
		}

	}


	public override void Update () {

		if (!Active) return;

		if (Health.IsEmptyHealth) {
			SetCharacterState(CharacterState.PassOut);
		} else if (CharacterState == CharacterState.PassOut) {
			SetCharacterState(CharacterState.GamePlay);
		}

		if (Teleporting) {
			var _poseType = GetCurrentPoseAnimationType();
			if (_poseType != AnimationType) {
				Rendering.CurrentAnimationFrame = 0;
				AnimationType = _poseType;
			}
			Movement.FacingFront = TeleportToFrontSide;
			IgnorePhysics.True(1);
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
				IgnorePhysics.True(1);
				break;

			case CharacterState.PassOut:
				VelocityX = 0;
				break;
		}

		Movement.UpdateLater();

		// Ani Frame
		var poseType = GetCurrentPoseAnimationType();
		if (poseType != AnimationType) {
			Rendering.CurrentAnimationFrame = 0;
			AnimationType = poseType;
		}
		base.Update();
	}


	private void Update_RepairEquipment () {
		if (Health.TakingDamage || Game.GlobalFrame != Movement.LastSquatStartFrame + 1) return;
		TryRepairAllEquipments();
	}


	// Late Update
	public override void LateUpdate () {
		if (!Active) {
			base.LateUpdate();
			return;
		}
		LateUpdate_RenderCharacter();
		LateUpdate_Event();
		LateUpdate_Inventory();
		Buff.ApplyOnLateUpdate();
		base.LateUpdate();
		LateUpdate_BouceHighlight();
	}


	private void LateUpdate_RenderCharacter () {

		bool blinking = Health.IsInvincible && !Health.TakingDamage && (Game.GlobalFrame - Health.InvincibleEndFrame).UMod(8) < 4;

		bool colorFlash = Health.TakingDamage && Health.HP >= 0 && (Game.GlobalFrame - Health.LastDamageFrame).UMod(8) < 4;
		RenderingCellIndex = colorFlash ? -1 : Renderer.GetUsedCellCount(RenderLayer.DEFAULT);
		using var _ = new LayerScope(colorFlash ? RenderLayer.COLOR : RenderLayer.DEFAULT);
		int cellIndexStart = Renderer.GetUsedCellCount();

		// Render
		Rendering.UpdateForBounce();
		Rendering.LateUpdate();
		Rendering.GrowAnimationFrame();

		// Flash Cell Effect
		if ((blinking || colorFlash) && Renderer.GetCells(out var cells, out int count)) {
			var targetColor = blinking ? Color32.CLEAR : Color32.WHITE;
			for (int i = cellIndexStart; i < count; i++) {
				var cell = cells[i];
				cell.Color = targetColor;
			}
		}

		// Portal Teleporting
		if (Teleporting && _TeleportDuration < 0) {
			int pointX = X;
			int pointY = Y + Const.CEL;
			int duration = _TeleportDuration.Abs();
			int localFrame = Game.GlobalFrame - _TeleportEndFrame.Abs() + duration;
			FrameworkUtil.SpiralSpinningCellEffect(localFrame, pointX, pointY, duration, cellIndexStart);
		}

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
					FrameworkUtil.InvokeOnCharacterTeleport(targetCharacter);
				}

				// Step
				if (IsGrounded) {
					if (
						(AnimationType == CharacterAnimationType.Brake && frame % 4 == 0) ||
						(Movement.LastStartRunFrame >= 0 && (frame - Movement.LastStartRunFrame) % 20 == 19) || // Run
						(Movement.IsDashing && (frame - Movement.LastDashFrame) % 8 == 0) || // Dash
						(Movement.IsRushing && (frame - Movement.LastRushStartFrame) % 3 == 0) // Rush
					) {
						FrameworkUtil.InvokeOnFootStepped(targetCharacter);
					}
				}

				if (Movement.IsSliding && frame % 24 == 0) {
					FrameworkUtil.InvokeOnCharacterSlideStepped(targetCharacter);
				}

				if (frame == Movement.LastJumpFrame || frame == Movement.LastGroundFrame) {
					FrameworkUtil.InvokeOnCharacterJump(targetCharacter);
				}

				if (IsGrounded && !Movement.IsPounding && frame == Movement.LastPoundingFrame + 1) {
					FrameworkUtil.InvokeOnCharacterPound(targetCharacter);
				}

				if (frame == Movement.LastFlyFrame) {
					FrameworkUtil.InvokeOnCharacterFly(targetCharacter);
				}

				if (frame == Movement.LastCrashFrame) {
					FrameworkUtil.InvokeOnCharacterCrash(targetCharacter);
				}
			}
		}

		// Sleep
		if (CharacterState == CharacterState.Sleep) {
			// ZZZ Particle
			if (frame % 42 == 0) {
				FrameworkUtil.InvokeOnCharacterSleeping(this);
			}
		}
	}


	private void LateUpdate_Inventory () {

		int invCapacity = Inventory.GetInventoryCapacity(InventoryID);
		if (invCapacity > 0) {

			bool eventAvailable = CharacterState == CharacterState.GamePlay && !TaskSystem.HasTask() && !Health.TakingDamage;
			int performLocalFrame = eventAvailable && Attackness.IsAttacking ? Game.GlobalFrame - Attackness.LastAttackFrame : -1;

			// Equipping
			bool attacking = false;
			Bullet attackingBullet = null;
			for (int i = 0; i < Const.EquipmentTypeCount; i++) {
				int id = Inventory.GetEquipment(InventoryID, (EquipmentType)i, out int equipmentCount);
				var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
				if (item == null || !item.ItemConditionCheck(this)) continue;
				item.OnItemUpdate_FromEquipment(this);
				if (item is HandTool tool && performLocalFrame == tool.PerformDelayFrame) {
					if (tool is Weapon weapon) {
						attackingBullet = weapon.SpawnBullet(this);
					} else {
						tool.OnToolPerform(this);
					}
					item.OnCharacterAttack_FromEquipment(this, attackingBullet);
					Buff.ApplyOnAttack(attackingBullet);
					attacking = true;
				}
			}

			// Inventory
			ResetInventoryUpdate(invCapacity);
			for (int i = 0; i < invCapacity; i++) {
				int id = Inventory.GetItemAt(InventoryID, i);
				var item = id != 0 ? ItemSystem.GetItem(id) : null;
				if (item == null || !item.ItemConditionCheck(this) || !item.CheckUpdateAvailable(TypeID)) continue;
				item.OnItemUpdate_FromInventory(this, InventoryID, i);
				if (attacking) {
					item.OnCharacterAttack_FromInventory(this, attackingBullet, InventoryID, i);
				}
			}

		}

	}


	private void LateUpdate_BouceHighlight () {
		if (this is not IActionTarget act || !act.IsHighlighted) return;
		// Bounce
		if (Game.GlobalFrame % 20 == 0) Bounce();
		// Hint
		ControlHintUI.DrawGlobalHint(
			X - Const.HALF, Y + Const.CEL * 2,
			Gamekey.Action, BuiltInText.HINT_SWITCH_PLAYER, true
		);
	}


	#endregion




	#region --- API ---


	public virtual void SetCharacterState (CharacterState state) {

		if (CharacterState == state) return;

		PassOutFrame = int.MinValue;
		CharacterState = state;

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
				FrameworkUtil.InvokeOnCharacterPassOut(this);
				break;

		}

	}


	public virtual CharacterAnimationType GetCurrentPoseAnimationType () {
		if (Game.GlobalFrame <= LockedAnimationTypeFrame) return LockedAnimationType;
		if (Teleporting) return _TeleportDuration < 0 ? CharacterAnimationType.Rolling : CharacterAnimationType.Idle;
		if (Health.TakingDamage) return CharacterAnimationType.TakingDamage;
		if (CharacterState == CharacterState.Sleep) return CharacterAnimationType.Sleep;
		if (CharacterState == CharacterState.PassOut) return CharacterAnimationType.PassOut;
		if (Movement.IsRolling) return CharacterAnimationType.Rolling;
		return Movement.MovementState switch {
			CharacterMovementState.Walk => CharacterAnimationType.Walk,
			CharacterMovementState.Run =>
				Movement.Target.VelocityX != 0 && Movement.IntendedX.Sign() == Movement.Target.VelocityX.Sign() ?
				CharacterAnimationType.Run : CharacterAnimationType.Brake,
			CharacterMovementState.JumpUp => CharacterAnimationType.JumpUp,
			CharacterMovementState.JumpDown => CharacterAnimationType.JumpDown,
			CharacterMovementState.SwimIdle => CharacterAnimationType.SwimIdle,
			CharacterMovementState.SwimMove => CharacterAnimationType.SwimMove,
			CharacterMovementState.SquatIdle => CharacterAnimationType.SquatIdle,
			CharacterMovementState.SquatMove => CharacterAnimationType.SquatMove,
			CharacterMovementState.Dash => CharacterAnimationType.Dash,
			CharacterMovementState.Rush => CharacterAnimationType.Rush,
			CharacterMovementState.Crash => CharacterAnimationType.Crash,
			CharacterMovementState.Pound => Rendering.SpinOnGroundPound ? CharacterAnimationType.Spin : CharacterAnimationType.Pound,
			CharacterMovementState.Climb => CharacterAnimationType.Climb,
			CharacterMovementState.Fly => CharacterAnimationType.Fly,
			CharacterMovementState.Slide => CharacterAnimationType.Slide,
			CharacterMovementState.GrabTop => CharacterAnimationType.GrabTop,
			CharacterMovementState.GrabSide => CharacterAnimationType.GrabSide,
			CharacterMovementState.GrabFlip => CharacterAnimationType.Rolling,
			_ => CharacterAnimationType.Idle,
		};
	}


	public virtual void OnCharacterRendered () => Buff.ApplyOnCharacterRenderered();


	public void EnterTeleportState (bool front, bool portal, bool lastHalfOnly = false) {
		int duration = portal ? -TeleportDuration : TeleportDuration;
		if (portal) {
			duration *= 2;
		}
		_TeleportEndFrame = (Game.GlobalFrame + duration.Abs()) * (front ? 1 : -1);
		_TeleportDuration = duration;
		if (lastHalfOnly) {
			_TeleportEndFrame -= _TeleportEndFrame.Sign() * duration.Abs() / 2;
		}
	}


	public void LockAnimationType (CharacterAnimationType type, int duration = 1) {
		LockedAnimationType = type;
		LockedAnimationTypeFrame = Game.GlobalFrame + duration;
	}


	public void GetBonusFromFullSleep () {
		Health.Heal(Health.MaxHP);
		Buff.ClearAllBuffs();
		Movement.StopCrash();
	}


	protected override bool InsideGroundCheck () {
		if (IgnoreInsideGround) return IsInsideGround;
		int mask = PhysicsMask.MAP & CollisionMask;
		if (mask == 0) return false;
		var rect = Rect;
		var point = rect.CenterInt();
		int shiftY = rect.height / 4;
		return
			Physics.Overlap(mask, IRect.Point(point.ShiftY(-shiftY)), this) ||
			Physics.Overlap(mask, IRect.Point(point.ShiftY(+shiftY)), this)
		;
	}


	// Damage
	public virtual void OnDamaged (Damage damage) {

		if (!Active || damage.Amount <= 0 || Health.HP <= 0) return;
		if (CharacterState != CharacterState.GamePlay || Health.IsInvincible) return;
		if (Health.InvincibleOnRush && Movement.IsRushing) return;
		if (Health.InvincibleOnDash && Movement.IsDashing) return;

		// Inventory
		int invCapacity = Inventory.GetInventoryCapacity(InventoryID);
		for (int i = 0; i < invCapacity && damage.Amount > 0; i++) {
			int id = Inventory.GetItemAt(InventoryID, i);
			var item = id != 0 ? ItemSystem.GetItem(id) : null;
			if (item == null || !item.ItemConditionCheck(this) || !item.CheckUpdateAvailable(TypeID)) continue;
			item.OnTakeDamage_FromInventory(this, InventoryID, i, ref damage);
		}

		// Equipment
		ResetInventoryUpdate(invCapacity);
		for (int i = 0; i < Const.EquipmentTypeCount && damage.Amount > 0; i++) {
			int id = Inventory.GetEquipment(InventoryID, (EquipmentType)i, out int equipmentCount);
			var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			if (item == null || !item.ItemConditionCheck(this)) continue;
			item.OnTakeDamage_FromEquipment(this, ref damage);
		}

		// Deal Damage
		damage.Amount = damage.Amount.GreaterOrEquelThanZero();
		Health.HP = (Health.HP - damage.Amount).Clamp(0, Health.MaxHP);

		// Knock Back
		if (!damage.IgnoreStun) {
			VelocityX = damage.Bullet != null && damage.Bullet.Rect.CenterX() > Rect.CenterX() ? -Health.KnockbackSpeed : Health.KnockbackSpeed;
			Health.LastDamageFrame = Game.GlobalFrame;
		}

		// Make Invincible
		if (!damage.IgnoreInvincible) {
			Health.InvincibleEndFrame = Game.GlobalFrame + Health.InvincibleDuration;
		}

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
		EquipmentType.HandTool => HandToolInteractable,
		EquipmentType.BodyArmor => BodySuitInteractable,
		EquipmentType.Helmet => HelmetInteractable,
		EquipmentType.Shoes => ShoesInteractable,
		EquipmentType.Gloves => GlovesInteractable,
		EquipmentType.Jewelry => JewelryInteractable,
		_ => false,
	};


	public int TryRepairAllEquipments (bool requireMultiple = false) {
		int repairedCount = 0;
		for (int i = 0; i < Const.EquipmentTypeCount; i++) {
			int id = Inventory.GetEquipment(InventoryID, (EquipmentType)i, out int equipmentCount);
			var item = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as Equipment : null;
			if (item == null) continue;
			if (item.TryRepairEquipment(this)) {
				repairedCount++;
				if (!requireMultiple) break;
			}
		}
		return repairedCount;
	}


	// Behaviour
	protected virtual CharacterMovement CreateNativeMovement () => new(this);
	protected virtual CharacterAttackness CreateNativeAttackness () => new(this);
	protected virtual CharacterHealth CreateNativeHealth () => new();
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
		int id = Inventory.GetEquipment(InventoryID, EquipmentType.HandTool, out int equipmentCount);
		var tool = id != 0 && equipmentCount >= 0 ? ItemSystem.GetItem(id) as HandTool : null;
		return tool != null && tool.AllowingUse(this);
	}


	public void ForceFillTrigger (int duration = 1) => ForceTriggerFrame = Game.GlobalFrame + duration;


	public void IgnoreDamageFromLevel (int duration = 1) => IgnoreDamageFromLevelFrame = Game.GlobalFrame + duration;


	public string GetDisplayName () => Language.Get(DisplayNameID, TypeName);


	public string GetDescription () => Language.Get(DescriptionID, "");


	#endregion




}
