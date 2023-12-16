using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Runtime.CompilerServices;


namespace AngeliaFramework.Editor {
	public abstract class CanvasWindow : EditorWindow {


		// Override
		protected virtual bool Enable => true;
		protected virtual Rect PanelLeft => default;
		protected virtual Rect PanelRight => default;
		protected virtual Int2 CanvasCellCount => new(128, 128);
		protected virtual float ViewPadding => 256f;
		protected virtual Float2 ZoomRange => new(1f, 16f);
		protected virtual Pixel32 GridTint => new(0, 0, 0, 16);
		protected virtual Pixel32 BackgroundTint => new(0, 0, 0, 255);
		protected virtual float StartZoom => 5f;
		protected virtual float CanvasLerp => 12f;
		protected virtual bool FitCanvasOnStart => false;
		protected virtual float MouseWheelZoomIntensity => 50f;
		protected virtual float MouseDragZoomIntensity => -100f;

		// Short
		protected Int2 CanvasMinPos { get; set; } = default;
		protected Int2 CanvasMaxPos { get; set; } = default;
		protected Rect TargetCanvasRect => _TargetCanvasRect;
		protected Rect CanvasRect => _CanvasRect;
		protected bool MouseInPanel { get; private set; } = false;
		protected int MouseButton { get; private set; } = -1;
		protected bool MouseHeavyDragged { get; private set; } = false;
		protected bool MouseDownInPanel { get; set; } = false;
		protected Float2 MouseDownPosition_GUI { get; private set; } = default;
		protected Int2 MouseDownPosition_Global { get; private set; } = default;
		protected Rect CursorRect { get; private set; } = default;
		protected float Zoom { get; private set; } = 1f;

		// Data
		private Rect _TargetCanvasRect = default;
		private Rect _CanvasRect = default;
		private Rect GuiWindowRect = default;
		private double PrevTime = double.MinValue;
		private float DeltaTime = 1f;
		private bool CanvasInitialized = false;


		// MSG
		protected virtual void Update () {

			double time = EditorApplication.timeSinceStartup;
			DeltaTime = PrevTime > 0f ? (float)(time - PrevTime) : 0f;
			PrevTime = time;

			bool needRepaint = false;

			// Clamp
			_TargetCanvasRect.x = _TargetCanvasRect.x.Clamp(
				-(CanvasMaxPos.x + 1) * _TargetCanvasRect.width + PanelRight.width + GuiWindowRect.width - ViewPadding,
				-CanvasMinPos.x * _TargetCanvasRect.width + PanelLeft.width + ViewPadding
			);
			_TargetCanvasRect.y = _TargetCanvasRect.y.Clamp(
				(CanvasMinPos.y - 1) * _TargetCanvasRect.height + GuiWindowRect.height - ViewPadding,
				CanvasMaxPos.y * _TargetCanvasRect.height + ViewPadding
			);

			// Lerp
			float lerp = CanvasLerp > 0f ? (DeltaTime * CanvasLerp).Clamp01() : 1f;

			// Lerp X
			if (Mathf.Abs(_CanvasRect.x - _TargetCanvasRect.x) > 1f) {
				_CanvasRect.x = Mathf.LerpUnclamped(_CanvasRect.x, _TargetCanvasRect.x, lerp);
				needRepaint = true;
			} else if (_CanvasRect.NotAlmost(_TargetCanvasRect)) {
				_CanvasRect.x = _TargetCanvasRect.x;
				needRepaint = true;
			}

			// Lerp Y
			if (Mathf.Abs(_CanvasRect.y - _TargetCanvasRect.y) > 1f) {
				_CanvasRect.y = Mathf.LerpUnclamped(_CanvasRect.y, _TargetCanvasRect.y, lerp);
				needRepaint = true;
			} else if (_CanvasRect.NotAlmost(_TargetCanvasRect)) {
				_CanvasRect.y = _TargetCanvasRect.y;
				needRepaint = true;
			}

			// Lerp Width
			if (Mathf.Abs(_CanvasRect.width - _TargetCanvasRect.width) > 1f) {
				_CanvasRect.width = Mathf.LerpUnclamped(_CanvasRect.width, _TargetCanvasRect.width, lerp);
				needRepaint = true;
			} else if (_CanvasRect.NotAlmost(_TargetCanvasRect)) {
				_CanvasRect.width = _TargetCanvasRect.width;
				needRepaint = true;
			}

			// Lerp Height
			if (Mathf.Abs(_CanvasRect.height - _TargetCanvasRect.height) > 1f) {
				_CanvasRect.height = Mathf.LerpUnclamped(_CanvasRect.height, _TargetCanvasRect.height, lerp);
				needRepaint = true;
			} else if (_CanvasRect.NotAlmost(_TargetCanvasRect)) {
				_CanvasRect.height = _TargetCanvasRect.height;
				needRepaint = true;
			}

			// Final
			if (needRepaint) Repaint();
		}


		protected virtual void OnEnable () {
			Zoom = StartZoom;
			CanvasInitialized = false;
			MouseHeavyDragged = false;
		}


		protected virtual void OnGUI () {

			// Mouse
			if (Event.current.isMouse) {
				MouseInPanel = PanelLeft.Contains(Event.current.mousePosition) || PanelRight.Contains(Event.current.mousePosition);
			}

			if (Enable) {
				GUI_View();
				GUI_Mouse();
				DrawBackground();
				GUI_Gizmos();
			}

		}


		private void GUI_View () {

			using var _v = new GUILayout.VerticalScope(GUILayout.ExpandWidth(true));

			var windowCanvas = new Rect(0, 0, position.width, position.height).Shrink(PanelLeft.width, PanelRight.width, 0, 0);
			var canvasRect = windowCanvas.Fit((float)CanvasCellCount.x / CanvasCellCount.y);
			GuiWindowRect = windowCanvas;
			Zoom = Zoom.Clamp(ZoomRange.x, ZoomRange.y);

			// Init
			if (!CanvasInitialized && Event.current.type == EventType.Repaint) {
				CanvasInitialized = true;
				if (FitCanvasOnStart) {
					FitView();
				} else {
					_TargetCanvasRect.x = _CanvasRect.x = canvasRect.x;
					_TargetCanvasRect.y = _CanvasRect.y = canvasRect.y - canvasRect.height * (Zoom - 1f);
				}
			}

			// Scroll to Zoom
			if (!MouseInPanel) {
				// Get Delta
				float delta = 0f;
				if (Event.current.type == EventType.ScrollWheel) {
					delta = Event.current.delta.y / -(MouseWheelZoomIntensity / Zoom);
				} else if (
					Event.current.type == EventType.MouseDrag &&
					Event.current.button == 1 &&
					Event.current.control
				) {
					delta = Event.current.delta.y / (MouseDragZoomIntensity / Zoom);
				}
				// Zoom
				ApplyZoom(delta, canvasRect, Event.current.mousePosition);
			}
			_TargetCanvasRect.width = _CanvasRect.width = canvasRect.width * Zoom;
			_TargetCanvasRect.height = _CanvasRect.height = canvasRect.height * Zoom;

			// Drag to Move
			if (
				Event.current.type == EventType.MouseDrag &&
				(Event.current.button == 2 || (Event.current.button == 0 && Event.current.control))
			) {
				_TargetCanvasRect.x += Event.current.delta.x;
				_TargetCanvasRect.y += Event.current.delta.y;
				Repaint();
			}

		}


		private void GUI_Mouse () {

			// Mouse
			if (Event.current.isMouse) MouseButton = Event.current.button;

			switch (Event.current.type) {
				case EventType.MouseDown:
					MouseHeavyDragged = false;
					MouseDownPosition_GUI = Event.current.mousePosition;
					MouseDownPosition_Global = GUI_to_Global(Event.current.mousePosition, TargetCanvasRect);
					MouseDownInPanel = MouseInPanel;
					OnMouseDown();
					Repaint();
					break;
				case EventType.MouseDrag:
					if (!MouseHeavyDragged && Float2.Distance(Event.current.mousePosition, MouseDownPosition_GUI) > 12f) {
						MouseHeavyDragged = true;
					}
					OnMouseDrag();
					break;
				case EventType.MouseUp:
					OnMouseUp();
					MouseHeavyDragged = false;
					Repaint();
					break;
				case EventType.MouseEnterWindow:
				case EventType.MouseLeaveWindow:
					OnMouseEnterLeaveWindow();
					Repaint();
					break;
			}

		}


		private void GUI_Gizmos () {
			var mousePos = Event.current.mousePosition;
			var cellSize = new Float2(CanvasRect.width / CanvasCellCount.x, CanvasRect.height / CanvasCellCount.y);
			var cursorRect = new Rect(
				(mousePos.x - CanvasRect.x).UFloor(cellSize.x) + CanvasRect.x,
				(mousePos.y - CanvasRect.y).UFloor(cellSize.y) + CanvasRect.y,
				cellSize.x, cellSize.y
			);
			if (CursorRect.NotAlmost(cursorRect)) {
				Repaint();
			}
			CursorRect = cursorRect;
		}


		// OVERLAP
		protected virtual void OnMouseDown () { }
		protected virtual void OnMouseDrag () { }
		protected virtual void OnMouseUp () { }
		protected virtual void OnMouseEnterLeaveWindow () { }


		// API
		protected void DrawBackground () {

			var windowCanvas = new Rect(0, 0, position.width, position.height).Shrink(PanelLeft.width, PanelRight.width, 0, 0);

			// BG
			if (BackgroundTint.a > 0) {
				EditorGUI.DrawRect(windowCanvas, BackgroundTint);
			}

			// Grid
			if (GridTint.a > 0) {
				var cellSize = new Float2(CanvasRect.width / CanvasCellCount.x, CanvasRect.height / CanvasCellCount.y);
				float l = (windowCanvas.xMin - CanvasRect.x).UFloor(cellSize.x) + CanvasRect.x;
				float r = (windowCanvas.xMax - CanvasRect.x).UCeil(cellSize.x) + CanvasRect.x;
				float d = (windowCanvas.yMin - CanvasRect.y).UFloor(cellSize.y) + CanvasRect.y;
				float u = (windowCanvas.yMax - CanvasRect.y).UCeil(cellSize.y) + CanvasRect.y;
				for (float x = l; x < r + cellSize.x / 2f; x += cellSize.x) {
					EditorGUI.DrawRect(new(x, windowCanvas.yMin, 1f, windowCanvas.height), GridTint);
				}
				for (float y = d; y < u + cellSize.y / 2f; y += cellSize.y) {
					EditorGUI.DrawRect(new(windowCanvas.xMin, y, windowCanvas.width, 1f), GridTint);
				}
			}
		}


		protected void SetCanvasPositionImmediately (float x, float y) {
			_TargetCanvasRect.x = _CanvasRect.x = x;
			_TargetCanvasRect.y = _CanvasRect.y = y;
		}


		protected Int2 GUI_to_Global (Float2 guiPos, Rect canvasRect) {
			var size = new Float2(canvasRect.width / CanvasCellCount.x, canvasRect.height / CanvasCellCount.y);
			return new(
				((guiPos.x - canvasRect.x) / size.x).FloorToInt(),
				-((guiPos.y - canvasRect.y) / size.y).CeilToInt() + CanvasCellCount.y
			);
		}


		protected Float2 Global_to_GUI (Int2 globalCellPos, Rect canvasRect) {
			var size = new Float2(canvasRect.width / CanvasCellCount.x, canvasRect.height / CanvasCellCount.y);
			return new(
				globalCellPos.x * size.x + canvasRect.x,
				-(globalCellPos.y - CanvasCellCount.y) * size.y + canvasRect.y
			);
		}


		protected void FitView () {
			var windowCanvas = new Rect(0, 0, position.width, position.height).Shrink(PanelLeft.width, PanelRight.width, 0, 0);
			var canvasRect = windowCanvas.Fit((float)CanvasCellCount.x / CanvasCellCount.y);
			_TargetCanvasRect = _CanvasRect = canvasRect;
			Zoom = Mathf.Min(
				_CanvasRect.width / canvasRect.width,
				_CanvasRect.height / canvasRect.height
			).Clamp(
				ZoomRange.x, ZoomRange.y
			);
			ApplyZoom(-0.05f, canvasRect, windowCanvas.center);
		}


		// Canvas
		protected void MoveCanvasTo (float x, float y, float zoom) {
			var windowCanvas = new Rect(0, 0, position.width, position.height).Shrink(PanelLeft.width, PanelRight.width, 0, 0);
			var canvasRect = windowCanvas.Fit((float)CanvasCellCount.x / CanvasCellCount.y);
			_TargetCanvasRect.x = _CanvasRect.x = x;
			_TargetCanvasRect.y = _CanvasRect.y = y;
			Zoom = zoom.Clamp(ZoomRange.x, ZoomRange.y);
			_TargetCanvasRect.width = _CanvasRect.width = canvasRect.width * zoom;
			_TargetCanvasRect.height = _CanvasRect.height = canvasRect.height * zoom;
		}


		// LGC
		private void ApplyZoom (float delta, Rect canvasRect, Float2 pivot) {
			if (!delta.NotAlmostZero()) return;
			float oldZoom = Zoom;
			Zoom = (Zoom + delta).Clamp(ZoomRange.x, ZoomRange.y);
			_CanvasRect.x = _TargetCanvasRect.x -= Util.RemapUnclamped(
				_CanvasRect.x, _CanvasRect.x + canvasRect.width * oldZoom,
				0f, canvasRect.width * (Zoom - oldZoom),
				pivot.x
			);
			_CanvasRect.y = _TargetCanvasRect.y -= Util.RemapUnclamped(
				_CanvasRect.y, _CanvasRect.y + canvasRect.height * oldZoom,
				0f, canvasRect.height * (Zoom - oldZoom),
				pivot.y
			);
			Repaint();
		}


	}
}