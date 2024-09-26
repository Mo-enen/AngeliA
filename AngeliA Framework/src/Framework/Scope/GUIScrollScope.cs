namespace AngeliA;



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
	public readonly bool PrevMouseInputIgnoring;
	public GUIScrollScope (IRect rect, Int2 position, bool mouseWheelForVertical = true) : this(rect, position, new Int2(int.MinValue, int.MinValue), new Int2(int.MaxValue, int.MaxValue), mouseWheelForVertical) { }
	public GUIScrollScope (IRect rect, Int2 position, Int2 min, Int2 max, bool mouseWheelForVertical = true, bool reverseMouseWheel = false) {

		bool mouseInside = rect.MouseInside();
		Rect = rect;
		PrevMouseInputIgnoring = Input.IgnoringMouseInput;
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
		if (PrevMouseInputIgnoring) {
			Input.IgnoreMouseInput();
		} else {
			Input.CancelIgnoreMouseInput();
		}
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
