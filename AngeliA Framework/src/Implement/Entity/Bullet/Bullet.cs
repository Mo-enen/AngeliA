using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(128)]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Bullet : Entity {

	// Api
	protected virtual int EnvironmentMask => PhysicsMask.MAP;
	protected virtual int ReceiverMask => PhysicsMask.ENTITY;
	protected virtual int Duration => 60;
	protected virtual int Damage => 1;
	protected virtual Tag DamageType => Tag.PhysicalDamage;
	protected virtual int SpawnWidth => Const.CEL;
	protected virtual int SpawnHeight => Const.CEL;
	protected virtual bool DestroyOnHitEnvironment => false;
	protected virtual bool DestroyOnHitReceiver => false;
	protected virtual bool RoundHitCheck => false;
	public Entity Sender { get; set; } = null;
	public int AttackIndex { get; set; } = 0;
	public bool AttackCharged { get; set; } = false;
	public int TargetTeam { get; set; } = Const.TEAM_ALL;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Width = SpawnWidth;
		Height = SpawnHeight;
		Sender = null;
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Life Check
		if (Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}
		// Environment Hit Check
		EnvironmentHitCheck();
	}

	public override void Update () {
		base.Update();
		ReceiverHitCheck();
	}

	// Api
	/// <returns>True if the bullet need to self destroy</returns>
	protected virtual bool ReceiverHitCheck () {
		var rect = Rect;
		bool requireSelfDestroy = false;
		var hits = Physics.OverlapAll(
			ReceiverMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IDamageReceiver receiver) continue;
			if ((receiver.Team & TargetTeam) != receiver.Team) continue;
			if (receiver is Entity e && !e.Active) continue;
			if (RoundHitCheck) {
				int dis = Util.DistanceInt(rect.CenterX(), rect.CenterY(), hit.Rect.CenterX(), hit.Rect.CenterY());
				int rad = (Width.Abs() + Height.Abs()) / 4;
				int hitRad = (hit.Rect.width.Abs() + hit.Rect.height.Abs()) / 4;
				if (dis > rad + hitRad) continue;
			}
			receiver.TakeDamage(new Damage(Damage, Sender, this, DamageType));
			// Type Logic
			switch (DamageType) {
				case Tag.FireDamage:
					Fire.SpreadFire(CommonFire.TYPE_ID, Rect.Expand(Const.CEL));
					break;
			}
			if (DestroyOnHitReceiver) {
				Active = false;
				requireSelfDestroy = true;
				BeforeDespawn(receiver);
			}
		}
		return requireSelfDestroy;
	}

	/// <returns>True if the bullet need to self destroy</returns>
	protected virtual bool EnvironmentHitCheck () {
		if (DestroyOnHitEnvironment && Physics.Overlap(EnvironmentMask, Rect, Sender)) {
			Active = false;
			BeforeDespawn(null);
			switch (DamageType) {
				case Tag.FireDamage:
					Fire.SpreadFire(CommonFire.TYPE_ID, Rect.Expand(Const.CEL));
					break;
			}
			return true;
		}
		return false;
	}

	public bool GroundCheck (out Color32 groundTint) {
		groundTint = Color32.WHITE;
		bool grounded =
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out var hit, Sender) ||
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out hit, Sender, OperationMode.TriggerOnly, Tag.OnewayUp);
		if (grounded && Renderer.TryGetSprite(hit.SourceID, out var groundSprite)) {
			groundTint = groundSprite.SummaryTint;
		}
		return grounded;
	}

	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }

	protected static void DrawBullet (Bullet bullet, int artworkID, bool facingRight, int rotation, int scale, int z = int.MaxValue - 16) {
		if (!Renderer.TryGetSprite(artworkID, out var sprite)) return;
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