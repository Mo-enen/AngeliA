using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class DeepCheepLeft : CheepCheep { // The Green One
	private static readonly SpriteCode BODY_SP = "DeepCheep";
	protected override SpriteCode BodySprite => BODY_SP;
	protected override bool ChasePlayer => false;
	protected override bool MoveToRight => false;
}

public class DeepCheepRight : CheepCheep { // The Green One
	private static readonly SpriteCode BODY_SP = "DeepCheep";
	protected override SpriteCode BodySprite => BODY_SP;
	protected override bool ChasePlayer => false;
	protected override bool MoveToRight => true;
}

public class CheepCheepLeft : CheepCheep { // The Red One
	private static readonly SpriteCode BODY_SP = "CheepCheep";
	protected override SpriteCode BodySprite => BODY_SP;
	protected override bool ChasePlayer => true;
	protected override bool MoveToRight => false;
}

public class CheepCheepRight : CheepCheep { // The Red One
	private static readonly SpriteCode BODY_SP = "CheepCheep";
	protected override SpriteCode BodySprite => BODY_SP;
	protected override bool ChasePlayer => true;
	protected override bool MoveToRight => true;
}


[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDespawnOutOfRange]
public abstract class CheepCheep : Enemy {

	// VAR
	public override int CollisionMask => 0;
	protected abstract SpriteCode BodySprite { get; }
	protected abstract bool ChasePlayer { get; }
	protected abstract bool MoveToRight { get; }
	protected override bool DelayPassoutOnStep => false;
	protected override bool AllowPlayerStepOn => true;
	protected override bool AttackOnTouchPlayer => true;
	private bool FreeFalling;
	private bool MovingToRight;
	private int JumpCount;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		MovingToRight = MoveToRight;
		JumpCount = 0;
		FreeFalling = !FromWorld;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		FillAsTrigger(1);
		FallingGravityScale.Override(800, 1);
		RisingGravityScale.Override(1200, 1);
	}

	public override void Update () {
		base.Update();

		var viewRect = Stage.ViewRect;

		// Out of Range Check
		if (!X.InRangeInclude(viewRect.xMin, viewRect.xMax)) {
			Active = false;
			return;
		}

		// Movement
		if (!FreeFalling) {
			VelocityX = MovingToRight ? 24 : -24;
		}
		if (Y < viewRect.yMin) {
			FreeFalling = false;
			bool bigJump = JumpCount % 3 == 2;
			VelocityY = bigJump ? 196 : 128;
			JumpCount++;
			if (ChasePlayer && bigJump) {
				var player = PlayerSystem.Selecting;
				MovingToRight = player != null && Rect.CenterX() < player.Rect.CenterX();
			}
		}


	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		Renderer.Draw(BodySprite, MovingToRight ? Rect : Rect.GetFlipHorizontal());
	}

}
