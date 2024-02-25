using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
[EntityAttribute.DontDestroyOnSquadTransition]
[EntityAttribute.DontDestroyOutOfRange]
[RequireLanguageFromField]
[RequireSpriteFromField]
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
	private static readonly SpriteCode HAND_ICON = "PlayerQMenu.Hand";
	private static readonly LanguageCode HAND_LABEL = ("PlayerQMenu.Hand", "Hand");

	// Api
	public static PlayerQuickMenuUI Instance { get; private set; } = null;
	public static bool ShowingUI => Instance != null && Instance.Active;
	public bool IsDirty { get; private set; } = false;
	public IRect BackgroundRect { get; private set; } = default;

	// Data
	private static readonly Weapon[] WeaponList = new Weapon[Character.INVENTORY_ROW * Character.INVENTORY_COLUMN + 2];
	private static readonly TextContent NameLabel = new() {
		Alignment = Alignment.MidMid,
		BackgroundTint = Color32.BLACK,
		CharSize = 22,
		BackgroundPadding = 2,
	};
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
		bool allowHand = Inventory.GetEquipment(invID, EquipmentType.Weapon) == 0 || Inventory.IndexOfItem(invID, 0) >= 0;

		// Hand
		if (allowHand) {
			WeaponList[currentIndex] = null;
			currentIndex++;
			WeaponCount++;
		}

		// Equipping
		int equippingID = Inventory.GetEquipment(invID, EquipmentType.Weapon);
		if (equippingID != 0 && ItemSystem.GetItem(equippingID) is Weapon equippingItem) {
			WeaponList[currentIndex] = equippingItem;
			currentIndex++;
			WeaponCount++;
		}

		// Inside Inventory
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
		Util.QuickSort(WeaponList, allowHand ? 1 : 0, WeaponList.Length - 1, WeaponSorter.Instance);

		// Set Current Slot Index
		if (equippingID != 0) {
			for (int i = 0; i < WeaponCount; i++) {
				var weapon = WeaponList[i];
				if (weapon != null && weapon.TypeID == equippingID) {
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
				if (CurrentSlotIndex == 0 && WeaponList[0] == null) {
					// Hand
					SwitchEquipTo(0);
				} else if (CurrentSlotIndex >= 0 && CurrentSlotIndex < WeaponCount) {
					// Weapon
					int itemIndex = Inventory.IndexOfItem(
						Player.Selecting.TypeID, WeaponList[CurrentSlotIndex].TypeID
					);
					if (itemIndex >= 0) {
						EquipFromInventory(itemIndex);
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
		int ITEM_SIZE = Unify(56);
		int PADDING = Unify(8);
		int BORDER = Unify(4);

		// Content
		int basicX = player.X - CurrentSlotIndex * ITEM_SIZE - ITEM_SIZE / 2;
		int basicY = player.Y - ITEM_SIZE - PADDING + offsetY - Const.HALF;
		var rect = new IRect(0, basicY, ITEM_SIZE, ITEM_SIZE);
		for (int i = 0; i < WeaponCount; i++) {

			var weapon = WeaponList[i];
			if (i != 0 && weapon is null) continue;
			int weaponID = weapon is null ? 0 : weapon.TypeID;

			rect.x = basicX + i * ITEM_SIZE;

			// Cursoring
			if (i == CurrentSlotIndex) {

				// Highlight
				Renderer.Draw(Const.PIXEL, rect.Shrink(BORDER), Color32.GREEN, int.MinValue + 2);

				// Name Label
				int labelWidth = ITEM_SIZE * 3;
				int labelHeight = Unify(NameLabel.CharSize + 4);
				GUI.Label(
					NameLabel.SetText(weaponID == 0 ? HAND_LABEL : ItemSystem.GetItemName(weaponID)),
					new IRect(
						rect.CenterX() - labelWidth / 2,
						rect.y - labelHeight,
						labelWidth, labelHeight
					)
				);

			}

			// From Inventory
			if (weaponID == 0) {
				Renderer.Draw(HAND_ICON, rect.Shrink(Unify(7)), z: int.MinValue + 10);
			} else {
				DrawItemIcon(rect, weaponID);
			}

		}

		// BG
		BackgroundRect = new IRect(basicX, basicY, rect.xMax - basicX, ITEM_SIZE).Expand(PADDING);
		Renderer.Draw(Const.PIXEL, BackgroundRect, Color32.BLACK, int.MinValue + 1);

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

		SwitchEquipTo(itemID);

	}


	private static void SwitchEquipTo (int newItemID) {
		int invID = Player.Selecting.TypeID;
		int oldEquipmentID = Inventory.GetEquipment(invID, EquipmentType.Weapon);
		if (Inventory.SetEquipment(invID, EquipmentType.Weapon, newItemID)) {
			if (oldEquipmentID != 0) {
				// Collect
				int collectCount = Inventory.CollectItem(invID, oldEquipmentID, out _, 1);
				if (collectCount == 0) {
					ItemSystem.SpawnItemAtTarget(Player.Selecting, oldEquipmentID);
				}
			}
		}
	}


	private static void DrawItemIcon (IRect rect, int id) {
		if (id == 0) return;
		if (!Renderer.TryGetSprite(id, out var sprite)) {
			id = Const.PIXEL;
			Renderer.TryGetSprite(Const.PIXEL, out sprite);
			rect = rect.Shrink(rect.width / 6);
		}
		Renderer.Draw(
			id,
			rect.Shrink(Unify(7)).Fit(sprite),
			z: int.MinValue + 10
		);
	}


	#endregion




}