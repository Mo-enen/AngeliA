using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class Goombrat : Goomba, IPingPongWalker {
	private static readonly SpriteCode WALK_SP = "Goombrat.Walk";
	private static readonly SpriteCode JUMPD_SP = "Goombrat.JumpD";
	private static readonly SpriteCode JUMPU_SP = "Goombrat.JumpU";
	private static readonly SpriteCode PASSOUT_SP = "Goombrat.Passout";
	protected override SpriteCode WalkSprite => WALK_SP;
	protected override SpriteCode JumpDownSprite => JUMPD_SP;
	protected override SpriteCode JumpUpSprite => JUMPU_SP;
	protected override SpriteCode PassoutSprite => PASSOUT_SP;
	bool IPingPongWalker.WalkOffEdge => false;
}


public class Goomba : Enemy, IPingPongWalker {

	// VAR
	private static readonly SpriteCode WALK_SP = "Goomba.Walk";
	private static readonly SpriteCode JUMPD_SP = "Goomba.JumpD";
	private static readonly SpriteCode JUMPU_SP = "Goomba.JumpU";
	private static readonly SpriteCode PASSOUT_SP = "Goomba.Passout";
	protected virtual SpriteCode WalkSprite => WALK_SP;
	protected virtual SpriteCode JumpDownSprite => JUMPD_SP;
	protected virtual SpriteCode JumpUpSprite => JUMPU_SP;
	protected virtual SpriteCode PassoutSprite => PASSOUT_SP;
	int IPingPongWalker.WalkSpeed => IsPassout ? 0 : 8;
	bool IPingPongWalker.WalkOffEdge => true;
	int IPingPongWalker.LastTurnFrame { get; set; }
	bool IPingPongWalker.WalkingRight { get; set; }
	int IPingPongWalker.TurningCheckMask => PhysicsMask.SOLID;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IPingPongWalker.OnActive(this);
		Width = 196;
		Height = 255;
	}

	public override void Update () {
		base.Update();
		if (!Active) return;
		IPingPongWalker.PingPongWalk(this);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		var sprite = IsPassout ? PassoutSprite : IsGrounded ? WalkSprite : VelocityY > 0 ? JumpUpSprite : JumpDownSprite;
		Renderer.Draw(sprite, X + Width / 2, Y, 500, 0, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
	}

}
