using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class TeleportTask : TaskItem {


		// Api
		private static readonly int TYPE_ID = typeof(TeleportTask).AngeHash();
		public Int2 TeleportFrom { get; set; } = default;
		public Int3 TeleportTo { get; set; } = default;
		public int WaitDuration { get; set; } = 30;
		public int Duration { get; set; } = 60;
		public bool UseVignette { get; set; } = true;
		public bool UseParallax { get; set; } = true;

		// Data
		private MapChannel? NewChannel = null;
		private string ChannelName = "";
		private bool ToBehind = true;
		private float VigRound = 0f;
		private float VigFeather = 1f;

		// MSG
		public override void OnStart () {
			base.OnStart();
			WaitDuration = WaitDuration.Clamp(0, Duration - 1);
			ToBehind = TeleportTo.z > Stage.ViewZ;
			if (UseVignette) {
				VigFeather = VignetteEffect.GetFeather();
				VigRound = VignetteEffect.GetRound();
				ScreenEffect.SetEffectEnable(VignetteEffect.TYPE_ID, true);
				VignetteEffect.SetFeather(0f);
				VignetteEffect.SetRound(1f);
			}
		}

		public override TaskResult FrameUpdate () {

			bool useVig = UseVignette;
			bool useParallax = UseParallax;

			// Teleport
			if (LocalFrame == WaitDuration) {
				// Channel
				if (NewChannel.HasValue) {
					WorldSquad.SetMapChannel(NewChannel.Value, ChannelName);
				}
				// Position
				int offsetX = TeleportFrom.x - Stage.ViewRect.xMin;
				int offsetY = TeleportFrom.y - Stage.ViewRect.yMin;
				Stage.SetViewPositionDelay(TeleportTo.x - offsetX, TeleportTo.y - offsetY, 1000, int.MaxValue);
				Stage.SetViewZ(TeleportTo.z);
				var player = Player.Selecting;
				if (player != null) {
					player.X = TeleportTo.x;
					player.Y = TeleportTo.y;
				}
			}

			// Add Squad Effect
			if (useParallax && LocalFrame > WaitDuration) {
				const int PARA = Const.SQUAD_BEHIND_PARALLAX;
				float scale = ToBehind ? 1000f / PARA : PARA / 1000f;
				float z01 = Mathf.InverseLerp(WaitDuration, Duration, LocalFrame);
				float lerp = Mathf.LerpUnclamped(scale, 1f, z01);
				var center = CellRenderer.CameraRect.center.CeilToInt();
				// Behind
				if (CellRenderer.GetCells(RenderLayer.BEHIND, out var cells, out int count)) {
					MapEffectLogic(cells, center, count, scale, lerp, true, ToBehind);
				}
				// Front
				for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
					if (
						layer == RenderLayer.WALLPAPER ||
						layer == RenderLayer.UI ||
						layer == RenderLayer.TOP_UI ||
						layer == RenderLayer.BEHIND
					) continue;
					if (CellRenderer.GetCells(layer, out cells, out count)) {
						MapEffectLogic(cells, center, count, scale, lerp, false, ToBehind);
					}
				}
			}

			// Update Vig Effect
			if (useVig) {
				var cameraRect = CellRenderer.CameraRect;
				if (LocalFrame < WaitDuration) {
					float radius = Util.RemapUnclamped(0, WaitDuration - 1, 1f, 0f, LocalFrame);
					float offsetX = Util.RemapUnclamped(cameraRect.xMin, cameraRect.xMax, -1f, 1f, TeleportFrom.x);
					float offsetY = Util.RemapUnclamped(cameraRect.yMin, cameraRect.yMax, -1f, 1f, TeleportFrom.y);
					VignetteEffect.SetRadius(radius * radius);
					VignetteEffect.SetOffsetX(offsetX);
					VignetteEffect.SetOffsetY(offsetY);
					return TaskResult.Continue;
				} else if (LocalFrame < Duration) {
					float t01 = Util.RemapUnclamped(WaitDuration, Duration - 1, 0f, 1f, LocalFrame);
					float radius = 1f - t01;
					radius = 1f - radius * radius;
					radius *= 2f;
					float offsetX = Util.RemapUnclamped(cameraRect.xMin, cameraRect.xMax, -1f, 1f, TeleportTo.x);
					float offsetY = Util.RemapUnclamped(cameraRect.yMin, cameraRect.yMax, -1f, 1f, TeleportTo.y);
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
					ScreenEffect.SetEffectEnable(VignetteEffect.TYPE_ID, false);
					VignetteEffect.SetRadius(1f);
					return TaskResult.End;
				}
			} else {
				return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
			}
		}

		// API
		public static TeleportTask Teleport (
			int fromX, int fromY, int toX, int toY, int toZ,
			int waitDuration = 6, int duration = 24, bool useVignette = false, bool useParallax = true,
			MapChannel? newChannel = null, string channelName = ""
		) {
			if (FrameTask.HasTask()) return null;
			if (FrameTask.TryAddToLast(TYPE_ID, out var task) && task is TeleportTask svTask) {
				svTask.TeleportFrom = new Int2(fromX, fromY);
				svTask.TeleportTo = new Int3(toX, toY, toZ);
				svTask.WaitDuration = waitDuration;
				svTask.Duration = duration;
				svTask.UseParallax = useParallax;
				svTask.UseVignette = useVignette;
				svTask.NewChannel = newChannel;
				svTask.ChannelName = channelName;
				var player = Player.Selecting;
				if (player != null) {
					player.X = fromX;
					player.Y = fromY;
					player.Stop();
					player.EnterTeleportState(svTask.Duration, Stage.ViewZ > toZ, false);
					player.VelocityX = 0;
					player.VelocityY = 0;
				}
				return svTask;
			}
			return null;
		}

		// LGC
		private static void MapEffectLogic (Cell[] cells, Int2 center, int count, float scale, float lerp, bool isBehind, bool toBehind) {
			// Behind Tint
			if (isBehind) {
				for (int i = 0; i < count; i++) {
					var cell = cells[i];
					var c = cell.Color;
					if (toBehind) {
						c.a = (byte)Util.Remap(scale, 1f, 0, c.a, lerp).Clamp(0, 255);
					} else {
						c.a = (byte)Util.Remap(scale, 1f, 255, c.a, lerp).Clamp(0, 255);
					}
					cell.Color = c;
				}
			}
			// Scale
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				if (cell.Rotation == 0) {
					cell.X = Mathf.LerpUnclamped(center.x, cell.X - cell.PivotX * cell.Width, lerp).FloorToInt();
					cell.Y = Mathf.LerpUnclamped(center.y, cell.Y - cell.PivotY * cell.Height, lerp).FloorToInt();
					cell.Width = cell.Width > 0 ? (cell.Width * lerp).CeilToInt() : (cell.Width * lerp).FloorToInt();
					cell.Height = cell.Height > 0 ? (cell.Height * lerp).CeilToInt() : (cell.Height * lerp).FloorToInt();
					cell.PivotX = 0;
					cell.PivotY = 0;
				} else {
					cell.X = Mathf.LerpUnclamped(center.x, cell.X, lerp).FloorToInt();
					cell.Y = Mathf.LerpUnclamped(center.y, cell.Y, lerp).FloorToInt();
					cell.Width = (cell.Width * lerp).CeilToInt();
					cell.Height = (cell.Height * lerp).CeilToInt();
				}
			}
		}

	}
}