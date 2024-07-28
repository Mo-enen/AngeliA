using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


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
	public virtual int AutoArtworkID => TypeID;

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


	public sealed override void BeforeUpdate () {
		base.BeforeUpdate();
		if (!Loop && Game.GlobalFrame >= SpawnFrame + Duration) {
			Active = false;
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active || !IsAutoParticle) return;
		// Artwork ID
		if (!Renderer.TryGetSpriteGroup(AutoArtworkID, out var group) || group.Count == 0) return;
		float framePerSprite = (float)Duration / group.Count;
		int spriteIndex = (LocalFrame / framePerSprite).RoundToInt().Clamp(0, group.Count - 1);
		var sprite = group.Sprites[spriteIndex];
		Renderer.Draw(
			sprite, X, Y, sprite.PivotX, sprite.PivotY, Rotation,
			sprite.GlobalWidth * Scale / 1000, sprite.GlobalHeight * Scale / 1000, Tint, RenderingZ
		);

	}


}