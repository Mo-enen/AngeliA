using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {
	public class DefaultParticle : Particle {
		public override int Duration => 30;
		public override bool Loop => false;
	}


	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(512)]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Layer(EntityLayer.DECORATE)]
	[RequireSprite("{0}")]
	public abstract class Particle : Entity {


		// Abs
		public abstract int Duration { get; }
		public abstract bool Loop { get; }
		public virtual int FramePerSprite => 5;
		public virtual int Scale => 1000;
		public virtual int RenderingZ => int.MinValue;

		// Api
		public Byte4 Tint { get; set; } = Const.WHITE;
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
						sprite, X, Y, sprite.PivotX, sprite.PivotY, Rotation,
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