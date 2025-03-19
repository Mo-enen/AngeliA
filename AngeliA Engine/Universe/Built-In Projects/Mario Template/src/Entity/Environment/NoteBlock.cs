using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class NoteBlock : Entity, IBumpable, IBlockEntity, IAutoTrackWalker {

	// VAR
	private static readonly AudioCode BUMP_AC = "Bump";
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
		if (Game.GlobalFrame > LastBumpedFrame + 12) {
			var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, Rect.EdgeOutsideUp(1), out int count, this);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody) continue;
				LastBumpedFrame = Game.GlobalFrame;
				Game.PlaySoundAtPosition(BUMP_AC, XY);
				FrameworkUtil.PerformSpringBounce(this, Direction4.Up, 64);
				ArtworkOffset.x = 0;
				ArtworkOffset.y = -Const.CEL;
			}
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect.Shift(ArtworkOffset));
		ArtworkOffset.x = ArtworkOffset.x.LerpTo(0, 0.4f);
		ArtworkOffset.y = ArtworkOffset.y.LerpTo(0, 0.4f);
	}

	void IBumpable.OnBumped (Entity entity, Damage damage) => NoteBlockBumpLogic(LastBumpFrom);

	bool IBumpable.AllowBump (Entity entity, Direction4 from) => IBumpable.IsValidBumpDirection(this, from);

	Damage IBumpable.GetBumpTransferDamage () => new(1, Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT);

	private void NoteBlockBumpLogic (Direction4 bumpFrom) {

		Game.PlaySoundAtPosition(BUMP_AC, XY);

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
