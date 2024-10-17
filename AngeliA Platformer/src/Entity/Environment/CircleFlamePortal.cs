using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class CircleFlamePortal : Portal {
	private static readonly SpriteCode CIRCLE_CODE = "PortalCircle";
	private static readonly SpriteCode FLAME_CODE = "PortalFlame";
	private static readonly SpriteCode LIGHT_CODE = "PortalLight";
	protected virtual int CircleCode => CIRCLE_CODE;
	protected virtual int FlameCode => FLAME_CODE;
	protected virtual int LightCode => LIGHT_CODE;
	protected virtual int CircleSize => Const.CEL * 3 / 2;
	protected int RenderingMinZ { get; private set; } = 0;
	protected int RenderingMaxZ { get; private set; } = 0;
	public override void OnActivated () {
		base.OnActivated();
		int size = CircleSize;
		X = X + Const.HALF - size / 2;
		Y = Y + Const.HALF - size / 2;
		Width = size;
		Height = size;
	}
	public override void LateUpdate () {

		int centerX = X + Width / 2;
		int centerY = Y + Height / 2;
		int scale = ((Game.GlobalFrame - SpawnFrame) * 30).Clamp(0, 1000);

		RenderingMinZ = int.MaxValue;
		RenderingMaxZ = int.MinValue;

		// Light
		if (Renderer.TryGetSprite(LightCode, out var light)) {
			int lightSize = CircleSize * scale / 1000;
			if (!light.GlobalBorder.IsZero) {
				lightSize += CircleSize * light.GlobalBorder.horizontal / light.GlobalWidth;
			}
			lightSize += (int)Util.PingPong(Game.GlobalFrame, 36);
			var tint = new Color32(0, 0, 0, (byte)(Util.PingPong(Game.GlobalFrame, 36) * 2 + 120).Clamp(0, 255));
			Renderer.SetLayerToMultiply();
			Renderer.Draw(
				light, centerX, centerY,
				500, 500, (Game.GlobalFrame * 6).UMod(360),
				lightSize, lightSize, tint
			);
			Renderer.Draw(
				light, centerX, centerY,
				500, 500, (Game.GlobalFrame * -4).UMod(360),
				lightSize * 19 / 20, lightSize * 19 / 20, tint
			);
			Renderer.SetLayerToDefault();
		}

		// Circle
		if (Renderer.TryGetSprite(CircleCode, out var circle)) {
			const int CIRCLE_DURATION = 24;
			const int CIRCLE_COUNT = 4;
			int circleFrame = Game.GlobalFrame % CIRCLE_DURATION;
			int darkIndex = (Game.GlobalFrame % (CIRCLE_DURATION * CIRCLE_COUNT)) / CIRCLE_DURATION;
			for (int i = 0; i < CIRCLE_COUNT; i++) {
				int size = Util.RemapUnclamped(
					0, CIRCLE_DURATION,
					CircleSize - CircleSize * i / CIRCLE_COUNT,
					CircleSize - CircleSize * (i + 1) / CIRCLE_COUNT,
					circleFrame
				);
				size = size * scale / 1000;
				int rgbA = Util.RemapUnclamped(0, 3, 255, 128, CIRCLE_COUNT - i);
				int rgbB = Util.RemapUnclamped(0, 3, 255, 128, CIRCLE_COUNT - i - 1);
				byte rgb = i == darkIndex || i == (darkIndex + 1) % CIRCLE_COUNT ?
					(byte)42 :
					(byte)Util.Lerp(rgbA, rgbB, (float)circleFrame / CIRCLE_DURATION);
				var tint = new Color32(
					rgb, rgb, rgb,
					(byte)(i > 0 ? 255 : Util.RemapUnclamped(0, CIRCLE_DURATION, 0, 400, circleFrame).Clamp(0, 255))
				);
				int z = circle.SortingZ + i;
				Renderer.Draw(
					circle, centerX, centerY,
					500, 500, 0,
					size, size,
					tint, z
				);
				RenderingMinZ = Util.Min(RenderingMinZ, z);
				RenderingMaxZ = Util.Max(RenderingMaxZ, z);
			}
		}

		// Flame
		if (Renderer.TryGetSprite(FlameCode, out var flame)) {
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
				var tint = Color32.WHITE;
				tint.a = (byte)Util.RemapUnclamped(
					0, FLAME_DURATION / 2,
					255, 128,
					flameFrame.PingPong(FLAME_DURATION / 2)
				).Clamp(0, 255);
				int z = flame.SortingZ + FLAME_COUNT + 1;
				Renderer.Draw(
					flame, centerX, centerY,
					flame.PivotX, flame.PivotY, rot,
					size, size,
					tint, z
				);
				RenderingMinZ = Util.Min(RenderingMinZ, z);
				RenderingMaxZ = Util.Max(RenderingMaxZ, z);
			}
		}
	}
}