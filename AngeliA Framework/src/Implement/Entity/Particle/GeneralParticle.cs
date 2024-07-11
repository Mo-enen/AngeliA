using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

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
	private int Rotation = 0;
	private int RotationSpeed = 0;
	private bool Loop = true;
	private Color32 Tint = Color32.WHITE;


	// MSG
	public sealed override void BeforeUpdate () {
		base.BeforeUpdate();
		if (Game.GlobalFrame >= SpawnFrame + Duration) {
			Active = false;
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		// Render
		if (Renderer.TryGetSpriteGroup(ArtworkID, out var group) && group.Count > 0) {
			Rotation += RotationSpeed;
			if (FramePerSprite < 0) {
				// Auto Ani
				Renderer.DrawAnimation(
					group, X, Y, PivotX, PivotY, Rotation, Width, Height, Game.GlobalFrame - SpawnFrame
				);
			} else {
				// Even Ani
				int spIndex = (Game.GlobalFrame - SpawnFrame).UDivide(FramePerSprite);
				spIndex = Loop ? spIndex.UMod(group.Count) : spIndex.Clamp(0, group.Count - 1);
				var sp = group[spIndex];
				Renderer.Draw(sp, X, Y, PivotX, PivotY, Rotation, Width, Height, Tint, RenderingZ);
			}
		}
	}


	// API
	public static void Spawn (int groupID, int x, int y, int framePerSprite = 3) {
		if (!Renderer.TryGetSpriteFromGroup(groupID, 0, out var sprite, true, true, true)) return;
		Spawn(
			groupID, x, y, sprite.GlobalWidth, sprite.GlobalHeight, sprite.PivotX, sprite.PivotY,
			0, 0, -1, framePerSprite, false, Color32.WHITE, int.MaxValue - 1
		);
	}
	public static void Spawn (int groupID, int x, int y, int rotation, int rotationSpeed, int duration, int framePerSprite, bool loop, Color32 tint, int z = int.MaxValue - 1) {
		if (!Renderer.TryGetSpriteFromGroup(groupID, 0, out var sprite, true, true, true)) return;
		Spawn(groupID, x, y, sprite.GlobalWidth, sprite.GlobalHeight, sprite.PivotX, sprite.PivotY, rotation, rotationSpeed, duration, framePerSprite, loop, tint, z);
	}
	public static void Spawn (int groupID, int x, int y, int width, int height, int pivotX, int pivotY, int rotation, int rotationSpeed, int duration, int framePerSprite, bool loop, Color32 tint, int z = int.MaxValue - 1) {
		if (!Renderer.TryGetSpriteGroup(groupID, out var group) || group.Count == 0) return;
		// Auto Duration
		if (duration <= 0) {
			loop = false;
			duration = Renderer.GetAnimationGroupDuration(group);
			if (duration == 0) {
				duration = framePerSprite * group.Count;
			} else {
				framePerSprite = -1;
			}
		}
		// Spawn
		if (duration <= 0 || Stage.SpawnEntity(TYPE_ID, x, y) is not GroupAnimation ani) return;
		ani.ArtworkID = groupID;
		ani.Duration = duration;
		ani.RenderingZ = z;
		ani.FramePerSprite = framePerSprite;
		ani.Loop = loop;
		ani.Width = width;
		ani.Height = height;
		ani.PivotX = pivotX;
		ani.PivotY = pivotY;
		ani.Rotation = rotation;
		ani.RotationSpeed = rotationSpeed;
		ani.Tint = tint;
	}


}
