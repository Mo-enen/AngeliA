using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class Icicle : Rigidbody, IAutoTrackWalker {

	// VAR
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int AirDragX => 0;
	public override int AirDragY => 0;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
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

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillBlock(PhysicsLayer.ENVIRONMENT, TypeID, Rect.EdgeInsideUp(Const.CEL), true, Tag.OnewayUp);
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

		// Damage Check for Player
		var player = PlayerSystem.Selecting;
		if (player != null) {
			var damageRect = Rect.Shrink(Const.QUARTER, Const.QUARTER, -16, 0);
			if (player.Rect.Overlaps(damageRect)) {
				(player as IDamageReceiver).TakeDamage(new Damage(1));
				Active = false;
				BreakingParticle.SpawnParticles(TypeID, Rect);
			}
		}

		// Damage Check for Enemy/Environment
		if (Falling) {
			IDamageReceiver.DamageAllOverlap(
				Rect.EdgeExact(Direction4.Down, 16),
				new Damage(1, Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT, type: Tag.MagicalDamage),
				PhysicsMask.DYNAMIC, this
			);
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
