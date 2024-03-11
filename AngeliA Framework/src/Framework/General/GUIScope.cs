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
		public GUIScope StartInt (int oldValue) {
			if (CurrentIndex >= COUNT) {
#if DEBUG
				Util.LogWarning("Too many layers of GUIScope");
#endif
				return null;
			}
			var scope = Scopes[CurrentIndex];
			CurrentIndex++;
			scope.IntData = oldValue;
			return scope;
		}
		public GUIScope StartColor (Color32 oldValue) {
			if (CurrentIndex >= COUNT) {
#if DEBUG
				Util.LogWarning("Too many layers of GUIScope");
#endif
				return null;
			}
			var scope = Scopes[CurrentIndex];
			CurrentIndex++;
			scope.ColorData = oldValue;
			return scope;
		}
		public void End () => CurrentIndex--;
	}

	private enum ScopeType { None, Layer, Color, ContentColor, BodyColor, Enable, }

	private static readonly GUIScope EmptyScope = new(ScopeType.None);
	private static readonly ScopeGroup LayerInstance = new(ScopeType.Layer);
	private static readonly ScopeGroup ColorInstance = new(ScopeType.Color);
	private static readonly ScopeGroup ContentColorInstance = new(ScopeType.ContentColor);
	private static readonly ScopeGroup BodyColorInstance = new(ScopeType.BodyColor);
	private static readonly ScopeGroup EnableInstance = new(ScopeType.Enable);

	private readonly ScopeType Type;
	private Color32 ColorData;
	private int IntData;

	private GUIScope (ScopeType type) => Type = type;

	public static GUIScope Layer (int layer) {
		var result = LayerInstance.StartInt(Renderer.CurrentLayerIndex);
		if (result == null) return EmptyScope;
		Renderer.SetLayer(layer);
		return result;
	}

	public static GUIScope Color (Color32 color) {
		var result = ColorInstance.StartColor(GUI.Color);
		if (result == null) return EmptyScope;
		GUI.Color = color;
		return result;
	}

	public static GUIScope ContentColor (Color32 color) {
		var result = ContentColorInstance.StartColor(GUI.ContentColor);
		if (result == null) return EmptyScope;
		GUI.ContentColor = color;
		return result;
	}

	public static GUIScope BodyColor (Color32 color) {
		var result = BodyColorInstance.StartColor(GUI.BodyColor);
		if (result == null) return EmptyScope;
		GUI.BodyColor = color;
		return result;
	}

	public static GUIScope BodyColor (bool enable) {
		var result = EnableInstance.StartInt(GUI.Enable ? 1 : 0);
		if (result == null) return EmptyScope;
		GUI.Enable = enable;
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
		}
	}

}