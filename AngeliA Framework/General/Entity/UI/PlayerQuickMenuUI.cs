using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[RequireLanguageFromField]
	public class PlayerQuickMenuUI : EntityUI, IWindowEntityUI {




		#region --- SUB ---


		private class WeaponSorter : IComparer<Weapon> {
			public static readonly WeaponSorter Instance = new();
			public int Compare (Weapon x, Weapon y) {
				if (x is null) return y is null ? 0 : 1;
				if (y is null) return x is null ? 0 : -1;
				int result = ((int)x.WeaponType).CompareTo((int)y.WeaponType);
				if (result == 0) result = x.TypeID.CompareTo(y.TypeID);
				return result;
			}
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly LanguageCode HINT_MOVE = "CtrlHint.Move";

		// Api
		public static PlayerQuickMenuUI Instance { get; private set; } = null;
		public static bool ShowingUI => Instance != null && Instance.Active;
		public bool IsDirty { get; private set; } = false;
		public IRect BackgroundRect { get; private set; } = default;

		// Data
		private static readonly Weapon[] WeaponList = new Weapon[Player.INVENTORY_ROW * Player.INVENTORY_COLUMN + 1];
		private static readonly CellContent NameLabel = new() {
			Alignment = Alignment.MidMid,
			BackgroundTint = Const.BLACK,
			CharSize = 22,
			TightBackground = true,
		};
		private int CurrentSlotIndex = 0;
		private int WeaponCount = 0;


		#endregion




		#region --- MSG ---


		public PlayerQuickMenuUI () => Instance = this;


		public override void OnActivated () {
			base.OnActivated();

			X = CellRenderer.CameraRect.CenterX();
			Y = CellRenderer.CameraRect.CenterY();
			IsDirty = false;
			CurrentSlotIndex = 0;
			WeaponCount = 0;

			// Init Item List
			int invID = Player.Selecting.TypeID;
			int currentIndex = 0;
			int equippingID = Inventory.GetEquipment(invID, EquipmentType.Weapon);
			if (ItemSystem.GetItem(equippingID) is Weapon equippingItem) {
				WeaponList[0] = equippingItem;
				currentIndex++;
				WeaponCount++;
			}
			int capacity = Inventory.GetInventoryCapacity(invID);
			for (int i = 0; i < capacity && currentIndex < WeaponList.Length; i++) {
				int itemID = Inventory.GetItemAt(invID, i);
				if (
					itemID == 0 ||
					ItemSystem.GetItem(itemID) is not Weapon weapon
				) continue;
				WeaponList[currentIndex] = weapon;
				currentIndex++;
				WeaponCount++;
			}
			for (int i = currentIndex; i < WeaponList.Length; i++) {
				WeaponList[i] = null;
			}
			Util.QuickSort(WeaponList, 0, WeaponList.Length - 1, WeaponSorter.Instance);

			// Set Current Slot Index
			if (equippingID != 0) {
				for (int i = 0; i < WeaponCount; i++) {
					var weapon = WeaponList[i];
					if (weapon != null && weapon.TypeID == equippingID) {
						CurrentSlotIndex = i;
						break;
					}
				}
			}
		}


		public override void OnInactivated () {
			base.OnInactivated();
			IsDirty = false;
			WeaponCount = 0;
		}


		public override void UpdateUI () {
			base.UpdateUI();

			if (!Active || Player.Selecting == null || FrameTask.HasTask() || WeaponCount <= 0) {
				Active = false;
				return;
			}

			// Close Menu Check
			if (
				!FrameInput.GameKeyHolding(Gamekey.Start) &&
				!FrameInput.GameKeyHolding(Gamekey.Select)
			) {
				if (IsDirty) {
					FrameInput.UseGameKey(Gamekey.Start);
					FrameInput.UseGameKey(Gamekey.Select);
				}
				if (IsDirty && CurrentSlotIndex >= 0 && CurrentSlotIndex < WeaponCount) {
					int itemIndex = Inventory.IndexOfItem(
						Player.Selecting.TypeID, WeaponList[CurrentSlotIndex].TypeID
					);
					if (itemIndex >= 0) {
						EquipFromInventory(itemIndex);
					}
				}
				Active = false;
				return;
			}

			// Dirty when Wait too Long
			if (Game.GlobalFrame - SpawnFrame > 24) IsDirty = true;

			// Logic
			if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
				CurrentSlotIndex = (CurrentSlotIndex - 1).UMod(WeaponCount);
				IsDirty = true;
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
				CurrentSlotIndex = (CurrentSlotIndex + 1).UMod(WeaponCount);
				IsDirty = true;
			}

			// Hint
			ControlHintUI.AddHint(Gamekey.Left, Language.Get(HINT_MOVE, "Move"));
			ControlHintUI.AddHint(Gamekey.Right, Language.Get(HINT_MOVE, "Move"));

			// Draw
			DrawMenu();

		}


		private void DrawMenu () {

			const int ANIMATION_DURATION = 12;
			int animationDelay = IsDirty ? 0 : 8;
			var player = Player.Selecting;
			int localAnimationFrame = Game.GlobalFrame - SpawnFrame - animationDelay;
			if (localAnimationFrame < 0) return;
			float lerp01 = Ease.OutCirc((float)localAnimationFrame / ANIMATION_DURATION);
			int offsetY = localAnimationFrame < ANIMATION_DURATION ?
				Util.LerpUnclamped(Unify(86), 0, lerp01).RoundToInt() : 0;
			int ITEM_SIZE = Unify(56);
			int PADDING = Unify(8);
			int BORDER = Unify(4);

			// Content
			int basicX = player.X - CurrentSlotIndex * ITEM_SIZE - ITEM_SIZE / 2;
			int basicY = player.Y - ITEM_SIZE - PADDING + offsetY - Const.HALF;
			var rect = new IRect(0, basicY, ITEM_SIZE, ITEM_SIZE);
			var bgRect = rect;
			for (int i = 0; i < WeaponCount; i++) {

				var weapon = WeaponList[i];
				if (weapon is null) continue;

				rect.x = basicX + i * ITEM_SIZE;
				bgRect.width += i * ITEM_SIZE;

				// Cursoring
				if (i == CurrentSlotIndex) {

					// Highlight
					CellRenderer.Draw(Const.PIXEL, rect.Shrink(BORDER), Const.GREEN, int.MinValue + 2);

					// Name Label
					NameLabel.Text = ItemSystem.GetItemName(weapon.TypeID);
					int labelWidth = ITEM_SIZE * 3;
					int labelHeight = Unify(NameLabel.CharSize + 4);
					CellRendererGUI.Label(NameLabel, new IRect(
						rect.CenterX() - labelWidth / 2,
						rect.y - labelHeight,
						labelWidth, labelHeight
					));

				}

				// From Inventory
				DrawItemIcon(rect, weapon.TypeID, Const.WHITE, int.MinValue + 10);

			}

			// BG
			BackgroundRect = bgRect.Expand(PADDING);
			CellRenderer.Draw(Const.PIXEL, BackgroundRect, Const.BLACK, int.MinValue + 1);

		}


		#endregion




		#region --- API ---


		public static PlayerQuickMenuUI OpenMenu () {
			var ins = Instance;
			if (ins == null) return null;
			if (!ins.Active) {
				Stage.SpawnEntity(ins.TypeID, 0, 0);
			} else {
				ins.OnInactivated();
				ins.OnActivated();
			}
			return ins;
		}


		#endregion




		#region --- LGC ---


		private static void EquipFromInventory (int itemIndex) {

			int invID = Player.Selecting.TypeID;
			if (!Inventory.HasInventory(invID)) return;

			int capacity = Inventory.GetInventoryCapacity(invID);
			if (itemIndex < 0 || itemIndex >= capacity) return;

			int itemID = Inventory.GetItemAt(invID, itemIndex);
			if (itemID == 0) return;

			if (!ItemSystem.IsEquipment(itemID, out var eqType) || eqType != EquipmentType.Weapon) return;
			if (!Player.Selecting.EquipmentAvailable(EquipmentType.Weapon)) return;

			int tookCount = Inventory.TakeItemAt(invID, itemIndex, 1);
			if (tookCount <= 0) return;

			int oldEquipmentID = Inventory.GetEquipment(invID, EquipmentType.Weapon);
			if (Inventory.SetEquipment(invID, EquipmentType.Weapon, itemID)) {
				if (oldEquipmentID != 0) {
					if (Inventory.GetItemAt(invID, itemIndex) == 0) {
						// Back to Cursor
						Inventory.SetItemAt(invID, itemIndex, oldEquipmentID, 1);
					} else {
						// Collect
						int collectCount = Inventory.CollectItem(invID, oldEquipmentID, out _, 1);
						if (collectCount == 0) {
							ItemSystem.SpawnItemAtPlayer(oldEquipmentID);
						}
					}
				}
			}

		}


		private static void DrawItemIcon (IRect rect, int id, Byte4 tint, int z) {
			if (id == 0) return;
			if (!CellRenderer.TryGetSprite(id, out var sprite)) {
				id = Const.PIXEL;
				CellRenderer.TryGetSprite(Const.PIXEL, out sprite);
				rect = rect.Shrink(rect.width / 6);
			}
			int iconShrink = Unify(7);
			CellRenderer.Draw(
				id,
				rect.Shrink(iconShrink).Fit(sprite),
				tint, z
			);
		}


		#endregion




	}
}