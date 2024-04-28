using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class GUISkin {


	public Color32 HighlightColor = Color32.GREEN;
	public Color32 HighlightColorAlt = Color32.GREEN_DARK;

	public Color32 Background = Color32.GREY_38;
	public Color32 Background_Panel = Color32.GREY_20;

	public Color32 GizmosNormal = Color32.BLACK;
	public Color32 GizmosSelecting = Color32.WHITE;
	public Color32 GizmosDotted = Color32.BLACK;
	public Color32 GizmosDottedAlt = Color32.WHITE;
	public Color32 GizmosCursor = Color32.WHITE;
	public Color32 GizmosCursorAlt = Color32.BLACK;
	public Color32 GizmosDragging = Color32.WHITE;
	public Color32 GizmosDraggingAlt = Color32.BLACK;

	public void LoadColorFromSheet (Sheet sheet) {
		foreach (var (field, _) in this.ForAllFields<Color32>(BindingFlags.Instance | BindingFlags.Public)) {
			int fieldID = field.Name.AngeHash();
			if (sheet.SpritePool.TryGetValue(fieldID, out var sprite)) {
				field.SetValue(this, sprite.SummaryTint);
			} else {
				field.SetValue(this, field.GetValue(Default));
			}
		}
	}

	public void LoadColorFromSkin (GUISkin otherSkin) {
		foreach (var (field, _) in this.ForAllFields<Color32>(BindingFlags.Instance | BindingFlags.Public)) {
			field.SetValue(this, field.GetValue(otherSkin));
		}
	}

}