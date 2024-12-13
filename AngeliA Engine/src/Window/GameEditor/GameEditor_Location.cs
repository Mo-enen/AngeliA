using System.Collections;
using System.Collections.Generic;
using System.IO;
using AngeliA;

namespace AngeliaEngine;

public partial class GameEditor {


	// Const
	private const string LOCATION_SLOT_FILE_NAME = "LocationSlot";
	private static readonly LanguageCode LABEL_ADD_LOCATION_SLOT = ("UI.RigEditor.AddLocationSlot", "+ New Location");
	private static readonly LanguageCode MSG_DELETE_LOCATION = ("UI.RigEditor.DeleteLocationMsg", "Delete Location {0}?");

	// Api
	public Int4? RequireSetViewPos { get; set; } = null; // x,y,z,w >> x,y,z,viewHeight (all in unit)
	public int ThumbnailSheetIndex { get; set; } = -1;

	// Data
	private static readonly GUIStyle NewLocationSlotButton = new(GUI.Skin.Button) { CharSize = 16, };
	private readonly List<(IRect rect, int z)> LocationSlots = [];
	private int LocationPanelScrollY = 0;
	private bool LocationSlotDirty = false;
	private WorldStream LocationThumbnailStream = null;


	// MSG
	private void DrawLocationPanel (ref IRect panelRect) {

		bool requireUI = Game.ShowingDoodle;
		if (requireUI) {
			Game.ForceGizmosOnTopOfUI(1);
		}
		using var _ = new SheetIndexScope(ThumbnailSheetIndex);
		using var __ = new LayerScope(requireUI ? RenderLayer.UI : RenderLayer.DEFAULT);

		int toolbarSize = Unify(56);
		int scrollbarWidth = Unify(12);
		int panelPaddingLarge = Unify(22);
		int panelPadding = Unify(6);
		int padding = Unify(4);
		int thumbnailPadding = Unify(4);
		int itemSize = Unify(72);
		panelRect.xMin = panelRect.xMax - Unify(256);
		panelRect.height = Unify(600);
		panelRect.y -= panelRect.height;
		var oldPanelRect = panelRect;
		var toolbarRect = panelRect.EdgeUp(toolbarSize).Shrink(panelPaddingLarge, panelPaddingLarge, panelPadding, panelPadding);
		panelRect = panelRect.Shrink(panelPadding, panelPadding + scrollbarWidth, panelPadding, panelPadding + toolbarSize);
		int cellStart = Renderer.GetUsedCellCount();

		// Toolbar
		var unitViewRect = Stage.ViewRect.Fit(1, 1).ToUnit();
		if (GUI.Button(toolbarRect, LABEL_ADD_LOCATION_SLOT, out var state, NewLocationSlotButton)) {
			LocationSlotDirty = true;
			LocationSlots.Add((unitViewRect, Stage.ViewZ));
			LocationPanelScrollY = int.MaxValue / 2;
		}
		var toolbarConRect = GUI.GetContentRect(toolbarRect, NewLocationSlotButton, state);
		DrawThumbnail(
			toolbarConRect.Shrink(thumbnailPadding / 2),
			LocationThumbnailStream, unitViewRect, Stage.ViewZ, 0, 500, toolbarRect
		);

		// Content
		int column = (panelRect.width / itemSize).GreaterOrEquel(1);
		const int EXTRA_ROW = 2;
		int row = LocationSlots.Count.CeilDivide(column);
		int pageRow = panelRect.height.CeilDivide(itemSize);
		int maxRow = (row + EXTRA_ROW - pageRow).GreaterOrEquel(0);
		int offsetX = (panelRect.width - itemSize * column - scrollbarWidth) / 2;
		using (var scroll = new GUIVerticalScrollScope(
			panelRect, LocationPanelScrollY, 0, maxRow * itemSize,
			layer: RenderLayer.DEFAULT
		)) {

			LocationPanelScrollY = scroll.PositionY.Clamp(0, maxRow * itemSize);

			for (int i = (LocationPanelScrollY / itemSize) * column; i < LocationSlots.Count; i++) {

				int currentColumn = i % column;
				int currentRow = i / column;
				if (currentRow > LocationPanelScrollY / itemSize + pageRow) break;

				var rect = new IRect(
					panelRect.x + currentColumn * itemSize + offsetX,
					panelRect.yMax - currentRow * itemSize - itemSize,
					itemSize, itemSize
				);
				var (viewRect, z) = LocationSlots[i];

				// Button
				if (GUI.Button(rect.Shrink(padding), 0, out var _state, GUI.Skin.DarkButton)) {
					int globalViewH = viewRect.height.ToGlobal();
					if (HavingGamePlay) {
						int maxHeight = CurrentProject.Universe.Info.DefaultViewHeight;
						globalViewH = globalViewH.Clamp(1, maxHeight);
					}
					int globalViewW = Game.GetViewWidthFromViewHeight(globalViewH);
					var globalCenterX = viewRect.x.ToGlobal() + viewRect.width.ToGlobal() / 2;
					var globalCenterY = viewRect.y.ToGlobal() + viewRect.height.ToGlobal() / 2;
					RequireSetViewPos = new Int4(
						(globalCenterX - globalViewW / 2).ToUnit(),
						(globalCenterY - globalViewH / 2).ToUnit(),
						z,
						globalViewH.ToUnit()
					);
				}
				var conRect = GUI.GetContentRect(rect.Shrink(padding), GUI.Skin.DarkButton, _state);

				// Thumbnail
				DrawThumbnail(conRect.Shift(0, LocationPanelScrollY).Shrink(thumbnailPadding), LocationThumbnailStream, viewRect, z, 500, 500, panelRect);

				// Menu
				if (Input.MouseRightButtonDown && rect.MouseInside()) {
					ShowLocaltionSlotMenu(Input.UnshiftedMouseGlobalPosition, i);
				}

			}
		}

		// Scrollbar
		if (maxRow > 0) {
			LocationPanelScrollY = GUI.ScrollBar(
				8925634, panelRect.EdgeRight(scrollbarWidth),
				LocationPanelScrollY, (row + EXTRA_ROW) * itemSize, panelRect.height
			).Clamp(0, maxRow * panelRect.height);
		}

		// Max Cell Z 
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = cellStart; i < count; i++) {
				cells[i].Z = int.MaxValue;
			}
		}

		// Final
		if (LocationSlotDirty) {
			LocationSlotDirty = false;
			SaveLocationSlotToFile();
		}
		panelRect = oldPanelRect;

		// Func
		static void DrawThumbnail (IRect rect, IBlockSquad squad, IRect unitViewRect, int z, int pivotX, int pivotY, IRect panelRect) {
			if (unitViewRect.width <= 0 || unitViewRect.height <= 0) return;
			int unitL = unitViewRect.x;
			int unitR = unitViewRect.xMax;
			int unitD = unitViewRect.y;
			int unitU = unitViewRect.yMax;
			rect = rect.Fit(unitViewRect.width, unitViewRect.height, pivotX, pivotY);
			float stepX = (float)rect.width / unitViewRect.width;
			float stepY = (float)rect.height / unitViewRect.height;
			var pxRect = new IRect(
				rect.x, 0,
				rect.width.UDivide(unitViewRect.width) + 1,
				rect.height.UDivide(unitViewRect.height) + 1
			);
			// Sky BG
			Game.DrawGizmosRect(
				rect.Clamp(panelRect),
				Color32.LerpUnclamped(Sky.SkyTintBottomColor, Sky.SkyTintTopColor, 0.5f)
			);
			// Content
			for (int j = unitD; j < unitU; j++) {
				pxRect.y = (rect.y + (j - unitD) * stepY).RoundToInt();
				if (!pxRect.Overlaps(panelRect)) continue;
				for (int i = unitL; i < unitR; i++) {
					var (lv, bg, en, _) = squad.GetAllBlocksAt(i, j, z);
					int id = en != 0 ? en : lv != 0 ? lv : bg;
					if (id == 0) continue;
					if (!Renderer.TryGetSpriteForGizmos(id, out var sprite)) continue;
					pxRect.x = (rect.x + (i - unitL) * stepX).RoundToInt();
					Game.DrawGizmosRect(pxRect, sprite.SummaryTint);
				}
			}
		}

	}


	// LGC
	private void LoadLocationSlotsFromFile () {
		LocationSlots.Clear();
		if (CurrentProject == null) return;
		string filePath = Util.CombinePaths(CurrentProject.Universe.BuiltInMapRoot, LOCATION_SLOT_FILE_NAME);
		if (!Util.FileExists(filePath)) return;
		using var stream = File.Open(filePath, FileMode.Open);
		using var reader = new BinaryReader(stream);
		while (reader.NotEnd()) {
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			int z = reader.ReadInt32();
			int w = reader.ReadInt32();
			int h = reader.ReadInt32();
			LocationSlots.Add((new IRect(x, y, w, h), z));
		}
	}


	private void SaveLocationSlotToFile () {
		if (CurrentProject == null) return;
		string filePath = Util.CombinePaths(CurrentProject.Universe.BuiltInMapRoot, LOCATION_SLOT_FILE_NAME);
		Util.CreateFolder(Util.GetParentPath(filePath));
		using var stream = File.Open(filePath, FileMode.Create);
		using var writer = new BinaryWriter(stream);
		foreach (var (rect, z) in LocationSlots) {
			writer.Write(rect.x);
			writer.Write(rect.y);
			writer.Write(z);
			writer.Write(rect.width);
			writer.Write(rect.height);
		}
	}


	private void ShowLocaltionSlotMenu (Int2 pos, int index) {
		GenericPopupUI.BeginPopup(pos);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteDialog, data: index);
		static void DeleteDialog () {
			if (GenericPopupUI.InvokingItemData is not int index) return;
			GenericDialogUI.SpawnDialog_Button(
				string.Format(MSG_DELETE_LOCATION, index),
				BuiltInText.UI_DELETE, Delete,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetCustomData(index);
			GenericDialogUI.SetItemTint(Color32.RED_BETTER);
		}
		static void Delete () {
			if (Instance == null) return;
			if (GenericDialogUI.InvokingData is not int index) return;
			Instance.LocationSlots.RemoveAt(index);
			Instance.LocationSlotDirty = true;
		}
	}


}
