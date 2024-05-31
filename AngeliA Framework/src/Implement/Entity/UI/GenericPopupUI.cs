using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.StageOrder(4096)]
public class GenericPopupUI : EntityUI, IWindowEntityUI {




	#region --- SUB ---


	private class Item {
		public bool IsSeparator => string.IsNullOrEmpty(Label);
		public string Label;
		public int Icon;
		public int Mark;
		public Direction2 IconPosition;
		public bool Checked;
		public bool Enabled;
		public bool IsSubMenu;
		public bool Visible;
		public int Level;
		public object Data;
		public System.Action Action;
	}


	#endregion




	#region --- VAR ---


	// Api
	public static GenericPopupUI Instance { get; private set; }
	public static bool ShowingPopup => Instance != null && Instance.Active;
	public static int CurrentItemCount => Instance != null ? Instance.ItemCount : 0;
	protected override bool BlockEvent => true;
	public IRect BackgroundRect { get; private set; }
	public int OffsetX { get; set; } = 0;
	public int OffsetY { get; set; } = 0;
	public object InvokingItemData { get; private set; }
	public int MenuID { get; private set; } = 0;
	public int CurrentSubLevel { get; set; } = 0;

	// Data
	private readonly Item[] Items = new Item[128];
	private int ItemCount = 0;
	private int HoveringIndex = -1;
	private int HoveringFrame = 0;
	private int VisibleDepth = 0;


	#endregion




	#region --- MSG ---


	public GenericPopupUI () {
		Instance = this;
		for (int i = 0; i < Items.Length; i++) {
			Items[i] = new();
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		ItemCount = 0;
		Input.UseMouseKey(0);
		Input.UseMouseKey(1);
		Input.UseMouseKey(2);
	}


	public override void OnInactivated () {
		base.OnInactivated();
		ClearItems();
	}


	public override void UpdateUI () {
		base.UpdateUI();

		if (ItemCount == 0) {
			Active = false;
			return;
		}

		Cursor.RequireCursor();


		int panelWidth = Unify(200);
		int itemHeight = Unify(26);
		int lineThickness = Unify(4);
		int panelPadding = Unify(4);
		var cameraRect = Renderer.CameraRect;
		bool anyItemHovering = false;
		bool ignoreClose = false;
		int panelTopOffsetY = cameraRect.y + OffsetY;
		var panelRect = new IRect(
			cameraRect.x + OffsetX,
			0,
			panelWidth, 1
		);

		// For all Panels
		for (int dep = 0; dep <= VisibleDepth; dep++) {

			// Count Items/Separators
			int separatorCount = 0;
			int itemCount = 0;
			for (int i = 0; i < ItemCount; i++) {
				var item = Items[i];
				if (!item.Visible || item.Level != dep) continue;
				if (item.IsSeparator) {
					separatorCount++;
				} else {
					itemCount++;
				}
			}

			// Get Panel Rect
			panelRect.height = itemHeight * itemCount + lineThickness * separatorCount;
			panelRect.y = panelTopOffsetY - panelRect.height;
			if (dep > 0) {
				panelRect.x += panelRect.width + panelPadding * 2;
			}
			panelRect.width = panelWidth;

			if (dep == 0 && cameraRect.y + OffsetY < cameraRect.y + panelRect.height) {
				panelRect.y += panelRect.height;
			}
			panelRect.ClampPositionInside(cameraRect);

			// BG
			var bgCell = Renderer.DrawPixel(default, new Color32(249, 249, 249, 255));

			// Items
			Cell highlightCell = null;
			Cell menuHeadCell = null;
			int textStart = Renderer.GetUsedCellCount();
			int indent = Unify(36);
			var rect = new IRect(panelRect.x, panelRect.yMax, panelRect.width, itemHeight);
			int checkShrink = itemHeight / 6;
			int maxWidth = panelWidth;
			int iconPadding = Unify(4);
			bool mouseLeftDown = Input.MouseLeftButtonDown;
			for (int i = 0; i < ItemCount; i++) {

				var item = Items[i];
				if (!item.Visible || item.Level != dep) continue;

				bool isSeparator = item.IsSeparator;
				bool isOpeningMenuHead = false;
				rect.y -= isSeparator ? lineThickness : itemHeight;

				// Menu Top Check
				var nextItem = i + 1 < ItemCount ? Items[i + 1] : null;
				if (nextItem != null && item.IsSubMenu && nextItem.Visible && nextItem.Level == dep + 1) {
					panelTopOffsetY = rect.yMax;
					isOpeningMenuHead = true;
				}

				if (isSeparator) {
					// Separator
					Renderer.Draw(
						BuiltInSprite.SOFT_LINE_H,
						new IRect(rect.x, rect.y + lineThickness / 4, rect.width, lineThickness / 2),
						Color32.BLACK_32
					);
				} else {
					// Item 

					bool hover = rect.MouseInside();

					// Hover
					if (hover) {
						HoveringFrame = i != HoveringIndex ? 0 : (HoveringFrame + 1);
						HoveringIndex = i;
						anyItemHovering = true;
						if (HoveringFrame == 30 || mouseLeftDown) {
							HoveringFrame = 31;
							panelTopOffsetY = rect.yMax;
							// Refresh Visible
							if (Items[i].IsSubMenu && i + 1 < ItemCount) {
								HideAll();
								MakeVisible(i + 1);
							} else {
								HideAll();
								MakeVisible(i);
							}
						}
					}

					// Draw Highlight
					if (hover && item.Enabled) {
						highlightCell = Renderer.DrawPixel(rect, Color32.GREY_230);
					} else if (isOpeningMenuHead) {
						menuHeadCell = Renderer.DrawPixel(rect, new Color32(238, 238, 238, 255));
					}

					// Menu Mark
					if (item.IsSubMenu) {
						Renderer.Draw(
							BuiltInSprite.ICON_TRIANGLE_RIGHT,
							rect.CornerInside(Alignment.MidRight, rect.height / 2),
							Color32.GREY_64
						);
					}

					// Content
					using (Scope.GUIEnable(item.Enabled)) {

						// Check Mark
						if (item.Checked) {
							Renderer.Draw(
								item.Mark != 0 ? item.Mark : BuiltInSprite.CHECK_MARK_32,
								new IRect(rect.x, rect.y, rect.height, rect.height).Shrink(checkShrink),
								item.Enabled ? Color32.BLACK : Color32.BLACK_128
							);
						}

						// Label
						GUI.Label(rect.Shrink(indent, 0, 0, 0), item.Label, out var labelBounds, GUI.Skin.SmallDarkLabel);
						maxWidth = Util.Max(
							maxWidth,
							labelBounds.width + indent * 4 / 3 + (item.Icon != 0 ? iconPadding + rect.height : 0)
						);

						// Icon
						if (item.Icon != 0) {
							int iconSize = rect.height;
							var iconRect = new IRect(
								item.IconPosition == Direction2.Left ? labelBounds.x - iconSize - iconPadding : labelBounds.xMax + iconPadding,
								rect.y, iconSize, iconSize
							);
							Renderer.Draw(item.Icon, iconRect);
						}

						// Hover
						if (hover && item.Enabled && mouseLeftDown) {
							if (!item.IsSubMenu) {
								// Click Item
								InvokingItemData = item.Data;
								item.Action?.Invoke();
								InvokingItemData = null;
							} else {
								// Click Sub Menu
								ignoreClose = true;
							}
						}

					}
				}

			}

			panelRect.width = Util.Max(panelRect.width, maxWidth);
			if (highlightCell != null) highlightCell.Width = panelRect.width;
			if (menuHeadCell != null) menuHeadCell.Width = panelRect.width;
			
			// BG
			BackgroundRect = panelRect.Expand(panelPadding);
			bgCell.X = BackgroundRect.x;
			bgCell.Y = BackgroundRect.y;
			bgCell.Width = BackgroundRect.width;
			bgCell.Height = BackgroundRect.height;

			// Clamp
			Renderer.ClampCells(panelRect, textStart);

			// Line Left
			Renderer.Draw(
				BuiltInSprite.SOFT_LINE_V,
				BackgroundRect.EdgeInside(Direction4.Left, lineThickness / 2),
				Color32.GREY_196
			);

		}

		// No Hover
		if (ItemCount > 0 && !anyItemHovering) {
			HoveringFrame = HoveringIndex != -1 ? 0 : (HoveringFrame + 1);
			HoveringIndex = -1;
			if (HoveringFrame == 48) {
				// Refresh Visible
				HideAll();
				MakeVisible(0);
			}
		}

		// Block Input
		Input.IgnoreMouseInput();

		// Cancel
		if (!ignoreClose && Game.GlobalFrame > SpawnFrame && (Input.MouseLeftButtonDown || Input.MouseRightButtonDown || Input.MouseMidButtonDown || Input.AnyKeyDown)) {
			Input.UseMouseKey(0);
			Input.UseMouseKey(1);
			Input.UseMouseKey(2);
			Active = false;
		}

		X = BackgroundRect.x;
		Y = BackgroundRect.y;
		Width = BackgroundRect.width;
		Height = BackgroundRect.height;

	}


	#endregion




	#region --- API ---


	public static void BeginPopup (int menuID = 0) => BeginPopup(Input.UnshiftedMouseGlobalPosition, menuID);
	public static void BeginPopup (Int2 globalOffset, int menuID = 0) => Instance?.BeginPopupLogic(globalOffset, menuID);
	private void BeginPopupLogic (Int2 globalOffset, int menuID = 0) {
		if (Stage.Enable) {
			Stage.SpawnEntity(TypeID, 0, 0);
		} else {
			Active = true;
			SpawnFrame = Game.GlobalFrame;
			OnActivated();
		}
		ClearItems();
		InvokingItemData = null;
		ItemCount = 0;
		OffsetX = globalOffset.x - Renderer.CameraRect.x;
		OffsetY = globalOffset.y - Renderer.CameraRect.y;
		MenuID = menuID;
		CurrentSubLevel = 0;
		HoveringFrame = 0;
		VisibleDepth = 0;
	}


	public static void AddSeparator () => AddItem("", Const.EmptyMethod, true, false);


	public static void AddItem (string label, System.Action action, bool enabled = true, bool @checked = false, object data = null) => AddItem(label, 0, default, 0, action, enabled, @checked, data);


	public static void AddItem (string label, int icon, Direction2 iconPosition, int checkMark, System.Action action, bool enabled = true, bool @checked = false, object data = null) {
		if (Instance == null || Instance.ItemCount >= Instance.Items.Length - 1) return;
		int level = Instance.CurrentSubLevel;
		var item = Instance.Items[Instance.ItemCount];
		item.Label = label;
		item.Icon = icon;
		item.IconPosition = iconPosition;
		item.Action = action;
		item.Enabled = enabled;
		item.Checked = @checked;
		item.Mark = checkMark;
		item.Level = level;
		item.IsSubMenu = false;
		item.Data = data;
		item.Visible = level == 0;
		Instance.ItemCount++;
	}


	public static void BeginSubItem () {
		Instance.CurrentSubLevel++;
		int last = Instance.ItemCount - 1;
		if (last >= 0 && last < Instance.Items.Length) {
			Instance.Items[last].IsSubMenu = true;
		}
	}


	public static void EndSubItem () => Instance.CurrentSubLevel = (Instance.CurrentSubLevel - 1).GreaterOrEquelThanZero();


	public static void ClosePopup () {
		if (Instance != null) Instance.Active = false;
	}


	public static void ClearItems () {
		if (Instance == null) return;
		for (int i = 0; i < Instance.ItemCount; i++) {
			var item = Instance.Items[i];
			item.Label = "";
			item.Action = null;
		}
	}


	#endregion




	#region --- LGC ---


	private void HideAll () {
		foreach (var item in Items) {
			item.Visible = false;
		}
		VisibleDepth = 0;
	}


	private void MakeVisible (int pointIndex) {

		// Set Visible
		int currentLevel = Items[pointIndex].Level;
		VisibleDepth = Util.Max(VisibleDepth, currentLevel);

		// Get Left
		int left = pointIndex;
		for (int i = pointIndex; i >= 0; i--) {
			int level = Items[i].Level;
			if (level == currentLevel) {
				left = i;
				Items[i].Visible = true;
			} else if (level < currentLevel) break;
		}

		// Get Right
		for (int i = pointIndex; i < ItemCount; i++) {
			int level = Items[i].Level;
			if (level == currentLevel) {
				Items[i].Visible = true;
			} else if (level < currentLevel) break;
		}

		if (left - 1 >= 0) {
			MakeVisible(left - 1);
		}

	}


	#endregion




}