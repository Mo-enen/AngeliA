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



public readonly struct GUIInteractableScope : System.IDisposable {
	private readonly bool OldInteractable;
	public GUIInteractableScope (bool interactable) {
		OldInteractable = GUI.Interactable;
		GUI.Interactable = interactable;
	}
	public readonly void Dispose () => GUI.Interactable = OldInteractable;
}



public readonly struct GUIVerticalScrollScope : System.IDisposable {
	public readonly int PositionY => Scope.Position.y;
	public readonly GUIScrollScope Scope;
	public GUIVerticalScrollScope (IRect rect, int positionY, int min = int.MinValue, int max = int.MaxValue) {
		Scope = new GUIScrollScope(rect, new Int2(0, positionY), new Int2(0, min), new Int2(0, max), true);
	}
	public readonly void Dispose () => Scope.Dispose();
}


public readonly struct GUIHorizontalScrollScope : System.IDisposable {
	public readonly int PositionX => Scope.Position.x;
	public readonly GUIScrollScope Scope;
	public GUIHorizontalScrollScope (IRect rect, int positionX, int min = int.MinValue, int max = int.MaxValue) {
		Scope = new GUIScrollScope(rect, new Int2(positionX, 0), new Int2(min, 0), new Int2(max, 0), false);
	}
	public readonly void Dispose () => Scope.Dispose();
}


public readonly struct GUIScrollScope : System.IDisposable {
	public readonly Int2 Position;
	public readonly IRect Rect;
	public readonly int CellCount;
	public readonly Int2 MousePosShift;
	public GUIScrollScope (IRect rect, Int2 position, bool mouseWheelForVertical = true) : this(rect, position, new Int2(int.MinValue, int.MinValue), new Int2(int.MaxValue, int.MaxValue), mouseWheelForVertical) { }
	public GUIScrollScope (IRect rect, Int2 position, Int2 min, Int2 max, bool mouseWheelForVertical = true, bool reverseMouseWheel = false) {

		bool mouseInside = rect.MouseInside();
		Rect = rect;
		CellCount = Renderer.GetUsedCellCount(RenderLayer.UI);
		MousePosShift = Input.MousePositionShift;
		if (!mouseInside) Input.IgnoreMouseInput();

		// Scroll by Mouse Wheel
		if (GUI.Interactable && mouseInside && Input.MouseWheelDelta != 0) {
			int delta = reverseMouseWheel ? Input.MouseWheelDelta * GUI.Unify(-96) : Input.MouseWheelDelta * GUI.Unify(96);
			position[mouseWheelForVertical ? 1 : 0] -= delta;
		}
		Position.x = position.x.Clamp(min.x, max.x);
		Position.y = position.y.Clamp(min.y, max.y);

		// Shift Input
		Input.SetMousePositionShift(Position.x, -Position.y);

	}
	public readonly void Dispose () {
		Input.SetMousePositionShift(MousePosShift.x, MousePosShift.y);
		Input.CancelIgnoreMouseInput();
		int startIndex = CellCount;
		if (startIndex >= 0) {
			if (Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) {
				for (int i = startIndex; i < count; i++) {
					cells[i].X -= Position.x;
					cells[i].Y += Position.y;
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


public readonly struct ClampCellsScope : System.IDisposable {
	private readonly int LayerIndex;
	private readonly int UsedCount;
	private readonly IRect Rect;
	public ClampCellsScope (IRect rect) {
		Rect = rect;
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
	}
	public readonly void Dispose () {
		Renderer.ClampCells(LayerIndex, Rect, UsedCount);
	}
}



public readonly struct EmptyScope : System.IDisposable {
	public readonly void Dispose () { }
}