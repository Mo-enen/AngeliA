using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireSpriteFromField]
	[RequireLanguageFromField]
	public class iDeveloperToolbox : Item {




		#region --- SUB ---


		private class BarData {
			public IntToChars I2C;
			public int Value;
			public int Capacity;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly SpriteCode[] BTN_SPRITES = {
			"DeveloperToolbox.Collider",
			"DeveloperToolbox.Bound",
			"DeveloperToolbox.Profiler",
			"DeveloperToolbox.Editor",
			"DeveloperToolbox.Effect",
		};
		private static readonly SpriteCode[] EDITOR_ICON = {
			"DeveloperToolbox.MapEditor",
			"DeveloperToolbox.SheetEditor",
			"DeveloperToolbox.LanguageEditor",
		};
		private static readonly LanguageCode[] EDITOR_LABEL = {
			"DeveloperToolbox.Label.MapEditor",
			"DeveloperToolbox.Label.SheetEditor",
			"DeveloperToolbox.Label.LanguageEditor",
		};
		private static readonly string[] EDITOR_LABEL_DEF = { "Map", "Sheet", "Language", };
		private static readonly int[] EDITOR_ID = {
			MapEditor.TYPE_ID, SheetEditor.TYPE_ID, LanguageEditor.TYPE_ID,
		};
		private static readonly Byte4[] COLLIDER_TINTS = { Const.RED_BETTER, Const.ORANGE_BETTER, Const.YELLOW, Const.GREEN, Const.CYAN, Const.BLUE, Const.GREY_128, };

		// Data
		private static readonly BarData[] RenderingUsages = new BarData[RenderLayer.COUNT];
		private static readonly BarData[] EntityUsages = new BarData[EntityLayer.COUNT];
		private static BarData[] TextUsages = new BarData[0];
		private static readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private static readonly bool[] EffectsEnabled = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
		private static int RequireToolboxFrame = int.MinValue;
		private static int RequireDataFrame = int.MinValue;
		private static int DrawColliderFrame = int.MinValue;
		private static int DrawBoundFrame = int.MinValue;
		private static int SelectingPanelIndex = -1;
		private static IRect PanelRect;


		#endregion




		#region --- MSG ---


		[OnGameInitializeLater(1024)]
		public static void OnGameInitialize () {
			TextUsages = new BarData[CellRenderer.TextLayerCount];
			for (int i = 0; i < RenderingUsages.Length; i++) {
				int capa = CellRenderer.GetLayerCapacity(i);
				RenderingUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{CellRenderer.GetLayerName(i)}  ", $" / {capa}"),
				};
			}
			for (int i = 0; i < TextUsages.Length; i++) {
				int capa = CellRenderer.GetTextLayerCapacity(i);
				TextUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{CellRenderer.GetTextLayerName(i)}  ", $" / {capa}"),
				};
			}
			for (int i = 0; i < EntityUsages.Length; i++) {
				int capa = Stage.Entities[i].Length;
				EntityUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{EntityLayer.LAYER_NAMES[i]}  ", $" / {capa}"),
				};
			}
		}


		[OnGameUpdateLater(-4097)]
		public static void DrawToolboxUI () {

			if (Game.GlobalFrame > RequireToolboxFrame + 1) return;

			CursorSystem.RequireCursor();
			int oldLayer = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();

			var panelRect = new IRect(CellRenderer.CameraRect.xMax, CellRenderer.CameraRect.yMax, 0, 0);
			int panelYMax = panelRect.y;

			// Tool Buttons
			int buttonSize = CellRendererGUI.Unify(32);
			int padding = CellRendererGUI.Unify(6);
			panelRect.height = buttonSize + padding * 2;
			panelRect.y -= panelRect.height;

			var rect = new IRect(panelRect.xMax - buttonSize - padding, panelRect.y + padding, buttonSize, buttonSize);

			// Dodge FPS Label
			if (Game.ShowFPS) rect.x -= CellRendererGUI.Unify(32);

			// Draw All Buttons
			for (int i = 0; i < BTN_SPRITES.Length; i++) {
				if (SelectingPanelIndex == i) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
				}
				int spriteCode = BTN_SPRITES[i];
				if (CellRendererGUI.Button(rect, spriteCode, spriteCode, spriteCode, 0, 0, 0, int.MaxValue)) {
					SelectingPanelIndex = SelectingPanelIndex != i ? i : -1;
					EffectsEnabled.FillWithValue(false);
					FrameInput.UseMouseKey(0);
					FrameInput.UseGameKey(Gamekey.Action);
				}
				CursorSystem.SetCursorAsHand(rect);
				rect.x -= i == BTN_SPRITES.Length - 1 ? padding : rect.width + padding;
			}
			panelRect.width = panelRect.x - rect.x;
			panelRect.x = rect.x;

			// Show Toolbox Panel
			switch (SelectingPanelIndex) {
				case 0:
					// Colliders
					DrawColliderFrame = Game.GlobalFrame;
					break;
				case 1:
					// Bounds
					DrawBoundFrame = Game.GlobalFrame;
					break;
				case 2:
					// Profiler
					DrawProfilerPanel(ref panelRect);
					break;
				case 3:
					// Editor
					DrawEditorPanel(ref panelRect);
					break;
				case 4:
					// Effect
					DrawEffectPanel(ref panelRect);
					break;
			}

			// BG
			PanelRect = new IRect(panelRect.x, panelRect.y, panelRect.width, panelYMax - panelRect.y);
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK, int.MaxValue - 16);

			// Finish
			CellRenderer.SetLayer(oldLayer);
			if (FrameInput.MouseLeftButton && PanelRect.MouseInside()) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}

		}


		[OnGameUpdateLater(4096)]
		public static void OnGameUpdateLater () {

			if (Game.GlobalFrame > RequireToolboxFrame + 1) return;

			// Collect Data
			if (Game.GlobalFrame <= RequireDataFrame + 1) {
				for (int i = 0; i < RenderLayer.COUNT; i++) {
					RenderingUsages[i].Value = CellRenderer.GetUsedCellCount(i);
				}
				for (int i = 0; i < CellRenderer.TextLayerCount; i++) {
					TextUsages[i].Value = CellRenderer.GetTextUsedCellCount(i);
				}
				for (int i = 0; i < EntityLayer.COUNT; i++) {
					EntityUsages[i].Value = Stage.EntityCounts[i];
				}
			}

			if (!PlayerMenuUI.ShowingUI) {
				// Draw Colliders
				if (Game.GlobalFrame <= DrawColliderFrame + 1) {
					// Init Cells
					if (CellPhysicsCells.Count == 0) {
						try {
							var layers = Util.GetStaticFieldValue(typeof(CellPhysics), "Layers") as System.Array;
							for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
								var layerObj = layers.GetValue(layerIndex);
								CellPhysicsCells.Add(Util.GetFieldValue(layerObj, "Cells") as PhysicsCell[,,]);
							}
						} catch (System.Exception ex) { Game.LogException(ex); }
						if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
					}
					// Draw Cells
					if (CellPhysicsCells.Count > 0 && CellPhysicsCells[0] != null) {
						var cameraRect = CellRenderer.CameraRect;
						int thick = CellRendererGUI.Unify(1);
						for (int layer = 0; layer < CellPhysicsCells.Count; layer++) {
							try {
								var tint = COLLIDER_TINTS[layer.Clamp(0, COLLIDER_TINTS.Length - 1)];
								var cells = CellPhysicsCells[layer];
								int cellWidth = cells.GetLength(0);
								int cellHeight = cells.GetLength(1);
								int celDepth = cells.GetLength(2);
								for (int y = 0; y < cellHeight; y++) {
									for (int x = 0; x < cellWidth; x++) {
										for (int d = 0; d < celDepth; d++) {
											var cell = cells[x, y, d];
											if (cell.Frame != CellPhysics.CurrentFrame) break;
											if (!cell.Rect.Overlaps(cameraRect)) continue;
											DrawGizmosRectAsLine(cell.Rect.EdgeInside(Direction4.Down, thick), tint, true);
											DrawGizmosRectAsLine(cell.Rect.EdgeInside(Direction4.Up, thick), tint, true);
											DrawGizmosRectAsLine(cell.Rect.EdgeInside(Direction4.Left, thick), tint, false);
											DrawGizmosRectAsLine(cell.Rect.EdgeInside(Direction4.Right, thick), tint, false);
										}
									}
								}
							} catch (System.Exception ex) { Game.LogException(ex); }
						}
					}
				}

				// Draw Bounds
				if (Game.GlobalFrame <= DrawBoundFrame + 1) {
					for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
						var entities = Stage.Entities[layer];
						int count = Stage.EntityCounts[layer];
						for (int i = 0; i < count; i++) {
							var e = entities[i];
							if (!e.Active) continue;
							DrawGizmosRect(e.GlobalBounds, Const.BLUE_BETTER.WithNewA(64));
						}
					}
				}
			}

			// Func
			static void DrawGizmosRectAsLine (IRect rect, Byte4 color, bool horizontal) {
				if (!rect.Overlaps(PanelRect)) {
					Game.DrawGizmosRect(rect, color);
				} else if (horizontal) {
					if (rect.x > PanelRect.x && rect.xMax < PanelRect.xMax) return;
					if (rect.x < PanelRect.x) {
						Game.DrawGizmosRect(new IRect(rect.x, rect.y, PanelRect.x - rect.x, rect.height), color);
					}
					if (rect.xMax > PanelRect.xMax) {
						Game.DrawGizmosRect(new IRect(PanelRect.xMax, rect.y, rect.xMax - PanelRect.xMax, rect.height), color);
					}
				} else {
					if (rect.y > PanelRect.y && rect.yMax < PanelRect.yMax) return;
					if (rect.y < PanelRect.y) {
						Game.DrawGizmosRect(new IRect(rect.x, rect.y, rect.width, PanelRect.y - rect.y), color);
					}
					if (rect.yMax > PanelRect.yMax) {
						Game.DrawGizmosRect(new IRect(rect.x, PanelRect.yMax, rect.width, rect.yMax - PanelRect.yMax), color);
					}
				}
			}


			static void DrawGizmosRect (IRect rect, Byte4 color) {
				if (!rect.Overlaps(PanelRect)) {
					Game.DrawGizmosRect(rect, color);
				} else {
					// Left Part
					if (rect.x < PanelRect.x) {
						Game.DrawGizmosRect(rect.Shrink(0, rect.xMax - PanelRect.x, 0, 0), color);
						rect = rect.Shrink(PanelRect.x - rect.x, 0, 0, 0);
					}
					// Right Part
					if (rect.xMax > PanelRect.xMax) {
						Game.DrawGizmosRect(rect.Shrink(PanelRect.xMax - rect.x, 0, 0, 0), color);
						rect = rect.Shrink(0, rect.xMax - PanelRect.xMax, 0, 0);
					}
					// Bottom Part
					if (rect.y < PanelRect.y) {
						Game.DrawGizmosRect(rect.Shrink(0, 0, 0, rect.yMax - PanelRect.y), color);
					}
					// Top Part
					if (rect.yMax > PanelRect.yMax) {
						Game.DrawGizmosRect(rect.Shrink(0, 0, PanelRect.yMax - rect.y, 0), color);
					}
				}
			}
		}


		public override void OnItemUpdate_FromInventory (Entity holder) {
			base.OnItemUpdate_FromInventory(holder);
			RequireToolboxFrame = Game.GlobalFrame;
		}


		private static void DrawProfilerPanel (ref IRect panelRect) {

			RequireDataFrame = Game.GlobalFrame;
			int barHeight = CellRendererGUI.Unify(24);
			panelRect.height = barHeight * (EntityUsages.Length + TextUsages.Length + RenderingUsages.Length);
			panelRect.y -= panelRect.height;
			int barPadding = CellRendererGUI.Unify(4);
			var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);

			// Entity
			for (int i = 0; i < EntityUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), EntityUsages[i], Const.CYAN);
				rect.y -= rect.height;
			}
			// Rendering
			for (int i = 0; i < RenderingUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), RenderingUsages[i], Const.GREEN);
				rect.y -= rect.height;
			}
			// Text
			for (int i = 0; i < TextUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), TextUsages[i], Const.GREEN);
				rect.y -= rect.height;
			}

			// Func
			static void DrawBar (IRect rect, BarData data, Byte4 barColor) {
				int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
				CellRenderer.Draw(Const.PIXEL, new IRect(rect.x, rect.y, width, rect.height), barColor, int.MaxValue);
				// Label
				int startIndex = CellRenderer.GetTextUsedCellCount();
				CellRendererGUI.Label(
					CellContent.Get(data.I2C.GetChars(data.Value), Const.GREY_230, 14, Alignment.MidMid), rect
				);
				if (CellRenderer.GetTextCells(out var cells, out int count)) {
					for (int i = startIndex; i < count && i < startIndex + data.I2C.Prefix.Length; i++) {
						cells[i].Color = new Byte4(96, 96, 96, 255);
					}
				}
			}
		}


		private static void DrawEditorPanel (ref IRect panelRect) {
			int editorCount = EDITOR_ICON.Length;
			int itemHeight = CellRendererGUI.Unify(48);
			int padding = CellRendererGUI.Unify(8);
			int buttonShrink = CellRendererGUI.Unify(10);
			int openingMarkSize = CellRendererGUI.Unify(12);
			panelRect.height = editorCount * (itemHeight + padding) + padding;
			panelRect.y -= panelRect.height;
			var rect = new IRect(panelRect.x, panelRect.yMax, panelRect.width, itemHeight);
			for (int i = 0; i < editorCount; i++) {

				rect.y -= itemHeight + padding;
				int editorID = EDITOR_ID[i];
				bool editorActived = GlobalEditorUI.HaveActiveInstance && GlobalEditorUI.Instance.TypeID == editorID;

				var buttonRect = rect.Shrink(buttonShrink, buttonShrink, buttonShrink / 2, buttonShrink / 2);

				CursorSystem.SetCursorAsHand(buttonRect);

				// Button
				if (CellRendererGUI.Button(
					buttonRect,
					BuiltInIcon.UI_BUTTON, BuiltInIcon.UI_BUTTON, BuiltInIcon.UI_BUTTON_DOWN,
					0, -1, 0, int.MaxValue - 9
				)) {
					if (editorActived) {
						GlobalEditorUI.CloseEditorSmoothly();
					} else {
						GlobalEditorUI.OpenEditorSmoothly(editorID);
					}
					FrameInput.UseMouseKey(0);
					FrameInput.UseGameKey(Gamekey.Action);
				}

				// Label
				CellRendererGUI.Label(
					CellContent.Get(
						EDITOR_LABEL[i].Get(EDITOR_LABEL_DEF[i]),
						tint: Const.GREY_32, charSize: 20, alignment: Alignment.MidMid
					), buttonRect, out var labelBounds
				);

				// Icon
				int iconSize = buttonRect.height;
				CellRenderer.Draw(
					EDITOR_ICON[i],
					new IRect(labelBounds.x - iconSize - iconSize / 5, buttonRect.y, iconSize, iconSize),
					int.MaxValue - 8
				);

				// Mark
				if (editorActived) {
					CellRenderer.Draw(
						BuiltInIcon.CIRCLE_16,
						buttonRect.EdgeInside(Direction4.Right, openingMarkSize).Fit(1, 1).Shift(-openingMarkSize, 0),
						Const.GREEN, int.MaxValue - 4
					);
				}

			}
		}


		private static void DrawEffectPanel (ref IRect panelRect) {
			int itemHeight = CellRendererGUI.Unify(28);
			int itemPadding = CellRendererGUI.Unify(8);
			panelRect.height = Const.SCREEN_EFFECT_COUNT * (itemHeight + itemPadding) + itemPadding;
			panelRect.y -= panelRect.height;
			var rect = new IRect(panelRect.x + itemPadding, panelRect.yMax, panelRect.width - itemPadding * 2, itemHeight);
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {

				rect.y -= itemHeight + itemPadding;

				// Label
				string name = Const.SCREEN_EFFECT_NAMES[i];
				CellRendererGUI.Label(CellContent.Get(
					name, Const.GREY_216, charSize: 18, alignment: Alignment.MidLeft
				), rect);

				// Enable Button
				var enableRect = rect.EdgeInside(Direction4.Right, itemHeight);
				bool enable = EffectsEnabled[i];
				if (CellRendererGUI.Button(
					enableRect,
					BuiltInIcon.CIRCLE_16, BuiltInIcon.CIRCLE_16, BuiltInIcon.CIRCLE_16,
					enable ? BuiltInIcon.CHECK_MARK_16 : 0,
					0, itemHeight / 5, z: int.MaxValue - 8,
					Const.GREY_32, Const.WHITE
				)) {
					EffectsEnabled[i] = !enable;
				}
				CursorSystem.SetCursorAsHand(enableRect);
			}

			// Update Values
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
				if (EffectsEnabled[i]) {
					Game.PassEffect(i);
				}
			}
			if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_DARKEN]) {
				Game.PassEffect_RetroDarken(Game.GlobalFrame.PingPong(60) / 60f);
			}
			if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_LIGHTEN]) {
				Game.PassEffect_RetroLighten(Game.GlobalFrame.PingPong(60) / 60f);
			}
			if (EffectsEnabled[Const.SCREEN_EFFECT_TINT]) {
				Game.PassEffect_Tint(Byte4.LerpUnclamped(new Byte4(255, 128, 196, 255), new Byte4(128, 255, 64, 255), Game.GlobalFrame.PingPong(120) / 120f));
			}

		}


		#endregion




	}
}