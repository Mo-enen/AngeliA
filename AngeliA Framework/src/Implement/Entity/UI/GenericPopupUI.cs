using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.StageOrder(4096)]
public class GenericPopupUI : EntityUI, IWindowEntityUI {




	#region --- SUB ---


	private class Item {
		public string Label;
		public int Icon;
		public Direction2 IconPosition;
		public bool Checked;
		public bool Enabled;
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

	// Data
	private readonly Item[] Items = new Item[128];
	private int ItemCount = 0;


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
		if (cameraRect.y + OffsetY > cameraRect.CenterY()) {
			panelRect.y -= itemHeight * ItemCount;
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
		int textStart = Renderer.GetTextUsedCellCount();
		int indent = Unify(36);
		var rect = new IRect(panelRect.x, panelRect.yMax, panelRect.width, itemHeight);
		int checkShrink = itemHeight / 6;
		int maxWidth = panelWidth;
		int iconPadding = Unify(4);
		for (int i = 0; i < ItemCount; i++) {

			var item = Items[i];
			bool isSeparator = string.IsNullOrEmpty(item.Label);
			rect.y -= isSeparator ? separatorHeight : itemHeight;

			if (isSeparator) {
				// Separator
				Renderer.Draw(
					LINE_CODE, new(rect.x, rect.y + separatorHeight / 4, rect.width, separatorHeight / 2),
					new Color32(0, 0, 0, 32), int.MaxValue
				);
			} else {
				// Item
				var tint = item.Enabled ? Color32.BLACK : Color32.BLACK_128;

				// Highlight
				bool hover = rect.MouseInside();
				if (hover && item.Enabled) {
					highlightCell = Renderer.DrawPixel(rect, Color32.GREY_230);
				}

				using (Scope.GUIEnable(item.Enabled)) {

					// Check Mark
					if (item.Checked) {
						Renderer.Draw(
							CHECK_CODE,
							new IRect(rect.x, rect.y, rect.height, rect.height).Shrink(checkShrink),
							tint
						);
					}

					// Label
					GUI.Label(rect.Shrink(indent, 0, 0, 0), item.Label, out var labelBounds, GUISkin.SmallDarkLabel);
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

					// Click
					if (hover && item.Enabled && Input.MouseLeftButtonDown) {
						item.Action?.Invoke();
					}
				}
			}

		}
		panelRect.width = Util.Max(panelRect.width, maxWidth);
		if (highlightCell != null) highlightCell.Width = panelRect.width;

		// BG
		BackgroundRect = panelRect.Expand(Unify(4));
		bgCell.X = BackgroundRect.x;
		bgCell.Y = BackgroundRect.y;
		bgCell.Width = BackgroundRect.width;
		bgCell.Height = BackgroundRect.height;

		// Clamp Text
		Renderer.ClampTextCells(panelRect, textStart);

		// Block Input
		Input.IgnoreMouseInput(0);

		// Cancel
		if (Game.GlobalFrame > SpawnFrame && (Input.AnyMouseButtonDown || Input.AnyKeyDown)) {
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


	public static void BeginPopup () {
		if (Instance == null) return;
		if (Stage.Enable) {
			Stage.SpawnEntity(Instance.TypeID, 0, 0);
		} else {
			Instance.Active = true;
			Instance.SpawnFrame = Game.GlobalFrame;
			Instance.OnActivated();
		}
		ClearItems();
		Instance.ItemCount = 0;
		Instance.OffsetX = Input.UnshiftedMouseGlobalPosition.x - Renderer.CameraRect.x;
		Instance.OffsetY = Input.UnshiftedMouseGlobalPosition.y - Renderer.CameraRect.y;
	}


	public static void AddSeparator () => AddItem("", Const.EmptyMethod, true, false);


	public static void AddItem (string label, System.Action action, bool enabled = true, bool @checked = false) => AddItem(label, 0, default, action, enabled, @checked);


	public static void AddItem (string label, int icon, Direction2 iconPosition, System.Action action, bool enabled = true, bool @checked = false) {
		if (Instance == null || Instance.ItemCount >= Instance.Items.Length - 1) return;
		var item = Instance.Items[Instance.ItemCount];
		item.Label = label;
		item.Icon = icon;
		item.IconPosition = iconPosition;
		item.Action = action;
		item.Enabled = enabled;
		item.Checked = @checked;
		Instance.ItemCount++;
	}


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




}