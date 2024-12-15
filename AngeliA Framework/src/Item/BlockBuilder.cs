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
	public override bool AttackWhenSquatting => true;
	public override bool AttackWhenWalking => true;
	public override bool AttackWhenSliding => true;
	public override bool AttackWhenClimbing => true;
	public override int? DefaultSpeedRateOnAttack => 618;
	public override int? RunningSpeedRateOnAttack => 618;
	public override int? WalkingSpeedRateOnAttack => 618;
	public override int MaxStackCount => 256;
	public override int BulletDelayRate => 250;
	public override int AttackDuration => 16;


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

		if (
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.Attackness.IsAttackIgnored ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			PlayerMenuUI.ShowingUI ||
			TaskSystem.HasTask() ||
			pHolder != PlayerSystem.Selecting
		) return;

		int targetUnitX, targetUnitY;
		bool available, inRange = true;

		// For Player
		PlayerSystem.IgnoreAction(1);

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

		// Target Block Highlight
		if (inRange) {
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
					pHolder, MOUSE_RANGE, BlockType, out targetUnitX, out targetUnitY, out bool inRange
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
				allowPut ? Color32.GREY_230 : Color32.WHITE_96, z: int.MaxValue
			);
		}
	}


	private bool IsBlockEmptyAt (int unitX, int unitY) {
		switch (BlockType) {
			case BlockType.Entity:
				// Check for Block Entity
				var hits = Physics.OverlapAll(
					PhysicsMask.MAP,
					new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
					out int count, null, OperationMode.ColliderAndTrigger
				);
				for (int i = 0; i < count; i++) {
					if (hits[i].Entity is IBlockEntity) return false;
				}
				return true;

			case BlockType.Level:
				// Check for Level Block
				return WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level) == 0;

			case BlockType.Background:
				// Check for BG Block
				return WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background) == 0;

		}
		return false;
	}


	private bool GetTargetUnitPosFromAI (Character holder, out int targetUnitX, out int targetUnitY) {

		// TODO

		targetUnitX = holder.X.ToUnit();
		targetUnitY = holder.Y.ToUnit();
		return IsBlockEmptyAt(targetUnitX, targetUnitY);
	}


}
