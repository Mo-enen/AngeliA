using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework;

public class GUIScope : System.IDisposable {

	private class ScopeGroup {
		private const int COUNT = 32;
		public GUIScope[] Scopes;
		public int CurrentIndex;
		public ScopeGroup (ScopeType type) {
			CurrentIndex = 0;
			Scopes = new GUIScope[COUNT];
			for (int i = 0; i < COUNT; i++) {
				Scopes[i] = new GUIScope(type);
			}
		}
		public GUIScope Start () {
			if (CurrentIndex >= COUNT) {
#if DEBUG
				Util.LogWarning("Too many layers of GUIScope");
#endif
				return null;
			}
			var scope = Scopes[CurrentIndex];
			CurrentIndex++;
			return scope;
		}
		public void End () => CurrentIndex--;
	}

	private enum ScopeType { None, Layer, Color, ContentColor, BodyColor, Enable, Scroll, }

	private static readonly GUIScope EmptyScope = new(ScopeType.None);
	private static readonly ScopeGroup LayerInstance = new(ScopeType.Layer);
	private static readonly ScopeGroup ColorInstance = new(ScopeType.Color);
	private static readonly ScopeGroup ContentColorInstance = new(ScopeType.ContentColor);
	private static readonly ScopeGroup BodyColorInstance = new(ScopeType.BodyColor);
	private static readonly ScopeGroup EnableInstance = new(ScopeType.Enable);
	private static readonly ScopeGroup ScrollInstance = new(ScopeType.Scroll);

	public Int2 Position => Int2Data;

	private readonly ScopeType Type;
	private Color32 ColorData;
	private int IntData;
	private int IntDataAlt;
	private IRect RectData;
	private Int2 Int2Data;
	private Int2 Int2DataAlt;

	private GUIScope (ScopeType type) => Type = type;

	public static GUIScope Layer (int layer) {
		var result = LayerInstance.Start();
		if (result == null) return EmptyScope;
		result.IntData = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(layer);
		return result;
	}

	public static GUIScope Color (Color32 color) {
		var result = ColorInstance.Start();
		if (result == null) return EmptyScope;
		result.ColorData = GUI.Color;
		GUI.Color = color;
		return result;
	}

	public static GUIScope ContentColor (Color32 color) {
		var result = ContentColorInstance.Start();
		if (result == null) return EmptyScope;
		result.ColorData = GUI.ContentColor;
		GUI.ContentColor = color;
		return result;
	}

	public static GUIScope BodyColor (Color32 color) {
		var result = BodyColorInstance.Start();
		if (result == null) return EmptyScope;
		result.ColorData = GUI.BodyColor;
		GUI.BodyColor = color;
		return result;
	}

	public static GUIScope BodyColor (bool enable) {
		var result = EnableInstance.Start();
		if (result == null) return EmptyScope;
		result.IntData = GUI.Enable ? 1 : 0;
		GUI.Enable = enable;
		return result;
	}

	public static GUIScope Scroll (IRect rect, int positionY, int min = int.MinValue, int max = int.MaxValue) {

		var result = ScrollInstance.Start();
		if (result == null) return EmptyScope;
		bool mouseInside = rect.MouseInside();

		result.RectData = rect;
		result.IntData = Renderer.GetUsedCellCount(RenderLayer.UI);
		result.IntDataAlt = Renderer.GetTextUsedCellCount(0);
		result.Int2DataAlt = Input.MousePositionShift;

		// Scroll by Mouse Wheel
		if (mouseInside && Input.MouseWheelDelta != 0) {
			positionY -= Input.MouseWheelDelta * GUI.Unify(96);
		}
		positionY = positionY.Clamp(min, max);
		result.Int2Data = new Int2(0, positionY);

		// Shift Input
		Input.SetMousePositionShift(0, -positionY);

		return result;
	}

	public void Dispose () {
		switch (Type) {
			case ScopeType.Layer:
				LayerInstance.End();
				Renderer.SetLayer(IntData);
				break;
			case ScopeType.Color:
				ColorInstance.End();
				GUI.Color = ColorData;
				break;
			case ScopeType.ContentColor:
				ContentColorInstance.End();
				GUI.ContentColor = ColorData;
				break;
			case ScopeType.BodyColor:
				BodyColorInstance.End();
				GUI.BodyColor = ColorData;
				break;
			case ScopeType.Enable:
				EnableInstance.End();
				GUI.Enable = IntData == 1;
				break;
			case ScopeType.Scroll:

				ScrollInstance.End();

				// Old Value Back
				Input.SetMousePositionShift(Int2DataAlt.x, Int2DataAlt.y);
				
				// Scroll Sprites
				int startIndex = IntData;
				if (Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) {
					for (int i = startIndex; i < count; i++) {
						var cell = cells[i];
						cell.Y += Int2Data.y;
					}
				}

				// Scroll Text
				int tStartIndex = IntDataAlt;
				if (Renderer.GetTextCells(0, out var tCells, out int tCount)) {
					for (int i = tStartIndex; i < tCount; i++) {
						var cell = tCells[i];
						cell.Y += Int2Data.y;
					}
				}

				// Clamp Sprites
				Renderer.ClampCells(RenderLayer.UI, RectData, startIndex);
				Renderer.ClampTextCells(RectData, tStartIndex);

				break;
		}
	}

}