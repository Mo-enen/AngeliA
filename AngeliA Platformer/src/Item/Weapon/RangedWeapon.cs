using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AngeliA;
namespace AngeliA.Platformer;


public abstract class RangedWeapon<B> : ProjectileWeapon<B> where B : ArrowBullet {


	protected int ArrowItemID { get; init; }
	private int SpriteIdAttack { get; init; }
	private int SpriteFrameCount { get; init; }
	protected abstract bool IsBow { get; }
	public sealed override ToolType ToolType => ToolType.Ranged;
	public sealed override ToolHandheld Handheld => IsBow ? ToolHandheld.Bow : ToolHandheld.Shooting;
	protected override ProjectileValidDirection ValidDirection => ProjectileValidDirection.Eight;
	public override int Cooldown => base.Cooldown;
	private int SpriteIdString { get; init; }
	public override bool AvailableWhenSquatting => true;
	public override int? DefaultMovementSpeedRateOnUse => 1000;
	public override int? WalkingMovementSpeedRateOnUse => 1000;
	public override int? RunningMovementSpeedRateOnUse => 618;
	public override int BulletDelayRate => 500;


	public RangedWeapon () {
		// Sprite
		if (IsBow) {
			SpriteIdString = $"{GetType().AngeName()}.String".AngeHash();
			if (!Renderer.HasSprite(SpriteIdString)) SpriteIdString = 0;
		} else {
			SpriteIdAttack = $"{GetType().AngeName()}.Attack".AngeHash();
			if (Renderer.HasSpriteGroup(SpriteIdAttack, out int length)) {
				SpriteFrameCount = length;
			} else {
				SpriteIdAttack = 0;
				SpriteFrameCount = 0;
			}
		}
		// Arrow ID
		var bType = typeof(B);
		if (bType.BaseType.IsConstructedGenericType) {
			var arrItemTypes = bType.BaseType.GetGenericArguments();
			if (arrItemTypes.Length > 0) {
				var arrItemType = arrItemTypes[0];
				ArrowItemID = arrItemType.AngeHash();
			}
		}
	}

	public override Bullet SpawnBullet (Character sender) {

		// Take Arrow
		int takenCount = base.BulletCountInOneShot;
		if (ArrowItemID != 0) {
			// Item Arrow
			takenCount = Inventory.FindAndTakeItem(sender.InventoryID, ArrowItemID, BulletCountInOneShot);
			if (takenCount == 0) {
				FrameworkUtil.InvokeItemErrorHint(sender, ArrowItemID);
				return null;
			}
		}

		// Shot Bullets
		ForceBulletCountNextShot = takenCount;
		var result = base.SpawnBullet(sender);
		ForceBulletCountNextShot = -1;

		return result;
	}

	protected override Cell DrawToolSprite (PoseCharacterRenderer renderer, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		if (IsBow) {
			// Bow
			var cell = base.DrawToolSprite(renderer, x, y, width, height, grabRotation, grabScale, sprite, z);
			DrawString(renderer, cell, default, default, default);
			return cell;
		} else {
			// Shooting
			var cell = base.DrawToolSprite(renderer, x, y, width, height, grabRotation, grabScale, sprite, z);
			// Draw Attack
			var attack = renderer.TargetCharacter.Attackness;
			if (attack.IsAttacking || attack.IsChargingAttack) {
				int localFrame = attack.IsAttacking ?
					(Game.GlobalFrame - attack.LastAttackFrame) * SpriteFrameCount / Duration :
					SpriteFrameCount - 1;
				if (Renderer.TryGetSpriteFromGroup(SpriteIdAttack, localFrame, out var attackSprite, false, true)) {
					cell.Sprite = attackSprite;
					cell.PivotX = attackSprite.PivotX / 1000f;
					cell.PivotY = attackSprite.PivotY / 1000f;
					cell.Width = attackSprite.GlobalWidth * cell.Width.Sign();
					cell.Height = attackSprite.GlobalHeight * cell.Height.Sign();
				}
			}
			return cell;
		}
	}

	protected void DrawString (PoseCharacterRenderer renderer, Cell mainCell, Int2 offsetDown, Int2 offsetUp, Int2 offsetCenter) {
		var character = renderer.TargetCharacter;
		var movement = character.Movement;
		var attackness = character.Attackness;
		int borderL = 0;
		int borderD = 0;
		int borderU = 0;
		if (Renderer.TryGetSprite(SpriteID, out var mainSprite)) {
			borderL = mainSprite.GlobalBorder.left * mainCell.Width.Sign();
			borderD = mainSprite.GlobalBorder.down;
			borderU = mainSprite.GlobalBorder.up;
		}
		if (!movement.FacingRight) {
			offsetDown.x = -offsetDown.x;
			offsetUp.x = -offsetUp.x;
			offsetCenter.x = -offsetCenter.x;
		}
		if (attackness.IsAttacking || attackness.IsChargingAttack) {

			// Attacking
			int duration = Duration;
			int localFrame = attackness.IsAttacking ? Game.GlobalFrame - attackness.LastAttackFrame : duration / 2 - 1;
			Int2 centerPos;
			var cornerU = mainCell.LocalToGlobal(borderL, mainCell.Height - borderU) + offsetUp;
			var cornerD = mainCell.LocalToGlobal(borderL, borderD) + offsetDown;
			var handPos = (movement.FacingRight ? renderer.HandL : renderer.HandR).GlobalLerp(0.5f, 0.5f);
			if (localFrame < duration / 2) {
				// Pulling
				centerPos = handPos + offsetCenter;
			} else {
				// Release
				centerPos = Float2.Lerp(
					handPos, mainCell.LocalToGlobal(borderL, mainCell.Height / 2),
					Ease.OutBack((localFrame - duration / 2f) / (duration / 2f))
				).RoundToInt() + offsetCenter;
			}

			// Draw Strings
			int stringWidth = movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE;
			Renderer.Draw(
				SpriteIdString, centerPos.x, centerPos.y, 500, 0,
				(cornerU - centerPos).GetRotation(),
				stringWidth, Util.DistanceInt(centerPos, cornerU), mainCell.Z - 1
			);
			Renderer.Draw(
				SpriteIdString, centerPos.x, centerPos.y, 500, 0,
				(cornerD - centerPos).GetRotation(),
				stringWidth, Util.DistanceInt(centerPos, cornerD), mainCell.Z - 1
			);

		} else {
			// Holding
			var point = mainCell.LocalToGlobal(borderL + offsetDown.x, borderD + offsetDown.y);
			Renderer.Draw(
				SpriteIdString,
				point.x, point.y,
				movement.FacingRight ? 0 : 1000, 0, mainCell.Rotation,
				Const.ORIGINAL_SIZE,
				mainCell.Height - borderD - borderU - offsetDown.y + offsetUp.y,
				mainCell.Z - 1
			);
		}
	}

}
