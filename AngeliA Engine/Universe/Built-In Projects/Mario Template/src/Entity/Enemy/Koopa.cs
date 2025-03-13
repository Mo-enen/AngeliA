using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class GreenKoopa : Koopa, IPingPongWalker {
	private static readonly SpriteCode WALK_SP = "GreenKoopa.Walk";
	private static readonly SpriteCode ROLLING_SP = "GreenKoopa.Rolling";
	protected override SpriteCode WalkSprite => WALK_SP;
	protected override SpriteCode RollingSprite => ROLLING_SP;
	bool IPingPongWalker.WalkOffEdge => true;
}


public class RedKoopa : Koopa, IPingPongWalker {
	private static readonly SpriteCode WALK_SP = "RedKoopa.Walk";
	private static readonly SpriteCode ROLLING_SP = "RedKoopa.Rolling";
	protected override SpriteCode WalkSprite => WALK_SP;
	protected override SpriteCode RollingSprite => ROLLING_SP;
	bool IPingPongWalker.WalkOffEdge => IsInShell && IsRolling;
}


public abstract class Koopa : Enemy, IPingPongWalker, IDamageReceiver {


	// VAR
	protected abstract SpriteCode WalkSprite { get; }
	protected abstract SpriteCode RollingSprite { get; }
	int IPingPongWalker.WalkSpeed => IsPassout ? 0 : !IsInShell ? 8 : IsRolling ? 32 : 0;
	bool IPingPongWalker.WalkOffEdge => true;
	int IPingPongWalker.LastTurnFrame { get; set; }
	bool IPingPongWalker.WalkingRight { get; set; }
	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	protected bool IsInShell = false;
	protected bool IsRolling = false;
	private int LastDamageFrame = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsInShell = false;
		IsRolling = false;
		LastDamageFrame = int.MinValue;
		IPingPongWalker.OnActive(this);
	}

	public override void Update () {
		base.Update();

		if (!IsInShell) IsRolling = false;

		// Walk
		IPingPongWalker.PingPongWalk(this);

		// Damage Enemy
		if (IsRolling) {


		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		bool facingRight = (this as IPingPongWalker).WalkingRight;
		if (IsInShell) {
			if (IsRolling) {
				// Rolling
				Renderer.Draw(RollingSprite, X + Width / 2, Y, 500, 0, 0, facingRight ? Const.ORIGINAL_SIZE_NEGATAVE : Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
			} else if (Renderer.TryGetAnimationGroup(RollingSprite, out var inShellGp) && inShellGp.Count > 0) {
				// In Shell
				Renderer.Draw(inShellGp.Sprites[0], X + Width / 2, Y, 500, 0, 0, facingRight ? Const.ORIGINAL_SIZE_NEGATAVE : Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
			}
		} else {
			// Walking
			Renderer.Draw(
				WalkSprite, X + Width / 2, Y, 500, 0, 0,
				facingRight ? Const.ORIGINAL_SIZE_NEGATAVE : Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE
			);
		}
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (damage.Amount <= 0) return;
		if (Game.GlobalFrame < LastDamageFrame + 20) return;
		LastDamageFrame = Game.GlobalFrame;
		IsInShell = true;
		IsRolling = !IsRolling;
	}

	protected override void OnPlayerStepOn (Character player) {
		player.VelocityY = 64;
		MarioUtil.PlayMarioAudio(Sound.StepOnEnemy, XY);
		if (IsInShell) {
			if (IsRolling) {
				IsRolling = false;
			} else {
				IsRolling = true;
				(this as IPingPongWalker).WalkingRight = player.Rect.CenterX() < X + Width / 2;
			}
		} else {
			IsInShell = true;
		}
	}

}
