using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.StageOrder(-1024)]
	public class GenericPopupUI : EntityUI {




		#region --- SUB ---


		private class Item {
			public string Label;
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

			int panelWidth = Unify(200);
			int itemHeight = Unify(30);
			var panelRect = new RectInt(
				CellRenderer.CameraRect.x + OffsetX,
				CellRenderer.CameraRect.y + OffsetY,
				panelWidth, itemHeight * ItemCount
			);
			if (OffsetY > CellRenderer.CameraRect.CenterY()) {
				panelRect.y -= itemHeight * ItemCount;
			}
			panelRect.ClampPositionInside(CellRenderer.CameraRect);

			// BG
			CellRenderer.Draw(
				Const.PIXEL, panelRect.Expand(Unify(8)), new Color32(249, 249, 249, 255), int.MaxValue - 2
			);

			// Items
			int textStart = CellRenderer.GetTextUsedCellCount();
			int indent = Unify(42);
			var rect = new RectInt(panelRect.x, 0, panelRect.width, itemHeight);
			int checkShrink = itemHeight / 6;
			for (int i = 0; i < ItemCount; i++) {

				var item = Items[i];
				rect.y = panelRect.yMax - (i + 1) * itemHeight;
				var tint = item.Enabled ? Const.BLACK : Const.BLACK_128;

				if (!string.IsNullOrEmpty(item.Label)) {
					// Item

					// Check
					if (item.Checked) {
						CellRenderer.Draw(
							CHECK_CODE, new RectInt(rect.x, rect.y, rect.height, rect.height).Shrink(checkShrink),
							tint, int.MaxValue
						);
					}

					// Label
					CellRendererGUI.Label(CellContent.Get(item.Label, tint, 20, Alignment.MidLeft), rect.Shrink(indent, 0, 0, 0));

					// Highlight
					bool hover = item.Enabled && rect.Contains(FrameInput.MouseGlobalPosition);
					if (hover) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_230, int.MaxValue - 1);
					}

					// Click
					if (hover && FrameInput.MouseLeftButtonDown) {
						item.Action?.Invoke();
					}

				} else {
					// Line
					int shrink = rect.height / 2 + Unify(2);
					CellRenderer.Draw(LINE_CODE, rect.Shrink(0, 0, shrink, shrink), Const.GREY_230, int.MaxValue);
				}

			}

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


		public static void AddItem (string label, System.Action action, bool enabled = true, bool @checked = false) {
			if (Instance == null || Instance.ItemCount >= Instance.Items.Length - 1) return;
			var item = Instance.Items[Instance.ItemCount];
			item.Label = label;
			item.Action = action;
			item.Enabled = enabled;
			item.Checked = @checked;
			Instance.ItemCount++;
		}


		public static void ClosePopup () {
			if (Instance != null) Instance.Active = false;
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}