using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDestroyOutOfRange]
public class PlayerQuickMenuUI : EntityUI, IWindowEntityUI {




	#region --- SUB ---


	private class WeaponSorter : IComparer<WeaponData> {
		public static readonly WeaponSorter Instance = new();
		public int Compare (WeaponData pairA, WeaponData pairB) {
			var a = pairA.Weapon;
			var b = pairB.Weapon;
			if (a is null) return b is null ? 0 : 1;
			if (b is null) return -1;
			int result = ((int)a.WeaponType).CompareTo((int)b.WeaponType);
			if (result != 0) return result;
			result = ((int)a.Handheld).CompareTo((int)b.Handheld);
			if (result != 0) return result;
			result = a.TypeName.CompareTo(b.TypeName);
			return result;
		}
	}


	private class WeaponData {
		public Weapon Weapon;
		public int InventoryIndex;
		public int Count;
		public void Reset () {
			Weapon = null;
			Count = 0;
			InventoryIndex = -1;
		}
		public void Set (Weapon weapon, int invIndex, int count) {
			Weapon = weapon;
			Count = count;
			InventoryIndex = invIndex;
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode HAND_ICON = "PlayerQMenu.Hand";
	private static readonly LanguageCode HAND_LABEL = ("PlayerQMenu.Hand", "Hand");

	// Api
	public static PlayerQuickMenuUI Instance { get; private set; } = null;
	public static bool ShowingUI => Instance != null && Instance.Active;
	public bool IsDirty { get; private set; } = false;
	public IRect BackgroundRect { get; private set; } = default;

	// Data
	private static readonly WeaponData[] WeaponList = new WeaponData[Character.INVENTORY_ROW * Character.INVENTORY_COLUMN + 2].FillWithNewValue();
	private int CurrentSlotIndex = 0;
	private int WeaponCount = 0;


	#endregion




	#region --- MSG ---


	public PlayerQuickMenuUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();

		X = Renderer.CameraRect.CenterX();
		Y = Renderer.CameraRect.CenterY();
		IsDirty = false;
		CurrentSlotIndex = 0;
		WeaponCount = 0;

		// Init Item List
		int invID = Player.Selecting.TypeID;
		int currentIndex = 0;
		bool allowHand = Inventory.GetEquipment(invID, EquipmentType.Weapon, out _) == 0 || Inventory.IndexOfItem(invID, 0) >= 0;

		// Hand
		if (allowHand) {
			WeaponList[currentIndex].Reset();
			currentIndex++;
			WeaponCount++;
		}

		// Equipping
		int equippingID = Inventory.GetEquipment(invID, EquipmentType.Weapon, out int eqCount);
		if (equippingID != 0 && ItemSystem.GetItem(equippingID) is Weapon equippingItem) {
			WeaponList[currentIndex].Set(equippingItem, -1, eqCount);
			currentIndex++;
			WeaponCount++;
		}

		// Inside Inventory
		int capacity = Inventory.GetInventoryCapacity(invID);
		for (int i = 0; i < capacity && currentIndex < WeaponList.Length; i++) {
			int itemID = Inventory.GetItemAt(invID, i, out int iCount);
			if (
				itemID == 0 ||
				ItemSystem.GetItem(itemID) is not Weapon weapon
			) continue;
			WeaponList[currentIndex].Set(weapon, i, iCount);
			currentIndex++;
			WeaponCount++;
		}
		for (int i = currentIndex; i < WeaponList.Length; i++) {
			WeaponList[i].Reset();
		}
		Util.QuickSort(WeaponList, allowHand ? 1 : 0, WeaponList.Length - 1, WeaponSorter.Instance);

		// Set Current Slot Index
		if (equippingID != 0) {
			for (int i = 0; i < WeaponCount; i++) {
				var weapon = WeaponList[i].Weapon;
				if (weapon == null) continue;
				if ((weapon is BlockItem bItem ? bItem.BlockID : weapon.TypeID) == equippingID) {
					CurrentSlotIndex = i;
					break;
				}
			}
		} else {
			CurrentSlotIndex = 0;
		}
	}


	public override void OnInactivated () {
		base.OnInactivated();
		IsDirty = false;
		WeaponCount = 0;
	}


	public override void UpdateUI () {
		base.UpdateUI();

		if (!Active || Player.Selecting == null || Task.HasTask() || WeaponCount <= 0) {
			Active = false;
			return;
		}

		// Close Menu Check
		if (
			!Input.GameKeyHolding(Gamekey.Start) &&
			!Input.GameKeyHolding(Gamekey.Select)
		) {
			if (IsDirty) {
				Input.UseGameKey(Gamekey.Start);
				Input.UseGameKey(Gamekey.Select);
			}
			if (IsDirty) {
				if (CurrentSlotIndex == 0 && WeaponList[0].Weapon == null) {
					// Hand
					SwitchEquipTo(-1, 0, 0);
				} else if (CurrentSlotIndex >= 0 && CurrentSlotIndex < WeaponCount) {
					// Weapon
					var currentSlot = WeaponList[CurrentSlotIndex];
					if (currentSlot.Weapon != null) {
						if (currentSlot.InventoryIndex >= 0) {
							EquipFromInventory(currentSlot.InventoryIndex);
						}
					}
				}
			}
			Active = false;
			return;
		}

		// Dirty when Wait too Long
		if (Game.GlobalFrame - SpawnFrame > 24) IsDirty = true;

		// Logic
		if (Input.GameKeyDownGUI(Gamekey.Left)) {
			CurrentSlotIndex = (CurrentSlotIndex - 1).UMod(WeaponCount);
			IsDirty = true;
		}
		if (Input.GameKeyDownGUI(Gamekey.Right)) {
			CurrentSlotIndex = (CurrentSlotIndex + 1).UMod(WeaponCount);
			IsDirty = true;
		}

		// Hint
		ControlHintUI.AddHint(Gamekey.Left, BuiltInText.HINT_MOVE);
		ControlHintUI.AddHint(Gamekey.Right, BuiltInText.HINT_MOVE);

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
		int ITEM_SIZE = Unify(42);
		int PADDING = Unify(6);
		int BORDER = Unify(2);

		// BG
		var bgCell = Renderer.DrawPixel(default, Color32.BLACK);

		// Content
		int basicX = player.X - CurrentSlotIndex * ITEM_SIZE - ITEM_SIZE / 2;
		int basicY = player.Y - ITEM_SIZE - PADDING + offsetY - Const.HALF;
		var rect = new IRect(0, basicY, ITEM_SIZE, ITEM_SIZE);
		int countLabelPadding = Unify(2);
		for (int i = 0; i < WeaponCount; i++) {

			var wData = WeaponList[i];
			var weapon = wData.Weapon;
			int wCount = wData.Count;
			if (i != 0 && weapon is null) continue;
			var bItem = weapon as BlockItem;
			int weaponID =
				bItem != null ? bItem.BlockID :
				weapon is null ? 0 :
				weapon.TypeID;

			rect.x = basicX + i * ITEM_SIZE;

			// Cursoring
			if (i == CurrentSlotIndex) {

				// Highlight
				Renderer.DrawPixel(rect.Shrink(BORDER), Color32.GREEN, int.MinValue + 2);

				// Name Label
				int labelWidth = ITEM_SIZE * 3;
				int labelHeight = Unify(36);
				GUI.BackgroundLabel(
					new IRect(
						rect.CenterX() - labelWidth / 2,
						rect.y - labelHeight,
						labelWidth, labelHeight
					),
					weaponID == 0 ? HAND_LABEL : ItemSystem.GetItemDisplayName(weaponID),
					backgroundColor: Color32.BLACK,
					backgroundPadding: Unify(6),
					false,
					GUI.Skin.SmallCenterLabel
				);

			}

			// From Inventory
			if (weaponID == 0) {
				Renderer.Draw(HAND_ICON, rect.Shrink(Unify(7)), z: int.MinValue + 10);
			} else {
				// Icon
				DrawItemIcon(rect, weaponID);
				// Count
				if (wCount > 1 || bItem != null) {
					var labelTint =
						bItem == null ? Color32.GREY_230 :
						bItem.BlockType == BlockType.Background ? Color32.GREY_230 :
						bItem.BlockType == BlockType.Level ? Color32.ORANGE_BETTER :
						Color32.CYAN_BETTER;
					var countRect = rect.Shrink(rect.width * 2 / 3, 0, 0, rect.height * 2 / 3); ;
					var bg = Renderer.DrawPixel(default, Color32.BLACK);
					using (new GUIContentColorScope(labelTint)) {
						GUI.IntLabel(countRect, wCount, out var bounds, GUISkin.Default.SmallCenterLabel);
						bg.SetRect(bounds.Expand(countLabelPadding));
					}
				}
			}

		}

		// BG
		BackgroundRect = new IRect(basicX, basicY, rect.xMax - basicX, ITEM_SIZE).Expand(PADDING);
		bgCell.X = BackgroundRect.x;
		bgCell.Y = BackgroundRect.y;
		bgCell.Width = BackgroundRect.width;
		bgCell.Height = BackgroundRect.height;

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

		if (!Player.Selecting.EquipmentAvailable(EquipmentType.Weapon)) return;

		int invID = Player.Selecting.TypeID;
		if (!Inventory.HasInventory(invID)) return;

		int capacity = Inventory.GetInventoryCapacity(invID);
		if (itemIndex < 0 || itemIndex >= capacity) return;

		int itemID = Inventory.GetItemAt(invID, itemIndex, out int itemCount);
		if (itemID == 0 || itemCount <= 0) return;

		if (!ItemSystem.IsEquipment(itemID, out var eqType) || eqType != EquipmentType.Weapon) return;

		int tookCount = Inventory.TakeItemAt(invID, itemIndex, itemCount);
		if (tookCount <= 0) return;

		SwitchEquipTo(itemIndex, itemID, itemCount);

	}


	private static void SwitchEquipTo (int itemIndex, int newItemID, int newItemCount) {
		int invID = Player.Selecting.TypeID;
		int oldEquipmentID = Inventory.GetEquipment(invID, EquipmentType.Weapon, out int oldEqCount);

		if (!Inventory.SetEquipment(invID, EquipmentType.Weapon, newItemID, newItemCount)) return;

		if (oldEquipmentID != 0) {
			if (itemIndex >= 0) {
				// Swap
				Inventory.SetItemAt(invID, itemIndex, oldEquipmentID, oldEqCount);
			} else {
				// Collect
				int collectCount = Inventory.CollectItem(invID, oldEquipmentID, out _, oldEqCount);
				if (collectCount < oldEqCount) {
					ItemSystem.SpawnItemAtTarget(
						Player.Selecting, oldEquipmentID, oldEqCount - collectCount
					);
				}
			}
		}

	}


	private static void DrawItemIcon (IRect rect, int id) {
		if (id == 0) return;
		if (
			!Renderer.TryGetSprite(id, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) {
			Renderer.TryGetSprite(Const.PIXEL, out sprite);
			rect = rect.Shrink(rect.width / 6);
		}
		Renderer.Draw(
			sprite,
			rect.Shrink(Unify(7)).Fit(sprite),
			z: int.MinValue + 10
		);
	}


	#endregion




}