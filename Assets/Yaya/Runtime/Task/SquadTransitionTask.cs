using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class SquadTransitionTask : TaskItem {

		public static readonly int TYPE_ID = typeof(SquadTransitionTask).AngeHash();
		public int NewZ = 0;
		private bool Front = true;
		private bool VigOn = false;
		private float VigRound = 0f;
		private float VigFeather = 1f;

		public override TaskResult FrameUpdate () {

			const int DURATION = 60;
			const int FIRST_DURATION = 24;
			bool useVig = Game.Current.WorldConfig.UseVignetteOnSquadTransition;
			bool useParallax = Game.Current.WorldConfig.UseParallaxOnSquadTransition;

			if (LocalFrame == 0) {
				// Player
				var game = Game.Current;
				var player = ePlayer.Current;
				if (player != null) {
					player.RenderEnterDoor(DURATION, NewZ < game.ViewZ);
				}
				Front = NewZ > game.ViewZ;
				// Vig
				if (useVig) {
					VigFeather = VignetteEffect.GetFeather();
					VigRound = VignetteEffect.GetRound();
					VigOn = game.UseScreenEffects;
					ScreenEffect.SetEffectEnable(VignetteEffect.TYPE_ID, true);
					VignetteEffect.SetFeather(0f);
					VignetteEffect.SetRound(1f);
				}
			}
			if (LocalFrame == FIRST_DURATION) {
				// Set View Z
				Game.Current.SetViewZ(NewZ);
			}
			if (LocalFrame == FIRST_DURATION + 1 && useParallax) {
				// Add Squad Effect
				var game = Game.Current;
				int para = game.WorldConfig.SquadBehindParallax;
				byte alpha = game.WorldConfig.SquadBehindAlpha;
				var effect = SquadTransitionEffect.Instance;
				effect.Duration = DURATION - FIRST_DURATION;
				effect.Scale = Front ? 1000f / para : para / 1000f;
				effect.Alpha = alpha / 255f;
				CellRenderer.RemoveEffect<SquadTransitionEffect>();
				CellRenderer.AddEffect(effect);
			}

			// Update Vig Effect
			if (useVig) {
				var player = ePlayer.Current;
				float offsetX = 0f;
				float offsetY = 0f;
				if (player != null) {
					var cameraRect = CellRenderer.CameraRect;
					offsetX = Util.RemapUnclamped(cameraRect.xMin, cameraRect.xMax, -1f, 1f, player.X);
					offsetY = Util.RemapUnclamped(cameraRect.yMin, cameraRect.yMax, -1f, 1f, player.Y + player.Height / 2f);
					VignetteEffect.SetOffsetX(offsetX);
					VignetteEffect.SetOffsetY(offsetY);
				}
				if (LocalFrame < FIRST_DURATION) {
					float radius = Util.RemapUnclamped(0, FIRST_DURATION - 1, 1f, 0f, LocalFrame);
					VignetteEffect.SetRadius(radius * radius);
					return TaskResult.Continue;
				} else if (LocalFrame < DURATION) {
					float t01 = Util.RemapUnclamped(FIRST_DURATION, DURATION - 1, 0f, 1f, LocalFrame);
					float radius = 1f - t01;
					radius = 1f - radius * radius;
					if (!VigOn) radius *= 2f;
					VignetteEffect.SetRadius(radius);
					VignetteEffect.SetFeather(Mathf.Lerp(0f, VigFeather, t01));
					VignetteEffect.SetRound(Mathf.Lerp(1f, VigRound, t01));
					VignetteEffect.SetOffsetX(Mathf.Lerp(offsetX, 0f, t01));
					VignetteEffect.SetOffsetY(Mathf.Lerp(offsetY, 0f, t01));
					return TaskResult.Continue;
				} else {
					VignetteEffect.SetFeather(VigFeather);
					VignetteEffect.SetOffsetX(0f);
					VignetteEffect.SetOffsetY(0f);
					VignetteEffect.SetRound(VigRound);
					ScreenEffect.SetEffectEnable(VignetteEffect.TYPE_ID, VigOn);
					VignetteEffect.SetRadius(1f);
					return TaskResult.End;
				}
			} else {
				return LocalFrame < DURATION ? TaskResult.Continue : TaskResult.End;
			}
		}

	}



	public class SquadTransitionEffect : CellEffect {


		public static readonly SquadTransitionEffect Instance = new();


		public float Scale { get; set; } = 1f;
		public float Alpha { get; set; } = 1f;


		public override void Perform (Cell[] cells, int cellCount, int layerIndex) {

			if (layerIndex == CellRenderer.LayerCount - 1) return;

			float z01 = Mathf.InverseLerp(0, Duration, LocalFrame);
			float z10 = 1f - z01;
			Vector2 center = CellRenderer.CameraRect.center;
			var scl = Mathf.LerpUnclamped(Scale, 1f, 1f - z10 * z10);

			// Behind
			PerformLogic(
				cells, center, 0, SortedIndex, scl,
				Scale > 1f ? Mathf.LerpUnclamped(1f, Alpha, z01) : z01 * Alpha
			);

			// Current
			PerformLogic(cells, center, SortedIndex + 1, cellCount - 1, scl, 1f);

		}


		private void PerformLogic (Cell[] cells, Vector2 center, int startIndex, int endIndex, float scale, float alpha) {
			Color32 c;
			for (int i = startIndex; i <= endIndex; i++) {
				var cell = cells[i];
				c = cell.Color;
				c.a = (byte)(alpha * 255);
				cell.Color = c;
				cell.X = cell.X.LerpTo(center.x.FloorToInt(), 1f - scale);
				cell.Y = cell.Y.LerpTo(center.y.FloorToInt(), 1f - scale);
				cell.Width = (cell.Width * scale).CeilToInt();
				cell.Height = (cell.Height * scale).CeilToInt();
			}
		}


	}
}
