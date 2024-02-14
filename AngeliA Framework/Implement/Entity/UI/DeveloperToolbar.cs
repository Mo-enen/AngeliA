using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework {
	[RequireSpriteFromField]
	public static class DeveloperToolbar {




		#region --- SUB ---


		private class BarData {
			public IntToChars I2C;
			public int Value;
			public int Capacity;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly Byte4[] COLLIDER_TINTS = { Const.RED_BETTER, Const.ORANGE_BETTER, Const.YELLOW, Const.GREEN, Const.CYAN, Const.BLUE, Const.GREY_128, };
		private static readonly SpriteCode BTN_COLLIDER = "DeveloperToolbox.Collider";
		private static readonly SpriteCode BTN_BOUND = "DeveloperToolbox.Bound";
		private static readonly SpriteCode BTN_PROFILER = "DeveloperToolbox.Profiler";
		private static readonly SpriteCode BTN_EFFECT = "DeveloperToolbox.Effect";
		private static readonly SpriteCode BTN_MAP = "DeveloperToolbox.MapEditor";
		private static readonly SpriteCode BTN_LANGUAGE = "DeveloperToolbox.Language";

		// Data
		private static readonly BarData[] RenderingUsages = new BarData[RenderLayer.COUNT];
		private static readonly BarData[] EntityUsages = new BarData[EntityLayer.COUNT];
		private static BarData[] TextUsages = new BarData[0];
		private static readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private static readonly bool[] EffectsEnabled = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
		private static IRect PanelRect;
		private static bool DrawCollider = false;
		private static bool DrawBounds = false;
		private static bool ProfilerPanelOpening = false;
		private static bool EffectPanelOpening = false;


		#endregion




		#region --- MSG ---


		[OnGameInitializeLater]
		internal static void OnGameInitializeLater () {
			if (!Game.IsEdittime) return;
			for (int i = 0; i < RenderingUsages.Length; i++) {
				int capa = CellRenderer.GetLayerCapacity(i);
				RenderingUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{CellRenderer.GetLayerName(i)}  ", $" / {capa}"),
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


		[OnProjectOpen]
		internal static void OnProjectOpen () {
			if (!Game.IsEdittime) return;
			TextUsages = new BarData[CellRenderer.TextLayerCount];
			for (int i = 0; i < TextUsages.Length; i++) {
				int capa = CellRenderer.GetTextLayerCapacity(i);
				TextUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{CellRenderer.GetTextLayerName(i)}  ", $" / {capa}"),
				};
			}
		}


		[OnGameUpdateLater(-4097)]
		internal static void UpdateToolbar () {
			if (!Game.IsEdittime) return;
			
			CursorSystem.RequireCursor();
			int oldLayer = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();

			var panelRect = new IRect(CellRenderer.CameraRect.xMax, CellRenderer.CameraRect.yMax, 0, 0);
			int panelYMax = panelRect.y;

			// Tool Buttons
			int buttonSize = CellGUI.Unify(36);
			int padding = CellGUI.Unify(6);
			panelRect.height = buttonSize + padding * 2;
			panelRect.y -= panelRect.height;

			var rect = new IRect(
				panelRect.xMax - buttonSize - padding, 
				panelRect.y + padding,
				buttonSize, buttonSize
			);

			if (Game.ShowFPS) rect.x -= CellGUI.Unify(32);

			// Collider Btn
			if (DrawCollider) CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			if (CellGUI.Button(rect, BTN_COLLIDER, BTN_COLLIDER, BTN_COLLIDER, 0, 0, 0, int.MaxValue)) {
				DrawCollider = !DrawCollider;
			}
			rect.x -= rect.width + padding;

			// Bound Btn
			if (DrawBounds) CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			if (CellGUI.Button(rect, BTN_BOUND, BTN_BOUND, BTN_BOUND, 0, 0, 0, int.MaxValue)) {
				DrawBounds = !DrawBounds;
			}
			rect.x -= rect.width + padding;

			// Profiler Btn
			if (ProfilerPanelOpening) CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			if (CellGUI.Button(rect, BTN_PROFILER, BTN_PROFILER, BTN_PROFILER, 0, 0, 0, int.MaxValue)) {
				ProfilerPanelOpening = !ProfilerPanelOpening;
			}
			rect.x -= rect.width + padding;

			// Effect Btn
			if (EffectPanelOpening) CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			if (CellGUI.Button(rect, BTN_EFFECT, BTN_EFFECT, BTN_EFFECT, 0, 0, 0, int.MaxValue)) {
				EffectPanelOpening = !EffectPanelOpening;
				EffectsEnabled.FillWithValue(false);
			}
			rect.x -= rect.width + padding;

			// Map Editor Btn
			if (MapEditor.IsActived) CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			if (CellGUI.Button(rect, BTN_MAP, BTN_MAP, BTN_MAP, 0, 0, 0, int.MaxValue)) {
				if (MapEditor.IsActived) {
					WindowUI.CloseWindow(MapEditor.TYPE_ID);
					Game.RestartGame();
				} else {
					WindowUI.OpenWindow(MapEditor.TYPE_ID);
				}
			}
			rect.x -= rect.width + padding;

			// Language Editor Btn
			if (LanguageEditor.IsActived) CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
			if (CellGUI.Button(rect, BTN_LANGUAGE, BTN_LANGUAGE, BTN_LANGUAGE, 0, 0, 0, int.MaxValue)) {
				if (LanguageEditor.IsActived) {
					WindowUI.CloseWindow(LanguageEditor.TYPE_ID);
				} else {
					WindowUI.OpenWindow(LanguageEditor.TYPE_ID);
				}
			}
			rect.x -= padding;
			panelRect.width = panelRect.x - rect.x;
			panelRect.x = rect.x;

			// Draw Panels
			if (ProfilerPanelOpening) DrawProfilerPanel(ref panelRect);
			if (EffectPanelOpening) DrawEffectPanel(ref panelRect);

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
		internal static void CollectProfilerData () {
			if (!Game.IsEdittime) return;
			if (!ProfilerPanelOpening) return;
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


		[OnGameUpdateLater(4096)]
		internal static void UpdateGizmos () {

			if (!Game.IsEdittime) return;
			if (PlayerMenuUI.ShowingUI) return;

			// Draw Colliders
			if (DrawCollider) {
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
					int thick = CellGUI.Unify(1);
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
										DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Down, thick), tint, true);
										DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Up, thick), tint, true);
										DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Left, thick), tint, false);
										DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Right, thick), tint, false);
									}
								}
							}
						} catch (System.Exception ex) { Game.LogException(ex); }
					}
				}
			}

			// Draw Bounds
			if (DrawBounds) {
				int thick = CellGUI.Unify(1);
				for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
					var entities = Stage.Entities[layer];
					int count = Stage.EntityCounts[layer];
					for (int i = 0; i < count; i++) {
						var e = entities[i];
						if (!e.Active) continue;
						DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Down, thick), Const.CYAN_BETTER, true);
						DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Up, thick), Const.CYAN_BETTER, true);
						DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Left, thick), Const.CYAN_BETTER, false);
						DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Right, thick), Const.CYAN_BETTER, false);
					}
				}
			}

			// Func
			static void DrawGizmosRectAsLine (IRect panelRect, IRect rect, Byte4 color, bool horizontal) {
				if (!rect.Overlaps(panelRect)) {
					Game.DrawGizmosRect(rect, color);
				} else if (horizontal) {
					if (rect.x > panelRect.x && rect.xMax < panelRect.xMax) return;
					if (rect.x < panelRect.x) {
						Game.DrawGizmosRect(new IRect(rect.x, rect.y, panelRect.x - rect.x, rect.height), color);
					}
					if (rect.xMax > panelRect.xMax) {
						Game.DrawGizmosRect(new IRect(panelRect.xMax, rect.y, rect.xMax - panelRect.xMax, rect.height), color);
					}
				} else {
					if (rect.y > panelRect.y && rect.yMax < panelRect.yMax) return;
					if (rect.y < panelRect.y) {
						Game.DrawGizmosRect(new IRect(rect.x, rect.y, rect.width, panelRect.y - rect.y), color);
					}
					if (rect.yMax > panelRect.yMax) {
						Game.DrawGizmosRect(new IRect(rect.x, panelRect.yMax, rect.width, rect.yMax - panelRect.yMax), color);
					}
				}
			}
		}


		#endregion




		#region --- LGC ---


		// Panel
		private static void DrawProfilerPanel (ref IRect panelRect) {
			
			int barHeight = CellGUI.Unify(24);
			panelRect.height = barHeight * (EntityUsages.Length + TextUsages.Length + RenderingUsages.Length);
			panelRect.y -= panelRect.height;
			int barPadding = CellGUI.Unify(4);
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
				CellGUI.Label(
					CellContent.Get(data.I2C.GetChars(data.Value), Const.GREY_230, 14, Alignment.MidMid), rect
				);
				if (CellRenderer.GetTextCells(out var cells, out int count)) {
					for (int i = startIndex; i < count && i < startIndex + data.I2C.Prefix.Length; i++) {
						cells[i].Color = new Byte4(96, 96, 96, 255);
					}
				}
			}
		}


		private static void DrawEffectPanel (ref IRect panelRect) {
			int itemHeight = CellGUI.Unify(28);
			int itemPadding = CellGUI.Unify(8);
			panelRect.height = Const.SCREEN_EFFECT_COUNT * (itemHeight + itemPadding) + itemPadding;
			panelRect.y -= panelRect.height;
			var rect = new IRect(panelRect.x + itemPadding, panelRect.yMax, panelRect.width - itemPadding * 2, itemHeight);
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {

				rect.y -= itemHeight + itemPadding;

				// Label
				string name = Const.SCREEN_EFFECT_NAMES[i];
				CellGUI.Label(CellContent.Get(
					name, Const.GREY_216, charSize: 18, alignment: Alignment.MidLeft
				), rect);

				// Enable Button
				var enableRect = rect.EdgeInside(Direction4.Right, itemHeight);
				bool enable = EffectsEnabled[i];
				if (CellGUI.Button(
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
			if (EffectsEnabled[Const.SCREEN_EFFECT_VIGNETTE]) {
				Game.PassEffect_Vignette(0.95f, 0.6f, 0f, 0f, 0f);
			}
		}


		#endregion




	}
}
