using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public sealed class CraftingTableUI : PlayerMenuPartnerUI {


		// SUB
		private enum ActionType { None, CombineOne, CombineAll, }

		private class DocComparer : IComparer<Int4> {
			public static readonly DocComparer Instance = new();
			public int Item0;
			public int Item1;
			public int Item2;
			public int Item3;
			public int Compare (Int4 a, Int4 b) {
				int countA = 0;
				int countB = 0;
				if (ContainItem(a.A)) countA++;
				if (ContainItem(a.B)) countA++;
				if (ContainItem(a.C)) countA++;
				if (ContainItem(a.D)) countA++;
				if (ContainItem(b.A)) countB++;
				if (ContainItem(b.B)) countB++;
				if (ContainItem(b.C)) countB++;
				if (ContainItem(b.D)) countB++;
				return countB.CompareTo(countA);
			}
			public bool ContainItem (int item) => item != 0 && (item == Item0 || item == Item1 || item == Item2 || item == Item3);
		}

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
		private static readonly int HINT_COMBINE = "CtrlHint.CombineItem".AngeHash();
		private static readonly int HINT_COMBINE_ALL = "CtrlHint.CombineAllItem".AngeHash();

		// Api
		public static readonly CraftingTableUI Instance = new();

		// Data
		private readonly List<Int4> DocumentContent = new();
		private Int4 CurrentCraftingItems = default;
		private bool CursorInDoc = false;
		private bool CursorInResult = false;
		private int CombineRepeat = 0;
		private int CombineResultID = 0;
		private int CombineResultCount = 0;
		private int DocumentScrollY = 0;
		private int DocumentPageSize = 1;


		// MSG
		public override void EnablePanel (int inventoryID, int column, int row, int itemSize = 64, bool centerPanel = false) {
			base.EnablePanel(inventoryID, column, row, itemSize, centerPanel);
			DocumentContent.Clear();
			CurrentCraftingItems.A = int.MinValue;
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
			switch (action) {
				case ActionType.CombineOne:
					QuickDropOneResult();
					break;
				case ActionType.CombineAll:
					TakeAllResults();
					break;
			}
		}


		private void Update_Cache () {

			// Inventory
			int invItem0 = Inventory.GetItemAt(InventoryID, 0);
			int invItem1 = Inventory.GetItemAt(InventoryID, 1);
			int invItem2 = Inventory.GetItemAt(InventoryID, 2);
			int invItem3 = Inventory.GetItemAt(InventoryID, 3);
			var inv = ItemSystem.GetSortedCombination(invItem0, invItem1, invItem2, invItem3);
			if (inv != CurrentCraftingItems) {
				CurrentCraftingItems = inv;
				InventoryChanged(inv);
			}

			// Result
			bool haveCombineResult = ItemSystem.TryGetCombination(
				invItem0, invItem1, invItem2, invItem3, out CombineResultID, out CombineResultCount
			);
			if (haveCombineResult) {
				CombineRepeat = 0;
				for (int i = 0; i < 4; i++) {
					int itemCount = Inventory.GetItemCount(InventoryID, i);
					if (itemCount == 0) continue;
					CombineRepeat = CombineRepeat == 0 ? itemCount : Mathf.Min(CombineRepeat, itemCount);
				}
			} else {
				CombineResultID = 0;
				CombineResultCount = 0;
				CombineRepeat = 0;
			}

		}


		private ActionType Update_Action (RectInt docItemRect, RectInt resultItemRect) {
			var action = ActionType.None;
			var menu = PlayerMenuUI.Instance;
			if (FrameInput.LastActionFromMouse) {
				// Result
				CursorInResult = resultItemRect.Contains(FrameInput.MouseGlobalPosition);
				if (CursorInResult && CombineResultID != 0) {
					if (FrameInput.MouseLeftButtonDown) {
						action = ActionType.CombineAll;
					} else if (FrameInput.MouseRightButtonDown) {
						action = ActionType.CombineOne;
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
							action = ActionType.CombineAll;
						}
					}
					if (FrameInput.GameKeyDown(Gamekey.Jump)) {
						FrameInput.UseGameKey(Gamekey.Jump);
						if (CombineResultID != 0) {
							action = ActionType.CombineOne;
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
				ControlHintUI.AddHint(Gamekey.Action, CombineRepeat > 1 ? Language.Get(HINT_COMBINE_ALL, "Combine All") : Language.Get(HINT_COMBINE, "Combine"), 0);
				ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_COMBINE, "Combine"), 0);
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
				bool haveResult = ItemSystem.TryGetCombination(com.A, com.B, com.C, com.D, out int result, out _);
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
				int repeatedCount = CombineResultCount * CombineRepeat;
				if (repeatedCount > 1) {
					int countSize = resultItemRect.width / 4;
					var countRect = new RectInt(
						resultItemRect.xMax - countSize,
						resultItemRect.y,
						countSize, countSize
					);
					CellRenderer.Draw(Const.PIXEL, countRect, Const.BLACK, int.MinValue + 8);
					string label = repeatedCount >= 0 && repeatedCount < CellRendererGUI.NUMBER_CACHE.Length ?
						CellRendererGUI.NUMBER_CACHE[repeatedCount] : "99+";
					CellRendererGUI.Label(CellContent.Get(label), countRect);
				}
			}

		}


		// Action
		private void TakeAllResults () {
			var menu = PlayerMenuUI.Instance;
			if (CombineResultID == 0 || CombineResultCount == 0 || menu.TakingID != 0) return;

			menu.SetTaking(CombineResultID, CombineResultCount * CombineRepeat);

			for (int i = 0; i < 4; i++) {
				int itemID = Inventory.GetItemAt(InventoryID, i, out int count);
				if (itemID == 0 || count == 0) continue;
				count = (count - CombineRepeat).GreaterOrEquelThanZero();
				if (count == 0) itemID = 0;
				Inventory.SetItemAt(InventoryID, i, itemID, count);
			}

		}


		private void QuickDropOneResult () {
			var menu = PlayerMenuUI.Instance;
			int playerID = Player.Selecting != null ? Player.Selecting.TypeID : 0;
			if (CombineResultID == 0 || CombineResultCount == 0 || menu.TakingID != 0 || playerID == 0) return;

			int collectedCount = Inventory.TryCollectItem(playerID, CombineResultID, CombineResultCount);
			if (collectedCount < CombineResultCount) {
				AngeUtil.ThrowItemToGround(CombineResultID, CombineResultCount - collectedCount);
			}

			for (int i = 0; i < 4; i++) {
				int itemID = Inventory.GetItemAt(InventoryID, i, out int count);
				if (itemID == 0 || count == 0) continue;
				count = (count - 1).GreaterOrEquelThanZero();
				if (count == 0) itemID = 0;
				Inventory.SetItemAt(InventoryID, i, itemID, count);
			}

		}


		// LGC
		private void InventoryChanged (Int4 invCombination) {

			int invItem0 = invCombination.A;
			int invItem1 = invCombination.B;
			int invItem2 = invCombination.C;
			int invItem3 = invCombination.D;
			bool checkForResult = invCombination.Count(0) == 3;

			// Refresh Doc Content
			DocumentContent.Clear();
			if (invItem0 != 0 || invItem1 != 0 || invItem2 != 0 || invItem3 != 0) {

				// Fill
				{
					FillRelatedCombinations(DocumentContent, invItem0, invCombination, checkForResult);
				}
				if (invItem1 != invItem0) {
					FillRelatedCombinations(DocumentContent, invItem1, invCombination, checkForResult);
				}
				if (invItem2 != invItem1 && invItem2 != invItem0) {
					FillRelatedCombinations(DocumentContent, invItem2, invCombination, checkForResult);
				}
				if (invItem3 != invItem2 && invItem3 != invItem1 && invItem3 != invItem0) {
					FillRelatedCombinations(DocumentContent, invItem3, invCombination, checkForResult);
				}

				// Sort
				DocComparer.Instance.Item0 = invItem0;
				DocComparer.Instance.Item1 = invItem1;
				DocComparer.Instance.Item2 = invItem2;
				DocComparer.Instance.Item3 = invItem3;
				DocumentContent.Sort(DocComparer.Instance);
				for (int i = 0; i < DocumentContent.Count - 1; i++) {
					if (DocumentContent[i] == DocumentContent[i + 1]) {
						DocumentContent.RemoveAt(i);
						i--;
					}
				}

				// Func
				static void FillRelatedCombinations (List<Int4> list, int itemID, Int4 comparing, bool checkForResult) {
					if (itemID == 0) return;
					var relateds = ItemSystem.GetAllRelatedCombinations(itemID);
					foreach (var com in relateds) {
						var _com = com;
						if (
							checkForResult &&
							ItemSystem.TryGetCombination(com.A, com.B, com.C, com.D, out int result, out _) &&
							comparing.Contains(result)
						) {
							list.Add(com);
							continue;
						}
						if (comparing.A != 0 && !_com.Swap(comparing.A, 0)) continue;
						if (comparing.B != 0 && !_com.Swap(comparing.B, 0)) continue;
						if (comparing.C != 0 && !_com.Swap(comparing.C, 0)) continue;
						if (comparing.D != 0 && !_com.Swap(comparing.D, 0)) continue;
						list.Add(com);
					}
				}
			}
		}


	}


	public class eCraftingTable : OpenableFurniture, IActionTarget {


		[AfterGameInitialize]
		public static void AfterGameInitialize () {
			int invID = typeof(eCraftingTable).AngeHash();
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
		}


		void IActionTarget.Invoke () {
			if (!Open) SetOpen(true);
			if (Player.Selecting == null) return;
			var playerMenu = PlayerMenuUI.OpenMenu();
			if (playerMenu == null) return;
			playerMenu.Partner = CraftingTableUI.Instance;
			playerMenu.Partner.EnablePanel(TypeID, 2, 2, 128, true);
		}


		protected override void SetOpen (bool open) {
			if (Open && !open) {
				PlayerMenuUI.CloseMenu();
			}
			base.SetOpen(open);
		}


	}
}