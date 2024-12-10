using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class IceZone : Entity {





	#region --- VAR ---


	// Api
	public static readonly int TYPE_ID = typeof(IceZone).AngeHash();
	public static event System.Action<Rigidbody, IceZone> OnTouchingIce;
	public int Duration { get; set; } = 300;

	// Data
	private IRect Range;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Bullet.OnBulletDealDamage += OnBulletDealDamage;
		Bullet.OnBulletHitEnvironment += OnBulletHitEnvironment;
		static void OnBulletDealDamage (Bullet bullet, IDamageReceiver receiver, Tag damageType) {
			if (!damageType.HasAll(Tag.IceDamage)) return;
			var range = bullet.Rect.Expand(Const.HALF);
			SpreadIce(TYPE_ID, range);
			Fire.PutoutFire(range);
		}
		static void OnBulletHitEnvironment (Bullet bullet, Tag damageType) {
			if (!damageType.HasAll(Tag.IceDamage)) return;
			var range = bullet.Rect.Expand(Const.HALF);
			SpreadIce(TYPE_ID, range);
			Fire.PutoutFire(range);
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		Range = default;
		if (FromWorld) {
			Duration = -1;
			int grow = Const.HALF;
			X -= grow;
			Y -= grow;
			Width += grow * 2;
			Height += grow * 2;
		} else {
			Duration = 300;
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.Slip);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();

		// Lifetime Check
		if (Duration >= 0 && Game.GlobalFrame - SpawnFrame > Duration) {
			Active = false;
			return;
		}

		// Touch Rigidbody
		var hits = Physics.OverlapAll(PhysicsMask.SOLID, Rect, out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is Rigidbody rig) {
				OnTouchingIce?.Invoke(rig, this);
			} else if (hit.Entity is Fire) {
				Active = false;
				return;
			}
		}

		// Tint Blocks Overlapping
		int minX = X;
		int minY = Y;
		int maxX = X + Width;
		int maxY = Y + Height;
		using (new LayerScope(RenderLayer.COLOR)) {
			byte alpha = Duration > 0 ?
				(byte)Util.LerpUnclamped(512f, 0f, (Game.GlobalFrame - SpawnFrame) / (float)Duration).Clamp(0, 64) :
				(byte)64;
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity == null || hit.Entity is IBlockEntity) {
					var rect = hit.Rect;
					if (hit.Entity == null) {
						rect = new IRect(rect.x.ToUnifyGlobal(), rect.y.ToUnifyGlobal(), Const.CEL, Const.CEL);
					}
					minX = Util.Min(minX, rect.x);
					minY = Util.Min(minY, rect.y);
					maxX = Util.Max(maxX, rect.xMax);
					maxY = Util.Max(maxY, rect.yMax);
				}
			}
		}
		Range = IRect.MinMaxRect(minX, minY, maxX, maxY);

	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		// Draw Particle Effect
		int seed = SpawnFrame + X + Y * 128 + InstanceOrder * 347345634;
		if (Renderer.TryGetSprite(Const.PIXEL, out var sprite, true)) {
			int left = Range.x;
			int right = Range.xMax;
			int down = Range.y;
			int height = Range.height;
			int frame = Game.GlobalFrame - SpawnFrame;
			byte alpha = Duration > 0 ?
				(byte)Util.LerpUnclamped(385f, 0f, (Game.GlobalFrame - SpawnFrame) / (float)Duration).Clamp(0, 200) :
				(byte)150;
			var tint = new Color32(200, 225, 255, alpha);
			int COUNT = Duration < 0 ? 16 : 32;
			float frame01 = frame / 120f;
			float fixedFrame01 = frame01 * Const.CEL / height;
			for (int i = 0; i < COUNT; i++) {
				float lerp01 = i / (float)COUNT;
				if (lerp01 > frame01) break;
				if (Util.QuickRandom(0, 100) < 30) continue;
				int x = Util.QuickRandomWithSeed(seed + i * 21632, left, right);
				int y = down + (((fixedFrame01 + lerp01) % 1f) * height).RoundToInt();
				int size = Util.QuickRandomWithSeed(seed + i * 1673 + TypeID, 16, 142);
				int rot = Util.QuickRandom(0, 360);
				Renderer.Draw(sprite, x, y, 500, 500, rot, size, size / 7, tint);
			}
		}
	}


	#endregion




	#region --- API ---


	public static void SpreadIce (int iceID, IRect range) {
		if (Stage.SpawnEntity(iceID, range.x, range.y) is not IceZone ice) return;
		ice.Width = range.width;
		ice.Height = range.height;
	}


	#endregion




	#region --- LGC ---



	#endregion




}
