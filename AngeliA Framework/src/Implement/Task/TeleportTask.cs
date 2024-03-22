using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
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

	// MSG
	public override void OnStart () {
		base.OnStart();
		WaitDuration = WaitDuration.Clamp(0, Duration - 1);
		ToBehind = TeleportTo.z > Stage.ViewZ;
	}

	public override TaskResult FrameUpdate () {

		bool useVig = UseVignette;
		bool useParallax = UseParallax;

		// Teleport
		if (LocalFrame == WaitDuration) {
			// Channel
			if (NewChannel.HasValue) {
				if (NewChannel.Value == MapChannel.Procedure) {
					WorldSquad.SwitchToProcedureMode(ChannelName);
				} else {
					WorldSquad.SwitchToCraftedMode();
				}
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
		if (useParallax && LocalFrame > WaitDuration + 1) {
			int PARA = Game.WorldBehindParallax;
			float scale = ToBehind ? 1000f / PARA : PARA / 1000f;
			float z01 = Util.InverseLerp(WaitDuration, Duration, LocalFrame);
			float lerp = Util.LerpUnclamped(scale, 1f, z01);
			var center = Renderer.CameraRect.center.CeilToInt();
			// Behind
			if (Renderer.GetCells(RenderLayer.BEHIND, out var cells, out int count)) {
				MapEffectLogic(cells, center, count, scale, lerp, true, ToBehind);
			}
			// Front
			for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
				if (
					layer == RenderLayer.WALLPAPER ||
					layer == RenderLayer.UI ||
					layer == RenderLayer.BEHIND
				) continue;
				if (Renderer.GetCells(layer, out cells, out count)) {
					MapEffectLogic(cells, center, count, scale, lerp, false, ToBehind);
				}
			}
		}

		// Update Vig Effect
		if (useVig) {
			var cameraRect = Renderer.CameraRect;
			if (LocalFrame < WaitDuration) {
				float radius = Util.RemapUnclamped(0, WaitDuration - 1, 1f, 0f, LocalFrame);
				float offsetX = Util.RemapUnclamped(cameraRect.xMin, cameraRect.xMax, -1f, 1f, TeleportFrom.x);
				float offsetY = Util.RemapUnclamped(cameraRect.yMin, cameraRect.yMax, -1f, 1f, TeleportFrom.y);
				Game.PassEffect_Vignette(radius * radius, 0f, offsetX, offsetY, 1f);
				return TaskResult.Continue;
			} else if (LocalFrame < Duration) {
				float t01 = Util.RemapUnclamped(WaitDuration, Duration - 1, 0f, 1f, LocalFrame);
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
				return TaskResult.End;
			}
		} else {
			return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
		}
	}

	// API
	public static TeleportTask Teleport (
		int fromX, int fromY, int toX, int toY, int toZ,
		int waitDuration = 6, int duration = 24, bool useVignette = false, bool useParallax = true, bool withPortal = false,
		MapChannel? newChannel = null, string channelName = ""
	) {
		if (Task.HasTask()) return null;
		if (Task.TryAddToLast(TYPE_ID, out var task) && task is TeleportTask svTask) {
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
				player.Stop();
				player.EnterTeleportState(svTask.Duration, Stage.ViewZ > toZ, withPortal);
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
				cell.X = Util.LerpUnclamped(center.x, cell.X - cell.PivotX * cell.Width, lerp).FloorToInt();
				cell.Y = Util.LerpUnclamped(center.y, cell.Y - cell.PivotY * cell.Height, lerp).FloorToInt();
				cell.Width = cell.Width > 0 ? (cell.Width * lerp).CeilToInt() : (cell.Width * lerp).FloorToInt();
				cell.Height = cell.Height > 0 ? (cell.Height * lerp).CeilToInt() : (cell.Height * lerp).FloorToInt();
				cell.PivotX = 0;
				cell.PivotY = 0;
			} else {
				cell.X = Util.LerpUnclamped(center.x, cell.X, lerp).FloorToInt();
				cell.Y = Util.LerpUnclamped(center.y, cell.Y, lerp).FloorToInt();
				cell.Width = (cell.Width * lerp).CeilToInt();
				cell.Height = (cell.Height * lerp).CeilToInt();
			}
		}
	}

}