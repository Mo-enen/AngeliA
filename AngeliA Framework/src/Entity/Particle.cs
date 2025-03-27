using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Entity that represent a animated decoration
/// </summary>
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.Capacity(512, 0)]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.DECORATE)]
public abstract class Particle : Entity {

	
	// Abs
	/// <summary>
	/// How long of this particle exists in stage in frames
	/// </summary>
	public abstract int Duration { get; }
	/// <summary>
	/// True if this particle animation loops
	/// </summary>
	public abstract bool Loop { get; }
	/// <summary>
	/// Size scale this particle should apply (0 means 0%, 1000 means 100%)
	/// </summary>
	public int Scale { get; set; } = 1000;
	/// <summary>
	/// Z value the sort rendering cells
	/// </summary>
	public virtual int RenderingZ => int.MinValue;
	/// <summary>
	/// Artwork sprite ID if this particle is using the built-in logic for rendering
	/// </summary>
	public virtual int AutoArtworkID => TypeID;
	/// <summary>
	/// Which layer should this particle rendering into
	/// </summary>
	public virtual int RenderingLayer => RenderLayer.DEFAULT;

	// Api
	/// <summary>
	/// Color tint for this particle
	/// </summary>
	public Color32 Tint { get; set; } = Color32.WHITE;
	/// <summary>
	/// Animation frame start from 0
	/// </summary>
	public int LocalFrame => (Game.GlobalFrame - SpawnFrame) % Duration;
	/// <summary>
	/// Angle of this particle
	/// </summary>
	public int Rotation { get; set; } = 0;
	/// <summary>
	/// Custom data this particle holds
	/// </summary>
	public object UserData { get; set; } = null;

	// Data
	private bool IsAutoParticle = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsAutoParticle = Renderer.HasSpriteGroup(TypeID);
		Scale = 1000;
		Tint = Color32.WHITE;
		Rotation = 0;
		UserData = null;
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
		using var _ = new LayerScope(RenderingLayer);
		float framePerSprite = (float)Duration / group.Count;
		int spriteIndex = (LocalFrame / framePerSprite).RoundToInt().Clamp(0, group.Count - 1);
		var sprite = group.Sprites[spriteIndex];
		Renderer.Draw(
			sprite, X, Y, sprite.PivotX, sprite.PivotY, Rotation,
			sprite.GlobalWidth * Scale / 1000,
			sprite.GlobalHeight * Scale / 1000,
			Tint, RenderingZ
		);
	}


}