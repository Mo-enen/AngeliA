using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(8, 0)]
public abstract class MovableBullet : Bullet {

	// Api
	public virtual int SpeedX => 42;
	public virtual int SpeedY => 0;
	public virtual Int2 AriDrag => default;
	public virtual int Gravity => 0;
	public virtual int StartRotation => 0;
	public virtual int RotateSpeed => 0;
	public virtual int EndRotation => 0;
	public virtual int EndRotationRandomRange => 180;
	public virtual int ResidueParticleID => 0;
	public virtual int ArtworkID => TypeID;
	public virtual int Scale => 1000;
	public virtual int WaterSpeedRate => 60;
	protected override int Duration => 600;
	protected override bool DestroyOnHitEnvironment => true;
	protected override bool DestroyOnHitReceiver => true;
	public int CurrentRotation { get; set; }
	public Int2 Velocity { get; set; }

	// MSG
	public override void BeforeUpdate () {
		base.BeforeUpdate();

		if (!Active) return;

		var vel = Velocity;
		if (Physics.Overlap(PhysicsMask.MAP, Rect, null, OperationMode.TriggerOnly, Tag.Water)) {
			vel.x = vel.x * WaterSpeedRate / 1000;
			vel.y = vel.y * WaterSpeedRate / 1000;
		}
		X += vel.x;
		Y += vel.y;

		if (AriDrag != default) {
			Velocity = Velocity.MoveTowards(Int2.zero, AriDrag);
		}
		if (Gravity > 0) {
			Velocity = new Int2(Velocity.x, Velocity.y - Gravity);
		}

		// Out of Range Check
		if (!Stage.ViewRect.Overlaps(Rect)) {
			Active = false;
			return;
		}

		// Oneway Check
		var hits = Physics.OverlapAll(EnvironmentMask, Rect, out int count, Sender, OperationMode.TriggerOnly);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Tag == 0 || !Util.TryGetOnewayDirection(hit.Tag, out var onewayDir)) continue;
			// Collide Check X
			bool collide = false;
			if (Velocity.x > 0) {
				int xMax = Rect.xMax;
				if (onewayDir == Direction4.Left && xMax >= hit.Rect.xMin && xMax - Velocity.x <= hit.Rect.xMin) {
					collide = true;
				}
			} else if (Velocity.x < 0) {
				int xMin = Rect.xMin;
				if (onewayDir == Direction4.Right && xMin <= hit.Rect.xMax && xMin - Velocity.x >= hit.Rect.xMax) {
					collide = true;
				}
			}
			// Collide Check Y
			if (!collide) {
				if (Velocity.y > 0) {
					int yMax = Rect.yMax;
					if (onewayDir == Direction4.Down && yMax >= hit.Rect.yMin && yMax - Velocity.y <= hit.Rect.yMin) {
						collide = true;
					}
				} else if (Velocity.y < 0) {
					int yMin = Rect.yMin;
					if (onewayDir == Direction4.Up && yMin <= hit.Rect.yMax && yMin - Velocity.y >= hit.Rect.yMax) {
						collide = true;
					}
				}
			}
			// Collide with Oneway
			if (collide) {
				if (DestroyOnHitEnvironment) {
					Active = false;
					BeforeDespawn(null);
				}
				break;
			}
		}
	}

	public override void Update () {
		int stepCount = Util.Max(Velocity.x.Abs().CeilDivide(Width), Velocity.y.Abs().CeilDivide(Height));
		if (stepCount <= 1) {
			ReceiverHitCheck(Rect);
		} else {
			var rect = Rect;
			int fromX = X - Velocity.x;
			int fromY = Y - Velocity.y;
			for (int i = 0; i < stepCount; i++) {
				rect.x = Util.RemapUnclamped(0, stepCount, fromX, X, i + 1);
				rect.y = Util.RemapUnclamped(0, stepCount, fromY, Y, i + 1);
				ReceiverHitCheck(rect);
				if (!Active) break;
			}
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (Renderer.TryGetSprite(ArtworkID, out var sprite)) {
			int facingSign = Velocity.x.Sign();
			CurrentRotation += facingSign * RotateSpeed;
			int x = X + Width / 2;
			int y = Y + Height / 2;
			if (Renderer.TryGetAnimationGroup(ArtworkID, out var aniGroup)) {
				Renderer.DrawAnimation(
					aniGroup,
					x, y,
					sprite.PivotX,
					sprite.PivotY,
					CurrentRotation,
					facingSign * sprite.GlobalWidth * Scale / 1000,
					sprite.GlobalHeight * Scale / 1000,
					Game.GlobalFrame - SpawnFrame
				);
			} else {
				Renderer.Draw(
					ArtworkID,
					x, y,
					sprite.PivotX,
					sprite.PivotY,
					CurrentRotation,
					facingSign * sprite.GlobalWidth * Scale / 1000,
					sprite.GlobalHeight * Scale / 1000
				);
			}
		}
	}

	protected override void BeforeDespawn (IDamageReceiver receiver) {

		base.BeforeDespawn(receiver);

		int particleID = ResidueParticleID != 0 ? ResidueParticleID : FreeFallParticle.TYPE_ID;
		if (Stage.SpawnEntity(particleID, X + Width / 2, Y + Height / 2) is not FreeFallParticle particle) return;

		particle.ArtworkID = ArtworkID;

		if (Renderer.TryGetSprite(ArtworkID, out var sprite)) {
			particle.Width = sprite.GlobalWidth;
			particle.Height = sprite.GlobalHeight;
		} else {
			particle.Width = Const.HALF;
			particle.Height = Const.HALF;
		}

		if (EndRotationRandomRange == -1) {
			particle.Rotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
		} else if (EndRotationRandomRange == 0) {
			particle.Rotation = EndRotation;
		} else {
			int endDelta = ((CurrentRotation - EndRotation + 180).UMod(360) - 180).Clamp(-EndRotationRandomRange, EndRotationRandomRange);
			particle.Rotation = EndRotation + endDelta;
		}
		if (Velocity.x < 0) {
			particle.Rotation += 180;
			particle.FlipX = true;
		}
		particle.CurrentSpeedX = -Velocity.x / 2;

		particle.AirDragX = 2;
		if (receiver == null) {
			// Environmnet
			particle.RotateSpeed = 0;
			particle.CurrentSpeedX = 0;
			particle.CurrentSpeedY = 0;
			particle.Gravity = 0;
		} else {
			// Receiver
			particle.RotateSpeed = 12;
			particle.CurrentSpeedY = 42;
			particle.Gravity = 5;
		}
	}

	// API
	public void StartMove (bool facingRight, int addSpeedX, int addSpeedY) {
		Velocity = new Int2(facingRight ? SpeedX + addSpeedX : -SpeedX - addSpeedX, SpeedY + addSpeedY);
		CurrentRotation = facingRight ? StartRotation : -StartRotation;
	}

}