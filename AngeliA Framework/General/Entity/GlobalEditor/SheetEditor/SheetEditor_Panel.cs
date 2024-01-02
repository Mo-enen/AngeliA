using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class SheetEditor {




		#region --- SUB ---


		private enum PanelType { Palette, File, }


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int UI_TAB_PAL = "UI.SheetTab.Palette".AngeHash();
		private static readonly int UI_TAB_FILE = "UI.SheetTab.File".AngeHash();
		private static readonly int UI_MENU_CREATE_ATLAS = "Menu.SheetEditor.CreateAtlas".AngeHash();
		private static readonly int UI_MENU_CREATE_UNIT = "Menu.SheetEditor.CreateUnit".AngeHash();
		private static readonly int UI_MENU_NEW_ATLAS_NAME = "Menu.SheetEditor.NewAtlasName".AngeHash();
		private static readonly int UI_MENU_NEW_UNIT_NAME = "Menu.SheetEditor.NewSpriteName".AngeHash();
		private static readonly int UI_DELETE_ATLAS_MSG = "Menu.SheetEditor.DeleteAtlasMSG".AngeHash();
		private static readonly int UI_DELETE_UNIT_MSG = "Menu.SheetEditor.DeleteUnitMSG".AngeHash();
		private static readonly int UI_CANCEL = "UI.Cancel".AngeHash();
		private static readonly int UI_DELETE = "UI.Delete".AngeHash();

		// Data
		private PanelType CurrentPanelType = PanelType.File;
		private int FilePanelScroll = 0;


		#endregion




		#region --- MSG ---


		private void Update_Panel () {

			var panelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Left, Unify(300));

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, 0);

			Update_Panel_Tab(ref panelRect);

			switch (CurrentPanelType) {
				case PanelType.Palette:
					Update_Panel_Palette(panelRect);
					break;
				case PanelType.File:
					Update_Panel_File(panelRect);
					break;
			}

		}


		private void Update_Panel_Tab (ref IRect panelRect) {

			int TAB_SIZE = Unify(36);
			var tabPanelRect = panelRect.Shrink(Unify(6), Unify(6), 0, 0);

			// Tabs
			for (int i = 0; i < 2; i++) {

				int tabBorder = Unify(10);
				bool selecting = i == (int)CurrentPanelType;
				var tabRect = new IRect(
					tabPanelRect.x + i * tabPanelRect.width / 2, tabPanelRect.y, tabPanelRect.width / 2, TAB_SIZE
				);
				bool tabInteractable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog;

				// Button
				CellRenderer.Draw_9Slice(
					BuiltInIcon.UI_TAB, tabRect,
					tabBorder, tabBorder, tabBorder, tabBorder,
					selecting ? Const.GREY_128 : Const.GREY_64, 1
				);
				if (tabInteractable) CursorSystem.SetCursorAsHand(tabRect);

				// Highlight
				if (selecting) {
					var cells = CellRenderer.Draw_9Slice(
						BuiltInIcon.UI_TAB, tabRect.EdgeOutside(Direction4.Up, tabBorder).Shift(0, -tabBorder),
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
					i == (int)PanelType.Palette ? BuiltInIcon.ICON_PALETTE : BuiltInIcon.ICON_FILE,
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


		private void Update_Panel_File (IRect panelRect) {

			const int EXTRA_ITEM = 6;
			int itemHeight = Unify(20);
			int scrollBarWidth = Unify(20);
			var selectingAtlas = SelectingAtlas;
			int totalItemCount = Atlas.Count + (selectingAtlas != null ? selectingAtlas.Units.Count : 0);
			int pageItemCount = panelRect.height / itemHeight;

			// Scroll bar
			FilePanelScroll = CellRendererGUI.ScrollBar(
				panelRect.EdgeInside(Direction4.Right, scrollBarWidth),
				z: 1, FilePanelScroll, totalItemCount + EXTRA_ITEM, pageItemCount
			);
			FilePanelScroll = FilePanelScroll.Clamp(0, totalItemCount - pageItemCount + EXTRA_ITEM);
			panelRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);

			// Content
			int requireSelectAtlas = -2;
			int requireSelectUnit = -1;
			int contentStartCellIndex = CellRenderer.GetUsedCellCount();
			int contentStartTextIndex = CellRenderer.GetTextUsedCellCount();
			var rect = new IRect(0, panelRect.yMax + FilePanelScroll * itemHeight, panelRect.width, itemHeight);
			for (int aIndex = 0; aIndex < Atlas.Count; aIndex++) {

				rect.y -= itemHeight;
				bool isSelectingAtlas = aIndex == SelectingAtlasIndex;
				var atlas = Atlas[aIndex];

				// Draw Atlas
				if (rect.y <= panelRect.yMax && rect.yMax >= panelRect.y) {

					bool mouseInside = rect.Contains(FrameInput.MouseGlobalPosition);

					// Tri Mark
					var atlasRect = rect.Shrink(rect.height, 0, 0, 0);
					CellRenderer.Draw(
						isSelectingAtlas ? BuiltInIcon.ICON_TRIANGLE_DOWN : BuiltInIcon.ICON_TRIANGLE_RIGHT,
						atlasRect.EdgeOutside(Direction4.Left, atlasRect.height),
						z: 2
					);

					// Icon
					CellRenderer.Draw(
						BuiltInIcon.ICON_ATLAS,
						atlasRect.EdgeInside(Direction4.Left, atlasRect.height),
						z: 2
					);

					// Label
					var labelRect = atlasRect.Shrink(atlasRect.height, 0, 0, 0);
					if (isSelectingAtlas) {
						atlas.Name = CellRendererGUI.TextField(3746234, labelRect, atlas.Name, out bool changed);
						if (changed) {
							SetDirty(aIndex, -1, -1);
						}
					} else {
						CellRendererGUI.Label(CellContent.Get(atlas.Name, alignment: Alignment.MidLeft), labelRect);
					}

					// Highlight
					if (mouseInside) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_12, z: 1);
					}

					// Click
					if (FrameInput.MouseLeftButtonDown && mouseInside) {
						requireSelectAtlas = isSelectingAtlas ? -1 : aIndex;
					}
				}

				if (isSelectingAtlas && selectingAtlas != null) {
					// Draw Units
					for (int uIndex = 0; uIndex < selectingAtlas.Units.Count; uIndex++) {

						rect.y -= itemHeight;
						if (rect.y > panelRect.yMax || rect.yMax < panelRect.y) continue;

						var unitRect = rect.Shrink(rect.height * 2, 0, 0, 0);
						var unit = selectingAtlas.Units[uIndex];
						bool isSelectingUnit = uIndex == SelectingUnitIndex;
						bool mouseInside = rect.Contains(FrameInput.MouseGlobalPosition);

						// Icon
						CellRenderer.Draw(
							BuiltInIcon.ICON_SPRITE,
							unitRect.EdgeInside(Direction4.Left, unitRect.height),
							z: 3
						);

						// Label
						var labelRect = unitRect.Shrink(unitRect.height, 0, 0, 0);
						if (isSelectingUnit) {
							unit.Name = CellRendererGUI.TextField(3746234, labelRect, unit.Name, out bool changed);
							if (changed) {
								SetDirty(aIndex, uIndex, -1);
							}
						} else {
							CellRendererGUI.Label(CellContent.Get(unit.Name, alignment: Alignment.MidLeft), labelRect);
						}

						// Highlight
						if (mouseInside) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_12, z: 1);
						}

						// Selecting Highlight
						if (isSelectingUnit) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, z: 2);
						}

						// Click
						if (!isSelectingUnit && FrameInput.MouseLeftButtonDown && mouseInside) {
							requireSelectUnit = uIndex;
						}

					}
				}
			}

			// Selection Change
			if (requireSelectAtlas >= -1) SelectAtlas(requireSelectAtlas);
			if (requireSelectUnit >= 0) SelectUnit(requireSelectUnit);

			// Clamp
			CellRenderer.ClampCells(panelRect, contentStartCellIndex, CellRenderer.GetUsedCellCount());
			CellRenderer.ClampTextCells(panelRect, contentStartTextIndex, CellRenderer.GetTextUsedCellCount());
		}


		private void Update_Panel_Palette (IRect panelRect) {

		}


		#endregion




		#region --- LGC ---


		private static void ShowPanelPopup () {
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(Language.Get(UI_MENU_CREATE_ATLAS, "Create Atlas"), CreateNewAtlas);
			// Func
			static void CreateNewAtlas () {
				if (Instance == null) return;
				Instance.Atlas.Insert(0, new EditalbeAtlas() {
					Guid = System.Guid.NewGuid().ToString(),
					Name = Language.Get(UI_MENU_NEW_ATLAS_NAME, "New Atlas"),
					SheetType = SheetType.General,
					SheetZ = 0,
					Units = new List<EditableUnit>(),
				});
				Instance.SetDirty(0, -1, -1);
			}
		}


		private static void ShowAtlasPopup (int atlasIndex) {
			if (Instance == null || atlasIndex < 0 || atlasIndex >= Instance.Atlas.Count) return;
			Instance.SelectAtlas(atlasIndex);
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(Language.Get(UI_MENU_CREATE_UNIT, "Create Sprite"), CreateNewUnit);
			GenericPopupUI.AddItem(Language.Get(UI_DELETE, "Delete"), Delete, enabled: Instance.Atlas.Count > 1);
			static void CreateNewUnit () {
				if (Instance == null || Instance.SelectingAtlasIndex < 0 || Instance.SelectingAtlasIndex >= Instance.Atlas.Count) return;
				var selectingAtlas = Instance.Atlas[Instance.SelectingAtlasIndex];
				selectingAtlas.Units.Add(new EditableUnit() {
					Guid = System.Guid.NewGuid().ToString(),
					Name = Language.Get(UI_MENU_NEW_UNIT_NAME, "New Sprite"),
					GroupType = GroupType.General,
					Sprites = new List<EditableSprite>(),
				});
			}
			static void Delete () {
				GenericDialogUI.SpawnDialog(
					Language.Get(UI_DELETE_ATLAS_MSG, "Delete Atlas?"),
					Language.Get(UI_DELETE, "Delete"), DeleteNow,
					Language.Get(UI_CANCEL, "Cancel"), Const.EmptyMethod
				);
				static void DeleteNow () {
					if (Instance.SelectingAtlasIndex < 0 || Instance.SelectingAtlasIndex >= Instance.Atlas.Count) return;
					Instance.Atlas.RemoveAt(Instance.SelectingAtlasIndex);
				}
			}
		}


		private static void ShowUnitPopup (int atlasIndex, int unitIndex) {
			if (Instance == null || atlasIndex < 0 || atlasIndex >= Instance.Atlas.Count) return;
			var atlas = Instance.Atlas[atlasIndex];
			if (unitIndex < 0 || unitIndex >= atlas.Units.Count) return;
			Instance.SelectAtlas(atlasIndex);
			Instance.SelectUnit(unitIndex);
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(Language.Get(UI_DELETE, "Delete"), Delete);
			static void Delete () {
				GenericDialogUI.SpawnDialog(
					Language.Get(UI_DELETE_UNIT_MSG, "Delete Unit?"),
					Language.Get(UI_DELETE, "Delete"), DeleteNow,
					Language.Get(UI_CANCEL, "Cancel"), Const.EmptyMethod
				);
				static void DeleteNow () {
					if (Instance.SelectingAtlasIndex < 0 || Instance.SelectingAtlasIndex >= Instance.Atlas.Count) return;
					var atlas = Instance.Atlas[Instance.SelectingAtlasIndex];
					if (Instance.SelectingUnitIndex < 0 || Instance.SelectingUnitIndex >= atlas.Units.Count) return;
					atlas.Units.RemoveAt(Instance.SelectingUnitIndex);
				}
			}
		}


		#endregion




	}
}