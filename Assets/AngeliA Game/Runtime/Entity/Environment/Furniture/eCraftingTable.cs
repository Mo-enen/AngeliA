using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public sealed class CraftingTableUI : PlayerMenuPartnerUI {


		// SUB
		private enum ActionType { None, CombineOne, CombineAll, OpenDoc, }

		// Const
		private static readonly int CRAFTING_FRAME_CODE = "CraftingTableFrame".AngeHash();
		private static readonly int ARROW_CODE = "TriangleRight13".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int ITEM_FRAME_CODE = "UI.ItemFrame".AngeHash();
		private static readonly int HINT_COMBINE = "CtrlHint.CombineItem".AngeHash();
		private static readonly int HINT_COMBINE_ALL = "CtrlHint.CombineAllItem".AngeHash();

		// Api
		public static readonly CraftingTableUI Instance = new();

		// Data
		private bool CursorInDoc = false;
		private bool CursorInResult = false;
		private int CombineRepeat = 0;
		private int CombineResultID = 0;
		private int CombineResultCount = 0;


		// MSG
		public override void DrawPanel (RectInt panelRect) {
			int sidePanelGap = CellRendererGUI.Unify(64);
			int docPanelSize = panelRect.height;
			int resultPanelSize = panelRect.height;
			var resultRect = new RectInt(panelRect.xMax, panelRect.y, resultPanelSize + sidePanelGap, panelRect.height);
			var resultItemRect = new RectInt(resultRect.xMax - resultPanelSize, resultRect.y, resultPanelSize, resultPanelSize);
			var docRect = new RectInt(panelRect.x - docPanelSize - sidePanelGap, panelRect.y, docPanelSize + sidePanelGap, docPanelSize);
			var docItemRect = new RectInt(panelRect.x - docPanelSize - sidePanelGap, panelRect.y, docPanelSize, docPanelSize);
			Update_Cache();
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
				case ActionType.OpenDoc:
					OpenDocumentation();
					break;
			}
		}


		private void Update_Cache () {
			bool haveCombineResult = ItemSystem.TryGetCombination(
				Inventory.GetItemAt(InventoryID, 0), Inventory.GetItemAt(InventoryID, 1),
				Inventory.GetItemAt(InventoryID, 2), Inventory.GetItemAt(InventoryID, 3),
				out CombineResultID, out CombineResultCount
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
				if (CursorInDoc && FrameInput.MouseLeftButtonDown) {
					action = ActionType.OpenDoc;
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
					if (FrameInput.GameKeyDown(Gamekey.Action)) {
						FrameInput.UseGameKey(Gamekey.Action);
						action = ActionType.OpenDoc;
					}
					if (FrameInput.GameKeyDown(Gamekey.Right)) {
						FrameInput.UseGameKey(Gamekey.Right);
						CursorInDoc = false;
						menu.CursorIndex = 0;
					}
					if (FrameInput.GameKeyDown(Gamekey.Down)) {
						FrameInput.UseGameKey(Gamekey.Down);
						menu.CursorIndex = 0;
						menu.CursorInBottomPanel = true;
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
			int itemSize = CellRendererGUI.Unify(ItemSize);
			var itemRect = new RectInt(0, 0, itemSize, itemSize);
			int padding = CellRendererGUI.Unify(12);
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
			int bgPadding = CellRendererGUI.Unify(12);
			CellRenderer.Draw(
				Const.PIXEL,
				docRect.Expand(bgPadding, 0, bgPadding, bgPadding),
				Const.BLACK, int.MinValue + 1
			);

			// Highlight Frame
			if (CursorInDoc && menu.TakingID == 0) {
				if (FrameInput.LastActionFromMouse) {
					CellRenderer.Draw(Const.PIXEL, docItemRect, Const.GREY_32, int.MinValue + 2);
				} else if (!menu.CursorInBottomPanel) {
					CellRendererGUI.HighlightCursor(FRAME_CODE, docItemRect, int.MinValue + 6);
				}
			}






		}


		private void Update_Result (RectInt resultPanelRect, RectInt resultItemRect) {

			int ARROW_SIZE = CellRendererGUI.Unify(64);
			var menu = PlayerMenuUI.Instance;

			// BG
			int bgPadding = CellRendererGUI.Unify(12);
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
				CellRenderer.Draw(CombineResultID, resultItemRect.Shrink(CellRendererGUI.Unify(12)), int.MinValue + 4);
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


		private void OpenDocumentation () {



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
			playerMenu.Partner.InventoryID = TypeID;
			playerMenu.Partner.Column = 2;
			playerMenu.Partner.Row = 2;
			playerMenu.Partner.ItemSize = 128;
			playerMenu.Partner.CenterPanel = true;
		}


		protected override void SetOpen (bool open) {
			if (Open && !open) {
				PlayerMenuUI.CloseMenu();
			}
			base.SetOpen(open);
		}


	}
}