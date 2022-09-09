using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eConveyorLeftSlow : eConveyor { protected override int MoveSpeed => -12; }
	public class eConveyorRightSlow : eConveyor { protected override int MoveSpeed => 12; }
	public class eConveyorLeftFast : eConveyor { protected override int MoveSpeed => -24; }
	public class eConveyorRightFast : eConveyor { protected override int MoveSpeed => 24; }
	public abstract class eConveyor : Entity {


		// SUB
		protected enum PartType {
			None = 0,
			LeftEdge = 1,
			Middle = 2,
			RightEdge = 3,
			Single = 4,
		}


		// Const
		private static readonly int[] MID_CODES = new int[8] { "Conveyor Mid 0".AngeHash(), "Conveyor Mid 1".AngeHash(), "Conveyor Mid 2".AngeHash(), "Conveyor Mid 3".AngeHash(), "Conveyor Mid 4".AngeHash(), "Conveyor Mid 5".AngeHash(), "Conveyor Mid 6".AngeHash(), "Conveyor Mid 7".AngeHash(), };
		private static readonly int[] LEFT_CODES = new int[8] { "Conveyor Left 0".AngeHash(), "Conveyor Left 1".AngeHash(), "Conveyor Left 2".AngeHash(), "Conveyor Left 3".AngeHash(), "Conveyor Left 4".AngeHash(), "Conveyor Left 5".AngeHash(), "Conveyor Left 6".AngeHash(), "Conveyor Left 7".AngeHash(), };
		private static readonly int[] RIGHT_CODES = new int[8] { "Conveyor Right 0".AngeHash(), "Conveyor Right 1".AngeHash(), "Conveyor Right 2".AngeHash(), "Conveyor Right 3".AngeHash(), "Conveyor Right 4".AngeHash(), "Conveyor Right 5".AngeHash(), "Conveyor Right 6".AngeHash(), "Conveyor Right 7".AngeHash(), };
		private static readonly int[] SINGLE_CODES = new int[8] { "Conveyor Single 0".AngeHash(), "Conveyor Single 1".AngeHash(), "Conveyor Single 2".AngeHash(), "Conveyor Single 3".AngeHash(), "Conveyor Single 4".AngeHash(), "Conveyor Single 5".AngeHash(), "Conveyor Single 6".AngeHash(), "Conveyor Single 7".AngeHash(), };

		// Api
		protected PartType Part { get; private set; } = PartType.None;
		protected abstract int MoveSpeed { get; }

		// Data
		private static readonly HitInfo[] c_Update = new HitInfo[16];


		// MSG
		public override void OnActived () {
			Part = PartType.None;
			base.OnActived();
		}


		public override void FillPhysics () {
			base.FillPhysics();
			Physics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			Update_Part();
			var rect = Rect;
			rect.y += rect.height;
			rect.height = 1;
			int count = Physics.OverlapAll(c_Update, YayaConst.MASK_SOLID, rect, this);
			for (int i = 0; i < count; i++) {
				var hit = c_Update[i];
				if (hit.Entity is eYayaRigidbody rig) {
					rig.PerformMove(MoveSpeed, 0, true, false);
					rig.Y = rect.yMax;
					rig.VelocityY = 0;
				}
			}
		}


		private void Update_Part () {
			if (Part != PartType.None) return;
			var rect = Rect;
			int width = rect.width;
			rect.width = 1;
			rect.x -= 1;
			bool hasLeft = Physics.HasEntity<eConveyor>(rect, YayaConst.MASK_ENVIRONMENT, this);
			rect.x += width + 1;
			bool hasRight = Physics.HasEntity<eConveyor>(rect, YayaConst.MASK_ENVIRONMENT, this);
			Part =
				hasLeft && hasRight ? PartType.Middle :
				hasLeft && !hasRight ? PartType.RightEdge :
				!hasLeft && hasRight ? PartType.LeftEdge :
				PartType.Single;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int frame = Game.GlobalFrame;
			int aFrame = (frame * Mathf.Abs(MoveSpeed) / 16).UMod(8);
			if (MoveSpeed > 0) aFrame = 7 - aFrame;
			switch (Part) {
				case PartType.LeftEdge:
                    AngeliaFramework.Renderer.Draw(LEFT_CODES[aFrame], base.Rect);
					break;
				case PartType.Middle:
                    AngeliaFramework.Renderer.Draw(MID_CODES[aFrame], base.Rect);
					break;
				case PartType.RightEdge:
                    AngeliaFramework.Renderer.Draw(RIGHT_CODES[aFrame], base.Rect);
					break;
				case PartType.Single:
				case PartType.None:
                    AngeliaFramework.Renderer.Draw(SINGLE_CODES[aFrame], base.Rect);
					break;
			}
		}


	}
}
