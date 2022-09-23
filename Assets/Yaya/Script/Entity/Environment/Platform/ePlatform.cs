using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.EntityCapacity(128)]
	public abstract class ePlatform : Entity, IRigidbodyCarrier {


		// Artwork
		protected virtual int ArtworkCode_Left => TrimedTypeID;
		protected virtual int ArtworkCode_Mid => TrimedTypeID;
		protected virtual int ArtworkCode_Right => TrimedTypeID;
		protected virtual int ArtworkCode_Single => TrimedTypeID;
		public int CarrierSpeed => X - PrevX;

		// Short
		protected bool TouchedByPlayer { get; private set; } = false;
		protected bool TouchedByCharacter { get; private set; } = false;
		protected bool TouchedByRigidbody { get; private set; } = false;
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
		protected int PrevX { get; private set; } = 0;
		protected int PrevY { get; private set; } = 0;

		// Data
		private static readonly HitInfo[] c_Overlaps = new HitInfo[32];


		// MSG
		public override void OnActived () {
			base.OnActived();
			TouchedByPlayer = false;
			TouchedByCharacter = false;
			TouchedByRigidbody = false;
			PrevX = X;
			PrevY = Y;
			Pose = Yaya.Current.WorldSquad.GetEntityPose(TypeID, X, Y, true);
		}


		public override void FillPhysics () => CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			PrevX = X;
			PrevY = Y;
			Move();
			Update_Carry();
			Update_Touch();
		}


		public override void FrameUpdate () => CellRenderer.Draw(Pose switch {
			FittingPose.Left => ArtworkCode_Left,
			FittingPose.Mid => ArtworkCode_Mid,
			FittingPose.Right => ArtworkCode_Right,
			FittingPose.Single => ArtworkCode_Single,
			_ => TrimedTypeID,
		}, Rect);


		private void Update_Touch () {
			if (!TouchedByRigidbody || !TouchedByCharacter || !TouchedByPlayer) {
				int count = CellPhysics.OverlapAll(c_Overlaps, YayaConst.MASK_ENTITY, Rect.Expand(1), this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Rect.y < Y + Height) continue;
					if (hit.Entity is not Rigidbody) continue;
					TouchedByRigidbody = true;
					if (hit.Entity is not eCharacter) continue;
					TouchedByCharacter = true;
					if (hit.Entity is not ePlayer) continue;
					TouchedByPlayer = true;
					break;
				}
			}
		}


		private void Update_Carry () {
			if (Y == PrevY) return;
			var rect = Rect;
			var prevRect = rect;
			prevRect.y = PrevY;
			int left = X;
			int right = X + Width;
			if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
				left = int.MinValue;
			}
			if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
				right = int.MaxValue;
			}
			if (Y > PrevY) {
				// Moving Up
				prevRect.height -= rect.height / 3;
				rect.y = PrevY + prevRect.height;
				rect.height = Y + Height - rect.y;
				int count = CellPhysics.OverlapAll(c_Overlaps, YayaConst.MASK_RIGIDBODY, rect, this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
					if (rig.VelocityY > 0) continue;
					if (!rig.Rect.Overlaps(prevRect)) {
						rig.PerformMove(0, rect.yMax - rig.Y);
						rig.MakeGrounded(1, TrimedTypeID);
					}
				}
			} else {
				// Moving Down
				prevRect.height += PrevY - Y;
				int count = CellPhysics.OverlapAll(c_Overlaps, YayaConst.MASK_RIGIDBODY, prevRect, this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
					if (rig.Y <= rect.yMax) continue;
					if (rig.VelocityY > 0) continue;
					rig.PerformMove(0, rect.yMax - 1 - rig.Y);
					rig.MakeGrounded(1, TrimedTypeID);
				}
			}
		}


		// ABS
		protected abstract void Move ();


		// API
		public void InvokePlayerTouch () {
			TouchedByRigidbody = true;
			TouchedByCharacter = true;
			TouchedByPlayer = true;
		}


	}
}