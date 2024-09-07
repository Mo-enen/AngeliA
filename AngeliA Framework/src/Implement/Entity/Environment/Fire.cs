using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class CommonFire : Fire {
	public static readonly int TYPE_ID = typeof(CommonFire).AngeHash();
}


public interface ICombustible {
	public bool IsBurning => this is Entity e && e.SpawnFrame <= BurnStartFrame;
	public int BurnedDuration => 120;
	public int BurnStartFrame { get; set; }
	public void OnBurned () {
		if (this is not Entity e) return;
		FrameworkUtil.RemoveFromWorldMemory(e);
	}
}


[EntityAttribute.DontDrawBehind]
public abstract class Fire : Entity {




	#region --- VAR ---


	// Api
	protected virtual int WeakenDuration => 22;
	protected virtual int SpreadDuration => 60;
	protected virtual int SpreadRange => Const.CEL;
	protected virtual bool UseAdditiveShader => false;
	protected virtual int DamageCooldown => 30;
	protected virtual Direction4 DefaultDirection => Direction4.Up;
	protected virtual int IlluminateUnitRadius => 6;
	protected virtual int IlluminateAmount => 1000;
	public Direction4 Direction { get; set; } = Direction4.Up;

	// Data
	private ICombustible Target = null;
	private int LifeEndFrame = 0;
	private int BurnedFrame = 0;
	private int SpreadFrame = int.MaxValue;
	private int DamageCharacterStartFrame = -1;
	private bool ManuallyPutout = false;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Target = null;
		LifeEndFrame = 0;
		BurnedFrame = 0;
		SpreadFrame = Game.GlobalFrame + SpreadDuration;
		Direction = DefaultDirection;
	}


	public override void OnInactivated () {
		base.OnInactivated();
		Target = null;
		LifeEndFrame = 0;
		BurnedFrame = 0;
		SpreadFrame = 0;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Deal Damage to Characters in All Teams
		bool damaged = false;
		if (Game.GlobalFrame < BurnedFrame) {
			var hits = Physics.OverlapAll(
				PhysicsMask.CHARACTER, Rect, out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Character ch) continue;
				damaged = true;
				if (DamageCharacterStartFrame < 0) {
					DamageCharacterStartFrame = Game.GlobalFrame;
				} else if (Game.GlobalFrame > DamageCharacterStartFrame + DamageCooldown) {
					DamageCharacterStartFrame = Game.GlobalFrame;
					ch.TakeDamage(new Damage(1, this, this, Tag.FireDamage));
				}
			}
		}
		if (!damaged) {
			DamageCharacterStartFrame = -1;
		}
	}


	public override void Update () {
		base.Update();

		if (Target != null && Direction != Direction4.Up) Direction = Direction4.Up;
		var eTarget = Target as Entity;

		// Put Out When Target Not Burning
		if (eTarget != null && !Target.IsBurning) {
			Putout(false);
			Active = false;
			return;
		}

		// Put Out when Hit Water
		if (Physics.Overlap(PhysicsMask.MAP, Rect, this, OperationMode.TriggerOnly, Tag.Water)) {
			Putout(true);
			Active = false;
			return;
		}

		// Wild Fire from Map Editor
		if (eTarget == null && FromWorld) {
			Spread();
			Active = false;
			return;
		}

		// Life Time Check
		if (Game.GlobalFrame >= LifeEndFrame) {
			Active = false;
			return;
		}

		// Spread
		if (Game.GlobalFrame >= SpreadFrame) {
			Spread();
			SpreadFrame += SpreadDuration;
		}

		if (eTarget != null) {
			if (eTarget.Active) {
				// Burned Check
				if (Game.GlobalFrame == BurnedFrame) {
					Target.OnBurned();
					eTarget.Active = false;
				}
				// Follow Target
				X = eTarget.X;
				Y = eTarget.Y;
			} else {
				if (BurnedFrame > Game.GlobalFrame) {
					Putout(false);
				}
			}
		}

	}


	public override void LateUpdate () {

		base.LateUpdate();
		if (!Active) return;

		// Putout Smoke
		if (ManuallyPutout) {
			GlobalEvent.InvokeFirePutout(TypeID, Rect);
			Active = false;
			return;
		}

		if (UseAdditiveShader) Renderer.SetLayerToAdditive();
		var cell = Renderer.Draw(
			TypeID,
			X + (Direction == Direction4.Left ? Width : Direction == Direction4.Right ? 0 : Width / 2),
			Y + (Direction == Direction4.Down ? Height : Direction == Direction4.Up ? 0 : Height / 2),
			500, 0,
			Direction.GetRotation(),
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			z: int.MaxValue
		);
		Renderer.SetLayerToDefault();

		// Fit Size to Target
		if (Target is Entity eTarget && cell.Width != eTarget.Width) {
			cell.Height = cell.Height * eTarget.Width / cell.Width;
			cell.Width = eTarget.Width;
		}

		// Illuminance
		LightingSystem.Illuminate(X.ToUnit(), Y.ToUnit(), IlluminateUnitRadius, IlluminateAmount);

		// Animation
		const int HOP_GAP = 8;
		const int FIERCE_GAP = 8;
		if (Game.GlobalFrame < SpawnFrame + FIERCE_GAP) {
			// Fierce
			int localFrame = Game.GlobalFrame - SpawnFrame;
			if (localFrame % 3 != 0) {
				cell.Width = Util.RemapUnclamped(
					0, FIERCE_GAP,
					cell.Width, cell.Width * 3 / 2,
					localFrame
				);
				cell.Height = Util.RemapUnclamped(
					0, FIERCE_GAP,
					cell.Height * 3 / 2, cell.Height,
					localFrame
				);
			} else {
				cell.Width = cell.Height = 0;
			}
		} else if (Game.GlobalFrame > BurnedFrame) {
			// Weaken
			int weakenFrame = Game.GlobalFrame - BurnedFrame;
			int weakenDuration = LifeEndFrame - BurnedFrame;
			if (weakenFrame < weakenDuration - HOP_GAP) {
				// Weaken
				float lerp01 = Ease.InOutElastic(Util.InverseLerpUnclamped(0, weakenDuration, weakenFrame));
				cell.Width = Util.LerpUnclamped(cell.Width, 0, lerp01).RoundToInt();
				cell.Height = Util.LerpUnclamped(cell.Height, 0, lerp01).RoundToInt();
			} else {
				// Hop
				cell.Width = weakenFrame % 3 == 0 ? 0 : cell.Width * 2 / 3;
				cell.Height = weakenFrame % 3 == 0 ? 0 : cell.Height * 2 / 3;
			}
		}
	}


	#endregion




	#region --- API ---


	public static void SpreadFire (int fireID, IRect rect, Entity host = null) {
		var hits = Physics.OverlapAll(
			PhysicsMask.ENTITY,
			rect, out int count,
			host, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not ICombustible com || !hit.Entity.Active || com.IsBurning) continue;
			if (Stage.TrySpawnEntity(fireID, hit.Rect.x, hit.Rect.y, out var fEntity) && fEntity is Fire fire) {
				fire.Setup(com);
			}
		}
	}


	public static void PutoutFire (IRect rect) {
		var hits = Physics.OverlapAll(
			PhysicsMask.ENVIRONMENT,
			rect, out int count,
			null, OperationMode.TriggerOnly
		);
		for (int i = 0; i < count; i++) {
			var e = hits[i].Entity;
			if (e is not Fire fire) continue;
			fire.Putout(true);
		}
	}


	public void Setup (int burnDuration, Direction4 direction, int width = Const.CEL, int height = Const.CEL) {
		Width = width;
		Height = height;
		BurnedFrame = Game.GlobalFrame + burnDuration;
		LifeEndFrame = BurnedFrame + WeakenDuration;
		Target = null;
		Direction = direction;
		ManuallyPutout = false;
	}


	public void Setup (ICombustible com) {
		if (com == null) return;
		if (com is Entity entity) {
			Width = entity.Width;
			Height = entity.Height;
		}
		BurnedFrame = Game.GlobalFrame + com.BurnedDuration;
		LifeEndFrame = BurnedFrame + WeakenDuration;
		Direction = Direction4.Up;
		Target = com;
		com.BurnStartFrame = Game.GlobalFrame;
		ManuallyPutout = false;
	}


	public void Spread () => SpreadFire(TypeID, Rect.Expand(SpreadRange), host: this);


	public void Putout (bool manually) {
		BurnedFrame = Game.GlobalFrame;
		LifeEndFrame = Game.GlobalFrame + WeakenDuration;
		SpreadFrame = int.MaxValue;
		ManuallyPutout = manually;
		if (Target != null) {
			Target.BurnStartFrame = -1;
		}
	}


	#endregion




}