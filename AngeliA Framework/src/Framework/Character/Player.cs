using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL * 2)]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDrawBehind]
public abstract class Player : PoseCharacter, IDamageReceiver, IActionTarget {




	#region --- VAR ---


	// Const
	private const int RUSH_TAPPING_GAP = 16;
	private const int ACTION_SCAN_RANGE = Const.HALF;
	private static readonly LanguageCode HINT_WAKE = ("CtrlHint.WakeUp", "Wake");
	private static readonly LanguageCode HINT_SWITCH_PLAYER = ("CtrlHint.SwitchPlayer", "Select Player");

	// Api
	public static Player Selecting { get; private set; } = null;
	public static Int3? RespawnCpUnitPosition { get; set; } = null;
	public static Int3? HomeUnitPosition {
		get {
			if (
				HomeUnitPositionX.Value == int.MinValue ||
				HomeUnitPositionY.Value == int.MinValue ||
				HomeUnitPositionZ.Value == int.MinValue
			) return null;
			return new Int3(HomeUnitPositionX.Value, HomeUnitPositionY.Value, HomeUnitPositionZ.Value);
		}
		set {
			if (value.HasValue) {
				HomeUnitPositionX.Value = value.Value.x;
				HomeUnitPositionY.Value = value.Value.y;
				HomeUnitPositionZ.Value = value.Value.z;
			} else {
				HomeUnitPositionX.Value = int.MinValue;
				HomeUnitPositionY.Value = int.MinValue;
				HomeUnitPositionZ.Value = int.MinValue;
			}

		}
	}
	public bool LockingInput => Game.GlobalFrame <= LockInputFrame;
	public int LockInputFrame { get; private set; } = -1;
	public int AimViewX { get; private set; } = 0;
	public int AimViewY { get; private set; } = 0;
	public IActionTarget TargetActionEntity { get; private set; } = null;
	public virtual bool HelmetAvailable => true;
	public virtual bool BodySuitAvailable => true;
	public virtual bool GlovesAvailable => true;
	public virtual bool ShoesAvailable => true;
	public virtual bool JewelryAvailable => true;
	public virtual bool WeaponAvailable => true;
	public virtual bool AllowPlayerMenuUI => InventoryCurrentAvailable;
	public virtual bool AllowQuickPlayerMenuUI => InventoryCurrentAvailable;
	int IDamageReceiver.Team => Const.TEAM_PLAYER;
	public override bool AllowInventory => true;
	public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT;

	// Data
	private int AttackRequiringFrame = int.MinValue;
	private int LastLeftKeyDown = int.MinValue;
	private int LastRightKeyDown = int.MinValue;
	private int LastGroundedY = 0;
	private int PrevZ = int.MinValue;
	private int IgnoreActionFrame = -1;
	private PlayerAttackness PlayerAttackness;

	// Saving
	private static readonly SavingInt LastPlayerID = new("Player.LastPlayerID", 0, SavingLocation.Slot);
	private static readonly SavingInt HomeUnitPositionX = new("Player.HomeX", int.MinValue, SavingLocation.Slot);
	private static readonly SavingInt HomeUnitPositionY = new("Player.HomeY", int.MinValue, SavingLocation.Slot);
	private static readonly SavingInt HomeUnitPositionZ = new("Player.HomeZ", int.MinValue, SavingLocation.Slot);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	[OnSavingSlotChanged]
	public static TaskResult OnGameInitializeLaterPlayer () {
		if (!Stage.IsReady) return TaskResult.Continue;
		SelectPlayer(LastPlayerID.Value);
		return TaskResult.End;
	}


	protected override CharacterAttackness CreateNativeAttackness () => PlayerAttackness = new PlayerAttackness(this);


	public override void OnActivated () {
		base.OnActivated();
		PrevZ = Stage.ViewZ;
		LockInputFrame = -1;
		TargetActionEntity = null;
		Inventory.SetUnlockItemsInside(TypeID, true);
	}


	public override void OnInactivated () {
		base.OnInactivated();
		if (Selecting == this && PlayerMenuUI.ShowingUI) {
			PlayerMenuUI.CloseMenu();
		}
	}


	public override void FirstUpdate () {
		if (TaskSystem.HasTask()) return;
		base.FirstUpdate();
	}


	// Before Physics Update
	public override void BeforeUpdate () {

		base.BeforeUpdate();

		// Non-Selecting Players Despawn on Z Changed
		if (PrevZ != Stage.ViewZ) {
			PrevZ = Stage.ViewZ;
			if (Selecting != this) {
				Active = false;
				return;
			} else {
				Bounce();
			}
		}

		// Stop when Not Selecting/Playing
		if (Selecting != this || Game.IsPausing) {
			Movement.Stop();
			return;
		}

		// Update Player
		switch (CharacterState) {
			case CharacterState.GamePlay:

				bool allowGamePlay = !TaskSystem.HasTask() && !GUI.IsTyping;

				if (allowGamePlay) {
					if (PlayerMenuUI.ShowingUI || PlayerQuickMenuUI.ShowingUI) {
						LockInput(0);
					}
				}

				if (allowGamePlay && !LockingInput) {

					// Update Aiming
					Update_Aiming();

					// Move
					Movement.Move(Input.DirectionX, Input.DirectionY);

					// Walk when Holding Up
					if (Input.GameKeyHolding(Gamekey.Up)) {
						Movement.ClearRunningAccumulate();
					}

					// Movement Actions
					Update_JumpDashPoundRush();

					// Hint
					ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
				} else {
					Movement.Stop();
				}

				if (allowGamePlay) {
					Update_Action();
					Update_Attack();
					Update_InventoryUI();
				}

				break;
			case CharacterState.Sleep:
				Update_Sleep();
				break;
			case CharacterState.PassOut:
				Update_PassOut();
				break;
		}

		// View
		Update_View();

	}


	private void Update_Aiming () {
		PlayerAttackness._AimingDirection =
			Input.Direction.TryGetDirection8(out var result) ? result :
			Movement.FacingRight ? Direction8.Right : Direction8.Left;
		// Ignore Check
		if (Attackness.IsAimingDirectionIgnored(PlayerAttackness._AimingDirection)) {
			var dir0 = PlayerAttackness._AimingDirection;
			var dir1 = PlayerAttackness._AimingDirection;
			for (int safe = 0; safe < 4; safe++) {
				dir0 = Movement.FacingRight ? dir0.Clockwise() : dir0.AntiClockwise();
				dir1 = Movement.FacingRight ? dir1.AntiClockwise() : dir1.Clockwise();
				if (!Attackness.IsAimingDirectionIgnored(dir0)) {
					PlayerAttackness._AimingDirection = dir0;
					break;
				}
				if (!Attackness.IsAimingDirectionIgnored(dir1)) {
					PlayerAttackness._AimingDirection = dir1;
					break;
				}
			}
		}
	}


	private void Update_JumpDashPoundRush () {

		if (LockingInput) return;

		ControlHintUI.AddHint(Gamekey.Jump, BuiltInText.HINT_JUMP);

		// Jump/Dash
		Movement.HoldJump(Input.GameKeyHolding(Gamekey.Jump));
		if (Input.GameKeyDown(Gamekey.Jump)) {
			// Movement Jump
			if (Input.GameKeyHolding(Gamekey.Down)) {
				Movement.Dash();
			} else {
				Movement.Jump();
				if (Attackness.CancelAttackOnJump) Attackness.CancelAttack();
				AttackRequiringFrame = int.MinValue;
			}
		}

		// Pound
		if (Input.GameKeyDown(Gamekey.Down)) {
			Movement.Pound();
		}

		// Rush
		if (Input.GameKeyDown(Gamekey.Left)) {
			if (
				Game.GlobalFrame < LastLeftKeyDown + RUSH_TAPPING_GAP &&
				!Input.GameKeyHolding(Gamekey.Up)
			) {
				Movement.Rush();
			}
			LastLeftKeyDown = Game.GlobalFrame;
			LastRightKeyDown = int.MinValue;
		}
		if (Input.GameKeyDown(Gamekey.Right)) {
			if (
				Game.GlobalFrame < LastRightKeyDown + RUSH_TAPPING_GAP &&
				!Input.GameKeyHolding(Gamekey.Up)
			) {
				Movement.Rush();
			}
			LastRightKeyDown = Game.GlobalFrame;
			LastLeftKeyDown = int.MinValue;
		}

	}


	private void Update_Action () {

		if (Health.TakingDamage || TaskSystem.HasTask() || Game.GlobalFrame <= IgnoreActionFrame) {
			TargetActionEntity = null;
			return;
		}

		// Search for Active Trigger
		if (TargetActionEntity == null || !TargetActionEntity.LockInput) {
			TargetActionEntity = null;
			Entity eActTarget = null;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC,
				Rect.Expand(ACTION_SCAN_RANGE, ACTION_SCAN_RANGE, 0, ACTION_SCAN_RANGE),
				out int count, this,
				OperationMode.ColliderAndTrigger
			);
			int dis = int.MaxValue;
			bool squatting = Movement.IsSquatting;
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not IActionTarget act) continue;
				if (!act.AllowInvoke()) continue;
				if (squatting) {
					if (!act.AllowInvokeOnSquat) continue;
				} else {
					if (!act.AllowInvokeOnStand) continue;
				}
				// Comparer X Distance
				int _dis =
					X >= hit.Rect.xMin && X <= hit.Rect.xMax ? 0 :
					X > hit.Rect.xMax ? Util.Abs(X - hit.Rect.xMax) :
					Util.Abs(X - hit.Rect.xMin);
				if (_dis < dis) {
					dis = _dis;
					TargetActionEntity = act;
					eActTarget = hit.Entity;
				} else if (_dis == dis && TargetActionEntity != null) {
					// Comparer Y Distance
					if (hit.Entity.Y < eActTarget.Y) {
						dis = _dis;
						TargetActionEntity = act;
						eActTarget = hit.Entity;
					} else if (hit.Entity.Rect.y == eActTarget.Y) {
						// Comparer Size
						if (hit.Entity.Width * hit.Entity.Height < eActTarget.Width * eActTarget.Height) {
							dis = _dis;
							TargetActionEntity = act;
							eActTarget = hit.Entity;
						}
					}
				}
			}
		}

		// Try Perform Action
		if (TargetActionEntity != null && !TargetActionEntity.LockInput) {
			ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_USE, int.MinValue + 1);
			if (Input.GameKeyDown(Gamekey.Action) && !PlayerMenuUI.ShowingUI && TargetActionEntity != null) {
				if (TargetActionEntity.Invoke()) {
					Input.UseGameKey(Gamekey.Action);
				}
			}
		}

		if (TargetActionEntity != null && TargetActionEntity.LockInput) {
			LockInput(1);
		}

	}


	private void Update_Attack () {

		if (LockingInput) return;

		// Try Perform Attack
		ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_ATTACK);
		bool attDown = Input.GameKeyDown(Gamekey.Action);
		bool attHolding = Input.GameKeyHolding(Gamekey.Action) && Attackness.RepeatAttackWhenHolding;
		if (attDown || attHolding) {
			if (Game.GlobalFrame >= Attackness.LastAttackFrame + Attackness.AttackDuration + Attackness.AttackCooldown + (attDown ? 0 : Attackness.HoldAttackPunish)) {
				Attackness.Attack(Movement.FacingRight);
			} else if (attDown) {
				AttackRequiringFrame = Game.GlobalFrame;
			}
			return;
		}

		// Reset Require on Move
		if (
			Input.GameKeyDown(Gamekey.Left) || Input.GameKeyDown(Gamekey.Right) ||
			Input.GameKeyDown(Gamekey.Down) || Input.GameKeyDown(Gamekey.Up)
		) {
			AttackRequiringFrame = int.MinValue;
		}

		// Perform Required Attack
		const int ATTACK_REQUIRE_GAP = 12;
		if (
			Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP &&
			Game.GlobalFrame >= Attackness.LastAttackFrame + Attackness.AttackDuration + Attackness.AttackCooldown
		) {
			AttackRequiringFrame = int.MinValue;
			Attackness.Attack(Movement.FacingRight);
		}

	}


	private void Update_InventoryUI () {

		if (!AllowPlayerMenuUI && PlayerMenuUI.ShowingUI) {
			PlayerMenuUI.CloseMenu();
		}
		bool requireHint = false;

		// Quick Menu
		if (AllowQuickPlayerMenuUI && !PlayerMenuUI.ShowingUI && !PlayerQuickMenuUI.ShowingUI && !LockingInput) {
			if (Input.GameKeyDown(Gamekey.Select) || Input.GameKeyDown(Gamekey.Start)) {
				PlayerQuickMenuUI.OpenMenu();
			}
			requireHint = true;
		}

		// Inventory Menu
		if (
			AllowPlayerMenuUI &&
			!PlayerMenuUI.ShowingUI &&
			(PlayerQuickMenuUI.ShowingUI || !LockingInput) &&
			(!PlayerQuickMenuUI.ShowingUI || !PlayerQuickMenuUI.Instance.IsDirty)
		) {
			if (Input.GameKeyUp(Gamekey.Select)) {
				Input.UseGameKey(Gamekey.Select);
				PlayerMenuUI.OpenMenu();
			}
			requireHint = true;
		}

		// Hint
		if (requireHint) {
			ControlHintUI.AddHint(
				Input.UsingGamepad ? Gamekey.Start : Gamekey.Select,
				BuiltInText.HINT_SHOW_MENU
			);
		}
	}


	private void Update_Sleep () {

		TargetActionEntity = null;

		if (TaskSystem.HasTask()) return;

		// Wake up on Press Action
		if (Input.GameKeyDown(Gamekey.Action) || Input.GameKeyDown(Gamekey.Jump)) {
			Input.UseGameKey(Gamekey.Action);
			Input.UseGameKey(Gamekey.Jump);
			SetCharacterState(CharacterState.GamePlay);
			Y -= 4;
		}

		// Hint
		ControlHintUI.DrawGlobalHint(
			X - Const.HALF, Y + Const.CEL * 3 / 2,
			Gamekey.Action, HINT_WAKE, background: true
		);
		ControlHintUI.AddHint(Gamekey.Action, HINT_WAKE);
	}


	private void Update_View () {

		const int LINGER_RATE = 32;
		bool notInGameplay = TaskSystem.HasTask() || CharacterState != CharacterState.GamePlay;
		bool notInAir =
			notInGameplay ||
			IsGrounded || InWater || Movement.IsSliding ||
			Movement.IsClimbing || Movement.IsGrabbingSide || Movement.IsGrabbingTop;

		if (notInAir || Movement.IsFlying) LastGroundedY = Y;

		// Aim X
		int linger = Stage.ViewRect.width * LINGER_RATE / 1000;
		int centerX = Stage.ViewRect.x + Stage.ViewRect.width / 2;
		if (notInGameplay) {
			AimViewX = X - Stage.ViewRect.width / 2;
		} else if (X < centerX - linger) {
			AimViewX = X + linger - Stage.ViewRect.width / 2;
		} else if (X > centerX + linger) {
			AimViewX = X - linger - Stage.ViewRect.width / 2;
		}

		// Aim Y
		AimViewY = Y <= LastGroundedY ? Y - GetCameraShiftOffset(Stage.ViewRect.height) : AimViewY;
		Stage.SetViewPositionDelay(AimViewX, AimViewY, 96, int.MinValue);

		// Clamp
		if (!Stage.ViewRect.Contains(X, Y)) {
			if (X >= Stage.ViewRect.xMax) AimViewX = X - Stage.ViewRect.width + 1;
			if (X <= Stage.ViewRect.xMin) AimViewX = X - 1;
			if (Y >= Stage.ViewRect.yMax) AimViewY = Y - Stage.ViewRect.height + 1;
			if (Y <= Stage.ViewRect.yMin) AimViewY = Y - 1;
			Stage.SetViewPositionDelay(AimViewX, AimViewY, 1000, int.MinValue + 1);
		}

	}


	private void Update_PassOut () {

		if (TaskSystem.HasTask()) return;

		bool fullPassOut = Health.HP == 0 && Game.GlobalFrame > PassOutFrame + 48;

		if (fullPassOut) {
			ControlHintUI.DrawGlobalHint(
				X - Const.HALF, Y + Const.CEL * 3 / 2,
				Gamekey.Action, BuiltInText.UI_CONTINUE, background: true
			);
		}

		// Close Menu UI
		if (PlayerMenuUI.ShowingUI) PlayerMenuUI.CloseMenu();

		// Reload Game After Player PassOut
		if (fullPassOut && Input.GameKeyDown(Gamekey.Action)) {
			TaskSystem.AddToLast(RestartGameTask.TYPE_ID);
			Input.UseGameKey(Gamekey.Action);
		}

	}


	// Physics Update
	public override void Update () {
		if (Selecting != this && !Stage.ViewRect.Overlaps(GlobalBounds)) return;
		base.Update();
		Update_Collect();
		Update_Repair();
	}


	private void Update_Collect () {
		if (Selecting != this) return;
		var hits = Physics.OverlapAll(
			PhysicsMask.DYNAMIC, Rect, out int count, this, OperationMode.TriggerOnly
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Collectable col) continue;
			bool success = col.OnCollect(this);
			if (success) {
				hit.Entity.Active = false;
				Stage.MarkAsGlobalAntiSpawn(hit.Entity);
			}
		}
	}


	private void Update_Repair () {
		if (
			CharacterState != CharacterState.GamePlay ||
			TaskSystem.HasTask() ||
			Health.TakingDamage ||
			Game.GlobalFrame != Movement.LastSquatFrame
		) return;
		for (int i = 0; i < EquipmentTypeCount; i++) {
			var item = GetEquippingItem((EquipmentType)i);
			if (item == null) continue;
			if (item.TryRepair(this)) break;
		}
	}


	// Frame Update
	public override void LateUpdate () {

		int oldZ = PoseRenderingZOffset;
		if (Selecting == this) PoseRenderingZOffset = 40;

		// Equipping
		int equippingID = Inventory.GetEquipment(TypeID, EquipmentType.Weapon, out _);
		if (equippingID != 0 && ItemSystem.GetItem(equippingID) is Weapon weapon) {
			EquippingWeaponType = weapon.WeaponType;
			EquippingWeaponHeld = weapon.Handheld;
		} else {
			EquippingWeaponType = WeaponType.Hand;
			EquippingWeaponHeld = WeaponHandheld.Float;
		}

		base.LateUpdate();
		PoseRenderingZOffset = oldZ;

		// Auto Pick Item on Ground
		if (!TaskSystem.HasTask() && !PlayerMenuUI.ShowingUI) {
			var cells = Physics.OverlapAll(PhysicsMask.ITEM, Rect, out int count, null, OperationMode.ColliderAndTrigger);
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				if (cell.Entity is not ItemHolder holder || !holder.Active) continue;
				holder.Collect(this, onlyStackOnExisting: true, ignoreEquipment: equippingID == 0);
			}
		}

		// Bounce when Highlight
		if ((this as IActionTarget).IsHighlighted) {
			// Bounce
			if (Game.GlobalFrame % 20 == 0) Bounce();
			// Hint
			ControlHintUI.DrawGlobalHint(
				X - Const.HALF, Y + Const.CEL * 2,
				Gamekey.Action, HINT_SWITCH_PLAYER, true
			);
		}

	}


	#endregion




	#region --- API ---


	public static int GetCameraShiftOffset (int cameraHeight) => cameraHeight * 382 / 1000;


	public static int GetDefaultPlayerID () {
		System.Type result = null;
		int currentPriority = int.MinValue;
		foreach (var (type, attribute) in Util.AllClassWithAttribute<EntityAttribute.DefaultSelectPlayerAttribute>()) {
			if ((type == typeof(Player) || type.IsSubclassOf(typeof(Player))) && attribute.Priority >= currentPriority) {
				result = type;
				currentPriority = attribute.Priority;
			}
		}
		return result != null ? result.AngeHash() : DefaultPlayer.TYPE_ID;
	}


	public static void SelectPlayer (int playerID) {
		if (Selecting != null && playerID == Selecting.TypeID) return;
		if (playerID == 0) playerID = GetDefaultPlayerID();
		if (playerID != 0 && Stage.PeekOrGetEntity(playerID) is Player player) {
			Selecting = player;
			LastPlayerID.Value = player.TypeID;
		}
	}


	public void MakeHome (Int3 unitPosition) {
		if (unitPosition != HomeUnitPosition) {
			HomeUnitPosition = unitPosition;
		}
	}


	bool IActionTarget.Invoke () {
		RespawnCpUnitPosition = null;
		TaskSystem.AddToLast(SelectPlayerTask.TYPE_ID, TypeID);
		TaskSystem.AddToLast(RestartGameTask.TYPE_ID);
		return true;
	}


	bool IActionTarget.AllowInvoke () => !Health.IsInvincible;


	public bool EquipmentAvailable (EquipmentType equipmentType) => equipmentType switch {
		EquipmentType.Weapon => WeaponAvailable,
		EquipmentType.BodyArmor => BodySuitAvailable,
		EquipmentType.Helmet => HelmetAvailable,
		EquipmentType.Shoes => ShoesAvailable,
		EquipmentType.Gloves => GlovesAvailable,
		EquipmentType.Jewelry => JewelryAvailable,
		_ => false,
	};


	public void LockInput (int duration = 1) => LockInputFrame = Game.GlobalFrame + duration;


	public void IgnoreAction (int duration = 1) => IgnoreActionFrame = Game.GlobalFrame + duration;


	#endregion




}