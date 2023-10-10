using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public sealed class InventoryPartnerUI : PlayerMenuPartnerUI {
		public static readonly InventoryPartnerUI Instance = new();
		public int AvatarIcon = 0;
		public override void DrawPanel (RectInt panelRect) {
			PlayerMenuUI.DrawTopInventory(InventoryID, Column, Row);
			// Icon
			if (AvatarIcon != 0) {
				int ICON_SIZE = Unify(96);
				int INFO_WIDTH = Unify(PlayerMenuUI.INFO_WIDTH);
				int PADDING = Unify(12);
				if (CellRenderer.TryGetSpriteFromGroup(AvatarIcon, 0, out var sprite, false, true)) {
					CellRenderer.Draw(sprite.GlobalID, new RectInt(
						panelRect.xMax + (PADDING + INFO_WIDTH - ICON_SIZE) / 2,
						panelRect.y + PADDING,
						ICON_SIZE, ICON_SIZE
					), int.MinValue + 16);
				}
			}
		}
	}


	public abstract class PlayerMenuPartnerUI {

		public int InventoryID { get; private set; } = 0;
		public int Column { get; private set; } = 1;
		public int Row { get; private set; } = 1;
		public int ItemSize { get; private set; } = PlayerMenuUI.ITEM_SIZE;
		public bool MouseInPanel { get; set; } = false;

		public virtual void EnablePanel (int inventoryID, int column, int row, int itemSize = PlayerMenuUI.ITEM_SIZE) {
			InventoryID = inventoryID;
			Column = column;
			Row = row;
			ItemSize = itemSize;
		}

		public abstract void DrawPanel (RectInt panelRect);

		protected static int Unify (int value) => CellRendererGUI.Unify(value);

	}


	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public class PlayerMenuUI : EntityUI {




		#region --- VAR ---


		// Const
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int ITEM_FRAME_CODE = "UI.ItemFrame".AngeHash();
		private static readonly int HINT_HIDE_MENU = "CtrlHint.HideMenu".AngeHash();
		private static readonly int HINT_TAKE = "CtrlHint.PlayerMenu.Take".AngeHash();
		private static readonly int HINT_DROP = "CtrlHint.Drop".AngeHash();
		private static readonly int HINT_THROW = "CtrlHint.Throw".AngeHash();
		private static readonly int HINT_HUSE = "CtrlHint.HoldToUse".AngeHash();
		private static readonly int HINT_TRANSFER = "CtrlHint.Transfer".AngeHash();
		private static readonly int HINT_EQUIP = "CtrlHint.Equip".AngeHash();
		private static readonly int HINT_MOVE = "CtrlHint.Move".AngeHash();
		private static readonly int UI_HELMET = "UI.Equipment.Helmet".AngeHash();
		private static readonly int UI_WEAPON = "UI.Equipment.Weapon".AngeHash();
		private static readonly int UI_SHOES = "UI.Equipment.Shoes".AngeHash();
		private static readonly int UI_GLOVES = "UI.Equipment.Gloves".AngeHash();
		private static readonly int UI_BODYSUIT = "UI.Equipment.Bodysuit".AngeHash();
		private static readonly int UI_JEWELRY = "UI.Equipment.Jewelry".AngeHash();
		private const int WINDOW_PADDING = 12;
		private const int HOLD_KEY_DURATION = 26;
		private const int ANIMATION_DURATION = 12;
		private const int FLASH_PANEL_DURATION = 52;
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
		private int TakingFromIndex = 0;
		private int ActionKeyDownFrame = int.MinValue;
		private int CancelKeyDownFrame = int.MinValue;
		private int HoveringItemID = 0;
		private bool TakingFromBottomPanel = false;
		private bool MouseInPanel = false;
		private bool RenderingBottomPanel = false;
		private bool HoveringItemField = false;
		private int EquipFlashStartFrame = int.MinValue;
		private int ItemInfoScrollPosition = 0;
		private int PrevCursorIndex = -1;
		private bool PrevCursorInBottomPanel = true;
		private EquipmentType EquipFlashType = EquipmentType.BodySuit;
		private RectInt TopPanelRect = default;
		private RectInt BottomPanelRect = default;
		private Vector3Int FlashingField = new(-1, 0, 0);


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
			FlashingField = new(-1, 0, 0);
			X = CellRenderer.CameraRect.CenterX();
			Y = CellRenderer.CameraRect.CenterY();
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
			CursorSystem.RequireCursor();

			Update_PanelUI();
			Update_ItemInfoUI();
			Update_MoveCursor();
			Update_Actions();
			DrawTakingItemMouseCursor();

			Update_CloseMenu();

			if (CursorIndex != PrevCursorIndex || CursorInBottomPanel != PrevCursorInBottomPanel) {
				PrevCursorIndex = CursorIndex;
				PrevCursorInBottomPanel = CursorInBottomPanel;
				ItemInfoScrollPosition = 0;
				FrameInput.UseGameKey(Gamekey.Action);
				FrameInput.UseGameKey(Gamekey.Jump);
			}
		}


		private void Update_PanelUI () {

			HoveringItemField = false;
			MouseInPanel = false;
			HoveringItemID = 0;

			// Bottom Panel
			RenderingBottomPanel = true;
			var playerPanelRect = GetMenuPanelRect(Player.INVENTORY_COLUMN, Player.INVENTORY_ROW, ITEM_SIZE, false);
			CellRenderer.Draw(Const.PIXEL, playerPanelRect.Expand(Unify(WINDOW_PADDING)), Const.BLACK, int.MinValue + 1);
			DrawInventory(Player.Selecting.TypeID, Player.INVENTORY_COLUMN, Player.INVENTORY_ROW, false);

			// Top Panel
			RenderingBottomPanel = false;
			if (Partner != null) {
				// Partner Panel
				var panelRect = GetMenuPanelRect(Partner.Column, Partner.Row, Partner.ItemSize, true);
				CellRenderer.Draw(Const.PIXEL, panelRect.Expand(Unify(WINDOW_PADDING)), Const.BLACK, int.MinValue + 1);
				Partner.MouseInPanel = panelRect.Contains(FrameInput.MouseGlobalPosition);
				Partner.DrawPanel(panelRect);
				TopPanelRect = panelRect;
				MouseInPanel = MouseInPanel || Partner.MouseInPanel;
			} else {
				// Equipment Panel
				DrawEquipmentUI();
			}

			if (!HoveringItemField) {
				if (FrameInput.MouseLeftButtonDown) {
					FrameInput.UseGameKey(Gamekey.Action);
				}
				if (FrameInput.MouseRightButtonDown) {
					FrameInput.UseGameKey(Gamekey.Jump);
				}
			}

		}


		private void Update_ItemInfoUI () {

			int panelWidth = Unify(INFO_WIDTH);
			int windowPadding = Unify(WINDOW_PADDING);
			var topRootRect = TopPanelRect;
			var bottomRootRect = BottomPanelRect;
			var topPanelRect = new RectInt(
				topRootRect.xMax + windowPadding * 2, topRootRect.y,
				panelWidth, topRootRect.height
			);
			var bottomPanelRect = new RectInt(
				bottomRootRect.xMax + windowPadding * 2, bottomRootRect.y,
				panelWidth, bottomRootRect.height
			);
			bool drawTopUI = Partner == null || Partner is InventoryPartnerUI;

			// Background
			var topWindowRect = topPanelRect.Expand(windowPadding);
			var bottomWindowRect = bottomPanelRect.Expand(windowPadding);
			if (drawTopUI) {
				CellRenderer.Draw(
					Const.PIXEL, topWindowRect, Const.BLACK, int.MinValue + 1
				);
			}
			CellRenderer.Draw(
				Const.PIXEL, bottomWindowRect, Const.BLACK, int.MinValue + 1
			);

			// Mouse in Panel
			MouseInPanel = MouseInPanel || topWindowRect.Contains(FrameInput.MouseGlobalPosition) || bottomWindowRect.Contains(FrameInput.MouseGlobalPosition);

			// Content
			int itemID = HoveringItemID;
			if (itemID == 0) return;
			if (!drawTopUI && !CursorInBottomPanel) return;

			var panelRect = CursorInBottomPanel ? bottomPanelRect : topPanelRect;
			int startIndex = CellRenderer.GetTextUsedCellCount();
			int labelHeight = Unify(24);

			// Type Icon
			CellRenderer.Draw(
				AngeUtil.GetItemTypeIcon(itemID),
				new RectInt(panelRect.x, panelRect.yMax - labelHeight, labelHeight, labelHeight),
				Const.ORANGE, int.MinValue + 3
			);

			// Name
			CellRendererGUI.Label(
				CellContent.Get(ItemSystem.GetItemName(itemID), charSize: 20, alignment: Alignment.MidLeft, tint: Const.ORANGE),
				new RectInt(panelRect.x + labelHeight + labelHeight / 4, panelRect.yMax - labelHeight, panelRect.width, labelHeight)
			);

			// Description
			CellRendererGUI.ScrollLabel(
				CellContent.Get(
					ItemSystem.GetItemDescription(itemID),
					charSize: 18,
					alignment: Alignment.TopLeft,
					wrap: true
				),
				panelRect.Shrink(0, 0, 0, labelHeight + Unify(6)),
				ref ItemInfoScrollPosition
			);
			if (FrameInput.MouseWheelDelta != 0) {
				ItemInfoScrollPosition -= FrameInput.MouseWheelDelta * labelHeight;
			}

			// Clamp
			int endIndex = CellRenderer.GetTextUsedCellCount();
			CellRenderer.ClampTextCells(panelRect, startIndex, endIndex);

		}


		private void Update_CloseMenu () {

			// Mouse
			if (
				!MouseInPanel &&
				Game.GlobalFrame != SpawnFrame &&
				FrameInput.AnyMouseButtonDown
			) {
				if (TakingID == 0) {
					Active = false;
				} else {
					ThrowTakingToGround();
				}
			}

			// Key
			if (FrameInput.GameKeyUp(Gamekey.Select) || FrameInput.GameKeyUp(Gamekey.Start)) {
				FrameInput.UseGameKey(Gamekey.Select);
				FrameInput.UseGameKey(Gamekey.Start);
				Active = false;
				return;
			}

			// Hint
			ControlHintUI.AddHint(Gamekey.Select, Language.Get(HINT_HIDE_MENU, "Hide Menu"));
		}


		private void Update_MoveCursor () {

			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
			ControlHintUI.AddHint(Gamekey.Action, "", int.MinValue + 2);

			if (FrameInput.DirectionX == Direction3.None && FrameInput.DirectionY == Direction3.None) return;

			int column = CursorInBottomPanel ? Player.INVENTORY_COLUMN : TopPanelColumn;
			int row = CursorInBottomPanel ? Player.INVENTORY_ROW : TopPanelRow;
			int x = CursorIndex % column;
			int y = CursorIndex / column;

			// Left
			if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
				x = Mathf.Max(x - 1, 0);
			}

			// Right
			if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
				x = Mathf.Min(x + 1, column - 1);
			}

			// Down
			if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
				if (!CursorInBottomPanel && y == 0) {
					CursorInBottomPanel = true;
					if (Partner == null) {
						x = x == 0 ? 0 : Player.INVENTORY_COLUMN / 2;
					} else {
						x = CursorWrap(x, false);
					}
					y = Player.INVENTORY_ROW - 1;
					column = Player.INVENTORY_COLUMN;
					row = Player.INVENTORY_ROW;
				} else {
					y = Mathf.Max(y - 1, 0);
				}
			}

			// Up
			if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
				if (CursorInBottomPanel && y == row - 1) {
					CursorInBottomPanel = false;
					if (Partner == null) {
						x = x < Player.INVENTORY_COLUMN / 2 ? 0 : 1;
					} else {
						x = CursorWrap(x, true);
					}
					y = 0;
					column = TopPanelColumn;
					row = TopPanelRow;
				} else {
					y = Mathf.Min(y + 1, row - 1);
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
			bool actionDown = FrameInput.GameKeyDown(Gamekey.Action);
			bool cancelDown = !actionDown && FrameInput.GameKeyDown(Gamekey.Jump);
			bool actionUp = FrameInput.GameKeyUp(Gamekey.Action);
			bool actionHolding = FrameInput.GameKeyHolding(Gamekey.Action);
			bool cancelHolding = !actionHolding && FrameInput.GameKeyHolding(Gamekey.Jump);

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
			if (intendedHoldAction) FrameInput.UseGameKey(Gamekey.Action);
			if (intendedHoldCancel) FrameInput.UseGameKey(Gamekey.Jump);

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
						ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_TAKE, "Take"), int.MinValue + 3);
						if (Partner != null) {
							// Has Partner
							if (Partner.InventoryID != 0) {
								ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_TRANSFER, "Transfer"), int.MinValue + 3);
							}
						} else if (cursoringEquip) {
							// Equip
							ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_EQUIP, "Equip"), int.MinValue + 3);
						} else {
							// Use
							if (ItemSystem.CanUseItem(cursorID, Player.Selecting)) {
								ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_HUSE, "Hold to Use"), int.MinValue + 3);
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
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_DROP, "Drop"), int.MinValue + 3);
				ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_THROW, "Throw"), int.MinValue + 3);
			}

			CursorIndex = oldCursorIndex;

		}


		private void DrawTakingItemMouseCursor () {
			if (!FrameInput.LastActionFromMouse || TakingID == 0) return;
			int x = FrameInput.MouseGlobalPosition.x;
			int y = FrameInput.MouseGlobalPosition.y;
			int size = Unify(ITEM_SIZE);
			CellRenderer.Draw(
				CellRenderer.HasSprite(TakingID) ? TakingID : Const.PIXEL,
				x, y, 500, 500, Game.GlobalFrame.PingPong(30) - 15,
				size, size, Const.WHITE, int.MaxValue
			);
			DrawItemCount(new RectInt(x, y - size / 2, size / 2, size / 2), TakingCount);
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


		public static void DrawItemFieldUI (int itemID, int itemCount, int frameCode, RectInt itemRect, bool interactable, int uiIndex) => Instance?.DrawItemField(itemID, itemCount, frameCode, itemRect, interactable, uiIndex);


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
			var panelRect = GetMenuPanelRect(column, row, ITEM_SIZE, panelOnTop);
			if (panelOnTop) {
				TopPanelRect = panelRect;
			} else {
				BottomPanelRect = panelRect;
			}

			var windowRect = panelRect.Expand(Unify(WINDOW_PADDING));
			MouseInPanel = MouseInPanel || windowRect.Contains(FrameInput.MouseGlobalPosition);

			// Content
			int index = 0;
			var itemRect = new RectInt(0, 0, panelRect.width / column, panelRect.height / row);
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


		public void DrawItemField (int itemID, int itemCount, int frameCode, RectInt itemRect, bool interactable, int uiIndex) {

			if (itemCount <= 0) itemID = 0;
			bool actionHolding = FrameInput.GameKeyHolding(Gamekey.Action);
			bool cancelHolding = FrameInput.GameKeyHolding(Gamekey.Jump);
			bool hovering = interactable && itemRect.Contains(FrameInput.MouseGlobalPosition);
			int cursorIndex = RenderingBottomPanel == CursorInBottomPanel ? CursorIndex : -1;
			HoveringItemField = HoveringItemField || hovering;

			// Frame
			int frameBorder = Unify(4);
			CellRenderer.Draw_9Slice(
				frameCode, itemRect,
				frameBorder, frameBorder, frameBorder, frameBorder,
				Const.WHITE, int.MinValue + 3
			);

			// Icon
			DrawItemIcon(itemRect, itemID, Const.WHITE, int.MinValue + 4);

			// Count
			if (TakingID == 0 || cursorIndex != uiIndex) {
				DrawItemCount(itemRect.Shrink(itemRect.width * 2 / 3, 0, 0, itemRect.height * 2 / 3), itemCount);
			}

			// Hover
			if (hovering) {
				if (FrameInput.LastActionFromMouse) {
					HoveringItemID = itemID;
					// Move UI Cursor from Mouse
					cursorIndex = CursorIndex = uiIndex;
					CursorInBottomPanel = RenderingBottomPanel;
					// Draw Highlight
					CellRenderer.Draw(Const.PIXEL, itemRect, Const.GREY_42, int.MinValue + 2);
				}
				if (itemID != 0) {
					// System Mouse Cursor
					CursorSystem.SetCursorAsHand();
				}
			}

			// UI Cursor
			if (!FrameInput.LastActionFromMouse && cursorIndex == uiIndex) {
				HoveringItemID = itemID;
				// Cursor
				CellRendererGUI.HighlightCursor(FRAME_CODE, itemRect, int.MinValue + 4);
				// Taking Item
				if (TakingID != 0) {
					CellRenderer.Draw(
						CellRenderer.HasSprite(TakingID) ? TakingID : Const.PIXEL,
						itemRect.x + itemRect.width / 2,
						itemRect.y + itemRect.height / 2,
						500, 500, Game.GlobalFrame.PingPong(30) - 15,
						itemRect.width * 3 / 2, itemRect.height * 3 / 2,
						Const.WHITE, int.MaxValue - 1
					);
					int _size = itemRect.width / 2;
					DrawItemCount(new RectInt(itemRect.xMax - _size, itemRect.y, _size, _size), TakingCount);
				}
			}

			// Flashing
			if (
				Game.GlobalFrame < FlashingField.y &&
				FlashingField.z == 0 == RenderingBottomPanel &&
				FlashingField.x >= 0 &&
				FlashingField.x == uiIndex
			) {
				var tint = Const.GREEN;
				tint.a = (byte)Util.RemapUnclamped(FLASH_PANEL_DURATION, 0, 255, 0, FlashingField.y - Game.GlobalFrame);
				CellRenderer.Draw(Const.PIXEL, itemRect, tint, int.MinValue + 3);
			}

			// Holding
			if (itemID != 0 && cursorIndex == uiIndex) {
				if (
					itemCount > 1 &&
					actionHolding && TakingID == 0 &&
					ActionKeyDownFrame >= 0 &&
					Game.GlobalFrame >= ActionKeyDownFrame + 6
				) {
					var cell = CellRenderer.Draw(Const.PIXEL, itemRect, Const.GREY_96, int.MinValue + 3);
					cell.Shift = new Vector4Int(
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
						var cell = CellRenderer.Draw(Const.PIXEL, itemRect, Const.GREEN, int.MinValue + 3);
						cell.Shift = new Vector4Int(
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

			int localAnimationFrame = Game.GlobalFrame - SpawnFrame;
			bool interactable = localAnimationFrame > ANIMATION_DURATION;
			var player = Player.Selecting;

			// Panel Rect
			int invWidth = Unify(400);
			int itemHeight = Unify(64);
			int invHeight = itemHeight * 3;
			int invY = player.Y + Const.CEL * 2 + Const.HALF + Unify(WINDOW_PADDING);
			if (localAnimationFrame < ANIMATION_DURATION) {
				float lerp01 = Ease.OutCirc((float)localAnimationFrame / ANIMATION_DURATION);
				invY += Mathf.LerpUnclamped(-Unify(86), 0, lerp01).RoundToInt();
				invWidth -= Mathf.LerpUnclamped(Unify(128), 0, lerp01).RoundToInt();
			}
			var panelRect = new RectInt(
				player.X - (invWidth + Unify(INFO_WIDTH)) / 2,
				invY, invWidth, invHeight
			);
			panelRect.ClampPositionInside(CellRenderer.CameraRect);
			TopPanelRect = panelRect;

			// Background
			var windowRect = panelRect.Expand(Unify(WINDOW_PADDING));
			CellRenderer.Draw(Const.PIXEL, windowRect, Const.BLACK, int.MinValue + 1);
			MouseInPanel = MouseInPanel || windowRect.Contains(FrameInput.MouseGlobalPosition);

			// Background Content
			CellRenderer.Draw(Const.PIXEL, windowRect, Const.BLACK, int.MinValue + 1);

			// Content
			int width = panelRect.width / 2;
			int left = panelRect.x;
			int top = panelRect.yMax;
			DrawEquipmentItem(
				0, interactable && player.JewelryAvailable, new RectInt(left, top - itemHeight * 3, width, itemHeight),
				EquipmentType.Jewelry, Language.Get(UI_JEWELRY, "Jewelry")
			);
			DrawEquipmentItem(
				1, interactable && player.ShoesAvailable, new RectInt(left + width, top - itemHeight * 3, width, itemHeight),
				EquipmentType.Shoes, Language.Get(UI_SHOES, "Boots")
			);
			DrawEquipmentItem(
				2, interactable && player.GlovesAvailable, new RectInt(left, top - itemHeight * 2, width, itemHeight),
				EquipmentType.Gloves, Language.Get(UI_GLOVES, "Gloves")
			);
			DrawEquipmentItem(
				3, interactable && player.BodySuitAvailable, new RectInt(left + width, top - itemHeight * 2, width, itemHeight),
				EquipmentType.BodySuit, Language.Get(UI_BODYSUIT, "Armor")
			);
			DrawEquipmentItem(
				4, interactable && player.WeaponAvailable, new RectInt(left, top - itemHeight * 1, width, itemHeight),
				EquipmentType.Weapon, Language.Get(UI_WEAPON, "Weapon")
			);
			DrawEquipmentItem(
				5, interactable && player.HelmetAvailable, new RectInt(left + width, top - itemHeight * 1, width, itemHeight),
				EquipmentType.Helmet, Language.Get(UI_HELMET, "Helmet")
			);

		}


		private void DrawEquipmentItem (int index, bool interactable, RectInt rect, EquipmentType type, string label) {

			int itemID = Inventory.GetEquipment(Player.Selecting.TypeID, type);
			int fieldPadding = Unify(4);
			var fieldRect = rect.Shrink(fieldPadding);
			bool actionDown = interactable && FrameInput.GameKeyDown(Gamekey.Action);
			bool cancelDown = interactable && FrameInput.GameKeyDown(Gamekey.Jump);
			var enableTint = Const.WHITE;
			var equipAvailable = Player.Selecting.EquipmentAvailable(type);
			if (TakingID != 0 && ItemSystem.IsEquipment(TakingID, out var eqType)) {
				if (type != eqType) {
					enableTint.a = 96;
				}
			}
			var itemRect = new RectInt(fieldRect.x, fieldRect.y, fieldRect.height, fieldRect.height);

			// Item Frame
			if (equipAvailable) {
				int border = Unify(4);
				CellRenderer.Draw_9Slice(ITEM_FRAME_CODE, itemRect, border, border, border, border, enableTint, int.MinValue + 2);
			}

			// Icon
			if (!equipAvailable) enableTint.a = 96;
			DrawItemIcon(itemRect, itemID, enableTint, int.MinValue + 3);

			// Label
			CellRendererGUI.Label(
				CellContent.Get(label, enableTint, 24, Alignment.MidLeft),
				fieldRect.Shrink(itemRect.width + fieldPadding * 2, 0, 0, 0)
			);

			// Bottom Line
			int lineSize = Unify(2);
			CellRenderer.Draw(
				Const.PIXEL,
				new RectInt(
					rect.x + fieldPadding,
					rect.y - lineSize / 2,
					rect.width - fieldPadding, lineSize
				), Const.GREY_32, int.MinValue + 2
			);

			// Flash
			if (
				EquipFlashType == type &&
				EquipFlashStartFrame >= 0 &&
				Game.GlobalFrame >= EquipFlashStartFrame &&
				Game.GlobalFrame < EquipFlashStartFrame + FLASH_PANEL_DURATION
			) {
				CellRenderer.Draw(
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

			bool highlighting = false;

			// Highlight
			if (FrameInput.LastActionFromMouse) {
				if (interactable && rect.Contains(FrameInput.MouseGlobalPosition)) {
					CursorIndex = index;
					CursorInBottomPanel = false;
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, int.MinValue + 1);
					highlighting = true;
				}
			} else {
				if (CursorIndex == index && !CursorInBottomPanel) {
					highlighting = true;
					var cursorTint = interactable ? Const.GREEN : Const.GREY_96;
					cursorTint.a = enableTint.a;
					CellRendererGUI.HighlightCursor(FRAME_CODE, rect, int.MinValue + 4, cursorTint);
					// Taking Item
					if (TakingID != 0) {
						CellRenderer.Draw(
							CellRenderer.HasSprite(TakingID) ? TakingID : Const.PIXEL,
							itemRect.x + itemRect.width / 2,
							itemRect.y + itemRect.height / 2,
							500, 500, Game.GlobalFrame.PingPong(30) - 15,
							itemRect.width * 3 / 2,
							itemRect.height * 3 / 2,
							Const.WHITE, int.MaxValue
						);
					}
				}
			}
			if (highlighting) HoveringItemID = itemID;

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
							ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_TAKE, "Take"), int.MinValue + 3);
						}
					} else {
						ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_EQUIP, "Equip"), int.MinValue + 3);
						ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_THROW, "Throw"), int.MinValue + 3);
					}
					if (itemID != 0) {
						ControlHintUI.AddHint(Gamekey.Jump, Language.Get(HINT_TRANSFER, "Transfer"), int.MinValue + 3);
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
							ItemSystem.ItemSpawnItemAtPlayer(oldEquipmentID);
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
			ItemSystem.ItemSpawnItemAtPlayer(TakingID, TakingCount);
			TakingID = 0;
			TakingCount = 0;
		}


		// Util
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


		private void DrawItemCount (RectInt rect, int number) {
			if (number <= 1) return;
			CellRenderer.Draw(Const.PIXEL, rect, Const.BLACK, int.MaxValue);
			CellRendererGUI.Label(CellContent.Get(CellRendererGUI.GetNumberCache(number), Const.WHITE), rect);
		}


		private RectInt GetMenuPanelRect (int column, int row, int itemSize, bool panelOnTop) {
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
				invY += Mathf.LerpUnclamped(
					panelOnTop ? -Unify(86) : Unify(86), 0, lerp01
				).RoundToInt();
				invWidth -= Mathf.LerpUnclamped(uItemSize * 4, 0, lerp01).RoundToInt();
			}
			int infoOffsetX = !panelOnTop || Partner == null || Partner is InventoryPartnerUI ? Unify(INFO_WIDTH) : 0;
			var result = new RectInt(invX - (invWidth + infoOffsetX) / 2, invY, invWidth, invHeight);
			result.ClampPositionInside(CellRenderer.CameraRect);
			return result;
		}


		private int CursorWrap (int x, bool fromBottom) {
			int fromColumn = fromBottom ? Player.INVENTORY_COLUMN : TopPanelColumn;
			int toColumn = !fromBottom ? Player.INVENTORY_COLUMN : TopPanelColumn;
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


		#endregion




	}
}