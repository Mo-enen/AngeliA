using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public abstract class AngeliaScreenEffect : MonoBehaviour {


		// Api
		public Material Material = null;
		public virtual int Order => 0;
		public string DisplayName { get; internal set; } = "";

		// MSG
		private void OnRenderImage (RenderTexture source, RenderTexture destination) {
			if (Material != null) {
				Graphics.Blit(source, destination, Material);
			} else {
				Graphics.Blit(source, destination);
			}
		}


		// API
		public abstract Shader GetShader ();


	}


	public static class ScreenEffect {



		// Data
		private static readonly Dictionary<int, AngeliaScreenEffect> Pool = new();
		private static readonly List<AngeliaScreenEffect> Effects = new();


		// API
		[OnGameInitialize(64)]
		public static void Initialize () {

			var mainCamera = Game.GameCamera;

			// Get Effects
			Effects.Clear();
			foreach (var type in typeof(AngeliaScreenEffect).AllChildClass()) {
				var effect = mainCamera.gameObject.AddComponent(type) as AngeliaScreenEffect;
				effect.enabled = false;
				var shader = effect.GetShader();
				if (shader == null) continue;
				effect.Material = new Material(shader) {
					name = shader.name,
					enableInstancing = true,
					mainTextureOffset = Float2.zero,
					mainTextureScale = Float2.one,
					doubleSidedGI = false,
					renderQueue = 3000
				};
				effect.DisplayName = Util.GetDisplayName(type.Name);
				Effects.Add(effect);
			}
			Effects.Sort((a, b) => a.Order.CompareTo(b.Order));

			// Fill Pool
			Pool.Clear();
			foreach (var effect in Effects) {
				Pool.TryAdd(effect.GetType().AngeHash(), effect);
			}
		}


		public static void SetEffectEnable (int id, bool enable) {
			if (Pool.TryGetValue(id, out var effect) && effect.enabled != enable) {
				effect.enabled = enable;
			}
		}


		public static bool GetEffectEnable (int id) => Pool.TryGetValue(id, out var effect) && effect.enabled;


		public static Material GetEffectMaterial (int id) => Pool.TryGetValue(id, out var effect) ? effect.Material : null;


		public static IEnumerator<AngeliaScreenEffect> GetEnumerator () => Effects.GetEnumerator();


	}
}