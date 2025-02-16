using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Tornado : Entity {

	// VAR
	private const int END_DUR = 30;
	public static readonly int TYPE_ID = typeof(Tornado).AngeHash();
	private static readonly SpriteCode TOR_PART = "Tornado.Part";
	protected abstract Int2 Velocity { get; }
	private int TornadingStopFrame = -1;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TornadingStopFrame = -1;
		if (FromWorld) {
			Width = Const.CEL;
			Height = Const.CEL * 2;
		}
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();

		if (TornadingStopFrame < 0) {

			// Move Self
			XY += Velocity;

			// Take Target
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC, Rect, out int count, this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig || rig.IgnorePhysics) continue;
				int velX = Velocity.x;
				int velY = Velocity.y;
				if (rig.IsGrounded) {
					velX -= (rig.X - (X + Width / 2)) / 4;
					velY -= (rig.Y - Y) / 4;
				} else {
					velX -= (rig.X - (X + Width / 2)) / 8;
					velY -= (rig.Y - Y) / 10;
				}
				rig.FillAsTrigger(1);
				rig.MomentumX = (velX, 1);
				rig.MomentumY = (velY, 1);
				if (rig is IWithCharacterMovement wMov) {
					var mov = wMov.CurrentMovement;
					if (Game.GlobalFrame % 6 == 0) {
						mov.RunSpeed.Multiply(-1000, 1);
						mov.WalkSpeed.Multiply(-1000, 1);
						mov.SquatMoveSpeed.Multiply(-1000, 1);
						mov.RushSpeed.Multiply(-1000, 1);
						mov.DashSpeed.Multiply(-1000, 1);
					}
					mov.LockFacingRight((Game.GlobalFrame / 6) % 2 == 0);
				}
				if (rig is IWithCharacterRenderer wRen && wRen.CurrentRenderer is PoseCharacterRenderer pRen) {
					pRen.ForceFaceExpressionIndex.Override((int)CharacterFaceExpression.Suffer, 1);
				}
			}

			// Stop Check
			if (Physics.Overlap(PhysicsMask.LEVEL, Rect, this)) {
				TornadingStopFrame = Game.GlobalFrame;
			}

		} else if (Game.GlobalFrame >= TornadingStopFrame + END_DUR) {
			Active = false;
		}

	}

	public override void LateUpdate () {
		base.LateUpdate();

		if (!Active) return;
		if (!Renderer.TryGetSprite(TOR_PART, out var sprite)) return;

		const int FREQ = 42;
		const int PART_COUNT = 20;
		int localFrame = Game.GlobalFrame - SpawnFrame;
		int partH = Height * 2 / PART_COUNT;
		int partStep = Height / PART_COUNT;
		int randomWidth = Width / 16;
		int randomHeight = Height / 12;
		int tightX = Width / 24;
		for (int i = 0; i < PART_COUNT; i++) {
			int frame = (localFrame + i * 12) % FREQ;
			float lerp010 = frame.PingPong(FREQ / 2) / (FREQ / 2f);
			float lerp010Alt = (frame + FREQ / 4).PingPong(FREQ / 2) / (FREQ / 2f);
			float ease010 = Ease.InOutSine(lerp010);
			var cell = Renderer.Draw(
				sprite,
				X + ((int)(ease010 * Width)),
				Y + i * partStep,
				500, 500, 0,
				Util.QuickRandomSign() * (i + 1) * Width / PART_COUNT + Util.QuickRandom(-randomWidth, randomWidth + 1),
				partH + Util.QuickRandom(-randomHeight, randomHeight + 1),
				Color32.LerpUnclamped(Color32.GREY_64, Color32.WHITE, lerp010Alt)
			);
			cell.Z = (lerp010Alt * 2 - 1).RoundToInt() * 1024;
		}

	}

	// API
	public static Tornado Spawn (int x, int y, int width = Const.CEL, int height = Const.CEL * 2) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is not Tornado tor) return null;
		tor.Width = width;
		tor.Height = height;
		return tor;
	}

}
