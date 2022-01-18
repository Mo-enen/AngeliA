using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities {
	public abstract class eCharacter : Entity {




		#region --- VAR ---


		// Api
		protected abstract CharacterMovement Movement { get; }
		protected abstract CharacterRenderer Renderer { get; }
		public int Width => m_Width;
		public int Height => m_Height;

		// Ser
		[SerializeField] int m_Width = Const.CELL_SIZE;
		[SerializeField] int m_Height = Const.CELL_SIZE;

		#endregion




		#region --- MSG ---


		public override void FillPhysics (int frame) {
			Movement.FillPhysics(this);
		}


		public override void FrameUpdate (int frame) {
			Movement.FrameUpdate(frame, this);
			Renderer.FrameUpdate(frame, this);
		}


		#endregion




	}
}
