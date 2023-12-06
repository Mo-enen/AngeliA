using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {
	public class AppearSmokeParticle : Particle {

		private static readonly int TYPE_ID = typeof(AppearSmokeParticle).AngeHash();
		public override int Duration => 24;
		public override bool Loop => false;
		public override int RenderingZ => int.MaxValue - 1;
		public override int FramePerSprite => 4;

		[OnGameInitialize(64)]
		public static void OnGameInitialize () {
			Character.OnTeleport += OnTeleport;
			static void OnTeleport (Character character) {
				Stage.TrySpawnEntity(TYPE_ID, character.X, character.Y + character.Height / 2, out var entity);
			}
		}

	}
}
