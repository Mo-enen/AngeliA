using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	public class eNormalBoy : NPC {
		protected override int PoseBodyAbove => 7 * A2G;
	}

	public class eNormalGirl : NPC { }

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
		
	}

	public class eBusinessElderly : NPC {
		protected override Color32 HairColor => new(212, 212, 212, 255);
	}

	public class eBlondWoman : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
	}

	public class eBlondMan : NPC {
		protected override Color32 HairColor => new(252, 213, 74, 255);
	}




}