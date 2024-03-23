using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
public class PixelEditor : WindowUI {




	#region --- SUB ---


	private class SpriteData {
		public AngeSprite Sprite;
		public bool PixelDirty;
	}


	#endregion




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private const int TOOLBAR_HEIGHT = 42;
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_LEVEL_ATLAS = "Icon.LevelAtlas";
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly SpriteCode ICON_SHOW_BG = "IconI.ShowBackground";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas {0}? All sprites inside will be delete too.");

	// Api
	public static PixelEditor Instance { get; private set; }
	protected override bool BlockEvent => true;

	// Short
	private bool PaintMode => PaintingDraggingRect != null || !Input.KeyboardHolding(KeyboardKey.LeftCtrl);
	private bool SliceMode => !PaintMode;

	// Data
	private readonly Sheet Sheet = new();
	private readonly List<SpriteData> StagedSprites = new();
	private string SheetPath = "";
	private int CurrentAtlasIndex = -1;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasMenuTargetIndex = -1;
	private int ZoomLevel = 1;
	private bool IsDirty = false;
	private FRect StageGlobalRect;
	private Color32 PaintingColor = Color32.CLEAR;
	private IRect? PaintingDraggingRect = null;
	private Int2? SliceDraggingStartPos = null;

	// Saving
	private static readonly SavingBool ShowBackground = new("PixEdt.ShowBG", true);


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		SaveSheetToDisk();
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sky.ForceSkyboxTint(new Color32(32, 33, 37, 255));
		Cursor.RequireCursor();
		int panelWidth = Unify(PANEL_WIDTH);
		var panelRect = WindowRect.EdgeInside(Direction4.Left, panelWidth);
		Update_Panel(panelRect);
		if (Sheet.Atlas.Count > 0) {
			var stageRect = WindowRect.Shrink(panelWidth, 0, 0, Unify(TOOLBAR_HEIGHT));
			Update_View(stageRect);
			Update_Toolbar(stageRect);
			Update_Editor(stageRect);
			Update_Stage(stageRect);
			Update_Gizmos(stageRect);
		}
	}


	private void Update_Panel (IRect panelRect) {

		const int INPUT_ID = 287234;

		// BG
		Renderer.DrawPixel(panelRect, Color32.GREY_20);

		// Rename Hotkey
		if (Input.KeyboardDown(KeyboardKey.F2) && RenamingAtlasIndex < 0 && CurrentAtlasIndex >= 0) {
			RenamingAtlasIndex = CurrentAtlasIndex;
			GUI.StartTyping(INPUT_ID + CurrentAtlasIndex);
		}

		// --- Bar ---
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, Unify(TOOLBAR_HEIGHT));
		panelRect = panelRect.Shrink(0, 0, 0, toolbarRect.height);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		int buttonPadding = Unify(4);
		var buttonRect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);
		if (GUI.Button(buttonRect, BuiltInSprite.ICON_PLUS, GUISkin.SmallDarkButton)) {
			AddAtlas();
		}
		buttonRect.SlideRight(buttonPadding);

		// --- Atlas ---
		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = Unify(12);
			int labelPadding = Unify(4);
			int atlasPadding = Unify(4);
			SetCurrentAtlas(CurrentAtlasIndex.Clamp(0, itemCount - 1));
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(32));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * (rect.height + atlasPadding) - panelRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;
			bool requireUseMouseButtons = false;

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
					if (!renaming && selecting) {
						Renderer.DrawPixel(rect, Color32.GREEN_DARK);
					}

					// Icon
					GUI.Icon(rect.EdgeInside(Direction4.Left, rect.height), atlas.Type == AtlasType.General ? ICON_SPRITE_ATLAS : ICON_LEVEL_ATLAS);

					// Label
					if (renaming) {
						atlas.Name = GUI.InputField(
							INPUT_ID + i, rect.Shrink(rect.height + labelPadding, 0, 0, 0),
							atlas.Name, out bool changed, out bool confirm, GUISkin.SmallInputField
						);
						if (changed || confirm) IsDirty = true;
					} else {
						GUI.Label(rect.Shrink(rect.height + labelPadding, 0, 0, 0), atlas.Name, GUISkin.SmallLabel);
					}

					// Right Click
					if (hover && Input.MouseRightButtonDown) {
						requireUseMouseButtons = true;
						ShowAtlasItemPopup(i);
					}

					// Next
					rect.SlideDown(atlasPadding);
				}
			}

			if (requireUseMouseButtons) Input.UseAllMouseKey();

			// Change Selection
			if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
				SetCurrentAtlas(newSelectingIndex);
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


	private void Update_View (IRect stageRect) {

		// Move
		if (
			(Input.MouseMidButtonHolding && stageRect.Contains(Input.MouseMidDownGlobalPosition)) ||
			(Input.MouseLeftButtonHolding && Input.KeyboardHolding(KeyboardKey.Space) && stageRect.Contains(Input.MouseLeftDownGlobalPosition))
		) {
			var delta = Input.MouseGlobalPositionDelta;
			StageGlobalRect = StageGlobalRect.Shift(delta.x, delta.y);
		}

		// Zoom
		if (stageRect.MouseInside() && Input.MouseWheelDelta != 0) {
			ZoomLevel = (ZoomLevel + Input.MouseWheelDelta).Clamp(1, 32);
			var mousePos = Input.MouseGlobalPosition;
			var fittedStage = stageRect.Fit(1, 1);
			StageGlobalRect = StageGlobalRect.ResizeFrom(
				fittedStage.width * ZoomLevel,
				fittedStage.height * ZoomLevel,
				mousePos.x, mousePos.y
			);
		}

	}


	private void Update_Toolbar (IRect stageRect) {
		int toolbarButtonPadding = Unify(4);
		var toolbarRect = stageRect.EdgeOutside(Direction4.Up, Unify(TOOLBAR_HEIGHT));
		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_20);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var toolbarBtnRect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);
		// Show BG
		ShowBackground.Value = GUI.ToggleButton(toolbarBtnRect, ShowBackground.Value, ICON_SHOW_BG, GUISkin.SmallDarkButton);
		toolbarBtnRect.SlideRight(toolbarButtonPadding);

	}


	private void Update_Editor (IRect stageRect) {








	}


	private void Update_Stage (IRect stageRect) {

		var stageRectInt = StageGlobalRect.ToIRect();

		// Checker Board / BG
		using (GUIScope.Layer(RenderLayer.DEFAULT)) {
			if (Renderer.TryGetSprite(ShowBackground.Value ? Const.PIXEL : UI_CHECKER_BOARD, out var checkerSprite)) {
				var tint = ShowBackground.Value ? new Color32(34, 47, 64, 255) : Color32.WHITE;
				const int CHECKER_COUNT = STAGE_SIZE / 32;
				int sizeX = stageRectInt.width / CHECKER_COUNT;
				int sizeY = stageRectInt.height / CHECKER_COUNT;
				for (int x = 0; x < CHECKER_COUNT; x++) {
					int globalX = x * sizeX + stageRectInt.x;
					if (globalX < stageRect.x - sizeX) continue;
					if (globalX > stageRect.xMax) break;
					for (int y = 0; y < CHECKER_COUNT; y++) {
						int globalY = y * sizeY + stageRectInt.y;
						if (globalY < stageRect.y - sizeY) continue;
						if (globalY > stageRect.yMax) break;
						Renderer.Draw(checkerSprite, globalX, globalY, 0, 0, 0, sizeX, sizeY, tint, z: 0);
					}
				}
			}
		}

		// Sprites
		foreach (var spriteData in StagedSprites) {
			var sprite = spriteData.Sprite;
			// Sync Texture
			if (spriteData.PixelDirty) {
				spriteData.PixelDirty = false;
				Sheet.SyncSpritePixelsIntoTexturePool(sprite);
			}
			// Render
			if (!Sheet.TexturePool.TryGetValue(sprite.ID, out var texture)) continue;
			var rect = Pixel_to_Stage(stageRect, sprite.PixelRect, out var uv);
			if (rect.HasValue) {
				if (uv.HasValue) {
					Game.DrawGizmosTexture(rect.Value.Clamp(stageRect), uv.Value, texture);
				} else {
					Game.DrawGizmosTexture(rect.Value, texture);
				}
			}
		}

	}


	private void Update_Gizmos (IRect stageRect) {

		if (Input.IgnoringMouseInput) return;

		int thickness = Unify(1);
		var mousePos = Input.MouseGlobalPosition;

		// Slice Gizmos
		if (SliceMode) {
			bool highlighted = false;
			bool sliceDragging = SliceDraggingStartPos != null;
			for (int i = StagedSprites.Count - 1; i >= 0; i--) {
				var spData = StagedSprites[i];
				var rect = Pixel_to_Stage(stageRect, spData.Sprite.PixelRect, out var uv);
				if (rect.HasValue) {
					// Mouse Inside
					if (rect.Value.MouseInside()) {
						// Cursor
						Cursor.SetCursorAsMove();
						// Highlight
						if (!highlighted && !sliceDragging) {
							highlighted = true;
							Game.DrawGizmosRect(rect.Value, Color32.BLUE_BETTER.WithNewA(32));
						}
					}
					// Frame
					if (uv.HasValue) {
						Game.DrawGizmosFrame(rect.Value, Color32.BLUE_BETTER, Int4.Direction(
							uv.Value.xMin.Almost(0f) ? thickness : 0,
							uv.Value.xMax.Almost(1f) ? thickness : 0,
							uv.Value.yMin.Almost(0f) ? thickness : 0,
							uv.Value.yMax.Almost(1f) ? thickness : 0
						));
					} else {
						Game.DrawGizmosFrame(rect.Value, Color32.BLUE_BETTER, thickness);
					}
				}
			}
		}

		// Mouse Cursor
		if (!SliceMode) {
			if (stageRect.MouseInside()) {
				float pixWidth = Util.Max(StageGlobalRect.width, 1f) / STAGE_SIZE;
				float pixHeight = Util.Max(StageGlobalRect.height, 1f) / STAGE_SIZE;
				var cursorRect = new FRect(
					(mousePos.x - StageGlobalRect.x).UFloor(pixWidth) + StageGlobalRect.x,
					(mousePos.y - StageGlobalRect.y).UFloor(pixHeight) + StageGlobalRect.y,
					pixWidth, pixHeight
				).ToIRect();
				if (PaintingColor == Color32.CLEAR) {
					// Empty
					Game.DrawGizmosFrame(cursorRect, Color32.WHITE, thickness);
					Game.DrawGizmosFrame(cursorRect.Expand(thickness), Color32.BLACK, thickness);
				} else {
					// Color
					Game.DrawGizmosRect(cursorRect, PaintingColor);
				}
			}
		}

	}


	#endregion




	#region --- API ---


	public void LoadSheetFromDisk (string sheetPath) {
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath)) return;
		IsDirty = false;
		CurrentAtlasIndex = -1;
		PaintingDraggingRect = null;
		PaintingColor = Color32.CLEAR;
		Sheet.LoadFromDisk(sheetPath);
	}


	public void SaveSheetToDisk (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		IsDirty = false;
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sheet.SaveToDisk(SheetPath);
	}


	#endregion




	#region --- LGC ---


	private void ShowAtlasItemPopup (int atlasIndex) {

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup();

		// Delete
		if (atlasIndex >= 0) {
			GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteAtlasConfirm, enabled: Sheet.Atlas.Count > 1);
		}

		// Add
		GenericPopupUI.AddItem(BuiltInText.UI_ADD, AddAtlas);

		// Func


	}


	private void SetCurrentAtlas (int atlasIndex) {
		if (CurrentAtlasIndex == atlasIndex) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		foreach (var sprite in Sheet.Sprites) {
			if (sprite.AtlasIndex != atlasIndex) continue;
			StagedSprites.Add(new SpriteData() {
				Sprite = sprite,
				PixelDirty = false,
			});
		}
		StageGlobalRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, Unify(TOOLBAR_HEIGHT)).Fit(1, 1).ToFRect();
		StageGlobalRect.width = Util.Max(StageGlobalRect.width, 1f);
		StageGlobalRect.height = Util.Max(StageGlobalRect.height, 1f);
		ZoomLevel = 1;
		PaintingColor = Color32.CLEAR;
	}


	private static void AddAtlas () {
		Instance.Sheet.Atlas.Add(new Atlas() {
			AtlasZ = 0,
			Name = "New Atlas",
			Type = AtlasType.General,
		});
		Instance.IsDirty = true;
		Instance.AtlasPanelScrollY = int.MaxValue;
		Instance.SetCurrentAtlas(Instance.Sheet.Atlas.Count - 1);
	}


	private static void DeleteAtlasConfirm () {
		var atlasList = Instance.Sheet.Atlas;
		int targetIndex = Instance.AtlasMenuTargetIndex;
		if (atlasList.Count <= 1) return;
		if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
		GenericDialogUI.SpawnDialog_Button(
			string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
			BuiltInText.UI_DELETE, DeleteAtlas,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetButtonTint(Color32.RED_BETTER);
	}


	private static void DeleteAtlas () {
		var atlasList = Instance.Sheet.Atlas;
		if (atlasList.Count <= 1) return;
		int targetIndex = Instance.AtlasMenuTargetIndex;
		if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
		Instance.Sheet.RemoveAtlasAndAllSpritesInside(targetIndex);
		Instance.IsDirty = true;
		Instance.SetCurrentAtlas(Instance.CurrentAtlasIndex);
	}


	// Util
	private IRect? Pixel_to_Stage (IRect stageRect, IRect pixRect) => Pixel_to_Stage(stageRect, pixRect, out _);
	private IRect? Pixel_to_Stage (IRect stageRect, IRect pixRect, out FRect? uv) {
		uv = null;
		var stageRectInt = StageGlobalRect.ToIRect();
		var rect = new IRect(
			stageRectInt.x + pixRect.x * stageRectInt.width / STAGE_SIZE,
			stageRectInt.y + pixRect.y * stageRectInt.height / STAGE_SIZE,
			stageRectInt.width * pixRect.width / STAGE_SIZE,
			stageRectInt.height * pixRect.height / STAGE_SIZE
		);
		if (rect.CompleteInside(stageRect)) {
			return rect;
		} else if (rect.Overlaps(stageRect)) {
			uv = FRect.MinMaxRect(
				Util.InverseLerpUnclamped(rect.xMin, rect.xMax, Util.Max(stageRect.xMin, rect.xMin)),
				Util.InverseLerpUnclamped(rect.yMin, rect.yMax, Util.Max(stageRect.yMin, rect.yMin)),
				Util.InverseLerpUnclamped(rect.xMin, rect.xMax, Util.Min(stageRect.xMax, rect.xMax)),
				Util.InverseLerpUnclamped(rect.yMin, rect.yMax, Util.Min(stageRect.yMax, rect.yMax))
			);
			return rect.Clamp(stageRect);
		}
		return null;
	}


	#endregion




}
