using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class GameEditor {

	// VAR
	private readonly ProfilerUiBarData[] RenderingUsages = new ProfilerUiBarData[RenderLayer.COUNT];
	private readonly ProfilerUiBarData[] EntityUsages = new ProfilerUiBarData[EntityLayer.COUNT];

	// MSG
	private void DrawProfilerPanel (ref IRect panelRect) {

		// Init
		if (RenderingUsages[0] == null || EntityUsages[0] == null) return;

		// Draw
		int barHeight = GUI.Unify(24);
		panelRect.height = barHeight * (EntityUsages.Length + RenderingUsages.Length + 2);
		panelRect.y -= panelRect.height;
		int barPadding = GUI.Unify(4);
		var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);

		// Entity
		GUI.Label(rect, "Entity", GUI.Skin.SmallGreyLabel);
		rect.SlideDown();
		for (int i = 0; i < EntityUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), EntityUsages[i], Color32.CYAN);
			rect.SlideDown();
		}

		// Rendering
		GUI.Label(rect, "Rendering", GUI.Skin.SmallGreyLabel);
		rect.SlideDown();
		for (int i = 0; i < RenderingUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), RenderingUsages[i], Color32.GREEN);
			rect.SlideDown();
		}

		// Func
		static void DrawBar (IRect rect, ProfilerUiBarData data, Color32 barColor) {
			int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
			Renderer.DrawPixel(new IRect(rect.x, rect.y, width, rect.height), data.Value < data.Capacity ? barColor : Color32.RED, int.MaxValue);
			// Label
			int padding = Unify(6);
			GUI.Label(rect.ShrinkLeft(padding), data.Name, GUI.Skin.SmallLabel);
			using (new GUIColorScope(new Color32(96, 96, 96, 255))) {
				GUI.IntLabel(rect.ShrinkRight(padding), data.Capacity, out var bounds, GUI.Skin.SmallRightLabel);
				GUI.Label(bounds.EdgeOutsideLeft(rect.width).ShrinkRight(padding), "/", out bounds, GUI.Skin.SmallRightLabel);
				GUI.IntLabel(bounds.EdgeOutsideLeft(rect.width).ShrinkRight(padding), data.Value, GUI.Skin.SmallRightLabel);
			}
		}
	}

	// API
	public void UpdateUsageData (int[] renderUsages, int[] renderCapacities, int[] entityUsages, int[] entityCapacities) {
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
