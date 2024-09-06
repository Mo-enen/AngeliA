using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public sealed class BlockBuilder : Weapon {


	// VAR
	public int BlockID { get; init; }
	public BlockType BlockType { get; init; }
	public override WeaponType WeaponType => WeaponType.Block;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
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
			holder is not PoseCharacter pHolder ||
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			PlayerMenuUI.ShowingUI ||
			TaskSystem.HasTask() ||
			WorldSquad.DontSaveChangesToFile
		) goto _BASE_;

		// Movement Override
		if (!pHolder.IsInsideGround) {
			pHolder.Movement.SquatSpeed.Override(0, 1, priority: 4096);
			pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
		}

		// Get Target Pos
		GetTargetUnitPos(pHolder, out int targetUnitX, out int targetUnitY, out bool isTargetEmpty);

		// Target Block Highlight
		if (!PlayerMenuUI.ShowingUI) {
			DrawTargetHighlight(targetUnitX, targetUnitY, isTargetEmpty);
		}

		// Put Block
		if (isTargetEmpty && Game.GlobalFrame == pHolder.Attackness.LastAttackFrame) {
			FrameworkUtil.PutBlockTo(BlockID, BlockType, pHolder, targetUnitX, targetUnitY);
		}

		// Base
		_BASE_:;
		base.PoseAnimationUpdate_FromEquipment(holder);
	}


	public override Bullet SpawnBullet (Character sender) => null;


	// LGC
	private void DrawTargetHighlight (int unitX, int unitY, bool allowPut) {
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


	private bool IsEmptyAt (int unitX, int unitY) {
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


	private void GetTargetUnitPos (Character pHolder, out int targetUnitX, out int targetUnitY, out bool isTargetEmpty) {

		var aim = pHolder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		if (!pHolder.Movement.IsClimbing) {
			// Normal
			int pointX = pHolder.Rect.CenterX();
			int pointY = aim.IsTop() ? pHolder.Rect.yMax - Const.HALF / 2 : pHolder.Rect.y + Const.HALF;
			targetUnitX = pointX.ToUnit() + aimNormal.x;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		} else {
			// Climbing
			int pointX = pHolder.Rect.CenterX();
			int pointY = pHolder.Rect.yMax - Const.HALF / 2;
			targetUnitX = pHolder.Movement.FacingRight ? pointX.ToUnit() + 1 : pointX.ToUnit() - 1;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		}

		isTargetEmpty = IsEmptyAt(targetUnitX, targetUnitY);

		// Redirect
		if (!isTargetEmpty) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += pHolder.Movement.FacingRight ? 1 : -1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += pHolder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY++;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				isTargetEmpty = IsEmptyAt(targetUnitX, targetUnitY);
			}
		}

	}


}
