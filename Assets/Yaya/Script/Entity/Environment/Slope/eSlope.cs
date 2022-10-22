using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public abstract class eSlope : Entity {




		#region --- VAR ---


		// Api
		public abstract Direction3 DirectionVertical { get; }
		public abstract Direction3 DirectionHorizontal { get; }
		public virtual int CollisionMask => YayaConst.MASK_RIGIDBODY;

		// Data
		private readonly HitInfo[] c_Overlap = new HitInfo[16];
		private bool? UnfillBottomBlock = null;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			UnfillBottomBlock = null;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			// Unfill
			if (!UnfillBottomBlock.HasValue) {
				if (DirectionVertical == Direction3.Up) {
					UnfillBottomBlock = CellPhysics.HasEntity<eSlope>(
						new(DirectionHorizontal == Direction3.Left ? X - Const.CEL / 2 : X + Const.CEL + Const.CEL / 2, Y - Const.CEL / 2, 1, 1), YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
					);
				} else {
					UnfillBottomBlock = false;
				}
			}
			if (UnfillBottomBlock.HasValue && UnfillBottomBlock.Value) {
				CellPhysics.Unfill(
					YayaConst.MASK_LEVEL,
					new(X + Const.CEL / 2, Y - Const.CEL / 2, 1, 1),
					false, true
				);
			}
			// Fix Rig
			int count = CellPhysics.OverlapAll(c_Overlap, CollisionMask, Rect);
			for (int i = 0; i < count; i++) {
				var hit = c_Overlap[i];
				if (hit.Entity is not eYayaRigidbody rig) continue;
				if (CheckOverlap(rig.Rect, out int dis)) FixPosition(rig, dis);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}


		#endregion




		#region --- LGC ---


		private bool CheckOverlap (RectInt rect, out int distance) {
			distance = 0;
			int cornerX = DirectionHorizontal == Direction3.Left ? rect.xMax : rect.xMin;
			if (!cornerX.InRange(Rect.xMin, Rect.xMax)) return false;
			int cornerY = DirectionVertical == Direction3.Up ? rect.yMin : rect.yMax;
			if ((DirectionHorizontal == Direction3.Left) == (DirectionVertical == Direction3.Up)) {
				// LU, RD ◿ ◸
				distance = cornerY - (Rect.yMin + (cornerX - Rect.x));
			} else {
				// RU, LD ◺◹
				distance = cornerY - (Rect.yMax - (cornerX - Rect.x));
			}
			bool underTarget = distance < 0;
			distance = Mathf.Abs(distance);
			return (DirectionVertical == Direction3.Up) == underTarget;
		}


		private void FixPosition (eYayaRigidbody target, int distance) {
			if (DirectionVertical == Direction3.Up) {
				// Fix Pos
				target.MakeGrounded(4);
				target.PerformMove(
					DirectionHorizontal == Direction3.Left ? -distance / 2 : distance / 2,
					DirectionVertical == Direction3.Down ? -distance / 2 : distance / 2,
					true, false
				);
				// Fix Velocity
				if (target.VelocityX == 0) {
					// Fix X (Drop)
					target.VelocityY = 0;
					target.IgnoreGravity();
				} else {
					// Fix Y (Walk)
					if ((DirectionHorizontal == Direction3.Left) == (target.VelocityX > 0)) {
						// Walk Toward
						target.VelocityY = DirectionVertical == Direction3.Down ?
							-Mathf.Abs(target.VelocityX) :
							Mathf.Abs(target.VelocityX);
					} else {
						// Walk Away
						target.VelocityY = -Mathf.Abs(target.VelocityX);
					}
				}
			} else {
				// Down
				// Fix Pos
				target.PerformMove(DirectionHorizontal == Direction3.Left ? -distance : distance, 0, true, false);
			}
		}


		#endregion




	}
}
