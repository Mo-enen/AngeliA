using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.RepositionWhenInactive]
public class Fire : Entity, IFire {




	#region --- VAR ---

	// Api
	public static readonly int TYPE_ID = typeof(Fire).AngeHash();
	public static event System.Action<int, IRect> OnFirePutout;
	protected virtual int PowerAmount => 1000;
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
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, isTrigger: true, tag: Tag.FireDamage);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();

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
			// Spread into Other Fires
			Spread();
			// Remove from Map
			Active = false;
			IgnoreReposition = true;
			FrameworkUtil.RemoveFromWorldMemory(this);
			return;
		}

		// Life Time Check
		if (Game.GlobalFrame >= LifeEndFrame) {
			Active = false;
			IgnoreReposition = true;
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
					IgnoreReposition = true;
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


	public override void Update () {

		base.Update();
		if (!Active) return;

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
					ch.Buff.GiveBuff(OnFireBuff.TYPE_ID, 200);
				}
			}
		}
		if (!damaged) {
			DamageCharacterStartFrame = -1;
		}
	}


	public override void LateUpdate () {

		base.LateUpdate();
		if (!Active) return;

		// Putout Smoke
		if (ManuallyPutout) {
			OnFirePutout?.Invoke(TypeID, Rect);
			Active = false;
			return;
		}

		using var _ = new LayerScope(UseAdditiveShader ? RenderLayer.ADD : RenderLayer.DEFAULT);

		var cell = Renderer.Draw(
			TypeID,
			X + (Direction == Direction4.Left ? Width : Direction == Direction4.Right ? 0 : Width / 2),
			Y + (Direction == Direction4.Down ? Height : Direction == Direction4.Up ? 0 : Height / 2),
			500, 0,
			Direction.GetRotation(),
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			z: int.MaxValue
		);

		// Fit Size to Target
		if (Target is Entity eTarget && cell.Width != eTarget.Width) {
			cell.Height = cell.Height * eTarget.Width / cell.Width;
			cell.Width = eTarget.Width;
		}

		// Illuminance
		LightingSystem.Illuminate(X, Y, IlluminateUnitRadius * Const.CEL, IlluminateAmount);

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


	public void Setup (int burnDuration, Direction4 direction, int width = Const.CEL, int height = Const.CEL) {
		Width = width;
		Height = height;
		BurnedFrame = Game.GlobalFrame + burnDuration;
		LifeEndFrame = BurnedFrame + WeakenDuration;
		Target = null;
		Direction = direction;
		ManuallyPutout = false;
		IgnoreReposition = false;
	}


	public void Setup (ICombustible com) {
		if (com == null) return;
		if (com is Entity entity) {
			Width = entity.Width;
			Height = entity.Height;
		}
		int fixedDuration = Util.Max(com.BurnedDuration * 1000 / PowerAmount.GreaterOrEquel(1), 1);
		BurnedFrame = Game.GlobalFrame + fixedDuration;
		LifeEndFrame = BurnedFrame + WeakenDuration;
		Direction = Direction4.Up;
		Target = com;
		com.BurnStartFrame = Game.GlobalFrame;
		ManuallyPutout = false;
		IgnoreReposition = false;
	}


	public void Spread () => IFire.SpreadFire(TypeID, Rect.Expand(SpreadRange), ignore: this);


	public void Putout (bool manually) {
		BurnedFrame = Game.GlobalFrame;
		LifeEndFrame = Game.GlobalFrame + WeakenDuration;
		SpreadFrame = int.MaxValue;
		ManuallyPutout = manually;
		IgnoreReposition = true;
		if (Target != null) {
			Target.BurnStartFrame = -1;
		}
	}


	#endregion




}