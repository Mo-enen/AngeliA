using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(4)]
public class SlideDustParticle : Particle {

	private static readonly int TYPE_ID = typeof(SlideDustParticle).AngeHash();
	public override int Duration => 20;
	public override bool Loop => false;

	[OnCharacterSlideStepped_Entity]
	internal static void OnSlideStepped (Entity target) {
		if (target is not IWithCharacterMovement wMov) return;
		var rect = target.Rect;
		Stage.SpawnEntity(
			TYPE_ID,
			wMov.CurrentMovement.FacingRight ? rect.xMax : rect.xMin,
			rect.yMin + rect.height * 3 / 4
		);
	}

}
