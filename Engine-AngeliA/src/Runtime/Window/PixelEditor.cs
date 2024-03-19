using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[RequireSpriteFromField]
public class PixelEditor : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_LEVEL_ATLAS = "Icon.LevelAtlas";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas {0}? All sprites inside will be delete too.");

	// Api
	public static PixelEditor Instance { get; private set; }
	public string SheetPath { get; private set; } = "";
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();
	private int CurrentAtlasIndex = 0;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasMenuTargetIndex = -1;
	private bool IsSheetDirty = false;
	private bool IsStageDirty = false;


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		SaveSheetToDisk();
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Cursor.RequireCursor();
		int panelWidth = Unify(240);
		Update_Panel(panelWidth);
		Update_Editor();
	}


	private void Update_Panel (int panelWidth) {

		const int INPUT_ID = 287234;
		var panelRect = WindowRect.EdgeInside(Direction4.Left, panelWidth);

		// BG
		Renderer.Draw(Const.PIXEL, panelRect, Color32.GREY_20);

		// Rename Hotkey
		if (Input.KeyboardDown(KeyboardKey.F2) && RenamingAtlasIndex < 0 && CurrentAtlasIndex >= 0) {
			RenamingAtlasIndex = CurrentAtlasIndex;
			GUI.StartTyping(INPUT_ID + CurrentAtlasIndex);
		}

		// --- Atlas ---
		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = Unify(12);
			int padding = Unify(4);
			int atlasPadding = Unify(4);
			CurrentAtlasIndex = CurrentAtlasIndex.Clamp(0, itemCount - 1);
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(32));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * (rect.height + atlasPadding) - panelRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;

			using (var scroll = GUIScope.Scroll(panelRect, AtlasPanelScrollY, 0, scrollMax)) {
				AtlasPanelScrollY = scroll.Position.y;
				for (int i = 0; i < itemCount; i++) {

					var atlas = Sheet.Atlas[i];
					bool selecting = CurrentAtlasIndex == i;
					bool renaming = RenamingAtlasIndex == i;
					bool hover = rect.MouseInside();
					if (renaming && !GUI.IsTyping) {
						RenamingAtlasIndex = -1;
						renaming = false;
					}

					// Button
					if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
						if (selecting) {
							GUI.CancelTyping();
							RenamingAtlasIndex = i;
							GUI.StartTyping(INPUT_ID + i);
						} else {
							newSelectingIndex = i;
							RenamingAtlasIndex = -1;
						}
					}

					// Selection Mark
					if (selecting) {
						Renderer.Draw(Const.PIXEL, rect, Color32.GREY_32);
					}

					// Icon
					GUI.Icon(rect.EdgeInside(Direction4.Left, rect.height), atlas.Type == AtlasType.General ? ICON_SPRITE_ATLAS : ICON_LEVEL_ATLAS);

					// Label
					if (renaming) {
						atlas.Name = GUI.InputField(
							INPUT_ID + i, rect.Shrink(rect.height + padding, 0, 0, 0),
							atlas.Name, out bool changed, out bool confirm, GUISkin.SmallInputField
						);
						if (changed || confirm) IsSheetDirty = true;
					} else {
						GUI.Label(rect.Shrink(rect.height + padding, 0, 0, 0), atlas.Name, GUISkin.SmallLabel);
					}

					// Right Click
					if (hover && Input.MouseRightButtonDown) {
						Input.UseAllMouseKey();
						ShowAtlasItemPopup(i);
					}

					// Next
					rect.y -= rect.height + atlasPadding;
				}
			}

			// Change Selection
			if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
				CurrentAtlasIndex = newSelectingIndex;
				if (IsStageDirty) SaveStageToSheet();
				LoadSheetToStage(newSelectingIndex);
			}

			// Scrollbar
			if (hasScrollbar) {
				var barRect = panelRect.EdgeInside(Direction4.Right, scrollbarWidth);
				AtlasPanelScrollY = GUI.ScrollBar(
					1256231, barRect,
					AtlasPanelScrollY, (itemCount + 6) * (rect.height + atlasPadding), panelRect.height
				);
			}

			// Right Click on Empty
			if (panelRect.MouseInside() && Input.MouseRightButtonDown) {
				Input.UseAllMouseKey();
				ShowAtlasItemPopup(-1);
			}


		}

	}


	private void Update_Editor () {

	}


	#endregion




	#region --- API ---


	public void LoadSheetFromDisk (string sheetPath) {
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath)) return;
		IsSheetDirty = false;
		IsStageDirty = false;
		Sheet.LoadFromDisk(sheetPath);
		LoadSheetToStage(CurrentAtlasIndex = 0);
	}


	public void SaveSheetToDisk (bool forceSave = false) {
		if (!forceSave && !IsSheetDirty) return;
		IsSheetDirty = false;
		if (string.IsNullOrEmpty(SheetPath)) return;
		if (IsStageDirty) SaveStageToSheet();
		Sheet.SaveToDisk(SheetPath);
	}


	#endregion




	#region --- LGC ---


	private void LoadSheetToStage (int atlasIndex) {

		IsStageDirty = false;

		// Clear Stage


		// Load New to Stage
		if (Sheet.Atlas == null || atlasIndex < 0 || atlasIndex >= Sheet.Atlas.Count) return;





	}


	private void SaveStageToSheet () {
		IsStageDirty = false;






	}


	private void ShowAtlasItemPopup (int atlasIndex) {

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup();

		if (atlasIndex >= 0) {
			// Delete
			GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteConfirm, enabled: Sheet.Atlas.Count > 1);
		}

		GenericPopupUI.AddSeparator();

		// For All
		GenericPopupUI.AddItem(BuiltInText.UI_ADD, Add);

		// Func
		static void Add () {
			Instance.Sheet.Atlas.Add(new Atlas() {
				AtlasZ = 0,
				Name = "New Atlas",
				Type = AtlasType.General,
			});
			Instance.IsSheetDirty = true;
			Instance.AtlasPanelScrollY = int.MaxValue;
		}
		static void DeleteConfirm () {
			var atlasList = Instance.Sheet.Atlas;
			int targetIndex = Instance.AtlasMenuTargetIndex;
			if (atlasList.Count <= 1) return;
			if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
			GenericDialogUI.SpawnDialog(
				string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
				BuiltInText.UI_DELETE, Delete,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.Instance.SetStyle(GUISkin.Message, GUISkin.Label, GUISkin.CenterLabel);
		}
		static void Delete () {
			var atlasList = Instance.Sheet.Atlas;
			if (atlasList.Count <= 1) return;
			int targetIndex = Instance.AtlasMenuTargetIndex;
			if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
			Instance.Sheet.RemoveAtlasWithAllSpritesInside(targetIndex);
			Instance.IsSheetDirty = true;
		}
	}


	#endregion




}
