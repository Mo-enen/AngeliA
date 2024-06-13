using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class FreeFallParticle : Particle {

	public static readonly int TYPE_ID = typeof(FreeFallParticle).AngeHash();
	public override int Duration => 60;
	public override bool Loop => false;
	public int ArtworkID { get; set; } = 0;
	public int CurrentSpeedX { get; set; } = 0;
	public int CurrentSpeedY { get; set; } = 0;
	public int AirDragX { get; set; } = 3;
	public int RotateSpeed { get; set; } = 0;
	public int Gravity { get; set; } = 5;
	public bool FlipX { get; set; } = false;
	public bool BlinkInEnd { get; set; } = true;
	public Color32 Color { get; set; } = Color32.WHITE;

	public override void OnActivated () {
		base.OnActivated();
		CurrentSpeedX = 0;
		CurrentSpeedY = 0;
		Gravity = 5;
		AirDragX = 3;
		RotateSpeed = 0;
		FlipX = false;
		BlinkInEnd = true;
		ArtworkID = 0;
		Color = Color32.WHITE;
	}

	public override void Update () {
		base.Update();
		CurrentSpeedX = CurrentSpeedX.MoveTowards(0, AirDragX);
		CurrentSpeedY = Util.Max(CurrentSpeedY - Gravity, -96);
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
		if (BlinkInEnd && LocalFrame > Duration / 2 && LocalFrame % 6 < 3) return;
		if (ArtworkID != 0) {
			using (new UILayerScope()) {
				Renderer.Draw(ArtworkID, X, Y, 500, 500, Rotation, FlipX ? -Width : Width, Height, Color, z: 0);
			}
		}
	}

}