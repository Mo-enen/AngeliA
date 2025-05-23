﻿using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Behavior to handle auto movement for a summon character
/// </summary>
public class SummonNavigation (Summon target) : RigidbodyNavigation(target) {

	private enum AimMode { FollowOwner, Wandering, }

	// VAR
	public override bool NavigationEnable => Target is not Character character || character.CharacterState == CharacterState.GamePlay;
	public override bool ClampInSpawnRect => (Target as Summon).Owner == PlayerSystem.Selecting;
	public override int InstanceShift => 17;
	public override int StartMoveDistance => Const.CEL * 12;
	public override int EndMoveDistance => Const.CEL * 6;
	public override int StartFlyDistance => Const.CEL * 26;
	public override int EndFlyDistance => Const.CEL * 6;
	public override int MinimumFlyDuration => 120;
	public override int JumpSpeed => 42;
	public override int MaxJumpDuration => 60;
	/// <summary>
	/// True if the summon following the owner
	/// </summary>
	public bool IsFollowingOwner => CurrentAmiMode == AimMode.FollowOwner;
	/// <summary>
	/// True if the summon move along a given position
	/// </summary>
	public bool IsWandering => CurrentAmiMode == AimMode.Wandering;

	private AimMode CurrentAmiMode { get; set; } = AimMode.FollowOwner;
	private Int2 CurrentWanderingPos;
	private bool RequireAimRefresh = true;
	private int StartX;
	private int StartY;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		StartX = Target.X;
		StartY = Target.Y;
		CurrentWanderingPos.x = StartX;
		CurrentWanderingPos.y = StartY;
		RequireAimRefresh = true;
	}

	public override void PhysicsUpdate () {
		base.PhysicsUpdate();

		if (!NavigationEnable) return;

		const int AIM_REFRESH_FREQUENCY = 60;

		// Scan Frequency Gate
		int insIndex = Target.InstanceOrder;
		if (
			RequireAimRefresh || !HasPerformableOperation ||
			(Game.GlobalFrame + insIndex) % AIM_REFRESH_FREQUENCY == 0
		) {
			RequireAimRefresh = false;

			// Get Aim at Ground
			var aimPosition = new Int2(StartX, StartY);
			var owner = (Target as Summon).Owner;
			switch (CurrentAmiMode) {
				case AimMode.FollowOwner:
					if (owner == null || !owner.Active) break;
					aimPosition = new Int2(owner.X, owner.Y);
					break;
				case AimMode.Wandering:
					aimPosition = CurrentWanderingPos;
					break;
			}
			NavigationAimGrounded = false;

			// Freedom Shift
			NavigationAim = PlatformerUtil.NavigationFreeWandering(
				aimPosition, Target, out bool grounded,
				frequency: 30 * 60,
				maxDistance: Const.CEL * 10
			);
			NavigationAimGrounded = grounded;
		}
	}

	// API
	public void Refresh () => RequireAimRefresh = true;

	public void MakeFollowOwner () => CurrentAmiMode = AimMode.FollowOwner;

	/// <summary>
	/// Make the summon find a given type of entity on stage and move along at it's position
	/// </summary>
	/// <returns>True if the target founded</returns>
	public bool MakeWander<E> () where E : Entity {
		if (Stage.TryFindEntityNearby(new Int2(Target.X, Target.Y), out E result)) {
			MakeWander(result.X, result.Y);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Make the summon move along with given position in global space
	/// </summary>
	public void MakeWander (int x, int y) {
		CurrentAmiMode = AimMode.Wandering;
		CurrentWanderingPos.x = x;
		CurrentWanderingPos.y = y;
	}

}
