using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eBrittle : eRigidbody {


		// SUB
		protected enum BreakMode {
			BreakOnCollideGround = 0,
			BreakOnFall = 1,
			DoNotBreak = 2,
		}

		// Api
		public override int CollisionLayer => YayaConst.ENVIRONMENT;
		protected virtual BreakMode BreakCondition { get; } = BreakMode.BreakOnCollideGround;
		public override bool CarryRigidbodyOnTop => false;
		protected virtual int HoldDuration { get; } = 60;
		protected virtual int FallingVelocity { get; } = 24;
		protected bool IsFalling { get; private set; } = false;
		protected bool IsHolding { get; private set; } = false;

		// Data
		private int HoldStartFrame = int.MaxValue;
		private bool LastHolding = false;


		// MSG
		public override void OnActived (int frame) {
			base.OnActived(frame);
			HoldStartFrame = int.MaxValue;
			LastHolding = false;
			IsFalling = false;
			IsHolding = false;
		}


		public override void PhysicsUpdate (int frame) {

			GravityScale = 0;
			VelocityX = 0;
			VelocityY = IsFalling ? -FallingVelocity : 0;
			var rect = Rect;

			// Fall Check
			if (!IsFalling) {
				IsHolding = !CellPhysics.RoomCheck((int)PhysicsMask.Character, rect, this, Direction4.Up);
				if (IsHolding) {
					if (!LastHolding) HoldStartFrame = frame;
					if (frame - HoldStartFrame > HoldDuration) {
						IsFalling = true;
					}
				}
				LastHolding = IsHolding;
			} else {
				IsHolding = false;
				LastHolding = false;
			}

			// Break Check
			if (IsFalling) {
				switch (BreakCondition) {
					case BreakMode.BreakOnCollideGround: {
						if (
							!CellPhysics.RoomCheck((int)PhysicsMask.Solid, rect, this, Direction4.Down) ||
							!CellPhysics.RoomCheck_Oneway((int)PhysicsMask.Map, rect, this, Direction4.Down, true)
						) {
							Break();
						}
						break;
					}
					case BreakMode.BreakOnFall: {
						Break();
						break;
					}
				}
			}

			base.PhysicsUpdate(frame);
		}


		protected virtual void Break () => Active = false;


		protected int GetHoldedFrame (int frame) => IsHolding ? frame - HoldStartFrame : 0;


	}
}
