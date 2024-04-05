using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class Scope : System.IDisposable {

	// SUB
	private class ScopeGroup {
		private const int COUNT = 32;
		public Scope[] Scopes;
		public int CurrentIndex;
		public ScopeGroup () {
			CurrentIndex = 0;
			Scopes = new Scope[COUNT];
			for (int i = 0; i < COUNT; i++) {
				Scopes[i] = new Scope(this);
			}
		}
		public Scope Start () {
			if (CurrentIndex >= COUNT) {
#if DEBUG
				Debug.LogWarning("Too many layers of GUIScope");
#endif
				return null;
			}
			var scope = Scopes[CurrentIndex];
			CurrentIndex++;
			return scope;
		}
		public void End () => CurrentIndex--;
	}

	// Const
	private static readonly Scope EmptyScope = new(null);
	private static readonly ScopeGroup LayerInstance = new();
	private static readonly ScopeGroup ColorInstance = new();
	private static readonly ScopeGroup ContentColorInstance = new();
	private static readonly ScopeGroup BodyColorInstance = new();
	private static readonly ScopeGroup EnableInstance = new();
	private static readonly ScopeGroup ScrollInstance = new();
	private static readonly ScopeGroup SheetInstance = new();
	private static readonly ScopeGroup IgnoreInputInstance = new();
	private static readonly ScopeGroup GUILabelWidthInstance = new();

	// Api
	public Int2 ScrollPosition => Int2Data;

	// Data
	private readonly ScopeGroup Group;
	private Color32 ColorData;
	private int IntData;
	private int IntDataAlt;
	private IRect RectData;
	private Int2 Int2Data;
	private Int2 Int2DataAlt;

	// MSG
	private Scope (ScopeGroup group) => Group = group;

	// API
	public static Scope RendererLayerUI () => RendererLayer(RenderLayer.UI);

	public static Scope RendererLayer (int layer) {
		var result = LayerInstance.Start();
		if (result == null) return EmptyScope;
		result.IntData = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(layer);
		return result;
	}

	public static Scope GUIColor (Color32 color) {
		var result = ColorInstance.Start();
		if (result == null) return EmptyScope;
		result.ColorData = GUI.Color;
		GUI.Color = color;
		return result;
	}

	public static Scope GUIContentColor (Color32 color) {
		var result = ContentColorInstance.Start();
		if (result == null) return EmptyScope;
		result.ColorData = GUI.ContentColor;
		GUI.ContentColor = color;
		return result;
	}

	public static Scope GUIBodyColor (Color32 color) {
		var result = BodyColorInstance.Start();
		if (result == null) return EmptyScope;
		result.ColorData = GUI.BodyColor;
		GUI.BodyColor = color;
		return result;
	}

	public static Scope GUIEnable (bool enable) {
		var result = EnableInstance.Start();
		if (result == null) return EmptyScope;
		result.IntData = GUI.Enable ? 1 : 0;
		GUI.Enable = enable;
		return result;
	}

	public static Scope GUIScroll (IRect rect, int positionY, int min = int.MinValue, int max = int.MaxValue) {

		var result = ScrollInstance.Start();
		if (result == null) return EmptyScope;
		bool mouseInside = rect.MouseInside();

		result.RectData = rect;
		result.IntData = Renderer.GetUsedCellCount(RenderLayer.UI);
		result.IntDataAlt = Renderer.GetTextUsedCellCount(0);
		result.Int2DataAlt = Input.MousePositionShift;
		if (!mouseInside) Input.IgnoreMouseInput();

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

	public static Scope Sheet (int index) {
		var result = SheetInstance.Start();
		if (result == null) return EmptyScope;
		result.IntData = Renderer.CurrentSheetIndex;
		Renderer.CurrentSheetIndex = index;
		return result;
	}

	public static Scope IgnoreInput (bool ignoreKey = true, bool ignoreMouse = true) {
		var result = IgnoreInputInstance.Start();
		if (result == null) return EmptyScope;
		result.Int2Data.x = Input.IgnoringKeyInput ? 1 : 0;
		result.Int2Data.y = Input.IgnoringMouseInput ? 1 : 0;
		if (ignoreKey) {
			Input.IgnoreKeyInput();
		} else {
			Input.CancelIgnoreKeyInput();
		}
		if (ignoreMouse) {
			Input.IgnoreMouseInput();
		} else {
			Input.CancelIgnoreMouseInput();
		}
		return result;
	}

	public static Scope GUILabelWidth (int width) {
		var result = GUILabelWidthInstance.Start();
		if (result == null) return EmptyScope;
		result.IntData = GUI.LabelWidth;
		GUI.LabelWidth = width;
		return result;
	}

	public void Dispose () {

		Group?.End();

		switch (0) {
			case var _ when Group == LayerInstance:
				if (Renderer.CurrentLayerIndex == RenderLayer.UI) {
					Renderer.ReverseUnsortedCells(RenderLayer.UI);
				}
				Renderer.SetLayer(IntData);
				break;

			case var _ when Group == ColorInstance:
				GUI.Color = ColorData;
				break;

			case var _ when Group == ContentColorInstance:
				GUI.ContentColor = ColorData;
				break;

			case var _ when Group == BodyColorInstance:
				GUI.BodyColor = ColorData;
				break;

			case var _ when Group == EnableInstance:
				GUI.Enable = IntData == 1;
				break;

			case var _ when Group == ScrollInstance:
				Input.SetMousePositionShift(Int2DataAlt.x, Int2DataAlt.y);
				Input.CancelIgnoreMouseInput();
				int startIndex = IntData;
				if (Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) {
					for (int i = startIndex; i < count; i++) {
						cells[i].Y += Int2Data.y;
					}
				}
				int tStartIndex = IntDataAlt;
				if (Renderer.GetTextCells(0, out var tCells, out int tCount)) {
					for (int i = tStartIndex; i < tCount; i++) {
						tCells[i].Y += Int2Data.y;
					}
				}
				Renderer.ClampCells(RenderLayer.UI, RectData, startIndex);
				Renderer.ClampTextCells(RectData, tStartIndex);
				break;

			case var _ when Group == SheetInstance:
				Renderer.CurrentSheetIndex = IntData;
				break;

			case var _ when Group == IgnoreInputInstance:
				if (Int2Data.x == 1) {
					Input.IgnoreKeyInput();
				} else {
					Input.CancelIgnoreKeyInput();
				}
				if (Int2Data.y == 1) {
					Input.IgnoreMouseInput();
				} else {
					Input.CancelIgnoreMouseInput();
				}
				break;

			case var _ when Group == GUILabelWidthInstance:
				GUI.LabelWidth = IntData;
				break;
		}
	}

}