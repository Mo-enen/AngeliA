using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class BrickBlock : Entity, IBumpable, IBlockEntity, IAutoTrackWalker, IDamageReceiver {


	// VAR
	private static readonly AudioCode BUMP_AC = "Bump";
	private static readonly AudioCode BREAK_AC = "BrickBreak";
	private static readonly SpriteCode REVEALED_SP = "RevealedBlock";
	public static readonly int TYPE_ID = typeof(BrickBlock).AngeHash();
	public int LastBumpedFrame { get; set; } = int.MinValue;
	bool IBlockEntity.EmbedEntityAsElement => true;
	public Direction4 LastBumpFrom { get; set; }
	bool IBumpable.TransferWithAttack => true;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	private bool IsCoin => SpawnItemStartFrame < 0 && PSwitch.Triggering;

	private int ItemInside;
	private int SpawnItemStartFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		SpawnItemStartFrame = int.MinValue;
		ItemInside = MarioUtil.GetEmbedItemID(Rect, failbackID: 0);
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
		MarioUtil.UpdateForBumpToSpawnItem(this, ItemInside, SpawnItemStartFrame, LastBumpFrom.Opposite());

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
			// Coin
			Renderer.Draw(Coin.TYPE_ID, Rect);
		} else if (SpawnItemStartFrame < 0) {
			// Brick
			var cell = Renderer.Draw(TypeID, Rect);
			IBumpable.AnimateForBump(this, cell);
		} else {
			// Revealed
			var cell = Renderer.Draw(REVEALED_SP, Rect);
			IBumpable.AnimateForBump(this, cell);
		}
	}

	bool IBumpable.AllowBump (Entity entity, Direction4 from) => IBumpable.IsValidBumpDirection(this, from) && entity == PlayerSystem.Selecting && SpawnItemStartFrame < 0;

	void IBumpable.OnBumped (Entity entity, Damage damage) {
		if (IsCoin) return;
		bool bigPlayer = entity is PlayableCharacter pCh && pCh.Health.HP >= 2;
		bool withMagDamage = damage.Amount > 0 && damage.Type.HasAll(Tag.MagicalDamage);
		if (ItemInside == 0 && (bigPlayer || withMagDamage)) {
			// Break
			Active = false;
			FrameworkUtil.InvokeObjectBreak(TypeID, Rect);
			Game.PlaySoundAtPosition(BREAK_AC, XY);
		} else {
			// Spawn Item
			if (ItemInside == 0) {
				Game.PlaySoundAtPosition(BUMP_AC, XY);
			}
			if (ItemInside == 0 || SpawnItemStartFrame >= 0) return;
			SpawnItemStartFrame = Game.GlobalFrame;
		}
	}

	Damage IBumpable.GetBumpTransferDamage () => new(1, Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT);

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (damage.Amount <= 0 || !damage.Type.HasAll(Tag.MagicalDamage)) return;
		if (IsCoin || SpawnItemStartFrame >= 0) return;
		Active = false;
		FrameworkUtil.InvokeObjectBreak(TypeID, Rect);
	}

}
