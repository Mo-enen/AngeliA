using System.Collections;
using System.Collections.Generic;
using AngeliA; using AngeliA.Platformer;

namespace MarioTemplate;

public sealed class DefaultParticle : Particle {
	public static readonly int TYPE_ID = typeof(DefaultParticle).AngeHash();
	public override int Duration => 24;
	public override bool Loop => false;
}