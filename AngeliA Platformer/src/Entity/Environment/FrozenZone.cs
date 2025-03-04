using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


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
	private static int FullscreenRenderedFrame = -1;
	private int SpawnedZ;
	private int RequireDespawnFrame = int.MaxValue;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Fullscreen = false;
		SpawnedZ = Stage.ViewZ;
		RequireDespawnFrame = int.MaxValue;
		Duration = -1;
		int gap = Const.HALF / 2;
		X -= gap;
		Y -= gap;
		Width += gap * 2;
		Height += gap * 2;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
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
			if (Game.GlobalFrame != FullscreenRenderedFrame) {
				FullscreenRenderedFrame = Game.GlobalFrame;
				DrawFrozenEffect(null);
				Game.PassEffect_Tint(new Color32(230, 245, 255, 255), 1);
			}
		} else {
			DrawFrozenEffect(Rect);
		}
	}


	private void DrawFrozenEffect (IRect? range) {
		var cameraRect = Renderer.CameraRect;
		cameraRect.width = cameraRect.width.ToUnifyGlobal();
		cameraRect.height = cameraRect.height.ToUnifyGlobal();
		var rect = range ?? cameraRect;
		byte alpha = Duration > 0 ?
			(byte)Util.LerpUnclamped(385f, 0f, (Game.GlobalFrame - SpawnFrame) / (float)Duration).Clamp(0, 200) :
			(byte)150;
		if (RequireDespawnFrame != int.MaxValue) {
			alpha = (byte)Util.Lerp(0, alpha, (RequireDespawnFrame - Game.GlobalFrame) / (float)DESPAWN_DURATION);
		}
		int count = Fullscreen ? 128 : Duration < 0 ? 16 : 32;
		int paraX = Fullscreen ? cameraRect.x / 2 : 0;
		int paraY = Fullscreen ? cameraRect.y / 2 : 0;
		int seed = X.ToUnit() * 1651243 + Y.ToUnit() * 128;
		using var _ = new UILayerScope();
		FrameworkUtil.DrawFrozenEffect(rect, alpha, count, new Int2(paraX, paraY), seed);
	}


	protected virtual void OnTouchingZone (Rigidbody rig) {
		if (rig is IWithCharacterBuff wBuff) {
			wBuff.CurrentBuff.GiveBuff(FreezeBuff.TYPE_ID, 1);
		}
	}


	#endregion




	#region --- API ---


	public static void SpreadFrozenZone (int zoneID, IRect range, int duration = 300) {
		if (Stage.SpawnEntity(zoneID, range.x, range.y) is not FrozenZone zone) return;
		zone.X = range.x;
		zone.Y = range.y;
		zone.Width = range.width;
		zone.Height = range.height;
		zone.Duration = duration;
	}


	#endregion




}
