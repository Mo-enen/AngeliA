using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class MovableParticle : Particle {


	public static readonly int TYPE_ID = typeof(MovableParticle).AngeHash();

	public override int Duration => _Duration;
	public override bool Loop => false;
	public int ArtworkID { get; set; } = 0;
	public int _Duration { get; set; } = 60;

	public int CurrentSpeedX { get; set; } = 0;
	public int CurrentSpeedY { get; set; } = 0;
	public int RotateSpeed { get; set; } = 0;

	public int AirDragX { get; set; } = 3;
	public int Gravity { get; set; } = 5;
	public int MaxGravitySpeed { get; set; } = int.MaxValue / 2;

	public bool FlipX { get; set; } = false;
	public bool BlinkInEnd { get; set; } = true;
	public bool FadeInEnd { get; set; } = false;


	[OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravity]
	internal static void OnObjectFreeFall (int spriteID, Int2 pos, int rotation, bool flipX, Int2 velocity, int rotateSpeed, int gravity) {
		if (Stage.SpawnEntity(TYPE_ID, pos.x, pos.y) is not MovableParticle particle) return;
		(particle.Width, particle.Height) = Renderer.TryGetSpriteForGizmos(spriteID, out var sprite) ? (sprite.GlobalWidth, sprite.GlobalHeight) : (Const.HALF, Const.HALF);
		particle.BlinkInEnd = true;
		particle.ArtworkID = spriteID;
		particle.Rotation = rotation;
		particle.CurrentSpeedX = velocity.x;
		particle.CurrentSpeedY = velocity.y;
		particle.RotateSpeed = rotateSpeed;
		particle.FlipX = flipX;
		particle.Gravity = gravity;
		particle.MaxGravitySpeed = 96;
	}

	public override void OnActivated () {
		base.OnActivated();
		ArtworkID = 0;
		_Duration = 60;
		CurrentSpeedX = 0;
		CurrentSpeedY = 0;
		RotateSpeed = 0;
		AirDragX = 3;
		Gravity = 5;
		MaxGravitySpeed = int.MaxValue / 2;
		FlipX = false;
		BlinkInEnd = true;
		FadeInEnd = false;
		Tint = Color32.WHITE;
	}

	public override void Update () {
		base.Update();
		CurrentSpeedX = CurrentSpeedX.MoveTowards(0, AirDragX);
		CurrentSpeedY = Util.Max(CurrentSpeedY - Gravity, -MaxGravitySpeed);
		X += CurrentSpeedX;
		Y += CurrentSpeedY;
		Rotation += RotateSpeed;
		// Despawn when Out Of Range
		if (!Renderer.CameraRect.Overlaps(Rect)) {
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		if (BlinkInEnd && LocalFrame > Duration / 2 && LocalFrame % 6 < 3) return;
		if (ArtworkID != 0) {
			using var _ = new UILayerScope();
			var tint = FadeInEnd && LocalFrame > Duration / 2 ? Tint.WithNewA(
				Util.Remap(Duration / 2, Duration, 255, 0, LocalFrame)
			) : Tint;
			Renderer.Draw(
				ArtworkID, X, Y, 500, 500, Rotation,
				FlipX ? -Width : Width, Height,
				tint, z: 0
			);
		}
	}

}
