using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;


public sealed class InventoryPartnerUI : PlayerMenuPartnerUI {
	public static readonly InventoryPartnerUI Instance = new();
	public int AvatarIcon = 0;
	public override void DrawPanel (IRect panelRect) {
		base.DrawPanel(panelRect);
		PlayerMenuUI.DrawTopInventory(InventoryID, Column, Row);
	}
}


public abstract class PlayerMenuPartnerUI : IWindowEntityUI {

	public int InventoryID { get; private set; } = 0;
	public int Column { get; private set; } = 1;
	public int Row { get; private set; } = 1;
	public int ItemSize { get; private set; } = PlayerMenuUI.ITEM_SIZE;
	public bool MouseInPanel { get; set; } = false;
	public IRect BackgroundRect { get; protected set; } = default;

	public virtual void EnablePanel (int inventoryID, int column, int row, int itemSize = PlayerMenuUI.ITEM_SIZE) {
		InventoryID = inventoryID;
		Column = column;
		Row = row;
		ItemSize = itemSize;
	}

	public virtual void DrawPanel (IRect panelRect) => BackgroundRect = panelRect;

	protected static int Unify (int value) => GUI.Unify(value);

}


[EntityAttribute.DontDestroyOnSquadTransition]
[EntityAttribute.DontDestroyOutOfRange]
[RequireSpriteFromField]
[RequireLanguageFromField]
public class PlayerMenuUI : EntityUI {




	#region --- VAR ---


	// Const
	private static readonly int FRAME_CODE = BuiltInSprite.FRAME_16;
	private static readonly int ITEM_FRAME_CODE = BuiltInSprite.UI_ITEM_FRAME;
	private static readonly SpriteCode ARMOR_ICON = "ArmorIcon";
	private static readonly SpriteCode ARMOR_EMPTY_ICON = "ArmorEmptyIcon";
	private static readonly LanguageCode HINT_HIDE_MENU = ("CtrlHint.HideMenu", "Hide Menu");
	private static readonly LanguageCode HINT_TAKE = ("CtrlHint.PlayerMenu.Take", "Take");
	private static readonly LanguageCode HINT_DROP = ("CtrlHint.Drop", "Drop");
	private static readonly LanguageCode HINT_THROW = ("CtrlHint.Throw", "Throw");
	private static readonly LanguageCode HINT_HUSE = ("CtrlHint.HoldToUse", "Hold to Use");
	private static readonly LanguageCode HINT_TRANSFER = ("CtrlHint.Transfer", "Transfer");
	private static readonly LanguageCode HINT_EQUIP = ("CtrlHint.Equip", "Equip");
	private static readonly LanguageCode UI_HELMET = ("UI.Equipment.Helmet", "Helmet");
	private static readonly LanguageCode UI_WEAPON = ("UI.Equipment.Weapon", "Weapon");
	private static readonly LanguageCode UI_SHOES = ("UI.Equipment.Shoes", "Shoes");
	private static readonly LanguageCode UI_GLOVES = ("UI.Equipment.Gloves", "Gloves");
	private static readonly LanguageCode UI_BODYSUIT = ("UI.Equipment.Bodysuit", "Cloth");
	private static readonly LanguageCode UI_JEWELRY = ("UI.Equipment.Jewelry", "Jewelry");
	private const int WINDOW_PADDING = 12;
	private const int HOLD_KEY_DURATION = 26;
	private const int ANIMATION_DURATION = 12;
	private const int FLASH_PANEL_DURATION = 52;
	public const int PREVIEW_SIZE = 220;
	public const int INFO_WIDTH = 180;
	public const int ITEM_SIZE = 52;

	// Api
	public static PlayerMenuUI Instance { get; private set; } = null;
	public static bool ShowingUI => Instance != null && Instance.Active;
	public PlayerMenuPartnerUI Partner { get; set; } = null;
	public int TopPanelColumn => Partner != null ? Partner.Column : 2;
	public int TopPanelRow => Partner != null ? Partner.Row : 3;
	public int CursorIndex { get; set; } = 0;
	public bool CursorInBottomPanel { get; set; } = true;
	public int TakingID { get; private set; } = 0;
	public int TakingCount { get; private set; } = 0;

	// Data
	private readonly IntToChars ItemCountChars = new();
	private int TakingFromIndex = 0;
	private int ActionKeyDownFrame = int.MinValue;
	private int CancelKeyDownFrame = int.MinValue;
	private int HoveringItemID = 0;
	private int MouseHoveringItemIndex = 0;
	private int EquipFlashStartFrame = int.MinValue;
	private int PrevCursorIndex = -1;
	private bool TakingFromBottomPanel = false;
	private bool MouseInPanel = false;
	private bool RenderingBottomPanel = false;
	private bool HoveringItemField = false;
	private bool PrevCursorInBottomPanel = true;
	private bool UsingMouseMode = false;
	private EquipmentType EquipFlashType = EquipmentType.BodyArmor;
	private IRect TopPanelRect = default;
	private IRect BottomPanelRect = default;
	private IRect HoveringItemUiRect = default;
	private Int3 FlashingField = new(-1, 0, 0);


	#endregion




	#region --- MSG ---


	public PlayerMenuUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		Partner = null;
		TakingID = 0;
		TakingCount = 0;
		CursorIndex = 0;
		PrevCursorIndex = 0;
		TakingFromIndex = 0;
		ActionKeyDownFrame = -1;
		CancelKeyDownFrame = -1;
		HoveringItemID = 0;
		MouseInPanel = false;
		EquipFlashStartFrame = int.MinValue;
		CursorInBottomPanel = true;
		PrevCursorInBottomPanel = true;
		UsingMouseMode = false;
		MouseHoveringItemIndex = int.MaxValue;
		FlashingField = new(-1, 0, 0);
		X = Renderer.CameraRect.CenterX();
		Y = Renderer.CameraRect.CenterY();
	}


	public override void OnInactivated () {
		base.OnInactivated();
		if (TakingID != 0) {
			AbandonTaking();
		}
	}


	public override void UpdateUI () {
		base.UpdateUI();

		if (Player.Selecting == null) {
			Active = false;
			return;
		}
		Cursor.RequireCursor();

		Update_PanelUI();
		Update_InfoUI();
		Update_MoveCursor();
		Update_Actions();
		DrawTakingItemMouseCursor();

		Update_CloseMenu();

		if (CursorIndex != PrevCursorIndex || CursorInBottomPanel != PrevCursorInBottomPanel) {
			PrevCursorIndex = CursorIndex;
			PrevCursorInBottomPanel = CursorInBottomPanel;
			Input.UseGameKey(Gamekey.Action);
			Input.UseGameKey(Gamekey.Jump);
		}
	}


	private void Update_PanelUI () {

		HoveringItemField = false;
		MouseInPanel = false;
		HoveringItemID = 0;

		// Bottom Panel
		RenderingBottomPanel = true;
		var playerPanelRect = GetPanelRect(Character.INVENTORY_COLUMN, Character.INVENTORY_ROW, ITEM_SIZE, false);
		Renderer.Draw(Const.PIXEL, playerPanelRect.Expand(Unify(WINDOW_PADDING)), Color32.BLACK, int.MinValue + 1);
		DrawInventory(Player.Selecting.TypeID, Character.INVENTORY_COLUMN, Character.INVENTORY_ROW, false);

		// Top Panel
		RenderingBottomPanel = false;
		if (Partner != null) {
			// Partner Panel
			var panelRect = GetPanelRect(Partner.Column, Partner.Row, Partner.ItemSize, true);
			Renderer.Draw(Const.PIXEL, panelRect.Expand(Unify(WINDOW_PADDING)), Color32.BLACK, int.MinValue + 1);
			Partner.MouseInPanel = panelRect.MouseInside();
			Partner.DrawPanel(panelRect);
			TopPanelRect = panelRect;
			MouseInPanel = MouseInPanel || Partner.MouseInPanel;
		} else {
			// Equipment Panel
			DrawEquipmentUI();
		}

		if (!HoveringItemField) {
			MouseHoveringItemIndex = int.MaxValue;
			if (Input.MouseLeftButtonDown) {
				Input.UseGameKey(Gamekey.Action);
			}
			if (Input.MouseRightButtonDown) {
				Input.UseGameKey(Gamekey.Jump);
			}
		}

	}


	private void Update_InfoUI () {

		int itemID = HoveringItemID;
		if (itemID == 0 || TakingID != 0) return;
		if (!CursorInBottomPanel && Partner != null && Partner is not InventoryPartnerUI) return;

		int panelWidth = Unify(INFO_WIDTH);
		int windowPadding = Unify(WINDOW_PADDING);
		int labelHeight = Unify(24);
		var topRootRect = TopPanelRect;
		var bottomRootRect = BottomPanelRect;
		var topPanelRect = new IRect(
			topRootRect.xMax + windowPadding * 4, topRootRect.y,
			panelWidth, topRootRect.height
		);
		var bottomPanelRect = new IRect(
			bottomRootRect.xMax + windowPadding * 4, bottomRootRect.y,
			panelWidth, bottomRootRect.height
		);
		var panelRect = CursorInBottomPanel ? bottomPanelRect : topPanelRect;

		// Mouse in Panel
		MouseInPanel = MouseInPanel || panelRect.MouseInside();

		// Background
		var bgCell = Renderer.Draw(Const.PIXEL, default, Color32.BLACK);

		// Type Icon
		Renderer.Draw(
			ItemSystem.GetItemTypeIcon(itemID),
			new IRect(panelRect.x, panelRect.yMax - labelHeight, labelHeight, labelHeight), Color32.ORANGE_BETTER, int.MinValue + 3
		);

		// Name
		var nameRect = new IRect(panelRect.x + labelHeight + labelHeight / 4, panelRect.yMax - labelHeight, panelRect.width, labelHeight);
		using (ContentColorScope.Start(Color32.ORANGE_BETTER)) {
			GUI.Label(nameRect, ItemSystem.GetItemName(itemID), GUISkin.MiniLabel);
		}

		// Description
		GUI.Label(
			panelRect.Shrink(0, 0, 0, labelHeight + Unify(12)),
			ItemSystem.GetItemDescription(itemID),
			out var desBounds, GUISkin.MiniTextArea
		);

		// Final
		bgCell.X = panelRect.x - windowPadding;
		bgCell.Y = desBounds.y - windowPadding;
		bgCell.Width = panelRect.width + 2 * windowPadding;
		bgCell.Height = nameRect.yMax - desBounds.y + 2 * windowPadding;

	}


	private void Update_CloseMenu () {

		// Mouse
		if (
			!MouseInPanel &&
			Game.GlobalFrame != SpawnFrame &&
			Input.AnyMouseButtonDown
		) {
			if (TakingID == 0) {
				Active = false;
			} else {
				ThrowTakingToGround();
			}
		}

		// Key
		if (Input.GameKeyUp(Gamekey.Select) || Input.GameKeyUp(Gamekey.Start)) {
			Input.UseGameKey(Gamekey.Select);
			Input.UseGameKey(Gamekey.Start);
			Active = false;
			return;
		}

		// Hint
		ControlHintUI.AddHint(Gamekey.Select, HINT_HIDE_MENU);
	}


	private void Update_MoveCursor () {

		ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
		ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);
		ControlHintUI.AddHint(Gamekey.Action, "", int.MinValue + 2);

		if (Input.DirectionX == Direction3.None && Input.DirectionY == Direction3.None) return;

		int column = CursorInBottomPanel ? Character.INVENTORY_COLUMN : TopPanelColumn;
		int row = CursorInBottomPanel ? Character.INVENTORY_ROW : TopPanelRow;
		int x = CursorIndex % column;
		int y = CursorIndex / column;

		// Left
		if (Input.GameKeyDownGUI(Gamekey.Left)) {
			x = Util.Max(x - 1, 0);
			UsingMouseMode = false;
		}

		// Right
		if (Input.GameKeyDownGUI(Gamekey.Right)) {
			x = Util.Min(x + 1, column - 1);
			UsingMouseMode = false;
		}

		// Down
		if (Input.GameKeyDownGUI(Gamekey.Down)) {
			UsingMouseMode = false;
			if (!CursorInBottomPanel && y == 0) {
				CursorInBottomPanel = true;
				if (Partner == null) {
					x = x == 0 ? 0 : Character.INVENTORY_COLUMN / 2;
				} else {
					x = CursorWrap(x, false);
				}
				y = Character.INVENTORY_ROW - 1;
				column = Character.INVENTORY_COLUMN;
				row = Character.INVENTORY_ROW;
			} else {
				y = Util.Max(y - 1, 0);
			}
		}

		// Up
		if (Input.GameKeyDownGUI(Gamekey.Up)) {
			UsingMouseMode = false;
			if (CursorInBottomPanel && y == row - 1) {
				CursorInBottomPanel = false;
				if (Partner == null) {
					x = x < Character.INVENTORY_COLUMN / 2 ? 0 : 1;
				} else {
					x = CursorWrap(x, true);
				}
				y = 0;
				column = TopPanelColumn;
				row = TopPanelRow;
			} else {
				y = Util.Min(y + 1, row - 1);
			}
		}

		x = x.Clamp(0, column - 1);
		y = y.Clamp(0, row - 1);

		CursorIndex = y * column + x;

	}


	private void Update_Actions () {

		// Action Cache
		int oldCursorIndex = CursorIndex;
		bool cursorChanged = CursorIndex != PrevCursorIndex;
		bool actionDown = Input.GameKeyDown(Gamekey.Action);
		bool cancelDown = !actionDown && Input.GameKeyDown(Gamekey.Jump);
		bool actionUp = Input.GameKeyUp(Gamekey.Action);
		bool actionHolding = Input.GameKeyHolding(Gamekey.Action);
		bool cancelHolding = !actionHolding && Input.GameKeyHolding(Gamekey.Jump);

		if (cursorChanged && (actionHolding || cancelHolding) && CursorInBottomPanel == PrevCursorInBottomPanel) {
			if (actionHolding) actionUp = true;
			actionHolding = false;
			cancelHolding = false;
			actionDown = false;
			cancelDown = false;
			CursorIndex = PrevCursorIndex;
		}

		if (actionDown) ActionKeyDownFrame = Game.GlobalFrame;
		if (cancelDown) CancelKeyDownFrame = Game.GlobalFrame;
		bool intendedDrop = actionDown;
		bool intendedStartTake = actionUp && Game.GlobalFrame < ActionKeyDownFrame + HOLD_KEY_DURATION;
		bool intendedHoldAction =
			actionHolding &&
			ActionKeyDownFrame >= 0 &&
			Game.GlobalFrame >= ActionKeyDownFrame + HOLD_KEY_DURATION;
		bool intendedHoldCancel =
			!actionHolding && cancelHolding &&
			CancelKeyDownFrame >= 0 &&
			Game.GlobalFrame >= CancelKeyDownFrame + HOLD_KEY_DURATION;
		if (!actionHolding) ActionKeyDownFrame = int.MinValue;
		if (!cancelHolding) CancelKeyDownFrame = int.MinValue;
		if (intendedHoldAction) Input.UseGameKey(Gamekey.Action);
		if (intendedHoldCancel) Input.UseGameKey(Gamekey.Jump);

		// Inv Logic
		int invID = CursorInBottomPanel ? Player.Selecting.TypeID : Partner != null ? Partner.InventoryID : 0;
		if (invID == 0) return;
		int cursorLength = Inventory.GetInventoryCapacity(invID);
		if (TakingID == 0) {
			// Normal
			if (CursorIndex >= 0 && CursorIndex < cursorLength) {
				// Try Start Take
				int cursorID = Inventory.GetItemAt(invID, CursorIndex, out int cursorCount);
				if (cursorID != 0) {
					if (intendedStartTake) {
						// Start Take
						TakeItemAtCursor();
					} else if (intendedHoldAction) {
						// Hold
						if (cursorCount > 1) SplitItemAtCursor();
					} else if (cancelDown) {
						// Quick
						QuickDropAtCursor_FromInventory();
					} else if (intendedHoldCancel) {
						// Use
						UseAtCursor();
					}
					bool cursoringEquip = ItemSystem.IsEquipment(cursorID);
					ControlHintUI.AddHint(Gamekey.Action, HINT_TAKE, int.MinValue + 3);
					if (Partner != null) {
						// Has Partner
						if (Partner.InventoryID != 0) {
							ControlHintUI.AddHint(Gamekey.Jump, HINT_TRANSFER, int.MinValue + 3);
						}
					} else if (cursoringEquip) {
						// Equip
						ControlHintUI.AddHint(Gamekey.Jump, HINT_EQUIP, int.MinValue + 3);
					} else {
						// Use
						if (ItemSystem.CanUseItem(cursorID, Player.Selecting)) {
							ControlHintUI.AddHint(Gamekey.Jump, HINT_HUSE, int.MinValue + 3);
						}
					}
				}
			}
		} else {
			// Taking
			if (CursorIndex >= 0 && CursorIndex < cursorLength && intendedDrop) {
				// Perform Drop
				DropTakingToCursor();
				ActionKeyDownFrame = int.MinValue;
				CancelKeyDownFrame = int.MinValue;
			}
			if (cancelDown) {
				// Throw to Ground
				ThrowTakingToGround();
			}
			ControlHintUI.AddHint(Gamekey.Action, HINT_DROP, int.MinValue + 3);
			ControlHintUI.AddHint(Gamekey.Jump, HINT_THROW, int.MinValue + 3);
		}

		CursorIndex = oldCursorIndex;

	}


	private void DrawTakingItemMouseCursor () {
		if (!UsingMouseMode) {
			GUI.HighlightCursor(FRAME_CODE, HoveringItemUiRect);
		}
		if (TakingID == 0) return;
		int size = Unify(ITEM_SIZE);
		var itemRect = UsingMouseMode ?
			new IRect(Input.MouseGlobalPosition.x - size / 2, Input.MouseGlobalPosition.y - size / 2, size, size) :
			HoveringItemUiRect;

		// Exclude Text for Count Mark
		var countRect = itemRect.Shrink(itemRect.width * 2 / 3, 0, 0, itemRect.height * 2 / 3);
		for (int i = 0; i < Renderer.TextLayerCount; i++) {
			Renderer.ExcludeTextCells(i, countRect, 0);
		}

		// Item Icon
		Renderer.Draw(
			TakingID,
			itemRect.x + size / 2, itemRect.y, 500, 500, Game.GlobalFrame.PingPong(30) - 15,
			size, size, Color32.WHITE, int.MaxValue
		);
		// Item Count
		DrawItemCount(countRect, TakingCount);
	}


	#endregion




	#region --- API ---


	public static PlayerMenuUI OpenMenu () {
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


	public static void CloseMenu () {
		if (Instance == null) return;
		if (Instance.Active) Instance.Active = false;
	}


	public static void DrawTopInventory (int inventoryID, int column, int row) => Instance?.DrawInventory(inventoryID, column, row, true);


	public static void DrawItemFieldUI (int itemID, int itemCount, int frameCode, IRect itemRect, bool interactable, int uiIndex) => Instance?.DrawItemField(itemID, itemCount, frameCode, itemRect, interactable, uiIndex);


	public void SetTaking (int takingID, int takingCount = 1) {
		TakingID = takingID;
		TakingCount = takingCount;
	}


	#endregion




	#region --- LGC ---


	// Inventory UI
	private void DrawInventory (int inventoryID, int column, int row, bool panelOnTop) {

		if (inventoryID == 0 || !Inventory.HasInventory(inventoryID)) return;

		var itemCount = Inventory.GetInventoryCapacity(inventoryID);
		bool interactable = Game.GlobalFrame - SpawnFrame > ANIMATION_DURATION;
		var panelRect = GetPanelRect(column, row, ITEM_SIZE, panelOnTop);
		if (panelOnTop) {
			TopPanelRect = panelRect;
		} else {
			BottomPanelRect = panelRect;
		}

		var windowRect = panelRect.Expand(Unify(WINDOW_PADDING));
		MouseInPanel = MouseInPanel || windowRect.MouseInside();

		// Content
		int index = 0;
		var itemRect = new IRect(0, 0, panelRect.width / column, panelRect.height / row);
		for (int j = 0; j < row; j++) {
			for (int i = 0; i < column; i++, index++) {
				if (index >= itemCount) {
					j = row;
					break;
				}
				int id = Inventory.GetItemAt(inventoryID, index, out int iCount);
				itemRect.x = panelRect.x + i * itemRect.width;
				itemRect.y = panelRect.y + j * itemRect.height;
				DrawItemField(id, iCount, ITEM_FRAME_CODE, itemRect, interactable, index);
			}
		}

	}


	public void DrawItemField (int itemID, int itemCount, int frameCode, IRect itemRect, bool interactable, int uiIndex) {

		if (itemCount <= 0) itemID = 0;
		bool actionHolding = Input.GameKeyHolding(Gamekey.Action);
		bool cancelHolding = Input.GameKeyHolding(Gamekey.Jump);
		bool mouseHovering = interactable && itemRect.MouseInside();
		int cursorIndex = RenderingBottomPanel == CursorInBottomPanel ? CursorIndex : -1;
		HoveringItemField = HoveringItemField || mouseHovering;

		// Frame
		int frameBorder = Unify(1);
		Renderer.Draw_9Slice(
			frameCode,
			itemRect.x,
			itemRect.y, 0, 0, 0, itemRect.width, itemRect.height,
			frameBorder, frameBorder, frameBorder, frameBorder,
			Const.SliceIgnoreCenter, Color32.WHITE, int.MinValue + 3
		);

		// Mouse Hovering
		if (mouseHovering) {
			if (MouseHoveringItemIndex != uiIndex) {
				// Mouse Hovering Change
				UsingMouseMode = true;
				MouseHoveringItemIndex = uiIndex;
			}
			if (UsingMouseMode) {
				HoveringItemID = itemID;
				// Move UI Cursor from Mouse
				cursorIndex = CursorIndex = uiIndex;
				CursorInBottomPanel = RenderingBottomPanel;
				// Draw Highlight
				Renderer.Draw(Const.PIXEL, itemRect, Color32.GREY_42, int.MinValue + 2);
			}
			if (itemID != 0) {
				// System Mouse Cursor
				Cursor.SetCursorAsHand();
			}
		}

		// Icon
		DrawItemIcon(itemRect, itemID, Color32.WHITE, int.MinValue + 4);

		// Count
		DrawItemCount(itemRect.Shrink(itemRect.width * 2 / 3, 0, 0, itemRect.height * 2 / 3), itemCount);

		// UI Cursor
		if (!UsingMouseMode && cursorIndex == uiIndex) {
			HoveringItemID = itemID;
			HoveringItemUiRect = itemRect;
		}

		// Flashing
		if (
			Game.GlobalFrame < FlashingField.y &&
			FlashingField.z == 0 == RenderingBottomPanel &&
			FlashingField.x >= 0 &&
			FlashingField.x == uiIndex
		) {
			var tint = Color32.GREEN;
			tint.a = (byte)Util.RemapUnclamped(FLASH_PANEL_DURATION, 0, 255, 0, FlashingField.y - Game.GlobalFrame);
			Renderer.Draw(Const.PIXEL, itemRect, tint, int.MinValue + 3);
		}

		// Holding
		if (itemID != 0 && cursorIndex == uiIndex) {
			if (
				itemCount > 1 &&
				actionHolding && TakingID == 0 &&
				ActionKeyDownFrame >= 0 &&
				Game.GlobalFrame >= ActionKeyDownFrame + 6
			) {
				var cell = Renderer.Draw(Const.PIXEL, itemRect, Color32.GREY_96, int.MinValue + 3);
				cell.Shift = new Int4(
					0, 0, 0,
					Util.RemapUnclamped(
						ActionKeyDownFrame + 6, ActionKeyDownFrame + HOLD_KEY_DURATION,
						itemRect.height, 0,
						Game.GlobalFrame
					)
				);
			}
			if (
				cancelHolding && TakingID == 0 &&
				CancelKeyDownFrame >= 0 &&
				Game.GlobalFrame >= CancelKeyDownFrame + 6
			) {
				if (ItemSystem.CanUseItem(itemID, Player.Selecting)) {
					var cell = Renderer.Draw(Const.PIXEL, itemRect, Color32.GREEN, int.MinValue + 3);
					cell.Shift = new Int4(
						0, 0, 0,
						Util.RemapUnclamped(
							CancelKeyDownFrame + 6, CancelKeyDownFrame + HOLD_KEY_DURATION,
							itemRect.height, 0,
							Game.GlobalFrame
						)
					);
				}
			}
		}

	}


	// Equipment UI
	private void DrawEquipmentUI () {

		bool interactable = Game.GlobalFrame - SpawnFrame > ANIMATION_DURATION;
		var player = Player.Selecting;
		int previewWidth = Unify(PREVIEW_SIZE);
		int itemHeight = Unify(64);
		int hashContentHeight = Unify(128);
		var panelRect = TopPanelRect = GetInventoryRect(itemHeight).Expand(
			previewWidth, 0, 0, hashContentHeight
		);

		// Background
		var windowRect = panelRect.Expand(Unify(WINDOW_PADDING));
		Renderer.Draw(Const.PIXEL, windowRect, Color32.BLACK, int.MinValue + 1);
		MouseInPanel = MouseInPanel || windowRect.MouseInside();

		// Content
		int width = (panelRect.width - previewWidth) / 2;
		int left = panelRect.x + previewWidth;
		int top = panelRect.yMax;
		DrawEquipmentItem(
			0, interactable && player.JewelryAvailable, new IRect(left, top - itemHeight * 3, width, itemHeight),
			EquipmentType.Jewelry, UI_JEWELRY
		);
		DrawEquipmentItem(
			1, interactable && player.ShoesAvailable, new IRect(left + width, top - itemHeight * 3, width, itemHeight),
			EquipmentType.Shoes, UI_SHOES
		);
		DrawEquipmentItem(
			2, interactable && player.GlovesAvailable, new IRect(left, top - itemHeight * 2, width, itemHeight),
			EquipmentType.Gloves, UI_GLOVES
		);
		DrawEquipmentItem(
			3, interactable && player.BodySuitAvailable, new IRect(left + width, top - itemHeight * 2, width, itemHeight),
			EquipmentType.BodyArmor, UI_BODYSUIT
		);
		DrawEquipmentItem(
			4, interactable && player.WeaponAvailable, new IRect(left, top - itemHeight * 1, width, itemHeight),
			EquipmentType.Weapon, UI_WEAPON
		);
		DrawEquipmentItem(
			5, interactable && player.HelmetAvailable, new IRect(left + width, top - itemHeight * 1, width, itemHeight),
			EquipmentType.Helmet, UI_HELMET
		);

		// Hashtag




		// Preview
		var previewRect = panelRect.EdgeOutside(Direction4.Left, previewWidth).Shift(previewWidth, 0);
		FrameworkUtil.DrawPoseCharacterAsUI(previewRect, player, player.CurrentAnimationFrame, 0, out _, out _);
		if (Input.MouseLeftButtonDown && previewRect.MouseInside()) {
			player.FacingRight = !player.FacingRight;
			player.Bounce();
		}

	}


	private void DrawEquipmentItem (int index, bool interactable, IRect rect, EquipmentType type, string label) {

		int itemID = Inventory.GetEquipment(Player.Selecting.TypeID, type);
		int fieldPadding = Unify(4);
		var fieldRect = rect.Shrink(fieldPadding);
		bool actionDown = interactable && Input.GameKeyDown(Gamekey.Action);
		bool cancelDown = interactable && Input.GameKeyDown(Gamekey.Jump);
		var enableTint = Color32.WHITE;
		var equipAvailable = Player.Selecting.EquipmentAvailable(type);
		bool mouseHovering = interactable && rect.MouseInside();
		HoveringItemField = HoveringItemField || mouseHovering;
		if (TakingID != 0 && ItemSystem.IsEquipment(TakingID, out var takingType) && type != takingType) {
			enableTint.a = 96;
			interactable = false;
		}
		var itemRect = new IRect(fieldRect.x, fieldRect.y, fieldRect.height, fieldRect.height);

		// Highlight
		bool highlighting = false;
		if (UsingMouseMode) {
			if (mouseHovering) {
				CursorIndex = index;
				CursorInBottomPanel = false;
				Renderer.Draw(Const.PIXEL, rect, Color32.GREY_32, int.MinValue + 1);
				highlighting = true;
			}
		} else {
			if (CursorIndex == index && !CursorInBottomPanel) {
				highlighting = true;
				HoveringItemUiRect = itemRect;
			}
		}
		if (highlighting) HoveringItemID = itemID;

		// Item Frame
		if (equipAvailable) {
			int border = Unify(4);
			Renderer.Draw_9Slice(ITEM_FRAME_CODE, itemRect, border, border, border, border, enableTint, int.MinValue + 2);
		}

		// Icon
		if (!equipAvailable || !interactable) enableTint.a = 96;
		DrawItemIcon(itemRect, itemID, enableTint, int.MinValue + 3);

		// Label
		using (ContentColorScope.Start(enableTint)) {
			GUI.Label(fieldRect.Shrink(itemRect.width + fieldPadding * 3, 0, itemRect.height / 2, 0), label);
		}

		// Progressive Icon
		ItemSystem.DrawItemShortInfo(
			itemID,
			fieldRect.Shrink(itemRect.width + fieldPadding * 3, 0, 0, itemRect.height / 2),
			int.MinValue + 3,
			ARMOR_ICON, ARMOR_EMPTY_ICON,
			enableTint
		);

		// Bottom Line
		int lineSize = Unify(2);
		Renderer.Draw(
			Const.PIXEL,
			new IRect(
				rect.x + fieldPadding,
				rect.y - lineSize / 2,
				rect.width - fieldPadding, lineSize
			), Color32.GREY_32, int.MinValue + 2
		);

		// Flash
		if (
			EquipFlashType == type &&
			EquipFlashStartFrame >= 0 &&
			Game.GlobalFrame >= EquipFlashStartFrame &&
			Game.GlobalFrame < EquipFlashStartFrame + FLASH_PANEL_DURATION
		) {
			Renderer.Draw(
				Const.PIXEL, rect.Shrink(lineSize), new Color32(
					0, 255, 0,
					(byte)Util.RemapUnclamped(
						EquipFlashStartFrame, EquipFlashStartFrame + FLASH_PANEL_DURATION,
						255, 0,
						Game.GlobalFrame
					)
				), int.MinValue + 2
			);
		}

		if (mouseHovering) {
			int uiIndex = int.MinValue + index;
			if (MouseHoveringItemIndex != uiIndex) {
				MouseHoveringItemIndex = uiIndex;
				UsingMouseMode = true;
			}
		}

		if (interactable) {

			// Action
			if (highlighting && !CursorInBottomPanel) {
				// Action
				if (actionDown) {
					if (TakingID == 0) {
						TakeEquipment(type);
					} else {
						EquipTaking();
					}
				} else if (cancelDown) {
					if (TakingID == 0) {
						QuickDropFromEquipment(type);
					} else {
						ThrowTakingToGround();
					}
				}
				// Hints
				if (TakingID == 0) {
					if (itemID != 0) {
						ControlHintUI.AddHint(Gamekey.Action, HINT_TAKE, int.MinValue + 3);
					}
				} else {
					ControlHintUI.AddHint(Gamekey.Action, HINT_EQUIP, int.MinValue + 3);
					ControlHintUI.AddHint(Gamekey.Jump, HINT_THROW, int.MinValue + 3);
				}
				if (itemID != 0) {
					ControlHintUI.AddHint(Gamekey.Jump, HINT_TRANSFER, int.MinValue + 3);
				}
			}
		}

	}


	// Equipment Operation
	private void TakeEquipment (EquipmentType type) {
		if (TakingID != 0) return;
		if (!Player.Selecting.EquipmentAvailable(type)) return;
		int currentEquipmentID = Inventory.GetEquipment(Player.Selecting.TypeID, type);
		if (currentEquipmentID == 0) return;
		if (Inventory.SetEquipment(Player.Selecting.TypeID, type, 0)) {
			TakingID = currentEquipmentID;
			TakingCount = 1;
			TakingFromBottomPanel = false;
			TakingFromIndex = (int)type;
		}
	}


	private void QuickDropFromEquipment (EquipmentType type) {
		if (TakingID != 0) return;
		if (!Player.Selecting.EquipmentAvailable(type)) return;
		int currentEquipmentID = Inventory.GetEquipment(Player.Selecting.TypeID, type);
		if (currentEquipmentID == 0) return;
		int invID = Player.Selecting.TypeID;
		if (invID == 0) return;
		int collectCount = Inventory.CollectItem(invID, currentEquipmentID, out int collectedIndex, 1);
		if (collectCount > 0) {
			Inventory.SetEquipment(invID, type, 0);
			FlashInventoryField(collectedIndex, true);
		}
	}


	private void EquipAtCursor () {

		if (TakingID != 0 || !CursorInBottomPanel) return;
		int playerInvID = Player.Selecting.TypeID;
		int cursorItemCount = Inventory.GetInventoryCapacity(playerInvID);
		if (CursorIndex < 0 || CursorIndex >= cursorItemCount) return;

		int cursorID = Inventory.GetItemAt(playerInvID, CursorIndex);
		if (cursorID == 0) return;
		if (!ItemSystem.IsEquipment(cursorID, out var eqType)) return;
		if (!Player.Selecting.EquipmentAvailable(eqType)) return;

		int tookCount = Inventory.TakeItemAt(playerInvID, CursorIndex, 1);
		if (tookCount <= 0) return;

		int oldEquipmentID = Inventory.GetEquipment(playerInvID, eqType);
		if (Inventory.SetEquipment(playerInvID, eqType, cursorID)) {
			if (oldEquipmentID != 0) {
				if (Inventory.GetItemAt(playerInvID, CursorIndex) == 0) {
					// Back to Cursor
					Inventory.SetItemAt(playerInvID, CursorIndex, oldEquipmentID, 1);
					FlashInventoryField(CursorIndex, true);
				} else {
					// Collect
					int collectCount = Inventory.CollectItem(playerInvID, oldEquipmentID, out int collectIndex, 1);
					if (collectCount == 0) {
						ItemSystem.SpawnItemAtTarget(Player.Selecting, oldEquipmentID);
					} else {
						FlashInventoryField(collectIndex, true);
					}
				}
			}
			EquipFlashType = eqType;
			EquipFlashStartFrame = Game.GlobalFrame;
		}
	}


	private void EquipTaking () {
		if (TakingID == 0) return;
		if (!ItemSystem.IsEquipment(TakingID, out var type)) return;
		if (!Player.Selecting.EquipmentAvailable(type)) return;
		int currentEquipmentID = Inventory.GetEquipment(Player.Selecting.TypeID, type);
		if (Inventory.SetEquipment(Player.Selecting.TypeID, type, TakingID)) {
			TakingCount--;
			if (TakingCount > 0) AbandonTaking();
			TakingID = currentEquipmentID;
			TakingCount = 1;
			EquipFlashStartFrame = Game.GlobalFrame;
			EquipFlashType = type;
		}
	}


	// Inventory Operation
	private void TakeItemAtCursor () {
		if (TakingID != 0) return;
		int invID = CursorInBottomPanel ? Player.Selecting.TypeID : Partner != null ? Partner.InventoryID : 0;
		if (invID == 0) return;
		int itemCount = Inventory.GetInventoryCapacity(invID);
		if (CursorIndex < 0 || CursorIndex >= itemCount) return;
		int cursorItem = Inventory.GetItemAt(invID, CursorIndex, out int cursorCount);
		if (cursorItem == 0) return;
		TakingFromBottomPanel = CursorInBottomPanel;
		TakingFromIndex = CursorIndex;
		TakingID = cursorItem;
		TakingCount = cursorCount;
		Inventory.SetItemAt(invID, CursorIndex, 0, 0);
	}


	private void DropTakingToCursor () {
		if (TakingID == 0) return;
		int invID = CursorInBottomPanel ? Player.Selecting.TypeID : Partner != null ? Partner.InventoryID : 0;
		if (invID == 0) return;
		int cursorItemCount = Inventory.GetInventoryCapacity(invID);
		if (CursorIndex < 0 || CursorIndex >= cursorItemCount) return;
		int cursorID = Inventory.GetItemAt(invID, CursorIndex, out int cursorCount);
		if (cursorID == TakingID) {
			// Overlap
			int addedCount = Inventory.AddItemAt(invID, CursorIndex, TakingCount);
			TakingCount -= addedCount;
			if (TakingCount <= 0) {
				TakingID = 0;
			}
		} else if (cursorID != 0) {
			// Swap
			int takingID = TakingID;
			int takingCount = TakingCount;
			TakingID = cursorID;
			TakingCount = cursorCount;
			Inventory.SetItemAt(invID, CursorIndex, takingID, takingCount);
		} else {
			// Drop
			Inventory.SetItemAt(invID, CursorIndex, TakingID, TakingCount);
			TakingID = 0;
			TakingCount = 0;
		}
	}


	private void QuickDropAtCursor_FromInventory () {
		if (TakingID != 0) return;
		int partnerID = Partner != null ? Partner.InventoryID : 0;
		int invID = CursorInBottomPanel ? Player.Selecting.TypeID : partnerID;
		if (invID != 0) {
			int cursorItemCount = Inventory.GetInventoryCapacity(invID);
			if (CursorIndex < 0 || CursorIndex >= cursorItemCount) return;
			int cursorID = Inventory.GetItemAt(invID, CursorIndex, out int cursorCount);
			if (cursorID == 0) return;
			if (Partner != null) {
				// Quick Transfer
				int invIdAlt = CursorInBottomPanel ? partnerID : Player.Selecting.TypeID;
				if (invIdAlt != 0) {
					int collectCount = Inventory.CollectItem(invIdAlt, cursorID, out int collectedIndex, cursorCount);
					int newCount = cursorCount - collectCount;
					if (newCount != cursorCount) {
						Inventory.SetItemAt(invID, CursorIndex, cursorID, newCount);
						FlashInventoryField(collectedIndex, !CursorInBottomPanel);
					}
				}
			} else if (ItemSystem.IsEquipment(cursorID)) {
				// Equip
				EquipAtCursor();
			}
		}
	}


	private void UseAtCursor () {
		if (TakingID != 0) return;
		int partnerID = Partner != null ? Partner.InventoryID : 0;
		int invID = CursorInBottomPanel ? Player.Selecting.TypeID : partnerID;
		if (invID == 0) return;
		int cursorItemCount = Inventory.GetInventoryCapacity(invID);
		if (CursorIndex < 0 || CursorIndex >= cursorItemCount) return;
		int cursorID = Inventory.GetItemAt(invID, CursorIndex, out int cursorCount);
		if (cursorID == 0 || cursorCount == 0) return;
		if (ItemSystem.IsEquipment(cursorID)) return;
		var item = ItemSystem.GetItem(cursorID);
		if (item != null && item.Use(Player.Selecting)) {
			Inventory.TakeItemAt(invID, CursorIndex);
		}
	}


	private void SplitItemAtCursor () {
		if (TakingID != 0) return;
		int invID = CursorInBottomPanel ? Player.Selecting.TypeID : Partner != null ? Partner.InventoryID : 0;
		if (invID == 0) return;
		int cursorItemCount = Inventory.GetInventoryCapacity(invID);
		if (CursorIndex < 0 || CursorIndex >= cursorItemCount) return;
		int cursorID = Inventory.GetItemAt(invID, CursorIndex, out int cursorCount);
		if (cursorID == 0) return;
		if (cursorCount > 1) {
			int deltaCount = cursorCount / 2;
			TakingID = cursorID;
			TakingCount = deltaCount;
			Inventory.SetItemAt(invID, CursorIndex, cursorID, cursorCount - deltaCount);
		}
	}


	private void AbandonTaking () {

		if (TakingID == 0) return;

		// Collect
		int invID = TakingFromBottomPanel ? Player.Selecting.TypeID : Partner != null ? Partner.InventoryID : 0;
		if (invID != 0) {
			int itemCount = Inventory.GetInventoryCapacity(invID);
			if (TakingFromIndex >= 0 && TakingFromIndex < itemCount) {
				int collectCount = Inventory.CollectItem(invID, TakingID, TakingCount);
				TakingCount -= collectCount;
				if (TakingCount == 0) {
					TakingID = 0;
				}
			}
		}

		// Throw
		ThrowTakingToGround();
	}


	private void ThrowTakingToGround () {
		if (TakingID == 0 || TakingCount == 0) return;
		ItemSystem.SpawnItemAtTarget(Player.Selecting, TakingID, TakingCount);
		TakingID = 0;
		TakingCount = 0;
	}


	// Util
	private static void DrawItemIcon (IRect rect, int id, Color32 tint, int z) {
		if (id == 0) return;
		if (!Renderer.TryGetSprite(id, out var sprite)) {
			id = Const.PIXEL;
			Renderer.TryGetSprite(Const.PIXEL, out sprite);
			rect = rect.Shrink(rect.width / 6);
		}
		int iconShrink = Unify(7);
		Renderer.Draw(id, rect.Shrink(iconShrink).Fit(sprite), tint, z);
	}


	private void DrawItemCount (IRect rect, int number) {
		if (number <= 1) return;
		Renderer.Draw(Const.PIXEL, rect, Color32.BLACK, int.MaxValue);
		GUI.Label(rect, ItemCountChars.GetChars(number), GUISkin.CenterMiniLabel);
	}


	private int CursorWrap (int x, bool fromBottom) {
		int fromColumn = fromBottom ? Character.INVENTORY_COLUMN : TopPanelColumn;
		int toColumn = !fromBottom ? Character.INVENTORY_COLUMN : TopPanelColumn;
		if (fromColumn != toColumn && fromColumn != 0 && toColumn != 0) {
			x = x * toColumn / fromColumn;
		}
		return x.Clamp(0, toColumn - 1);
	}


	private void FlashInventoryField (int index, bool forBottom) {
		FlashingField.x = index;
		FlashingField.y = Game.GlobalFrame + FLASH_PANEL_DURATION;
		FlashingField.z = forBottom ? 0 : 1;
	}


	private IRect GetPanelRect (int column, int row, int itemSize, bool panelOnTop) {
		var player = Player.Selecting;
		int localAnimationFrame = Game.GlobalFrame - SpawnFrame;
		int uItemSize = Unify(itemSize);
		int invWidth = uItemSize * column;
		int invHeight = uItemSize * row;
		int invX = Player.Selecting.X;
		int invY = panelOnTop ?
			player.Y + Const.CEL * 2 + Const.HALF + Unify(WINDOW_PADDING) :
			player.Y - invHeight - Const.HALF - Unify(WINDOW_PADDING);
		if (localAnimationFrame < ANIMATION_DURATION) {
			float lerp01 = Ease.OutCirc((float)localAnimationFrame / ANIMATION_DURATION);
			invY += Util.LerpUnclamped(
				panelOnTop ? -Unify(86) : Unify(86), 0, lerp01
			).RoundToInt();
			invWidth -= Util.LerpUnclamped(uItemSize * 4, 0, lerp01).RoundToInt();
		}
		var result = new IRect(invX - invWidth / 2, invY, invWidth, invHeight);
		result.ClampPositionInside(Renderer.CameraRect);
		return result;
	}


	private IRect GetInventoryRect (int itemHeight) {
		var player = Player.Selecting;
		int localAnimationFrame = Game.GlobalFrame - SpawnFrame;
		int invWidth = Unify(400);
		int invHeight = itemHeight * 3;
		int invY = player.Y + Const.CEL * 2 + Const.HALF + Unify(WINDOW_PADDING);
		if (localAnimationFrame < ANIMATION_DURATION) {
			float lerp01 = Ease.OutCirc((float)localAnimationFrame / ANIMATION_DURATION);
			invY += Util.LerpUnclamped(-Unify(86), 0, lerp01).RoundToInt();
			invWidth -= Util.LerpUnclamped(Unify(128), 0, lerp01).RoundToInt();
		}
		var panelRect = new IRect(player.X - invWidth / 2 + Unify(PREVIEW_SIZE) / 2, invY, invWidth, invHeight);
		panelRect.ClampPositionInside(Renderer.CameraRect);
		return panelRect;
	}


	#endregion




}