using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PickWeapon : Weapon {


	// VAR
	private const int PICK_MASK = PhysicsMask.MAP;
	public override WeaponType WeaponType => WeaponType.Axe;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public override bool AttackWhenSquatting => true;
	public override bool AttackWhenWalking => true;
	public override bool AttackWhenSliding => true;
	public override int? DefaultSpeedLoseOnAttack => 618;
	public override int? RunningSpeedLoseOnAttack => 618;
	public override int? WalkingSpeedLoseOnAttack => 618;
	public virtual bool AllowPickLevelBlock => true;
	public virtual bool AllowPickBackgroundBlock => true;
	public virtual bool AllowPickBlockEntity => true;
	public virtual bool DropItemAfterPicked => true;


	// MSG
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
		GetTargetUnitPosition(pHolder, out int targetUnitX, out int targetUnitY, out bool hasTraget);

		// Target Block Highlight
		if (!PlayerMenuUI.ShowingUI) {
			DrawPickTargetHighlight(targetUnitX, targetUnitY, hasTraget);
		}

		// Pick
		if (Game.GlobalFrame == pHolder.LastAttackFrame) {
			FrameworkUtil.PickBlockAt(
				holder, targetUnitX, targetUnitY,
				allowPickBlockEntity: AllowPickBlockEntity,
				allowPickLevelBlock: AllowPickLevelBlock,
				allowPickBackgroundBlock: AllowPickBackgroundBlock
			);
		}

		// Base
		_BASE_:;
		base.PoseAnimationUpdate_FromEquipment(holder);

	}


	protected virtual void DrawPickTargetHighlight (int unitX, int unitY, bool hasTarget) {
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
	protected virtual void GetTargetUnitPosition (Character pHolder, out int targetUnitX, out int targetUnitY, out bool hasTraget) {

		var aim = pHolder.AimingDirection;
		var aimNormal = aim.Normal();
		int pointX = aim.IsTop() ? pHolder.Rect.CenterX() : pHolder.FacingRight ? pHolder.Rect.xMax - 16 : pHolder.Rect.xMin + 16;
		int pointY = pHolder.Rect.yMax - 16;
		targetUnitX = pointX.ToUnit() + aimNormal.x;
		targetUnitY = pointY.ToUnit() + aimNormal.y;
		hasTraget = FrameworkUtil.HasPickableBlockAt(
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
					targetUnitX += pHolder.FacingRight ? 1 : -1;
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

	}


}
