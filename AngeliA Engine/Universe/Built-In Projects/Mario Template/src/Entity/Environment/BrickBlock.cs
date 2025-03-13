using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class BrickBlock : Entity, IBumpable, IBlockEntity, IAutoTrackWalker {

	// VAR
	private static readonly SpriteCode REVEALED_SP = "RevealedBlock";
	public static readonly int TYPE_ID = typeof(BrickBlock).AngeHash();
	public int LastBumpedFrame { get; set; } = int.MinValue;
	Direction4 IBumpable.LastBumpFrom { get; set; }
	bool IBlockEntity.EmbedEntityAsElement => true;
	bool IBumpable.TransferWithAttack => true;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	private bool IsCoin => SpawnItemStartFrame < 0 && PSwitch.Triggering;

	private int ItemInside;
	private int SpawnItemStartFrame = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		SpawnItemStartFrame = int.MinValue;
		ItemInside = MarioUtil.GetEmbedItemID(Rect);
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(
			PhysicsLayer.ENVIRONMENT, this,
			isTrigger: IsCoin || (this as IAutoTrackWalker).OnTrack
		);
	}

	public override void Update () {
		base.Update();

		// Spawn Item Update
		MarioUtil.UpdateForBumpToSpawnItem(Rect, ItemInside, SpawnItemStartFrame);

		// Collect As Coin
		var player = PlayerSystem.Selecting;
		if (IsCoin && player != null && player.Rect.Overlaps(Rect)) {
			// Collect Now
			Coin.Collect(1);
			Active = false;
		}

	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (IsCoin) {
			Renderer.Draw(Coin.TYPE_ID, Rect);
		} else {
			var cell = Renderer.Draw(SpawnItemStartFrame < 0 ? TypeID : REVEALED_SP, Rect);
			IBumpable.AnimateForBump(this, cell);
		}
	}

	bool IBumpable.AllowBump (Rigidbody rig) => rig == PlayerSystem.Selecting && SpawnItemStartFrame < 0;

	void IBumpable.OnBumped (Rigidbody rig) {
		if (ItemInside == 0 || SpawnItemStartFrame >= 0) return;
		SpawnItemStartFrame = Game.GlobalFrame;
	}

}
