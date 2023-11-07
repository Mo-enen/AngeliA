using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class FreeFallParticle : Particle {


		// Api
		public override int Duration => 1;
		public override bool Loop => true;
		protected int CurrentSpeedX { get; set; } = 0;
		protected int CurrentSpeedY { get; set; } = 0;
		protected int AirDragX { get; set; } = 3;
		protected int RotateSpeed { get; set; } = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			CurrentSpeedX = 0;
			CurrentSpeedY = 0;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			CurrentSpeedX = CurrentSpeedX.MoveTowards(0, AirDragX);
			CurrentSpeedY = Mathf.Max(CurrentSpeedY - 5, -96);
			X += CurrentSpeedX;
			Y += CurrentSpeedY;
			Rotation += RotateSpeed;
			// Despawn when Out Of Range
			if (!CellRenderer.CameraRect.Overlaps(Rect)) {
				Active = false;
			}
		}


	}



	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(512)]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Layer(Const.ENTITY_LAYER_DECORATE)]
	public abstract class Particle : Entity {


		// Abs
		public abstract int Duration { get; }
		public abstract bool Loop { get; }
		public virtual int FramePerSprite => 5;
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