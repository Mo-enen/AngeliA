using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	public class eWoodStoneDoorFront : Door {
		protected override bool IsFrontDoor => true;
		private static readonly int OPEN_CODE = "WoodStoneDoorFront Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}


	public class eWoodStoneDoorBack : Door {
		protected override bool IsFrontDoor => false;
		private static readonly int OPEN_CODE = "WoodStoneDoorBack Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}



	public class eWoodDoorFront : Door {
		protected override bool IsFrontDoor => true;
		private static readonly int OPEN_CODE = "WoodDoorFront Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}


	public class eWoodDoorBack : Door {
		protected override bool IsFrontDoor => false;
		private static readonly int OPEN_CODE = "WoodDoorBack Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}




	public class ePortalFront : ePortal {
		private static readonly int CIRCLE_CODE = "PortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "PortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override bool IsFrontDoor => true;
	}


	public class ePortalBack : ePortal {
		private static readonly int CIRCLE_CODE = "PortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "PortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override bool IsFrontDoor => false;
	}


	[EntityAttribute.Capacity(1)]
	public class eCheckPointPortal : ePortal {
		private static readonly int CIRCLE_CODE = "CheckPointPortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "CheckPointPortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override Vector3Int TargetGlobalPosition => CheckPoint.LastInvokedCheckPointUnitPosition.ToGlobal();
		[AfterGameInitialize]
		public static void Initialize () {
			CheckPoint.BackPortalEntityID = typeof(eCheckPointPortal).AngeHash();
		}
		public override bool Invoke (Player player) {
			bool result = base.Invoke(player);
			if (result) Active = false;
			return result;
		}
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public abstract class ePortal : Door {


		// VAR
		protected abstract int CircleCode { get; }
		protected abstract int FlameCode { get; }
		protected override bool TouchToEnter => true;
		protected override int ArtworkCode => 0;
		protected override int ArtworkCode_Open => 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			X -= Const.CEL / 2;
			Y -= Const.CEL / 2;
			Width = Const.CEL * 2;
			Height = Const.CEL * 2;
		}


		public override void FrameUpdate () {

			int centerX = X + Width / 2;
			int centerY = Y + Height / 2;
			int scale = ((Game.GlobalFrame - SpawnFrame) * 30).Clamp(0, 1000);

			// Circle
			if (CellRenderer.TryGetSprite(CircleCode, out var circle)) {
				const int CIRCLE_DURATION = 24;
				const int CIRCLE_SIZE = Const.CEL * 2;
				const int CIRCLE_COUNT = 4;
				int circleFrame = Game.GlobalFrame % CIRCLE_DURATION;
				bool revertZ = IsFrontDoor != FrameTask.IsTasking<TeleportTask>();
				for (int i = 0; i < CIRCLE_COUNT; i++) {
					int size = Util.RemapUnclamped(
						0, CIRCLE_DURATION,
						CIRCLE_SIZE - CIRCLE_SIZE * i / CIRCLE_COUNT,
						CIRCLE_SIZE - CIRCLE_SIZE * (i + 1) / CIRCLE_COUNT,
						circleFrame
					);
					size = size * scale / 1000;
					int rgbA = Util.RemapUnclamped(0, 3, 255, 128, IsFrontDoor ? i : CIRCLE_COUNT - i);
					int rgbB = Util.RemapUnclamped(0, 3, 255, 128, IsFrontDoor ? i + 1 : CIRCLE_COUNT - i - 1);
					byte rgb = (byte)Mathf.Lerp(rgbA, rgbB, (float)circleFrame / CIRCLE_DURATION);
					var tint = new Color32(
						rgb, rgb, rgb,
						(byte)(i > 0 ? 255 : Util.RemapUnclamped(0, CIRCLE_DURATION, 0, 400, circleFrame).Clamp(0, 255))
					);
					CellRenderer.Draw(
						circle.GlobalID, centerX, centerY,
						500, 500, 0,
						size, size,
						tint, (revertZ ? -circle.SortingZ : circle.SortingZ) + i
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
						Const.CEL, Const.CEL * 5 / 4,
						flameFrame.PingPong(FLAME_DURATION / 2)
					);
					size = size * scale / 1000;
					var tint = IsFrontDoor ? Const.WHITE : Const.GREY_128;
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


}