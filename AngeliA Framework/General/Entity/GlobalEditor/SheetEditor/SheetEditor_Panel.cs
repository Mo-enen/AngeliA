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
		private static int PopupAtlasIndex = -1;
		private static int PopupUnitIndex = -1;
		private PanelType CurrentPanelType = PanelType.File;
		private int FilePanelScroll = 0;
		private readonly CellContent ItemLabelContent = new() { Alignment = Alignment.MidLeft, CharSize = 18, Clip = false, Wrap = false, Tint = Const.GREY_230, };
		private readonly CellContent ItemInputContent = new() { Alignment = Alignment.MidLeft, CharSize = 18, Clip = false, Wrap = false, Tint = Const.GREY_230, };


		#endregion




		#region --- MSG ---


		private void Update_Panel () {

			var panelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, 0);

			Update_Panel_Tab(ref panelRect);

			switch (CurrentPanelType) {
				case PanelType.Palette:
					Update_PalettePanel(panelRect);
					break;
				case PanelType.File:
					Update_FilePanel(panelRect);
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
				if (tabInteractable && FrameInput.MouseLeftButtonDown && tabRect.MouseInside()) {
					CurrentPanelType = (PanelType)i;
				}
			}

			// Final
			panelRect = panelRect.Shrink(0, 0, TAB_SIZE, 0);
		}


		private void Update_FilePanel (IRect panelRect) {

			const int EXTRA_ITEM = 6;
			int itemHeight = Unify(24);
			int itemPadding = Unify(2);
			int iconShrink = Unify(2);
			int labelPadding = Unify(6);
			int scrollBarWidth = Unify(20);
			int pageItemCount = panelRect.height / itemHeight;
			panelRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);

			// Content
			int requireSelectAtlas = -1;
			int requireSelectUnit = -1;
			int contentStartCellIndex = CellRenderer.GetUsedCellCount();
			int contentStartTextIndex = CellRenderer.GetTextUsedCellCount();
			int totalItemCount = 0;
			var rect = new IRect(panelRect.x, panelRect.yMax + FilePanelScroll * itemHeight, panelRect.width, itemHeight);
			for (int aIndex = 0; aIndex < Atlas.Count; aIndex++) {

				rect.y -= itemHeight;
				totalItemCount++;
				bool isSelectingAtlas = aIndex == SelectingAtlasIndex;
				var atlas = Atlas[aIndex];

				// Draw Atlas
				if (rect.y <= panelRect.yMax && rect.yMax >= panelRect.y) {

					// Tri Mark
					var atlasRect = rect.Shrink(rect.height, 0, itemPadding, itemPadding);
					var triMarkRect = atlasRect.EdgeOutside(Direction4.Left, atlasRect.height);
					CellRenderer.Draw(
						atlas.Unfold ? BuiltInIcon.ICON_TRIANGLE_DOWN : BuiltInIcon.ICON_TRIANGLE_RIGHT,
						triMarkRect, Const.GREY_196, z: 2
					);
					CursorSystem.SetCursorAsHand(triMarkRect);

					// Icon
					CellRenderer.Draw(
						BuiltInIcon.ICON_ATLAS,
						atlasRect.EdgeInside(Direction4.Left, atlasRect.height),
						Const.ORANGE_BETTER, z: 2
					);

					// Label
					var labelRect = atlasRect.Shrink(atlasRect.height + labelPadding, 0, 0, 0);
					int atlasInputID = 3746234 + aIndex;
					if (CellRendererGUI.TypingTextFieldID == atlasInputID) {
						CellRenderer.Draw_9Slice(BuiltInIcon.FRAME_16, labelRect, Const.GREY_128, z: 2);
					}
					atlas.Name = CellRendererGUI.TextField(
						atlasInputID, labelRect, ItemInputContent.SetText(atlas.Name), out bool changed
					);
					if (changed) {
						SetDirty(aIndex, -1, -1);
					}

					// Highlight
					if (rect.MouseInside()) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_12, z: 1);
					}

					// Click for Folding
					if (FrameInput.MouseLeftButtonDown && triMarkRect.MouseInside()) {
						atlas.Unfold = !atlas.Unfold;
					}

				}

				if (atlas.Unfold) {
					// Draw Units
					for (int uIndex = 0; uIndex < atlas.Units.Count; uIndex++) {

						rect.y -= itemHeight;
						totalItemCount++;
						if (rect.y > panelRect.yMax || rect.yMax < panelRect.y) continue;

						var unitRect = rect.Shrink(rect.height * 2, 0, 0, 0);
						var unit = atlas.Units[uIndex];
						bool isSelectingUnit = aIndex == SelectingAtlasIndex && uIndex == SelectingUnitIndex;
						bool mouseInside = rect.MouseInside();

						// Icon
						int icon = BuiltInIcon.ICON_SPRITE;
						var iconRect = unitRect.EdgeInside(Direction4.Left, unitRect.height).Shrink(iconShrink);
						if (CellRenderer.TryGetSpriteFromGroup(unit.GlobalID, 0, out var sprite, false, true)) {
							icon = sprite.GlobalID;
							iconRect = iconRect.Fit(sprite);
						}
						CellRenderer.Draw(icon, iconRect, z: 3);

						// Label
						var labelRect = unitRect.Shrink(unitRect.height + labelPadding, 0, 0, 0);
						if (isSelectingUnit) {
							int unitInputID = 3746239 + Atlas.Count + uIndex;
							if (CellRendererGUI.TypingTextFieldID == unitInputID) {
								CellRenderer.Draw_9Slice(BuiltInIcon.FRAME_16, labelRect, Const.GREY_128, z: 2);
							}
							unit.Name = CellRendererGUI.TextField(
								unitInputID, labelRect, ItemInputContent.SetText(unit.Name), out bool changed
							);
							if (changed) {
								SetDirty(aIndex, uIndex, -1);
							}
						} else {
							CellRendererGUI.Label(ItemLabelContent.SetText(unit.Name), labelRect);
						}

						// Highlight
						if (mouseInside) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_12, z: 1);
						}

						// Selecting Highlight
						if (isSelectingUnit) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_42, z: 2);
						}

						// Click
						if (!isSelectingUnit && FrameInput.MouseLeftButtonDown && mouseInside) {
							requireSelectAtlas = aIndex;
							requireSelectUnit = uIndex;
						}

					}
				}
			}

			// Scroll bar
			if (totalItemCount - pageItemCount + EXTRA_ITEM > 0) {
				FilePanelScroll = CellRendererGUI.ScrollBar(
					panelRect.EdgeOutside(Direction4.Right, scrollBarWidth),
					z: 1, FilePanelScroll, totalItemCount + EXTRA_ITEM, pageItemCount
				);
				if (FrameInput.MouseWheelDelta != 0 && panelRect.MouseInside()) {
					FilePanelScroll -= 2 * FrameInput.MouseWheelDelta;
				}
				FilePanelScroll = FilePanelScroll.Clamp(0, totalItemCount - pageItemCount + EXTRA_ITEM);
			} else {
				FilePanelScroll = 0;
			}

			// Selection Change
			if (requireSelectAtlas >= 0 && requireSelectUnit >= 0) {
				SelectUnit(requireSelectAtlas, requireSelectUnit);
			}

			// Clamp
			CellRenderer.ClampCells(panelRect, contentStartCellIndex, CellRenderer.GetUsedCellCount());
			CellRenderer.ClampTextCells(panelRect, contentStartTextIndex, CellRenderer.GetTextUsedCellCount());
		}


		private void Update_PalettePanel (IRect panelRect) {

		}


		#endregion




		#region --- LGC ---


		private static void ShowPanelPopup () {
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(Language.Get(UI_MENU_CREATE_ATLAS, "Create Atlas"), CreateNewAtlas);
			// Func
			static void CreateNewAtlas () {
				if (Instance == null) return;
				Instance.Atlas.Insert(0, new EditableAtlas() {
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
			PopupAtlasIndex = atlasIndex;
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(Language.Get(UI_MENU_CREATE_UNIT, "Create Sprite"), CreateNewUnit);
			GenericPopupUI.AddItem(Language.Get(UI_DELETE, "Delete"), Delete, enabled: Instance.Atlas.Count > 1);
			static void CreateNewUnit () {
				if (Instance == null || PopupAtlasIndex < 0 || PopupAtlasIndex >= Instance.Atlas.Count) return;
				var selectingAtlas = Instance.Atlas[PopupAtlasIndex];
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
					if (PopupAtlasIndex < 0 || PopupAtlasIndex >= Instance.Atlas.Count) return;
					Instance.Atlas.RemoveAt(PopupAtlasIndex);
				}
			}
		}


		private static void ShowUnitPopup (int atlasIndex, int unitIndex) {
			if (Instance == null) return;
			if (atlasIndex < 0 || atlasIndex >= Instance.Atlas.Count) return;
			var atlas = Instance.Atlas[atlasIndex];
			if (unitIndex < 0 || unitIndex >= atlas.Units.Count) return;
			PopupAtlasIndex = atlasIndex;
			PopupUnitIndex = unitIndex;
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(Language.Get(UI_DELETE, "Delete"), Delete);
			static void Delete () {
				GenericDialogUI.SpawnDialog(
					Language.Get(UI_DELETE_UNIT_MSG, "Delete Unit?"),
					Language.Get(UI_DELETE, "Delete"), DeleteNow,
					Language.Get(UI_CANCEL, "Cancel"), Const.EmptyMethod
				);
				static void DeleteNow () {
					if (PopupAtlasIndex < 0 || PopupAtlasIndex >= Instance.Atlas.Count) return;
					var atlas = Instance.Atlas[PopupAtlasIndex];
					if (PopupUnitIndex < 0 || PopupUnitIndex >= atlas.Units.Count) return;
					atlas.Units.RemoveAt(PopupUnitIndex);
				}
			}
		}


		#endregion




	}
}