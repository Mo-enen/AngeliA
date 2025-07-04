namespace AngeliA;


/// <summary>
/// Scope that make GUI elements inside scrolls
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	static int ScrollPos;
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		var cameraRect = Renderer.CameraRect;
/// 
/// 		using (var scroll = new GUIVerticalScrollScope(cameraRect, ScrollPos, 0, 4096)) {
/// 			ScrollPos = scroll.PositionY;
/// 
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y, 0, 0, 0, 512, 512);
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y + 1024, 0, 0, 0, 512, 512);
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y + 2048, 0, 0, 0, 512, 512);
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIVerticalScrollScope : System.IDisposable {
	public readonly int PositionY => Scope.Position.y;
	public readonly GUIScrollScope Scope;

	/// <summary>
	/// Scope that make GUI elements inside scrolls
	/// </summary>
	public GUIVerticalScrollScope (IRect panelRect, int positionY, int min = int.MinValue, int max = int.MaxValue, int layer = RenderLayer.UI) {
		Scope = new(panelRect, new Int2(0, positionY), new Int2(0, min), new Int2(0, max), true, false, layer);
	}

	public readonly void Dispose () => Scope.Dispose();
}


/// <summary>
/// Scope that make GUI elements inside scrolls
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	static int ScrollPos;
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		var cameraRect = Renderer.CameraRect;
/// 
/// 		using (var scroll = new GUIHorizontalScrollScope(cameraRect, ScrollPos, 0, 4096)) {
/// 			ScrollPos = scroll.PositionX;
/// 
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y, 0, 0, 0, 512, 512);
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y + 1024, 0, 0, 0, 512, 512);
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y + 2048, 0, 0, 0, 512, 512);
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIHorizontalScrollScope : System.IDisposable {
	public readonly int PositionX => Scope.Position.x;
	public readonly GUIScrollScope Scope;

	/// <summary>
	/// Scope that make GUI elements inside scrolls
	/// </summary>
	public GUIHorizontalScrollScope (IRect rect, int positionX, int min = int.MinValue, int max = int.MaxValue, int layer = RenderLayer.UI) {
		Scope = new(rect, new Int2(positionX, 0), new Int2(min, 0), new Int2(max, 0), false, false, layer);
	}

	public readonly void Dispose () => Scope.Dispose();
}


/// <summary>
/// Scope that make GUI elements inside scrolls
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	static Int2 ScrollPos;
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		var cameraRect = Renderer.CameraRect;
/// 
/// 		using (var scroll = new GUIScrollScope(cameraRect, ScrollPos, new Int2(0, -4096), new Int2(0, 4096))) {
/// 			ScrollPos = scroll.Position;
/// 
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y, 0, 0, 0, 512, 512);
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y + 1024, 0, 0, 0, 512, 512);
/// 			Renderer.Draw(BuiltInSprite.ICON_ENTITY, cameraRect.x + 1024, cameraRect.y + 2048, 0, 0, 0, 512, 512);
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GUIScrollScope : System.IDisposable {
	public readonly Int2 Position;
	public readonly IRect Rect;
	public readonly int CellCount;
	public readonly int Layer;
	public readonly Int2 MousePosShift;
	public readonly bool PrevMouseInputIgnoring;
	/// <summary>
	/// Scope that make GUI elements inside scrolls
	/// </summary>
	/// <param name="rect">Content rect position in global space</param>
	/// <param name="position">Scroll position in global space</param>
	/// <param name="min">Minimal limitation for the scrolling</param>
	/// <param name="max">Maximal limitation for the scrolling</param>
	/// <param name="mouseWheelForVertical">True if the mouse wheel control vertical scroll instead of horizontal</param>
	/// <param name="reverseMouseWheel">True if the mouse wheel scroll should reverse in direction</param>
	/// <param name="layer">Which render layer does it scrolls</param>
	public GUIScrollScope (IRect rect, Int2 position, Int2 min, Int2 max, bool mouseWheelForVertical = true, bool reverseMouseWheel = false, int layer = int.MinValue) {

		Layer = layer == int.MinValue ? Renderer.CurrentLayerIndex : layer;
		bool mouseInside = rect.MouseInside();
		Rect = rect;
		PrevMouseInputIgnoring = Input.IgnoringMouseInput;
		CellCount = Renderer.GetUsedCellCount(Layer);
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
			if (Renderer.GetCells(Layer, out var cells, out int count)) {
				for (int i = startIndex; i < count; i++) {
					cells[i].X -= Position.x;
					cells[i].Y += Position.y;
				}
			}
			Renderer.ClampCells(Layer, Rect, startIndex);
		}
	}
}
