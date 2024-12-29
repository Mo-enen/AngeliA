using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public sealed class BlockBuilder : HandTool {


	// VAR
	const int MOUSE_RANGE = 128;
	public int BlockID { get; init; }
	public BlockType BlockType { get; init; }
	public override ToolType ToolType => ToolType.Block;
	public override ToolHandheld Handheld => ToolHandheld.SingleHanded;
	public override bool AvailableWhenSquatting => true;
	public override bool AvailableWhenWalking => true;
	public override bool AvailableWhenSliding => true;
	public override bool AvailableWhenClimbing => true;
	public override int? DefaultMovementSpeedRateOnUse => 618;
	public override int? RunningMovementSpeedRateOnUse => 618;
	public override int? WalkingMovementSpeedRateOnUse => 618;
	public override int MaxStackCount => 256;
	public override int BulletDelayRate => 0;
	public override int Duration => 12;


	// MSG
	public BlockBuilder (int blockID, string blockName, BlockType blockType) {
		BlockID = blockID;
		BlockType = blockType;
		SpriteID = blockID;
		TypeName = blockName;
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
		bool available, inRange = true;

		// Get Target Pos
		if (Game.IsMouseAvailable) {
			Cursor.RequireCursor();
			available = FrameworkUtil.GetAimingBuilderPositionFromMouse(
				pHolder, MOUSE_RANGE, BlockType, out targetUnitX, out targetUnitY, out inRange
			);
		} else {
			if (!pHolder.IsInsideGround) {
				pHolder.Movement.SquatMoveSpeed.Override(0, 1, priority: 4096);
				pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
			}
			available = FrameworkUtil.GetAimingBuilderPositionFromKey(
				pHolder, BlockType, out targetUnitX, out targetUnitY
			);
		}

		// Ignore Attack
		if (!available) {
			pHolder.Attackness.IgnoreAttack(1);
		}

		// Target Block Highlight
		if (inRange && !pHolder.Attackness.IsAttackIgnored) {
			DrawTargetHighlight(targetUnitX, targetUnitY, available);
		}

	}


	public override Bullet SpawnBullet (Character sender) {

		var pHolder = sender;
		if (
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.Attackness.IsAttackIgnored ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			TaskSystem.HasTask()
		) return null;

		if (pHolder == PlayerSystem.Selecting) {
			int targetUnitX, targetUnitY;
			bool available;

			// Get Target Pos
			if (Game.IsMouseAvailable) {
				Cursor.RequireCursor();
				available = FrameworkUtil.GetAimingBuilderPositionFromMouse(
					pHolder, MOUSE_RANGE, BlockType, out targetUnitX, out targetUnitY, out _
				);
			} else {
				if (!pHolder.IsInsideGround) {
					pHolder.Movement.SquatMoveSpeed.Override(0, 1, priority: 4096);
					pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
				}
				available = FrameworkUtil.GetAimingBuilderPositionFromKey(
					pHolder, BlockType, out targetUnitX, out targetUnitY
				);
			}

			// Put Block
			if (available) {
				bool success = FrameworkUtil.PutBlockTo(BlockID, BlockType, pHolder, targetUnitX, targetUnitY);
				if (!success) {
					pHolder.Attackness.CancelAttack();
				}
			} else {
				pHolder.Attackness.CancelAttack();
			}

		} else {
			// For NPC
			if (GetTargetUnitPosFromAI(pHolder, out int targetUnitX, out int targetUnitY)) {
				// Put Block
				FrameworkUtil.PutBlockTo(BlockID, BlockType, pHolder, targetUnitX, targetUnitY);
			}
		}

		return null;
	}


	public override void DrawItem (IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(BlockID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z: z);
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, tint, z: z);
		}
	}


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


	private bool GetTargetUnitPosFromAI (Character holder, out int targetUnitX, out int targetUnitY) {

		targetUnitX = holder.X.ToUnit();
		targetUnitY = holder.Y.ToUnit();

		return false;
	}


}
