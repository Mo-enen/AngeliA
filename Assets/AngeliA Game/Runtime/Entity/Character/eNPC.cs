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
		public override Color32 HairColor => new(207, 123, 60, 255);
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eHousewife : NPC {
		public override Color32 HairColor => new(176, 94, 196, 255);
		protected override int PoseBodyAbove => 9 * A2G;
		protected override bool StaggerLegWhenIdle => false;
	}


	public class eSchoolTeacher : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eBusinessElderly : NPC {
		public override Color32 HairColor => new(212, 212, 212, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eBlondWoman : NPC {
		public override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eBlondMan : NPC {
		public override Color32 HairColor => new(252, 213, 74, 255);
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
		public override Color32 HairColor => new(240, 86, 86, 255);
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eSailorKid : NPC {
		protected override int PoseBodyAbove => 8 * A2G;
	}


	public class eBigBoy : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
	}


	public class eLittleGirl : NPC {
		public override Color32 HairColor => new(252, 213, 74, 255);
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
	public class eFootmanA : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanA.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanA.Armor.Hip".AngeHash();
		private static readonly int ARM_CODE = "FootmanA.Armor.LowerArm".AngeHash();
		private static readonly int LEG_CODE = "FootmanA.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(ARM_CODE, LEG_CODE);
			if (Body.FrontSide) {
				AttachClothOn(Body, Direction3.Down, HIP_CODE, 20, !FacingRight, 0);
			}
		}
	}


	public class eFootmanB : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanB.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanB.Armor.Hip".AngeHash();
		private static readonly int ARM_CODE = "FootmanB.Armor.LowerArm".AngeHash();
		private static readonly int LEG_CODE = "FootmanB.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(ARM_CODE, LEG_CODE);
			if (Body.FrontSide) AttachClothOn(Body, Direction3.Down, HIP_CODE, 10, !FacingRight);
		}
	}


	public class eFootmanC : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanC.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanC.Armor.Hip".AngeHash();
		private static readonly int LEG_CODE = "FootmanC.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(0, LEG_CODE);
			if (Body.FrontSide) AttachClothOn(Body, Direction3.Down, HIP_CODE, 10, !FacingRight, 0);
		}
	}


	public class eFootmanD : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int HIP_CODE = "FootmanD.Armor.Hip".AngeHash();
		private static readonly int SHOULDER_CODE = "FootmanD.Suit.Shoulder".AngeHash();
		private static readonly int ARM_CODE = "FootmanD.Armor.LowerArm".AngeHash();
		private static readonly int LEG_CODE = "FootmanD.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE, 0);
			DrawArmorForLimb(ARM_CODE, LEG_CODE);
			if (Body.FrontSide) AttachClothOn(Body, Direction3.Down, HIP_CODE, 10, !FacingRight, 0);
		}
	}


	public class eFootmanE : NPC {
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanE.Suit.Shoulder".AngeHash();
		private static readonly int HIP_CODE = "FootmanE.Armor.Hip".AngeHash();
		private static readonly int ARM_CODE = "FootmanE.Armor.LowerArm".AngeHash();
		private static readonly int LEG_CODE = "FootmanE.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(ARM_CODE, 0);
			AttachClothOn(UpperLegL, Direction3.Up, LEG_CODE, LowerLegL.Z + 16, true);
			if (Body.FrontSide) AttachClothOn(Body, Direction3.Down, HIP_CODE, 10, !FacingRight, 0);
		}
	}


	public class eFootmanF : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanF.Suit.Shoulder".AngeHash();
		private static readonly int ARM_CODE = "FootmanF.Armor.LowerArm".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(ARM_CODE, 0);
		}
	}


	public class eFootmanG : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanG.Suit.Shoulder".AngeHash();
		private static readonly int ARM_CODE = "FootmanG.Armor.LowerArm".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(ARM_CODE, 0);
		}
	}


	public class eFootmanH : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
		private static readonly int SHOULDER_CODE = "FootmanH.Suit.Shoulder".AngeHash();
		private static readonly int LEG_CODE = "FootmanH.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eFootmanI : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
		private static readonly int ARM_CODE = "FootmanI.Armor.LowerArm".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForLimb(ARM_CODE, 0);
		}
	}


	public class eFootmanJ : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
		private static readonly int ARM_CODE = "FootmanJ.Armor.LowerArm".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForLimb(ARM_CODE, 0);
		}
	}


	// Viking
	public class eVikingA : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
		private static readonly int SHOULDER_CODE = "VikingA.Armor.Shoulder".AngeHash();
		private static readonly int LEG_CODE = "VikingA.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Body.FrontSide) DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingB : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
		private static readonly int SHOULDER_CODE = "VikingB.Armor.Shoulder".AngeHash();
		private static readonly int LEG_CODE = "VikingB.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Body.FrontSide) DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingC : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
		private static readonly int LEG_CODE = "VikingC.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingD : NPC {
		protected override int PoseBodyAbove => 11 * A2G;
		private static readonly int SHOULDER_CODE = "VikingD.Armor.Shoulder".AngeHash();
		private static readonly int LEG_CODE = "VikingD.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Body.FrontSide) DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingE : NPC {
		protected override int PoseBodyAbove => 9 * A2G;
		private static readonly int LEG_CODE = "VikingE.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingF : NPC {
		protected override int PoseBodyAbove => 13 * A2G;
		public override Color32 HairColor => new(252, 213, 74, 255);
		private static readonly int LEG_CODE = "VikingF.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingG : NPC {
		public override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 11 * A2G;
		private static readonly int LEG_CODE = "VikingG.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


	public class eVikingH : NPC {
		public override Color32 HairColor => new(252, 213, 74, 255);
		protected override int PoseBodyAbove => 11 * A2G;
	}


	public class eVikingI : NPC {
		public override Color32 HairColor => new(85, 85, 85, 255);
		protected override int PoseBodyAbove => 10 * A2G;
	}


	public class eVikingJ : NPC {
		public override Color32 HairColor => new(199, 58, 74, 255);
		protected override int PoseBodyAbove => 10 * A2G;
		private static readonly int SHOULDER_CODE = "VikingJ.Armor.Shoulder".AngeHash();
		private static readonly int LEG_CODE = "VikingG.Armor.LowerLeg".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Body.FrontSide) DrawArmorForShoulder(SHOULDER_CODE);
			DrawArmorForLimb(0, LEG_CODE);
		}
	}


}