using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class PortalFront : CircleFlamePortal {
		protected override Vector3Int TargetGlobalPosition => new(X + Width / 2, Y, Stage.ViewZ - 1);
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class PortalBack : CircleFlamePortal {
		protected override Vector3Int TargetGlobalPosition => new(X + Width / 2, Y, Stage.ViewZ + 1);
	}


	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.DontSpawnFromWorld]
	public class CheckPointPortal : CircleFlamePortal {

		public static readonly int TYPE_ID = typeof(CheckPointPortal).AngeHash();
		protected override Vector3Int TargetGlobalPosition => (CheckPoint.TurnBackUnitPosition ?? default).ToGlobal() + new Vector3Int(Const.HALF, 0, 0);
		private int InvokeFrame = -1;


		[OnGameInitialize(64)]
		public static void Initialize () => CheckPoint.BackPortalEntityID = typeof(CheckPointPortal).AngeHash();


		public override void OnActivated () {
			base.OnActivated();
			InvokeFrame = -1;
			if (!CheckPoint.TurnBackUnitPosition.HasValue) Active = false;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (InvokeFrame >= 0 && Game.GlobalFrame > InvokeFrame + 30) {
				Active = false;
				InvokeFrame = -1;
			}
		}


		public override bool Invoke (Player player) {
			bool result = base.Invoke(player);
			if (result) {
				InvokeFrame = Game.GlobalFrame;
			}
			return result;
		}


	}


	public abstract class CircleFlamePortal : Portal {
		private static readonly int CIRCLE_CODE = "PortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "PortalFlame".AngeHash();
		protected virtual int CircleCode => CIRCLE_CODE;
		protected virtual int FlameCode => FLAME_CODE;
		protected virtual int CircleSize => Const.CEL * 3 / 2;
		public override void OnActivated () {
			base.OnActivated();
			int size = CircleSize;
			X = X + Const.HALF - size / 2;
			Y = Y + Const.HALF - size / 2;
			Width = size;
			Height = size;
		}
		public override void FrameUpdate () {

			int centerX = X + Width / 2;
			int centerY = Y + Height / 2;
			int scale = ((Game.GlobalFrame - SpawnFrame) * 30).Clamp(0, 1000);

			// Circle
			if (CellRenderer.TryGetSprite(CircleCode, out var circle)) {
				const int CIRCLE_DURATION = 24;
				const int CIRCLE_COUNT = 4;
				int circleFrame = Game.GlobalFrame % CIRCLE_DURATION;
				int darkIndex = (Game.GlobalFrame % (CIRCLE_DURATION * CIRCLE_COUNT)) / CIRCLE_DURATION;
				for (int i = 0; i < CIRCLE_COUNT; i++) {
					int size = Util.RemapUnclamped(
						0, CIRCLE_DURATION,
						CircleSize - CircleSize * i / CIRCLE_COUNT,
						CircleSize - CircleSize * (i + 1) / CIRCLE_COUNT,
						circleFrame
					);
					size = size * scale / 1000;
					int rgbA = Util.RemapUnclamped(0, 3, 255, 128, CIRCLE_COUNT - i);
					int rgbB = Util.RemapUnclamped(0, 3, 255, 128, CIRCLE_COUNT - i - 1);
					byte rgb = i == darkIndex || i == (darkIndex + 1) % CIRCLE_COUNT ?
						(byte)42 :
						(byte)Mathf.Lerp(rgbA, rgbB, (float)circleFrame / CIRCLE_DURATION);
					var tint = new Color32(
						rgb, rgb, rgb,
						(byte)(i > 0 ? 255 : Util.RemapUnclamped(0, CIRCLE_DURATION, 0, 400, circleFrame).Clamp(0, 255))
					);
					CellRenderer.Draw(
						circle.GlobalID, centerX, centerY,
						500, 500, 0,
						size, size,
						tint, circle.SortingZ + i
					);
				}
			}

			// Flame
			if (CellRenderer.TryGetSprite(FlameCode, out var flame)) {
				const int FLAME_COUNT = 3;
				const int FLAME_DURATION = 51;
				int flameFrame = Game.GlobalFrame % FLAME_DURATION;
				for (int i = 0; i < FLAME_COUNT; i++) {
					int rot = Util.RemapUnclamped(
						0, FLAME_DURATION, 0, 360,
						(flameFrame + i * 360 / FLAME_COUNT) % FLAME_DURATION
					);
					int size = Util.RemapUnclamped(
						0, FLAME_DURATION / 2,
						CircleSize / 2, CircleSize * 5 / 8,
						flameFrame.PingPong(FLAME_DURATION / 2)
					);
					size = size * scale / 1000;
					var tint = Const.WHITE;
					tint.a = (byte)Util.RemapUnclamped(
						0, FLAME_DURATION / 2,
						255, 128,
						flameFrame.PingPong(FLAME_DURATION / 2)
					).Clamp(0, 255);
					CellRenderer.Draw(
						flame.GlobalID, centerX, centerY,
						flame.PivotX, flame.PivotY, rot,
						size, size,
						tint, flame.SortingZ + FLAME_COUNT + 1
					);
				}
			}
		}
	}


	public abstract class Portal : Entity {

		protected abstract Vector3Int TargetGlobalPosition { get; }

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}
		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Invoke
			var player = Player.Selecting;
			if (
				player != null &&
				!player.LockingInput &&
				!player.Teleporting &&
				player.Rect.Overlaps(Rect)
			) {
				Invoke(player);
			}
		}
		public virtual bool Invoke (Player player) {
			if (player == null || FrameTask.HasTask()) return false;
			int fromX = X + (Width - player.Width) / 2 - player.OffsetX;
			player.X = fromX;
			player.Y = Y;
			player.Stop();
			var task = TeleportTask.Teleport(
				fromX, Y + player.Height / 2,
				TargetGlobalPosition.x, TargetGlobalPosition.y, TargetGlobalPosition.z
			);
			if (task != null) {
				task.TeleportEntity = player;
				task.UsePortalEffect = true;
				task.WaitDuration = 30;
				task.Duration = 60;
				task.UseVignette = true;
				player.EnterTeleportState(task.Duration, Stage.ViewZ > TargetGlobalPosition.z, true);
			}
			player.VelocityX = 0;
			player.VelocityY = 0;
			return true;
		}

	}
}
