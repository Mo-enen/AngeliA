using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class NoteBlock : Entity, IBumpable, IBlockEntity, IAutoTrackWalker {

	// VAR
	bool IBumpable.FromAbove => true;
	bool IBumpable.FromBelow => true;
	public int LastBumpedFrame { get; set; } = int.MinValue;
	bool IBumpable.TransferWithAttack => true;
	public Direction4 LastBumpFrom { get; set; }
	bool IBlockEntity.EmbedEntityAsElement => true;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }


	private int ItemInside;
	private int OffsetY;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		ItemInside = MarioUtil.GetEmbedItemID(Rect);
		OffsetY = 0;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, (this as IAutoTrackWalker).OnTrack);
	}

	public override void Update () {
		base.Update();
		// Spring Check
		if (Physics.Overlap(PhysicsMask.DYNAMIC, Rect.EdgeOutsideUp(1), this)) {
			NoteBlockBumpLogic(Direction4.Up);
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect.Shift(0, OffsetY));
		OffsetY = OffsetY.LerpTo(0, 0.4f);
	}

	void IBumpable.OnBumped (Rigidbody rig) => NoteBlockBumpLogic(LastBumpFrom);

	bool IBumpable.AllowBump (Rigidbody rig) => true;

	private void NoteBlockBumpLogic (Direction4 bumpFrom) {

		// Bounce
		if (bumpFrom != Direction4.Down) {
			FrameworkUtil.PerformSpringBounce(this, bumpFrom, 64);
		}

		// Animation
		if (bumpFrom == Direction4.Down) {
			OffsetY = Const.CEL;
		} else {
			OffsetY = -Const.CEL;
		}

		// Spawn Item
		if (ItemInside != 0) {
			if (bumpFrom == Direction4.Down) {
				// From Below
				// Spawn Item Inside
				if (MarioUtil.SpawnEmbedItem(ItemInside, Rect, Direction4.Up) is Rigidbody rItem) {
					rItem.VelocityY = 64;
				}
				ItemInside = 0;
			} else {
				// From Above
				// Spawn Item Inside
				if (MarioUtil.SpawnEmbedItem(ItemInside, Rect, Direction4.Down) is Rigidbody rItem) {
					rItem.VelocityY = -32;
				}
				ItemInside = 0;
			}
		}
	}

}
