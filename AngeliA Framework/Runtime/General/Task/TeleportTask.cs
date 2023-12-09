using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class TeleportTask : TaskItem {


		// Api
		public static readonly int TYPE_ID = typeof(TeleportTask).AngeHash();
		public Vector2Int TeleportFrom { get; set; } = default;
		public Vector3Int TeleportTo { get; set; } = default;
		public int WaitDuration { get; set; } = 30;
		public int Duration { get; set; } = 60;
		public bool UseVignette { get; set; } = true;
		public bool UseParallax { get; set; } = true;
		public bool UsePortalEffect { get; set; } = false;
		public Entity TeleportEntity { get; set; } = null;

		// Data
		private Entity CurrentTeleportEntity = null;
		private bool ToFront = true;
		private float VigRound = 0f;
		private float VigFeather = 1f;

		// MSG
		public override void OnStart () {
			base.OnStart();
			CurrentTeleportEntity = TeleportEntity;
			TeleportEntity = null;
			WaitDuration = WaitDuration.Clamp(0, Duration - 1);
		}


		public override TaskResult FrameUpdate () {

			bool useVig = UseVignette;
			bool useParallax = UseParallax;

			// Start
			if (LocalFrame == 0) {
				// Player
				ToFront = TeleportTo.z > Stage.ViewZ;
				// Vig
				if (useVig) {
					VigFeather = VignetteEffect.GetFeather();
					VigRound = VignetteEffect.GetRound();
					ScreenEffect.SetEffectEnable(VignetteEffect.TYPE_ID, true);
					VignetteEffect.SetFeather(0f);
					VignetteEffect.SetRound(1f);
				}
			}

			// Teleport
			if (LocalFrame == WaitDuration) {
				int offsetX = TeleportFrom.x - Stage.ViewRect.xMin;
				int offsetY = TeleportFrom.y - Stage.ViewRect.yMin;
				Stage.SetViewPositionDelay(TeleportTo.x - offsetX, TeleportTo.y - offsetY, 1000, int.MaxValue);
				Stage.SetViewZ(TeleportTo.z);
				if (CurrentTeleportEntity != null) {
					CurrentTeleportEntity.X = TeleportTo.x;
					CurrentTeleportEntity.Y = TeleportTo.y;
				}
			}

			// Add Squad Effect
			if (LocalFrame > WaitDuration && useParallax) {
				const int PARA = Const.SQUAD_BEHIND_PARALLAX;
				float scale = ToFront ? 1000f / PARA : PARA / 1000f;
				float z10 = 1f - Mathf.InverseLerp(0, Duration, LocalFrame);
				Vector2Int center = CellRenderer.CameraRect.center.CeilToInt();
				var lerp = Mathf.LerpUnclamped(scale, 1f, 1f - z10 * z10);
				if (CellRenderer.GetCells(RenderLayer.BEHIND, out var cells, out int count)) {
					MapEffectLogic(cells, center, count, scale, lerp, true, ToFront);
				}
				for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
					if (
						layer == RenderLayer.WALLPAPER ||
						layer == RenderLayer.UI ||
						layer == RenderLayer.TOP_UI
					) continue;
					if (CellRenderer.GetCells(layer, out cells, out count)) {
						MapEffectLogic(cells, center, count, scale, lerp, false, ToFront);
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
		public static TeleportTask Teleport (int fromX, int fromY, int toX, int toY, int toZ) {
			if (FrameTask.HasTask()) return null;
			if (FrameTask.TryAddToLast(TYPE_ID, out var task) && task is TeleportTask svTask) {
				svTask.TeleportFrom = new Vector2Int(fromX, fromY);
				svTask.TeleportTo = new Vector3Int(toX, toY, toZ);
				svTask.WaitDuration = 6;
				svTask.Duration = 24;
				svTask.UseParallax = true;
				svTask.UseVignette = false;
				svTask.TeleportEntity = null;
				svTask.UsePortalEffect = false;
				return svTask;
			}
			return null;
		}

		// LGC
		private static void MapEffectLogic (
			Cell[] cells, Vector2Int center, int count, float scale, float lerp, bool isBehind, bool toBehind
		) {
			Color32 c;
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				c = cell.Color;
				if (isBehind) {
					if (toBehind) {
						c.a = (byte)Util.Remap(scale, 1f, 0, c.a, lerp);
					} else {
						c.a = (byte)Util.Remap(scale, 1f, 255, c.a, lerp);
					}
				}
				cell.Color = c;
				if (cell.Rotation == 0) {
					cell.X = Mathf.LerpUnclamped(cell.X - cell.PivotX * cell.Width, center.x, 1f - lerp).FloorToInt();
					cell.Y = Mathf.LerpUnclamped(cell.Y - cell.PivotY * cell.Height, center.y, 1f - lerp).FloorToInt();
					cell.Width = cell.Width > 0 ? (cell.Width * lerp).CeilToInt() : (cell.Width * lerp).FloorToInt();
					cell.Height = cell.Height > 0 ? (cell.Height * lerp).CeilToInt() : (cell.Height * lerp).FloorToInt();
					cell.PivotX = 0;
					cell.PivotY = 0;
				} else {
					cell.X = Mathf.LerpUnclamped(cell.X, center.x, 1f - lerp).FloorToInt();
					cell.Y = Mathf.LerpUnclamped(cell.Y, center.y, 1f - lerp).FloorToInt();
					cell.Width = (cell.Width * lerp).CeilToInt();
					cell.Height = (cell.Height * lerp).CeilToInt();
				}
			}
		}

	}
}