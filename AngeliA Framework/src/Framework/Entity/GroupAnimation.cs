using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public class GroupAnimation : Entity {

	// Const
	private static readonly int TYPE_ID = typeof(GroupAnimation).AngeHash();

	// Data
	private int ArtworkID = 0;
	private int Duration = 1;
	private int RenderingZ = int.MaxValue - 1;
	private int FramePerSprite = 3;
	private int PivotX = 0;
	private int PivotY = 0;
	private int Rotation1000 = 0;
	private int RotationSpeed = 0;
	private int RenderingLayer = RenderLayer.DEFAULT;
	private bool Loop = true;
	private Color32 Tint = Color32.WHITE;
	private int RenderedFrame = 0;

	// MSG
	public sealed override void BeforeUpdate () {
		base.BeforeUpdate();
		if (RenderedFrame >= Duration) {
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();

		if (!Active) return;
		if (!Renderer.TryGetSpriteGroup(ArtworkID, out var group) || group.Count <= 0) {
			Active = false;
			return;
		}

		// Render
		using var _ = new LayerScope(RenderingLayer);
		Rotation1000 += RotationSpeed * 1000;

		// Get Sprite ID
		Cell[] cells = null;
		Cell cell = null;
		AngeSprite sprite = null;
		if (FramePerSprite < 0) {
			// Auto Ani
			Renderer.CurrentSheet.TryGetSpriteFromAnimationFrame(group, RenderedFrame, out sprite);
		} else {
			// Even-Frame Ani
			int spIndex = RenderedFrame.UDivide(FramePerSprite);
			spIndex = Loop ? spIndex.UMod(group.Count) : spIndex.Clamp(0, group.Count - 1);
			sprite = group.Sprites[spIndex];
		}
		RenderedFrame++;

		// Draw Sprite
		if (sprite != null) {
			if (sprite.GlobalBorder == Int4.zero) {
				cell = Renderer.Draw(
					sprite, X, Y, PivotX, PivotY, 0, Width, Height, Tint, RenderingZ
				);
			} else {
				cells = Renderer.DrawSlice(
					sprite, X, Y, PivotX, PivotY, 0, Width, Height, Tint, RenderingZ
				);
			}
		}

		// Fix Rotation
		if (cells != null) {
			for (int i = 0; i < cells.Length; i++) {
				cells[i].Rotation1000 = Rotation1000;
			}
		}
		if (cell != null) {
			cell.Rotation1000 = Rotation1000;
		}

	}


	// API
	public static GroupAnimation Spawn (int groupID, int x, int y, int renderLayer = RenderLayer.DEFAULT, int rotation1000 = 0, int rotationSpeed = 0, int scale = 1000) {
		if (!Renderer.TryGetSpriteFromGroup(groupID, 0, out var sprite, true, true, true)) return null;
		return Spawn(
			groupID, x, y,
			sprite.GlobalWidth * scale / 1000, sprite.GlobalHeight * scale / 1000,
			sprite.PivotX, sprite.PivotY,
			rotation1000, rotationSpeed, -1, 1, false, Color32.WHITE, int.MaxValue - 1, renderLayer
		);
	}
	public static GroupAnimation Spawn (int groupID, int x, int y, int rotation1000, int rotationSpeed, int duration, int framePerSprite, bool loop, Color32 tint, int z = int.MaxValue - 1, int renderLayer = RenderLayer.DEFAULT) {
		if (!Renderer.TryGetSpriteFromGroup(groupID, 0, out var sprite, true, true, true)) return null;
		return Spawn(groupID, x, y, sprite.GlobalWidth, sprite.GlobalHeight, sprite.PivotX, sprite.PivotY, rotation1000, rotationSpeed, duration, framePerSprite, loop, tint, z, renderLayer);
	}
	public static GroupAnimation Spawn (int groupID, int x, int y, int width, int height, int pivotX, int pivotY, int rotation1000, int rotationSpeed, int duration, int framePerSprite, bool loop, Color32 tint, int z = int.MaxValue - 1, int renderLayer = RenderLayer.DEFAULT) {
		if (!Renderer.TryGetSpriteGroup(groupID, out var group) || group.Count == 0) return null;
		// Auto Duration
		if (duration <= 0) {
			loop = false;
			duration = group.Animated ? Renderer.GetAnimationGroupDuration(group) : 0;
			if (duration == 0) {
				duration = framePerSprite * group.Count;
			} else {
				framePerSprite = -1;
			}
		}
		// Spawn
		if (duration <= 0 || Stage.SpawnEntity(TYPE_ID, x, y) is not GroupAnimation ani) return null;
		ani.ArtworkID = groupID;
		ani.Duration = duration;
		ani.RenderingZ = z;
		ani.FramePerSprite = framePerSprite;
		ani.Loop = loop;
		ani.Width = width;
		ani.Height = height;
		ani.PivotX = pivotX;
		ani.PivotY = pivotY;
		ani.Rotation1000 = rotation1000;
		ani.RotationSpeed = rotationSpeed;
		ani.Tint = tint;
		ani.RenderingLayer = renderLayer;
		ani.RenderedFrame = 0;
		return ani;
	}

}
