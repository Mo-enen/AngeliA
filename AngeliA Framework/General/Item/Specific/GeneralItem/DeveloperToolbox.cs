using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
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
		private static readonly int BTN_COL = "DeveloperToolbox.Collider".AngeHash();
		private static readonly int BTN_BOUND = "DeveloperToolbox.Bound".AngeHash();
		private static readonly int BTN_PROFILER = "DeveloperToolbox.Profiler".AngeHash();
		private static readonly int BTN_MAP = "DeveloperToolbox.Map".AngeHash();
		private static readonly int BTN_SHEET = "DeveloperToolbox.Sheet".AngeHash();
		private static readonly Byte4[] COLLIDER_TINTS = { Const.RED_BETTER, Const.ORANGE_BETTER, Const.YELLOW, Const.GREEN, Const.CYAN, Const.BLUE, Const.GREY_128, };

		// Data
		private static readonly BarData[] RenderingUsages = new BarData[RenderLayer.COUNT];
		private static readonly BarData[] EntityUsages = new BarData[EntityLayer.COUNT];
		private static BarData[] TextUsages = new BarData[0];
		private static readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private static int RequireToolboxFrame = int.MinValue;
		private static int RequireDataFrame = int.MinValue;
		private static int DrawColliderFrame = int.MinValue;
		private static int DrawBoundFrame = int.MinValue;
		private static bool ShowCollider = false;
		private static bool ShowBound = false;
		private static bool ShowProfiler = false;
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

			int panelWidth = CellRendererGUI.Unify(256);
			var panelRect = new IRect(CellRenderer.CameraRect.xMax - panelWidth, CellRenderer.CameraRect.yMax, panelWidth, 0);
			int panelYMax = panelRect.y;

			// Tool Buttons
			DrawToolButtons(ref panelRect);

			// Profiler
			if (ShowProfiler) {
				panelRect.height = 0;
				DrawProfiler(ref panelRect);
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
		public static void UpdateGizmos () {

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
					Game.DrawRect(rect, color);
				} else if (horizontal) {
					if (rect.x > PanelRect.x && rect.xMax < PanelRect.xMax) return;
					if (rect.x < PanelRect.x) {
						Game.DrawRect(new IRect(rect.x, rect.y, PanelRect.x - rect.x, rect.height), color);
					}
					if (rect.xMax > PanelRect.xMax) {
						Game.DrawRect(new IRect(PanelRect.xMax, rect.y, rect.xMax - PanelRect.xMax, rect.height), color);
					}
				} else {
					if (rect.y > PanelRect.y && rect.yMax < PanelRect.yMax) return;
					if (rect.y < PanelRect.y) {
						Game.DrawRect(new IRect(rect.x, rect.y, rect.width, PanelRect.y - rect.y), color);
					}
					if (rect.yMax > PanelRect.yMax) {
						Game.DrawRect(new IRect(rect.x, PanelRect.yMax, rect.width, rect.yMax - PanelRect.yMax), color);
					}
				}
			}


			static void DrawGizmosRect (IRect rect, Byte4 color) {
				if (!rect.Overlaps(PanelRect)) {
					Game.DrawRect(rect, color);
				} else {
					// Left Part
					if (rect.x < PanelRect.x) {
						Game.DrawRect(rect.Shrink(0, rect.xMax - PanelRect.x, 0, 0), color);
						rect = rect.Shrink(PanelRect.x - rect.x, 0, 0, 0);
					}
					// Right Part
					if (rect.xMax > PanelRect.xMax) {
						Game.DrawRect(rect.Shrink(PanelRect.xMax - rect.x, 0, 0, 0), color);
						rect = rect.Shrink(0, rect.xMax - PanelRect.xMax, 0, 0);
					}
					// Bottom Part
					if (rect.y < PanelRect.y) {
						Game.DrawRect(rect.Shrink(0, 0, 0, rect.yMax - PanelRect.y), color);
					}
					// Top Part
					if (rect.yMax > PanelRect.yMax) {
						Game.DrawRect(rect.Shrink(0, 0, PanelRect.yMax - rect.y, 0), color);
					}
				}
			}
		}


		public override void OnItemUpdate_FromInventory (Entity holder) {
			base.OnItemUpdate_FromInventory(holder);
			RequireToolboxFrame = Game.GlobalFrame;
		}


		private static void DrawToolButtons (ref IRect panelRect) {

			int buttonSize = CellRendererGUI.Unify(32);
			int padding = CellRendererGUI.Unify(6);
			panelRect.height = buttonSize + padding * 2;
			panelRect.y -= panelRect.height;

			var rect = new IRect(panelRect.xMax - buttonSize - padding, panelRect.y + padding, buttonSize, buttonSize);

			// Dodge FPS Label
			if (Game.ShowFPS) {
				rect.x -= CellRendererGUI.Unify(40);
			}

			// Collider
			if (ShowCollider) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(rect, BTN_COL, BTN_COL, BTN_COL, 0, 0, 0, int.MaxValue)) {
				ShowCollider = !ShowCollider;
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x -= rect.width + padding;

			// Bounds
			if (ShowBound) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(rect, BTN_BOUND, BTN_BOUND, BTN_BOUND, 0, 0, 0, int.MaxValue)) {
				ShowBound = !ShowBound;
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x -= rect.width + padding;

			// Profiler
			if (ShowProfiler) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(rect, BTN_PROFILER, BTN_PROFILER, BTN_PROFILER, 0, 0, 0, int.MaxValue)) {
				ShowProfiler = !ShowProfiler;
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x -= rect.width + padding;

			// Map
			if (MapEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(rect, BTN_MAP, BTN_MAP, BTN_MAP, 0, 0, 0, int.MaxValue)) {
				if (MapEditor.IsActived) {
					GlobalEditorUI.CloseEditorSmoothly();
				} else {
					GlobalEditorUI.OpenEditorSmoothly(MapEditor.TYPE_ID);
				}
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x -= rect.width + padding;

			// Sheet
			if (SheetEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(rect, BTN_SHEET, BTN_SHEET, BTN_SHEET, 0, 0, 0, int.MaxValue)) {
				if (SheetEditor.IsActived) {
					GlobalEditorUI.CloseEditorSmoothly();
				} else {
					GlobalEditorUI.OpenEditorSmoothly(SheetEditor.TYPE_ID);
				}
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x -= rect.width + padding;

			// Final
			if (ShowCollider) DrawColliderFrame = Game.GlobalFrame;
			if (ShowBound) DrawBoundFrame = Game.GlobalFrame;

		}


		private static void DrawProfiler (ref IRect panelRect) {

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


		#endregion




	}
}