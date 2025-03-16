using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class Ghost : Enemy {

	// VAR
	private static readonly SpriteCode SHY_SP = "Ghost.Shy";
	public override int CollisionMask => 0;
	protected override bool AllowPlayerStepOn => false;
	protected override bool AttackOnTouchPlayer => true;
	private bool CurrentFacingRight;
	private Float2 CurrentVelocity;
	private bool IsShy;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		CurrentVelocity = default;
		IsShy = false;
		CurrentFacingRight = true;
	}

	public override void FirstUpdate () => IgnorePhysics.True(1);

	public override void Update () {
		base.Update();

		var player = PlayerSystem.Selecting;
		if (player != null) {

			// Cache
			CurrentFacingRight = player.Center.x > Center.x;
			IsShy = player.FacingRight != CurrentFacingRight;

			// Movement
			var targetVel = IsShy ? Int2.zero : (((Float2)(player.Center - Center)).normalized * 8).RoundToInt();
			CurrentVelocity.x = Util.LerpUnclamped(CurrentVelocity.x, targetVel.x, 0.1f);
			CurrentVelocity.y = Util.LerpUnclamped(CurrentVelocity.y, targetVel.y, 0.1f);
			XY += CurrentVelocity.RoundToInt();

		}

	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(IsShy ? SHY_SP : TypeID, CurrentFacingRight ? Rect : Rect.GetFlipHorizontal());
	}

}
