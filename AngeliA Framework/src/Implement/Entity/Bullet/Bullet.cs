using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(8 * Const.TEAM_COUNT, 0)]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Bullet : Entity {

	// SUB
	private class BulletTrack {
		public int Frame = -2;
		public int Count = 0;
		public int Capacity;
		public void AddCount () {
			int frame = Game.GlobalFrame;
			if (frame != Frame) {
				Frame = frame;
				Count = 1;
			} else {
				Count++;
			}
		}
		public int GetCount () => Game.GlobalFrame == Frame ? Count : 0;
	}

	// Api
	protected virtual int EnvironmentMask => PhysicsMask.MAP;
	protected virtual int ReceiverMask => PhysicsMask.ENTITY;
	protected virtual int Duration => 60;
	protected virtual int Damage => 1;
	protected virtual Tag DamageType => Tag.PhysicalDamage;
	protected virtual int EnvironmentHitCount => int.MaxValue;
	protected virtual int ReceiverHitCount => int.MaxValue;
	protected virtual bool RoundHitCheck => false;
	public Entity Sender { get; set; } = null;
	public int AttackIndex { get; set; } = 0;
	public bool AttackCharged { get; set; } = false;

	// Data
	private static readonly Dictionary<int, BulletTrack[]> TrackPool = new();
	private readonly BulletTrack[] Track;
	private int CurrentEnvironmentHitCount;
	private int CurrentReceiverHitCount;
	private int TargetTeam;

	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		TrackPool.Clear();
		foreach (var type in typeof(Bullet).AllChildClass()) {
			int id = type.AngeHash();
			if (TrackPool.ContainsKey(id)) continue;
			int capacity = Stage.GetEntityCapacity(id) / Const.TEAM_COUNT;
			var tracks = new BulletTrack[Const.TEAM_COUNT].FillWithNewValue();
			for (int i = 0; i < tracks.Length; i++) {
				tracks[i].Capacity = capacity;
			}
			TrackPool.Add(id, tracks);
		}
	}

	public Bullet () {
		if (!TrackPool.TryGetValue(TypeID, out Track)) {
			int capacity = Stage.GetEntityCapacity(TypeID) / Const.TEAM_COUNT;
			var tracks = new BulletTrack[Const.TEAM_COUNT].FillWithNewValue();
			for (int i = 0; i < tracks.Length; i++) {
				tracks[i].Capacity = capacity;
			}
			TrackPool.Add(TypeID, tracks);
#if DEBUG
			Debug.LogWarning($"Bullet {GetType().Name} do not init it's track from static init func. This should not happen.");
#endif
		}
	}

	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL;
		Height = Const.CEL;
		Sender = null;
		CurrentEnvironmentHitCount = EnvironmentHitCount;
		CurrentReceiverHitCount = ReceiverHitCount;
		TargetTeam = Const.TEAM_ALL;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		TargetTeam = Sender is Character chSender ? chSender.AttackTargetTeam : Const.TEAM_ALL;
		// Team Track Check
		var track = Track[Const.GetTeamIndex(TargetTeam)];
		if (track.GetCount() >= track.Capacity) {
			Active = false;
			return;
		}
		track.AddCount();
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Life Check
		if (!Active || Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}
		// Environment Hit Check
		EnvironmentHitCheck();
	}

	public override void Update () {
		base.Update();
		if (!Active) return;
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
			// Gate
			if (hit.Entity is not IDamageReceiver receiver) continue;
			if ((receiver.Team & TargetTeam) != receiver.Team) continue;
			var fixedDamageType = DamageType & ~receiver.IgnoreDamageType;
			if (fixedDamageType == Tag.None) continue;
			if (receiver is Entity e && !e.Active) continue;
			// Round Shape Gate
			if (RoundHitCheck) {
				int dis = Util.DistanceInt(rect.CenterX(), rect.CenterY(), hit.Rect.CenterX(), hit.Rect.CenterY());
				int rad = (Width.Abs() + Height.Abs()) / 4;
				int hitRad = (hit.Rect.width.Abs() + hit.Rect.height.Abs()) / 4;
				if (dis > rad + hitRad) continue;
			}
			// Perform Damage
			PerformDamage(receiver, fixedDamageType);
			// Destroy Check
			requireSelfDestroy = PerformHitReceiver(receiver) || requireSelfDestroy;
		}
		return requireSelfDestroy;
	}

	/// <returns>True if the bullet need to self destroy</returns>
	protected virtual bool EnvironmentHitCheck () {
		if (Physics.Overlap(EnvironmentMask, Rect, Sender)) {
			return PerformHitEnvironment();
		}
		return false;
	}

	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }

	protected virtual void PerformDamage (IDamageReceiver receiver, Tag damageType) {
		// Perform Damage
		receiver.TakeDamage(new Damage(Damage, Sender, this, damageType));
		// Fire Logic
		if (damageType.HasAll(Tag.FireDamage)) {
			Fire.SpreadFire(CommonFire.TYPE_ID, Rect.Expand(Const.CEL));
		}
	}

	protected bool PerformHitEnvironment () {
		CurrentEnvironmentHitCount--;
		if (CurrentEnvironmentHitCount <= 0) {
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

	protected bool PerformHitReceiver (IDamageReceiver receiver) {
		CurrentReceiverHitCount--;
		if (CurrentReceiverHitCount <= 0) {
			Active = false;
			BeforeDespawn(receiver);
			return true;
		}
		return false;
	}

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

}