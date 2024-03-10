using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework;

public class GUIScope : System.IDisposable {

	private enum ScopeType { Layer, Color, ContentColor, BodyColor, Enable, }

	private static readonly GUIScope LayerInstance = new(ScopeType.Layer);
	private static readonly GUIScope ColorInstance = new(ScopeType.Color);
	private static readonly GUIScope ContentColorInstance = new(ScopeType.ContentColor);
	private static readonly GUIScope BodyColorInstance = new(ScopeType.BodyColor);
	private static readonly GUIScope EnableInstance = new(ScopeType.Enable);

	private readonly ScopeType Type;
	private Color32 ColorData;
	private int IntData;

	private GUIScope (ScopeType type) => Type = type;

	public static GUIScope Layer (int layer) {
		LayerInstance.IntData = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(layer);
		return LayerInstance;
	}

	public static GUIScope Color (Color32 color) {
		ColorInstance.ColorData = GUI.Color;
		GUI.Color = color;
		return ColorInstance;
	}

	public static GUIScope ContentColor (Color32 color) {
		ContentColorInstance.ColorData = GUI.ContentColor;
		GUI.ContentColor = color;
		return ContentColorInstance;
	}

	public static GUIScope BodyColor (Color32 color) {
		BodyColorInstance.ColorData = GUI.BodyColor;
		GUI.BodyColor = color;
		return BodyColorInstance;
	}

	public static GUIScope BodyColor (bool enable) {
		EnableInstance.IntData = GUI.Enable ? 1 : 0;
		GUI.Enable = enable;
		return EnableInstance;
	}

	public void Dispose () {
		switch (Type) {
			case ScopeType.Layer:
				Renderer.SetLayer(IntData);
				break;
			case ScopeType.Color:
				GUI.Color = ColorData;
				break;
			case ScopeType.ContentColor:
				GUI.ContentColor = ColorData;
				break;
			case ScopeType.BodyColor:
				GUI.BodyColor = ColorData;
				break;
			case ScopeType.Enable:
				GUI.Enable = IntData == 1;
				break;
		}
	}
}