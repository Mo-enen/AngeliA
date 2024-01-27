using System.Collections;
using System.Collections.Generic;


[assembly: AngeliaFramework.RequireGlobalSprite(atlas: "Character", "Player")]


namespace AngeliaFramework {
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDrawBehind]
	[RequireLanguageFromField]
	public abstract class Player : PoseCharacter, IGlobalPosition, IDamageReceiver, IActionTarget {




		#region --- SUB ---


		private class PlayerGameData {
			public int HomeUnitPositionX = int.MinValue;
			public int HomeUnitPositionY = int.MinValue;
			public int HomeUnitPositionZ = int.MinValue;
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int RUSH_TAPPING_GAP = 16;
		private const int ACTION_SCAN_RANGE = Const.HALF;
		private static readonly LanguageCode HINT_WAKE = ("CtrlHint.WakeUp", "Wake");
		private static readonly LanguageCode HINT_SWITCH_PLAYER = ("CtrlHint.SwitchPlayer", "Select Player");

		// Api
		public static Player Selecting { get; private set; } = null;
		public static bool HasActivePlayer => Selecting != null && Selecting.Active;
		public static Int3? RespawnCpUnitPosition { get; set; } = null;
		public static Int3? HomeUnitPosition { get; private set; } = null;
		public override bool IsChargingAttack =>
			Selecting == this &&
			MinimalChargeAttackDuration != int.MaxValue &&
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown + MinimalChargeAttackDuration &&
			!FrameTask.HasTask() &&
			!LockingInput &&
			FrameInput.GameKeyHolding(Gamekey.Action);
		public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT;
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
		public virtual bool AllowPlayerMenuUI => true;
		public virtual bool AllowQuickPlayerMenuUI => true;
		protected override bool IsCharacterWithInventory => true;
		protected override bool InventoryCurrentAvailable => Game.GlobalFrame > IgnoreInventoryFrame;
		int IDamageReceiver.Team => Const.TEAM_PLAYER;

		// Data
		private int AttackRequiringFrame = int.MinValue;
		private int LastLeftKeyDown = int.MinValue;
		private int LastRightKeyDown = int.MinValue;
		private int LastGroundedY = 0;
		private int PrevZ = int.MinValue;
		private int IgnoreInventoryFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		[OnProjectOpen]
		public static void OnProjectOpen_Player () {
			LoadGameDataFromFile();
			SelectPlayer(0);
		}


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


		public override void FillPhysics () {
			if (FrameTask.HasTask()) return;
			base.FillPhysics();
		}


		// Before Physics Update
		public override void BeforePhysicsUpdate () {

			base.BeforePhysicsUpdate();

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
				Stop();
				return;
			}

			// Update Player
			switch (CharacterState) {
				case CharacterState.GamePlay:

					bool allowGamePlay = !FrameTask.HasTask() && !CellGUI.IsTyping;

					if (allowGamePlay) {
						if (PlayerMenuUI.ShowingUI || PlayerQuickMenuUI.ShowingUI) {
							LockInput(0);
						}
					}

					if (allowGamePlay && !LockingInput) {

						// Move
						Move(FrameInput.DirectionX, FrameInput.DirectionY);

						// Walk when Holding Up
						if (FrameInput.GameKeyHolding(Gamekey.Up)) {
							RunningAccumulateFrame = -1;
						}

						// Movement Actions
						Update_JumpDashPoundRush();

						// Hint
						ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
					} else {
						Stop();
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


		private void Update_JumpDashPoundRush () {

			if (LockingInput) return;

			ControlHintUI.AddHint(Gamekey.Jump, BuiltInText.HINT_JUMP);

			// Jump/Dash
			HoldJump(FrameInput.GameKeyHolding(Gamekey.Jump));
			if (FrameInput.GameKeyDown(Gamekey.Jump)) {
				// Movement Jump
				if (FrameInput.GameKeyHolding(Gamekey.Down)) {
					Dash();
				} else {
					Jump();
					AttackRequiringFrame = int.MinValue;
				}
			}

			// Pound
			if (FrameInput.GameKeyDown(Gamekey.Down)) {
				Pound();
			}

			// Rush
			if (FrameInput.GameKeyDown(Gamekey.Left)) {
				if (
					Game.GlobalFrame < LastLeftKeyDown + RUSH_TAPPING_GAP &&
					!FrameInput.GameKeyHolding(Gamekey.Up)
				) {
					Rush();
				}
				LastLeftKeyDown = Game.GlobalFrame;
				LastRightKeyDown = int.MinValue;
			}
			if (FrameInput.GameKeyDown(Gamekey.Right)) {
				if (
					Game.GlobalFrame < LastRightKeyDown + RUSH_TAPPING_GAP &&
					!FrameInput.GameKeyHolding(Gamekey.Up)
				) {
					Rush();
				}
				LastRightKeyDown = Game.GlobalFrame;
				LastLeftKeyDown = int.MinValue;
			}

		}


		private void Update_Action () {

			if (TakingDamage || FrameTask.HasTask()) return;

			// Search for Active Trigger
			if (TargetActionEntity == null || !TargetActionEntity.LockInput) {
				TargetActionEntity = null;
				Entity eActTarget = null;
				var hits = CellPhysics.OverlapAll(
					PhysicsMask.DYNAMIC,
					Rect.Expand(ACTION_SCAN_RANGE, ACTION_SCAN_RANGE, 0, ACTION_SCAN_RANGE),
					out int count, this,
					OperationMode.ColliderAndTrigger
				);
				int dis = int.MaxValue;
				bool squatting = IsSquatting;
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
				if (FrameInput.GameKeyDown(Gamekey.Action) && !PlayerMenuUI.ShowingUI) {
					TargetActionEntity?.Invoke();
					FrameInput.UseGameKey(Gamekey.Action);
				}
			}

			if (!LockingInput && TargetActionEntity != null && TargetActionEntity.LockInput) {
				LockInput(0);
			}

		}


		private void Update_Attack () {

			if (LockingInput) return;

			// Try Perform Attack
			ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_ATTACK);
			bool attDown = FrameInput.GameKeyDown(Gamekey.Action);
			bool attHolding = FrameInput.GameKeyHolding(Gamekey.Action) && RepeatAttackWhenHolding;
			if (attDown || attHolding) {
				if (Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown + (attDown ? 0 : HoldAttackPunish)) {
					Attack();
				} else if (attDown) {
					AttackRequiringFrame = Game.GlobalFrame;
				}
				return;
			}

			// Reset Require on Move
			if (
				FrameInput.GameKeyDown(Gamekey.Left) || FrameInput.GameKeyDown(Gamekey.Right) ||
				FrameInput.GameKeyDown(Gamekey.Down) || FrameInput.GameKeyDown(Gamekey.Up)
			) {
				AttackRequiringFrame = int.MinValue;
			}

			// Perform Required Attack
			const int ATTACK_REQUIRE_GAP = 12;
			if (
				Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP &&
				Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown
			) {
				AttackRequiringFrame = int.MinValue;
				Attack();
			}

		}


		private void Update_InventoryUI () {

			if (!AllowPlayerMenuUI && PlayerMenuUI.ShowingUI) {
				PlayerMenuUI.CloseMenu();
			}
			bool requireHint = false;

			// Quick Menu
			if (AllowQuickPlayerMenuUI && !PlayerMenuUI.ShowingUI && !PlayerQuickMenuUI.ShowingUI && !LockingInput) {
				if (FrameInput.GameKeyDown(Gamekey.Select) || FrameInput.GameKeyDown(Gamekey.Start)) {
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
				if (FrameInput.GameKeyUp(Gamekey.Select)) {
					FrameInput.UseGameKey(Gamekey.Select);
					PlayerMenuUI.OpenMenu();
				}
				requireHint = true;
			}

			// Hint
			if (requireHint) {
				ControlHintUI.AddHint(
					FrameInput.UsingGamepad ? Gamekey.Start : Gamekey.Select,
					BuiltInText.HINT_SHOW_MENU
				);
			}
		}


		private void Update_Sleep () {

			TargetActionEntity = null;

			if (FrameTask.HasTask()) return;

			// Wake up on Press Action
			if (FrameInput.GameKeyDown(Gamekey.Action) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				SetCharacterState(CharacterState.GamePlay);
				Y -= 4;
			}

			// Hint
			ControlHintUI.DrawGlobalHint(
				X - Const.HALF,
				Y + Const.CEL * 3 / 2,
				Gamekey.Action,
				HINT_WAKE,
				true
			);
			ControlHintUI.AddHint(Gamekey.Action, HINT_WAKE);
		}


		private void Update_View () {

			const int LINGER_RATE = 32;
			bool notInGameplay = FrameTask.HasTask() || CharacterState != CharacterState.GamePlay;
			bool notInAir =
				notInGameplay ||
				IsGrounded || InWater || InSand || IsSliding ||
				IsClimbing || IsGrabbingSide || IsGrabbingTop;

			if (notInAir || IsFlying) LastGroundedY = Y;

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

			if (FrameTask.HasTask()) return;

			if (IsFullPassOut) {
				ControlHintUI.DrawGlobalHint(
					X - Const.HALF, Y + Const.CEL * 3 / 2,
					Gamekey.Action, BuiltInText.UI_CONTINUE, background: true
				);
			}

			// Reload Game After Player PassOut
			if (IsFullPassOut && FrameInput.GameKeyDown(Gamekey.Action)) {
				FrameTask.AddToLast(RestartGameTask.TYPE_ID);
				FrameInput.UseGameKey(Gamekey.Action);
			}

		}


		// Physics Update
		public override void PhysicsUpdate () {
			if (Selecting != this && !Stage.ViewRect.Overlaps(GlobalBounds)) return;
			base.PhysicsUpdate();
			PhysicsUpdate_Collect();
		}


		private void PhysicsUpdate_Collect () {
			if (Selecting != this) return;
			var hits = CellPhysics.OverlapAll(
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


		// Frame Update
		public override void FrameUpdate () {

			int oldZ = PoseRenderingZOffset;
			if (Selecting == this) PoseRenderingZOffset = 40;

			// Equipping
			int equippingID = Inventory.GetEquipment(TypeID, EquipmentType.Weapon);
			if (equippingID != 0 && ItemSystem.GetItem(equippingID) is Weapon weapon) {
				EquippingWeaponType = weapon.WeaponType;
				EquippingWeaponHeld = weapon.Handheld;
			} else {
				EquippingWeaponType = WeaponType.Hand;
				EquippingWeaponHeld = WeaponHandheld.Float;
			}

			base.FrameUpdate();
			PoseRenderingZOffset = oldZ;

			// Auto Pick Item
			if (!FrameTask.HasTask() && !PlayerMenuUI.ShowingUI) {
				var cells = CellPhysics.OverlapAll(PhysicsMask.ITEM, Rect, out int count, null, OperationMode.ColliderAndTrigger);
				for (int i = 0; i < count; i++) {
					var cell = cells[i];
					if (cell.Entity is not ItemHolder holder || !holder.Active) continue;
					holder.Collect(this, true);
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
			return result != null ? result.AngeHash() : 0;
		}


		public static void SelectPlayer (int playerID) {
			if (Selecting != null && playerID == Selecting.TypeID) return;
			if (playerID == 0) playerID = GetDefaultPlayerID();
			if (playerID != 0 && Stage.PeekOrGetEntity(playerID) is Player player) {
				Selecting = player;
			}
		}


		public void MakeHome (Int3 unitPosition) {
			if (unitPosition != HomeUnitPosition) {
				HomeUnitPosition = unitPosition;
				SaveGameDataToFile();
			}
		}


		void IActionTarget.Invoke () {
			RespawnCpUnitPosition = null;
			FrameTask.AddToLast(SelectPlayerTask.TYPE_ID, TypeID);
			FrameTask.AddToLast(RestartGameTask.TYPE_ID);
		}


		bool IActionTarget.AllowInvoke () => !IsInvincible;


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


		public void IgnoreInventory (int duration = 1) => IgnoreInventoryFrame = Game.GlobalFrame + duration;


		#endregion




		#region --- LGC ---


		private static void LoadGameDataFromFile () {
			var data = JsonUtil.LoadOrCreateJson<PlayerGameData>(ProjectSystem.CurrentProject.SavingMetaRoot);
			HomeUnitPosition =
				data.HomeUnitPositionX != int.MinValue &&
				data.HomeUnitPositionY != int.MinValue &&
				data.HomeUnitPositionZ != int.MinValue ?
				new(data.HomeUnitPositionX, data.HomeUnitPositionY, data.HomeUnitPositionZ) : null;
		}


		private static void SaveGameDataToFile () {
			JsonUtil.SaveJson(new PlayerGameData() {
				HomeUnitPositionX = HomeUnitPosition.HasValue ? HomeUnitPosition.Value.x : int.MinValue,
				HomeUnitPositionY = HomeUnitPosition.HasValue ? HomeUnitPosition.Value.y : int.MinValue,
				HomeUnitPositionZ = HomeUnitPosition.HasValue ? HomeUnitPosition.Value.z : int.MinValue,
			}, ProjectSystem.CurrentProject.SavingMetaRoot);
		}


		#endregion




	}
}