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
	private Int2 ArtworkOffset;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		ItemInside = MarioUtil.GetEmbedItemID(Rect);
		ArtworkOffset = default;
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
		Renderer.Draw(TypeID, Rect.Shift(ArtworkOffset));
		ArtworkOffset.x = ArtworkOffset.x.LerpTo(0, 0.4f);
		ArtworkOffset.y = ArtworkOffset.y.LerpTo(0, 0.4f);
	}

	void IBumpable.OnBumped (Rigidbody rig, Damage damage) => NoteBlockBumpLogic(LastBumpFrom);

	bool IBumpable.AllowBump (Rigidbody rig, Direction4 from) => IBumpable.IsValidBumpDirection(this, from);

	Damage IBumpable.GetBumpTransferDamage () => new(1, Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT);

	private void NoteBlockBumpLogic (Direction4 bumpFrom) {

		if (Game.GlobalFrame < LastBumpedFrame + 12) return;
		LastBumpedFrame = Game.GlobalFrame;

		// Bounce
		if (bumpFrom == Direction4.Up) {
			FrameworkUtil.PerformSpringBounce(this, Direction4.Up, 64);
		}

		// Animation
		ArtworkOffset = bumpFrom.Opposite().Normal() * Const.CEL;

		// Spawn Item
		if (ItemInside != 0) {
			// Spawn Item Inside
			if (MarioUtil.SpawnEmbedItem(ItemInside, Rect, bumpFrom.Opposite()) is Rigidbody rItem) {
				var normal = bumpFrom.Opposite().Normal();
				rItem.VelocityX = normal.x * 32;
				rItem.VelocityY = bumpFrom == Direction4.Down ? 64 : normal.y * 32;
			}
			ItemInside = 0;
		}
	}

}
