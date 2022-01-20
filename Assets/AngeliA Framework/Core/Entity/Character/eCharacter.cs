using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eCharacter : Entity {




		#region --- VAR ---


		// Api
		protected abstract CharacterMovement Movement { get; }
		protected abstract CharacterRenderer Renderer { get; }
		public int Width => m_Width;
		public int Height => m_Height;
		public RectInt Rect => new(X - Width / 2, Y, Width, Height);

		// Ser
		[SerializeField] int m_Width = Const.CELL_SIZE;
		[SerializeField] int m_Height = Const.CELL_SIZE;

		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			Movement.Init();
		}


		public override void FillPhysics (int frame) => CellPhysics.Fill(PhysicsLayer.Character, Rect, this);


		public override void FrameUpdate (int frame) {
			Movement.FrameUpdate(frame, this);
			Renderer.FrameUpdate(frame, this);
		}


		#endregion




	}
}
