using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class CheckPointTouchParticle : Particle {
	private static readonly int TYPE_ID = typeof(CheckPointTouchParticle).AngeHash();
	public override int Duration => 32;
	public override bool Loop => false;
	[OnGameInitializeLater(64)]
	public static void Init () {
		CheckPoint.OnCheckPointTouched += SpawnParticleForCheckPoint;
		static void SpawnParticleForCheckPoint (CheckPoint checkPoint, Character target) {
			if (Stage.SpawnEntity(TYPE_ID, checkPoint.X + Const.HALF, checkPoint.Y + Const.HALF) is Particle particle) {
				particle.Width = Const.CEL * 2;
				particle.Height = Const.CEL * 2;
				particle.UserData = checkPoint;
			}
		}
	}
	public override void DrawParticle () {
		if (UserData is not CheckPoint targetCP) return;
		// Flash
		if (Renderer.TryGetSprite(targetCP.TypeID, out var cpSprite)) {
			Renderer.SetLayerToAdditive();
			Renderer.Draw(cpSprite, targetCP.Rect.Expand(LocalFrame), new Color32(0, 255, 0,
				(byte)Util.RemapUnclamped(0, Duration, 128, 0, LocalFrame).Clamp(0, 255)
			));
			Renderer.SetLayerToDefault();
		}
	}
}