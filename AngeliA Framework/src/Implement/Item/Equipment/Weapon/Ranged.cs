using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class RangedWeapon<B, A> : RangedWeapon<B> where B : ArrowBullet where A : Item {
	public RangedWeapon () : base() {
		ArrowItemID = typeof(A).AngeHash();
	}
}
public abstract class RangedWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {

	public virtual int ArrowCountInOneShot => 1;
	public virtual int AngleSpeed => 0;
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
	}

	public override Bullet SpawnBullet (Character sender) {

		// Take Arrow
		int takenCount = ArrowCountInOneShot;
		if (ArrowItemID != 0) {
			// Item Arrow
			takenCount = Inventory.FindAndTakeItem(sender.TypeID, ArrowItemID, ArrowCountInOneShot);
			if (takenCount == 0) {
				// Hint
				InvokeOnItemInsufficient(sender, ArrowItemID);
				return null;
			}
		}

		// Spawn Bullet
		Bullet result = null;
		for (int i = 0; i < takenCount; i++) {

			result = base.SpawnBullet(sender);

			// Arrow Bullet
			if (ArrowItemID != 0) {
				if (result is ArrowBullet aBullet) {
					var item = ItemSystem.GetItem(ArrowItemID);
					aBullet.ArrowItemID = ArrowItemID;
					aBullet.ArrowArtworkID = item is BulletItem aItem ? aItem.BulletArtworkID : item.TypeID;
				}
			}

			// Movable Bullet
			if (result is MovableBullet mBullet) {
				if (AngleSpeed != 0) {
					int deltaAngle = (int)Util.Atan(mBullet.SpeedX, mBullet.SpeedY);
					int offset = (i % 2 == 0 ? 1 : -1) * ((i + 1) / 2);
					mBullet.CurrentRotation += offset * deltaAngle;
					mBullet.Velocity = new Int2(mBullet.Velocity.x, mBullet.Velocity.y + offset * AngleSpeed);
					mBullet.Y += offset * mBullet.Height / ArrowCountInOneShot;
				}
			}
		}
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



// Implement
[ItemCombination(typeof(iRope), typeof(iTreeBranch), 1)]
public class iBowWood : RangedWeapon<iBowWood.BowWoodBullet, iArrowWood> {
	protected override bool IsBow => true;
	public class BowWoodBullet : ArrowBullet {
		public override int SpeedX => 42;
	}
}


[ItemCombination(typeof(iBowWood), typeof(iIngotIron), 1)]
public class iBowIron : RangedWeapon<iBowIron.BowIronBullet, iArrowIron> {
	protected override bool IsBow => true;
	public class BowIronBullet : ArrowBullet {
		protected override bool DestroyOnHitReceiver => false;
		public override int SpeedX => 52;
	}
}


[ItemCombination(typeof(iBowIron), typeof(iIngotGold), 1)]
public class iBowGold : RangedWeapon<iBowGold.BowGoldBullet, iArrowGold> {
	protected override bool IsBow => true;
	public class BowGoldBullet : ArrowBullet {
		protected override bool DestroyOnHitReceiver => false;
		public override int SpeedX => 62;
	}
}


[ItemCombination(typeof(iRope), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
public class iCrossbowWood : RangedWeapon<iCrossbowWood.CrossbowWoodBullet, iBoltsWood> {
	protected override bool IsBow => false;
	public class CrossbowWoodBullet : ArrowBullet { }
}


[ItemCombination(typeof(iCrossbowWood), typeof(iIngotIron), 1)]
public class iCrossbowIron : RangedWeapon<iCrossbowIron.CrossbowIronBullet, iBoltsIron> {
	protected override bool IsBow => false;
	public class CrossbowIronBullet : ArrowBullet {
		protected override bool DestroyOnHitReceiver => false;
	}
}


[ItemCombination(typeof(iCrossbowIron), typeof(iIngotGold), 1)]
public class iCrossbowGold : RangedWeapon<iCrossbowGold.CrossbowGoldBullet, iBoltsGold> {
	protected override bool IsBow => false;
	public class CrossbowGoldBullet : ArrowBullet {
		protected override bool DestroyOnHitReceiver => false;
	}
}


[ItemCombination(typeof(iRunePoison), typeof(iNeedle), typeof(iTreeBranch), 1)]
public class iBlowgun : RangedWeapon<iBlowgun.BlowgunBullet, iPoisonDarts> {
	protected override bool IsBow => false;
	public class BlowgunBullet : ArrowBullet {
		protected override int DamageType => SpriteTag.DAMAGE_POISON_TAG;
	}
}


[ItemCombination(typeof(iRubberBall), typeof(iRibbon), typeof(iIngotIron), 1)]
public class iSlingshot : RangedWeapon<iSlingshot.SlingshotBullet, iMarbles> {
	protected override bool IsBow => true;
	public class SlingshotBullet : ArrowBullet {
		public override int ArtworkID => Renderer.TryGetSpriteFromGroup(
			base.ArtworkID, SpawnFrame, out var sprite, true, true
		) ? sprite.ID : 0;
	}
}


[ItemCombination(typeof(iBowIron), typeof(iBowIron), 1)]
public class iCompoundBow : RangedWeapon<iCompoundBow.CompoundBowBullet, iArrowIron> {
	protected override bool IsBow => true;
	public class CompoundBowBullet : ArrowBullet {
		protected override bool DestroyOnHitReceiver => false;
	}
}


[ItemCombination(typeof(iBowWood), typeof(iBowWood), 1)]
public class iRepeatingCrossbow : RangedWeapon<iRepeatingCrossbow.RepeatingCrossbowBullet, iBoltsWood> {
	protected override bool IsBow => false;
	public override int AttackDuration => 12;
	public override int AttackCooldown => 0;
	public override bool RepeatAttackWhenHolding => true;
	public class RepeatingCrossbowBullet : ArrowBullet { }
}


[ItemCombination(typeof(iBowWood), typeof(iLeaf), 1)]
public class iBowNature : RangedWeapon<iBowNature.BowNatureBullet, iLeaf> {
	public class BowNatureBullet : ArrowBullet {
		public override int StartRotation => 45;
		public override int Scale => 500;
	}
	protected override bool IsBow => true;
}


[ItemCombination(typeof(iBowWood), typeof(iSkull), typeof(iSkull), typeof(iRibbon), 1)]
public class iBowSkull : RangedWeapon<iBowSkull.BowSkullBullet, iSkull> {
	public class BowSkullBullet : ArrowBullet {
		public override int RotateSpeed => 12;
		public override int Scale => 500;
	}
	protected override bool IsBow => true;
}


[ItemCombination(typeof(iBowIron), typeof(iRuneCube), 1)]
public class iBowMage : RangedWeapon<iBowMage.BowMageBullet> {
	public class BowMageBullet : MovableBullet {
		public override int SpeedX => 96;
		protected override void BeforeDespawn (IDamageReceiver receiver) {
			if (Stage.SpawnEntity(AppearSmokeParticle.TYPE_ID, X + Width / 2, Y + Height / 2) is AppearSmokeParticle particle0) {
				particle0.Tint = new(246, 196, 255, 255);
				particle0.X += Util.QuickRandom(Game.GlobalFrame * 181).UMod(Const.HALF) - Const.HALF / 2;
				particle0.Y += Util.QuickRandom(Game.GlobalFrame * 832).UMod(Const.HALF) - Const.HALF / 2;
				particle0.Rotation = Util.QuickRandom(Game.GlobalFrame * 163).UMod(360);
				particle0._Scale = Util.QuickRandom(Game.GlobalFrame * 4116).UMod(800) + 300;
			}
			if (Stage.SpawnEntity(AppearSmokeParticle.TYPE_ID, X + Width / 2, Y + Height / 2) is AppearSmokeParticle particle1) {
				particle1.Tint = Color32.WHITE;
				particle1.X += Util.QuickRandom(Game.GlobalFrame * 125).UMod(Const.HALF) - Const.HALF / 2;
				particle1.Y += Util.QuickRandom(Game.GlobalFrame * 67).UMod(Const.HALF) - Const.HALF / 2;
				particle1.Rotation = Util.QuickRandom(Game.GlobalFrame * 127).UMod(360);
				particle1._Scale = Util.QuickRandom(Game.GlobalFrame * 9).UMod(800) + 300;
				particle1._RenderingZ = Util.QuickRandom(Game.GlobalFrame * 12) % 2 == 0 ? int.MaxValue : int.MaxValue - 2;
			}
		}
	}
	protected override bool IsBow => true;
	protected override int BulletPivotY => 618;
}


[ItemCombination(typeof(iBowNature), typeof(iLeafLegend), 1)]
public class iBowSky : RangedWeapon<iBowSky.BowSkyBullet, iLeaf> {

	[EntityAttribute.Capacity(24, 0)]
	public class BowSkyBullet : ArrowBullet {
		public override int SpeedX => 86;
		public override int StartRotation => 45;
		public override int Scale => 500;
	}
	protected override bool IsBow => true;
	public override int AttackCooldown => 18;
	public override int ArrowCountInOneShot => 3;
	public override int AngleSpeed => 3;
}


[ItemCombination(typeof(iBowGold), typeof(iRope), typeof(iRope), typeof(iRope), 1)]
public class iBowHarp : RangedWeapon<iBowHarp.BowHarpBullet, iArrowGold> {
	[EntityAttribute.Capacity(24, 0)]
	public class BowHarpBullet : ArrowBullet {
		public override int SpeedX => 96;
		protected override bool DestroyOnHitReceiver => false;
	}
	protected override bool IsBow => true;
	public override int AttackCooldown => 2;
	public override int ArrowCountInOneShot => 4;
	public override int AngleSpeed => 2;
	protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
		const int GAP_X0 = -32;
		const int GAP_X1 = -64;
		const int GAP_X2 = -96;
		int centerDeltaX0 = GAP_X0;
		int centerDeltaX1 = GAP_X1;
		int centerDeltaX2 = GAP_X2;
		if (character.IsAttacking) {
			int localFrame = Game.GlobalFrame - character.LastAttackFrame;
			if (localFrame < AttackDuration / 2) {
				centerDeltaX0 = 0;
				centerDeltaX1 = 0;
				centerDeltaX2 = 0;
			} else {
				localFrame -= AttackDuration / 2;
				if (localFrame < AttackDuration / 8) centerDeltaX0 = 0;
				if (localFrame < AttackDuration / 6) centerDeltaX1 = 0;
				if (localFrame < AttackDuration / 4) centerDeltaX2 = 0;
			}
		}
		DrawString(character, cell, new(GAP_X0, 00), new(GAP_X0, 000), new(centerDeltaX0, 0));
		DrawString(character, cell, new(GAP_X1, 16), new(GAP_X1, -16), new(centerDeltaX1, 0));
		DrawString(character, cell, new(GAP_X2, 20), new(GAP_X2, -20), new(centerDeltaX2, 0));
		return cell;
	}
}
