using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
public class FrozenZone : Entity {





	#region --- VAR ---


	// Api
	private const int DESPAWN_DURATION = 30;
	public static readonly int TYPE_ID = typeof(FrozenZone).AngeHash();
	public int Duration { get; set; } = 300;
	public bool Fullscreen { get; set; } = false;

	// Data
	private static int FullscreenUpdateFrame = -1;
	private int SpawnedZ;
	private int RequireDespawnFrame = int.MaxValue;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Bullet.OnBulletDealDamage += OnBulletDealDamage;
		Bullet.OnBulletHitEnvironment += OnBulletHitEnvironment;
		static void OnBulletDealDamage (Bullet bullet, IDamageReceiver receiver, Tag damageType) {
			if (!damageType.HasAll(Tag.IceDamage)) return;
			var range = bullet.Rect.Expand(Const.HALF);
			SpreadFrozenZone(TYPE_ID, range);
			IFire.PutoutFire(range);
		}
		static void OnBulletHitEnvironment (Bullet bullet, Tag damageType) {
			if (!damageType.HasAll(Tag.IceDamage)) return;
			var range = bullet.Rect.Expand(Const.HALF);
			SpreadFrozenZone(TYPE_ID, range);
			IFire.PutoutFire(range);
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		Duration = 300;
		Fullscreen = false;
		SpawnedZ = Stage.ViewZ;
		RequireDespawnFrame = int.MaxValue;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();

		if (RequireDespawnFrame == int.MaxValue) {

			// Lifetime Check
			// Z Check
			// Out of Range Check
			if (
				(Duration >= 0 && Game.GlobalFrame - SpawnFrame > Duration) ||
				Stage.ViewZ != SpawnedZ ||
				!Rect.Overlaps(Stage.SpawnRect)
			) {
				RequireDespawnFrame = Game.GlobalFrame + DESPAWN_DURATION;
			}

			// Touch Rigidbody
			if (Fullscreen) {
				// Fullscreen
				if (Game.GlobalFrame != FullscreenUpdateFrame) {
					FullscreenUpdateFrame = Game.GlobalFrame;
					FullscreenTouch(EntityLayer.ENVIRONMENT, this);
					FullscreenTouch(EntityLayer.CHARACTER, this);
					static void FullscreenTouch (int entityLayer, FrozenZone zone) {
						Stage.TryGetEntities(entityLayer, out var entities, out int count);
						for (int i = 0; i < count; i++) {
							var entity = entities[i];
							if (entity is Rigidbody rig) {
								zone.OnTouchingZone(rig);
							} else if (entity is IFire) {
								zone.RequireDespawnFrame = Game.GlobalFrame + DESPAWN_DURATION;
								return;
							}
						}
					}
				}
			} else {
				// Ranged
				var hits = Physics.OverlapAll(PhysicsMask.ENTITY, Rect, out int count, this, OperationMode.ColliderAndTrigger);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is Rigidbody rig) {
						OnTouchingZone(rig);
					} else if (hit.Entity is IFire) {
						RequireDespawnFrame = Game.GlobalFrame + DESPAWN_DURATION;
						break;
					}
				}
			}

		} else {
			// Performing Despawn
			if (Game.GlobalFrame >= RequireDespawnFrame) {
				Active = false;
				return;
			}
		}

	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		if (Fullscreen) {
			DrawFrozenEffect(null);
			Game.PassEffect_Tint(new Color32(230, 245, 255, 255), 1);
		} else {
			DrawFrozenEffect(Rect);
		}
	}


	private void DrawFrozenEffect (IRect? range) {

		if (!Renderer.TryGetSprite(Const.PIXEL, out var sprite, true)) return;

		var cameraRect = Renderer.CameraRect;
		var rect = range ?? cameraRect;
		int left = rect.x;
		int down = rect.y;
		int width = rect.width;
		int height = rect.height;

		int seed = SpawnFrame + X + Y * 128 + InstanceOrder * 347345634;
		int frame = Game.GlobalFrame - SpawnFrame;
		byte alpha = Duration > 0 ?
			(byte)Util.LerpUnclamped(385f, 0f, (Game.GlobalFrame - SpawnFrame) / (float)Duration).Clamp(0, 200) :
			(byte)150;
		if (RequireDespawnFrame != int.MaxValue) {
			alpha = (byte)Util.Lerp(0, alpha, (RequireDespawnFrame - Game.GlobalFrame) / (float)DESPAWN_DURATION);
		}

		var tint = new Color32(200, 225, 255, alpha);
		int COUNT = Fullscreen ? 128 : Duration < 0 ? 16 : 32;
		float frame01 = frame / 120f;
		float fixedFrame01 = frame01 * Const.CEL / height;
		int paraX = Fullscreen ? cameraRect.x / 2 : 0;
		int paraY = Fullscreen ? cameraRect.y / 2 : 0;
		for (int i = 0; i < COUNT; i++) {
			float lerp01 = i / (float)COUNT;
			if (lerp01 > frame01) break;
			if (Util.QuickRandom(0, 100) < 30) continue;
			int x = left + (Util.QuickRandomWithSeed(seed + i * 21632, 0, width) - paraX).UMod(width);
			int y = down + (((fixedFrame01 + lerp01) * height).RoundToInt() - paraY).UMod(height);
			int size = Util.QuickRandomWithSeed(seed + i * 1673 + TypeID, 16, 142);
			int rot = Util.QuickRandom(0, 360);
			Renderer.Draw(sprite, x, y, 500, 500, rot, size, size / 7, tint);
		}
	}


	protected virtual void OnTouchingZone (Rigidbody rig) {
		if (rig is IWithCharacterBuff wBuff) {
			wBuff.CurrentBuff.GiveBuff(FreezeBuff.TYPE_ID, 1);
		}
	}


	#endregion




	#region --- API ---


	public static void SpreadFrozenZone (int zoneID, IRect range) {
		if (Stage.SpawnEntity(zoneID, range.x, range.y) is not FrozenZone zone) return;
		zone.Width = range.width;
		zone.Height = range.height;
	}


	#endregion




}
