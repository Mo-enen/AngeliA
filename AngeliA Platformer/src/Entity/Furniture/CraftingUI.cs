using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


public sealed class CraftingUI : PlayerMenuPartnerUI {


	// SUB
	private enum CraftActionType { None, TakeOne, QuickDropOne, TakeAll, QuickDropAll, }

	// Const
	private const int DOC_ITEM_HEIGHT = 22;
	private const int DOC_ITEM_PADDING = 4;
	private static readonly int QUESTION_MARK_CODE = BuiltInSprite.ICON_QUESTION_MARK;
	private static readonly int PLUS_CODE = BuiltInSprite.PLUS_16;
	private static readonly int EQUAL_CODE = BuiltInSprite.EQUAL_16;
	private static readonly int ARROW_CODE = BuiltInSprite.TRIANGLE_RIGHT_13;
	private static readonly int FRAME_CODE = BuiltInSprite.FRAME_16;
	private static readonly int ITEM_FRAME_CODE = BuiltInSprite.UI_ITEM_FRAME;
	private static readonly LanguageCode HINT_CRAFT = ("CtrlHint.Craft", "Craft");
	private static readonly LanguageCode MARK_KEEP = ("UI.Crafting.Keep", "Keep");

	// Api
	public override int ItemFieldSize => 96;
	public override int Column => _Column;
	public override int Row => _Row;
	public SpriteCode FrameCode { get; set; } = "CraftingTableFrame";

	// Data
	private readonly List<Int4> DocumentContent = [];
	private readonly int[] IgnoreConsumes = [0, 0, 0, 0,];
	private Int4 CurrentCraftingItems = default;
	private bool CursorInDoc = false;
	private bool CursorInResult = false;
	private int CombineResultID = 0;
	private int CombineResultCount = 0;
	private int DocumentScrollY = 0;
	private int DocumentPageSize = 1;
	private int _Column = 2;
	private int _Row = 2;


	// MSG
	public override void EnablePanel () {
		base.EnablePanel();
		DocumentContent.Clear();
		CurrentCraftingItems.x = int.MinValue;
	}


	public override void DrawPanel (IRect panelRect) {
		base.DrawPanel(panelRect);
		int sidePanelGapL = Unify(24);
		int sidePanelGapR = Unify(64);
		int docPanelWidth = Unify(196);
		int docPanelHeight = panelRect.height;
		int resultPanelSize = panelRect.height;
		var resultRect = new IRect(
			panelRect.xMax,
			panelRect.y,
			resultPanelSize + sidePanelGapR,
			panelRect.height
		);
		var resultItemRect = new IRect(
			resultRect.xMax - resultPanelSize,
			resultRect.y,
			resultPanelSize,
			resultPanelSize
		);
		var docRect = new IRect(
			panelRect.x - docPanelWidth - sidePanelGapL,
			panelRect.y,
			docPanelWidth + sidePanelGapL,
			docPanelHeight
		);
		var docItemRect = new IRect(
			panelRect.x - docPanelWidth - sidePanelGapL,
			panelRect.y,
			docPanelWidth,
			docPanelHeight
		);
		BackgroundRect = panelRect.Expand(docItemRect.width, resultPanelSize, 0, 0);
		Update_Cache();
		DocumentPageSize = panelRect.height.CeilDivide(Unify(DOC_ITEM_HEIGHT + DOC_ITEM_PADDING));
		var action = Update_Action(docItemRect, resultItemRect);
		MouseInPanel = MouseInPanel || new IRect(docRect.x, panelRect.y, resultRect.xMax - docRect.x, panelRect.height).MouseInside();
		Update_Inventory(panelRect);
		Update_Documentation(docRect, docItemRect);
		Update_Result(resultRect, resultItemRect);
		Craft(action);
	}


	private void Update_Cache () {

		// Inventory
		int invItem0 = Inventory.GetItemAt(InventoryID, 0);
		int invItem1 = Inventory.GetItemAt(InventoryID, 1);
		int invItem2 = Inventory.GetItemAt(InventoryID, 2);
		int invItem3 = Inventory.GetItemAt(InventoryID, 3);
		var crafting = ItemSystem.GetSortedCombination(invItem0, invItem1, invItem2, invItem3);
		if (crafting != CurrentCraftingItems) {
			CurrentCraftingItems = crafting;
			DocumentContent.Clear();
			ItemSystem.GetRelatedCombinations(crafting, DocumentContent, Column * Row);
		}

		// Craft Result
		if (!ItemSystem.TryGetCombination(
			invItem0, invItem1, invItem2, invItem3, out CombineResultID, out CombineResultCount,
			out IgnoreConsumes[0], out IgnoreConsumes[1], out IgnoreConsumes[2], out IgnoreConsumes[3]
		)) {
			CombineResultID = 0;
			CombineResultCount = 0;
			IgnoreConsumes[0] = IgnoreConsumes[1] = IgnoreConsumes[2] = IgnoreConsumes[3] = 0;
		}

	}


	private CraftActionType Update_Action (IRect docItemRect, IRect resultItemRect) {

		var action = CraftActionType.None;
		var menu = PlayerMenuUI.Instance;
		if (Input.LastActionFromMouse) {
			// Mouse
			action = Update_Action_Mouse(docItemRect, resultItemRect);
		} else if (!menu.CursorInBottomPanel) {
			// Result
			action = Update_Action_Key();
		}
		// Hint
		if (CombineResultID != 0 && CursorInResult) {
			string hint = HINT_CRAFT;
			ControlHintUI.AddHint(Gamekey.Action, hint, 0);
			ControlHintUI.AddHint(Gamekey.Jump, hint, 0);
		}
		return action;
	}


	private CraftActionType Update_Action_Mouse (IRect docItemRect, IRect resultItemRect) {

		var action = CraftActionType.None;
		bool craftAll = Input.HoldingShift;

		// Result
		CursorInResult = resultItemRect.MouseInside();
		if (CursorInResult && CombineResultID != 0) {
			if (Input.GameKeyDown(Gamekey.Action)) {
				Input.UseGameKey(Gamekey.Action);
				action = craftAll ? CraftActionType.TakeAll : CraftActionType.TakeOne;
			} else if (Input.GameKeyDown(Gamekey.Jump)) {
				Input.UseGameKey(Gamekey.Jump);
				action = craftAll ? CraftActionType.QuickDropAll : CraftActionType.QuickDropOne;
			}
		}

		// Doc
		CursorInDoc = docItemRect.MouseInside();
		if (CursorInDoc && Input.MouseWheelDelta != 0 && DocumentContent.Count > DocumentPageSize) {
			// Scroll Doc
			DocumentScrollY = (DocumentScrollY - Input.MouseWheelDelta).Clamp(
				0, (DocumentContent.Count - DocumentPageSize / 2).GreaterOrEquelThanZero()
			);
		}

		return action;
	}


	private CraftActionType Update_Action_Key () {

		bool craftAll = Input.HoldingShift;
		var menu = PlayerMenuUI.Instance;
		var action = CraftActionType.None;

		if (menu.TakingID != 0) {
			CursorInResult = false;
			CursorInDoc = false;
		}

		// Result
		if (CursorInResult) {

			if (Input.GameKeyDown(Gamekey.Action)) {
				// Action Key
				Input.UseGameKey(Gamekey.Action);
				if (CombineResultID != 0) {
					action = craftAll ? CraftActionType.TakeAll : CraftActionType.TakeOne;
				}
			} else if (Input.GameKeyDown(Gamekey.Jump)) {
				// Cancel Key
				Input.UseGameKey(Gamekey.Jump);
				if (CombineResultID != 0) {
					action = craftAll ? CraftActionType.QuickDropAll : CraftActionType.QuickDropOne;
				}
			}

			// Down Key
			if (Input.GameKeyDown(Gamekey.Down)) {
				Input.UseGameKey(Gamekey.Down);
				int x = (Character.INVENTORY_COLUMN - 1).Clamp(0, Character.INVENTORY_COLUMN - 1);
				int y = Character.INVENTORY_ROW - 1;
				menu.CursorIndex = x + Character.INVENTORY_COLUMN * y;
				menu.CursorInBottomPanel = true;
			}

			// Left Key
			if (Input.GameKeyDown(Gamekey.Left)) {
				Input.UseGameKey(Gamekey.Left);
				CursorInResult = false;
				menu.CursorIndex = Column - 1;
			}

		} else if (menu.CursorIndex % Column == Column - 1) {
			// Right Key >> Into Result Field
			if (Input.GameKeyDown(Gamekey.Right)) {
				Input.UseGameKey(Gamekey.Right);
				if (menu.TakingID == 0) CursorInResult = true;
			}
		}

		// Doc
		if (CursorInDoc) {

			// Right Key
			if (Input.GameKeyDown(Gamekey.Right)) {
				Input.UseGameKey(Gamekey.Right);
				CursorInDoc = false;
				menu.CursorIndex = 0;
			}

			// Down Key
			if (Input.GameKeyDown(Gamekey.Down)) {
				Input.UseGameKey(Gamekey.Down);
				DocumentScrollY = (DocumentScrollY + 4).Clamp(
					0, DocumentContent.Count - DocumentPageSize
				);
			}

			// Up Key
			if (Input.GameKeyDown(Gamekey.Up)) {
				Input.UseGameKey(Gamekey.Up);
				DocumentScrollY = (DocumentScrollY - 4).Clamp(
					0, DocumentContent.Count - DocumentPageSize
				);
			}
		} else if (menu.CursorIndex % Column == 0) {
			// Left Key >> Into Doc Field
			if (Input.GameKeyDown(Gamekey.Left)) {
				if (menu.TakingID == 0) CursorInDoc = true;
			}
		}

		return action;
	}


	private void Update_Inventory (IRect panelRect) {
		int itemSize = Unify(ItemFieldSize);
		var itemRect = new IRect(0, 0, itemSize, itemSize);
		int padding = Unify(12);
		int itemBorder = Unify(6);
		bool cursorInInventory = !CursorInResult && !CursorInDoc;
		int len = Column * Row;
		for (int i = 0; i < len; i++) {
			int itemID = Inventory.GetItemAt(InventoryID, i, out int count);
			itemRect.x = panelRect.x + (i % Column) * itemSize;
			itemRect.y = panelRect.y + (i / Column) * itemSize;
			Renderer.DrawSlice(
				FrameCode, itemRect.Shrink(itemSize / 32),
				itemBorder, itemBorder, itemBorder, itemBorder
			);
			PlayerMenuUI.DrawItemFieldUI(
				itemID, count, 0, itemRect.Shrink(padding),
				cursorInInventory,
				cursorInInventory ? i : -2
			);
			// No Consume Mark
			if (itemID != 0 && (IgnoreConsumes[0] == itemID || IgnoreConsumes[1] == itemID || IgnoreConsumes[2] == itemID || IgnoreConsumes[3] == itemID)) {
				GUI.BackgroundLabel(itemRect.EdgeDown(Unify(18)), MARK_KEEP, Color32.BLACK, Unify(6), style: GUI.Skin.SmallCenterLabel);
			}
		}
	}


	private void Update_Documentation (IRect docRect, IRect docItemRect) {

		var menu = PlayerMenuUI.Instance;

		// BG
		int bgPadding = Unify(6);
		Renderer.Draw(
			Const.PIXEL,
			docRect.Expand(bgPadding, 0, bgPadding, bgPadding), Color32.BLACK, int.MinValue + 1
		);

		// Highlight Frame
		if (CursorInDoc && !menu.CursorInBottomPanel && menu.TakingID == 0 && !Input.LastActionFromMouse) {
			GUI.HighlightCursor(FRAME_CODE, docItemRect);
		}

		// Content
		int itemHeight = Unify(DOC_ITEM_HEIGHT);
		int iconPadding = Unify(4);
		int linePadding = Unify(DOC_ITEM_PADDING);
		int startIndex = Renderer.GetUsedCellCount();
		var lineRect = new IRect(docItemRect.x, 0, docItemRect.width, itemHeight);
		DocumentScrollY = DocumentScrollY.Clamp(0, Util.Max(DocumentContent.Count - DocumentPageSize / 2, 0));
		int iconSize = lineRect.height;
		int tipID = 0;
		IRect tipRect = default;
		for (int i = DocumentScrollY; i < DocumentContent.Count; i++) {
			var com = DocumentContent[i];
			lineRect.y = docItemRect.yMax - (i + 1 - DocumentScrollY) * (lineRect.height + linePadding);
			if (lineRect.yMax < docRect.y) break;
			bool haveResult = ItemSystem.TryGetCombination(com.x, com.y, com.z, com.w, out int resultID, out _, out _, out _, out _, out _);
			if (!haveResult) continue;
			var iRect = new IRect(lineRect.xMax, lineRect.y, iconSize, iconSize);

			// Draw Result
			iRect.x -= iconSize;
			bool resultUnlocked = ItemSystem.IsItemUnlocked(resultID);
			if (resultUnlocked) {
				// Draw Item Icon
				if (ItemSystem.GetItem(resultID) is Item conItem) {
					conItem.DrawItem(iRect, Color32.WHITE, int.MinValue + 4);
				}
			} else {
				// Draw "?"
				Renderer.Draw(QUESTION_MARK_CODE, iRect, int.MinValue + 4);
			}
			if (resultUnlocked && Input.LastActionFromMouse && iRect.MouseInside()) {
				tipID = resultID;
				tipRect = iRect;
			}

			// Draw Equal Sign
			iRect.x -= iconSize / 2 + iconPadding;
			Renderer.Draw(EQUAL_CODE, new IRect(iRect.x, iRect.y + iconSize / 4, iconSize / 2, iconSize / 2), Color32.GREY_128, int.MinValue + 4);

			// Draw Combination 
			for (int j = 3; j >= 0; j--) {
				int id = com[j];
				if (id == 0) continue;
				bool unlocked = ItemSystem.IsItemUnlocked(id);
				// Icon
				iRect.x -= iconSize + iconPadding;
				if (unlocked) {
					// Draw Item Icon
					if (ItemSystem.GetItem(id) is Item conItem) {
						conItem.DrawItem(iRect, Color32.WHITE, int.MinValue + 4);
					}
				} else {
					// Draw "?"
					Renderer.Draw(QUESTION_MARK_CODE, iRect, int.MinValue + 4);
				}
				// Tip
				if (unlocked && Input.LastActionFromMouse && iRect.MouseInside()) {
					tipID = id;
					tipRect = iRect;
				}
				// Plus Sign
				if (j > 0) {
					iRect.x -= iconSize / 2 + iconPadding;
					Renderer.Draw(PLUS_CODE, new IRect(iRect.x, iRect.y + iconSize / 4, iconSize / 2, iconSize / 2), Color32.GREY_128, int.MinValue + 4);
				}
			}

		}

		// Clamp
		Renderer.ClampCells(docRect, startIndex);

		// Tip
		if (tipID != 0) {
			GUI.BackgroundLabel(
				new IRect(tipRect.x - tipRect.width * 2, tipRect.yMax + tipRect.height / 2, tipRect.width * 5, tipRect.height),
				ItemSystem.GetItemDisplayName(tipID),
				Color32.BLACK, Unify(6)
			);
		}

	}


	private void Update_Result (IRect resultPanelRect, IRect resultItemRect) {

		int ARROW_SIZE = Unify(64);
		var menu = PlayerMenuUI.Instance;

		// BG
		int bgPadding = Unify(6);
		Renderer.Draw(
			Const.PIXEL,
			resultPanelRect.Expand(0, bgPadding, bgPadding, bgPadding), Color32.BLACK, int.MinValue + 1
		);

		// Arrow
		Renderer.Draw(ARROW_CODE, new IRect(
			resultItemRect.x - ARROW_SIZE,
			resultItemRect.y + resultItemRect.height / 2 - ARROW_SIZE / 2,
			ARROW_SIZE, ARROW_SIZE
		), Color32.GREY_96, int.MinValue + 2);

		// Highlight Frame
		if (CursorInResult && menu.TakingID == 0) {
			if (Input.LastActionFromMouse) {
				Renderer.DrawPixel(resultItemRect, Color32.GREY_32, int.MinValue + 2);
			} else if (!menu.CursorInBottomPanel) {
				GUI.HighlightCursor(FRAME_CODE, resultItemRect);
			}
		}

		// Item Frame
		Renderer.DrawSlice(ITEM_FRAME_CODE, resultItemRect, Color32.WHITE, int.MinValue + 3);

		// Result Item
		if (CombineResultID != 0) {
			// Icon
			if (ItemSystem.GetItem(CombineResultID) is Item resultItem) {
				resultItem.DrawItem(resultItemRect.Shrink(Unify(12)), Color32.WHITE, int.MinValue + 4);
			}
			// Count
			if (CombineResultCount > 1) {
				int countSize = resultItemRect.width / 4;
				var countRect = new IRect(
					resultItemRect.xMax - countSize,
					resultItemRect.y,
					countSize, countSize
				);
				Renderer.DrawPixel(countRect, Color32.BLACK, int.MinValue + 8);
				GUI.IntLabel(countRect, CombineResultCount, GUI.Skin.CenterLabel);
			}
		}

	}


	private void Craft (CraftActionType action) {

		if (action == CraftActionType.None) return;

		var menu = PlayerMenuUI.Instance;
		if (CombineResultID == 0 || CombineResultCount == 0 || menu.TakingID != 0) return;

		bool consumeOne = action == CraftActionType.QuickDropOne || action == CraftActionType.TakeOne;
		int minMatCount = int.MaxValue;
		for (int i = 0; i < 4; i++) {
			int _count = Inventory.GetItemCount(InventoryID, i);
			if (_count > 0) {
				minMatCount = Util.Min(minMatCount, _count);
			}
		}
		if (minMatCount == int.MaxValue) return;

		int consumeMatCount = consumeOne ? 1 : minMatCount;
		int consumeResultCount = CombineResultCount * consumeMatCount;

		switch (action) {
			case CraftActionType.TakeOne:
			case CraftActionType.TakeAll:
				// Take Crafted
				menu.SetTaking(CombineResultID, consumeResultCount);
				ItemSystem.SetItemUnlocked(CombineResultID, true);
				break;
			case CraftActionType.QuickDropOne:
			case CraftActionType.QuickDropAll:
				// Quick Drop Crafted
				int invID = PlayerSystem.Selecting != null ? PlayerSystem.Selecting.InventoryID : 0;
				if (invID == 0) return;
				int collectedCount = Inventory.CollectItem(invID, CombineResultID, consumeResultCount);
				if (collectedCount < consumeResultCount) {
					Inventory.GiveItemToTarget(PlayerSystem.Selecting, CombineResultID, consumeResultCount - collectedCount);
					ItemSystem.SetItemUnlocked(CombineResultID, true);
				}
				break;
		}

		// Reduce Source Material by One
		for (int i = 0; i < 4; i++) {
			int itemID = Inventory.GetItemAt(InventoryID, i, out int count);
			if (itemID == 0 || count == 0) continue;
			if (IgnoreConsumes[0] == itemID || IgnoreConsumes[1] == itemID || IgnoreConsumes[2] == itemID || IgnoreConsumes[3] == itemID) continue;
			count = (count - consumeMatCount).GreaterOrEquelThanZero();
			if (count == 0) itemID = 0;
			Inventory.SetItemAt(InventoryID, i, itemID, count);
		}

	}


	// API
	public void SetColumnAndRow (int newColumn, int newRow) {
		_Column = newColumn;
		_Row = newRow;
	}


}