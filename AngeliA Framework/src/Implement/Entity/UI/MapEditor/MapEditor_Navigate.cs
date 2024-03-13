using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;
public partial class MapEditor {




	#region --- SUB ---


	private class NavWorldSlot {
		public int WorldX;
		public int WorldY;
		public bool IsEmpty;
		public object Texture;
	}


	#endregion




	#region --- VAR ---


	// Const
	private const int NAV_WORLD_SIZE = 13;

	// Data
	private readonly NavWorldSlot[,] NavSlots = new NavWorldSlot[NAV_WORLD_SIZE + 1, NAV_WORLD_SIZE + 1];
	private Int2 NavPosition = default; // Global Pos, Screen Center
	private int NavLoadedSlotX = int.MinValue;
	private int NavLoadedSlotY = int.MinValue;
	private int NavLoadedSlotZ = int.MinValue;


	#endregion




	#region --- MSG ---


	[OnGameQuitting]
	internal static void OnGameQuitting_Nav () {
		var mapEditor = Stage.PeekOrGetEntity<MapEditor>();
		if (mapEditor != null) {
			int slotSize = NAV_WORLD_SIZE + 1;
			for (int i = 0; i < slotSize; i++) {
				for (int j = 0; j < slotSize; j++) {
					var slot = mapEditor.NavSlots[i, j];
					if (slot == null || slot.Texture == null) continue;
					Game.UnloadTexture(slot.Texture);
				}
			}
		}
	}


	private void Initialize_Nav () {
		// Create Slots
		int slotSize = NAV_WORLD_SIZE + 1;
		for (int i = 0; i < slotSize; i++) {
			for (int j = 0; j < slotSize; j++) {
				NavSlots[i, j] = new NavWorldSlot() {
					WorldX = int.MinValue + i,
					WorldY = int.MinValue + j,
					Texture = Game.GetTextureFromPixels(null, Const.MAP, Const.MAP),
					IsEmpty = true,
				};
			}
		}
	}


	private void Update_Move () {

		if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;
		var camerarect = Renderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);

		// View Z
		if (CtrlHolding) {
			if (Input.MouseWheelDelta > 0) {
				SetViewZ(CurrentZ + 1);
			}
			if (Input.MouseWheelDelta < 0) {
				SetViewZ(CurrentZ - 1);
			}
		}

		// Move with Mouse
		if (Input.MouseMidButtonHolding) {
			var totalRect = camerarect.Envelope(1, 1);
			int squadScale = NAV_WORLD_SIZE * Const.MAP * Const.CEL;
			NavPosition.x -= Input.MouseGlobalPositionDelta.x * squadScale / totalRect.height;
			NavPosition.y -= Input.MouseGlobalPositionDelta.y * squadScale / totalRect.height;
		}

		// Jump to Mouse
		if (Input.MouseLeftButtonDown && !CheckPointLaneRect.MouseInside() && !ToolbarRect.MouseInside()) {
			var totalRect = camerarect.Envelope(1, 1);
			int squadScale = NAV_WORLD_SIZE * Const.MAP * Const.CEL;
			var delta = Input.MouseGlobalPosition - camerarect.CenterInt();
			NavPosition.x += ((float)delta.x * squadScale / totalRect.height).RoundToInt();
			NavPosition.y += ((float)delta.y * squadScale / totalRect.height).RoundToInt();
			Input.UseAllHoldingKeys();
			SetNavigating(false);
		}

		// Move with Direction Keys
		if (!ShiftHolding && Input.Direction != Int2.zero) {
			int speed = Const.MAP * Const.CEL * 2 / NAV_WORLD_SIZE;
			NavPosition.x += Input.Direction.x * speed / 1000;
			NavPosition.y += Input.Direction.y * speed / 1000;
		}

		// Reset Camera
		if (CtrlHolding && Input.KeyboardDown(KeyboardKey.R)) {
			ResetCamera();
		}

		// Tab
		if (
			Input.KeyboardDown(KeyboardKey.Tab) ||
			Input.KeyboardUp(KeyboardKey.Escape) ||
			Input.KeyboardDown(KeyboardKey.Space) ||
			Input.KeyboardDown(KeyboardKey.Enter)
		) {
			SetNavigating(!IsNavigating);
			Input.UseKeyboardKey(KeyboardKey.Escape);
			Input.UseKeyboardKey(KeyboardKey.Tab);
			Input.UseKeyboardKey(KeyboardKey.Enter);
			Input.UseGameKey(Gamekey.Start);
			Input.UseGameKey(Gamekey.Select);
		}
		ControlHintUI.AddHint(KeyboardKey.Tab, BuiltInText.UI_CANCEL);

	}


	private void Update_NavMapTextureSlots () {

		// Sync
		int slotSize = NAV_WORLD_SIZE + 1;
		int targetWorldX = (NavPosition.x.ToUnit() - (slotSize * Const.MAP) / 2).UDivide(Const.MAP);
		int targetWorldY = (NavPosition.y.ToUnit() - (slotSize * Const.MAP) / 2).UDivide(Const.MAP);
		int headX = (-targetWorldX).UMod(slotSize);
		int headY = (-targetWorldY).UMod(slotSize);
		int z = CurrentZ;

		// Reload
		if (NavLoadedSlotZ != z || NavLoadedSlotX != targetWorldX || NavLoadedSlotY != targetWorldY) {
			string mapFolder = Stream.MapRoot;
			for (int j = 0; j < slotSize; j++) {
				for (int i = 0; i < slotSize; i++) {
					var slot = NavSlots[(i - headX).UMod(slotSize), (j - headY).UMod(slotSize)];
					int worldX = targetWorldX + i;
					int worldY = targetWorldY + j;
					if (NavLoadedSlotZ != z || slot.WorldX != worldX || slot.WorldY != worldY) {
						slot.WorldX = worldX;
						slot.WorldY = worldY;
						slot.IsEmpty = !World.LoadMapIntoTexture(mapFolder, worldX, worldY, z, slot.Texture);
					}
				}
			}
			NavLoadedSlotX = targetWorldX;
			NavLoadedSlotY = targetWorldY;
			NavLoadedSlotZ = z;
		}

		// Draw
		var camerarect = Renderer.CameraRect;
		const int MAP_GLOBAL_SIZE = Const.CEL * Const.MAP;
		var totalRect = camerarect.Shrink(PanelRect.width, 0, 0, 0).Envelope(1, 1);
		int slotRectWidth = totalRect.width / NAV_WORLD_SIZE;
		int slotRectHeight = totalRect.height / NAV_WORLD_SIZE;
		int globalOffsetX = -(NavPosition.x.UMod(MAP_GLOBAL_SIZE) * slotRectWidth / MAP_GLOBAL_SIZE);
		int globalOffsetY = -(NavPosition.y.UMod(MAP_GLOBAL_SIZE) * slotRectHeight / MAP_GLOBAL_SIZE);
		var navPanelRect = camerarect.EdgeInside(Direction4.Left, PanelRect.width);
		for (int j = 0; j < slotSize; j++) {
			for (int i = 0; i < slotSize; i++) {
				var slot = NavSlots[i, j];
				if (slot.IsEmpty || slot.Texture == null) continue;
				var rect = new IRect(
					globalOffsetX + totalRect.x + (slot.WorldX - targetWorldX) * slotRectWidth,
					globalOffsetY + totalRect.y + (slot.WorldY - targetWorldY) * slotRectHeight,
					slotRectWidth, slotRectHeight
				);
				if (rect.Overlaps(navPanelRect)) {
					if (rect.xMax > navPanelRect.xMax) {
						rect = rect.Shrink(navPanelRect.xMax - rect.xMin, 0, 0, 0);
						var uv = new FRect(1f - (float)rect.width / slotRectWidth, 0f, (float)rect.width / slotRectWidth, 1f);
						Game.DrawGizmosTexture(rect, uv, slot.Texture);
					}
				} else {
					Game.DrawGizmosTexture(rect, slot.Texture);
				}
			}
		}

	}


	private void Update_NavGizmos () {

		// Camera Rect
		var cameraRect = Renderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
		var totalRect = cameraRect.Envelope(1, 1);
		int width, height;

		if (cameraRect.width > cameraRect.height) {
			width = cameraRect.width * totalRect.width / (NAV_WORLD_SIZE * Const.MAP * Const.CEL);
			height = width * cameraRect.height / cameraRect.width;
		} else {
			height = cameraRect.height * totalRect.height / (NAV_WORLD_SIZE * Const.MAP * Const.CEL);
			width = height * cameraRect.width / cameraRect.height;
		}

		var rect = new IRect(
			cameraRect.x + cameraRect.width / 2 - width / 2,
			cameraRect.y + cameraRect.height / 2 - height / 2,
			width, height
		);

		int BORDER = Unify(1);
		Game.DrawGizmosFrame(rect, Color32.WHITE, BORDER);
		Game.DrawGizmosFrame(rect.Expand(BORDER), Color32.BLACK, BORDER);

	}


	#endregion




	#region --- LGC ---


	private void SetNavigating (bool navigating) {
		ApplyPaste();
		if (IsNavigating != navigating) {
			IsNavigating = navigating;
			if (navigating) {
				var cameraRect = Renderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
				Save();
				NavLoadedSlotZ = int.MinValue;
				NavPosition.x = cameraRect.CenterX() + Const.MAP * Const.HALF;
				NavPosition.y = cameraRect.CenterY() + Const.MAP * Const.HALF;
			} else {
				TargetViewRect.width = TargetViewRect.height * Const.VIEW_RATIO / 1000;
				TargetViewRect.x = NavPosition.x - (TargetViewRect.width + PanelRect.width) / 2 - Const.MAP * Const.HALF;
				TargetViewRect.y = NavPosition.y - TargetViewRect.height / 2 - Const.MAP * Const.HALF;
				SetViewZ(CurrentZ);
				Stage.SetViewPositionDelay(TargetViewRect.x, TargetViewRect.y, 1000, int.MaxValue);
				Stage.SetViewSizeDelay(TargetViewRect.height, 1000, int.MaxValue);
			}
		}
		MouseDownPosition = null;
		SelectionUnitRect = null;
		DraggingUnitRect = null;
		SearchingText = "";
		SearchResult.Clear();
	}


	#endregion




}