using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

internal partial class GameEditor {

	// SUB
	private class ProfilerUiBarData {
		public string Name;
		public int Value;
		public int Capacity;
	}

	// VAR
	private float FrameDurationMilliSecond;
	private readonly ProfilerUiBarData[] RenderingUsages = new ProfilerUiBarData[RenderLayer.COUNT];
	private readonly ProfilerUiBarData[] EntityUsages = new ProfilerUiBarData[EntityLayer.COUNT];
	private static readonly SavingBool ShowRenderLayers = new("Toolbar.ShowRenderLayers", true, SavingLocation.Global);
	private static readonly SavingBool ShowEntityLayers = new("Toolbar.ShowEntityLayers", true, SavingLocation.Global);
	private static readonly SavingBool ShowTimeUsage = new("Toolbar.ShowTimeUsage", true, SavingLocation.Global);

	// MSG
	private void DrawProfilerPanel (ref IRect panelRect) {

		// Init
		if (RenderingUsages[0] == null || EntityUsages[0] == null) return;

		// Draw
		int barHeight = GUI.Unify(24);
		int barPadding = GUI.Unify(4);
		int toggleShift = GUI.Unify(18);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - barHeight, panelRect.width, barHeight);

		// Entity
		bool eOpening = ShowEntityLayers.Value;
		if (GUI.ToggleFold(rect.ShrinkLeft(toggleShift), ref eOpening, BuiltInSprite.ICON_ENTITY, "Entity", paddingLeft: GUI.FieldPadding)) {
			rect.SlideDown();
			for (int i = 0; i < EntityUsages.Length; i++) {
				var data = EntityUsages[i];
				DrawBar(rect.Shrink(barPadding), data.Name, data.Value, data.Capacity, Color32.CYAN);
				rect.SlideDown();
			}
		}
		ShowEntityLayers.Value = eOpening;
		rect.SlideDown();

		// Rendering
		bool rOpening = ShowRenderLayers.Value;
		if (GUI.ToggleFold(rect.ShrinkLeft(toggleShift), ref rOpening, BuiltInSprite.ICON_RENDER, "Rendering", paddingLeft: GUI.FieldPadding)) {
			rect.SlideDown();
			for (int i = 0; i < RenderingUsages.Length; i++) {
				var data = RenderingUsages[i];
				DrawBar(rect.Shrink(barPadding), data.Name, data.Value, data.Capacity, Color32.GREEN);
				rect.SlideDown();
			}
		}
		ShowRenderLayers.Value = rOpening;
		rect.SlideDown();

		// Time Usage
		bool tOpening = ShowTimeUsage.Value;
		if (GUI.ToggleFold(rect.ShrinkLeft(toggleShift), ref tOpening, BuiltInSprite.ICON_CPU, "Time Usage", paddingLeft: GUI.FieldPadding)) {
			rect.SlideDown();
			float targetFrameMilSec = 1000f / Game.GetTargetFramerate();
			float usage01 = FrameDurationMilliSecond / targetFrameMilSec;
			float totalUsage01 = Game.FrameDurationMilliSecond / targetFrameMilSec;
			DrawBar(rect.Shrink(barPadding), "", (totalUsage01 * 100).RoundToInt(), 100, Color32.GREY_96);
			DrawBar(rect.Shrink(barPadding), "CPU (%)", (usage01 * 100).RoundToInt(), 100, Color32.ORANGE);
			rect.SlideDown();
		}
		ShowTimeUsage.Value = tOpening;
		rect.SlideDown();

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;

		// Func
		static void DrawBar (IRect rect, string name, int value, int capacity, Color32 barColor) {
			int width = Util.RemapUnclamped(0, capacity, 0, rect.width, value);
			int padding = Unify(6);
			var barRect = new IRect(rect.x, rect.y, width, rect.height);

			// Bar
			Renderer.DrawPixel(
				barRect,
				value < capacity ? barColor : Color32.RED,
				z: int.MaxValue
			);

			// Label
			GUI.Label(rect.ShrinkLeft(padding), name, GUI.Skin.SmallLabel);

			// Number Label
			using (new GUIColorScope(new Color32(96, 96, 96, 255))) {
				GUI.IntLabel(rect.ShrinkRight(padding), capacity, out var bounds, GUI.Skin.SmallRightLabel);
				GUI.Label(bounds.EdgeOutsideLeft(rect.width).ShrinkRight(padding), "/", out bounds, GUI.Skin.SmallRightLabel);
				GUI.IntLabel(bounds.EdgeOutsideLeft(rect.width).ShrinkRight(padding), value, GUI.Skin.SmallRightLabel);
			}

			using (new ClampCellsScope(barRect)) {
				// Black Label
				using (new GUIColorScope(new Color32(24, 24, 24, 255))) {
					GUI.Label(rect.ShrinkLeft(padding), name, GUI.Skin.SmallLabel);
				}
				// Black Number Label
				using (new GUIColorScope(new Color32(12, 12, 12, 255))) {
					GUI.IntLabel(rect.ShrinkRight(padding), capacity, out var bounds, GUI.Skin.SmallRightLabel);
					GUI.Label(bounds.EdgeOutsideLeft(rect.width).ShrinkRight(padding), "/", out bounds, GUI.Skin.SmallRightLabel);
					GUI.IntLabel(bounds.EdgeOutsideLeft(rect.width).ShrinkRight(padding), value, GUI.Skin.SmallRightLabel);
				}
			}

		}
	}

	// API
	public void UpdateUsageData (float frameDurationMilliSecond, int[] renderUsages, int[] renderCapacities, int[] entityUsages, int[] entityCapacities) {
		FrameDurationMilliSecond = frameDurationMilliSecond;
		if (RenderingUsages[0] == null) {
			for (int i = 0; i < RenderLayer.COUNT; i++) {
				RenderingUsages[i] = new ProfilerUiBarData() {
					Name = RenderLayer.NAMES[i],
				};
			}
		}
		if (EntityUsages[0] == null) {
			for (int i = 0; i < EntityLayer.COUNT; i++) {
				EntityUsages[i] = new ProfilerUiBarData() {
					Name = EntityLayer.LAYER_NAMES[i],
				};
			}
		}
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			RenderingUsages[i].Value = renderUsages[i];
			RenderingUsages[i].Capacity = renderCapacities[i];
		}
		for (int i = 0; i < EntityLayer.COUNT; i++) {
			EntityUsages[i].Value = entityUsages[i];
			EntityUsages[i].Capacity = entityCapacities[i];
		}
	}

}
