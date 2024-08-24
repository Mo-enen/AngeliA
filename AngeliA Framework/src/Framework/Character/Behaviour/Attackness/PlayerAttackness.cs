using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PlayerAttackness : CharacterAttackness {
	public override bool IsChargingAttack =>
		MinimalChargeAttackDuration != int.MaxValue &&
		Player.Selecting == TargetCharacter &&
		Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown + MinimalChargeAttackDuration &&
		!TaskSystem.HasTask() &&
		!Player.Selecting.LockingInput &&
		Input.GameKeyHolding(Gamekey.Action) &&
		TargetCharacter.IsAttackAllowedByMovement() &&
		TargetCharacter.IsAttackAllowedByEquipment();
	public override Direction8 AimingDirection => _AimingDirection;
	public Direction8 _AimingDirection = Direction8.Right;
	public PlayerAttackness (Character character) : base(character) { }
}
