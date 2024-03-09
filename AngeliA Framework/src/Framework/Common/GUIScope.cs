using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Framework {


	public abstract class GUIScope : System.IDisposable {
		public abstract void Dispose ();
	}

	public class LayerScope : GUIScope {
		private static readonly LayerScope Instance = new();
		private int OldLayer;
		public static LayerScope Start (int layer) {
			Instance.OldLayer = Renderer.CurrentLayerIndex;
			Renderer.SetLayer(layer);
			return Instance;
		}
		public override void Dispose () => Renderer.SetLayer(OldLayer);
	}

	public class ColorScope : GUIScope {
		private static readonly ColorScope Instance = new();
		private Color32 OldColor;
		public static ColorScope Start (Color32 color) {
			Instance.OldColor = GUI.Color;
			GUI.Color = color;
			return Instance;
		}
		public override void Dispose () => GUI.Color = Instance.OldColor;
	}

	public class BodyColorScope : GUIScope {
		private static readonly BodyColorScope Instance = new();
		private Color32 OldColor;
		public static BodyColorScope Start (Color32 color) {
			Instance.OldColor = GUI.BodyColor;
			GUI.BodyColor = color;
			return Instance;
		}
		public override void Dispose () => GUI.BodyColor = Instance.OldColor;
	}

	public class ContentColorScope : GUIScope {
		private static readonly ContentColorScope Instance = new();
		private Color32 OldColor;
		public static ContentColorScope Start (Color32 color) {
			Instance.OldColor = GUI.ContentColor;
			GUI.ContentColor = color;
			return Instance;
		}
		public override void Dispose () => GUI.ContentColor = Instance.OldColor;
	}

	public class EnableScope : GUIScope {
		private static readonly EnableScope Instance = new();
		private bool OldEnable;
		public static EnableScope Start (bool enable) {
			Instance.OldEnable = GUI.Enable;
			GUI.Enable = enable;
			return Instance;
		}
		public override void Dispose () => GUI.Enable = Instance.OldEnable;
	}

}