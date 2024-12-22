using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class SummonNavigation (Character character) : CharacterNavigation(character) {

	private enum AimMode { FollowOwner, Wandering, }

	// VAR
	private const int AIM_REFRESH_FREQUENCY = 60;
	public override bool NavigationEnable => TargetCharacter.CharacterState == CharacterState.GamePlay;
	public override bool ClampInSpawnRect => Owner == PlayerSystem.Selecting;
	public Entity Owner { get; set; }
	public bool IsFollowingOwner => CurrentAmiMode == AimMode.FollowOwner;
	public bool IsWandering => CurrentAmiMode == AimMode.Wandering;

	private AimMode CurrentAmiMode { get; set; } = AimMode.FollowOwner;
	private Int2 CurrentWanderingPos;
	private bool RequireAimRefresh = true;
	private int StartX;
	private int StartY;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		StartX = TargetCharacter.X;
		StartY = TargetCharacter.Y;
		CurrentWanderingPos.x = StartX;
		CurrentWanderingPos.y = StartY;
	}

	public override void PhysicsUpdate () {
		base.PhysicsUpdate();
		UpdateNavigationAim();
	}

	private void UpdateNavigationAim () {

		// Scan Frequency Gate
		int insIndex = TargetCharacter.InstanceOrder;
		if (
			!RequireAimRefresh &&
			(Game.GlobalFrame + insIndex) % AIM_REFRESH_FREQUENCY != 0 &&
			HasPerformableOperation
		) return;
		RequireAimRefresh = false;

		// Get Aim at Ground
		var aimPosition = new Int2(StartX, StartY);
		switch (CurrentAmiMode) {
			case AimMode.FollowOwner:
				if (Owner == null || !Owner.Active) break;
				aimPosition = new Int2(Owner.X, Owner.Y);
				break;
			case AimMode.Wandering:
				aimPosition = CurrentWanderingPos;
				break;
		}
		NavigationAimGrounded = false;

		// Freedom Shift
		const int SHIFT_AMOUNT = Const.CEL * 10;
		const int SHIFT_FREQ = 15 * 60;
		int freeShiftX = Util.QuickRandomWithSeed(
			TargetCharacter.TypeID + (insIndex + (Game.GlobalFrame / SHIFT_FREQ)) * TargetCharacter.TypeID
		) % SHIFT_AMOUNT;

		// Find Available Ground
		int offsetX = freeShiftX + Const.CEL * ((insIndex % 12) / 2 + 2) * (insIndex % 2 == 0 ? -1 : 1);
		int offsetY = NavigationState == CharacterNavigationState.Fly ? Const.CEL : Const.HALF;
		if (Navigation.ExpandTo(
			Game.GlobalFrame, Stage.ViewRect,
			aimPosition.x, aimPosition.y,
			aimPosition.x + offsetX, aimPosition.y + offsetY,
			maxIteration: 12,
			out int groundX, out int groundY
		)) {
			aimPosition.x = groundX;
			aimPosition.y = groundY;
			NavigationAimGrounded = true;
		} else {
			aimPosition.x += offsetX;
			NavigationAimGrounded = false;
		}

		// Instance Shift
		aimPosition = new Int2(
			aimPosition.x + (insIndex % 2 == 0 ? 8 : -8) * (insIndex / 2),
			aimPosition.y
		);

		NavigationAim = aimPosition;
	}

	// API
	public void Refresh () => RequireAimRefresh = true;

	public void MakeFollowOwner () => CurrentAmiMode = AimMode.FollowOwner;

	public bool MakeWander<E> () where E : Entity {
		if (Stage.TryGetEntityNearby(new Int2(TargetCharacter.X, TargetCharacter.Y), out E result)) {
			MakeWander(result.X, result.Y);
			return true;
		}
		return false;
	}

	public void MakeWander (int x, int y) {
		CurrentAmiMode = AimMode.Wandering;
		CurrentWanderingPos.x = x;
		CurrentWanderingPos.y = y;
	}

}
