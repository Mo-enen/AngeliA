using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// A bullet that moves after being spawned
/// </summary>
public abstract class MovableBullet : Bullet {

	// Api
	/// <summary>
	/// Movement speed to shooting direction
	/// </summary>
	public virtual int SpeedForward => 42;
	/// <summary>
	/// Movement speed to 90° of shooting direction
	/// </summary>
	public virtual int SpeedSide => 0;
	public virtual Int2 AriDrag => default;
	public virtual int Gravity => 0;
	public virtual int StartRotation => 0;
	public virtual int RotateSpeed => 0;
	/// <summary>
	/// Bullet will become this rotation after hit target
	/// </summary>
	public virtual int EndRotation => 0;
	/// <summary>
	/// Random angle applys to the end rotation
	/// </summary>
	public virtual int EndRotationRandomRange => 180;
	/// <summary>
	/// This entity spawns after bullet despawn
	/// </summary>
	public virtual int ResidueID => 0;
	/// <summary>
	/// Artwork sprite ID to render this bullet
	/// </summary>
	public virtual int ArtworkID => TypeID;
	/// <summary>
	/// Size scale for artwork only. 0 means 0%, 1000 means 100%
	/// </summary>
	public virtual int Scale => 1000;
	/// <summary>
	/// Speed multiply rate when bullet inside water. 0 means 0%, 1000 means 100%
	/// </summary>
	public virtual int WaterSpeedRate => 200;
	/// <summary>
	/// Bullet get despawn when fly longer than this range in global space
	/// </summary>
	public virtual int MaxRange => 46339;
	/// <summary>
	/// True if bullet facing right currently
	/// </summary>
	public virtual bool FacingRight => Velocity.x > 0;
	public override int Duration => 600;
	protected override int EnvironmentHitCount => 1;
	protected override int ReceiverHitCount => 1;
	public int CurrentRotation { get; set; }
	public Int2 Velocity { get; set; }
	public bool InWater { get; private set; } = false;
	/// <summary>
	/// True if the bullet already hit a receiver
	/// </summary>
	public bool HitReceiver { get; private set; } = false;

	// Data
	private int HitStartX;
	private int HitStartY;
	private int BeamLength = 0;
	private int HitEndX;
	private int HitEndY;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		BeamLength = 0;
		HitEndX = X;
		HitEndY = Y;
		HitStartX = X;
		HitStartY = Y;
		HitReceiver = false;
	}


	public override void BeforeUpdate () {

		// Life Check
		if (Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}

		if (!Active) return;

		// Out of Range Check
		if (!Stage.SpawnRect.Overlaps(Rect)) {
			Active = false;
			return;
		}

		HitStartX = X;
		HitStartY = Y;

		var vel = Velocity;
		InWater = Physics.Overlap(PhysicsMask.MAP, Rect, null, OperationMode.TriggerOnly, Tag.Water);
		if (InWater) {
			vel.x = vel.x * WaterSpeedRate / 1000;
			vel.y = vel.y * WaterSpeedRate / 1000;
		}
		X += vel.x;
		Y += vel.y;
		CurrentRotation += Velocity.x > 0 ? RotateSpeed : -RotateSpeed;

		if (AriDrag != default) {
			Velocity = Velocity.MoveTowards(Int2.Zero, AriDrag);
		}
		if (Gravity > 0) {
			Velocity = new Int2(Velocity.x, Velocity.y - Gravity);
		}

		// Hit Check
		MovableHitCheck();

		// Oneway Check
		var hits = Physics.OverlapAll(EnvironmentMask, Rect, out int count, Sender, OperationMode.TriggerOnly);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Tag == 0 || !FrameworkUtil.TryGetOnewayDirection(hit.Tag, out var onewayDir)) continue;
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
				PerformHitEnvironment(out bool requireSelfDestroy);
				if (requireSelfDestroy) {
					break;
				}
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		DrawBullet(this, ArtworkID, FacingRight, CurrentRotation, Scale);
	}


	private void MovableHitCheck () {
		int fromX = HitStartX;
		int fromY = HitStartY;
		int stepCount = Util.Max(
			(X - fromX).Abs().CeilDivide(Width),
			(Y - fromY).Abs().CeilDivide(Height)
		);
		if (stepCount <= 1) {
			base.EnvironmentHitCheck(out _);
			base.ReceiverHitCheck(out _);
			HitEndX = X;
			HitEndY = Y;
			return;
		}
		int limitedStepCount = stepCount.LessOrEquel(Util.Max(
			Stage.ViewRect.width.CeilDivide(Width),
			Stage.ViewRect.height.CeilDivide(Height)
		));
		int oldX = X, oldY = Y;
		int maxRangeSq = MaxRange * MaxRange;
		int rangeSq = 0;
		bool selfDestroy = false;
		for (int i = 0; i < limitedStepCount; i++) {
			X = Util.RemapUnclamped(0, stepCount, fromX, oldX, i + 1);
			Y = Util.RemapUnclamped(0, stepCount, fromY, oldY, i + 1);
			rangeSq = (X - fromX) * (X - fromX) + (Y - fromY) * (Y - fromY);
			if (rangeSq >= maxRangeSq) break;
			base.EnvironmentHitCheck(out bool hitE);
			base.ReceiverHitCheck(out bool hitRec);
			selfDestroy = hitRec || hitE;
			if (selfDestroy) {
				BeamLength = Util.BabylonianSqrt(rangeSq);
				HitReceiver = hitRec;
				break;
			}
		}
		HitEndX = X;
		HitEndY = Y;
		if (!selfDestroy) {
			BeamLength = Util.BabylonianSqrt(rangeSq);
		}
		X = oldX;
		Y = oldY;
	}


	protected override void BeforeDespawn (IDamageReceiver receiver) {
		base.BeforeDespawn(receiver);
		if (ResidueID == int.MinValue) return;
		if (ResidueID != 0) {
			// Custom
			Stage.SpawnEntity(ResidueID, X + Width / 2, Y + Height / 2);
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
			FrameworkUtil.InvokeObjectFreeFall(
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


	// API
	public virtual void StartMove (Direction8 dir, int speedForward, int speedSide) {
		const int SQT2 = 14142;
		HitStartX = X;
		HitStartY = Y;
		switch (dir) {

			case Direction8.Top: // ↑
				Velocity = new Int2(speedSide, speedForward);
				CurrentRotation += 90;
				break;

			case Direction8.Right: // →
				Velocity = new Int2(speedForward, speedSide);
				CurrentRotation += 0;
				break;

			case Direction8.Bottom: // ↓
				Velocity = new Int2(speedSide, -speedForward);
				CurrentRotation += -90;
				break;

			case Direction8.Left: // ←
				Velocity = new Int2(-speedForward, speedSide);
				CurrentRotation += 0;
				break;

			case Direction8.TopRight: // ↗
				Velocity = new Int2(
					speedForward * 10000 / SQT2 + speedSide * 5000 / SQT2,
					speedForward * 10000 / SQT2 - speedSide * 5000 / SQT2
				);
				CurrentRotation += -45;
				break;

			case Direction8.BottomRight: // ↘
				Velocity = new Int2(
					speedForward * 10000 / SQT2 - speedSide * 5000 / SQT2,
					-speedForward * 10000 / SQT2 - speedSide * 5000 / SQT2
				);
				CurrentRotation += 45;
				break;

			case Direction8.BottomLeft: // ↙
				Velocity = new Int2(
					-speedForward * 10000 / SQT2 + speedSide * 5000 / SQT2,
					-speedForward * 10000 / SQT2 - speedSide * 5000 / SQT2
				);
				CurrentRotation += -45;
				break;

			case Direction8.TopLeft: // ↖
				Velocity = new Int2(
					-speedForward * 10000 / SQT2 - speedSide * 5000 / SQT2,
					speedForward * 10000 / SQT2 - speedSide * 5000 / SQT2
				);
				CurrentRotation += 45;
				break;

		}

	}


	protected (int startX, int startY, int endX, int endY, int length, int rotation1000, bool beamHitReceiver) GetLastUpdatedTramsform () => (
		HitStartX + Width / 2,
		HitStartY + Height / 2,
		HitEndX + Width / 2,
		HitEndY + Height / 2,
		BeamLength,
		(Float2.SignedAngle(Float2.Up, Velocity) * 1000).RoundToInt(),
		HitReceiver
	);


	// LGC
	private static void DrawBullet (Bullet bullet, int artworkID, bool facingRight, int rotation, int scale, int z = int.MaxValue - 16) {
		if (!Renderer.TryGetSprite(artworkID, out var sprite, false)) return;
		int facingSign = facingRight ? 1 : -1;
		int x = bullet.X + bullet.Width / 2;
		int y = bullet.Y + bullet.Height / 2;
		if (Renderer.TryGetAnimationGroup(artworkID, out var aniGroup)) {
			Renderer.DrawAnimation(
				aniGroup,
				x, y,
				sprite.PivotX,
				sprite.PivotY,
				rotation,
				facingSign * sprite.GlobalWidth * scale / 1000,
				sprite.GlobalHeight * scale / 1000,
				Game.GlobalFrame - bullet.SpawnFrame,
				z
			);
		} else {
			Renderer.Draw(
				artworkID,
				x, y,
				sprite.PivotX,
				sprite.PivotY,
				rotation,
				facingSign * sprite.GlobalWidth * scale / 1000,
				sprite.GlobalHeight * scale / 1000,
				z
			);
		}
	}

}
