using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class GUISkin {

	public Color32 DeleteTint = Color32.RED_BETTER;
	public Color32 ErrorTint = Color32.RED_BETTER;
	public Color32 LinkTint = new(133, 196, 255, 255);
	public Color32 LinkTintHover = new(163, 226, 255, 255);

	public Color32 HighlightColor = Color32.GREEN;
	public Color32 HighlightColorAlt = Color32.GREEN_DARK;

	public Color32 Background = Color32.GREY_38;

	public Color32 GizmosNormal = Color32.BLACK;
	public Color32 GizmosSelecting = Color32.WHITE;
	public Color32 GizmosDotted = Color32.BLACK;
	public Color32 GizmosDottedAlt = Color32.WHITE;
	public Color32 GizmosCursor = Color32.WHITE;
	public Color32 GizmosCursorAlt = Color32.BLACK;
	public Color32 GizmosDragging = Color32.WHITE;
	public Color32 GizmosDraggingAlt = Color32.BLACK;

}