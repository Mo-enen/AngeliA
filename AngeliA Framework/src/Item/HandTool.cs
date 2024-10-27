using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public enum ToolType { Hand, Sword, Axe, Hammer, Flail, Ranged, Polearm, Hook, Claw, Magic, Throwing, Block, Pick, Tool, }


public enum ToolHandheld { SingleHanded, DoubleHanded, OneOnEachHand, Pole, MagicPole, Bow, Shooting, Float, }


[EntityAttribute.MapEditorGroup("ItemHandTool")]
public abstract class HandTool : Equipment {


	// VAR
	public int BulletID { get; protected set; }
	public int SpriteID { get; protected set; }
	public sealed override EquipmentType EquipmentType => EquipmentType.HandTool;
	public abstract ToolType ToolType { get; }
	public abstract ToolHandheld Handheld { get; }
	public int BulletDelayFrame => AttackDuration * BulletDelay / 1000;
	public string TypeName { get; init; }
	public virtual int BulletDelay => 0;
	public virtual int AttackDuration => 12;
	public virtual int AttackCooldown => 2;
	public virtual int HoldAttackPunish => 4;
	public virtual int ChargeAttackDuration => int.MaxValue;
	public virtual int? DefaultSpeedRateOnAttack => null;
	public virtual int? WalkingSpeedRateOnAttack => null;
	public virtual int? RunningSpeedRateOnAttack => null;
	public virtual bool UseStackAsUsage => false;
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
	public virtual bool AttackWhenRushing => false;
	public virtual bool AttackWhenPounding => false;
	public virtual bool IgnoreGrabTwist => false;


	// MSG
	public HandTool () : this(true) => TypeName = GetType().AngeName();


	public HandTool (bool loadArtwork) {
		if (loadArtwork) LoadFromSheet();
	}


	public void LoadFromSheet () {
		SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = TypeID;
	}


	public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

		base.PoseAnimationUpdate_FromEquipment(holder);

		if (
			holder is not Character character ||
			character.Rendering is not PoseCharacterRenderer renderer ||
			character.AnimationType == CharacterAnimationType.Sleep ||
			character.AnimationType == CharacterAnimationType.PassOut ||
			character.AnimationType == CharacterAnimationType.Crash
		) return;

		if (
			!Renderer.TryGetSprite(SpriteID, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(SpriteID, 0, out sprite)
		) return;

		int oldGrabSclL = renderer.HandGrabScaleL;
		int oldGrabSclR = renderer.HandGrabScaleR;
		if (ToolType == ToolType.Claw) {
			renderer.HandGrabScaleL = renderer.HandGrabScaleL * 8 / 10;
			renderer.HandGrabScaleR = renderer.HandGrabScaleR * 8 / 10;
		}

		// Tool Handheld
		switch (character.EquippingToolHeld) {
			default:
			case ToolHandheld.Float:
				DrawTool_Float(renderer, sprite);
				break;

			case ToolHandheld.SingleHanded:
				DrawTool_SingleHanded(renderer, sprite);
				break;

			case ToolHandheld.DoubleHanded:
			case ToolHandheld.Shooting:
				DrawTool_Double_Shoot(renderer, sprite);
				break;

			case ToolHandheld.OneOnEachHand:
				DrawTool_Each(renderer, sprite);
				break;

			case ToolHandheld.Pole:
				DrawTool_Pole(renderer, sprite);
				break;

			case ToolHandheld.Bow:
				DrawTool_Bow(renderer, sprite);
				break;

		}

		renderer.HandGrabScaleL = oldGrabSclL;
		renderer.HandGrabScaleR = oldGrabSclR;

	}


	private void DrawTool_Float (PoseCharacterRenderer renderer, AngeSprite sprite) {
		const int SHIFT_X = 148;
		var character = renderer.TargetCharacter;
		int grabScaleL = character.Attackness.IsAttacking ? renderer.HandGrabScaleL : 700;
		int facingSign = character.Movement.FacingRight ? 1 : -1;
		int moveDeltaX = -character.DeltaPositionX * 2;
		int moveDeltaY = -character.DeltaPositionY;
		int facingFrame = Game.GlobalFrame - character.Movement.LastFacingChangeFrame;
		if (facingFrame < 30) {
			moveDeltaX += (int)Util.LerpUnclamped(
				facingSign * SHIFT_X * 2, 0,
				Ease.OutBack(facingFrame / 30f)
			);
		}
		DrawToolSprite(
			renderer,
			character.X + (facingSign * -SHIFT_X) + moveDeltaX,
			character.Y + Const.CEL * renderer.CharacterHeight / 263 + Game.GlobalFrame.PingPong(240) / 4 + moveDeltaY,
			sprite.GlobalWidth,
			sprite.GlobalHeight,
			0,
			(sprite.IsTrigger ? facingSign : 1) * grabScaleL,
			sprite,
			36
		);
	}


	private void DrawTool_SingleHanded (PoseCharacterRenderer renderer, AngeSprite sprite) {
		bool attacking = renderer.TargetCharacter.Attackness.IsAttacking;
		int twistR = attacking && !IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		int facingSign = renderer.TargetCharacter.Movement.FacingRight ? 1 : -1;
		int grabScale = renderer.HandGrabScaleR;
		int grabRotation = renderer.HandGrabRotationR;
		int z = renderer.HandR.Z - 1;
		var toolType = renderer.TargetCharacter.EquippingToolType;
		if (toolType == ToolType.Throwing) {
			if (
				attacking &&
				Game.GlobalFrame - renderer.TargetCharacter.Attackness.LastAttackFrame > AttackDuration / 6
			) return;
			grabScale = 700;
			z = renderer.TargetCharacter.Movement.FacingFront ? renderer.HandR.Z.Abs() + 1 : -renderer.HandR.Z.Abs() - 1;
		}
		// Fix Rotation
		if (sprite.IsTrigger) {
			if (!attacking) {
				grabRotation = 0;
			} else {
				grabRotation = Util.RemapUnclamped(
					0, AttackDuration,
					facingSign * 90, 0,
					Game.GlobalFrame - renderer.TargetCharacter.Attackness.LastAttackFrame
				);
			}
		}
		// Draw
		var center = renderer.HandR.GlobalLerp(0.5f, 0.5f);
		if (toolType == ToolType.Block) {
			Renderer.Draw(
				sprite, center.x, center.y, 500, 500, grabRotation,
				sprite.GlobalWidth * grabScale / 1000,
				sprite.GlobalHeight * grabScale / 1000,
				renderer.HandR.Z + 3
			);
		} else {
			DrawToolSprite(
				renderer,
				center.x, center.y,
				sprite.GlobalWidth * twistR / 1000,
				sprite.GlobalHeight,
				grabRotation, grabScale,
				sprite, z
			);
		}
	}


	private void DrawTool_Double_Shoot (PoseCharacterRenderer renderer, AngeSprite sprite) {
		int twistR = renderer.TargetCharacter.Attackness.IsAttacking && !IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		var centerL = renderer.HandL.GlobalLerp(0.5f, 0.5f);
		var centerR = renderer.HandR.GlobalLerp(0.5f, 0.5f);
		DrawToolSprite(
			renderer,
			(centerL.x + centerR.x) / 2,
			(centerL.y + centerR.y) / 2,
			sprite.GlobalWidth * twistR / 1000,
			sprite.GlobalHeight,
			renderer.HandGrabRotationL,
			renderer.HandGrabScaleL,
			sprite,
			renderer.TargetCharacter.Movement.FacingRight ? renderer.HandR.Z + 12 : renderer.HandL.Z + 12
		);
	}


	private void DrawTool_Each (PoseCharacterRenderer renderer, AngeSprite sprite) {
		bool attacking = renderer.TargetCharacter.Attackness.IsAttacking;
		int grabScaleL = renderer.HandGrabScaleL;
		int grabScaleR = renderer.HandGrabScaleR;
		int twistL = attacking && !IgnoreGrabTwist ? renderer.HandGrabAttackTwistL : 1000;
		int twistR = attacking && !IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		int zLeft = renderer.HandL.Z - 1;
		int zRight = renderer.HandR.Z - 1;
		var centerL = renderer.HandL.GlobalLerp(0.5f, 0.5f);
		var centerR = renderer.HandR.GlobalLerp(0.5f, 0.5f);
		DrawToolSprite(
			renderer,
			centerL.x, centerL.y,
			sprite.GlobalWidth * twistL / 1000,
			sprite.GlobalHeight,
			renderer.HandGrabRotationL,
			grabScaleL, sprite,
			zLeft
		);
		DrawToolSprite(
			renderer,
			centerR.x, centerR.y,
			sprite.GlobalWidth * twistR / 1000,
			sprite.GlobalHeight,
			renderer.HandGrabRotationR,
			grabScaleR, sprite,
			zRight
		);
	}


	private void DrawTool_Pole (PoseCharacterRenderer renderer, AngeSprite sprite) {
		var centerL = renderer.HandL.GlobalLerp(0.5f, 0.5f);
		var centerR = renderer.HandR.GlobalLerp(0.5f, 0.5f);
		int twistR = renderer.TargetCharacter.Attackness.IsAttacking && !IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		DrawToolSprite(
			renderer,
			(centerL.x + centerR.x) / 2,
			(centerL.y + centerR.y) / 2,
			sprite.GlobalWidth * twistR / 1000,
			sprite.GlobalHeight,
			renderer.HandGrabRotationR,
			renderer.HandGrabScaleR,
			sprite,
			renderer.HandR.Z - 1
		);
	}


	private void DrawTool_Bow (PoseCharacterRenderer renderer, AngeSprite sprite) {
		var attack = renderer.TargetCharacter.Attackness;
		var movement = renderer.TargetCharacter.Movement;
		if (attack.IsAttacking) {
			// Attacking
			var center = (movement.FacingRight ? renderer.HandR : renderer.HandL).GlobalLerp(0.5f, 0.5f);
			int width = sprite.GlobalWidth;
			int height = sprite.GlobalHeight;
			if (!sprite.IsTrigger) {
				int localFrame = Game.GlobalFrame - attack.LastAttackFrame;
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
			DrawToolSprite(
				renderer, center.x, center.y, width, height,
				movement.FacingRight ? renderer.HandGrabRotationL : renderer.HandGrabRotationR,
				movement.FacingRight ? renderer.HandGrabScaleL : renderer.HandGrabScaleR,
				sprite, movement.FacingRight ? renderer.HandR.Z - 1 : renderer.HandL.Z - 1
			);
		} else {
			// Holding
			var center = (movement.FacingRight ? renderer.HandR : renderer.HandL).GlobalLerp(0.5f, 0.5f);
			DrawToolSprite(
				renderer, center.x, center.y,
				sprite.GlobalWidth, sprite.GlobalHeight,
				movement.FacingRight ? renderer.HandGrabRotationL : renderer.HandGrabRotationR,
				movement.FacingRight ? renderer.HandGrabScaleL : renderer.HandGrabScaleR,
				sprite,
				renderer.HandR.Z - 1
			);
		}
	}


	// API
	protected virtual Cell DrawToolSprite (PoseCharacterRenderer renderer, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) => Renderer.Draw(
		sprite,
		x, y,
		sprite.PivotX, sprite.PivotY, grabRotation,
		width * grabScale / 1000,
		height * grabScale.Abs() / 1000,
		z
	);


	public virtual bool AllowingAttack (Character character) => true;


	public virtual Bullet SpawnBullet (Character sender) => SpawnRawBullet(sender, BulletID);


	public static Bullet SpawnRawBullet (Character sender, int bulletID) {
		if (sender == null || bulletID == 0) return null;
		var rect = sender.Rect;
		if (Stage.SpawnEntity(bulletID, rect.x, rect.y) is not Bullet bullet) return null;
		bullet.Sender = sender;
		var sourceRect = sender.Rect;
		bullet.X = sourceRect.CenterX() - bullet.Width / 2;
		bullet.Y = sourceRect.CenterY() - bullet.Height / 2;
		bullet.AttackIndex = sender.Attackness.AttackStyleIndex;
		bullet.AttackCharged = sender.Attackness.LastAttackCharged;
		return bullet;
	}

}
