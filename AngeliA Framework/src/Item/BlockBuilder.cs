using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
[NoItemCombination]
public sealed class BlockBuilder : HandTool {


	// VAR
	public int BlockID { get; init; }
	public BlockType BlockType { get; init; }
	public override bool AvailableWhenSquatting => true;
	public override bool AvailableWhenWalking => true;
	public override bool AvailableWhenSliding => true;
	public override bool AvailableWhenClimbing => true;
	public override int? DefaultMovementSpeedRateOnUse => 618;
	public override int? RunningMovementSpeedRateOnUse => 618;
	public override int? WalkingMovementSpeedRateOnUse => 618;
	public override int MaxStackCount => _MaxStackCount;
	public override int Duration => 12;
	private int _MaxStackCount { get; init; }
	

	// MSG
	public BlockBuilder (int blockID, string blockName, BlockType blockType, int maxStackCount) {
		BlockID = blockID;
		BlockType = blockType;
		SpriteID = blockID;
		TypeName = blockName;
		_MaxStackCount = maxStackCount;
	}


	public override void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) {
		base.OnPoseAnimationUpdate_FromEquipment(rendering);

		var pHolder = rendering.TargetCharacter;
		if (pHolder == PlayerSystem.Selecting) {
			PlayerSystem.IgnoreAction(1);
		}

		if (
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			PlayerMenuUI.ShowingUI ||
			TaskSystem.HasTask()
		) return;

		int targetUnitX, targetUnitY;
		bool available;

		// Get Target Pos
		if (Game.IsMouseAvailable) {
			Cursor.RequireCursor();
			available = FrameworkUtil.GetAimingBuilderPositionFromMouse(
				pHolder, BlockType, out targetUnitX, out targetUnitY, out _
			);
		} else {
			if (!pHolder.IsInsideGround) {
				pHolder.Movement.SquatMoveSpeed.Override(0, 1, priority: 4096);
				pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
			}
			available = FrameworkUtil.GetAimingBuilderPositionFromKey(
				pHolder, BlockType, out targetUnitX, out targetUnitY, out _
			);
		}

		// Ignore Attack
		if (!available) {
			pHolder.Attackness.IgnoreAttack(1);
		}

		// Target Block Highlight
		if (!pHolder.Attackness.IsAttackIgnored) {
			DrawTargetHighlight(targetUnitX, targetUnitY, available);
		}

	}


	public override void OnToolPerform (Character sender) {

		if (
			sender != PlayerSystem.Selecting ||
			!sender.IsAttackAllowedByMovement() ||
			sender.Attackness.IsAttackIgnored ||
			sender.CharacterState != CharacterState.GamePlay ||
			TaskSystem.HasTask()
		) return;

		int targetUnitX, targetUnitY;
		bool available, requireEmbedAsElement;

		// Get Target Pos
		if (Game.IsMouseAvailable) {
			Cursor.RequireCursor();
			available = FrameworkUtil.GetAimingBuilderPositionFromMouse(
				sender, BlockType, out targetUnitX, out targetUnitY, out requireEmbedAsElement
			);
		} else {
			if (!sender.IsInsideGround) {
				sender.Movement.SquatMoveSpeed.Override(0, 1, priority: 4096);
				sender.Movement.WalkSpeed.Override(0, 1, priority: 4096);
			}
			available = FrameworkUtil.GetAimingBuilderPositionFromKey(
				sender, BlockType, out targetUnitX, out targetUnitY, out requireEmbedAsElement
			);
		}

		// Put Block
		if (available) {
			bool success = FrameworkUtil.PutBlockTo(
				BlockID,
				requireEmbedAsElement ? BlockType.Element : BlockType,
				targetUnitX, targetUnitY
			);
			if (success) {
				if (!requireEmbedAsElement) {
					// Reduce Block Count by 1
					int eqID = Inventory.GetEquipment(sender.InventoryID, EquipmentType.HandTool, out int eqCount);
					if (eqID != 0) {
						int newEqCount = (eqCount - 1).GreaterOrEquelThanZero();
						if (newEqCount == 0) eqID = 0;
						Inventory.SetEquipment(sender.InventoryID, EquipmentType.HandTool, eqID, newEqCount);
					}
				}
			} else {
				// Cancel Attack
				sender.Attackness.CancelAttack();
			}
		} else {
			sender.Attackness.CancelAttack();
		}

	}


	public override void DrawItem (IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(BlockID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z: z);
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, tint, z: z);
		}
	}

	public override int GetPerformPoseAnimationID (Character character) => PosePerform_Block.TYPE_ID;
	
	public override int GetHandheldPoseAnimationID (Character character) => PoseHandheld_Block.TYPE_ID;

	// LGC
	private void DrawTargetHighlight (int unitX, int unitY, bool allowPut) {
		using var _ = new UILayerScope();
		// Frame
		int border = GUI.Unify(2);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			border, border, border, border,
			allowPut ? Color32.GREY_230 : Color32.WHITE_96, z: int.MaxValue
		);
		// Block
		if (allowPut && (
			Renderer.TryGetSprite(BlockID, out var sp) ||
			Renderer.TryGetSpriteFromGroup(BlockID, 0, out sp)
		)) {
			const int GAP = Const.CEL / 10;
			Renderer.Draw(
				sp,
				new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL).Shrink(GAP).Fit(sp),
				allowPut ? Color32.GREY_230 : Color32.WHITE_46, z: int.MaxValue
			);
		}
	}


}

