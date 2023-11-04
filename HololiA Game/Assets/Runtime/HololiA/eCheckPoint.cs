using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace HololiaGame {

	public class eCheckTako : CheckPoint { }
	[LinkedCheckPoint(typeof(eCheckTako))] public class eAltarTako : CheckAltar { }

	public class eCheckShark : CheckPoint { }
	[LinkedCheckPoint(typeof(eCheckShark))] public class eAltarShark : CheckAltar { }

}
