using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class PickTool : HandTool {


	// VAR
	public sealed override ToolType ToolType => ToolType.Pick;
	public override ToolHandheld Handheld => ToolHandheld.SingleHanded;
	public override bool AttackWhenSquatting => true;
	public override bool AttackWhenWalking => true;
	public override bool AttackWhenSliding => true;
	public override int? DefaultSpeedRateOnAttack => 0;
	public override int? RunningSpeedRateOnAttack => 0;
	public override int? WalkingSpeedRateOnAttack => 0;
	public virtual bool AllowPickLevelBlock => true;
	public virtual bool AllowPickBackgroundBlock => true;
	public virtual bool AllowPickBlockEntity => true;
	public virtual bool DropItemAfterPicked => true;
	public virtual bool UseMouseToPick => false;
	public virtual int MouseUnitRange => 6;
	public override bool UseStackAsUsage => true;
	public override int MaxStackCount => 4096;
	public override int BulletDelayRate => 250;
	public override int AttackDuration => 16;


	// MSG
	public override void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) {
		var pHolder = rendering.TargetCharacter;
		if (
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.Attackness.IsAttackIgnored ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			PlayerMenuUI.ShowingUI ||
			TaskSystem.HasTask()
		) {
			base.OnPoseAnimationUpdate_FromEquipment(rendering);
			return;
		}

		// Override
		if (!pHolder.IsInsideGround) {
			pHolder.Movement.SquatMoveSpeed.Override(0, 1, priority: 4096);
			pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
		}
		if (pHolder == PlayerSystem.Selecting) {
			PlayerSystem.IgnoreAction(1);
		}

		// Get Target Pos
		int targetUnitX, targetUnitY;
		bool hasTraget, inRange = true;
		if (UseMouseToPick && Game.IsMouseAvailable) {
			Cursor.RequireCursor();
			hasTraget = FrameworkUtil.GetAimingPickerPositionFromMouse(
				pHolder, MouseUnitRange, out targetUnitX, out targetUnitY, out inRange,
				AllowPickBlockEntity, AllowPickLevelBlock, AllowPickBackgroundBlock
			);
		} else {
			hasTraget = FrameworkUtil.GetAimingPickerPositionFromKey(
				pHolder, out targetUnitX, out targetUnitY,
				AllowPickBlockEntity, AllowPickLevelBlock, AllowPickBackgroundBlock
			);
		}

		// Target Block Highlight
		if (inRange && !PlayerMenuUI.ShowingUI) {
			DrawPickTargetHighlight(targetUnitX, targetUnitY, hasTraget);
		}

		// Base
		base.OnPoseAnimationUpdate_FromEquipment(rendering);

	}


	protected virtual void DrawPickTargetHighlight (int unitX, int unitY, bool hasTarget) {
		using var _ = new UILayerScope();
		int border = GUI.Unify(2);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			border, border, border, border,
			hasTarget ? Color32.GREY_230 : Color32.WHITE_96, z: int.MaxValue
		);
	}


	public override Bullet SpawnBullet (Character sender) {

		var pHolder = sender;
		if (
			pHolder.CharacterState != CharacterState.GamePlay ||
			TaskSystem.HasTask()
		) return null;

		// Get Target Pos
		int targetUnitX, targetUnitY;
		bool hasTraget, inRange = true;
		if (UseMouseToPick && Game.IsMouseAvailable) {
			Cursor.RequireCursor();
			hasTraget = FrameworkUtil.GetAimingPickerPositionFromMouse(
				pHolder, MouseUnitRange, out targetUnitX, out targetUnitY, out inRange,
				AllowPickBlockEntity, AllowPickLevelBlock, AllowPickBackgroundBlock
			);
		} else {
			hasTraget = FrameworkUtil.GetAimingPickerPositionFromKey(
				pHolder, out targetUnitX, out targetUnitY,
				AllowPickBlockEntity, AllowPickLevelBlock, AllowPickBackgroundBlock
			);
		}

		// Pick Block
		if (hasTraget && inRange) {
			// Erase Block from Map
			bool picked = FrameworkUtil.PickBlockAt(
				targetUnitX, targetUnitY,
				allowPickBlockEntity: AllowPickBlockEntity,
				allowPickLevelBlock: AllowPickLevelBlock,
				allowPickBackgroundBlock: AllowPickBackgroundBlock
			);
			// Reduce Weapon Usage
			if (picked) {
				Inventory.ReduceEquipmentCount(pHolder.InventoryID, 1, EquipmentType.HandTool);
			} else {
				pHolder.Attackness.CancelAttack();
			}
		} else {
			pHolder.Attackness.CancelAttack();
		}

		return null;
	}


}
