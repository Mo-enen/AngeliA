using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public class TeleportTask : Task {


	// Api
	private static readonly int TYPE_ID = typeof(TeleportTask).AngeHash();
	public Int2 TeleportFrom { get; set; } = default;
	public Int3 TeleportTo { get; set; } = default;
	public int WaitDuration { get; set; } = 30;
	public int Duration { get; set; } = 60;
	public bool UseVignette { get; set; } = true;
	public bool UseParallax { get; set; } = true;

	// Data
	private bool ToBehind = true;


	// MSG
	public override void OnStart () {
		base.OnStart();
		WaitDuration = WaitDuration.Clamp(0, Duration - 1);
		ToBehind = TeleportTo.z > Stage.ViewZ;
	}


	public override TaskResult FrameUpdate () {

		int teleFrame = Duration - (Duration - WaitDuration) / 2;

		// Parallax
		if (UseParallax && LocalFrame > WaitDuration) {
			int PARA = Universe.BuiltInInfo.WorldBehindParallax;
			float scale = ToBehind ? 1000f / PARA : PARA / 1000f;
			var center = Renderer.CameraRect.CenterInt();
			float lerp = Util.InverseLerp(WaitDuration + 1, Duration, LocalFrame);
			if (LocalFrame <= teleFrame) {
				// First Para
				float localLerp = lerp * 2f;
				float ease = Util.LerpUnclamped(1f, 1f / scale, Ease.InOutSine(lerp));

				// Behind
				if (Renderer.GetCells(RenderLayer.BEHIND, out var cells, out int count)) {
					ParaLogic(cells, center, count, ease);
					if (ToBehind) {
						for (int i = 0; i < count; i++) {
							var cell = cells[i];
							cell.Color.a = (byte)Util.LerpUnclamped(cell.Color.a, 255, localLerp).Clamp(0, 255);
						}
					} else {
						var tint = Renderer.GetLayerTint(RenderLayer.BEHIND);
						tint.a = (byte)Util.LerpUnclamped(255, 0, localLerp).Clamp(0, 255);
						Renderer.SetLayerTint(RenderLayer.BEHIND, tint);
					}
				}

				// Front
				for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
					if (
						layer == RenderLayer.WALLPAPER ||
						layer == RenderLayer.UI ||
						layer == RenderLayer.BEHIND
					) continue;
					if (Renderer.GetCells(layer, out cells, out count)) {
						ParaLogic(cells, center, count, ease);
					}
					if (ToBehind) {
						var tint = Renderer.GetLayerTint(layer);
						tint.a = (byte)Util.LerpUnclamped(255, 0, localLerp).Clamp(0, 255);
						Renderer.SetLayerTint(layer, tint);
					}
				}

				// Lightmap
				LightingSystem.ForceCameraScale(ease);
				LightingSystem.ForceAirLerp(localLerp);

			} else {
				// Second Para
				float localLerp = lerp * 2f - 1f;
				float ease = Util.LerpUnclamped(scale, 1f, Ease.InOutSine(lerp));

				// Behind
				if (Renderer.GetCells(RenderLayer.BEHIND, out var cells, out int count)) {
					ParaLogic(cells, center, count, ease);
					if (ToBehind) {
						var tint = Renderer.GetLayerTint(RenderLayer.BEHIND);
						tint.a = (byte)Util.LerpUnclamped(0, 255, localLerp).Clamp(0, 255);
						Renderer.SetLayerTint(RenderLayer.BEHIND, tint);
					} else {
						for (int i = 0; i < count; i++) {
							var cell = cells[i];
							cell.Color.a = (byte)Util.LerpUnclamped(255, cell.Color.a, localLerp).Clamp(0, 255);
						}
					}
				}

				// Front
				for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
					if (
						layer == RenderLayer.WALLPAPER ||
						layer == RenderLayer.UI ||
						layer == RenderLayer.BEHIND
					) continue;
					if (Renderer.GetCells(layer, out cells, out count)) {
						ParaLogic(cells, center, count, ease);
					}
					if (!ToBehind) {
						var tint = Renderer.GetLayerTint(layer);
						tint.a = (byte)Util.LerpUnclamped(0, 255, localLerp).Clamp(0, 255);
						Renderer.SetLayerTint(layer, tint);
					}
				}

				// Lightmap
				LightingSystem.ForceCameraScale(ease);
				LightingSystem.ForceAirLerp(1f - (localLerp * 2f).Clamp01());

			}

		}

		// Teleport
		if (LocalFrame == teleFrame - 1) {
			// Position
			int offsetX = TeleportFrom.x - Stage.ViewRect.xMin;
			int offsetY = TeleportFrom.y - Stage.ViewRect.yMin;
			Stage.SetViewPositionDelay(TeleportTo.x - offsetX, TeleportTo.y - offsetY, 1000, int.MaxValue);
			Stage.SetViewZ(TeleportTo.z);
			var player = PlayerSystem.Selecting;
			if (player != null) {
				player.X = TeleportTo.x;
				player.Y = TeleportTo.y;
			}
		}

		// Update Vig Effect
		if (UseVignette) {
			var cameraRect = Renderer.CameraRect;
			if (LocalFrame < teleFrame) {
				float radius = Util.RemapUnclamped(0, teleFrame - 1, 1f, 0f, LocalFrame);
				float offsetX = Util.RemapUnclamped(cameraRect.xMin, cameraRect.xMax, -1f, 1f, TeleportFrom.x);
				float offsetY = Util.RemapUnclamped(cameraRect.yMin, cameraRect.yMax, -1f, 1f, TeleportFrom.y);
				Game.PassEffect_Vignette(radius * radius, 0f, offsetX, offsetY, 1f);
				return TaskResult.Continue;
			} else if (LocalFrame < Duration) {
				float t01 = Util.RemapUnclamped(teleFrame, Duration - 1, 0f, 1f, LocalFrame);
				float radius = 1f - t01;
				radius = 1f - radius * radius;
				radius *= 2f;
				float offsetX = Util.RemapUnclamped(cameraRect.xMin, cameraRect.xMax, -1f, 1f, TeleportTo.x);
				float offsetY = Util.RemapUnclamped(cameraRect.yMin, cameraRect.yMax, -1f, 1f, TeleportTo.y);
				Game.PassEffect_Vignette(
					radius, 0f,
					Util.Lerp(offsetX, 0f, t01),
					Util.Lerp(offsetY, 0f, t01),
					1f
				);
				return TaskResult.Continue;
			} else {
				PlayerSystem.Selecting?.Bounce();
				return TaskResult.End;
			}
		} else if (LocalFrame < Duration) {
			return TaskResult.Continue;
		} else {
			PlayerSystem.Selecting?.Bounce();
			return TaskResult.End;
		}
	}


	// API
	public static TeleportTask TeleportFromDoor (int fromX, int fromY, int toX, int toY, int toZ) => TeleportLogic(false, fromX, fromY, toX, toY, toZ, false, true);
	public static TeleportTask TeleportFromPortal (int fromX, int fromY, int toX, int toY, int toZ) => TeleportLogic(true, fromX, fromY, toX, toY, toZ, true, false);
	private static TeleportTask TeleportLogic (bool portal, int fromX, int fromY, int toX, int toY, int toZ, bool useVignette = false, bool useParallax = true) {
		if (TaskSystem.HasTask()) return null;
		if (TaskSystem.TryAddToLast(TYPE_ID, out var task) && task is TeleportTask svTask) {
			svTask.TeleportFrom = new Int2(fromX, fromY);
			svTask.TeleportTo = new Int3(toX, toY, toZ);
			svTask.WaitDuration = 0;
			svTask.UseParallax = useParallax;
			svTask.UseVignette = useVignette;
			int duration = portal ? 60 : 30;
			var player = PlayerSystem.Selecting;
			if (player != null) {
				duration = portal ? player.TeleportDuration * 2 : player.TeleportDuration;
				player.Movement.Stop();
				player.EnterTeleportState(Stage.ViewZ > toZ, portal);
				player.VelocityX = 0;
				player.VelocityY = 0;
			}
			svTask.Duration = duration;
			return svTask;
		}
		return null;
	}


	// LGC
	private static void ParaLogic (Cell[] cells, Int2 center, int count, float ease) {
		for (int i = 0; i < count; i++) {
			var cell = cells[i];
			if (cell.Rotation1000 == 0) {
				cell.X = Util.LerpUnclamped(center.x, cell.X - cell.PivotX * cell.Width, ease).FloorToInt();
				cell.Y = Util.LerpUnclamped(center.y, cell.Y - cell.PivotY * cell.Height, ease).FloorToInt();
				cell.Width = cell.Width > 0 ? (cell.Width * ease).CeilToInt() : (cell.Width * ease).FloorToInt();
				cell.Height = cell.Height > 0 ? (cell.Height * ease).CeilToInt() : (cell.Height * ease).FloorToInt();
				cell.PivotX = 0;
				cell.PivotY = 0;
			} else {
				cell.X = Util.LerpUnclamped(center.x, cell.X, ease).FloorToInt();
				cell.Y = Util.LerpUnclamped(center.y, cell.Y, ease).FloorToInt();
				cell.Width = (cell.Width * ease).CeilToInt();
				cell.Height = (cell.Height * ease).CeilToInt();
			}
		}
	}


}