using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {
	public class CommonFire : Fire { }


	public interface ICombustible {
		public bool IsBurning => this is Entity e && e.SpawnFrame <= BurnStartFrame;
		public int BurnedDuration => 120;
		public int BurnStartFrame { get; set; }
		public void OnBurned () {
			if (this is Entity e && e.FromWorld) {
				Stage.MarkAsGlobalAntiSpawn(e);
			}
		}
	}


	[EntityAttribute.DontDrawBehind]
	[RequireSprite("{0}")]
	public abstract class Fire : Entity {




		#region --- VAR ---


		// Api
		protected virtual int WeakenDuration => 42;
		protected virtual int SpreadDuration => 60;
		protected virtual int SpreadRange => Const.CEL;
		protected virtual bool UseAdditiveShader => false;
		protected virtual Direction4 DefaultDirection => Direction4.Up;
		public Direction4 Direction { get; set; } = Direction4.Up;

		// Data
		private ICombustible Target = null;
		private int LifeEndFrame = 0;
		private int BurnedFrame = 0;
		private int SpreadFrame = int.MaxValue;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Target = null;
			LifeEndFrame = 0;
			BurnedFrame = 0;
			SpreadFrame = Game.GlobalFrame + SpreadDuration;
			Direction = DefaultDirection;
		}


		public override void OnInactivated () {
			base.OnInactivated();
			Target = null;
			LifeEndFrame = 0;
			BurnedFrame = 0;
			SpreadFrame = 0;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

			if (Target != null && Direction != Direction4.Up) Direction = Direction4.Up;
			var eTarget = Target as Entity;

			// Put Out When Target Not Burning
			if (eTarget != null && !Target.IsBurning) {
				PutOut();
				Active = false;
				return;
			}

			// Put Out when Hit Water
			if (CellPhysics.Overlap(PhysicsMask.MAP, Rect, this, OperationMode.TriggerOnly, SpriteTag.WATER_TAG)) {
				PutOut();
				Active = false;
				return;
			}

			// Wild Fire from Map Editor
			if (eTarget == null && FromWorld) {
				Spread();
				Active = false;
				return;
			}

			// Life Time Check
			if (Game.GlobalFrame >= LifeEndFrame) {
				Active = false;
				return;
			}

			// Spread
			if (Game.GlobalFrame >= SpreadFrame) {
				Spread();
				SpreadFrame += SpreadDuration;
			}

			if (eTarget != null) {
				if (eTarget.Active) {
					// Burned Check
					if (Game.GlobalFrame == BurnedFrame) {
						Target.OnBurned();
						eTarget.Active = false;
					}
					// Follow Target
					X = eTarget.X;
					Y = eTarget.Y;
				} else {
					if (BurnedFrame > Game.GlobalFrame) {
						PutOut();
					}
				}
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Active) return;

			if (UseAdditiveShader) CellRenderer.SetLayerToAdditive();
			var cell = CellRenderer.Draw(
				TypeID,
				X + (Direction == Direction4.Left ? Width : Direction == Direction4.Right ? 0 : Width / 2),
				Y + (Direction == Direction4.Down ? Height : Direction == Direction4.Up ? 0 : Height / 2),
				500, 0,
				Direction.GetRotation(),
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, int.MaxValue
			);
			CellRenderer.SetLayerToDefault();

			// Fit Size to Target
			if (Target is Entity eTarget && cell.Width != eTarget.Width) {
				cell.Height = cell.Height * eTarget.Width / cell.Width;
				cell.Width = eTarget.Width;
			}

			// Animation
			const int HOP_GAP = 8;
			const int FIERCE_GAP = 8;
			if (Game.GlobalFrame < SpawnFrame + FIERCE_GAP) {
				// Fierce
				int localFrame = Game.GlobalFrame - SpawnFrame;
				if (localFrame % 3 != 0) {
					cell.Width = Util.RemapUnclamped(
						0, FIERCE_GAP,
						cell.Width, cell.Width * 3 / 2,
						localFrame
					);
					cell.Height = Util.RemapUnclamped(
						0, FIERCE_GAP,
						cell.Height * 3 / 2, cell.Height,
						localFrame
					);
				} else {
					cell.Width = cell.Height = 0;
				}
			} else if (Game.GlobalFrame > BurnedFrame) {
				// Weaken
				int weakenFrame = Game.GlobalFrame - BurnedFrame;
				int weakenDuration = LifeEndFrame - BurnedFrame;
				if (weakenFrame < weakenDuration - HOP_GAP) {
					// Weaken
					cell.Width = Util.RemapUnclamped(0, weakenDuration, cell.Width, 0, weakenFrame);
					cell.Height = Util.RemapUnclamped(0, weakenDuration, cell.Height, 0, weakenFrame);
				} else {
					// Hop
					cell.Width = weakenFrame % 3 == 0 ? 0 : cell.Width * 2 / 3;
					cell.Height = weakenFrame % 3 == 0 ? 0 : cell.Height * 2 / 3;
				}
			}
		}


		#endregion




		#region --- API ---


		public void Setup (int burnDuration, Direction4 direction, int width = Const.CEL, int height = Const.CEL) {
			Width = width;
			Height = height;
			BurnedFrame = Game.GlobalFrame + burnDuration;
			LifeEndFrame = BurnedFrame + WeakenDuration;
			Target = null;
			Direction = direction;
		}
		public void Setup (ICombustible com) {
			if (com == null) return;
			if (com is Entity entity) {
				Width = entity.Width;
				Height = entity.Height;
			}
			BurnedFrame = Game.GlobalFrame + com.BurnedDuration;
			LifeEndFrame = BurnedFrame + WeakenDuration;
			Direction = Direction4.Up;
			Target = com;
			com.BurnStartFrame = Game.GlobalFrame;
		}


		public void Spread () {
			var hits = CellPhysics.OverlapAll(
				PhysicsMask.ENTITY,
				Rect.Expand(SpreadRange), out int count,
				this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not ICombustible com || !hit.Entity.Active || com.IsBurning) continue;
				if (Stage.TrySpawnEntity(TypeID, hit.Rect.x, hit.Rect.y, out var fEntity) && fEntity is Fire fire) {
					fire.Setup(com);
				}
			}
		}


		public void PutOut () {
			BurnedFrame = Game.GlobalFrame;
			LifeEndFrame = Game.GlobalFrame + WeakenDuration;
			SpreadFrame = int.MaxValue;
		}


		#endregion




	}
}