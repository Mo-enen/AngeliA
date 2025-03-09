using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

public class SparkParticle : Particle {


	public static readonly int TYPE_ID = typeof(SparkParticle).AngeHash();

	public override int Duration => 5;
	public override bool Loop => false;
	public override int RenderingLayer => RenderLayer.ADD;
	public Int2 Velocity { get; set; }
	public Int2 Deceleration { get; set; }

	public override void OnActivated () {
		base.OnActivated();
		Velocity = default;
		Deceleration = default;
	}

	public override void Update () {
		base.Update();
		X += Velocity.x;
		Y += Velocity.y;
		Velocity = new Int2(
			Velocity.x.MoveTowards(0, Deceleration.x),
			Velocity.y.MoveTowards(0, Deceleration.y)
		);
	}

}
