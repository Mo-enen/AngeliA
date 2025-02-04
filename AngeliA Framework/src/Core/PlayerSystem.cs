using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AngeliA;

public static class PlayerSystem {




	#region --- VAR ---


	// Const
	private const int RUSH_TAPPING_GAP = 16;
	private const int ACTION_SCAN_RANGE = Const.HALF;
	private static readonly LanguageCode HINT_WAKE = ("CtrlHint.WakeUp", "Wake");

	// Api
	public static bool Enable { get; private set; } = false;
	public static Character Selecting { get; private set; } = null;
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
	public static bool AllowPlayerMenuUI => Selecting != null && Selecting.InventoryType != CharacterInventoryType.None && Game.GlobalFrame > IgnorePlayerMenuFrame;
	public static bool AllowQuickPlayerMenuUI => Selecting != null && Selecting.InventoryType != CharacterInventoryType.None && Game.GlobalFrame > IgnorePlayerQuickMenuFrame;
	public static bool IgnoringInput => Game.GlobalFrame <= IgnoreInputFrame;
	public static bool IgnoringAction => Game.GlobalFrame <= IgnoreActionFrame;
	public static int IgnoreInputFrame { get; private set; } = -1;
	public static int AimViewX { get; private set; } = 0;
	public static int AimViewY { get; private set; } = 0;
	public static IActionTarget TargetActionEntity { get; private set; } = null;
	public static int UnlockedPlayerCount => UnlockedPlayer.Count;
	public static int PlayableCharactersCount => AllPlayablesID.Count;
	public static readonly FrameBasedBool DragPlayerInMiddleButtonToMove_DebugOnly = new(true);

	// Data
	private static readonly HashSet<int> UnlockedPlayer = [];
	private static readonly List<int> AllPlayablesID = [];
	private static int AttackRequiringFrame = int.MinValue;
	private static int LastLeftKeyDown = int.MinValue;
	private static int LastRightKeyDown = int.MinValue;
	private static int LastGroundedY = 0;
	private static int IgnoreActionFrame = -1;
	private static int IgnorePlayerMenuFrame = -1;
	private static int IgnorePlayerQuickMenuFrame = -1;
	private static int IgnoreAttackFrame = int.MinValue;
	private static int ForceUpdateViewGroundingFrame = int.MinValue;
	private static bool UnlockPlayerDirty = false;

	// Frame Based
	public static readonly FrameBasedInt TargetViewHeight = new(Const.CEL * 26);
	public static readonly FrameBasedBool IgnorePlayerView = new(false);

	// Saving
	private static readonly SavingInt SelectingPlayerID = new("Player.SelectingPlayerID", 0, SavingLocation.Slot);
	private static readonly SavingInt HomeUnitPositionX = new("Player.HomeX", int.MinValue, SavingLocation.Slot);
	private static readonly SavingInt HomeUnitPositionY = new("Player.HomeY", int.MinValue, SavingLocation.Slot);
	private static readonly SavingInt HomeUnitPositionZ = new("Player.HomeZ", int.MinValue, SavingLocation.Slot);


	#endregion




	#region --- MSG ---


	// Cheat
	[CheatCode("FillInventory")]
	internal static void CheatCodeFillInventory () {
		if (Selecting == null) return;
		int len = Inventory.GetInventoryCapacity(Selecting.InventoryID);
		for (int i = 0; i < len; i++) {
			int id = Inventory.GetItemAt(Selecting.InventoryID, i);
			if (id == 0) continue;
			int maxCount = ItemSystem.GetItemMaxStackCount(id);
			Inventory.SetItemAt(Selecting.InventoryID, i, id, maxCount);
		}
	}


	[CheatCode("LockAllCharacter")]
	internal static void CheatCode_LockAllCharacters () {
		UnlockedPlayer.Clear();
		if (Selecting != null) {
			UnlockedPlayer.Add(Selecting.TypeID);
		}
		UnlockPlayerDirty = true;
	}


	[CheatCode("UnlockAllCharacter")]
	internal static void CheatCode_UnlockAllCharacters () {
		foreach (int id in AllPlayablesID) {
			UnlockPlayer(id);
		}
	}


	// Message
	[OnRemoteSettingChanged_IntID_IntData(1)]
	internal static void OnRemoteSettingChanged (int id, int data) {
		if (Selecting == null) return;
		switch (id) {
			case Stage.SETTING_SET_VIEW_X: {
				Selecting.X = data.ToGlobal() + Stage.ViewRect.width / 2;
				break;
			}
			case Stage.SETTING_SET_VIEW_Y: {
				Selecting.Y = data.ToGlobal() + Stage.ViewRect.height / 2;
				break;
			}
		}
	}


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		if (Util.TryGetAttributeFromAllAssemblies<EnablePlayerSystemAttribute>()) {
			Enable = true;
		}
		TargetViewHeight.BaseValue = Universe.BuiltInInfo.DefaultViewHeight;
	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		AllPlayablesID.Clear();
		foreach (var ch in typeof(PlayableCharacter).AllChildClass()) {
			int id = ch.AngeHash();
			if (Stage.IsValidEntityID(id)) {
				AllPlayablesID.Add(id);
			}
		}
		AllPlayablesID.TrimExcess();
	}


	[OnGameInitializeLater]
	[OnSavingSlotChanged]
	internal static TaskResult OnGameInitializeLaterPlayer () {
		if (!Enable) return TaskResult.End;
		if (!Stage.IsReady) return TaskResult.Continue;
		RespawnCpUnitPosition = null;
		UnlockPlayerDirty = false;
		LoadUnlockedPlayerListFromFile();
		SelectCharacterAsPlayer(SelectingPlayerID.Value);
		return TaskResult.End;
	}


	[OnViewZChanged]
	internal static void OnViewZChanged () {
		if (Selecting != null) {
			IWithCharacterBuff.GiveBuffFromMap(Selecting);
		}
	}


	// Before Update
	[BeforeBeforeUpdate]
	internal static void BeforeBeforeUpdate () {

		if (!Enable) return;
		if (Selecting == null || !Selecting.Active) return;

		Selecting.IgnoreDespawnFromMap(1);
		Selecting.DespawnAfterPassoutDelay = -1;

		// Update Player
		switch (Selecting.CharacterState) {
			case CharacterState.GamePlay:

				bool allowGamePlay = !TaskSystem.HasTask() && !GUI.IsTyping;

				if (allowGamePlay) {
					if (PlayerMenuUI.ShowingUI || PlayerQuickMenuUI.ShowingUI) {
						IgnoreInput(0);
					}
				}

				if (allowGamePlay && !IgnoringInput) {

					// Update Aiming
					UpdateAiming();

					// Move
					Selecting.Movement.Move(Input.DirectionX, Input.DirectionY, Input.GameKeyHolding(Gamekey.Up));

					// Movement Actions
					UpdateJumpDashPoundRush();

					// Hint
					ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
				} else {
					Selecting.Movement.Stop();
				}

				if (allowGamePlay) {
					UpdateAction();
					UpdateAttack();
					UpdateInventoryUI();
				}

				break;
			case CharacterState.Sleep:
				UpdateSleep();
				break;
			case CharacterState.PassOut:
				UpdatePassOut();
				break;
		}

		// Final
		UpdateDebugDragging();
		UpdateView();

	}


	private static void UpdateAiming () {
		var att = Selecting.Attackness;
		var mov = Selecting.Movement;
		att.AimingDirection =
			Input.Direction.TryGetDirection8(out var result) ? result :
			mov.FacingRight ? Direction8.Right : Direction8.Left;
		// Ignore Check
		if (att.IsAimingDirectionIgnored(att.AimingDirection)) {
			var dir0 = att.AimingDirection;
			var dir1 = att.AimingDirection;
			bool clockwiseFirst = mov.FacingRight == dir0.IsTop();
			for (int safe = 0; safe < 4; safe++) {
				dir0 = clockwiseFirst ? dir0.Clockwise() : dir0.AntiClockwise();
				dir1 = clockwiseFirst ? dir1.AntiClockwise() : dir1.Clockwise();
				if (!att.IsAimingDirectionIgnored(dir0)) {
					att.AimingDirection = dir0;
					break;
				}
				if (!att.IsAimingDirectionIgnored(dir1)) {
					att.AimingDirection = dir1;
					break;
				}
			}
		}
	}


	private static void UpdateJumpDashPoundRush () {

		if (IgnoringInput) return;

		var att = Selecting.Attackness;
		var mov = Selecting.Movement;

		ControlHintUI.AddHint(Gamekey.Jump, BuiltInText.HINT_JUMP);

		// Jump/Dash
		mov.HoldJump(Input.GameKeyHolding(Gamekey.Jump));
		if (Input.GameKeyDown(Gamekey.Jump)) {
			// Movement Jump
			if (Input.GameKeyHolding(Gamekey.Down)) {
				mov.Dash();
			} else {
				mov.Jump();
				if (att.CancelAttackOnJump) att.CancelAttack();
				AttackRequiringFrame = int.MinValue;
			}
		}

		// Pound
		if (Input.GameKeyDown(Gamekey.Down)) {
			mov.Pound();
		}

		// Rush
		if (Input.GameKeyDown(Gamekey.Left)) {
			if (
				Game.GlobalFrame < LastLeftKeyDown + RUSH_TAPPING_GAP &&
				!Input.GameKeyHolding(Gamekey.Up)
			) {
				mov.Rush();
			}
			LastLeftKeyDown = Game.GlobalFrame;
			LastRightKeyDown = int.MinValue;
		}
		if (Input.GameKeyDown(Gamekey.Right)) {
			if (
				Game.GlobalFrame < LastRightKeyDown + RUSH_TAPPING_GAP &&
				!Input.GameKeyHolding(Gamekey.Up)
			) {
				mov.Rush();
			}
			LastRightKeyDown = Game.GlobalFrame;
			LastLeftKeyDown = int.MinValue;
		}

	}


	private static void UpdateAction () {

		var health = Selecting.Health;
		var mov = Selecting.Movement;

		if (health.TakingDamage || TaskSystem.HasTask() || Game.GlobalFrame <= IgnoreActionFrame) {
			TargetActionEntity = null;
			return;
		}

		// Search for Active Trigger
		if (TargetActionEntity == null || !TargetActionEntity.LockInput) {
			TargetActionEntity = null;
			Entity eActTarget = null;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC,
				Selecting.Rect.Expand(ACTION_SCAN_RANGE, ACTION_SCAN_RANGE, 0, ACTION_SCAN_RANGE),
				out int count, Selecting,
				OperationMode.ColliderAndTrigger
			);
			int dis = int.MaxValue;
			bool squatting = mov.IsSquatting;
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
					Selecting.X >= hit.Rect.xMin && Selecting.X <= hit.Rect.xMax ? 0 :
					Selecting.X > hit.Rect.xMax ? Util.Abs(Selecting.X - hit.Rect.xMax) :
					Util.Abs(Selecting.X - hit.Rect.xMin);
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
			if ((TargetActionEntity.InvokeOnTouch || Input.GameKeyDown(Gamekey.Action)) && !PlayerMenuUI.ShowingUI) {
				if (TargetActionEntity.Invoke()) {
					Input.UseGameKey(Gamekey.Action);
				}
			}
		}

		// Lock Input from Highlight Target
		if (TargetActionEntity != null && TargetActionEntity.LockInput) {
			IgnoreInput(1);
		}

	}


	private static void UpdateAttack () {

		var att = Selecting.Attackness;
		var mov = Selecting.Movement;
		bool actionHolding = Input.GameKeyHolding(Gamekey.Action);
		att.HoldingAttack = actionHolding;

		att.IsChargingAttack =
			att.MinimalChargeAttackDuration != int.MaxValue &&
			Game.GlobalFrame >= att.LastAttackFrame + att.AttackDuration + att.AttackCooldown + att.MinimalChargeAttackDuration &&
			!TaskSystem.HasTask() &&
			!IgnoringInput &&
			actionHolding &&
			Selecting.IsAttackAllowedByMovement() &&
			Selecting.IsAttackAllowedByEquipment();


		if (IgnoringInput || Game.GlobalFrame <= IgnoreAttackFrame) return;

		// Try Perform Attack
		ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_ATTACK);
		bool attDown = Input.GameKeyDown(Gamekey.Action);
		if (attDown || (actionHolding && att.RepeatAttackWhenHolding)) {
			if (Game.GlobalFrame >= att.LastAttackFrame + att.AttackDuration + att.AttackCooldown + (attDown ? 0 : att.HoldAttackPunishFrame)) {
				att.Attack(mov.FacingRight);
			} else if (attDown) {
				AttackRequiringFrame = Game.GlobalFrame;
			}
			return;
		}

		// Reset Require on Move
		if (Input.GameKeyDown(Gamekey.Left) || Input.GameKeyDown(Gamekey.Right) || Input.GameKeyDown(Gamekey.Down) || Input.GameKeyDown(Gamekey.Up)) {
			AttackRequiringFrame = int.MinValue;
		}

		// Perform Required Attack
		const int ATTACK_REQUIRE_GAP = 12;
		if (
			Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP &&
			Game.GlobalFrame >= att.LastAttackFrame + att.AttackDuration + att.AttackCooldown
		) {
			AttackRequiringFrame = int.MinValue;
			att.Attack(mov.FacingRight);
		}

	}


	private static void UpdateInventoryUI () {

		if (!AllowPlayerMenuUI) {
			PlayerMenuUI.CloseMenu();
		}
		if (!AllowQuickPlayerMenuUI) {
			PlayerQuickMenuUI.CloseMenu();
		}
		bool requireHint = false;
		bool showingMenu = PlayerMenuUI.ShowingUI;
		bool showingQuickMenu = PlayerQuickMenuUI.ShowingUI;

		// Quick Menu
		if (AllowQuickPlayerMenuUI && !showingMenu && !showingQuickMenu && !IgnoringInput) {
			if (Input.GameKeyDown(Gamekey.Select) || Input.GameKeyDown(Gamekey.Start)) {
				PlayerQuickMenuUI.OpenMenu();
			}
			showingQuickMenu = PlayerQuickMenuUI.ShowingUI;
			requireHint = true;
		}

		// Inventory Menu
		if (
			AllowPlayerMenuUI &&
			!showingMenu &&
			(showingQuickMenu || !IgnoringInput) &&
			(!showingQuickMenu || !PlayerQuickMenuUI.Instance.IsDirty)
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


	private static void UpdateSleep () {

		TargetActionEntity = null;

		if (TaskSystem.HasTask()) return;

		// Wake up on Press Action
		if (Input.GameKeyDown(Gamekey.Action) || Input.GameKeyDown(Gamekey.Jump)) {
			Input.UseGameKey(Gamekey.Action);
			Input.UseGameKey(Gamekey.Jump);
			Selecting.SetCharacterState(CharacterState.GamePlay);
			Selecting.SleepStartFrame = int.MinValue;
			Selecting.Y -= 4;
		}

		// Hint
		ControlHintUI.DrawGlobalHint(
			Selecting.X - Const.HALF, Selecting.Y + Const.CEL * 3 / 2,
			Gamekey.Action, HINT_WAKE, background: true
		);
		ControlHintUI.AddHint(Gamekey.Action, HINT_WAKE);
	}


	private static void UpdateView () {

		if (IgnorePlayerView.FinalValue) return;

		var mov = Selecting.Movement;

		const int LINGER_RATE = 32;
		bool notInGameplay = TaskSystem.HasTask() || Selecting.CharacterState != CharacterState.GamePlay;

		// Update Grounded Y
		if (
			Game.GlobalFrame <= ForceUpdateViewGroundingFrame ||
			notInGameplay ||
			Selecting.IsGrounded || Selecting.InWater || mov.IsSliding ||
			mov.IsClimbing || mov.IsGrabbingSide || mov.IsGrabbingTop || mov.IsFlying
		) {
			LastGroundedY = Selecting.Y;
		}

		// Aim X
		int linger = Stage.ViewRect.width * LINGER_RATE / 1000;
		int centerX = Stage.ViewRect.x + Stage.ViewRect.width / 2;
		if (notInGameplay) {
			AimViewX = Selecting.X - Stage.ViewRect.width / 2;
		} else if (Selecting.X < centerX - linger) {
			AimViewX = Selecting.X + linger - Stage.ViewRect.width / 2;
		} else if (Selecting.X > centerX + linger) {
			AimViewX = Selecting.X - linger - Stage.ViewRect.width / 2;
		}

		// Aim Y
		AimViewY = Selecting.Y <= LastGroundedY ? Selecting.Y - GetCameraShiftOffset(Stage.ViewRect.height) : AimViewY;
		Stage.SetViewPositionDelay(AimViewX, AimViewY, 96, int.MinValue);

		// Size
		Stage.SetViewSizeDelay(TargetViewHeight, 96, int.MinValue, centralized: true);

		// Clamp
		if (!Stage.ViewRect.Contains(Selecting.X, Selecting.Y)) {
			if (Selecting.X >= Stage.ViewRect.xMax) AimViewX = Selecting.X - Stage.ViewRect.width + 1;
			if (Selecting.X <= Stage.ViewRect.xMin) AimViewX = Selecting.X - 1;
			if (Selecting.Y >= Stage.ViewRect.yMax) AimViewY = Selecting.Y - Stage.ViewRect.height + 1;
			if (Selecting.Y <= Stage.ViewRect.yMin) AimViewY = Selecting.Y - 1;
			Stage.SetViewPositionDelay(AimViewX, AimViewY, 1000, int.MinValue + 1);
		}

	}


	private static void UpdatePassOut () {

		if (TaskSystem.HasTask()) return;

		bool fullPassOut = Selecting.Health.HP == 0 && Game.GlobalFrame > Selecting.PassOutFrame + 48;

		if (fullPassOut) {
			ControlHintUI.DrawGlobalHint(
				Selecting.X - Const.HALF, Selecting.Y + Const.CEL * 3 / 2,
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


	private static void UpdateDebugDragging () {

#if !DEBUG
		return;
#endif

		if (!DragPlayerInMiddleButtonToMove_DebugOnly || Selecting == null || !Input.MouseMidButtonHolding) {
			// End Hook Task
			if (TaskSystem.IsTasking<EntityHookTask>()) {
				TaskSystem.GetCurrentTask().UserData = null;
			}
			return;
		}

		// Move Player to Cursor Pos
		var mousePos = Input.MouseGlobalPosition;
		Selecting.X = mousePos.x;
		Selecting.Y = mousePos.y - Const.CEL * 2;
		IgnorePlayerView.True(1, 4096);
		if (Selecting.Rendering is PoseCharacterRenderer pRenderer) {
			pRenderer.ManualPoseAnimate(PoseAnimation_Idle.TYPE_ID, 1);
		}
		Selecting.IgnorePhysics.True(1, 4096);
		Selecting.Health.MakeInvincible(1);

		// Move View when Hover on Edge
		var cameraRect = Renderer.CameraRect;
		var center = cameraRect.CenterInt();
		int startMoveDis = cameraRect.height / 4;
		int mouseCenterDis = Util.DistanceInt(mousePos, center);
		if (mouseCenterDis > startMoveDis) {
			// Move
			var viewRect = Stage.ViewRect;
			var delta = (Float2)(mousePos - center);
			float targetMag = (delta.magnitude - startMoveDis) / 16f;
			delta = delta.normalized * targetMag;
			viewRect.x += delta.x.RoundToInt();
			viewRect.y += delta.y.RoundToInt() * cameraRect.width / cameraRect.height;
			Stage.SetViewPositionDelay(viewRect.x, viewRect.y, priority: 4096);
			AimViewX = viewRect.x;
			AimViewY = viewRect.y;
		}

		// Task
		if (!TaskSystem.IsTasking<EntityHookTask>()) {
			TaskSystem.AddToFirst(EntityHookTask.TYPE_ID, Selecting);
		}

	}


	// Update
	[BeforeUpdateUpdate]
	internal static void BeforeUpdateUpdate () {
		if (!Enable) return;
		if (Selecting == null || !Selecting.Active) return;
		if (!Stage.ViewRect.Overlaps(Selecting.Rect)) return;
		if (UnlockPlayerDirty) {
			UnlockPlayerDirty = false;
			SaveUnlockedPlayerListToFile();
		}
	}


	#endregion




	#region --- API ---


	// Select Player
	public static void SelectCharacterAsPlayer (int characterTypeID, bool failbackToDefault = true) {

		if (!Enable) return;
		if (Selecting != null && Selecting.Active && characterTypeID == Selecting.TypeID) return;

		if (characterTypeID == 0 && failbackToDefault) {
			characterTypeID = GetDefaultPlayerID();
		}

		if (characterTypeID != 0) {
			var unitPos = GetPlayerFinalRespawnUnitPosition();
			if (Stage.GetOrSpawnEntity(characterTypeID, unitPos.x.ToGlobal(), unitPos.y.ToGlobal()) is Character target) {
				SetCharacterAsPlayer(target);
			}
		}

	}


	public static void SetCharacterAsPlayer (Character target) {

		if (!Enable) return;

		if (Selecting != null) {
			Selecting.Movement.Stop();
			Selecting.OnActivated();
		}
		Selecting = target;
		if (target == null) {
			SelectingPlayerID.Value = 0;
			return;
		}
		target.IgnoreDespawnFromMap(1);
		SelectingPlayerID.Value = target.TypeID;
		IgnoreInputFrame = -1;
		TargetActionEntity = null;
		PlayerMenuUI.CloseMenu();
		UnlockedPlayer.Add(target.TypeID);
		UnlockPlayerDirty = true;
	}


	// Unlock Player
	public static IEnumerable<int> ForAllUnlockedPlayers () {
		foreach (var id in UnlockedPlayer) {
			yield return id;
		}
	}


	public static IEnumerable<int> ForAllPlayables () {
		foreach (var id in AllPlayablesID) {
			yield return id;
		}
	}


	public static bool IsPlayerUnlocked (int id) => UnlockedPlayer.Contains(id);


	public static void UnlockPlayer (int id) {
		UnlockedPlayer.Add(id);
		UnlockPlayerDirty = true;
	}


	// Misc
	public static int GetDefaultPlayerID () {
		if (!Enable) return 0;
		System.Type result = null;
		int currentPriority = int.MinValue;
		foreach (var (type, attribute) in Util.AllClassWithAttribute<EntityAttribute.DefaultSelectedPlayerAttribute>()) {
			if (type.IsSubclassOf(typeof(Character)) && attribute.Priority >= currentPriority) {
				result = type;
				currentPriority = attribute.Priority;
			}
		}
		return result != null ? result.AngeHash() : 0;
	}


	public static void ForceUpdateGroundedForView (int duration = 1) => ForceUpdateViewGroundingFrame = Game.GlobalFrame + duration;


	public static Int3 GetPlayerFinalRespawnUnitPosition () {

		if (!Enable) return default;

		Int3 result;
		if (RespawnCpUnitPosition.HasValue) {
			// CP Respawn Pos
			result = RespawnCpUnitPosition.Value;
		} else if (HomeUnitPosition.HasValue) {
			// Home Pos
			result = new Int3(
				HomeUnitPosition.Value.x,
				HomeUnitPosition.Value.y,
				HomeUnitPosition.Value.z
			);
		} else if (Selecting != null && Selecting.Active) {
			// Player
			result = new Int3(
				Selecting.X.ToUnit(),
				Selecting.Y.ToUnit(),
				Stage.ViewZ
			);
		} else {
			// Fail
			result = default;
		}
		return result;
	}


	public static int GetCameraShiftOffset (int cameraHeight) => cameraHeight * 382 / 1000;


	// Ignore
	public static void IgnoreInput (int duration = 1) => IgnoreInputFrame = Game.GlobalFrame + duration;


	public static void IgnoreAction (int duration = 1) => IgnoreActionFrame = Game.GlobalFrame + duration;


	public static void IgnorePlayerMenu (int duration = 1) => IgnorePlayerMenuFrame = Game.GlobalFrame + duration;


	public static void IgnorePlayerQuickMenu (int duration = 1) => IgnorePlayerQuickMenuFrame = Game.GlobalFrame + duration;


	public static void IgnoreAttack (int duration = 1) => IgnoreAttackFrame = Game.GlobalFrame + duration;


	#endregion




	#region --- LGC ---


	// Unlock Player
	private static void LoadUnlockedPlayerListFromFile () {
		UnlockedPlayer.Clear();
		string path = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "UnlockedPlayers");
		if (!Util.FileExists(path)) return;
		using var stream = File.Open(path, FileMode.Open);
		using var reader = new BinaryReader(stream);
		while (reader.NotEnd()) {
			int id = reader.ReadInt32();
			if (!Stage.IsValidEntityID(id)) continue;
			UnlockedPlayer.Add(id);
		}
	}


	private static void SaveUnlockedPlayerListToFile () {
		if (UnlockedPlayer.Count == 0) return;
		string path = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "UnlockedPlayers");
		using var stream = File.Open(path, FileMode.Create);
		using var writer = new BinaryWriter(stream);
		foreach (int id in UnlockedPlayer) {
			writer.Write(id);
		}
	}


	#endregion




}
