using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class CharacterPose {




		#region --- SUB ---


		public class PosePart {
			public int Type = POSE_Idle;
			public int Start = 0;
		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly int POSE_Idle = "Pose_Idle".AngeHash();

		// Pose
		public bool FacingFront { get; set; } = true;
		public bool FacingRight { get; set; } = true;

		// Api
		public int Frame { get; private set; } = 0;
		protected eCharacter Character { get; init; } = null;

		// Part
		public PosePart Hair { get; } = new();
		public PosePart Head { get; } = new();
		public PosePart Face { get; } = new();
		public PosePart Body { get; } = new();
		public PosePart BoingBoing { get; } = new();
		public PosePart Tail { get; } = new();
		public PosePart ArmL { get; } = new();
		public PosePart ArmR { get; } = new();
		public PosePart LegL { get; } = new();
		public PosePart LegR { get; } = new();
		public PosePart HandL { get; } = new();
		public PosePart HandR { get; } = new();
		public PosePart FootL { get; } = new();
		public PosePart FootR { get; } = new();


		#endregion




		#region --- API ---


		public CharacterPose (eCharacter character) => Character = character;


		public virtual void CalculatePose (int frame) {
			Frame = frame;
			FacingRight = Character.Movement.FacingRight;
			FacingFront = Character.Movement.FacingFront;





		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}