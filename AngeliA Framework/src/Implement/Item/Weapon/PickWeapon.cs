using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PickWeapon : Weapon {


	// VAR
	public override WeaponType WeaponType => WeaponType.Axe;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
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
	public virtual int MouseRange => 6;
	public override bool UseStackAsUsage => true;
	public override int MaxStackCount => 4096;


	// MSG
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

		// Override
		if (!pHolder.IsInsideGround) {
			pHolder.Movement.SquatSpeed.Override(0, 1, priority: 4096);
			pHolder.Movement.WalkSpeed.Override(0, 1, priority: 4096);
		}
		if (pHolder is Player plHolder) {
			plHolder.IgnoreAction(1);
		}

		// Get Target Pos
		int targetUnitX, targetUnitY;
		bool hasTraget, inRange = true;
		if (UseMouseToPick && Game.IsMouseAvailable) {
			Cursor.RequireCursor();
			hasTraget = GetTargetUnitPositionFromMouse(pHolder, out targetUnitX, out targetUnitY, out inRange);
		} else {
			hasTraget = GetTargetUnitPositionFromKey(pHolder, out targetUnitX, out targetUnitY);
		}

		// Target Block Highlight
		if (inRange && !PlayerMenuUI.ShowingUI) {
			DrawPickTargetHighlight(targetUnitX, targetUnitY, hasTraget);
		}

		// Pick Block
		if (Game.GlobalFrame == pHolder.Attackness.LastAttackFrame) {
			// Erase Block from Map
			bool picked = FrameworkUtil.PickBlockAt(
				targetUnitX, targetUnitY,
				allowPickBlockEntity: AllowPickBlockEntity,
				allowPickLevelBlock: AllowPickLevelBlock,
				allowPickBackgroundBlock: AllowPickBackgroundBlock
			);
			// Reduce Weapon Usage
			if (picked) {
				Inventory.ReduceEquipmentCount(holder.TypeID, 1, EquipmentType.Weapon);
			}
		}

		// Base
		base.PoseAnimationUpdate_FromEquipment(holder);

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


	public override Bullet SpawnBullet (Character sender) => null;


	// LGC
	protected virtual bool GetTargetUnitPositionFromMouse (Character holder, out int targetUnitX, out int targetUnitY, out bool inRange) {

		var mouseUnitPos = Input.MouseGlobalPosition.ToUnit();
		targetUnitX = mouseUnitPos.x;
		targetUnitY = mouseUnitPos.y;

		// Range Check
		int holderUnitX = holder.Rect.CenterX().ToUnit();
		int holderUnitY = (holder.Rect.y + Const.HALF).ToUnit();
		if (
			!targetUnitX.InRangeInclude(holderUnitX - MouseRange, holderUnitX + MouseRange) ||
			!targetUnitY.InRangeInclude(holderUnitY - MouseRange, holderUnitY + MouseRange)
		) {
			inRange = false;
			return false;
		}

		// Has Pickable Block
		inRange = true;
		return FrameworkUtil.HasPickableBlockAt(
			targetUnitX, targetUnitY,
			allowPickBlockEntity: AllowPickBlockEntity,
			allowPickLevelBlock: AllowPickLevelBlock,
			allowPickBackgroundBlock: AllowPickBackgroundBlock
		);
	}


	protected virtual bool GetTargetUnitPositionFromKey (Character pHolder, out int targetUnitX, out int targetUnitY) {

		var aim = pHolder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		int pointX = aim.IsTop() ? pHolder.Rect.CenterX() : pHolder.Movement.FacingRight ? pHolder.Rect.xMax - 16 : pHolder.Rect.xMin + 16;
		int pointY = pHolder.Rect.yMax - 16;
		targetUnitX = pointX.ToUnit() + aimNormal.x;
		targetUnitY = pointY.ToUnit() + aimNormal.y;
		bool hasTraget = FrameworkUtil.HasPickableBlockAt(
			targetUnitX, targetUnitY,
			allowPickBlockEntity: AllowPickBlockEntity,
			allowPickLevelBlock: AllowPickLevelBlock,
			allowPickBackgroundBlock: AllowPickBackgroundBlock
		);

		// Redirect
		if (!hasTraget) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += pointX.UMod(Const.CEL) < Const.HALF ? -1 : 1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += pHolder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY--;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				hasTraget = FrameworkUtil.HasPickableBlockAt(
					targetUnitX, targetUnitY,
					allowPickBlockEntity: AllowPickBlockEntity,
					allowPickLevelBlock: AllowPickLevelBlock,
					allowPickBackgroundBlock: AllowPickBackgroundBlock
				);
			}
		}

		return hasTraget;

	}


}
