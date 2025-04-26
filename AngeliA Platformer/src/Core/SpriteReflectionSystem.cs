using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public static class SpriteReflectionSystem {

	// VAR
	private static readonly Dictionary<int, byte> SpritePool = [];

	// MSG
	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (SpritePool.Count == 0) return;
		if (TaskSystem.GetCurrentTask() is TeleportTask) return;
		if (!Renderer.GetCells(RenderLayer.DEFAULT, out var cells, out int count)) return;
		using var _ = new LayerScope(RenderLayer.DEFAULT);
		int size = Const.CEL * 1000 / Universe.BuiltInInfo.WorldBehindParallax;
		var center = Renderer.CameraRect.CenterInt();
		float scale = Universe.BuiltInInfo.WorldBehindParallax / 1000f;
		for (int i = 0; i < count; i++) {
			var cell = cells[i];
			if (cell.Sprite == null || !SpritePool.TryGetValue(cell.Sprite.ID, out byte alpha)) continue;
			var rect = new IRect(
				cell.X - (int)(cell.Width * cell.PivotX),
				cell.Y - (int)(cell.Height * cell.PivotY),
				cell.Width, cell.Height
			).GetFlipNegative();
			// Render Reflection
			var target = rect.ScaleFrom(scale, center.x, center.y);
			DrawReflection(
				rect, target, size,
				(cell.Sprite.SummaryTint * cell.Color).WithNewA(alpha),
				cell.Z + 1
			);
		}
	}

	// API
	public static void RegisterSprite (int spriteID, byte alpha) {
		// Sprite
		if (Renderer.TryGetSprite(spriteID, out var sprite)) {
			SpritePool[spriteID] = alpha;
		}
		// Group
		if (Renderer.TryGetSpriteGroup(spriteID, out var group)) {
			foreach (var sp in group.Sprites) {
				SpritePool[sp.ID] = alpha;
			}
		}
#if DEBUG
		// Warning
		if (sprite == null && group == null) {
			Debug.LogWarning($"Fail to register reflection for {spriteID}");
		}
#endif
	}

	// LGC
	private static void DrawReflection (IRect source, IRect target, int size, Color32 tint, int renderingZ) {
		int globalL = target.xMin;
		int globalR = target.xMax;
		int globalD = target.yMin;
		int globalU = target.yMax;
		int sGlobalL = source.xMin;
		int sGlobalR = source.xMax;
		int sGlobalD = source.yMin;
		int sGlobalU = source.yMax;
		int l = globalL.UDivide(Const.CEL);
		int r = globalR.UDivide(Const.CEL);
		int d = globalD.UDivide(Const.CEL);
		int u = globalU.UDivide(Const.CEL);
		int z = Stage.ViewZ - 1;
		var stream = WorldSquad.Stream;
		var rect = new IRect(0, 0, size + 1, size + 1);
		var expSource = source.Expand(1);
		for (int j = d; j <= u; j++) {
			for (int i = l; i <= r; i++) {
				var blocks = stream.GetAllBlocksAt(i, j, z);
				blocks.element = 0;
				if (blocks == default) continue;
				rect.x = Util.RemapUnclamped(
					globalL, globalR, sGlobalL, sGlobalR, i * Const.CEL
				);
				rect.y = Util.RemapUnclamped(
					globalD, globalU, sGlobalD, sGlobalU, j * Const.CEL
				);
				if (!rect.Overlaps(expSource)) continue;
				if (blocks.bg != 0) {
					if (Renderer.TryGetSpriteForGizmos(blocks.bg, out var sp)) {
						var cell = Renderer.Draw(sp, rect, tint, renderingZ);
						cell.Clamp(expSource);
					}
				} else if (blocks.level != 0) {
					if (Renderer.TryGetSpriteForGizmos(blocks.level, out var sp)) {
						var cell = Renderer.Draw(sp, rect, tint, renderingZ);
						cell.Clamp(expSource);
					}
				} else if (blocks.entity != 0) {
					if (Renderer.TryGetSpriteForGizmos(blocks.entity, out var sp)) {
						var _rect = rect;
						if (sp.GlobalWidth != Const.CEL || sp.GlobalHeight != Const.CEL) {
							_rect = _rect.Fit(sp);
						}
						var cell = Renderer.Draw(sp, _rect, tint, renderingZ);
						cell.Clamp(expSource);
					}
				}
			}
		}
	}

}
