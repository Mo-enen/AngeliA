using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class Slime : Enemy, ISlimeWalker {

	// VAR
	public override IRect Rect {
		get {
			int width = Movement.MovementWidth;
			int height = Movement.MovementHeight;
			return (this as ISlimeWalker).AttachingDirection switch {
				Direction5.Down => new(X - width / 2, Y, width, height),
				Direction5.Up => new(X - width / 2, Y - height, width, height),
				Direction5.Left => new(X, Y - width / 2, height, width),
				Direction5.Right => new(X - height, Y - width / 2, height, width),
				_ => base.Rect,
			};
		}
	}
	public override bool AllowBeingPush => false;
	public Direction5 AttachingDirection { get; set; }
	public Int2 LocalPosition { get; set; }
	public IRect AttachingRect { get; set; }
	public Entity AttachingTarget { get; set; }
	public int AttachingID { get; set; }
	public int WalkSpeed => Movement.RunSpeed;
	public bool FacingPositive { get; set; }
	private float CurrentRotation = 0;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		if (FromWorld) {
			X = X.ToUnifyGlobal() + Const.HALF;
		}
		Movement.RunSpeed.BaseValue = 12;
		Movement.PushAvailable.BaseValue = false;
		Movement.SquatAvailable.BaseValue = false;
		Movement.FlyAvailable.BaseValue = false;
		Movement.RushAvailable.BaseValue = false;
		Movement.DashAvailable.BaseValue = false;
		Movement.MovementWidth.BaseValue = 220;
		Movement.MovementHeight.BaseValue = 136;
		ISlimeWalker.ActiveWalker(this);
		FacingPositive = true;
		CurrentRotation = 0;
	}

	public override void FirstUpdate () {
		if (!Health.TakingDamage) {
			FillAsTrigger(0);
		} else {
			IgnorePhysics.False(1);
		}
		base.FirstUpdate();
		if (CharacterState == CharacterState.GamePlay && IgnorePhysics) {
			Physics.FillEntity(PhysicalLayer, this, true);
		}
	}

	public override void Update () {
		base.Update();

		if (CharacterState != CharacterState.GamePlay || Health.TakingDamage) return;

		// Inside Ground
		if (IsInsideGround) {
			Movement.Stop();
			VelocityY = Util.QuickRandom(3,12);
			return;
		}

		XY = ISlimeWalker.GetAttachingPosition(this);
		if (ISlimeWalker.RefreshAttachingDirection(this) != Direction5.Center) {
			// Walking
			IgnorePhysics.True(1);
			XY = ISlimeWalker.GetNextSlimePosition(this);
			Movement.Move(FacingPositive ? Direction3.Right : Direction3.Left, Direction3.None);
			Movement.LockFacingRight(FacingPositive);
		} else {
			// Falling
			Movement.Stop();
		}
	}

	public override void OnCharacterRendered () {
		base.OnCharacterRendered();

		// Rotate Cell
		if (Rendering is not SheetCharacterRenderer sRenderer) return;
		var cell = sRenderer.RenderedCell;
		if (cell == null || cell.Sprite == null) return;
		var walker = this as ISlimeWalker;
		var renderingDir = walker.AttachingDirection.Opposite().ToDirection4();
		CurrentRotation = Util.LerpAngleUnclamped(
			CurrentRotation,
			renderingDir.GetRotation(),
			0.3f
		);
		cell.Rotation = CurrentRotation.RoundToInt();

	}

	protected override bool GroundedCheck () {
		var walker = this as ISlimeWalker;
		GroundedID = walker.AttachingID;
		return IsInsideGround || walker.AttachingDirection != Direction5.Center;
	}

	public override void OnDamaged (Damage damage) {
		base.OnDamaged(damage);
		LocalPosition = Int2.zero;
		AttachingDirection = Direction5.Center;
		AttachingTarget = null;
		AttachingRect = default;
		IgnorePhysics.False(1);
	}

}
