using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {


	public class DonutBlockDirt : DonutBlock {
		protected override BreakMode BreakCondition => BreakMode.BreakOnCollideGround;
	}


	[EntityAttribute.Capacity(256)]
	public abstract class DonutBlock : EnvironmentEntity {


		// SUB
		protected enum BreakMode {
			BreakOnCollideGround = 0,
			BreakOnFall = 1,
			DoNotBreak = 2,
		}

		// Api
		protected virtual BreakMode BreakCondition { get; } = BreakMode.BreakOnCollideGround;
		protected virtual int HoldDuration => 60;
		protected virtual int FallingVelocity => 24;
		protected bool IsFalling { get; private set; } = false;
		protected bool IsHolding { get; private set; } = false;

		// Data
		private int HoldStartFrame = int.MaxValue;
		private bool LastHolding = false;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			HoldStartFrame = int.MaxValue;
			LastHolding = false;
			IsFalling = false;
			IsHolding = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
		}


		public override void PhysicsUpdate () {

			int frame = Game.GlobalFrame;
			var rect = Rect;

			// Fall Check
			if (!IsFalling) {
				IsHolding = !CellPhysics.RoomCheck(PhysicsMask.CHARACTER, rect, this, Direction4.Up);
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
							!CellPhysics.RoomCheck(PhysicsMask.SOLID, rect, this, Direction4.Down) ||
							!CellPhysics.RoomCheckOneway(PhysicsMask.MAP, rect, this, Direction4.Down, true)
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
				Y -= FallingVelocity;
			}

			base.PhysicsUpdate();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(
				TypeID, X + Width / 2, Y + Height / 2,
				500, 500,
				IsHolding ? (Game.GlobalFrame * 4 - HoldStartFrame).PingPong(12) - 6 : 0,
				Width, Height,
				IsHolding || IsFalling ? new Byte4(255, 196, 164, 255) : new Byte4(255, 255, 255, 255)
			);
		}


		protected virtual void Break () => Active = false;


	}

}
