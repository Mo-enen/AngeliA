using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {



	public class eWoodPlatformH : ePingPongPlatform {
		protected override uint SpeedX => 8;
		protected override Vector2Int Distance => new(Const.CELL_SIZE * 5, 0);
	}



	public class eWoodPlatformV : ePingPongPlatform {
		protected override uint SpeedY => 8;
		protected override Vector2Int Distance => new(0, Const.CELL_SIZE * 5);
	}



	public abstract class ePingPongPlatform : ePlatform, IRigidbodyCarrier {


		// Abs
		protected virtual uint SpeedX => 0;
		protected virtual uint SpeedY => 0;
		protected abstract Vector2Int Distance { get; }
		public int CarrierSpeed { get; set; } = 0;

		// Data
		private Vector2Int From = default;
		private Vector2Int To = default;
		private int DurationX = 0;
		private int DurationY = 0;


		// MSG
		public override void OnActived () {
			base.OnActived();
			From.x = X - Distance.x / 2;
			From.y = Y - Distance.y / 2;
			To.x = X + Distance.x / 2;
			To.y = Y + Distance.y / 2;
			DurationX = SpeedX > 0 ? Distance.x / (int)SpeedX : 0;
			DurationY = SpeedY > 0 ? Distance.y / (int)SpeedY : 0;
		}


		protected override void Move () {
			if (DurationX > 0) {
				int localFrameX = Game.GlobalFrame.PingPong(DurationX);
				int prevX = X;
				X = Util.RemapUnclamped(0, DurationX, From.x, To.x, localFrameX);
				CarrierSpeed = X - prevX;
			} else {
				CarrierSpeed = 0;
			}
			if (DurationY > 0) {
				int localFrameY = Game.GlobalFrame.PingPong(DurationY);
				Y = Util.RemapUnclamped(0, DurationY, From.y, To.y, localFrameY);
			}
		}


	}



	public abstract class ePlatform : Entity {


		// Data
		private static readonly HitInfo[] c_Overlaps = new HitInfo[32];
		protected bool TouchedByPlayer = false;
		protected bool TouchedByCharacter = false;
		protected bool TouchedByRigidbody = false;
		private int PrevY = 0;


		// MSG
		public override void OnActived () {
			base.OnActived();
			TouchedByPlayer = false;
			TouchedByCharacter = false;
			TouchedByRigidbody = false;
			PrevY = 0;
		}


		public override void FillPhysics () => CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			PrevY = Y;
			Move();
			Update_CarryY();
			Update_Touch();
		}


		public override void FrameUpdate () => CellRenderer.Draw(TrimedTypeID, Rect);


		private void Update_Touch () {
			if (!TouchedByRigidbody || !TouchedByCharacter || !TouchedByPlayer) {
				int count = CellPhysics.OverlapAll(c_Overlaps, YayaConst.MASK_ENTITY, Rect.Expand(1), this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
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


		private void Update_CarryY () {
			if (Y > PrevY) {
				// Moving Up
				var rect = Rect;
				var prevRect = rect;
				prevRect.y = PrevY;
				prevRect.height -= rect.height / 3;
				int count = CellPhysics.OverlapAll(c_Overlaps, YayaConst.MASK_RIGIDBODY, rect, this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (!rig.Rect.Overlaps(prevRect)) {
						rig.Y = rect.yMax;
						rig.MakeGrounded(0, TrimedTypeID);
					}
				}
			} else if (Y < PrevY) {
				// Moving Down
				var rect = Rect;
				var prevRect = rect;
				prevRect.y = PrevY;
				prevRect = prevRect.Expand(0, 0, 0, 8);
				int count = CellPhysics.OverlapAll(c_Overlaps, YayaConst.MASK_RIGIDBODY, prevRect, this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.VelocityY > 0) continue;
					rig.Y = rect.yMax - 1;
					rig.MakeGrounded(0, TrimedTypeID);
				}
			}
		}


		// ABS
		protected abstract void Move ();


	}
}