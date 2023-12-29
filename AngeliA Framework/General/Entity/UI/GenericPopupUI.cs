using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.StageOrder(-1024)]
	public class GenericPopupUI : EntityUI {




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
		private static readonly int CHECK_CODE = "CheckMark32".AngeHash();
		private static readonly int LINE_CODE = "Soft Line H".AngeHash();

		// Api
		public static bool ShowingPopup => Instance != null && Instance.Active;
		public static int CurrentItemCount => Instance != null ? Instance.ItemCount : 0;

		// Data
		private static GenericPopupUI Instance;
		private readonly Item[] Items = new Item[64];
		private int ItemCount = 0;
		private int OffsetX = 0;
		private int OffsetY = 0;


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
			FrameInput.UseMouseKey(0);
			FrameInput.UseMouseKey(1);
			FrameInput.UseMouseKey(2);
		}


		public override void OnInactivated () {
			base.OnInactivated();
			for (int i = 0; i < ItemCount; i++) {
				var item = Items[i];
				item.Label = "";
				item.Action = null;
			}
		}


		public override void UpdateUI () {
			base.UpdateUI();

			if (ItemCount == 0) {
				Active = false;
				return;
			}

			int separatorCount = 0;
			for (int i = 0; i < ItemCount; i++) {
				if (string.IsNullOrEmpty(Items[i].Label)) separatorCount++;
			}
			int panelWidth = Unify(200);
			int itemHeight = Unify(30);
			int separatorHeight = Unify(6);
			var panelRect = new IRect(
				CellRenderer.CameraRect.x + OffsetX,
				CellRenderer.CameraRect.y + OffsetY,
				panelWidth,
				itemHeight * (ItemCount - separatorCount) + separatorHeight * separatorCount
			);
			if (CellRenderer.CameraRect.y + OffsetY > CellRenderer.CameraRect.CenterY()) {
				panelRect.y -= itemHeight * ItemCount;
			}
			panelRect.ClampPositionInside(CellRenderer.CameraRect);

			// Items
			Cell highlightCell = null;
			int textStart = CellRenderer.GetTextUsedCellCount();
			int indent = Unify(42);
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
					CellRenderer.Draw(
						LINE_CODE, new(rect.x, rect.y + separatorHeight / 4, rect.width, separatorHeight / 2),
						new Byte4(0, 0, 0, 32), int.MaxValue
					);
				} else {
					// Item
					var tint = item.Enabled ? Const.BLACK : Const.BLACK_128;

					// Check Mark
					if (item.Checked) {
						CellRenderer.Draw(
							CHECK_CODE, new IRect(rect.x, rect.y, rect.height, rect.height).Shrink(checkShrink),
							tint, int.MaxValue
						);
					}

					// Label
					CellRendererGUI.Label(
						CellContent.Get(item.Label, tint, 20, Alignment.MidLeft),
						rect.Shrink(indent, 0, 0, 0),
						out var labelBounds
					);
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
						CellRenderer.Draw(item.Icon, iconRect, int.MaxValue);
					}

					// Highlight
					bool hover = rect.Contains(FrameInput.MouseGlobalPosition);
					if (hover && item.Enabled) {
						highlightCell = CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_230, int.MaxValue - 1);
					}

					// Click
					if (hover && item.Enabled && FrameInput.MouseLeftButtonDown) {
						item.Action?.Invoke();
					}
				}

			}
			panelRect.width = Util.Max(panelRect.width, maxWidth);
			if (highlightCell != null) highlightCell.Width = panelRect.width;

			// BG
			CellRenderer.Draw(
				Const.PIXEL, panelRect.Expand(Unify(8)), new Byte4(249, 249, 249, 255), int.MaxValue - 2
			);

			// Clamp Text
			int textEnd = CellRenderer.GetTextUsedCellCount();
			CellRenderer.ClampTextCells(panelRect, textStart, textEnd);

			// Cancel
			if (FrameInput.AnyMouseButtonDown || FrameInput.AnyKeyDown) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseMouseKey(1);
				FrameInput.UseMouseKey(2);
				Active = false;
			}

		}


		#endregion




		#region --- API ---


		public static void BeginPopup () {
			if (Instance == null || Instance.Active) return;
			Stage.SpawnEntity(Instance.TypeID, 0, 0);
			Instance.ItemCount = 0;
			Instance.OffsetX = FrameInput.MouseGlobalPosition.x - CellRenderer.CameraRect.x;
			Instance.OffsetY = FrameInput.MouseGlobalPosition.y - CellRenderer.CameraRect.y;
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


		#endregion




	}
}