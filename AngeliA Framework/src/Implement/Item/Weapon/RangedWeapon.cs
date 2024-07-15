using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;


public abstract class RangedWeapon<B> : ProjectileWeapon<B> where B : ArrowBullet {



	protected int ArrowItemID { get; init; }
	private int SpriteIdAttack { get; init; }
	private int SpriteFrameCount { get; init; }
	protected abstract bool IsBow { get; }
	public sealed override WeaponType WeaponType => WeaponType.Ranged;
	public sealed override WeaponHandheld Handheld => IsBow ? WeaponHandheld.Bow : WeaponHandheld.Shooting;
	public override int AttackCooldown => base.AttackCooldown;
	private int SpriteIdString { get; init; }
	public override bool AttackWhenSquatting => true;
	public override int? DefaultSpeedLoseOnAttack => 1000;
	public override int? WalkingSpeedLoseOnAttack => 1000;
	public override int? RunningSpeedLoseOnAttack => 618;
	protected override int BulletDelay => 500;


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
			takenCount = Inventory.FindAndTakeItem(sender.TypeID, ArrowItemID, BulletCountInOneShot);
			if (takenCount == 0) {
				// Hint
				InvokeOnItemInsufficient(sender, ArrowItemID);
				return null;
			}
		}

		// Shot Bullets
		ForceBulletCountNextShot = takenCount;
		var result = base.SpawnBullet(sender);
		ForceBulletCountNextShot = -1;

		return result;
	}

	protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		if (IsBow) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			DrawString(character, cell, default, default, default);
			return cell;
		} else {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			// Draw Attack
			if (character.IsAttacking || character.IsChargingAttack) {
				int localFrame = character.IsAttacking ?
					(Game.GlobalFrame - character.LastAttackFrame) * SpriteFrameCount / AttackDuration :
					SpriteFrameCount - 1;
				if (Renderer.TryGetSpriteFromGroup(SpriteIdAttack, localFrame, out var attackSprite, false, true)) {
					cell.Color = Color32.CLEAR;
					Renderer.Draw(
						attackSprite,
						cell.X, cell.Y, attackSprite.PivotX, attackSprite.PivotY, cell.Rotation,
						attackSprite.GlobalWidth,
						character.FacingRight ? attackSprite.GlobalHeight : -attackSprite.GlobalHeight,
						cell.Z
					);
				}
			}
			return cell;
		}
	}

	protected void DrawString (PoseCharacter character, Cell mainCell, Int2 offsetDown, Int2 offsetUp, Int2 offsetCenter) {
		int borderL = 0;
		int borderD = 0;
		int borderU = 0;
		if (Renderer.TryGetSprite(SpriteID, out var mainSprite)) {
			borderL = mainSprite.GlobalBorder.left * mainCell.Width.Sign();
			borderD = mainSprite.GlobalBorder.down;
			borderU = mainSprite.GlobalBorder.up;
		}
		if (!character.FacingRight) {
			offsetDown.x = -offsetDown.x;
			offsetUp.x = -offsetUp.x;
			offsetCenter.x = -offsetCenter.x;
		}
		if (character.IsAttacking || character.IsChargingAttack) {

			// Attacking
			int duration = AttackDuration;
			int localFrame = character.IsAttacking ? Game.GlobalFrame - character.LastAttackFrame : duration / 2 - 1;
			Int2 centerPos;
			var cornerU = mainCell.LocalToGlobal(borderL, mainCell.Height - borderU) + offsetUp;
			var cornerD = mainCell.LocalToGlobal(borderL, borderD) + offsetDown;
			var handPos = (character.FacingRight ? character.HandL : character.HandR).GlobalLerp(0.5f, 0.5f);
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
			int stringWidth = character.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE;
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
				character.FacingRight ? 0 : 1000, 0, mainCell.Rotation,
				Const.ORIGINAL_SIZE,
				mainCell.Height - borderD - borderU - offsetDown.y + offsetUp.y,
				mainCell.Z - 1
			);
		}
	}

}
