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
		private bool Front = true;
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
				Front = TeleportTo.z > Stage.ViewZ;
				// Vig
				if (useVig) {
					VigFeather = VignetteEffect.GetFeather();
					VigRound = VignetteEffect.GetRound();
					ScreenEffect.SetEffectEnable(VignetteEffect.TYPE_ID, true);
					VignetteEffect.SetFeather(0f);
					VignetteEffect.SetRound(1f);
				}
			}

			// Portal Effect
			if (UsePortalEffect && LocalFrame < WaitDuration && CurrentTeleportEntity is Character character) {
				float lerp01 = (float)LocalFrame / WaitDuration;
				int centerX = TeleportFrom.x;
				int centerY = TeleportFrom.y;
				int twistOffsetX = (int)((1f - lerp01) * Const.CEL * Mathf.Sin(lerp01 * 360f * Mathf.Deg2Rad));
				int twistOffsetY = (int)((1f - lerp01) * Const.CEL * Mathf.Cos(lerp01 * 360f * Mathf.Deg2Rad));
				character.X = centerX + twistOffsetX;
				character.Y = centerY + twistOffsetY - character.Height / 2;
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
			if (LocalFrame == WaitDuration + 1 && useParallax) {
				const int PARA = Const.SQUAD_BEHIND_PARALLAX;
				var effect = TeleportEffect.Instance;
				effect.Duration = Duration - WaitDuration;
				effect.Scale = Front ? 1000f / PARA : PARA / 1000f;
				effect.ToBehind = Front;
				CellRenderer.RemoveEffect<TeleportEffect>();
				CellRenderer.AddEffect(effect);
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

	}



	public class TeleportEffect : CellEffect {

		public static readonly TeleportEffect Instance = new();

		public float Scale { get; set; } = 1f;
		public bool ToBehind { get; set; } = true;

		public override void Perform (Cell[] cells, int cellCount, int layerIndex) {

			if (layerIndex == Const.SHADER_UI || layerIndex == Const.SHADER_TOP_UI) return;

			float z01 = Mathf.InverseLerp(0, Duration, LocalFrame);
			float z10 = 1f - z01;
			Vector2Int center = CellRenderer.CameraRect.center.CeilToInt();
			var scl = Mathf.LerpUnclamped(Scale, 1f, 1f - z10 * z10);

			if (layerIndex == Const.SHADER_BEHIND) {
				// Behind
				PerformLogic(cells, center, cellCount, scl, true);
			} else if (layerIndex != Const.SHADER_WALLPAPER) {
				// Front
				PerformLogic(cells, center, cellCount, scl, false);
			}

		}

		private void PerformLogic (Cell[] cells, Vector2Int center, int count, float scale, bool behind) {
			Color32 c;
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				c = cell.Color;
				if (behind) {
					if (ToBehind) {
						c.a = (byte)Util.Remap(Scale, 1f, 0, c.a, scale);
					} else {
						c.a = (byte)Util.Remap(Scale, 1f, 255, c.a, scale);
					}
				}
				cell.Color = c;
				if (cell.Rotation == 0) {
					cell.X = Mathf.LerpUnclamped(cell.X - cell.PivotX * cell.Width, center.x, 1f - scale).FloorToInt();
					cell.Y = Mathf.LerpUnclamped(cell.Y - cell.PivotY * cell.Height, center.y, 1f - scale).FloorToInt();
					cell.Width = cell.Width > 0 ? (cell.Width * scale).CeilToInt() : (cell.Width * scale).FloorToInt();
					cell.Height = cell.Height > 0 ? (cell.Height * scale).CeilToInt() : (cell.Height * scale).FloorToInt();
					cell.PivotX = 0;
					cell.PivotY = 0;
				} else {
					cell.X = Mathf.LerpUnclamped(cell.X, center.x, 1f - scale).FloorToInt();
					cell.Y = Mathf.LerpUnclamped(cell.Y, center.y, 1f - scale).FloorToInt();
					cell.Width = (cell.Width * scale).CeilToInt();
					cell.Height = (cell.Height * scale).CeilToInt();
				}
			}
		}

	}


}
