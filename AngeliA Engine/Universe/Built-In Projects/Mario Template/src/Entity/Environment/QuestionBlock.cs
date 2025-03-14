using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class HiddenBlock : QuestionBlock {
	protected override bool IsHidden => true;
}


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[NoItemCombination]
public class QuestionBlock : Entity, IBlockEntity, IBumpable, IAutoTrackWalker {

	// VAR
	private static readonly SpriteCode REVEALED_SP = "RevealedBlock";
	protected virtual bool IsHidden => false;
	bool IBlockEntity.EmbedEntityAsElement => true;
	int IBumpable.LastBumpedFrame { get; set; }
	bool IBumpable.FromBelow => true;
	bool IBumpable.TransferWithAttack => true;
	public Direction4 LastBumpFrom { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	private int ItemInside;
	private int SpawnItemStartFrame = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		ItemInside = MarioUtil.GetEmbedItemID(Rect, failbackID: Coin.TYPE_ID);
		SpawnItemStartFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		if ((this as IAutoTrackWalker).OnTrack) {
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		} else {
			if (IsHidden && SpawnItemStartFrame < 0) {
				// Hide
				Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.OnewayDown);
			} else {
				// Reveal
				Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
			}
		}
	}

	public override void Update () {
		base.Update();
		// Spawn Item Update
		MarioUtil.UpdateForBumpToSpawnItem(Rect, ItemInside, SpawnItemStartFrame);

	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (IsHidden && SpawnItemStartFrame < 0) return;
		var cell = Renderer.Draw(SpawnItemStartFrame < 0 ? TypeID : REVEALED_SP, Rect);
		IBumpable.AnimateForBump(this, cell);
	}

	void IBumpable.OnBumped (Rigidbody rig, Damage damage) {
		if (ItemInside == 0 || SpawnItemStartFrame >= 0) return;
		SpawnItemStartFrame = Game.GlobalFrame;
	}

	bool IBumpable.AllowBump (Rigidbody rig, Direction4 from) {
		if (!IBumpable.IsValidBumpDirection(this, from)) return false;
		if (rig != PlayerSystem.Selecting) return false;
		return SpawnItemStartFrame < 0 && ItemInside != 0;
	}

	Damage IBumpable.GetBumpTransferDamage () => new(1, Const.TEAM_ENEMY);

}
