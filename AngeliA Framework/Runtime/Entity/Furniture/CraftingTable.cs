using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public sealed class CraftingTableUI : PlayerMenuPartnerUI {


		// SUB
		private enum CraftActionType { None, Take, QuickDrop, }

		// Const
		private const int DOC_ITEM_HEIGHT = 32;
		private const int DOC_ITEM_PADDING = 6;
		private static readonly int QUESTION_MARK_CODE = "QuestionMark16".AngeHash();
		private static readonly int PLUS_CODE = "Plus16".AngeHash();
		private static readonly int EQUAL_CODE = "Equal16".AngeHash();
		private static readonly int CRAFTING_FRAME_CODE = "CraftingTableFrame".AngeHash();
		private static readonly int ARROW_CODE = "TriangleRight13".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int ITEM_FRAME_CODE = "UI.ItemFrame".AngeHash();
		private static readonly int HINT_CRAFT = "CtrlHint.Craft".AngeHash();

		// Api
		public static readonly CraftingTableUI Instance = new();

		// Data
		private readonly List<Vector4Int> DocumentContent = new();
		private Vector4Int CurrentCraftingItems = default;
		private bool CursorInDoc = false;
		private bool CursorInResult = false;
		private int CombineResultID = 0;
		private int CombineResultCount = 0;
		private int DocumentScrollY = 0;
		private int DocumentPageSize = 1;


		// MSG
		public override void EnablePanel (int inventoryID, int column, int row, int itemSize = 52) {
			base.EnablePanel(inventoryID, column, row, itemSize);
			DocumentContent.Clear();
			CurrentCraftingItems.x = int.MinValue;
		}


		public override void DrawPanel (RectInt panelRect) {
			int sidePanelGap = Unify(64);
			int docPanelSize = panelRect.height;
			int resultPanelSize = panelRect.height;
			var resultRect = new RectInt(panelRect.xMax, panelRect.y, resultPanelSize + sidePanelGap, panelRect.height);
			var resultItemRect = new RectInt(resultRect.xMax - resultPanelSize, resultRect.y, resultPanelSize, resultPanelSize);
			var docRect = new RectInt(panelRect.x - docPanelSize - sidePanelGap, panelRect.y, docPanelSize + sidePanelGap, docPanelSize);
			var docItemRect = new RectInt(panelRect.x - docPanelSize - sidePanelGap, panelRect.y, docPanelSize, docPanelSize);
			Update_Cache();
			DocumentPageSize = panelRect.height / Unify(DOC_ITEM_HEIGHT + DOC_ITEM_PADDING);
			var action = Update_Action(docItemRect, resultItemRect);
			MouseInPanel = MouseInPanel || new RectInt(docRect.x, panelRect.y, resultRect.xMax - docRect.x, panelRect.height).Contains(FrameInput.MouseGlobalPosition);
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
				ItemSystem.FillAllRelatedCombinations(crafting, DocumentContent);
			}

			// Craft Result
			if (!ItemSystem.TryGetCombination(
				invItem0, invItem1, invItem2, invItem3, out CombineResultID, out CombineResultCount
			)) {
				CombineResultID = 0;
				CombineResultCount = 0;
			}

		}


		private CraftActionType Update_Action (RectInt docItemRect, RectInt resultItemRect) {
			var action = CraftActionType.None;
			var menu = PlayerMenuUI.Instance;
			if (FrameInput.LastActionFromMouse) {
				// Result
				CursorInResult = resultItemRect.Contains(FrameInput.MouseGlobalPosition);
				if (CursorInResult && CombineResultID != 0) {
					if (FrameInput.MouseLeftButtonDown) {
						action = CraftActionType.Take;
					} else if (FrameInput.MouseRightButtonDown) {
						action = CraftActionType.QuickDrop;
					}
				}
				// Doc
				CursorInDoc = docItemRect.Contains(FrameInput.MouseGlobalPosition);
				if (CursorInDoc && FrameInput.MouseWheelDelta != 0 && DocumentContent.Count > DocumentPageSize) {
					// Scroll Doc
					DocumentScrollY = (DocumentScrollY - FrameInput.MouseWheelDelta).Clamp(
						0, DocumentContent.Count - DocumentPageSize
					);
				}
			} else if (!menu.CursorInBottomPanel) {
				// Result
				if (menu.TakingID != 0) {
					CursorInResult = false;
					CursorInDoc = false;
				}
				if (CursorInResult) {
					if (FrameInput.GameKeyDown(Gamekey.Action)) {
						FrameInput.UseGameKey(Gamekey.Action);
						if (CombineResultID != 0) {
							action = CraftActionType.Take;
						}
					}
					if (FrameInput.GameKeyDown(Gamekey.Jump)) {
						FrameInput.UseGameKey(Gamekey.Jump);
						if (CombineResultID != 0) {
							action = CraftActionType.QuickDrop;
						}
					}
					if (FrameInput.GameKeyDown(Gamekey.Down)) {
						FrameInput.UseGameKey(Gamekey.Down);
						int x = (Player.INVENTORY_COLUMN - 1).Clamp(0, Player.INVENTORY_COLUMN - 1);
						int y = Player.INVENTORY_ROW - 1;
						menu.CursorIndex = x + Player.INVENTORY_COLUMN * y;
						menu.CursorInBottomPanel = true;
					}
					if (FrameInput.GameKeyDown(Gamekey.Left)) {
						FrameInput.UseGameKey(Gamekey.Left);
						CursorInResult = false;
						menu.CursorIndex = 1;
					}
				} else if (menu.CursorIndex % 2 == 1) {
					if (FrameInput.GameKeyDown(Gamekey.Right)) {
						FrameInput.UseGameKey(Gamekey.Right);
						if (menu.TakingID == 0) CursorInResult = true;
					}
				}
				// Doc
				if (CursorInDoc) {
					if (FrameInput.GameKeyDown(Gamekey.Right)) {
						FrameInput.UseGameKey(Gamekey.Right);
						CursorInDoc = false;
						menu.CursorIndex = 0;
					}
					if (FrameInput.GameKeyDown(Gamekey.Down)) {
						FrameInput.UseGameKey(Gamekey.Down);
						DocumentScrollY = (DocumentScrollY + 4).Clamp(
							0, DocumentContent.Count - DocumentPageSize
						);
					}
					if (FrameInput.GameKeyDown(Gamekey.Up)) {
						FrameInput.UseGameKey(Gamekey.Up);
						DocumentScrollY = (DocumentScrollY - 4).Clamp(
							0, DocumentContent.Count - DocumentPageSize
						);
					}
				} else if (menu.CursorIndex % 2 == 0) {
					if (FrameInput.GameKeyDown(Gamekey.Left)) {
						FrameInput.UseGameKey(Gamekey.Left);
						if (menu.TakingID == 0) CursorInDoc = true;
					}
				}
			}
			// Hint
			if (CombineResultID != 0 && CursorInResult) {
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_CRAFT, "Craft"), 0);
				ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_CRAFT, "Craft"), 0);
			}
			return action;
		}


		private void Update_Inventory (RectInt panelRect) {
			int itemSize = Unify(ItemSize);
			var itemRect = new RectInt(0, 0, itemSize, itemSize);
			int padding = Unify(12);
			int itemBorder = itemSize / 16;
			bool cursorInInventory = !CursorInResult && !CursorInDoc;
			for (int i = 0; i < 4; i++) {
				int itemID = Inventory.GetItemAt(InventoryID, i, out int count);
				itemRect.x = panelRect.x + (i % 2) * itemSize;
				itemRect.y = panelRect.y + (i / 2) * itemSize;
				PlayerMenuUI.DrawItemFieldUI(
					itemID, count, 0, itemRect.Shrink(padding),
					cursorInInventory,
					cursorInInventory ? i : -2
				);
				CellRenderer.Draw_9Slice(
					CRAFTING_FRAME_CODE, itemRect.Shrink(itemSize / 32),
					itemBorder, itemBorder, itemBorder, itemBorder,
					Const.WHITE, int.MinValue + 1
				);
			}
		}


		private void Update_Documentation (RectInt docRect, RectInt docItemRect) {

			var menu = PlayerMenuUI.Instance;

			// BG
			int bgPadding = Unify(12);
			CellRenderer.Draw(
				Const.PIXEL,
				docRect.Expand(bgPadding, 0, bgPadding, bgPadding),
				Const.BLACK, int.MinValue + 1
			);

			// Highlight Frame
			if (CursorInDoc && !menu.CursorInBottomPanel && menu.TakingID == 0 && !FrameInput.LastActionFromMouse) {
				CellRendererGUI.HighlightCursor(FRAME_CODE, docItemRect, int.MinValue + 6);
			}

			// Content
			int startIndex = CellRenderer.GetUsedCellCount();
			var lineRect = new RectInt(docItemRect.x, 0, docItemRect.width, Unify(DOC_ITEM_HEIGHT));
			DocumentScrollY = DocumentScrollY.Clamp(0, DocumentContent.Count);
			int iconPadding = Unify(4);
			int linePadding = Unify(DOC_ITEM_PADDING);
			int iconSize = lineRect.height;
			int tipID = 0;
			RectInt tipRect = default;
			for (int i = DocumentScrollY; i < DocumentContent.Count; i++) {
				var com = DocumentContent[i];
				lineRect.y = docItemRect.yMax - (i + 1 - DocumentScrollY) * (lineRect.height + linePadding);
				if (lineRect.yMax < docRect.y) break;
				bool haveResult = ItemSystem.TryGetCombination(com.x, com.y, com.z, com.w, out int result, out _);
				if (!haveResult) continue;
				var iRect = new RectInt(lineRect.xMax, lineRect.y, iconSize, iconSize);

				// Draw Result
				iRect.x -= iconSize;
				bool resultUnlocked = ItemSystem.IsItemUnlocked(result);
				CellRenderer.Draw(resultUnlocked ? result : QUESTION_MARK_CODE, iRect, int.MinValue + 4);
				if (resultUnlocked && FrameInput.LastActionFromMouse && iRect.Contains(FrameInput.MouseGlobalPosition)) {
					tipID = result;
					tipRect = iRect;
				}

				// Draw Equal Sign
				iRect.x -= iconSize / 2 + iconPadding;
				CellRenderer.Draw(EQUAL_CODE, new RectInt(iRect.x, iRect.y + iconSize / 4, iconSize / 2, iconSize / 2), Const.GREY_128, int.MinValue + 4);

				// Draw Combination 
				for (int j = 3; j >= 0; j--) {
					int id = com[j];
					if (id == 0) continue;
					bool unlocked = ItemSystem.IsItemUnlocked(id);
					// Icon
					iRect.x -= iconSize + iconPadding;
					CellRenderer.Draw(unlocked ? id : QUESTION_MARK_CODE, iRect, int.MinValue + 4);
					// Tip
					if (unlocked && FrameInput.LastActionFromMouse && iRect.Contains(FrameInput.MouseGlobalPosition)) {
						tipID = id;
						tipRect = iRect;
					}
					// Plus Sign
					if (j > 0) {
						iRect.x -= iconSize / 2 + iconPadding;
						CellRenderer.Draw(PLUS_CODE, new RectInt(iRect.x, iRect.y + iconSize / 4, iconSize / 2, iconSize / 2), Const.GREY_128, int.MinValue + 4);
					}
				}

			}

			// Clamp
			int endIndex = CellRenderer.GetUsedCellCount();
			CellRenderer.ClampCells(docRect, startIndex, endIndex);

			// Tip
			if (tipID != 0) {
				CellRendererGUI.Label(
					CellContent.Get(ItemSystem.GetItemName(tipID), alignment: Alignment.BottomMid),
					new RectInt(tipRect.x - tipRect.width * 2, tipRect.yMax + tipRect.height / 2, tipRect.width * 5, tipRect.height),
					out var tipBounds
				);
				CellRenderer.Draw(Const.PIXEL, tipBounds, Const.BLACK, int.MaxValue);
			}

		}


		private void Update_Result (RectInt resultPanelRect, RectInt resultItemRect) {

			int ARROW_SIZE = Unify(64);
			var menu = PlayerMenuUI.Instance;

			// BG
			int bgPadding = Unify(12);
			CellRenderer.Draw(
				Const.PIXEL,
				resultPanelRect.Expand(0, bgPadding, bgPadding, bgPadding),
				Const.BLACK, int.MinValue + 1
			);

			// Arrow
			CellRenderer.Draw(ARROW_CODE, new RectInt(
				resultItemRect.x - ARROW_SIZE,
				resultItemRect.y + resultItemRect.height / 2 - ARROW_SIZE / 2,
				ARROW_SIZE, ARROW_SIZE
			), Const.GREY_96, int.MinValue + 2);

			// Highlight Frame
			if (CursorInResult && menu.TakingID == 0) {
				if (FrameInput.LastActionFromMouse) {
					CellRenderer.Draw(Const.PIXEL, resultItemRect, Const.GREY_32, int.MinValue + 2);
				} else if (!menu.CursorInBottomPanel) {
					CellRendererGUI.HighlightCursor(FRAME_CODE, resultItemRect, int.MinValue + 6);
				}
			}

			// Item Frame
			CellRenderer.Draw_9Slice(ITEM_FRAME_CODE, resultItemRect, Const.WHITE, int.MinValue + 3);

			// Item
			if (CombineResultID != 0) {
				CellRenderer.Draw(CombineResultID, resultItemRect.Shrink(Unify(12)), int.MinValue + 4);
				if (CombineResultCount > 1) {
					int countSize = resultItemRect.width / 4;
					var countRect = new RectInt(
						resultItemRect.xMax - countSize,
						resultItemRect.y,
						countSize, countSize
					);
					CellRenderer.Draw(Const.PIXEL, countRect, Const.BLACK, int.MinValue + 8);
					CellRendererGUI.Label(CellContent.Get(CellRendererGUI.GetNumberCache(CombineResultCount)), countRect);
				}
			}

		}


		// Action
		private void Craft (CraftActionType action) {

			if (action == CraftActionType.None) return;

			var menu = PlayerMenuUI.Instance;
			if (CombineResultID == 0 || CombineResultCount == 0 || menu.TakingID != 0) return;

			if (action == CraftActionType.Take) {
				// Take Crafted
				menu.SetTaking(CombineResultID, CombineResultCount);
			} else {
				// Quick Drop Crafted
				int playerID = Player.Selecting != null ? Player.Selecting.TypeID : 0;
				if (playerID == 0) return;
				int collectedCount = Inventory.CollectItem(playerID, CombineResultID, CombineResultCount);
				if (collectedCount < CombineResultCount) {
					ItemSystem.ItemSpawnItemAtPlayer(CombineResultID, CombineResultCount - collectedCount);
				}
			}

			// Reduce Source Material by One
			for (int i = 0; i < 4; i++) {
				int itemID = Inventory.GetItemAt(InventoryID, i, out int count);
				if (itemID == 0 || count == 0) continue;
				count = (count - 1).GreaterOrEquelThanZero();
				if (count == 0) itemID = 0;
				Inventory.SetItemAt(InventoryID, i, itemID, count);
			}

		}


	}


	public abstract class CraftingTable : OpenableFurniture, IActionTarget {


		[OnGameInitialize(64)]
		public static void AfterGameInitialize () {
			int invID = typeof(CraftingTable).AngeHash();
			const int TARGET_COUNT = 4;
			if (Inventory.HasInventory(invID)) {
				int iCount = Inventory.GetInventoryCapacity(invID);
				if (iCount != TARGET_COUNT) {
					// Resize
					Inventory.ResizeItems(invID, TARGET_COUNT);
				}
			} else {
				// Create New Items
				Inventory.AddNewInventoryData(invID, TARGET_COUNT);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// UI Close Check
			if (Open && !PlayerMenuUI.ShowingUI) {
				SetOpen(false);
			}
			// Draw Items
			if (CellRenderer.TryGetSprite(TypeID, out var sprite)) {
				var itemRect = Rect;
				for (int i = 0; i < 4; i++) {
					int id = Inventory.GetItemAt(TypeID, i);
					if (id == 0) continue;
					CellRenderer.Draw(
						id, new RectInt(
							itemRect.x + (i % 2) * itemRect.width / 2,
							itemRect.y + (i / 2) * itemRect.height / 2,
							itemRect.width / 2,
							itemRect.height / 2
						).Shrink(itemRect.width / 16),
						sprite.SortingZ + 1
					);
				}
			}
		}


		void IActionTarget.Invoke () {
			if (!Open) SetOpen(true);
			if (Player.Selecting == null) return;
			var playerMenu = PlayerMenuUI.OpenMenu();
			if (playerMenu == null) return;
			playerMenu.Partner = CraftingTableUI.Instance;
			playerMenu.Partner.EnablePanel(TypeID, 2, 2, 128);
		}


		protected override void SetOpen (bool open) {
			if (Open && !open) PlayerMenuUI.CloseMenu();
			base.SetOpen(open);
		}


	}
}