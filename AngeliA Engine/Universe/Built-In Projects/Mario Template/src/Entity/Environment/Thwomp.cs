using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class ThwompLeft : Thwomp {
	private static readonly SpriteCode IDLE_SP = "ThwompH";
	private static readonly SpriteCode READY_SP = "ThwompH.Ready";
	private static readonly SpriteCode RUNNING_SP = "ThwompH.Run";
	protected override SpriteCode IdleSprite => IDLE_SP;
	protected override SpriteCode ReadySprite => READY_SP;
	protected override SpriteCode RunningSprite => RUNNING_SP;
	protected override Direction4 Direction => Direction4.Left;
}


public class ThwompRight : Thwomp {
	private static readonly SpriteCode IDLE_SP = "ThwompH";
	private static readonly SpriteCode READY_SP = "ThwompH.Ready";
	private static readonly SpriteCode RUNNING_SP = "ThwompH.Run";
	protected override SpriteCode IdleSprite => IDLE_SP;
	protected override SpriteCode ReadySprite => READY_SP;
	protected override SpriteCode RunningSprite => RUNNING_SP;
	protected override Direction4 Direction => Direction4.Right;
}


public class ThwompDown : Thwomp {
	private static readonly SpriteCode IDLE_SP = "ThwompV";
	private static readonly SpriteCode READY_SP = "ThwompV.Ready";
	private static readonly SpriteCode RUNNING_SP = "ThwompV.Run";
	protected override SpriteCode IdleSprite => IDLE_SP;
	protected override SpriteCode ReadySprite => READY_SP;
	protected override SpriteCode RunningSprite => RUNNING_SP;
	protected override Direction4 Direction => Direction4.Down;
}


public class ThwompUp : Thwomp {
	private static readonly SpriteCode IDLE_SP = "ThwompV";
	private static readonly SpriteCode READY_SP = "ThwompV.Ready";
	private static readonly SpriteCode RUNNING_SP = "ThwompV.Run";
	protected override SpriteCode IdleSprite => IDLE_SP;
	protected override SpriteCode ReadySprite => READY_SP;
	protected override SpriteCode RunningSprite => RUNNING_SP;
	protected override Direction4 Direction => Direction4.Up;
}


[EntityAttribute.MapEditorGroup(nameof(Enemy))]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.DontDrawBehind()]
public abstract class Thwomp : Rigidbody, IDamageReceiver {

	// VAR
	private const int RUN_SPEED = 56;
	private const int BACK_SPEED = 12;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int SelfCollisionMask => 0;
	protected abstract SpriteCode IdleSprite { get; }
	protected abstract SpriteCode ReadySprite { get; }
	protected abstract SpriteCode RunningSprite { get; }
	protected abstract Direction4 Direction { get; }
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

	private bool IsReady;
	private bool IsRunning;
	private Int2 HomePosition;
	private int LastReachedFrame;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsRunning = false;
		IsReady = false;
		LastReachedFrame = int.MinValue;
		Width = Const.CEL * 3 / 2;
		Height = Const.CEL * 2;
		if (FromWorld) {
			X = X + Const.HALF - Width / 2;
		}
		HomePosition = XY;
	}

	public override void FirstUpdate () {
		IgnorePhysics.True(1);
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void Update () {
		base.Update();

		// Damage Check
		IDamageReceiver.DamageAllOverlap(
			Rect.Expand(1),
			new Damage(1, Const.TEAM_PLAYER | Const.TEAM_ENVIRONMENT, this),
			host: this
		);
		if (IsRunning) {
			IDamageReceiver.DamageAllOverlap(
				Rect.EdgeOutside(Direction, 1),
				new Damage(1, Const.TEAM_ENEMY, this, type: Tag.MagicalDamage)
			);
			IBumpable.BumpAllOverlap(
				this, Direction,
				forceBump: true,
				new Damage(1, Const.TEAM_ENVIRONMENT, this, type: Tag.MagicalDamage),
				collisionMask: PhysicsMask.MAP
			);
		}

		// Ready/Run Check
		IsReady = false;
		if (!IsRunning && XY == HomePosition && Game.GlobalFrame > LastReachedFrame + 42) {
			var player = PlayerSystem.Selecting;
			if (player != null) {
				var basicCheckRect = Rect.EdgeOutside(Direction, Const.CEL * 16);
				if (player.Rect.Overlaps(basicCheckRect.Expand(Const.CEL * 2))) {
					IsReady = true;
					if (player.Rect.Overlaps(basicCheckRect.Expand(Const.CEL))) {
						IsRunning = true;
					}
				}
			}
		}

		// Movement
		var velocity =
			IsRunning ? Direction.Normal() * RUN_SPEED :
			Game.GlobalFrame < LastReachedFrame + 42 ? default :
			(HomePosition - XY).Clamped(-BACK_SPEED, -BACK_SPEED, BACK_SPEED, BACK_SPEED);
		var oldPos = XY;
		XY = Physics.Move(PhysicsMask.MAP, oldPos, velocity.x, velocity.y, Size, this);
		if (XY == oldPos) {
			if (IsRunning) {
				// Hit
				IsRunning = false;
				LastReachedFrame = Game.GlobalFrame;
			} else if (velocity != default) {
				// Force Back Home
				HomePosition = XY;
				LastReachedFrame = Game.GlobalFrame;
			}
		} else if (XY == HomePosition) {
			LastReachedFrame = Game.GlobalFrame;
		}

	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(
			IsRunning ? RunningSprite : IsReady ? ReadySprite : IdleSprite,
			Direction == Direction4.Left ? Rect.GetFlipHorizontal() : Rect
		);
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		//if (!damage.Type.HasAll(Tag.MagicalDamage)) return;
		if (damage.Bullet is Thwomp) return;
		Active = false;
		FrameworkUtil.InvokeObjectFreeFall(
			IdleSprite,
			X + Width / 2, Y + Height / 2,
			speedX: Util.QuickRandomSign() * 32,
			speedY: 82,
			rotationSpeed: Util.QuickRandomSign() * 8
		);
	}

}
