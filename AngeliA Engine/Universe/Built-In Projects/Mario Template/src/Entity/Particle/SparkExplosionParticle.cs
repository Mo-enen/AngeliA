using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

public class SparkExplosionParticle : Particle {

	public static readonly int TYPE_ID = typeof(SparkExplosionParticle).AngeHash();

	public override int Duration => 5;
	public override bool Loop => false;
	public override int RenderingLayer => RenderLayer.ADD;

	public override void OnActivated () {
		base.OnActivated();
		Scale = 1500;
	}

}

