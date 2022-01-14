using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
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
			IsGrounded = GroundCheck(character.X, character.Y);
			InWater = WaterCheck(character.X, character.Y);
			IsDashing = frame < LastDashFrame + m_Dash.Duration;
			IsSquating = IsGrounded && ((!IsDashing && IntendedY < 0) || ForceSquatCheck(character.X, character.Y));
			IsPounding = !IsGrounded && !InWater && !IsDashing && (IsPounding ? IntendedY < 0 : IntendedPound);
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
				LastDashFrame = frame;
				IsDashing = true;
			}
		}


		private void Update_VelocityX () {




			//VelocityX = 0;
		}


		private void Update_VelocityY () {




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


		private bool GroundCheck (int x, int y) {


			return false;
		}


		private bool WaterCheck (int x, int y) => CellPhysics.Overlap(
			PhysicsLayer.Level,
			new RectInt(x - m_General.Width / 2, y, m_General.Width, m_General.Height),
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
