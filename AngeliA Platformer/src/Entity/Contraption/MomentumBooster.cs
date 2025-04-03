using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that force character on top run in given direction or their current facing direction
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class MomentumBooster : Entity, IBlockEntity {

	// VAR
	/// <summary>
	/// Target moving speed
	/// </summary>
	protected abstract int BoostSpeed { get; }
	/// <summary>
	/// Target moving direction. Set to None to make target run to their facing direction
	/// </summary>
	protected abstract Direction3 BoostDirection { get; }
	/// <summary>
	/// Decay value for target momentum
	/// </summary>
	protected virtual int MomentumDecay => 1;
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
		var hits = Physics.OverlapAll(
			PhysicsMask.DYNAMIC, Rect.EdgeOutsideUp(1), out int count,
			this, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {

			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig || rig.IgnorePhysics) continue;

			// Get Boost Direction
			int sign = BoostDirection == Direction3.None ?
				rig.FacingRight ? 1 : -1 :
				BoostDirection == Direction3.Right ? 1 : -1;

			// Boost
			int momentumX = rig.MomentumX.value.Abs();
			rig.MomentumX.value = sign * BoostSpeed.ReverseClamp(-momentumX, momentumX);
			rig.MomentumX.decay = MomentumDecay;

			// Movement
			if (rig is IWithCharacterMovement wMov) {
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
				FrameworkUtil.InvokeOnFootStepped(rig);
			}

			// Final
			OnBoosting(rig);
			LastBoostFrame = Game.GlobalFrame;
		}
	}

	/// <summary>
	/// This function is called when target is getting boost
	/// </summary>
	protected virtual void OnBoosting (Rigidbody rig) { }

}
