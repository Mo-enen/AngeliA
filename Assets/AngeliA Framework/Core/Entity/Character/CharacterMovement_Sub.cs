using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public partial class CharacterMovement {



		[System.Serializable]
		public class GeneralConfig {
			public int Width = Const.CELL_SIZE;
			public int Height = Const.CELL_SIZE;
			public int Gravity = Const.CELL_SIZE * 20;
			public int MaxGravitySpeed = Const.CELL_SIZE * 24;
		}



		[System.Serializable]
		public class MoveConfig {
			public int Speed = Const.CELL_SIZE * 4;
			public int Acceleration = Const.CELL_SIZE * 8 / 60;
			public int Decceleration = Const.CELL_SIZE * 6 / 60;
			public bool WalkOnWater = false;
		}



		[System.Serializable]
		public class JumpConfig {
			public int Speed = Const.CELL_SIZE * 6;
			public int Count = 2;
			public int ReleaseLoseRate = 700;
			public int RaiseGravity = Const.CELL_SIZE * 16;
		}



		[System.Serializable]
		public class DashConfig {
			public bool Available = true;
			public int Speed = Const.CELL_SIZE * 6;
			public int Duration = 18;
			public int Cooldown = 6;
			public int Acceleration = int.MaxValue;
			public int Decceleration = Const.CELL_SIZE * 18 / 60;
		}


		[System.Serializable]
		public class SquatConfig {
			public bool Available = true;
			public int Speed = Const.CELL_SIZE * 2;
			public int Acceleration = int.MaxValue;
			public int Decceleration = int.MaxValue;
		}


		[System.Serializable]
		public class PoundConfig {
			public bool Available = true;
			public int Speed = Const.CELL_SIZE * 20;

		}



		[SerializeField] GeneralConfig m_General = null;
		[SerializeField] MoveConfig m_Move = null;
		[SerializeField] JumpConfig m_Jump = null;
		[SerializeField] DashConfig m_Dash = null;
		[SerializeField] SquatConfig m_Squat = null;
		[SerializeField] PoundConfig m_Pound = null;


	}
}