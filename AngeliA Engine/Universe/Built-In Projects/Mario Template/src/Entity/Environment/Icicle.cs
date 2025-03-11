using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class Icicle : Rigidbody {

	// VAR
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int AirDragX => 0;
	public override int AirDragY => 0;
	private bool Falling;
	private int FallAccumulationFrame;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Falling = false;
		FallAccumulationFrame = 0;
		Width = Const.CEL;
		Height = Const.CEL * 2;
		if (FromWorld) {
			Y -= Const.CEL;
		}
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		FillAsTrigger(1);
		if (!Falling) {
			IgnorePhysics.True(1);
			IgnoreGravity.True(1);
			VelocityX = 0;
			VelocityY = 0;
			MomentumX.value = 0;
			MomentumY.value = 0;
		} else if (IsGrounded) {
			// Touch Ground to Break
			BreakingParticle.SpawnParticles(TypeID, Rect);
			Active = false;
		}
	}

	public override void Update () {
		base.Update();

		// Movement
		if (!Falling) {
			if (IsPlayerNearby()) {
				FallAccumulationFrame++;
				if (FallAccumulationFrame > 60) {
					Falling = true;
				}
			} else {
				FallAccumulationFrame = 0;
			}
		}

		// Damage Check
		var player = PlayerSystem.Selecting;
		if (player != null) {
			var damageRect = Rect.Shrink(Const.QUARTER, Const.QUARTER, -16, 0);
			if (player.Rect.Overlaps(damageRect)) {
				(player as IDamageReceiver).TakeDamage(new Damage(1));
				Active = false;
				BreakingParticle.SpawnParticles(TypeID, Rect);
			}
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		int rot = !Falling && IsPlayerNearby() ? (Game.GlobalFrame.PingPong(12) - 6) : 0;
		Renderer.Draw(TypeID, X + Width / 2, Y + Height, 500, 1000, rot, Width, Height);
	}

	// LGC
	private bool IsPlayerNearby () {
		var player = PlayerSystem.Selecting;
		var checkingRect = Rect.CornerInside(Alignment.TopMid, Const.CEL * 2, Const.CEL * 8);
		return player != null && player.Rect.Overlaps(checkingRect);
	}

}
