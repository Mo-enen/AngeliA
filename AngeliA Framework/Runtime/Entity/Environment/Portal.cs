using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Portal : Door {

		protected abstract int CircleCode { get; }
		protected abstract int FlameCode { get; }
		protected override bool TouchToEnter => true;
		protected override bool UsePortalEffect => true;
		protected virtual int CircleSize => Const.CEL * 3 / 2;

		public override void OnActivated () {
			base.OnActivated();
			int size = CircleSize;
			X = X + Const.HALF - size / 2;
			Y = Y + Const.HALF - size / 2;
			Width = size;
			Height = size;
		}

		public override void FrameUpdate () {

			int centerX = X + Width / 2;
			int centerY = Y + Height / 2;
			int scale = ((Game.GlobalFrame - SpawnFrame) * 30).Clamp(0, 1000);

			// Circle
			if (CellRenderer.TryGetSprite(CircleCode, out var circle)) {
				const int CIRCLE_DURATION = 24;
				const int CIRCLE_COUNT = 4;
				int circleFrame = Game.GlobalFrame % CIRCLE_DURATION;
				bool revertZ = IsFrontDoor != FrameTask.IsTasking<TeleportTask>();
				for (int i = 0; i < CIRCLE_COUNT; i++) {
					int size = Util.RemapUnclamped(
						0, CIRCLE_DURATION,
						CircleSize - CircleSize * i / CIRCLE_COUNT,
						CircleSize - CircleSize * (i + 1) / CIRCLE_COUNT,
						circleFrame
					);
					size = size * scale / 1000;
					int rgbA = Util.RemapUnclamped(0, 3, 255, 128, IsFrontDoor ? i : CIRCLE_COUNT - i);
					int rgbB = Util.RemapUnclamped(0, 3, 255, 128, IsFrontDoor ? i + 1 : CIRCLE_COUNT - i - 1);
					byte rgb = (byte)Mathf.Lerp(rgbA, rgbB, (float)circleFrame / CIRCLE_DURATION);
					var tint = new Color32(
						rgb, rgb, rgb,
						(byte)(i > 0 ? 255 : Util.RemapUnclamped(0, CIRCLE_DURATION, 0, 400, circleFrame).Clamp(0, 255))
					);
					CellRenderer.Draw(
						circle.GlobalID, centerX, centerY,
						500, 500, 0,
						size, size,
						tint, (revertZ ? -circle.SortingZ : circle.SortingZ) + i
					);
				}
			}

			// Flame
			if (CellRenderer.TryGetSprite(FlameCode, out var flame)) {
				const int FLAME_COUNT = 3;
				const int FLAME_DURATION = 51;
				int flameFrame = Game.GlobalFrame % FLAME_DURATION;
				for (int i = 0; i < FLAME_COUNT; i++) {
					int rot = Util.RemapUnclamped(
						0, FLAME_DURATION, 0, 360,
						(flameFrame + i * 360 / FLAME_COUNT) % FLAME_DURATION
					);
					int size = Util.RemapUnclamped(
						0, FLAME_DURATION / 2,
						CircleSize / 2, CircleSize * 5 / 8,
						flameFrame.PingPong(FLAME_DURATION / 2)
					);
					size = size * scale / 1000;
					var tint = IsFrontDoor ? Const.WHITE : Const.GREY_128;
					tint.a = (byte)Util.RemapUnclamped(
						0, FLAME_DURATION / 2,
						255, 128,
						flameFrame.PingPong(FLAME_DURATION / 2)
					).Clamp(0, 255);
					CellRenderer.Draw(
						flame.GlobalID, centerX, centerY,
						flame.PivotX, flame.PivotY, rot,
						size, size,
						tint, flame.SortingZ + FLAME_COUNT + 1
					);
				}
			}
		}

	}
}
