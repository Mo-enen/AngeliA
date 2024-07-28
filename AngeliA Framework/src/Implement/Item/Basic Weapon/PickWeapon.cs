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
	public virtual bool AllowPickIBlockEntity => true;
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

		pHolder.OverridePoseAttackAnimation(WeaponType, PoseAttack_PickaxeKnock.TYPE_ID);
		pHolder.SquatSpeed.Override(0, 1);
		pHolder.WalkSpeed.Override(0, 1);

		var aim = pHolder.AimingDirection;
		var aimNormal = aim.Normal();
		int pointX = aim.IsTop() ? pHolder.Rect.CenterX() : pHolder.FacingRight ? pHolder.Rect.xMax - 16 : pHolder.Rect.xMin + 16;
		int pointY = pHolder.Rect.yMax - 16;
		int targetUnitX = pointX.ToUnit() + aimNormal.x;
		int targetUnitY = pointY.ToUnit() + aimNormal.y;
		bool hasTraget = HasPickableBlockAt(targetUnitX, targetUnitY);

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
				hasTraget = HasPickableBlockAt(targetUnitX, targetUnitY);
			}
		}

		// Target Block Highlight
		if (!PlayerMenuUI.ShowingUI) {
			DrawPickTargetHighlight(targetUnitX, targetUnitY, hasTraget);
		}

		// Pick
		if (Game.GlobalFrame == pHolder.LastAttackFrame) {
			PickBlockAt(targetUnitX, targetUnitY);
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
	protected bool HasPickableBlockAt (int unitX, int unitY) {
		// Check for Block Entity
		if (AllowPickIBlockEntity) {
			var hits = Physics.OverlapAll(
				PICK_MASK,
				new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
				out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				if (hits[i].Entity is IBlockEntity) return true;
			}
		}

		// Check for Level Block
		if (AllowPickLevelBlock && WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level) != 0) {
			return true;
		}

		// Check for BG Block
		if (AllowPickBackgroundBlock && WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background) != 0) {
			return true;
		}

		return false;
	}


	protected void PickBlockAt (int unitX, int unitY) {

		// Try Pick Block Entity
		if (AllowPickIBlockEntity) {
			var hits = Physics.OverlapAll(
				PICK_MASK,
				new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
				out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var e = hits[i].Entity;
				if (e is not IBlockEntity) continue;
				e.Active = false;
				var mapPos = e.MapUnitPos;
				if (mapPos.HasValue) {
					WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, BlockType.Entity, 0);
					GlobalEvent.InvokeObjectBreak(
						e.TypeID, new IRect(
							e.X, e.Y, Const.CEL, Const.CEL
						)
					);
				}
				if (DropItemAfterPicked && ItemSystem.HasItem(e.TypeID)) {
					ItemSystem.SpawnItem(e.TypeID, e.X, e.Y, jump: false);
				}
				return;
			}
		}

		// Try Pick Level Block
		if (AllowPickLevelBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level);
			if (blockID != 0) {
				WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Level, 0);
				GlobalEvent.InvokeObjectBreak(
					blockID, new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL)
				);
				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					// Rule
					if (sprite.Group.WithRule) {
						FrameworkUtil.RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}
				if (DropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), jump: false);
				}
				return;
			}
		}

		// Try Pick BG Block
		if (AllowPickBackgroundBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background);
			if (blockID != 0) {
				WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Background, 0);
				GlobalEvent.InvokeObjectBreak(
					blockID, new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL)
				);
				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					// Rule
					if (sprite.Group.WithRule) {
						FrameworkUtil.RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}
				if (DropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), jump: false);
				}
				return;
			}
		}

	}


}
