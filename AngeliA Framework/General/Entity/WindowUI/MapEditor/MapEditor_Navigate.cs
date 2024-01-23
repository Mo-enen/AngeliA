using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
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
		private Int3 NavPosition = default; // Global Pos, Screen Center
		private int NavLoadedSlotX = int.MinValue;
		private int NavLoadedSlotY = int.MinValue;
		private int NavLoadedSlotZ = int.MinValue;


		#endregion




		#region --- MSG ---


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


		private void Update_NavHotkey () {

			if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;

			// View Z
			if (CtrlHolding) {
				if (FrameInput.MouseWheelDelta > 0) {
					NavPosition.z++;
				}
				if (FrameInput.MouseWheelDelta < 0) {
					NavPosition.z--;
				}
			}

			// Move
			if (FrameInput.AnyMouseButtonHolding) {
				int squadScale = (NAV_WORLD_SIZE - 1) * Const.MAP * Const.CEL;
				int cameraHeight = CellRenderer.CameraRect.height;
				NavPosition.x -= FrameInput.MouseGlobalPositionDelta.x * squadScale / cameraHeight;
				NavPosition.y -= FrameInput.MouseGlobalPositionDelta.y * squadScale / cameraHeight;
			} else if (!ShiftHolding && FrameInput.Direction != Int2.zero) {
				int speed = Const.MAP * Const.CEL * 2 / NAV_WORLD_SIZE;
				NavPosition.x += FrameInput.Direction.x * speed / 1000;
				NavPosition.y += FrameInput.Direction.y * speed / 1000;
			}

			// Reset Camera
			if (CtrlHolding && FrameInput.KeyboardDown(KeyboardKey.R)) {
				ResetCamera();
			}

			// Tab
			if (
				FrameInput.KeyboardDown(KeyboardKey.Tab) ||
				FrameInput.KeyboardUp(KeyboardKey.Escape) ||
				FrameInput.KeyboardDown(KeyboardKey.Space) ||
				FrameInput.KeyboardDown(KeyboardKey.Enter)
			) {
				SetNavigating(!IsNavigating);
				FrameInput.UseKeyboardKey(KeyboardKey.Escape);
				FrameInput.UseKeyboardKey(KeyboardKey.Tab);
				FrameInput.UseKeyboardKey(KeyboardKey.Enter);
				FrameInput.UseGameKey(Gamekey.Start);
				FrameInput.UseGameKey(Gamekey.Select);
			}
			ControlHintUI.AddHint(KeyboardKey.Tab, BuiltInText.UI_CANCEL.Get("Cancel"));

		}


		private void Update_NavMapTextureSlots () {

			// Sync
			int slotSize = NAV_WORLD_SIZE + 1;
			int targetWorldX = (NavPosition.x.ToUnit() - (slotSize * Const.MAP) / 2).UDivide(Const.MAP);
			int targetWorldY = (NavPosition.y.ToUnit() - (slotSize * Const.MAP) / 2).UDivide(Const.MAP);
			int headX = (-targetWorldX).UMod(slotSize);
			int headY = (-targetWorldY).UMod(slotSize);
			int z = NavPosition.z;

			// Reload
			if (NavLoadedSlotZ != z || NavLoadedSlotX != targetWorldX || NavLoadedSlotY != targetWorldY) {
				string mapFolder = WorldSquad.MapRoot;
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
			var totalRect = (CellRenderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0)).Envelope(1, 1);
			int slotRectWidth = totalRect.width / NAV_WORLD_SIZE;
			int slotRectHeight = totalRect.height / NAV_WORLD_SIZE;
			int globalOffsetX = -(NavPosition.x.UMod(Const.CEL * Const.MAP) * slotRectWidth / (Const.CEL * Const.MAP));
			int globalOffsetY = -(NavPosition.y.UMod(Const.CEL * Const.MAP) * slotRectHeight / (Const.CEL * Const.MAP));
			var navPanelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Left, PanelRect.width);
			for (int j = 0; j < slotSize; j++) {
				for (int i = 0; i < slotSize; i++) {
					var slot = NavSlots[i, j];
					if (slot.IsEmpty) continue;
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
			var cameraRect = CellRenderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
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
			Game.DrawGizmosFrame(rect, Const.WHITE, BORDER);
			Game.DrawGizmosFrame(rect.Expand(BORDER), Const.BLACK, BORDER);

			// Click Camera Rect
			bool hoverRect = rect.MouseInside();
			if (hoverRect) CursorSystem.SetCursorAsHand();
			if (hoverRect && FrameInput.MouseLeftButtonDown) {
				FrameInput.UseAllHoldingKeys();
				SetNavigating(false);
			}

		}


		#endregion




		#region --- LGC ---


		private void SetNavigating (bool navigating) {
			ApplyPaste();
			if (IsNavigating != navigating) {
				IsNavigating = navigating;
				if (navigating) {
					var cameraRect = CellRenderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
					Save();
					NavLoadedSlotZ = int.MinValue;
					NavPosition.x = cameraRect.CenterX() + Const.MAP * Const.HALF;
					NavPosition.y = cameraRect.CenterY() + Const.MAP * Const.HALF;
					NavPosition.z = Stage.ViewZ;
				} else {
					TargetViewRect.width = TargetViewRect.height * Const.VIEW_RATIO / 1000;
					TargetViewRect.x = NavPosition.x - (TargetViewRect.width + PanelRect.width) / 2 - Const.MAP * Const.HALF;
					TargetViewRect.y = NavPosition.y - TargetViewRect.height / 2 - Const.MAP * Const.HALF;
					Stage.SetViewZ(NavPosition.z);
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
}