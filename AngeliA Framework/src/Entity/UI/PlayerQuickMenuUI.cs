using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// UI menu that display when player hold "select" button during gameplay
/// </summary>
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.Capacity(1, 1)]
public class PlayerQuickMenuUI : EntityUI, IWindowEntityUI {




	#region --- SUB ---


	private class HandToolSorter : IComparer<HandToolData> {
		public static readonly HandToolSorter Instance = new();
		public int Compare (HandToolData pairA, HandToolData pairB) {
			var a = pairA.Tool;
			var b = pairB.Tool;
			if (a is null) return b is null ? 0 : 1;
			if (b is null) return -1;
			int result = a.TypeName.CompareTo(b.TypeName);
			return result;
		}
	}


	private class HandToolData {
		public HandTool Tool;
		public int InventoryIndex;
		public int Count;
		public void Reset () {
			Tool = null;
			Count = 0;
			InventoryIndex = -1;
		}
		public void Set (HandTool tool, int invIndex, int count) {
			Tool = tool;
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
	/// <summary>
	/// Global single instance for this entity
	/// </summary>
	public static PlayerQuickMenuUI Instance { get; private set; } = null;
	/// <summary>
	/// True if the ui id currently displaying
	/// </summary>
	public static bool ShowingUI => Instance != null && Instance.Active;
	/// <summary>
	/// Rect position for background panel range in global size
	/// </summary>
	public IRect BackgroundRect { get; private set; } = default;
	internal bool IsDirty { get; private set; } = false;

	// Data
	private static HandToolData[] HandToolList;
	private int CurrentSlotIndex = 0;
	private int HandToolCount = 0;


	#endregion




	#region --- MSG ---


	public PlayerQuickMenuUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();

		var player = PlayerSystem.Selecting;
		if (player == null || !player.Active) {
			Active = false;
			return;
		}

		int targetLen = player.InventoryColumn * player.InventoryRow;
		if (HandToolList == null || HandToolList.Length != targetLen) {
			HandToolList = new HandToolData[targetLen + 2].FillWithNewValue();
		}

		X = Renderer.CameraRect.CenterX();
		Y = Renderer.CameraRect.CenterY();
		IsDirty = false;
		CurrentSlotIndex = 0;
		HandToolCount = 0;

		// Init Item List
		int invID = player.InventoryID;
		int currentIndex = 0;
		bool allowHand =
			Inventory.GetEquipment(invID, EquipmentType.HandTool, out _) == 0 ||
			Inventory.HasItem(invID, 0, includeEquipment: false);

		// Hand
		if (allowHand) {
			HandToolList[currentIndex].Reset();
			currentIndex++;
			HandToolCount++;
		}

		// Equipping
		int equippingID = Inventory.GetEquipment(invID, EquipmentType.HandTool, out int eqCount);
		if (equippingID != 0 && ItemSystem.GetItem(equippingID) is HandTool equippingItem) {
			HandToolList[currentIndex].Set(equippingItem, -1, eqCount);
			currentIndex++;
			HandToolCount++;
		}

		// Inside Inventory
		int capacity = Inventory.GetInventoryCapacity(invID);
		for (int i = 0; i < capacity && currentIndex < HandToolList.Length; i++) {
			int itemID = Inventory.GetItemAt(invID, i, out int iCount);
			if (
				itemID == 0 ||
				ItemSystem.GetItem(itemID) is not HandTool tool
			) continue;
			HandToolList[currentIndex].Set(tool, i, iCount);
			currentIndex++;
			HandToolCount++;
		}
		for (int i = currentIndex; i < HandToolList.Length; i++) {
			HandToolList[i].Reset();
		}
		Util.QuickSort(HandToolList, allowHand ? 1 : 0, HandToolList.Length - 1, HandToolSorter.Instance);

		// Set Current Slot Index
		if (equippingID != 0) {
			for (int i = 0; i < HandToolCount; i++) {
				var tool = HandToolList[i].Tool;
				if (tool == null) continue;
				if ((tool is BlockBuilder bItem ? bItem.BlockID : tool.TypeID) == equippingID) {
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
		HandToolCount = 0;
	}


	public override void UpdateUI () {
		base.UpdateUI();

		if (!Active || PlayerSystem.Selecting == null || TaskSystem.HasTask() || HandToolCount <= 0) {
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
				if (CurrentSlotIndex == 0 && HandToolList[0].Tool == null) {
					// Hand
					SwitchEquipTo(-1, 0, 0);
				} else if (CurrentSlotIndex >= 0 && CurrentSlotIndex < HandToolCount) {
					// Tool
					var currentSlot = HandToolList[CurrentSlotIndex];
					if (currentSlot.Tool != null) {
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
			CurrentSlotIndex = (CurrentSlotIndex - 1).UMod(HandToolCount);
			IsDirty = true;
		}
		if (Input.GameKeyDownGUI(Gamekey.Right)) {
			CurrentSlotIndex = (CurrentSlotIndex + 1).UMod(HandToolCount);
			IsDirty = true;
		}
		if (Input.GameKeyDown(Gamekey.Down) || Input.GameKeyDown(Gamekey.Up)) {
			CurrentSlotIndex = 0;
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
		var player = PlayerSystem.Selecting;
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
		for (int i = 0; i < HandToolCount; i++) {

			var wData = HandToolList[i];
			var tool = wData.Tool;
			int wCount = wData.Count;
			if (i != 0 && tool is null) continue;
			var bItem = tool as BlockBuilder;
			int toolID =
				bItem != null ? bItem.BlockID :
				tool is null ? 0 :
				tool.TypeID;

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
					toolID == 0 ? HAND_LABEL : ItemSystem.GetItemDisplayName(toolID),
					backgroundColor: Color32.BLACK,
					backgroundPadding: Unify(6),
					false,
					GUI.Skin.SmallCenterLabel
				);

			}

			// From Inventory
			if (toolID == 0) {
				Renderer.Draw(HAND_ICON, rect.Shrink(Unify(7)), z: int.MinValue + 10);
			} else {
				// Icon
				DrawItemIcon(rect, toolID);

				if (tool.UseStackAsUsage) {
					// Usage
					FrameworkUtil.DrawItemUsageBar(rect.EdgeInsideDown(rect.height / 4), wCount, tool.MaxStackCount);
				} else {
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


	/// <summary>
	/// Open the quick menu UI
	/// </summary>
	/// <returns>Instance of the opened menu</returns>
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


	/// <summary>
	/// Close the quick menu UI if it's opening
	/// </summary>
	public static void CloseMenu () {
		if (Instance == null) return;
		Instance.Active = false;
	}


	#endregion




	#region --- LGC ---


	private static void EquipFromInventory (int itemIndex) {

		if (!PlayerSystem.Selecting.EquipmentAvailable(EquipmentType.HandTool)) return;

		int invID = PlayerSystem.Selecting.InventoryID;
		if (!Inventory.HasInventory(invID)) return;

		int capacity = Inventory.GetInventoryCapacity(invID);
		if (itemIndex < 0 || itemIndex >= capacity) return;

		int itemID = Inventory.GetItemAt(invID, itemIndex, out int itemCount);
		if (itemID == 0 || itemCount <= 0) return;

		if (!ItemSystem.IsEquipment(itemID, out var eqType) || eqType != EquipmentType.HandTool) return;

		int tookCount = Inventory.TakeItemAt(invID, itemIndex, itemCount);
		if (tookCount <= 0) return;

		SwitchEquipTo(itemIndex, itemID, itemCount);

	}


	private static void SwitchEquipTo (int itemIndex, int newItemID, int newItemCount) {
		int invID = PlayerSystem.Selecting.InventoryID;
		int oldEquipmentID = Inventory.GetEquipment(invID, EquipmentType.HandTool, out int oldEqCount);

		if (!Inventory.SetEquipment(invID, EquipmentType.HandTool, newItemID, newItemCount)) return;

		if (oldEquipmentID != 0) {
			if (itemIndex >= 0) {
				// Swap
				Inventory.SetItemAt(invID, itemIndex, oldEquipmentID, oldEqCount);
			} else {
				// Collect
				int collectCount = Inventory.CollectItem(invID, oldEquipmentID, out _, oldEqCount);
				if (collectCount < oldEqCount) {
					Inventory.GiveItemToTarget(
						PlayerSystem.Selecting, oldEquipmentID, oldEqCount - collectCount
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
			Renderer.TryGetSprite(Const.PIXEL, out sprite, false);
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