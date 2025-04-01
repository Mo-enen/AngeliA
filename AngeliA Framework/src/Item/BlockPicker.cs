using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Item that represent a map tool inside inventory. Also handles the pick block from map logic
/// </summary>
public abstract class BlockPicker : HandTool {


	// VAR
	public override bool AvailableWhenSquatting => true;
	public override bool AvailableWhenWalking => true;
	public override bool AvailableWhenSliding => true;
	public override int? DefaultMovementSpeedRateOnUse => 0;
	public override int? RunningMovementSpeedRateOnUse => 0;
	public override int? WalkingMovementSpeedRateOnUse => 0;
	public override bool UseStackAsUsage => true;
	public override int MaxStackCount => 4096;
	public override int PerformDelayRate => 0;
	public override int Duration => 16;
	/// <summary>
	/// True if the tool can pick level blocks from map
	/// </summary>
	public virtual bool AllowPickLevelBlock => true;
	/// <summary>
	/// True if the tool can pick background blocks from map
	/// </summary>
	public virtual bool AllowPickBackgroundBlock => true;
	/// <summary>
	/// True if the tool can pick entity blocks from map
	/// </summary>
	public virtual bool AllowPickBlockEntity => true;
	/// <summary>
	/// True if the tool create an ItemHolder holds the block after pick the block
	/// </summary>
	public virtual bool DropItemAfterPicked => true;
	/// <summary>
	/// True if the tool allow user to use mouse 
	/// </summary>
	public virtual bool UseMouseToPick => false;
	/// <summary>
	/// Range limitation for mouse in unit space
	/// </summary>
	public virtual int MouseUnitRange => 6;


	// MSG
	public override void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) {

		var pHolder = rendering.TargetCharacter;
		if (pHolder == PlayerSystem.Selecting) {
			PlayerSystem.IgnoreAction(1);
		}

		if (
			!pHolder.IsAttackAllowedByMovement() ||
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

		// Ignore Attack
		if (!hasTraget) {
			pHolder.Attackness.IgnoreAttack(1);
		}

		// Target Block Highlight
		if (inRange && !PlayerMenuUI.ShowingUI && !pHolder.Attackness.IsAttackIgnored) {
			DrawPickTargetHighlight(targetUnitX, targetUnitY, hasTraget);
		}

		// Base
		base.OnPoseAnimationUpdate_FromEquipment(rendering);

	}

	/// <summary>
	/// Draw the UI cursor for target block
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="hasTarget">True if target map block founded</param>
	protected virtual void DrawPickTargetHighlight (int unitX, int unitY, bool hasTarget) {
		using var _ = new UILayerScope();
		int border = GUI.Unify(2);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			border, border, border, border,
			hasTarget ? Color32.GREY_230 : Color32.WHITE_46, z: int.MaxValue
		);
	}

	public override void OnToolPerform (Character sender) {

		var pHolder = sender;
		if (
			pHolder.CharacterState != CharacterState.GamePlay ||
			TaskSystem.HasTask()
		) return;

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

	}

}
