using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	[CreateAssetMenu(fileName = "New Movement", menuName = "AngeliA/Character/Movement", order = 99)]
	public partial class CharacterMovement : ScriptableObject {




		#region --- VAR ---


		// Const
		private static readonly int WATER_TAG = "Water".ACode();
		private const int JUMP_TOLERANCE = 4;

		// Api
		public bool IsGrounded { get; private set; } = false;
		public bool InWater { get; private set; } = false;
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public int JumpCount { get; private set; } = 0;
		public bool IsPounding { get; private set; } = false;
		public int VelocityX { get; private set; } = 0;
		public int VelocityY { get; private set; } = 0;

		// Ser
		[SerializeField] GeneralConfig m_General = null;
		[SerializeField] MoveConfig m_Move = null;
		[SerializeField] JumpConfig m_Jump = null;
		[SerializeField] DashConfig m_Dash = null;
		[SerializeField] SquatConfig m_Squat = null;
		[SerializeField] PoundConfig m_Pound = null;

		// Data
		private int IntendedX = 0;
		private int IntendedY = 0;
		private bool HoldingJump = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private int LastGroundedFrame = int.MinValue;
		private int LastDashFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public void FillPhysics (eCharacter character) => CellPhysics.Fill(
			PhysicsLayer.Character,
			new RectInt(
				character.X - character.Width / 2, character.Y, character.Width, character.Height
			),
			character
		);


		public void FrameUpdate (int frame, eCharacter character) {
			Update_Cache(frame, character);
			Update_Jump(frame);
			Update_Dash(frame);
			Update_VelocityX();
			Update_VelocityY();
			Update_ApplyPhysics();
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevHoldingJump = HoldingJump;
		}


		private void Update_Cache (int frame, eCharacter character) {
			IsGrounded = GroundCheck(character);
			InWater = WaterCheck(character);
			IsDashing = m_Dash.Available && frame < LastDashFrame + m_Dash.Duration;
			IsSquating = m_Squat.Available && IsGrounded && ((!IsDashing && IntendedY < 0) || ForceSquatCheck(character.X, character.Y));
			IsPounding = m_Pound.Available && !IsGrounded && !InWater && !IsDashing && (IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsGrounded) LastGroundedFrame = frame;
		}


		private void Update_Jump (int frame) {
			// Reset Count on Grounded
			if (IsGrounded && !IntendedJump) {
				JumpCount = 0;
			}
			// Perform Jump
			if (IntendedJump && JumpCount < m_Jump.Count) {
				JumpCount++;
				LastDashFrame = int.MinValue;
				IsDashing = false;
			}
			// Fall off Edge
			if (JumpCount == 0 && !IsGrounded && frame > LastGroundedFrame + JUMP_TOLERANCE) {
				JumpCount++;
			}
			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && JumpCount <= m_Jump.Count && VelocityY > 0) {
					VelocityY = VelocityY * m_Jump.ReleaseLoseRate / 1000;
				}
			}
		}


		private void Update_Dash (int frame) {
			if (
				IntendedDash &&
				IsGrounded &&
				frame > LastDashFrame + m_Dash.Duration + m_Dash.Cooldown
			) {
				// Perform Dash
				LastDashFrame = frame;
				IsDashing = true;
				VelocityY = 0;
			}
		}


		private void Update_VelocityX () {
			int speed, acc, dcc;
			if (IsDashing) {
				speed = IntendedX * m_Dash.Speed;
				acc = m_Dash.Acceleration;
				dcc = m_Dash.Decceleration;
			} else if (IsSquating) {
				speed = IntendedX * m_Squat.Speed;
				acc = m_Squat.Acceleration;
				dcc = m_Squat.Decceleration;
			} else {
				speed = IntendedX * m_Move.Speed;
				acc = m_Move.Acceleration;
				dcc = m_Move.Decceleration;
			}
			VelocityX = VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void Update_VelocityY () {

			// Gravity



			//VelocityY = 0;
		}


		private void Update_ApplyPhysics () {



		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) {
			IntendedX = (int)x;
			IntendedY = (int)y;
		}
		public void HoldJump (bool holding) => HoldingJump = holding;
		public void Jump () => IntendedJump = true;
		public void Dash () => IntendedDash = true;
		public void Pound () => IntendedPound = true;


		#endregion




		#region --- LGC ---


		private bool GroundCheck (eCharacter character) {




			return false;
		}


		private bool WaterCheck (eCharacter character) => CellPhysics.Overlap(
			PhysicsLayer.Level,
			new RectInt(character.X - character.Width / 2, character.Y, character.Width, character.Height),
			null,
			CellPhysics.OperationMode.TriggerOnly,
			WATER_TAG
		) != null;


		private bool ForceSquatCheck (int x, int y) {




			return false;
		}


		#endregion




	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	using AngeliaFramework.Entities;
	[CustomEditor(typeof(CharacterMovement))]
	public class CharacterMovement_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
