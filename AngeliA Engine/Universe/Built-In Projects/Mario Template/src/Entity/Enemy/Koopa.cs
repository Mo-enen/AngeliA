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
	bool IPingPongWalker.WalkOffEdge => IsRolling;
}


public abstract class Koopa : Enemy, IPingPongWalker, IDamageReceiver {


	// VAR
	protected override bool AttackOnTouchPlayer => !IsInShell || (IsRolling && Game.GlobalFrame > RollingStartFrame + 12);
	protected abstract SpriteCode WalkSprite { get; }
	protected abstract SpriteCode RollingSprite { get; }
	int IPingPongWalker.WalkSpeed => IsPassout ? 0 : !IsInShell ? 8 : IsRolling ? 32 : 0;
	bool IPingPongWalker.WalkOffEdge => true;
	int IPingPongWalker.TurningCheckMask => IsRolling ? PhysicsMask.MAP : PhysicsMask.SOLID;
	public int LastTurnFrame { get; set; }
	public bool WalkingRight { get; set; }
	bool IPingPongWalker.OnlyWalkWhenGrounded => !IsRolling;
	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	protected bool IsRolling => RollingStartFrame >= 0;
	private bool IsInShell = false;
	private int LastDamageFrame = int.MinValue;
	private int RollingStartFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsInShell = false;
		LastDamageFrame = int.MinValue;
		RollingStartFrame = int.MinValue;
		IPingPongWalker.OnActive(this);
	}

	public override void Update () {
		base.Update();

		if (!IsInShell) RollingStartFrame = int.MinValue;

		// Walk
		IPingPongWalker.PingPongWalk(this);

		// Rolling
		if (IsRolling) {
			// Damage Enemy
			IDamageReceiver.DamageAllOverlap(
				Rect, new Damage(1, Const.TEAM_ENEMY, type: Tag.MagicalDamage), PhysicsMask.CHARACTER, this
			);
			// Bump Obj on Side
			if (Game.GlobalFrame == LastTurnFrame) {
				IBumpable.BumpAllOverlap(
					this, WalkingRight ? Direction4.Left : Direction4.Right,
					forceBump: true,
					damageToBumpedObject: new Damage(1, Const.TEAM_ALL, type: Tag.MagicalDamage)
				);
			}
		}

		// Player Kick Check
		if (IsInShell && !IsRolling && Game.GlobalFrame > LastPlayerStepOnFrame + 12) {
			var player = PlayerSystem.Selecting;
			if (player != null && player.Rect.Overlaps(Rect)) {
				RollingStartFrame = Game.GlobalFrame;
				bool toRight = player.Rect.CenterX() < X + Width / 2;
				(this as IPingPongWalker).WalkingRight = toRight;
				MomentumX = (toRight ? 32 : -32, 8);
			}
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
		// Kill by MagicalDamage
		if (damage.Type.HasAll(Tag.MagicalDamage)) {
			MakePassout(RollingSprite);
			return;
		}
		IsInShell = true;
		RollingStartFrame = IsRolling ? int.MinValue : Game.GlobalFrame;
	}

	protected override void OnPlayerStepOn (Character player) {
		player.VelocityY = 64;
		MarioUtil.PlayMarioAudio(Sound.StepOnEnemy, XY);
		if (IsInShell) {
			if (IsRolling) {
				RollingStartFrame = int.MinValue;
			} else {
				RollingStartFrame = Game.GlobalFrame;
				(this as IPingPongWalker).WalkingRight = player.Rect.CenterX() < X + Width / 2;
			}
		} else {
			IsInShell = true;
			RollingStartFrame = int.MinValue;
		}
	}


}
