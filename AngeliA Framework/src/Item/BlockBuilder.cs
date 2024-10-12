using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public sealed class BlockBuilder : HandTool {


	// VAR
	const int MOUSE_RANGE = 6;
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


	// MSG
	public BlockBuilder (int blockID, string blockName, BlockType blockType) {
		BlockID = blockID;
		BlockType = blockType;
		SpriteID = blockID;
		TypeName = blockName;
	}


	public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

		if (
			holder is not Character pHolder ||
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			PlayerMenuUI.ShowingUI ||
			TaskSystem.HasTask() ||
			WorldSquad.DontSaveChangesToFile
		) {
			base.PoseAnimationUpdate_FromEquipment(holder);
			return;
		}

		if (pHolder == PlayerSystem.Selecting) {
			int targetUnitX, targetUnitY;
			bool available, inRange = true;
			// For Player
			PlayerSystem.IgnoreAction(1);
			// Get Target Pos
			if (Game.IsMouseAvailable) {
				Cursor.RequireCursor();
				available = GetTargetUnitPositionFromMouse(pHolder, out targetUnitX, out targetUnitY, out inRange);
			} else {
				if (!pHolder.IsInsideGround) {
					pHolder.Movement.SquatMoveSpeed.Override(0, 1, priority: 4096);
					pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
				}
				available = GetTargetUnitPosFromKey(pHolder, out targetUnitX, out targetUnitY);
			}
			// Target Block Highlight
			if (inRange && !PlayerMenuUI.ShowingUI) {
				DrawTargetHighlight(targetUnitX, targetUnitY, available);
			}
			// Put Block
			if (available && Game.GlobalFrame == pHolder.Attackness.LastAttackFrame) {
				FrameworkUtil.PutBlockTo(BlockID, BlockType, pHolder, targetUnitX, targetUnitY);
			}
		} else {
			// For NPC
			if (
				Game.GlobalFrame == pHolder.Attackness.LastAttackFrame &&
				GetTargetUnitPosFromAI(pHolder, out int targetUnitX, out int targetUnitY)
			) {
				// Put Block
				FrameworkUtil.PutBlockTo(BlockID, BlockType, pHolder, targetUnitX, targetUnitY);
			}
		}

	}


	public override Bullet SpawnBullet (Character sender) => null;


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
			Renderer.TryGetSprite(BlockID, out var sp, true) ||
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


	private bool GetTargetUnitPositionFromMouse (Character holder, out int targetUnitX, out int targetUnitY, out bool inRange) {

		var mouseUnitPos = Input.MouseGlobalPosition.ToUnit();
		targetUnitX = mouseUnitPos.x;
		targetUnitY = mouseUnitPos.y;

		// Range Check
		int holderUnitX = holder.Rect.CenterX().ToUnit();
		int holderUnitY = (holder.Rect.y + Const.HALF).ToUnit();
		if (
			!targetUnitX.InRangeInclude(holderUnitX - MOUSE_RANGE, holderUnitX + MOUSE_RANGE) ||
			!targetUnitY.InRangeInclude(holderUnitY - MOUSE_RANGE, holderUnitY + MOUSE_RANGE)
		) {
			inRange = false;
			return false;
		}
		inRange = true;

		// Overlap with Holder Check
		var mouseRect = new IRect(targetUnitX.ToGlobal(), targetUnitY.ToGlobal(), Const.CEL, Const.CEL);
		if (holder.Rect.Overlaps(mouseRect)) {
			return false;
		}

		// Overlap with Entity Check
		if (
			BlockType == BlockType.Entity &&
			Physics.Overlap(PhysicsMask.ENTITY, mouseRect, null, OperationMode.ColliderAndTrigger
		)) {
			return false;
		}

		// Block Empty Check
		return IsBlockEmptyAt(targetUnitX, targetUnitY);

	}


	private bool GetTargetUnitPosFromKey (Character holder, out int targetUnitX, out int targetUnitY) {

		bool result;
		var aim = holder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		if (!holder.Movement.IsClimbing) {
			// Normal
			int pointX = holder.Rect.CenterX();
			int pointY = aim.IsTop() ? holder.Rect.yMax - Const.HALF / 2 : holder.Rect.y + Const.HALF;
			targetUnitX = pointX.ToUnit() + aimNormal.x;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		} else {
			// Climbing
			int pointX = holder.Rect.CenterX();
			int pointY = holder.Rect.yMax - Const.HALF / 2;
			targetUnitX = holder.Movement.FacingRight ? pointX.ToUnit() + 1 : pointX.ToUnit() - 1;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		}

		result = IsBlockEmptyAt(targetUnitX, targetUnitY);

		// Redirect
		if (!result) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += holder.Movement.FacingRight ? 1 : -1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += holder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY++;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				result = IsBlockEmptyAt(targetUnitX, targetUnitY);
			}
		}

		return result;
	}


	private bool GetTargetUnitPosFromAI (Character holder, out int targetUnitX, out int targetUnitY) {

		// TODO

		targetUnitX = holder.X.ToUnit();
		targetUnitY = holder.Y.ToUnit();
		return IsBlockEmptyAt(targetUnitX, targetUnitY);
	}


}
