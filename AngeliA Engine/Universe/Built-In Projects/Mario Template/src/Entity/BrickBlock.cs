using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

public class BrickBlock : Entity, IBumpable {

	// VAR
	private static readonly SpriteCode REVEALED_SP = "RevealedBlock";
	private static readonly SpriteCode SPIN_COIN_SP = "CoinSpin";
	public static readonly int TYPE_ID = typeof(BrickBlock).AngeHash();
	int IBumpable.LastBumpedFrame { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }
	private bool IsCoin => SpawnItemStartFrame < 0 && PSwitch.Triggering;

	private int ItemInside;
	private int SpawnItemStartFrame = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		SpawnItemStartFrame = int.MinValue;
		// Item Inside
		ItemInside = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ, BlockType.Element);
		if (!Stage.IsValidEntityID(ItemInside)) {
			ItemInside = 0;
		}

	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, isTrigger: IsCoin);
	}

	public override void Update () {
		base.Update();
		// Spawn Item
		const int RISE_DUR = 24;
		if (SpawnItemStartFrame >= 0 && Game.GlobalFrame <= SpawnItemStartFrame + RISE_DUR) {
			if (ItemInside == Coin.TYPE_ID) {
				// For Coin
				// Bounce Animation
				if (Renderer.TryGetSprite(SPIN_COIN_SP, out var iconSp, ignoreAnimation: false)) {
					int offsetY = Util.RemapUnclamped(
						0, RISE_DUR / 2,
						0, Height * 3,
						(Game.GlobalFrame - SpawnItemStartFrame).PingPong(RISE_DUR * 2 / 3)
					);
					Renderer.Draw(iconSp, Rect.Shift(0, offsetY));
				}
				// Collect Coin
				if (ItemInside != 0) {
					if (Game.GlobalFrame == SpawnItemStartFrame) {
						Coin.Collect(1);
						WorldSquad.Front.SetBlockAt(
							(X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ, BlockType.Element, 0
						);
					} else if (Game.GlobalFrame == SpawnItemStartFrame + RISE_DUR) {
						ItemInside = 0;
					}
				}
			} else {
				// For Entity
				// Rise Animation
				if (Renderer.TryGetSpriteForGizmos(ItemInside, out var iconSp)) {
					Renderer.Draw(iconSp, Rect.Shift(0, Util.RemapUnclamped(
						SpawnItemStartFrame, SpawnItemStartFrame + RISE_DUR,
						0, Height,
						Game.GlobalFrame
					)));
				}
				// Spawn
				if (ItemInside != 0 && Game.GlobalFrame == SpawnItemStartFrame + RISE_DUR) {
					var entity = Stage.SpawnEntity(ItemInside, X, Y + Height);
					if (entity != null) {
						var eRect = entity.Rect;
						entity.X += X + Width / 2 - eRect.CenterX();
						entity.Y += Y + Height - eRect.y;
					}
					ItemInside = 0;
					WorldSquad.Front.SetBlockAt(
						(X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ, BlockType.Element, 0
					);
				}
			}
		}
		// Collect As Coin
		var player = PlayerSystem.Selecting;
		if (IsCoin && player != null && player.Rect.Overlaps(Rect)) {
			// Collect Now
			Coin.Collect(1);
			FrameworkUtil.RemoveFromWorldMemory(this);
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
