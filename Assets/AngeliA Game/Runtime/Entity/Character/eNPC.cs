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



}