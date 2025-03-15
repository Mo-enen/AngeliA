using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class BreakingParticle : Particle {


	// Const
	private static readonly int TYPE_ID = typeof(BreakingParticle).AngeHash();

	// Api
	public override int Duration => _Duration;
	public override bool Loop => false;

	// Data
	private Int4 Shift = default;
	private int _Duration = 180;
	private int SpriteID = 0;
	private int SpeedX = 0;
	private int SpeedY = 0;
	private int RotateSpeed = 10;
	private int Gravity = 4;
	private int AirDragX = 3;
	private int MaxSpeedY = 96;


	// MSG
	public override void Update () {
		base.Update();
		X += SpeedX;
		Y += SpeedY;
		SpeedX = SpeedX.LerpTo(0, AirDragX);
		SpeedY = (SpeedY - Gravity).GreaterOrEquel(-MaxSpeedY);
	}


	public override void LateUpdate () {
		base.LateUpdate();

		if (!Rect.Expand(Const.CEL).Overlaps(Renderer.CameraRect)) {
			Active = false;
			return;
		}

		int x = X + Width / 2;
		int y = Y + Height / 2;
		int originalX = X - Shift.left;
		int originalY = Y - Shift.down;
		int px = (x - originalX) * 1000 / (Width * 2);
		int py = (y - originalY) * 1000 / (Height * 2);
		int r = Game.GlobalFrame * RotateSpeed;
		byte alpha = (byte)Util.RemapUnclamped(0, Duration, 512, 0, Game.GlobalFrame - SpawnFrame).Clamp(0, 255);
		var cell = Renderer.Draw(
			SpriteID, x, y, px, py, r, Width * 2, Height * 2,
			new Color32(255, 255, 255, alpha), int.MaxValue
		);
		cell.Shift = Shift;

	}


	[OnObjectBreak_IntSpriteID_IRect]
	public static void SpawnParticles (int spriteID, IRect rect) {

		bool lightWeight = false;
		if (Renderer.TryGetSprite(spriteID, out var sprite, true)) {
			lightWeight = sprite.IsTrigger;
		} else if (Renderer.TryGetSpriteFromGroup(spriteID, 0, out var groupSprite, false)) {
			spriteID = groupSprite.ID;
			lightWeight = groupSprite.IsTrigger;
		} else {
			return;
		}

		int sizeX = rect.width / 2;
		int sizeY = rect.height / 2;
		for (int i = 0; i < 4; i++) {
			if (
				!Stage.TrySpawnEntity(TYPE_ID, rect.x, rect.y, out var entity) ||
				entity is not BreakingParticle particle
			) break;
			bool isLeft = i % 2 == 0;
			bool isBottom = i / 2 == 0;
			particle.Width = sizeX;
			particle.Height = sizeY;
			particle.SpriteID = spriteID;
			particle.SpeedX = (isLeft ? -1 : 1) * Util.QuickRandom(32, 42);
			particle.SpeedY = (isBottom ? Util.QuickRandom(32, 42) : Util.QuickRandom(42, 64));
			if (lightWeight) {
				particle._Duration = 32;
				particle.SpeedX /= 2;
				particle.SpeedY /= 8;
				particle.Gravity = 1;
				particle.RotateSpeed = 1;
				particle.AirDragX = 512;
				particle.MaxSpeedY = 6;
			} else {
				particle._Duration = 180;
				particle.Gravity = 4;
				particle.RotateSpeed = 10;
				particle.AirDragX = 3;
				particle.MaxSpeedY = 96;
			}

			if (!isLeft) particle.X += sizeX;
			if (!isBottom) particle.Y += sizeY;

			// Shift
			particle.Shift.left = isLeft ? 0 : sizeX;
			particle.Shift.right = !isLeft ? 0 : sizeX;
			particle.Shift.down = isBottom ? 0 : sizeY;
			particle.Shift.up = !isBottom ? 0 : sizeY;
		}

	}


}