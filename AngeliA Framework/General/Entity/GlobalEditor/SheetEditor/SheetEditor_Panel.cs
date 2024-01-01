using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class SheetEditor {




		#region --- SUB ---


		private enum PanelType { Palette, File, }


		#endregion




		#region --- VAR ---


		// Const
		//private static readonly int BTN_DARK_CODE = "UI.DarkButton".AngeHash();
		//private static readonly int BTN_DARK_DOWN_CODE = "UI.DarkButtonDown".AngeHash();
		private static readonly int UI_TAB = "UI.Tab".AngeHash();
		private static readonly int UI_TAB_PAL = "UI.SheetTab.Palette".AngeHash();
		private static readonly int UI_TAB_FILE = "UI.SheetTab.File".AngeHash();
		private static readonly int UI_TAB_ICON_PAL = "SheetTabIcon.Palette".AngeHash();
		private static readonly int UI_TAB_ICON_FILE = "SheetTabIcon.File".AngeHash();

		// Data
		private PanelType CurrentPanelType = PanelType.File;


		#endregion




		#region --- MSG ---


		private void Update_Panel () {

			var panelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Left, Unify(300));

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, 0);

			Update_Panel_Tab(ref panelRect);

			switch (CurrentPanelType) {
				case PanelType.Palette:
					Update_Panel_Palette(ref panelRect);
					break;
				case PanelType.File:
					Update_Panel_File(ref panelRect);
					break;
			}

		}


		private void Update_Panel_Tab (ref IRect panelRect) {

			int TAB_SIZE = Unify(36);

			// Tabs
			for (int i = 0; i < 2; i++) {

				int tabBorder = Unify(10);
				bool selecting = i == (int)CurrentPanelType;
				var tabRect = new IRect(
					panelRect.x + i * panelRect.width / 2, panelRect.y, panelRect.width / 2, TAB_SIZE
				);
				bool tabInteractable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog;

				// Button
				CellRenderer.Draw_9Slice(
					UI_TAB, tabRect,
					tabBorder, tabBorder, tabBorder, tabBorder,
					selecting ? Const.GREY_128 : Const.GREY_64, 1
				);
				if (tabInteractable) CursorSystem.SetCursorAsHand(tabRect);

				// Highlight
				if (selecting) {
					var cells = CellRenderer.Draw_9Slice(
						UI_TAB, tabRect.EdgeOutside(Direction4.Up, tabBorder).Shift(0, -tabBorder),
						tabBorder, tabBorder, 0, tabBorder,
						new(225, 171, 48, 255), 2
					);
					cells[0].Shift.down = tabBorder / 2;
					cells[1].Shift.down = tabBorder / 2;
					cells[2].Shift.down = tabBorder / 2;
				}

				// Label
				CellRendererGUI.Label(
					CellContent.Get(
						i == 0 ? Language.Get(UI_TAB_PAL, "Palette") : Language.Get(UI_TAB_FILE, "File"),
						Const.WHITE,
						alignment: Alignment.MidMid, charSize: 22
					),
					tabRect.Shrink(tabRect.height * 2 / 3, 0, 0, 0), out var labelBounds
				);

				// Icon
				CellRenderer.Draw(
					i == (int)PanelType.Palette ? UI_TAB_ICON_PAL : UI_TAB_ICON_FILE,
					labelBounds.EdgeOutside(Direction4.Left, labelBounds.height).Shift(-labelBounds.height / 3, 0),
					Const.WHITE, 2
				);

				// Click
				if (tabInteractable && FrameInput.MouseLeftButtonDown && tabRect.Contains(FrameInput.MouseGlobalPosition)) {
					CurrentPanelType = (PanelType)i;
				}
			}

			// Final
			panelRect = panelRect.Shrink(0, 0, TAB_SIZE, 0);
		}


		private void Update_Panel_File (ref IRect panelRect) {






		}


		private void Update_Panel_Palette (ref IRect panelRect) {

		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}