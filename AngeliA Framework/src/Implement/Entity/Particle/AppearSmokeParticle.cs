using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 
public class AppearSmokeParticle : Particle {

	public static readonly int TYPE_ID = typeof(AppearSmokeParticle).AngeHash();
	public override int Duration => 24;
	public override bool Loop => false;
	public override int RenderingZ => _RenderingZ;
	public override int Scale => _Scale;
	public override int FramePerSprite => 4;
	public int _RenderingZ { get; set; } = int.MaxValue - 1;
	public int _Scale { get; set; } = 1000;

	[OnGameInitializeLater(64)]
	public static void OnGameInitialize () {
		Character.OnTeleport += OnTeleport;
		static void OnTeleport (Character character) {
			Stage.TrySpawnEntity(TYPE_ID, character.X, character.Y + character.Height / 2, out var entity);
		}
	}

	public override void OnActivated () {
		base.OnActivated();
		_RenderingZ = int.MaxValue - 1;
		_Scale = 1000;
	}

}
