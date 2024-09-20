using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class SummonNavigation : CharacterNavigation {


	private const int AIM_REFRESH_FREQUENCY = 60;
	public override bool NavigationEnable => TargetCharacter.CharacterState == CharacterState.GamePlay && Owner != null && Owner.Active;
	public override bool ClampInSpawnRect => Owner == Player.Selecting;
	public bool FollowOwner { get; set; } = true;
	public Entity Owner { get; set; }
	public SummonNavigation (Character character) : base(character) { }
	private bool RequireAimRefresh = true;
	private int StartX;
	private int StartY;


	public override void OnActivated () {
		base.OnActivated();
		StartX = TargetCharacter.X;
		StartY = TargetCharacter.Y;
	}

	protected override Int2? GetNavigationAim (out bool grounded) {

		// Get Aim at Ground
		var result = Owner != null && Owner.Active && FollowOwner ?
			new Int2(Owner.X, Owner.Y) :
			new Int2(StartX, StartY);
		grounded = false;

		// Scan Frequency Gate
		int insIndex = TargetCharacter.InstanceOrder;
		if (
			!RequireAimRefresh &&
			(Game.GlobalFrame + insIndex) % AIM_REFRESH_FREQUENCY != 0 &&
			HasPerformableOperation
		) return null;
		RequireAimRefresh = false;

		// Freedom Shift
		const int SHIFT_AMOUNT = Const.CEL * 10;
		int freeShiftX = Util.QuickRandomWithSeed(
			TargetCharacter.TypeID + (insIndex + (Game.GlobalFrame / 300)) * TargetCharacter.TypeID
		) % SHIFT_AMOUNT;

		// Find Available Ground
		int offsetX = freeShiftX + Const.CEL * ((insIndex % 12) / 2 + 2) * (insIndex % 2 == 0 ? -1 : 1);
		int offsetY = NavigationState == CharacterNavigationState.Fly ? Const.CEL : Const.HALF;
		if (Navigation.ExpandTo(
			Game.GlobalFrame, Stage.ViewRect,
			result.x, result.y,
			result.x + offsetX, result.y + offsetY,
			maxIteration: 12,
			out int groundX, out int groundY
		)) {
			result.x = groundX;
			result.y = groundY;
			grounded = true;
		} else {
			result.x += offsetX;
			grounded = false;
		}

		// Instance Shift
		result = new Int2(
			result.x + (insIndex % 2 == 0 ? 8 : -8) * (insIndex / 2),
			result.y
		);

		return result;
	}


	public void Refresh () => RequireAimRefresh = true;


}
