using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public sealed class BlockItem : Weapon {


	// VAR
	private static readonly System.Type BLOCK_ENTITY_TYPE = typeof(IBlockEntity);
	public int BlockID { get; init; }
	public BlockType BlockType { get; init; }
	public override WeaponType WeaponType => WeaponType.Block;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public override bool AttackWhenSquatting => true;
	public override bool AttackWhenWalking => true;
	public override bool AttackWhenSliding => true;
	public override int? DefaultSpeedLoseOnAttack => 618;
	public override int? RunningSpeedLoseOnAttack => 618;
	public override int? WalkingSpeedLoseOnAttack => 618;
	public override int MaxStackCount => 256;


	// MSG
	public BlockItem (int blockID, string blockName, BlockType blockType) {
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
			Task.HasTask() ||
			WorldSquad.Readonly
		) goto _BASE_;

		// Movement Override
		if (!pHolder.IsInsideGround) {
			pHolder.SquatSpeed.Override(0, 1);
			pHolder.WalkSpeed.Override(0, 1);
		}

		// Get Target Pos
		GetTargetUnitPos(pHolder, out int targetUnitX, out int targetUnitY, out bool isTargetEmpty);

		// Target Block Highlight
		if (!PlayerMenuUI.ShowingUI) {
			DrawTargetHighlight(targetUnitX, targetUnitY, isTargetEmpty);
		}

		// Put Block
		if (isTargetEmpty && Game.GlobalFrame == pHolder.LastAttackFrame) {
			PutBlockTo(pHolder, targetUnitX, targetUnitY);
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

		var aim = pHolder.AimingDirection;
		var aimNormal = aim.Normal();
		int pointX = pHolder.Rect.CenterX();
		int pointY = aim.IsTop() ? pHolder.Rect.yMax - Const.HALF / 2 : pHolder.Rect.y + Const.HALF;
		targetUnitX = pointX.ToUnit() + aimNormal.x;
		targetUnitY = pointY.ToUnit() + aimNormal.y;
		isTargetEmpty = IsEmptyAt(targetUnitX, targetUnitY);

		// Redirect
		if (!isTargetEmpty) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += pHolder.FacingRight ? 1 : -1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += pHolder.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY++;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				isTargetEmpty = IsEmptyAt(targetUnitX, targetUnitY);
			}
		}

	}


	private void PutBlockTo (Character pHolder, int targetUnitX, int targetUnitY) {

		// Set Block to Map
		if (
			Renderer.TryGetSprite(BlockID, out var sprite, true) ||
			Renderer.TryGetSpriteFromGroup(BlockID, 0, out sprite)
		) {
			WorldSquad.Front.SetBlockAt(targetUnitX, targetUnitY, BlockType, sprite.ID);
			// Rule
			if (sprite.Group != null && sprite.Group.WithRule) {
				FrameworkUtil.RedirectForRule(
					WorldSquad.Stream, new IRect(targetUnitX - 1, targetUnitY - 1, 3, 3), Stage.ViewZ
				);
			}
		} else {
			WorldSquad.Front.SetBlockAt(targetUnitX, targetUnitY, BlockType, BlockID);
		}

		// Spawn Block Entity
		if (
			BlockType == BlockType.Entity &&
			BLOCK_ENTITY_TYPE.IsAssignableFrom(Stage.GetEntityType(BlockID)) &&
			Stage.SpawnEntity(BlockID, targetUnitX.ToGlobal(), targetUnitY.ToGlobal()) is IBlockEntity bEntity
		) {
			// Event
			bEntity.OnEntityPut(pHolder);
		}

		// Reduce Block Count by 1
		int eqID = Inventory.GetEquipment(pHolder.TypeID, EquipmentType.Weapon, out int eqCount);
		if (eqID != 0) {
			int newEqCount = (eqCount - 1).GreaterOrEquelThanZero();
			if (newEqCount == 0) eqID = 0;
			Inventory.SetEquipment(pHolder.TypeID, EquipmentType.Weapon, eqID, newEqCount);
		}
	}


}
