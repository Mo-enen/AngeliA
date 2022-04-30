using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[CreateAssetMenu(fileName = "New Renderer", menuName = "бя Yaya/Character Renderer", order = 99)]
	[AddIntoGameData]
	public class CharacterRenderer : ScriptableObject {



		
		#region --- SUB ---


		[System.Serializable]
		public struct CRFrame {
			public int Code;
			public int Frame;
			public int Param;
			public CRFrame (int code, int frame, int param = 0) {
				Code = code;
				Frame = frame;
				Param = param;
			}
		}


		[System.Serializable]
		public class CRConfig {
			public int Width = Const.CELL_SIZE;
			public int Height = Const.CELL_SIZE * 2;
			public CRFrame[] Head = null;
			public CRFrame[] HairF = null;
			public CRFrame[] HairB = null;
			public CRFrame[] BodyF = null;
			public CRFrame[] BodyB = null;
			public CRFrame[] ArmL = null;
			public CRFrame[] ArmR = null;
			public CRFrame[] Legs = null;
		}


		protected class Pose {
			public int Code = 0;
			public RectInt Rect = default;
			public Int4 Border = default;
		}


		#endregion




		#region --- VAR ---


		// Api
		public virtual RectInt LocalBounds => _LocalBounds;
		public int ConfigIndex { get; set; } = 0;
		public eCharacter Character { get; private set; } = null;

		// Ser
		[SerializeField] CRConfig[] m_Configs = null;

		// Data
		private bool LeftArmOnFront = true;
		private bool RightArmOnFront = true;
		private RectInt _LocalBounds = default;
		private readonly Pose Pose_Head = new();
		private readonly Pose Pose_HairF = new();
		private readonly Pose Pose_HairB = new();
		private readonly Pose Pose_BodyF = new();
		private readonly Pose Pose_BodyB = new();
		private readonly Pose Pose_ArmL = new();
		private readonly Pose Pose_ArmR = new();
		private readonly Pose Pose_Legs = new();
		private readonly Pose Pose_EyeL = new();
		private readonly Pose Pose_EyeR = new();
		private readonly Pose Pose_Mouth = new();


		#endregion




		#region --- MSG ---


		public void Init (eCharacter ch) {
			Character = ch;
			ConfigIndex = 0;
		}


		public virtual void FrameUpdate (int frame) {
			if (m_Configs == null || m_Configs.Length == 0) return;
			CalculatePoses();
			DrawPoses();
		}


		protected virtual void CalculatePoses () {
			var config = m_Configs[ConfigIndex % m_Configs.Length];
			_LocalBounds = new(-config.Width / 2, 0, config.Width, config.Height);






			LeftArmOnFront = true;
			RightArmOnFront = true;


		}


		protected virtual void DrawPoses () {
			if (Character.Movement.FacingFront) {
				// Facing Front

				DrawHairBack(Pose_HairB);

				if (!LeftArmOnFront) DrawArm(Pose_ArmL);
				if (!RightArmOnFront) DrawArm(Pose_ArmR);
				DrawLegs(Pose_Legs);

				DrawBody(Pose_BodyF);
				DrawHead(Pose_Head);
				DrawFace(Pose_Head.Rect, Pose_Head.Border);
				DrawHairFront(Pose_HairF);

				if (LeftArmOnFront) DrawArm(Pose_ArmL);
				if (RightArmOnFront) DrawArm(Pose_ArmR);

			} else {
				// Facing Back

				if (!LeftArmOnFront) DrawArm(Pose_ArmL);
				if (!RightArmOnFront) DrawArm(Pose_ArmR);
				DrawLegs(Pose_Legs);

				DrawHairFront(Pose_HairF);
				DrawFace(Pose_Head.Rect, Pose_Head.Border);
				DrawHead(Pose_Head);
				DrawBody(Pose_BodyB);

				if (LeftArmOnFront) DrawArm(Pose_ArmL);
				if (RightArmOnFront) DrawArm(Pose_ArmR);

				DrawHairBack(Pose_HairB);
			}
		}


		#endregion




		#region --- API ---


		// Body-Part
		protected virtual void DrawHairFront (Pose pose) {
			CellRenderer.Draw(pose.Code, pose.Rect);
		}


		protected virtual void DrawHairBack (Pose pose) {
			CellRenderer.Draw(pose.Code, pose.Rect);
		}


		protected virtual void DrawHead (Pose pose) {
			CellRenderer.Draw(pose.Code, pose.Rect);
		}


		protected virtual void DrawFace (RectInt headRect, Int4 headBorder) {
			CellRenderer.Draw(Pose_EyeL.Code, Pose_EyeL.Rect);
			CellRenderer.Draw(Pose_EyeR.Code, Pose_EyeR.Rect);
			CellRenderer.Draw(Pose_Mouth.Code, Pose_Mouth.Rect);
		}


		protected virtual void DrawBody (Pose pose) {
			CellRenderer.Draw(pose.Code, pose.Rect);
		}


		protected virtual void DrawArm (Pose pose) {
			CellRenderer.Draw(pose.Code, pose.Rect);
		}


		protected virtual void DrawLegs (Pose pose) {
			CellRenderer.Draw(pose.Code, pose.Rect);
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}