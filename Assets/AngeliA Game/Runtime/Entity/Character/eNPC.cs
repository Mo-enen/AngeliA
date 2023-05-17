using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	// Citizen A
	public class eNormalBoy : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
	}


	public class eNormalGirl : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
	}


	public class eStreetGangster : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eSchoolBoy : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
	}


	public class eNormalLady : NPC {
		protected override Color32 HairColor => new(207, 123, 60, 255);
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eHousewife : NPC {
		protected override Color32 HairColor => new(176, 94, 196, 255);
		protected override int PoseBodyAbove => 9 * A2G;
		protected override bool StaggerLegWhenIdle => false;
	}


	public class eSchoolTeacher : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eBusinessElderly : NPC {
		protected override Color32 HairColor => new(212, 212, 212, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eBlondWoman : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eBlondMan : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 9 * A2G;
	}


	// Citizen B
	public class eRichLady : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eRichMan : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eFisherMan : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eShopkeeper : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	public class ePoet : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eCowboy : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eYoungWoman : NPC {
		protected override Color32 HairColor => new(240, 86, 86, 255);
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eSailorKid : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	public class eBigBoy : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eLittleGirl : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 6 * A2G;
	}


	// Businessman
	public class eBusinessmanA : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	public class eBusinessmanB : NPC {
		protected override int PoseBodyAbove => 5 * A2G;
	}


	public class eBusinessmanC : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eBusinessmanD : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eBusinessmanE : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eBusinessmanF : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eBusinessmanG : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eBusinessmanH : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
	}


	public class eBusinessmanI : NPC {
		protected override int PoseBodyAbove => 6 * A2G;
	}


	public class eBusinessmanJ : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	// Doctor
	public class eDoctorA : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorB : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	public class eDoctorC : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorD : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorE : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorF : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorG : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eDoctorH : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorI : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eDoctorJ : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	// Athlete
	public class eAthleteA : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eAthleteB : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eAthleteC : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eAthleteD : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eAthleteE : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eAthleteF : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eAthleteG : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eAthleteH : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eAthleteI : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eAthleteJ : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	// Student
	public class eStudentA : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eStudentB : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eStudentC : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eStudentD : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	public class eStudentE : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eStudentF : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eStudentG : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eStudentH : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eStudentI : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eStudentJ : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}



	// Footman
	public abstract class eFootman : NPC {


		protected void DrawShoulderArmor (int code) {
			AttachClothOn(
				UpperArmL, code, 36, true,
				(-UpperArmL.Rotation * 7 / 10).Clamp(-UpperArmL.Rotation - 30, -UpperArmL.Rotation + 30)
			);
			AttachClothOn(
				UpperArmR, code, 36, false,
				(-UpperArmR.Rotation * 7 / 10).Clamp(-UpperArmR.Rotation - 30, -UpperArmR.Rotation + 30)
			);
		}


		protected void DrawLimbArmor (int armCode, int legCode) {
			AttachClothOn(LowerArmL, armCode, LowerArmL.Z + 16, true);
			AttachClothOn(LowerArmR, armCode, LowerArmR.Z + 16, false);
			AttachClothOn(LowerLegL, legCode, LowerLegL.Z + 16, !FacingRight);
			AttachClothOn(LowerLegR, legCode, LowerLegR.Z + 16, !FacingRight);
		}


	}
	public class eFootmanA : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanA.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanA.Armor.Hip".AngeHash();
		private static readonly int ARM_CODE = "FootmanA.Armor.LowerArm".AngeHash();
		private static readonly int LEG_CODE = "FootmanA.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
			DrawLimbArmor(ARM_CODE, LEG_CODE);
			if (Body.FrontSide) {
				AttachClothOn(Body, HIP_CODE, 20, !FacingRight, 0);
			}
		}
	}


	public class eFootmanB : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanB.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanB.Armor.Hip".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
			if (Body.FrontSide) AttachClothOn(Body, HIP_CODE, 10, !FacingRight);
		}
	}


	public class eFootmanC : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanC.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanC.Armor.Hip".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
			if (Body.FrontSide) AttachClothOn(Body, HIP_CODE, 10, !FacingRight, 0);
		}
	}


	public class eFootmanD : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int HIP_CODE = "FootmanD.Armor.Hip".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Body.FrontSide) AttachClothOn(Body, HIP_CODE, 10, !FacingRight, 0);
		}
	}


	public class eFootmanE : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanE.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanE.Armor.Hip".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
			if (Body.FrontSide) AttachClothOn(Body, HIP_CODE, 10, !FacingRight, 0);
		}
	}


	public class eFootmanF : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanF.Suit.Shoulder".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
		}
	}


	public class eFootmanG : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanG.Suit.Shoulder".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
		}
	}


	public class eFootmanH : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanH.Suit.Shoulder".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawShoulderArmor(SHOULDER_CODE);
		}
	}


	public class eFootmanI : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		public override void FrameUpdate () {
			base.FrameUpdate();


		}
	}


	public class eFootmanJ : eFootman {
		protected override int PoseBodyAbove => 10 * A2G;
		public override void FrameUpdate () {
			base.FrameUpdate();


		}
	}


	// Viking
	public class eVikingA : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eVikingB : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eVikingC : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eVikingD : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eVikingE : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eVikingF : NPC {
		protected override int PoseBodyAbove => 13 * A2G;
		protected override Color32 HairColor => new(252, 213, 74, 255);
	}


	public class eVikingG : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eVikingH : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eVikingI : NPC {
		protected override Color32 HairColor => new(85, 85, 85, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eVikingJ : NPC {
		protected override Color32 HairColor => new(199, 58, 74, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}



}