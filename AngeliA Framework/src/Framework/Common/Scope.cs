namespace AngeliA;



public readonly struct LayerScope : System.IDisposable {
	private readonly int OldLayer;
	public LayerScope (int layer) {
		OldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(layer);
	}
	public readonly void Dispose () {
		if (Renderer.CurrentLayerIndex == RenderLayer.UI) {
			Renderer.ReverseUnsortedCells(RenderLayer.UI);
		}
		Renderer.SetLayer(OldLayer);
	}
}



public readonly struct UILayerScope : System.IDisposable {
	private readonly int OldLayer;
	private readonly bool IgnoreSorting;
	public UILayerScope () : this(false) { }
	public UILayerScope (bool ignoreSorting) {
		OldLayer = Renderer.CurrentLayerIndex;
		IgnoreSorting = ignoreSorting;
		Renderer.SetLayer(RenderLayer.UI);
	}
	public readonly void Dispose () {
		if (!IgnoreSorting) {
			Renderer.ReverseUnsortedCells(RenderLayer.UI);
		}
		Renderer.SetLayer(OldLayer);
	}
}



public readonly struct DefaultLayerScope : System.IDisposable {
	private readonly int OldLayer;
	public DefaultLayerScope () {
		OldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(RenderLayer.DEFAULT);
	}
	public readonly void Dispose () => Renderer.SetLayer(OldLayer);
}



public readonly struct GUIColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIColorScope (Color32 color) {
		OldColor = GUI.Color;
		GUI.Color = color;
	}
	public readonly void Dispose () => GUI.Color = OldColor;
}



public readonly struct GUIContentColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIContentColorScope (Color32 color) {
		OldColor = GUI.ContentColor;
		GUI.ContentColor = color;
	}
	public readonly void Dispose () => GUI.ContentColor = OldColor;
}



public readonly struct GUIBodyColorScope : System.IDisposable {
	private readonly Color32 OldColor;
	public GUIBodyColorScope (Color32 color) {
		OldColor = GUI.BodyColor;
		GUI.BodyColor = color;
	}
	public readonly void Dispose () => GUI.BodyColor = OldColor;
}



public readonly struct GUIEnableScope : System.IDisposable {
	private readonly bool OldEnable;
	public GUIEnableScope (bool enable) {
		OldEnable = GUI.Enable;
		GUI.Enable = enable;
	}
	public readonly void Dispose () => GUI.Enable = OldEnable;
}



public readonly struct GUIScrollScope : System.IDisposable {
	public readonly int PositionY;
	public readonly IRect Rect;
	public readonly int CellCount;
	public readonly Int2 MousePosShift;
	public GUIScrollScope (IRect rect, int positionY, int min = int.MinValue, int max = int.MaxValue) {

		bool mouseInside = rect.MouseInside();
		Rect = rect;
		CellCount = Renderer.GetUsedCellCount(RenderLayer.UI);
		MousePosShift = Input.MousePositionShift;
		if (!mouseInside) Input.IgnoreMouseInput();

		// Scroll by Mouse Wheel
		if (mouseInside && Input.MouseWheelDelta != 0) {
			positionY -= Input.MouseWheelDelta * GUI.Unify(96);
		}
		PositionY = positionY.Clamp(min, max);

		// Shift Input
		Input.SetMousePositionShift(0, -PositionY);

	}
	public readonly void Dispose () {
		Input.SetMousePositionShift(MousePosShift.x, MousePosShift.y);
		Input.CancelIgnoreMouseInput();
		int startIndex = CellCount;
		if (startIndex >= 0) {
			if (Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) {
				for (int i = startIndex; i < count; i++) {
					cells[i].Y += PositionY;
				}
			}
			Renderer.ClampCells(RenderLayer.UI, Rect, startIndex);
		}
	}
}


public readonly struct GUIHorizontalScrollScope : System.IDisposable {
	public readonly int PositionX;
	public readonly IRect Rect;
	public readonly int CellCount;
	public readonly Int2 MousePosShift;
	public GUIHorizontalScrollScope (IRect rect, int positionX, int min = int.MinValue, int max = int.MaxValue) {

		bool mouseInside = rect.MouseInside();
		Rect = rect;
		CellCount = Renderer.GetUsedCellCount(RenderLayer.UI);
		MousePosShift = Input.MousePositionShift;
		if (!mouseInside) Input.IgnoreMouseInput();

		// Scroll by Mouse Wheel
		if (mouseInside && Input.MouseWheelDelta != 0) {
			positionX -= Input.MouseWheelDelta * GUI.Unify(96);
		}
		PositionX = positionX.Clamp(min, max);

		// Shift Input
		Input.SetMousePositionShift(-PositionX, 0);

	}
	public readonly void Dispose () {
		Input.SetMousePositionShift(MousePosShift.x, MousePosShift.y);
		Input.CancelIgnoreMouseInput();
		int startIndex = CellCount;
		if (startIndex >= 0) {
			if (Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) {
				for (int i = startIndex; i < count; i++) {
					cells[i].X += PositionX;
				}
			}
			Renderer.ClampCells(RenderLayer.UI, Rect, startIndex);
		}
	}
}


public readonly struct SheetIndexScope : System.IDisposable {
	private readonly int OldSheet;
	public SheetIndexScope (int sheet) {
		OldSheet = Renderer.CurrentSheetIndex;
		Renderer.CurrentSheetIndex = sheet;
	}
	public readonly void Dispose () => Renderer.CurrentSheetIndex = OldSheet;
}



public readonly struct IgnoreInputScope : System.IDisposable {
	private readonly bool OldIgnoreKey;
	private readonly bool OldIgnoreMouse;
	public IgnoreInputScope (bool ignoreKey = true, bool ignoreMouse = true) {
		OldIgnoreKey = Input.IgnoringKeyInput;
		OldIgnoreMouse = Input.IgnoringMouseInput;
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
	}
	public readonly void Dispose () {
		if (OldIgnoreKey) {
			Input.IgnoreKeyInput();
		} else {
			Input.CancelIgnoreKeyInput();
		}
		if (OldIgnoreMouse) {
			Input.IgnoreMouseInput();
		} else {
			Input.CancelIgnoreMouseInput();
		}
	}
}



public readonly struct GUILabelWidthScope : System.IDisposable {
	private readonly int OldWidth;
	public GUILabelWidthScope (int width) {
		OldWidth = GUI.LabelWidth;
		GUI.LabelWidth = width;
	}
	public readonly void Dispose () => GUI.LabelWidth = OldWidth;
}



public readonly struct GUISkinScope : System.IDisposable {
	private readonly GUISkin OldSkin;
	public GUISkinScope (GUISkin skin) {
		OldSkin = GUI.Skin;
		GUI.Skin = skin;
	}
	public readonly void Dispose () => GUI.Skin = OldSkin;
}



public readonly struct ReverseCellsScope : System.IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	public ReverseCellsScope () {
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
	}
	public readonly void Dispose () {
		int start = UsedCount;
		if (Renderer.GetCells(LayerIndex, out var cells, out int count) && start < count) {
			System.Array.Reverse(cells, start, count - start);
		}
	}
}


