using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public class DefaultParticle : Particle {
	public override int Duration => 30;
	public override bool Loop => false;
}


[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.Capacity(512)]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.DECORATE)]
public abstract class Particle : Entity {


	// Abs
	public abstract int Duration { get; }
	public abstract bool Loop { get; }
	public virtual int Scale => 1000;
	public virtual int RenderingZ => int.MinValue;

	// Api
	public Color32 Tint { get; set; } = Color32.WHITE;
	public int LocalFrame => (Game.GlobalFrame - SpawnFrame) % Duration;
	public int Rotation { get; set; } = 0;
	public object UserData { get; set; } = null;

	// Data
	private bool IsAutoParticle = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsAutoParticle = Renderer.HasSpriteGroup(TypeID);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Loop && Game.GlobalFrame >= SpawnFrame + Duration) {
			Active = false;
			return;
		}
		if (IsAutoParticle) {
			// Artwork ID
			if (Renderer.TryGetSpriteGroup(TypeID, out var group) && group.Count > 0) {
				float framePerSprite = (float)Duration / group.Count;
				if (Renderer.TryGetSprite(group[(LocalFrame / framePerSprite).RoundToInt().Clamp(0, group.Count - 1)], out var sprite, true)) {
					Renderer.Draw(
						sprite, X, Y, sprite.PivotX, sprite.PivotY, Rotation,
						sprite.GlobalWidth * Scale / 1000, sprite.GlobalHeight * Scale / 1000, Tint, RenderingZ
					);
				}
			}
		} else {
			// Procedure
			DrawParticle();
		}
	}


	public virtual void DrawParticle () { }


}