using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(36)]
public class FootstepParticle : Particle {

	public static readonly int TYPE_ID = typeof(FootstepParticle).AngeHash();
	public override int Duration => 20;
	public override bool Loop => false;
	public override int RenderingZ => -1024;

	[OnCharacterFootStepped_Entity]
	internal static void OnStepped (Entity target) {
		var rect = target.Rect;
		if (
			Stage.TrySpawnEntity(TYPE_ID, rect.CenterX(), rect.y, out var entity) &&
			entity is Particle particle
		) {
			if (Renderer.TryGetSpriteFromGroup(target is Rigidbody rig ? rig.GroundedID : 0, 0, out var sprite)) {
				particle.Tint = sprite.SummaryTint;
			} else {
				particle.Tint = Color32.WHITE;
			}
		}
	}

}
