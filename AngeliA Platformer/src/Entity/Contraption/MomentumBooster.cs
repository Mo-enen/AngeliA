using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class MomentumBooster : Entity, IBlockEntity {

	// VAR
	protected abstract int BoostSpeed { get; }
	protected abstract Direction3 BoostDirection { get; }
	protected int LastBoostFrame { get; private set; } = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		LastBoostFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Check for Boost
		var hits = Physics.OverlapAll(PhysicsMask.ENTITY, Rect.EdgeOutsideUp(1), out int count, this);
		for (int i = 0; i < count; i++) {

			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			var wMov = rig as IWithCharacterMovement;

			// Get Boost Direction
			int sign = 1;
			if (BoostDirection == Direction3.None) {
				if (wMov != null) {
					sign = wMov.CurrentMovement.FacingRight ? 1 : -1;
				}
			} else {
				sign = BoostDirection == Direction3.Right ? 1 : -1;
			}

			// Boost
			int momentumX = rig.CurrentMomentumX.Abs();
			rig.SetMomentum(
				sign * BoostSpeed.ReverseClamp(-momentumX, momentumX),
				rig.CurrentMomentumY, 1, 1
			);

			// Movement
			if (wMov != null) {
				var mov = wMov.CurrentMovement;
				mov.SquatAvailable.False(1);
				mov.DashAvailable.False(1);
				mov.RushAvailable.False(1);
				mov.RunAvailable.False(1);
				mov.WalkAvailable.False(1);
				mov.RunSpeed.Override(BoostSpeed + 16, 1);
				mov.PushAvailable.True(1);
				mov.PushSpeed.Override(BoostSpeed, 1);
			}

			// Animation
			if (rig is IWithCharacterRenderer wRen) {
				wRen.CurrentRenderer.TargetCharacter.LockAnimationType(CharacterAnimationType.Run);
			}

			// Footstep
			if (Game.GlobalFrame % 4 == 0) {
				FrameworkUtil.InvokeOnFootStepped(
					rig.X + Util.QuickRandom(-32, 33),
					rig.Y + Util.QuickRandom(-32, 33),
					TypeID
				);
			}

			// Final
			OnBoosting(rig);
			LastBoostFrame = Game.GlobalFrame;
		}
	}

	protected virtual void OnBoosting (Rigidbody rig) { }

}
