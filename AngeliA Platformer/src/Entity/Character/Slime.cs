using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class Slime : Enemy, ISlimeWalker {

	// VAR
	protected virtual bool DamageOnTouch => true;

	// Walker
	Direction5 ISlimeWalker.AttachingDirection { get; set; }
	Int2 ISlimeWalker.LocalPosition { get; set; }
	IRect ISlimeWalker.AttachingRect { get; set; }
	Entity ISlimeWalker.AttachingTarget { get; set; }
	int ISlimeWalker.WalkSpeed => 12;
	bool ISlimeWalker.FacingPositive { get; set; }


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Movement.MovementWidth.BaseValue = 220;
		Movement.MovementHeight.BaseValue = 136;
		ISlimeWalker.ActiveWalker(this);
	}

	public override void FirstUpdate () {
		FillAsTrigger(1);
		base.FirstUpdate();
		if (CharacterState == CharacterState.GamePlay && IgnorePhysics) {
			Physics.FillEntity(PhysicalLayer, this, true);
		}
		if (DamageOnTouch) {
			Physics.FillBlock(PhysicsLayer.DAMAGE, TypeID, Rect);
		}
	}

	public override void Update () {
		base.Update();
		if (ISlimeWalker.RefreshAttachingDirection(this) != Direction5.Center) {
			IgnorePhysics.True(1);
			XY = ISlimeWalker.GetNextSlimePosition(this);
		}
	}

	public override void OnCharacterRendered () {
		base.OnCharacterRendered();

		// Rotate Cell
		if (Rendering is not SheetCharacterRenderer sRenderer) return;
		var cell = sRenderer.RenderedCell;
		if (cell == null || cell.Sprite == null) return;
		var walker = this as ISlimeWalker;
		var renderingDir = walker.AttachingDirection.ToDirection4();
		if (renderingDir == Direction4.Up) return;

		cell.Rotation = renderingDir.Opposite().GetRotation();

	}

}
