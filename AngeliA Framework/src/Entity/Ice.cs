using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class Ice : Entity {





	#region --- VAR ---


	// Api
	public static readonly int TYPE_ID = typeof(Ice).AngeHash();
	public static event System.Action<Rigidbody, Ice> OnTouchingIce;
	public int Duration { get; set; } = 120;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Bullet.OnBulletDealDamage += OnBulletDealDamage;
		Bullet.OnBulletHitEnvironment += OnBulletHitEnvironment;
		static void OnBulletDealDamage (Bullet bullet, IDamageReceiver receiver, Tag damageType) {
			if (!damageType.HasAll(Tag.IceDamage)) return;
			SpreadIce(TYPE_ID, bullet.Rect.Expand(Const.HALF));

		}
		static void OnBulletHitEnvironment (Bullet bullet, Tag damageType) {
			if (!damageType.HasAll(Tag.IceDamage)) return;
			SpreadIce(TYPE_ID, bullet.Rect.Expand(Const.HALF));
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.Slip);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();

		// Lifetime Check
		if (Game.GlobalFrame - SpawnFrame > Duration) {
			Active = false;
			return;
		}

		// Touch Rigidbody
		var hits = Physics.OverlapAll(PhysicsMask.SOLID, Rect, out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is Rigidbody rig) {
				OnTouchingIce?.Invoke(rig, this);
			}
			if (
				(hit.Entity == null || hit.Entity is IBlockEntity) &&
				hit.SourceID != 0 &&
				Renderer.TryGetSprite(hit.SourceID, out var blockSP, true)
			) {
				var rect = hit.Rect;
				if (hit.Entity == null) {
					rect = new IRect(rect.x.ToUnifyGlobal(), rect.y.ToUnifyGlobal(), Const.CEL, Const.CEL);
				}
				Renderer.Draw(blockSP, rect, new Color32(142, 200, 220, 255), z: int.MaxValue);
			}
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		// Draw Particle




	}


	#endregion




	#region --- API ---


	public static void SpreadIce (int iceID, IRect range) {
		if (Stage.SpawnEntity(iceID, range.x, range.y) is not Ice ice) return;
		ice.Width = range.width;
		ice.Height = range.height;
	}


	#endregion




	#region --- LGC ---



	#endregion




}
