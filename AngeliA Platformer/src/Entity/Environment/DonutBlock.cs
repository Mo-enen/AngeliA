using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


[EntityAttribute.Capacity(256)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class DonutBlock : Entity, IBlockEntity {


	// SUB
	protected enum BreakMode {
		BreakOnTouchGround = 0,
		BreakOnFall = 1,
		DoNotBreak = 2,
	}

	// Api
	protected virtual BreakMode BreakCondition { get; } = BreakMode.BreakOnTouchGround;
	protected virtual int HoldDuration => 60;
	protected virtual int FallingVelocity => 24;
	protected bool IsFalling { get; private set; } = false;
	protected bool IsHolding { get; private set; } = false;
	protected int HoldStartFrame { get; private set; } = int.MaxValue;

	// Data
	private bool LastHolding = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		HoldStartFrame = int.MaxValue;
		LastHolding = false;
		IsFalling = false;
		IsHolding = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}


	public override void Update () {

		int frame = Game.GlobalFrame;
		var rect = Rect;

		if (IsFalling) {
			// Falling
			IsHolding = false;
			LastHolding = false;
			Y -= FallingVelocity;
			ICarrier.CarryTargetsOnTopVertically(this, -FallingVelocity, OperationMode.ColliderAndTrigger);
		} else {
			// Idle
			IsHolding = !Physics.RoomCheck(PhysicsMask.CHARACTER, rect, this, Direction4.Up);
			if (IsHolding) {
				if (!LastHolding) HoldStartFrame = frame;
				if (frame - HoldStartFrame > HoldDuration) {
					IsFalling = true;
				}
			}
			LastHolding = IsHolding;
		}
		// Break Check
		if (IsFalling) {
			switch (BreakCondition) {
				case BreakMode.BreakOnTouchGround: {
					if (
						!Physics.RoomCheck(PhysicsMask.SOLID, rect, this, Direction4.Down) ||
						!Physics.RoomCheckOneway(PhysicsMask.MAP, rect, this, Direction4.Down, true)
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

		base.Update();
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(
			TypeID, X + Width / 2, Y + Height / 2,
			500, 500,
			IsHolding ? (Game.GlobalFrame * 4 - HoldStartFrame).PingPong(12) - 6 : 0,
			Width, Height,
			IsHolding || IsFalling ? new Color32(255, 196, 164, 255) : new Color32(255, 255, 255, 255)
		);
	}


	protected virtual void Break () {
		Active = false;
		FrameworkUtil.InvokeObjectBreak(TypeID, Rect);
	}

}
