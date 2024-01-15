using UnityEngine;
namespace AngeliaForUnity {
	public class ScreenEffect : MonoBehaviour {
		public Material Materal;
		public float EnableTime;
		public void SetEnable (bool enable) {
			enabled = enable;
			EnableTime = enable ? Time.time : -1f;
		}
		private void OnRenderImage (RenderTexture source, RenderTexture destination) {
			if (Materal != null) {
				Graphics.Blit(source, destination, Materal);
			} else {
				Graphics.Blit(source, destination);
			}
		}
	}
}