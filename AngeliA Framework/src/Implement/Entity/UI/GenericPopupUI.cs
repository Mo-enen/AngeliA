using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.StageOrder(4096)]
public class GenericPopupUI : EntityUI, IWindowEntityUI {




	#region --- SUB ---


	private class Item {
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


	// Const
	private static readonly int CHECK_CODE = BuiltInSprite.CHECK_MARK_32;
	private static readonly int LINE_CODE = BuiltInSprite.SOFT_LINE_H;

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
	private int SeparatorCount = 0;
	private int HoveringIndex = -1;
	private int HoveringFrame = 0;


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

		int separatorCount = 0;
		for (int i = 0; i < ItemCount; i++) {
			if (string.IsNullOrEmpty(Items[i].Label)) separatorCount++;
		}
		int panelWidth = Unify(200);
		int itemHeight = Unify(26);
		int separatorHeight = Unify(4);
		var cameraRect = Renderer.CameraRect;
		var panelRect = new IRect(
			cameraRect.x + OffsetX,
			cameraRect.y + OffsetY,
			panelWidth,
			itemHeight * (ItemCount - separatorCount) + separatorHeight * separatorCount
		);
		int uiHeight = (itemHeight * (ItemCount - SeparatorCount)) + (separatorHeight * SeparatorCount);
		if (cameraRect.y + OffsetY > cameraRect.y + uiHeight) {
			panelRect.y -= uiHeight;
		}
		if (panelRect.height < cameraRect.height) {
			panelRect.ClampPositionInside(cameraRect);
		} else {
			int padding = Unify(64);
			panelRect.y = Util.RemapUnclamped(
				cameraRect.y + padding, cameraRect.yMax - padding,
				cameraRect.y, cameraRect.y - panelRect.height + cameraRect.height,
				Input.MouseGlobalPosition.y
			);
		}

		// BG
		var bgCell = Renderer.DrawPixel(default, new Color32(249, 249, 249, 255));

		// Items
		Cell highlightCell = null;
		int textStart = Renderer.GetUsedCellCount();
		int indent = Unify(36);
		var rect = new IRect(panelRect.x, panelRect.yMax, panelRect.width, itemHeight);
		int checkShrink = itemHeight / 6;
		int maxWidth = panelWidth;
		int iconPadding = Unify(4);
		bool ignoreClose = false;
		int localHoverIndex = -1;
		for (int i = 0; i < ItemCount; i++) {

			var item = Items[i];
			bool isSeparator = string.IsNullOrEmpty(item.Label);
			rect.y -= isSeparator ? separatorHeight : itemHeight;

			if (!item.Visible) continue;

			if (isSeparator) {
				// Separator
				Renderer.Draw(
					LINE_CODE, new(rect.x, rect.y + separatorHeight / 4, rect.width, separatorHeight / 2),
					Color32.BLACK_32
				);
			} else {
				// Item 

				// Highlight
				bool hover = rect.MouseInside();

				if (hover) {
					HoveringFrame = i != HoveringIndex ? 0 : (HoveringFrame + 1);
					HoveringIndex = i;
					localHoverIndex = i;
					if (HoveringFrame == 48) {
						// Refresh Visible
						if (Items[i].IsSubMenu && i + 1 < Items.Length) {
							HideAll();
							MakeVisible(i + 1);
						} else {
							HideAll();
							MakeVisible(i);
						}
					}
				}

				if (hover && item.Enabled) {
					highlightCell = Renderer.DrawPixel(rect, Color32.GREY_230);
				}

				using (Scope.GUIEnable(item.Enabled)) {

					// Check Mark
					if (item.Checked) {
						Renderer.Draw(
							item.Mark != 0 ? item.Mark : CHECK_CODE,
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
					if (hover && item.Enabled && Input.MouseLeftButtonDown) {
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

		// No Hover
		if (ItemCount > 0 && localHoverIndex == -1) {
			HoveringFrame = HoveringIndex != -1 ? 0 : (HoveringFrame + 1);
			HoveringIndex = -1;
			if (HoveringFrame == 48) {
				// Refresh Visible
				HideAll();
				MakeVisible(0);
			}
		}

		// BG
		BackgroundRect = panelRect.Expand(Unify(4));
		bgCell.X = BackgroundRect.x;
		bgCell.Y = BackgroundRect.y;
		bgCell.Width = BackgroundRect.width;
		bgCell.Height = BackgroundRect.height;

		// Clamp Text
		Renderer.ClampCells(panelRect, textStart);

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
	public static void BeginPopup (Int2 globalOffset, int menuID = 0) {
		if (Instance == null) return;
		if (Stage.Enable) {
			Stage.SpawnEntity(Instance.TypeID, 0, 0);
		} else {
			Instance.Active = true;
			Instance.SpawnFrame = Game.GlobalFrame;
			Instance.OnActivated();
		}
		ClearItems();
		Instance.InvokingItemData = null;
		Instance.ItemCount = 0;
		Instance.OffsetX = globalOffset.x - Renderer.CameraRect.x;
		Instance.OffsetY = globalOffset.y - Renderer.CameraRect.y;
		Instance.MenuID = menuID;
		Instance.CurrentSubLevel = 0;
		Instance.SeparatorCount = 0;
		Instance.HoveringFrame = 0;
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
		if (string.IsNullOrEmpty(label)) {
			Instance.SeparatorCount++;
		}
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
	}


	private void MakeVisible (int pointIndex) {

		// Set Visible
		int currentLevel = Items[pointIndex].Level;

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
		for (int i = pointIndex; i < Items.Length; i++) {
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