using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 


public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Ranged, Polearm, Hook, Claw, Magic, Throwing, }


public enum WeaponHandheld { SingleHanded, DoubleHanded, OneOnEachHand, Pole, MagicPole, Bow, Shooting, Float, }


[EntityAttribute.MapEditorGroup("ItemWeapon")]
[RequireSprite("{0}.Main")]
public abstract class Weapon : Equipment {

	// VAR
	protected int BulletID { get; init; } = 0;
	protected int SpriteID { get; init; }
	public sealed override EquipmentType EquipmentType => EquipmentType.Weapon;
	public abstract WeaponType WeaponType { get; }
	public abstract WeaponHandheld Handheld { get; }
	public int BulletDelayFrame => AttackDuration * BulletDelay / 1000;
	protected virtual int BulletDelay => 0;
	public virtual int AttackDuration => 12;
	public virtual int AttackCooldown => 2;
	public virtual int ChargeAttackDuration => int.MaxValue;
	public virtual int? DefaultSpeedLoseOnAttack => null;
	public virtual int? WalkingSpeedLoseOnAttack => null;
	public virtual int? RunningSpeedLoseOnAttack => null;
	public virtual bool RepeatAttackWhenHolding => false;
	public virtual bool LockFacingOnAttack => false;
	public virtual bool AttackInAir => true;
	public virtual bool AttackInWater => true;
	public virtual bool AttackWhenWalking => true;
	public virtual bool AttackWhenRunning => true;
	public virtual bool AttackWhenClimbing => false;
	public virtual bool AttackWhenFlying => false;
	public virtual bool AttackWhenRolling => false;
	public virtual bool AttackWhenSquatting => false;
	public virtual bool AttackWhenDashing => false;
	public virtual bool AttackWhenSliding => false;
	public virtual bool AttackWhenGrabbing => false;
	public virtual bool AttackWhenRush => false;
	public virtual bool AttackWhenPounding => false;
	protected virtual bool IgnoreGrabTwist => false;

	// MSG
	public Weapon () {
		SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = 0;
	}

	public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

		base.PoseAnimationUpdate_FromEquipment(holder);

		if (
			holder is not PoseCharacter character ||
			character.AnimationType == CharacterAnimationType.Sleep ||
			character.AnimationType == CharacterAnimationType.PassOut ||
			character.AnimationType == CharacterAnimationType.Crash ||
			!Renderer.TryGetSprite(SpriteID, out var sprite)
		) return;

		DrawWeaponLogic(character, sprite);

	}

	private void DrawWeaponLogic (PoseCharacter character, AngeSprite sprite) {

		bool attacking = character.IsAttacking;
		int grabScaleL = character.HandGrabScaleL;
		int grabScaleR = character.HandGrabScaleR;
		int twistL = attacking && !IgnoreGrabTwist ? character.HandGrabAttackTwistL : 1000;
		int twistR = attacking && !IgnoreGrabTwist ? character.HandGrabAttackTwistR : 1000;
		int zLeft = character.HandL.Z - 1;
		int zRight = character.HandR.Z - 1;
		int facingSign = character.FacingRight ? 1 : -1;

		if (character.EquippingWeaponType == WeaponType.Claw) {
			grabScaleL = grabScaleL * 700 / 1000;
			grabScaleR = grabScaleR * 700 / 1000;
			if (sprite.IsTrigger) {
				zLeft = character.HandL.Z + 1;
				zRight = character.HandR.Z + 1;
			}
		}

		// Draw
		switch (character.EquippingWeaponHeld) {

			default:
			case WeaponHandheld.Float: {
				// Floating
				const int SHIFT_X = 148;
				int moveDeltaX = -character.DeltaPositionX * 2;
				int moveDeltaY = -character.DeltaPositionY;
				int facingFrame = Game.GlobalFrame - character.LastFacingChangeFrame;
				if (facingFrame < 30) {
					moveDeltaX += (int)Util.LerpUnclamped(
						facingSign * SHIFT_X * 2, 0,
						Ease.OutBack(facingFrame / 30f)
					);
				}
				DrawWeaponSprite(
					character,
					character.X + (facingSign * -SHIFT_X) + moveDeltaX,
					character.Y + Const.CEL * character.CharacterHeight / 263 + Game.GlobalFrame.PingPong(240) / 4 + moveDeltaY,
					sprite.GlobalWidth,
					sprite.GlobalHeight,
					0,
					(sprite.IsTrigger ? facingSign : 1) * (attacking ? grabScaleL : 700),
					sprite,
					36
				);
				break;
			}

			case WeaponHandheld.SingleHanded: {
				// Single 
				int grabScale = grabScaleR;
				int grabRotation = character.HandGrabRotationR;
				int z = zRight;
				if (character.EquippingWeaponType == WeaponType.Throwing) {
					if (
						attacking &&
						Game.GlobalFrame - character.LastAttackFrame > AttackDuration / 6
					) break;
					grabScale = 700;
					z = character.FacingFront ? character.HandR.Z.Abs() + 1 : -character.HandR.Z.Abs() - 1;
				}
				// Fix Rotation
				if (sprite.IsTrigger) {
					if (!attacking) {
						grabRotation = 0;
					} else {
						grabRotation = Util.RemapUnclamped(
							0, AttackDuration,
							facingSign * 90, 0,
							Game.GlobalFrame - character.LastAttackFrame
						);
					}
				}
				// Draw
				var center = character.HandR.GlobalLerp(0.5f, 0.5f);
				DrawWeaponSprite(
					character,
					center.x, center.y,
					sprite.GlobalWidth * twistR / 1000,
					sprite.GlobalHeight,
					grabRotation, grabScale,
					sprite, z
				);
				break;
			}

			case WeaponHandheld.DoubleHanded:
			case WeaponHandheld.Shooting: {
				// Double
				var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
				var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
				DrawWeaponSprite(
					character,
					(centerL.x + centerR.x) / 2,
					(centerL.y + centerR.y) / 2,
					sprite.GlobalWidth * twistR / 1000,
					sprite.GlobalHeight,
					character.HandGrabRotationL,
					grabScaleL, sprite,
					zRight
				);
				break;
			}

			case WeaponHandheld.OneOnEachHand: {
				// Each Hand
				var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
				var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
				DrawWeaponSprite(
					character,
					centerL.x, centerL.y,
					sprite.GlobalWidth * twistL / 1000,
					sprite.GlobalHeight,
					character.HandGrabRotationL,
					grabScaleL, sprite,
					zLeft
				);
				DrawWeaponSprite(
					character,
					centerR.x, centerR.y,
					sprite.GlobalWidth * twistR / 1000,
					sprite.GlobalHeight,
					character.HandGrabRotationR,
					grabScaleR, sprite,
					zRight
				);
				break;
			}

			case WeaponHandheld.Pole: {
				// Polearm
				var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
				var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
				DrawWeaponSprite(
					character,
					(centerL.x + centerR.x) / 2,
					(centerL.y + centerR.y) / 2,
					sprite.GlobalWidth * twistR / 1000,
					sprite.GlobalHeight,
					character.HandGrabRotationR,
					grabScaleR,
					sprite,
					zRight
				);
				break;
			}

			case WeaponHandheld.Bow: {
				if (attacking) {
					// Attacking
					var center = (character.FacingRight ? character.HandR : character.HandL).GlobalLerp(0.5f, 0.5f);
					int width = sprite.GlobalWidth;
					int height = sprite.GlobalHeight;
					if (!sprite.IsTrigger) {
						int localFrame = Game.GlobalFrame - character.LastAttackFrame;
						if (localFrame < AttackDuration / 2) {
							// Pulling
							float ease01 = Ease.OutQuad(localFrame / (AttackDuration / 2f));
							width += Util.LerpUnclamped(0, width * 2 / 3, ease01).RoundToInt();
							height -= Util.LerpUnclamped(0, height / 2, ease01).RoundToInt();
						} else {
							// Release
							float ease01 = Ease.OutQuad((localFrame - AttackDuration / 2f) / (AttackDuration / 2f));
							width += Util.LerpUnclamped(width * 2 / 3, 0, ease01).RoundToInt();
							height -= Util.LerpUnclamped(height / 2, 0, ease01).RoundToInt();
						}
					}
					DrawWeaponSprite(
						character, center.x, center.y, width, height,
						0, facingSign * 1000,
						sprite, character.FacingRight ? zRight : zLeft
					);
				} else {
					// Holding
					var center = (character.FacingRight ? character.HandR : character.HandL).GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character, center.x, center.y,
						sprite.GlobalWidth, sprite.GlobalHeight,
						character.HandGrabRotationL,
						grabScaleL, sprite,
						zRight
					);
				}
				break;
			}

		}


	}

	protected virtual Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) => Renderer.Draw(
		sprite,
		x, y,
		sprite.PivotX, sprite.PivotY, grabRotation,
		width * grabScale / 1000,
		height * grabScale.Abs() / 1000,
		z
	);

	public virtual bool AllowingAttack (PoseCharacter character) => true;

	public virtual int GetOverrideHandheldAnimationID (Character character) => 0;

	public virtual int GetOverrideAttackAnimationID (Character character) => 0;

	public virtual Bullet SpawnBullet (Character sender) => SpawnRawBullet(sender, BulletID);

	public static Bullet SpawnRawBullet (Character sender, int bulletID) {
		if (sender == null || bulletID == 0) return null;
		var rect = sender.Rect;
		if (Stage.SpawnEntity(bulletID, rect.x, rect.y) is not Bullet bullet) return null;
		bullet.Sender = sender;
		var sourceRect = sender.Rect;
		bullet.X = sourceRect.CenterX() - bullet.Width / 2;
		bullet.Y = sourceRect.CenterY() - bullet.Height / 2;
		bullet.AttackIndex = sender.AttackStyleIndex;
		bullet.AttackCharged = sender.LastAttackCharged;
		bullet.TargetTeam = sender.AttackTargetTeam;
		return bullet;
	}

}
