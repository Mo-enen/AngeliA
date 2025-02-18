using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Tornado : Entity {

	// VAR
	private static readonly SpriteCode TOR_PART = "Tornado.Part";
	private const int END_DUR = 30;
	protected abstract Int2 Velocity { get; }
	protected virtual SpriteCode ArtworkPart => TOR_PART;
	protected virtual int StrengthGrounded => 1000;
	protected virtual int StrengthInAir => 600;
	protected virtual int StiffFrameGap => 6;
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
			int still = StiffFrameGap.GreaterOrEquel(1);
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC, Rect, out int count, this, OperationMode.ColliderAndTrigger
			);
			var tornadoCenter = Rect.CenterInt();
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig || rig.IgnorePhysics) continue;
				// Drag
				int velX = Velocity.x;
				int velY = Velocity.y;
				var rect = rig.Rect;
				if (rig.IsGrounded) {
					velX -= (rect.CenterX() - tornadoCenter.x) * StrengthGrounded / 6000;
					velY -= (rect.y - tornadoCenter.y) * StrengthGrounded / 6000;
					// Stiff
					if (rig is IWithCharacterMovement wMov) {
						var mov = wMov.CurrentMovement;
						if (Game.GlobalFrame % still == 0) {
							mov.RunSpeed.Multiply(-1000, 1);
							mov.WalkSpeed.Multiply(-1000, 1);
							mov.SquatMoveSpeed.Multiply(-1000, 1);
							mov.RushSpeed.Multiply(-1000, 1);
							mov.DashSpeed.Multiply(-1000, 1);
						}
						mov.LockFacingRight((Game.GlobalFrame / still) % 2 == 0);
					}
				} else {
					velX -= (rect.CenterX() - tornadoCenter.x) * StrengthInAir / 6000;
					velY -= (rect.y - tornadoCenter.y) * StrengthInAir / 6000;
				}

				if (rig is not IWithCharacterMovement) {
					rig.IgnorePhysics.True();
				}
				rig.X += velX;
				rig.Y += velY;

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
		if (!Renderer.TryGetSprite(ArtworkPart, out var sprite)) return;

		int freq = 42;
		const int PART_COUNT = 20;
		int localFrame = (Game.GlobalFrame - SpawnFrame) * Util.RemapUnclamped(
			0, 1000, 1, 2, StrengthGrounded - 1000
		);
		int partH = Height * 2 / PART_COUNT;
		int partStep = Height / PART_COUNT;
		int randomWidth = Width / 16;
		int randomHeight = Height / 12;
		for (int i = 0; i < PART_COUNT; i++) {
			int frame = (localFrame + i * 12) % freq;
			float lerp010 = frame.PingPong(freq / 2) / (freq / 2f);
			float lerp010Alt = (frame + freq / 4).PingPong(freq / 2) / (freq / 2f);
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

}
