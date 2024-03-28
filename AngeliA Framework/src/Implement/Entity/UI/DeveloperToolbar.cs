using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
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
	private static readonly Color32[] COLLIDER_TINTS = { Color32.RED_BETTER, Color32.ORANGE_BETTER, Color32.YELLOW, Color32.GREEN, Color32.CYAN, Color32.BLUE, Color32.GREY_128, };
	private static readonly SpriteCode BTN_COLLIDER = "DeveloperToolbox.Collider";
	private static readonly SpriteCode BTN_BOUND = "DeveloperToolbox.Bound";
	private static readonly SpriteCode BTN_PROFILER = "DeveloperToolbox.Profiler";
	private static readonly SpriteCode BTN_EFFECT = "DeveloperToolbox.Effect";
	private static readonly SpriteCode BTN_MAP = "DeveloperToolbox.MapEditor";

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
	private static bool Enable = false;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		Enable = Game.IsEdittime && Game.ProjectType == ProjectType.Game;
		if (!Enable) return;
		for (int i = 0; i < RenderingUsages.Length; i++) {
			int capa = Renderer.GetLayerCapacity(i);
			RenderingUsages[i] = new BarData() {
				Capacity = capa,
				I2C = new IntToChars($"{Renderer.GetLayerName(i)}  ", $" / {capa}"),
			};
		}
		for (int i = 0; i < EntityUsages.Length; i++) {
			int capa = Stage.Entities[i].Length;
			EntityUsages[i] = new BarData() {
				Capacity = capa,
				I2C = new IntToChars($"{EntityLayer.LAYER_NAMES[i]}  ", $" / {capa}"),
			};
		}
		TextUsages = new BarData[Renderer.TextLayerCount];
		for (int i = 0; i < TextUsages.Length; i++) {
			int capa = Renderer.GetTextLayerCapacity(i);
			TextUsages[i] = new BarData() {
				Capacity = capa,
				I2C = new IntToChars($"{Renderer.GetTextLayerName(i)}  ", $" / {capa}"),
			};
		}
	}


	[OnGameUpdateLater(-4097)]
	internal static void UpdateToolbar () {

		if (!Enable) return;

		Cursor.RequireCursor();

		using var _ = Scope.RendererLayerUI();

		var panelRect = new IRect(Renderer.CameraRect.xMax, Renderer.CameraRect.yMax, 0, 0);
		int panelYMax = panelRect.y;

		// BG
		var bgCell = Renderer.DrawPixel(default, Color32.BLACK);

		// Tool Buttons
		int buttonSize = GUI.Unify(36);
		int padding = GUI.Unify(6);
		panelRect.height = buttonSize + padding * 2;
		panelRect.y -= panelRect.height;

		var rect = new IRect(
			panelRect.xMax - buttonSize - padding,
			panelRect.y + padding,
			buttonSize, buttonSize
		);

		// Collider Btn
		DrawCollider = GUI.IconToggle(rect, DrawCollider, BTN_COLLIDER);
		rect.x -= rect.width + padding;

		// Bound Btn
		DrawBounds = GUI.IconToggle(rect, DrawBounds, BTN_BOUND);
		rect.x -= rect.width + padding;

		// Profiler Btn
		ProfilerPanelOpening = GUI.IconToggle(rect, ProfilerPanelOpening, BTN_PROFILER);
		rect.x -= rect.width + padding;

		// Effect Btn
		EffectPanelOpening = GUI.IconToggle(rect, EffectPanelOpening, BTN_EFFECT);
		rect.x -= rect.width + padding;

		// Map Editor Btn
		bool isMapEditorActived = MapEditor.IsActived;
		bool newIsOn = GUI.IconToggle(rect, isMapEditorActived, BTN_MAP);
		if (newIsOn != isMapEditorActived) {
			if (isMapEditorActived) {
				WindowUI.CloseWindow(MapEditor.TYPE_ID);
				Game.RestartGame();
			} else {
				WindowUI.OpenWindow(MapEditor.TYPE_ID);
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
		bgCell.X = PanelRect.x;
		bgCell.Y = PanelRect.y;
		bgCell.Width = PanelRect.width;
		bgCell.Height = PanelRect.height;

		// Finish
		if (Input.MouseLeftButtonHolding && PanelRect.MouseInside()) {
			Input.UseMouseKey(0);
			Input.UseGameKey(Gamekey.Action);
		}

	}


	[OnGameUpdateLater(4096)]
	internal static void CollectProfilerData () {
		if (!Enable) return;
		if (!ProfilerPanelOpening) return;
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			RenderingUsages[i].Value = Renderer.GetUsedCellCount(i);
		}
		for (int i = 0; i < Renderer.TextLayerCount; i++) {
			TextUsages[i].Value = Renderer.GetTextUsedCellCount(i);
		}
		for (int i = 0; i < EntityLayer.COUNT; i++) {
			EntityUsages[i].Value = Stage.EntityCounts[i];
		}
	}


	[OnGameUpdateLater(4096)]
	internal static void UpdateGizmos () {

		if (!Enable) return;
		if (PlayerMenuUI.ShowingUI) return;

		// Draw Colliders
		if (DrawCollider) {
			// Init Cells
			if (CellPhysicsCells.Count == 0) {
				try {
					var layers = Util.GetStaticFieldValue(typeof(Physics), "Layers") as System.Array;
					for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
						var layerObj = layers.GetValue(layerIndex);
						CellPhysicsCells.Add(Util.GetFieldValue(layerObj, "Cells") as PhysicsCell[,,]);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
				if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
			}
			// Draw Cells
			if (CellPhysicsCells.Count > 0 && CellPhysicsCells[0] != null) {
				var cameraRect = Renderer.CameraRect;
				int thick = GUI.Unify(1);
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
									if (cell.Frame != Physics.CurrentFrame) break;
									if (!cell.Rect.Overlaps(cameraRect)) continue;
									DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Down, thick), tint, true);
									DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Up, thick), tint, true);
									DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Left, thick), tint, false);
									DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Right, thick), tint, false);
								}
							}
						}
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}
		}

		// Draw Bounds
		if (DrawBounds) {
			int thick = GUI.Unify(1);
			for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
				var entities = Stage.Entities[layer];
				int count = Stage.EntityCounts[layer];
				for (int i = 0; i < count; i++) {
					var e = entities[i];
					if (!e.Active) continue;
					DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Down, thick), Color32.CYAN_BETTER, true);
					DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Up, thick), Color32.CYAN_BETTER, true);
					DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Left, thick), Color32.CYAN_BETTER, false);
					DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Right, thick), Color32.CYAN_BETTER, false);
				}
			}
		}

		// Func
		static void DrawGizmosRectAsLine (IRect panelRect, IRect rect, Color32 color, bool horizontal) {
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

		int barHeight = GUI.Unify(24);
		panelRect.height = barHeight * (EntityUsages.Length + TextUsages.Length + RenderingUsages.Length);
		panelRect.y -= panelRect.height;
		int barPadding = GUI.Unify(4);
		var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);

		// Entity
		for (int i = 0; i < EntityUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), EntityUsages[i], Color32.CYAN);
			rect.y -= rect.height;
		}
		// Rendering
		for (int i = 0; i < RenderingUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), RenderingUsages[i], Color32.GREEN);
			rect.y -= rect.height;
		}
		// Text
		for (int i = 0; i < TextUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), TextUsages[i], Color32.GREEN);
			rect.y -= rect.height;
		}

		// Func
		static void DrawBar (IRect rect, BarData data, Color32 barColor) {
			int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
			Renderer.DrawPixel(new IRect(rect.x, rect.y, width, rect.height), barColor, int.MaxValue);
			// Label
			int startIndex = Renderer.GetTextUsedCellCount();
			GUI.Label(rect, data.I2C.GetChars(data.Value), GUISkin.SmallCenterLabel);
			if (Renderer.GetTextCells(out var cells, out int count)) {
				for (int i = startIndex; i < count && i < startIndex + data.I2C.Prefix.Length; i++) {
					cells[i].Color = new Color32(96, 96, 96, 255);
				}
			}
		}
	}


	private static void DrawEffectPanel (ref IRect panelRect) {
		int itemHeight = GUI.Unify(28);
		int itemPadding = GUI.Unify(8);
		panelRect.height = Const.SCREEN_EFFECT_COUNT * (itemHeight + itemPadding) + itemPadding;
		panelRect.y -= panelRect.height;
		var rect = new IRect(panelRect.x + itemPadding, panelRect.yMax, panelRect.width - itemPadding * 2, itemHeight);
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {

			rect.y -= itemHeight + itemPadding;

			// Label
			GUI.Label(rect, Const.SCREEN_EFFECT_NAMES[i], GUISkin.SmallLabel);

			// Toggle
			var enableRect = rect.EdgeInside(Direction4.Right, rect.height);
			EffectsEnabled[i] = GUI.Toggle(enableRect, EffectsEnabled[i]);
		}

		// Update Values
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (EffectsEnabled[i]) Game.PassEffect(i);
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_DARKEN]) {
			Game.PassEffect_RetroDarken(Game.GlobalFrame.PingPong(60) / 60f);
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_LIGHTEN]) {
			Game.PassEffect_RetroLighten(Game.GlobalFrame.PingPong(60) / 60f);
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_TINT]) {
			Game.PassEffect_Tint(Color32.LerpUnclamped(new Color32(255, 128, 196, 255), new Color32(128, 255, 64, 255), Game.GlobalFrame.PingPong(120) / 120f));
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_VIGNETTE]) {
			Game.PassEffect_Vignette(0.95f, 0.6f, 0f, 0f, 0f);
		}
	}


	#endregion




}
