using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(512)]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Layer(Const.ENTITY_LAYER_DECORATE)]
	public abstract class Particle : Entity {


		// Abs
		public abstract int Duration { get; }
		public abstract bool Loop { get; }
		public virtual int FramePerSprite { get; }
		public virtual int Scale => 1000;
		public virtual int RenderingZ => int.MinValue;

		// Api
		public Color32 Tint { get; set; } = Const.WHITE;
		public int LocalFrame => (Game.GlobalFrame - SpawnFrame) % Duration;
		public int Rotation { get; set; } = 0;
		public object UserData { get; set; } = null;

		// Data
		private bool IsAutoParticle = false;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			IsAutoParticle = CellRenderer.HasSpriteGroup(TypeID);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Loop && Game.GlobalFrame >= SpawnFrame + Duration) {
				Active = false;
				return;
			}
			if (IsAutoParticle) {
				// Artwork ID
				if (CellRenderer.TryGetSpriteFromGroup(TypeID, LocalFrame / FramePerSprite, out var sprite, Loop)) {
					CellRenderer.Draw(
						sprite.GlobalID, X, Y, sprite.PivotX, sprite.PivotY, Rotation,
						sprite.GlobalWidth * Scale / 1000, sprite.GlobalHeight * Scale / 1000, Tint, RenderingZ
					);
				}
			} else {
				// Procedure
				DrawParticle();
			}
		}


		public virtual void DrawParticle () { }


	}
}