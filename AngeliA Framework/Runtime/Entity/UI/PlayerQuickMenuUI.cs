using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public class PlayerQuickMenuUI : EntityUI {




		#region --- SUB ---


		private class ItemSorter : IComparer<int> {
			public static readonly ItemSorter Instance = new();
			public int Compare (int x, int y) {
				if (x == 0) return y == 0 ? 0 : 1;
				if (y == 0) return x == 0 ? 0 : -1;
				return x.CompareTo(y);
			}
		}


		#endregion




		#region --- VAR ---


		// Api
		public static PlayerQuickMenuUI Instance { get; private set; } = null;
		public static bool ShowingUI => Instance != null && Instance.Active;
		public bool IsDirty { get; private set; } = false;

		// Data
		private static readonly int[] ItemList = new int[Player.INVENTORY_ROW * Player.INVENTORY_COLUMN + 1];
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
			if (equippingID != 0) {
				ItemList[0] = equippingID;
				currentIndex++;
				WeaponCount++;
			}
			int capacity = Inventory.GetInventoryCapacity(invID);
			for (int i = 0; i < capacity && currentIndex < ItemList.Length; i++) {
				int itemID = Inventory.GetItemAt(invID, i);
				if (
					itemID == 0 ||
					!ItemSystem.IsEquipment(itemID, out var eqType) ||
					eqType != EquipmentType.Weapon
				) continue;
				ItemList[currentIndex] = itemID;
				currentIndex++;
				WeaponCount++;
			}
			for (int i = currentIndex; i < ItemList.Length; i++) {
				ItemList[i] = 0;
			}
			Util.QuickSort(ItemList, 0, ItemList.Length - 1, ItemSorter.Instance);

			// Set Current Slot Index
			if (equippingID != 0) {
				for (int i = 0; i < WeaponCount; i++) {
					if (ItemList[i] == equippingID) {
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
					int itemIndex = Inventory.IndexOfItem(Player.Selecting.TypeID, ItemList[CurrentSlotIndex]);
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
				CurrentSlotIndex = (CurrentSlotIndex - 1).Clamp(0, WeaponCount - 1);
				IsDirty = true;
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
				CurrentSlotIndex = (CurrentSlotIndex + 1).Clamp(0, WeaponCount - 1);
				IsDirty = true;
			}

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
				Mathf.LerpUnclamped(Unify(86), 0, lerp01).RoundToInt() : 0;
			int ITEM_SIZE = Unify(56);
			int PADDING = Unify(8);
			int BORDER = Unify(4);

			// Content
			int basicX = player.X - CurrentSlotIndex * ITEM_SIZE - ITEM_SIZE / 2;
			int basicY = player.Y - ITEM_SIZE - PADDING + offsetY - Const.HALF;
			var rect = new RectInt(0, basicY, ITEM_SIZE, ITEM_SIZE);
			for (int i = 0; i < WeaponCount; i++) {

				rect.x = basicX + i * ITEM_SIZE;

				// BG
				CellRenderer.Draw(Const.PIXEL, rect.Expand(PADDING), Const.BLACK, int.MinValue + 1);

				// Highlight
				if (i == CurrentSlotIndex) {
					CellRenderer.Draw(Const.PIXEL, rect.Shrink(BORDER), Const.GREEN, int.MinValue + 2);
				}

				// From Inventory
				DrawItemIcon(rect, ItemList[i], Const.WHITE, int.MinValue + 10);

			}

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
							ItemSystem.ItemSpawnItemAtPlayer(oldEquipmentID);
						}
					}
				}
			}

		}


		private static void DrawItemIcon (RectInt rect, int id, Color32 tint, int z) {
			if (id == 0) return;
			if (!CellRenderer.TryGetSprite(id, out var sprite)) {
				id = Const.PIXEL;
				CellRenderer.TryGetSprite(Const.PIXEL, out sprite);
				rect = rect.Shrink(rect.width / 6);
			}
			int iconShrink = Unify(7);
			CellRenderer.Draw(
				id,
				rect.Shrink(iconShrink).Fit(sprite.GlobalWidth, sprite.GlobalHeight),
				tint, z
			);
		}


		#endregion




	}
}