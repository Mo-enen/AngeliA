using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class EmptyMovableBullet : MovableBullet { }


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
	public virtual int WaterSpeedRate => 200;
	public virtual int MaxRange => 46339;
	protected override int Duration => 600;
	protected override bool DestroyOnHitEnvironment => true;
	protected override bool DestroyOnHitReceiver => true;
	public int CurrentRotation { get; set; }
	public Int2 Velocity { get; set; }
	public bool InWater { get; private set; } = false;

	// Data
	private int BeamStartX;
	private int BeamStartY;
	private int BeamLength = 0;
	private int BeamEndX;
	private int BeamEndY;
	private bool BeamHitReceiver = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		BeamLength = 0;
		BeamEndX = X;
		BeamEndY = Y;
		BeamStartX = X;
		BeamStartY = Y;
		BeamHitReceiver = false;
	}

	public override void BeforeUpdate () {

		// Life Check
		if (Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}

		if (!Active) return;

		var vel = Velocity;
		InWater = Physics.Overlap(PhysicsMask.MAP, Rect, null, OperationMode.TriggerOnly, Tag.Water);
		if (InWater) {
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

		// Hit Check
		MovableHitCheck();

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

	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
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
		if (ResidueParticleID != 0) {
			// Custom
			Stage.SpawnEntity(ResidueParticleID, X + Width / 2, Y + Height / 2);
		} else {
			// Default
			int rot =
				EndRotationRandomRange == -1 ? Util.QuickRandom(0, 360) :
				EndRotationRandomRange == 0 ? EndRotation :
				EndRotation + ((CurrentRotation - EndRotation + 180).UMod(360) - 180).Clamp(-EndRotationRandomRange, EndRotationRandomRange);
			rot += Velocity.x < 0 ? 180 : 0;
			int rotSpeed = receiver == null ? 0 : 12;
			int speedY = receiver == null ? 0 : 42;
			int gravity = receiver == null ? 0 : 5;
			GlobalEvent.InvokeObjectFreeFall(
				ArtworkID,
				x: X + Width / 2,
				y: Y + Height / 2,
				speedX: 0,
				speedY: speedY,
				rotation: rot,
				rotationSpeed: rotSpeed,
				gravity: gravity,
				flipX: Velocity.x < 0
			);
		}
	}

	private bool MovableHitCheck () {
		bool selfDestroy = false;
		int stepCount = Util.Max(
			(X - BeamStartX).Abs().CeilDivide(Width),
			(Y - BeamStartY).Abs().CeilDivide(Height)
		);
		if (stepCount <= 1) {
			selfDestroy = base.EnvironmentHitCheck();
			selfDestroy = base.ReceiverHitCheck() || selfDestroy;
			return selfDestroy;
		}
		int limitedStepCount = stepCount.LessOrEquel(Util.Max(
			Stage.ViewRect.width.CeilDivide(Width),
			Stage.ViewRect.height.CeilDivide(Height)
		));
		int oldX = X, oldY = Y;
		int fromX = BeamStartX;
		int fromY = BeamStartY;
		int maxRangeSq = MaxRange * MaxRange;
		int rangeSq = 0;
		for (int i = 0; i < limitedStepCount; i++) {
			X = Util.RemapUnclamped(0, stepCount, fromX, oldX, i + 1);
			Y = Util.RemapUnclamped(0, stepCount, fromY, oldY, i + 1);
			rangeSq = (X - fromX) * (X - fromX) + (Y - fromY) * (Y - fromY);
			if (rangeSq >= maxRangeSq) break;
			selfDestroy = base.EnvironmentHitCheck();
			bool hitRec = base.ReceiverHitCheck();
			selfDestroy = hitRec || selfDestroy;
			if (selfDestroy) {
				BeamLength = Util.BabylonianSqrt(rangeSq);
				BeamHitReceiver = hitRec;
				break;
			}
		}
		BeamEndX = X;
		BeamEndY = Y;
		if (!selfDestroy) {
			BeamLength = Util.BabylonianSqrt(rangeSq);
		}
		X = oldX;
		Y = oldY;
		return selfDestroy;
	}

	// API
	public virtual void StartMove (Direction8 dir, int speedX, int speedY) {
		const int SQT2 = 14142;
		BeamStartX = X;
		BeamStartY = Y;
		switch (dir) {

			case Direction8.Top: // ↑
				Velocity = new Int2(speedY, speedX);
				CurrentRotation = StartRotation - 90;
				break;
			case Direction8.Right: // →
				Velocity = new Int2(speedX, speedY);
				CurrentRotation = StartRotation;
				break;
			case Direction8.Bottom: // ↓
				Velocity = new Int2(speedY, -speedX);
				CurrentRotation = StartRotation + 90;
				break;
			case Direction8.Left: // ←
				Velocity = new Int2(-speedX, speedY);
				CurrentRotation = -StartRotation;
				break;

			case Direction8.TopRight: // ↗
				Velocity = new Int2(
					speedX * 10000 / SQT2 - speedY * 5000 / SQT2,
					speedX * 10000 / SQT2 + speedY * 5000 / SQT2
				);
				CurrentRotation = StartRotation - 45;
				break;
			case Direction8.BottomRight: // ↘
				Velocity = new Int2(
					speedX * 10000 / SQT2 - speedY * 5000 / SQT2,
					-speedX * 10000 / SQT2 + speedY * 5000 / SQT2
				);
				CurrentRotation = StartRotation + 45;
				break;
			case Direction8.BottomLeft: // ↙
				Velocity = new Int2(
					-speedX * 10000 / SQT2 + speedY * 5000 / SQT2,
					-speedX * 10000 / SQT2 - speedY * 5000 / SQT2
				);
				CurrentRotation = -StartRotation - 45;
				break;
			case Direction8.TopLeft: // ↖
				Velocity = new Int2(
					-speedX * 10000 / SQT2 + speedY * 5000 / SQT2,
					speedX * 10000 / SQT2 - speedY * 5000 / SQT2
				);
				CurrentRotation = -StartRotation + 45;
				break;

		}
	}

	protected (int x, int y, int endX, int endY, int length, int rotation1000, bool beamHitReceiver) GetLastBeamTramsform () => (
		BeamStartX + Width / 2,
		BeamStartY + Height / 2,
		BeamEndX,
		BeamEndY,
		BeamLength,
		(-Float2.SignedAngle(Float2.up, Velocity) * 1000).RoundToInt(),
		BeamHitReceiver
	);

}
